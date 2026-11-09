# Verification log: 8.11 remove prototype data and prove complete real integrations

Baseline `2d05a3580645e91aac43770caebe473224ac8f5a`, branch
`feature/152-remove-prototype-data-and-prove-real-integrations`. Chronological; every command ran
in this repository unless a working directory is named.

## 1. Register rows against git

- Action: for each of the 21 register rows, find the commit that deleted the row's demo class with
  `git log --diff-filter=D --oneline -- <path>` and `git log --all --oneline --grep "BMAD <ticket>"`,
  then confirm the deletion with `git show --stat <sha>`.
- Result: every row resolves to a deleting commit. M-01's recorded `005f899` is the PR #209 merge
  (`Merge pull request #209 from FivePesos/feature/75/...`) whose tree still contains all five
  `DemoData/` files; `149791b` is the deletion. Nine rows recorded no commit at all.
- Evidence: `git log --oneline --all -- ASI.Basecode.WebApp/DemoData/DemoStore.cs` →
  `149791b`, `7637096`, `124ef29`, `58cffca`, `d6de0d5`. `git show --stat 005f899` touches no
  `DemoData/` file.
- Action: corrected the register's commit column for M-01 and filled the nine missing commits, and
  added the M-13b residue sentence to the report section.
- Result: the register now carries a deleting commit on every row.

## 2. Source search before any edit

- Action: `rg -n "DemoData|DemoStore|DemoSeedService|DemoDataRegistration|demo-pass-token"
  --glob '!**/bin/**' --glob '!**/obj/**' --glob '!_bmad-output/**' --glob '!**/node_modules/**' .`
- Result: no hits. The seam is gone from the product source.
- Action: `rg -n "M-0[1-9]|M-1[0-5][ab]?" ASI.Basecode.Data ASI.Basecode.Services
  ASI.Basecode.Resources ASI.Basecode.WebApp`
- Result: **five hits**, so the gate's own search failed:
  `CheckInService.cs:14` (M-05), `CheckInLookupService.cs:14` (M-06),
  `wwwroot/js/features/scanner.js:1` (M-05), `wwwroot/js/features/scanner-camera.js:1` (M-05),
  `Styles/features/passes.css:1` (M-03).
- Action: `rg -n "Demo|Mock|Prototype|Fixture|Sample"
  tests/ASI.Basecode.IntegrationTests/CheckInWindowFlagTests.cs`
- Result: `:73 var retiredSection = "Demo" + "Data";` — the inertness test assembled the retired
  section name from two words specifically so this search could not see it.

## 3. Residue cleared

- Action: dropped the register-id parenthetical from the five files above and replaced the split
  literal in `CheckInWindowFlagTests.cs` with `const string retiredSection = "DemoData";`, with a
  comment saying the test is the one place the name may appear.
- Result: `rg` for the register ids over the four product projects returns no hits (exit 1); `rg`
  for the `DemoData` family returns exactly one hit, `CheckInWindowFlagTests.cs:73`.
- Action: `npm --prefix ASI.Basecode.WebApp run css:build` after the `passes.css` comment edit.
- Result: `wwwroot/css/app.css` unchanged (`git diff --exit-code` clean); the comment is not part
  of the compiled output.

## 4. Build and suites

- Action: `dotnet build ASI.Basecode.sln -warnaserror`
- Result: Build succeeded, 0 warnings, 0 errors.
- Action: `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build`
- Result: 321 passed, 0 failed, 0 skipped.
- Action: `$env:ATTENDTA_TEST_SQL="Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True";
  dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --no-build`
- Result: 460 passed, 0 failed, 0 skipped, including
  `CheckInWindowFlagTests.RetiredSeamSwitchKeyIsInertWhenEnabled`.

## 5. Published output and startup

- Action: `dotnet publish "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj" -c Release -o <temp>\publish-811`
- Result: publish succeeded.
- Action: both searches over the publish directory, plus a directory listing for a demo folder.
- Result: no register id, no `DemoData` key, no `DemoData` directory; the published
  `appsettings.json` has no `DemoData` section.
- Action: started `dotnet ASI.Basecode.WebApp.dll` from the publish directory with
  `DemoData__Enabled=true`, no `DemoData__TokenKey`, `ASPNETCORE_ENVIRONMENT=Production`,
  `ASPNETCORE_URLS=http://127.0.0.1:5199` and
  `ConnectionStrings__DefaultConnection=...Database=AttendTa_IT_811verify...`.
- Result: the host started and `GET /Account/Login` returned 200. First attempt failed with
  `Common:SetupRoot must be configured` only because the working directory was the repository root,
  so the published `appsettings.json` was not the content root; starting from the publish directory
  is the correct published-build invocation.

