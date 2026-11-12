# Mechanical checks over the PR #226 evidence record.
# Each check states what it proves and prints PASS/FAIL with the evidence.
param(
    [string]$Record = "_bmad-output/initiative-attendta/demo-walkthrough-evidence.md",
    [string]$Plan   = "_bmad-output/initiative-attendta/tickets/epic-release-acceptance/tickets/story-full-functional-demo-script-walkthrough-across-all-26-featur-plan.md"
)

$lines = Get-Content $Record
$fail  = 0

function Section([string]$from, [string]$to) {
    $start = ($lines | Select-String -SimpleMatch $from | Select-Object -First 1).LineNumber
    $end   = ($lines | Select-String -SimpleMatch $to   | Select-Object -First 1).LineNumber
    return $lines[($start)..($end - 2)]
}

# --- 1. Journey rows are J1..J68 with no gap -------------------------------
$journey = Section "## 4. The journey" "## 5. FR-1..FR-36"
$jIds = @($journey | ForEach-Object { if ($_ -match '^\|\s*(J\d+)\s*\|') { [int]$Matches[1].Substring(1) } })
$expected = 1..68
$missing  = $expected | Where-Object { $_ -notin $jIds }
$dupes    = $jIds | Group-Object | Where-Object Count -gt 1 | ForEach-Object Name
if ($jIds.Count -eq 68 -and -not $missing -and -not $dupes) {
    "PASS  journey table: 68 rows, J1..J68 contiguous, no duplicates"
} else { $fail++; "FAIL  journey table: count=$($jIds.Count) missing=$($missing -join ',') dupes=$($dupes -join ',')" }

# --- 2. Every J referenced by the index tables exists ----------------------
$frSec  = Section "## 5. FR-1..FR-36" "## 6. The 26 functional IDs"
$idSec  = Section "## 6. The 26 functional IDs" "## 7. Blocked rows"
$refs = @(($frSec + $idSec) | ForEach-Object { [regex]::Matches($_, '\bJ(\d+)\b') } | ForEach-Object { [int]$_.Groups[1].Value } | Sort-Object -Unique)
$dangling = $refs | Where-Object { $_ -notin $jIds }
if (-not $dangling) {
    "PASS  index tables reference only existing journey rows ($($refs.Count) distinct J-rows cited)"
} else { $fail++; "FAIL  dangling journey references: $($dangling -join ',')" }

# --- 3. FR-1..FR-36 each appear exactly once in section 5 ------------------
$frIds = @($frSec | ForEach-Object { if ($_ -match '^\|\s*FR-(\d+)\s*\|') { [int]$Matches[1] } })
$frMissing = 1..36 | Where-Object { $_ -notin $frIds }
$frDupes   = $frIds | Group-Object | Where-Object Count -gt 1 | ForEach-Object Name
if ($frIds.Count -eq 36 -and -not $frMissing -and -not $frDupes) {
    "PASS  FR table: 36 rows, FR-1..FR-36 each exactly once"
} else { $fail++; "FAIL  FR table: count=$($frIds.Count) missing=$($frMissing -join ',') dupes=$($frDupes -join ',')" }

# --- 4. Every FR row carries a result -------------------------------------
$frNoResult = @($frSec | Where-Object { $_ -match '^\|\s*FR-\d+\s*\|' -and ($_ -split '\|').Count -lt 6 })
if (-not $frNoResult) { "PASS  every FR row has a result cell" } else { $fail++; "FAIL  FR rows without a result: $($frNoResult.Count)" }

# --- 5. The 26 functional IDs each appear exactly once --------------------
$expectedIds = @(
    'AUTH-01','AUTH-02','AUTH-03',
    'MD-01','MD-02','MD-03','MD-04','MD-05',
    'EVT-01','EVT-02','EVT-03','EVT-04',
    'REG-01','REG-02','REG-03','REG-04','REG-05',
    'QR-01',
    'ATT-01','ATT-02','ATT-03',
    'CERT-01','CERT-02',
    'REP-01',
    'ADM-01','ADM-02'
)
$idIds = @($idSec | ForEach-Object { if ($_ -match '^\|\s*([A-Z]+-\d+)\s*\|') { $Matches[1] } })
$idMissing = $expectedIds | Where-Object { $_ -notin $idIds }
$idDupes   = $idIds | Group-Object | Where-Object Count -gt 1 | ForEach-Object Name
$idExtra   = $idIds | Where-Object { $_ -notin $expectedIds }
if ($idIds.Count -eq 26 -and -not $idMissing -and -not $idDupes -and -not $idExtra) {
    "PASS  functional-ID table: 26 rows, all expected IDs, each exactly once"
} else { $fail++; "FAIL  functional-ID table: count=$($idIds.Count) missing=$($idMissing -join ',') dupes=$($idDupes -join ',') extra=$($idExtra -join ',')" }

# --- 6. Blocked rows: count, and every row names an owning evidence file --
$blkSec = Section "## 7. Blocked rows" "## 8. Notes and discrepancies"
$blkRows = @($blkSec | Where-Object { $_ -match '^\|\s*[A-Z][A-Z0-9]*(-[A-Z0-9]+)+[\s(|]' })
$noOwner = @($blkRows | Where-Object { ($_ -split '\|').Count -lt 6 -or $_ -notmatch '\.md|checklist' })
"INFO  blocked rows: $($blkRows.Count)"
if (-not $noOwner) { "PASS  every blocked row names an owning evidence file" } else { $fail++; "FAIL  blocked rows without an owner: $($noOwner.Count)" }
$blkRows | ForEach-Object { "        " + (($_ -split '\|')[1..2] -join '|').Trim() }

# --- 7. Plan's own row counts agree with the record ----------------------
$planText = Get-Content $Plan -Raw
$claim6 = $planText -match '(\d+) blocked rows'
$claims = [regex]::Matches($planText, '(\d+) blocked rows') | ForEach-Object { [int]$_.Groups[1].Value }
"INFO  plan claims blocked-row counts: $($claims -join ', ')"
if (($claims | Sort-Object -Unique).Count -gt 1) {
    $fail++
    "FAIL  plan disagrees with itself about the blocked-row count (record has $($blkRows.Count))"
} elseif ($claims[0] -ne $blkRows.Count) {
    $fail++
    "FAIL  plan says $($claims[0]) blocked rows, record has $($blkRows.Count)"
} else { "PASS  plan and record agree on $($blkRows.Count) blocked rows" }

# --- 8. Result vocabulary stays inside the legend ------------------------
$legend = Section "## 2. Result legend" "## 3. Provisioning"
$allowed = @('PASS','FAIL','BLOCKED','REUSED','Composite')
$used = @(($journey + $frSec + $idSec) | ForEach-Object {
    if ($_ -match '^\|.*\|\s*([^|]+?)\s*\|\s*$') { $Matches[1] }
} | Where-Object { $_ -and $_ -notmatch '^---' } | ForEach-Object {
    ($_ -replace '\([^)]*\)','').Trim()
} | Where-Object { $_ } | Sort-Object -Unique)
"INFO  result words used: $($used -join ' / ')"

""
if ($fail -eq 0) { "RESULT: all checks passed" } else { "RESULT: $fail check(s) failed" }