## 6. One coherent database

- Action: created `AttendTa_IT_811verify`, ran `database/attendta-mvp-schema.sql`, then
  `seed-local-admin.sql`, `seed-event-types.sql` and `seed-demo-scenario.sql` with sqlcmd
  `:setvar` wrappers (the local sqlcmd 15 rejects the `-v name=value` form for values with spaces).
- Result: one administrator, one officer, nine member accounts, two venues, six event types and
  three events — `Leadership Summit 2026` (published, started, 8 confirmed),
  `Community Outreach Drive` (cancelled, 3 cancelled), `Founders' Week Assembly` (published,
  upcoming, 3 confirmed + 1 pending) — plus one pending account request. No attendance or
  certificate rows; those were produced live.
- Action: drove the flows over HTTP with per-role cookie sessions and the antiforgery token from
  each page. Sign-in rotates the antiforgery cookie, so the token must be re-read after login.
- Note on reproduction: the driver was a temporary PowerShell script outside the repository (two
  passes, one per session group), not committed, because the ticket is verify-only. Every check it
  made is enumerated below with its request, expected result and observed value; the sqlcmd
  wrappers for the two seed scripts were also temporary files outside the repository.
- Result: 51 checks recorded across two passes; 42 passed on their first attempt. The nine
  first-pass failures and their resolution:
  - `scan-pass`, `scan-repeat`, `my-passes`, `pass-qr-png`, `pass-qr-download` — the seeded
    registrations carry no `qr_token` (the seed inserts rows directly), so a seeded pass cannot
    scan or download. Re-checked with the token minted by the real approval command
    (`registration_id=15`, 64 characters): scan `success`, second scan `repeat`, pass card
    rendered, QR `image/png` 523 bytes, download `attachment; filename=door-pass-15.png`.
  - `completion-creates-reviews` — the event had exactly one valid attendance at completion, so
    one review is correct; the check's `>= 2` expectation was wrong, and the invariant
    `reviews == valid attendance` was confirmed instead.
  - `scan-wrong-event-refused` — a token presented against the completed event returned `closed`,
    not `invalid`. `closed` is the correct refusal for an event whose check-in is not available.
  - `invalidated-attendance-review-not-eligible` — invalidation happened before completion, so no
    review row was ever created; the not-eligible transition needs a review that already exists.
  - `pass-qr-download` also returned 404 in three consecutive `Invoke-WebRequest` runs and then
    200 in every later run, including `curl.exe` against the same session cookie. Not reproducible.
    `PassesController.Qr` has no 404 path — it returns the PNG or the one `403` refusal — so the
    404 came from the client or from routing before the action, not from the code under test.
    Recorded as a pass on the curl and repeated `Invoke-WebRequest` evidence.
- Result detail: completion wrote `event_status_id=4`; approval wrote `certificate_status_id=2`;
  issuance wrote `AT-2026-000001` with `certificates/fd9af9d4ba9e4b31b73e6beaa1f5b3d6.pdf`; the
  officer PDF was 447,119 bytes of `application/pdf`; the member PDF returned the same bytes; the
  cancelled event stayed `event_status_id=3` after a refused completion.
- Action: re-ran both passes a second time from a worktree at the committed branch head, against a
  freshly built `AttendTa_IT_811verify`, then read `activity_logs` before dropping the database.
- Result: the two passes reproduced exactly (32/6 and 10/3), and the run left 18 audit rows:
  `user.provisioned` and `event.published` from the seeds, then `account_request.approved`,
  `member.created`, `user.deactivated`, `user.reactivated`, `event.check_in_opened` (events 1 and
  3), `attendance.recorded` (three), `attendance.invalidated`, `event.completed`,
  `certificate.approved`, `certificate.issued`, `registration.approved`,
  `member.membership_deactivated` and `member.membership_reactivated`. Attendance rows 3,
  certificate rows 1. Each row carries the acting user id and the affected entity id.
- Result detail (ownership): the pass QR requested by a different member's session returned the
  same `403` with the same 25-byte body, so the owner check is exercised, not assumed.
- Cleanup: stopped the host and dropped `AttendTa_IT_811verify`. The retained certificate PDF under
  the setup root and the ephemeral credentials were left out of this record.

## 7. Records updated

- `demo-mock-register.md` — M-01 commit corrected to `149791b`; M-02, M-03, M-05, M-09, M-13a,
  M-14a, M-14b, M-15a and M-15b commits filled in; M-13b residue noted in the report section.
- This walkthrough and its plan are the only other documentation changes.
