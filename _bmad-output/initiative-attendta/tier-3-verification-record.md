# Tier 3 verification record — manual system integration and regression

Ticket 8.8 / #81. Authored and executed 2026-11-11 against the tested commit below, on a local
workstation. This is the **release-gate decision record**, not a third checklist: it cites the
evidence 8.4, 8.6, 8.7, 8.2 and 8.3 already produced, executes only the rows nothing else covers
(the integrated build and the database migration checks), and leaves the human Tier 3 decision
blank.

The record is complete when every Tier 3 row carries a result. The release is **not** approved by
this file; only the human sign-off in §8 can close the gate.

## 1. Record header

| Field | Value |
|---|---|
| Ticket | 8.8 / #81 — Tier 3 manual system integration and regression verification |
| Tested commit | `84b3f54e58d127f993d55c45a9287dc9b267dda5` (`dev` tip, PR #224 merge) |
| Date | 2026-11-11 |
| Operator | AI agent (subagent), executing under this ticket plan; human Tier 3 sign-off pending |
| OS | Windows 11 Pro, build 10.0.26200 |
| Runtime | .NET SDK 10.0.403, Node v24.12.0 |
| Database | SQL Server LocalDB `MSSQLLocalDB`; disposable `AttendTa_IT_t3mig523f203e` created for T3-03 and dropped afterwards |
| CLI | `sqlcmd` 15.0.1300.359 |
| Environment | Local developer workstation. **This is not the demo host** and supplies no host, phone or restore evidence. |
| Build under test | `dotnet build ASI.Basecode.sln --configuration Release` — 0 warnings, 0 errors |

Every result below belongs to this one commit and this environment. A source or test change after
`84b3f54` invalidates the rows that depend on it and requires a new tested-commit reference.

## 2. Result legend

| Result | Meaning |
|---|---|
| **PASS** | Executed on the tested commit and environment above; the evidence field names the observation. |
| **FAIL** | Executed; the observed result contradicts the expected outcome. The row names the defect. |
| **BLOCKED** | Not executable here. The row names the exact missing input and the evidence file that owns it. A closed issue, a merged tool or a planning document is never substituted. |
| **REUSED** | Covered by a cited record whose own validity rule still holds at this commit; not re-run for this ticket. |
| **NOT RUN** | Deliberately not executed and not covered elsewhere. No row below is left NOT RUN. |
| **PASS (none required)** | Assessed and found to need no execution: nothing landed that would create a retest obligation. |
| **BLANK** | Reserved for a decision this record cannot make. The human Tier 3 decision is the only such row. |

## 3. Evidence reuse and validity

Each source is cited with its own commit and the verdict that decides whether it may be reused at
`84b3f54`.

| Source | Ticket | Tested commit (record commit) | Validity verdict at `84b3f54` |
|---|---|---|---|
| `execution-manifest-fa9ae82.md` | 8.4 / #77 | `fa9ae82`, 2026-11-11 (record commit `970781b`) | **VALID.** 321 service / 479 integration, 0 failed, 0 skipped. Its rule is that a change under `ASI.Basecode.*/` or `tests/` invalidates it and documentation/evidence-only commits do not. `git diff --stat fa9ae82..84b3f54` touches five files, all under `_bmad-output/` (this manifest, the 8.6 record, two 8.4 review records and one ticket plan). No source, test, schema or CI file changed, so the suite is **not** re-run. |
| `manual-acceptance-checklist.md` | 8.6 / #79 | `d8046bf`, 2026-11-11 (record commit `211a1f2`) | **VALID.** 8 PASS, 5 BLOCKED, 0 FAIL, evaluator decision blank. Since `d8046bf` only `211a1f2` and the PR #224 merge landed, both documentation-only. |
| `acceptance-coverage-matrix.md` | 8.5 / #78 | `95493ac`, 2026-11-10 | **VALID as the case-id list only.** It is a coverage map, not execution evidence; its §1/§3 ids are what §4 of this record cross-references. Its "JPEG only end-to-end" premise is stale (8.6 M-MD-01 disproved it); the stale sentence is 8.5's record to correct, not a Tier 3 defect. |
| `finale-test-cases.md` §"One finale execution programme" | 8.5 / #78 | `95493ac`, 2026-11-10 | **VALID as the step definition.** Step 5 is 8.7 and step 6 is this ticket. |
| `docs/hotspot-https-evidence.md` | 8.2 / #75 | `fb783f1`, 2026-11-08 | **VALID as a BLOCKED record.** Rows 1–12 all BLOCKED: no host, no hotspot adapter, no certificate thumbprint, no phone. A closed #75 is not host evidence. |
| `docs/demo-host-readiness-evidence.md` | 8.14 / #150 | `fb783f1` (last touched; evidence dated 2026-11-06) | **VALID as a BLOCKED record.** Checks 1–10 all BLOCKED: no demo host, no `AttendTa_Demo`, no runtime SQL login, no staged browser, no disconnection method. Its own header states that closed tracking issues, including #74, are not proof. |
| `docs/storage-survival-evidence.md` | 8.3 / #76 | `8247832`, 2026-11-08 | **VALID as a BLOCKED record.** Every host stage BLOCKED; its own local section states the helper proves fixtures only and is not host acceptance. |
| `_bmad-output/initiative-attendta/demo-script.md` | 8.13 / #151 | `149791b`, 2026-11-08 | **VALID as the journey definition.** It defines the beats 8.7 must demonstrate; it is not execution evidence for them. |
| `demo-walkthrough-evidence.md` | 8.7 / #80 | `84b3f54`, 2026-11-11 (record commit `f50d8d5`) | **VALID.** The continuous request → approval → registration → check-in → certificate → report journey, executed on the same tested commit as this record, with 36 FR rows, 26 functional-ID rows and seven blocked parts that are the same seven rows T3-05 carries. It is execution evidence, not a definition, so it settles T3-04. |

## 4. Tier 3 matrix

Tier 3 scope, per `docs/git_qa_guidelines.pdf`: cross-feature compatibility, the integrated build
and database migrations on the shared `dev` build before `dev` goes to `main`.

| Id | Tier 3 row | Result | Evidence |
|---|---|---|---|
| T3-01 | Cross-feature compatibility on the integrated build | **REUSED** | 8.6 UI-01 executed every Administrator, Officer and Member route on `d8046bf` with 200 and its own heading, crawled 78 distinct same-origin links with no dead link, and refused wrong-role and anonymous requests (`/Account/AccessDenied`, `/Account/Login?ReturnUrl=%2FMember`). 8.6 UI-02 executed keyboard, dialog-focus, validation and 200 % zoom at 1440/1024/768/390 with zero document-level overflow. Cross-feature behaviour is owned by the 8.4 suite: 321 service and 479 integration tests, 0 failed, 0 skipped, including the SM-4 concurrency cases (zero overbooking, zero duplicate attendance, zero duplicate certificates, zero duplicate registrations) and last-admin concurrency. Both sources are valid at `84b3f54` per §3. |
| T3-02 | Integrated build on the tested commit | **PASS** | Executed on `84b3f54`, local workstation, .NET SDK 10.0.403 / Node v24.12.0. `dotnet build ASI.Basecode.sln --configuration Release` → `Build succeeded. 0 Warning(s) 0 Error(s)`, 2.97 s. `npm --prefix ASI.Basecode.WebApp run css:build` → Tailwind v4.3.4, `Done in 282ms`, exit 0. `npm --prefix ASI.Basecode.WebApp run test:js` → 5 tests, 5 pass, 0 fail, exit 0. `dotnet test --project tests/ASI.Basecode.Services.Tests/... --no-build --no-restore --configuration Release` → 321 total, 321 succeeded, 0 failed, 0 skipped, exit 0. Commands and output in §5. |
| T3-03 | Database migration checks | **PASS** | Executed on the disposable `AttendTa_IT_t3mig523f203e`, created from `database/attendta-mvp-schema.sql` and dropped afterwards. The prefix, idempotence and over-length assertions are already automated by `VenueImagePathMigrationTests.MigrationIsIdempotentAndLongKeysAbortBeforeAnyRowChanges` inside the 479 suite; this run re-observed them on a real `sqlcmd` invocation and added the one assertion no test covers — the allow-list guard. Results: legacy `venues/<name>` keys prefixed to `uploads/venues/<name>`; rerun changed nothing (idempotent); the guard refused a non-allow-listed database with exit 1; an over-length path aborted with exit 1 and no rows changed. Commands and observed output in §6. No other database was touched. |
| T3-04 | Request-to-certificate/report integration journey | **REUSED** | 8.7 / #80 executed the continuous request → approval → registration → check-in → certificate → report journey on the same tested commit `84b3f54`, covering all 26 functional IDs plus FR-35/FR-36, and recorded it in `demo-walkthrough-evidence.md` with 36 FR rows and 26 functional-ID rows. Its seven blocked parts are the same seven rows T3-05 carries, so the journey itself is proven and only the host, phone and human rows remain. `finale-test-cases.md` §"One finale execution programme" step 5 assigns this row to 8.7; that record now exists and is valid at `84b3f54` per §3. |
| T3-05 | Environment, device and host rows | **BLOCKED** | Seven rows remain blocked on inputs no local run can supply. ATT-03 (camera scan on the real hotspot HTTPS origin) — `docs/hotspot-https-evidence.md` rows 1–12 BLOCKED. OPS-01 (demo-host readiness under the runtime identity) — `docs/demo-host-readiness-evidence.md` checks 1–10 BLOCKED. OPS-02 (backup, restart/redeploy, restore to a separate target) — `docs/storage-survival-evidence.md` every stage BLOCKED. M-AUD-01 (`activity_logs` column permissions under the runtime SQL login) — `docs/demo-host-readiness-evidence.md` check 3 BLOCKED. M-AUTH-01 (8-hour, non-sliding cookie expiry) — 8.6 recorded the cookie flags but not the 8-hour idle observation. CERT-03 signatory (name, title and signature approved by the human) — 8.6 recorded the configured values, human approval NOT RECORDED. M-ADM-01 mixed race (a demotion racing a deactivation of the last two administrators) — 8.6 recorded it BLOCKED as not reproducible by hand; the two same-kind races are automated, the mixed pair is not. Per-row detail and unblocking action in §7. |
| T3-06 | Affected retests after fixes | **PASS (none required)** | No source, test, schema or CI change landed between `fa9ae82` and `84b3f54` (`git diff --stat fa9ae82..84b3f54` = five documentation files under `_bmad-output/`), so the 8.4 manifest is not invalidated. 8.6 recorded 0 FAIL and 0 defects, so no fix produced a retest obligation. The single 8.6 note — the stale "JPEG only" sentence in `acceptance-coverage-matrix.md` — is a documentation correction owned by 8.5 and retests nothing. |
| T3-07 | Human Tier 3 decision | **BLANK** | Reserved for the team lead and testers. §8 carries the sign-off block with the decision, date and affected-retest fields empty. This record does not decide it and does not state that the release is approved. |

## 5. T3-02 — integrated build, commands and observed output

Run from the repository root on `84b3f54`:

| Command | Exit | Observed output |
|---|---|---|
| `dotnet build ASI.Basecode.sln --configuration Release` | 0 | `Build succeeded. 0 Warning(s) 0 Error(s)`; six projects built (Resources, Data, Services, Services.Tests, WebApp, IntegrationTests); elapsed 2.97 s |
| `npm --prefix ASI.Basecode.WebApp run css:build` | 0 | `tailwindcss v4.3.4`; `Done in 282ms` |
| `npm --prefix ASI.Basecode.WebApp run test:js` | 0 | `tests 5`, `pass 5`, `fail 0`, `cancelled 0`, `skipped 0`, `todo 0`; the five dialog/antiforgery contract tests |
| `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` | 0 | `total: 321`, `succeeded: 321`, `failed: 0`, `skipped: 0` |

`ASI.Basecode.WebApp/wwwroot/css/app.css` was not left modified by `css:build`; `git status` shows
no tracked change.

The 479-test integration suite was **not** re-run: the 8.4 manifest is still valid under its own
rule (§3), and re-running it while valid is outside this ticket's boundaries.

## 6. T3-03 — database migration checks, commands and observed output

Target: a disposable database created for this check only, named `AttendTa_IT_t3mig523f203e`. The name
matches the `AttendTa_IT_` prefix and length the guard checks; the suffix is not a GUID, which the
guard does not require. It was created empty, loaded from the canonical schema, exercised, and
dropped at the end. `AttendTaMVP_Test`, `AttendTa_Demo`, `AsiBasecodeDB` and every other existing
database were not connected to.

```powershell
$db = "AttendTa_IT_t3mig523f203e"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -Q "CREATE DATABASE [$db];"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -d $db -i "database\attendta-mvp-schema.sql"
```

Schema load: exit 0, eight lookup-table seed batches applied.

Fixture, written to a throw-away temp file (not committed) and run with
`sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -d $db -i "<temp>\t3-fixture.sql"`: one `users` row for the
required `created_by`, then four venues — two legacy `venues/...` keys (one with a space in the name),
one already-migrated `uploads/venues/...` key, and one unrelated `other/...` key.

```sql
SET NOCOUNT ON;

INSERT INTO dbo.users (full_name, email, password_hash, role_id, is_active)
VALUES (N'Tier Three Operator', N't3.operator@example.edu', N'$argon2id$fixture-not-a-real-hash', 1, 1);

DECLARE @u int = SCOPE_IDENTITY();

INSERT INTO dbo.venues (room_name, capacity, image_path, created_by) VALUES
 (N'Legacy One',       120, N'venues/legacy-one.jpg',               @u),
 (N'Legacy Two',        80, N'venues/legacy two.png',               @u),
 (N'Already Migrated',  50, N'uploads/venues/already-migrated.jpg', @u),
 (N'Unrelated Path',    30, N'other/not-a-venue-path.jpg',          @u);

SELECT venue_id, room_name, image_path FROM dbo.venues ORDER BY venue_id;
```

### Assertion 1 — legacy keys are prefixed

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -d $db -i "database\migrate-venue-image-paths.sql"
```

Exit **0**. Rows observed afterwards:

| venue_id | image_path | length |
|---|---|---|
| 1 | `uploads/venues/legacy-one.jpg` | 29 |
| 2 | `uploads/venues/legacy two.png` | 29 |
| 3 | `uploads/venues/already-migrated.jpg` | 35 |
| 4 | `other/not-a-venue-path.jpg` | 26 |

Both legacy keys gained exactly one `uploads/` prefix; the already-migrated key was not
double-prefixed and the unrelated key was untouched.

### Assertion 2 — rerun changes nothing

The same script was run a second time: exit **0**, and all four `image_path` values were byte-identical
to assertion 1. The `WHERE image_path LIKE N'venues/%'` predicate no longer matches, so the update is
a no-op.

### Assertion 3 — the guard refuses a non-allow-listed database

A second disposable database, `AttendTa_ITprobe_587c8f0b` (deliberately outside the allow-list),
was created empty and the migration was run against it:

```powershell
$probe = "AttendTa_ITprobe_" + ([guid]::NewGuid().ToString("N").Substring(0,8))
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -Q "CREATE DATABASE [$probe];"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -d $probe -i "database\migrate-venue-image-paths.sql"
```

Exit **1**, message:

```
Msg 50000, Level 16, State 1
This venue image path migration may only run in AttendTaMVP_Test, AttendTa_IT_<guid>, or AttendTa_Demo.
```

The guard is the first statement after `SET NOCOUNT`/`SET XACT_ABORT` (lines 9–15) and returns
before the `venues` check or any DML, so nothing in the probe database was read or written.

### Assertion 4 — an over-length path aborts with no rows changed

An over-length legacy row was inserted into the first database (`venues/` + 493 characters = 500
characters, which would migrate to 508 and exceed the `nvarchar(500)` column):

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -d $db -Q "SET NOCOUNT ON; INSERT INTO dbo.venues (room_name, capacity, image_path, created_by) VALUES (N'Over Length', 10, N'venues/' + REPLICATE(CAST(N'a' AS nvarchar(4000)), 493), 1);"
```

Exit **0**; the new row reported `len = 500` and `LEFT(image_path, 20) = venues/aaaaaaaaaaaaa`. A
snapshot was then taken before and after the migration:

```sql
SELECT COUNT(*) AS rows_total,
       SUM(CASE WHEN image_path LIKE N'uploads/venues/%' THEN 1 ELSE 0 END) AS prefixed,
       SUM(CASE WHEN image_path LIKE N'venues/%' THEN 1 ELSE 0 END) AS legacy,
       SUM(LEN(image_path)) AS len_sum,
       CHECKSUM_AGG(BINARY_CHECKSUM(image_path)) AS path_checksum
FROM dbo.venues;
```

| Snapshot | rows_total | prefixed | legacy | len_sum | path_checksum |
|---|---|---|---|---|---|
| Before | 5 | 3 | 1 | 619 | -950962929 |
| After | 5 | 3 | 1 | 619 | -950962929 |

Migration, run with the same command as assertion 1, exited **1** with this output:

```
venue_id    migrated_length
----------- ---------------
          5             508
Msg 50000, Level 16, State 1
Venue image path migration stopped: one or more prefixed paths exceed the 500-character column limit. No rows were changed.
```

The affected venue id and its would-be length are reported, the transaction rolls back and the
checksum is unchanged — no row was migrated.

### Cleanup

Both disposable databases were dropped:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -Q "ALTER DATABASE [$db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$db]; ALTER DATABASE [$probe] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$probe];"
```

Exit 0. `sys.databases` afterwards lists no `t3mig` or `ITprobe` database. An unrelated
`AttendTa_IT_999543524a754ff280742afd0463a4d9` left over from an earlier integration run was present
before this check and was left untouched.

## 7. Blockers

Every BLOCKED row above, with its owner and the action that would unblock it. None of these is
inferable from a closed issue or a merged tool.

| Blocker | Rows affected | Missing input | Owning evidence file | Owner | Unblocking action |
|---|---|---|---|---|---|
| ATT-03 phone camera on the real HTTPS origin | T3-05 | A real demo host with the hotspot HTTPS binding, its generated root CA installed on a real officer phone, a valid printed pass, and the app reachable from the hotspot subnet | `docs/hotspot-https-evidence.md` (rows 1–12) | 8.2 / #75 owner | Provision the host, generate the root CA and server certificate, install the root on the phone, and record rows 1–12 with redacted output |
| OPS-01 demo-host readiness | T3-05 | A provisioned demo host, a dedicated Windows service identity, a dedicated SQL login, `AttendTa_Demo`, a staged `PLAYWRIGHT_BROWSERS_PATH`, and an approved disconnection method | `docs/demo-host-readiness-evidence.md` (checks 1–10) | 8.14 / #150 owner | Provision the host and identity, run `--demo-readiness` as the service identity, and record checks 1–10 |
| OPS-02 backup / restart / restore | T3-05 | A real host, an approved backup destination, an approved empty restore target, a maintenance window and an approver | `docs/storage-survival-evidence.md` | 8.3 / #76 owner | Execute the `storage-backup-restore.md` runbook end to end and record every stage with redacted evidence |
| M-AUD-01 `activity_logs` column permissions | T3-05 | A provisioned runtime SQL login on a demo host | `docs/demo-host-readiness-evidence.md` check 3 | 8.14 / #150 owner | Connect as the runtime login and record effective table- and column-level `UPDATE`/`DELETE` permissions on `activity_logs` |
| M-AUTH-01 8-hour non-sliding expiry | T3-05 | An 8-hour idle observation of a real browser session | `manual-acceptance-checklist.md` §5 M-AUTH-01 (flags recorded, expiry not observed) | 8.6 / #79 owner or the human tester | Sign in, leave the session idle past 8 hours, re-request a management route and record the outcome |
| CERT-03 signatory approval | T3-05 | The human confirming the authorized signatory name, title and signature as official | `manual-acceptance-checklist.md` §5 CERT-03 signatory sub-field | Project owner / human signatory | Confirm or replace `Christian Kyle Ducay` / `Organization President` / the packaged signature asset, and record the approval |
| M-ADM-01 mixed race | T3-05 | A demotion and a deactivation of the last two administrators interleaving inside the same transaction window, which two live sessions cannot produce by hand | `manual-acceptance-checklist.md` §5 M-ADM-01 (mixed race BLOCKED; the two same-kind races are automated in `AdminAccountDeactivationTests` / `AdminRoleAssignmentTests`) | 8.6 / #79 owner or the human tester | Either script the mixed pair as a SQL-gated concurrency test in `tests/ASI.Basecode.IntegrationTests` or record a human decision that the two same-kind automated races are sufficient |

## 8. Sign-off block

| Field | Value |
|---|---|
| Record complete | Yes — every Tier 3 row carries a result, and every BLOCKED row names its missing input and owning file |
| Tested commit | `84b3f54e58d127f993d55c45a9287dc9b267dda5` |
| Operator | AI agent (subagent), 2026-11-11 |
| Team lead | — (blank, to be filled by the human) |
| Tester 1 | — (blank) |
| Tester 2 | — (blank) |
| Tier 3 decision | — (blank: PASS / FAIL / retest) |
| Date | — |
| Affected retests | — (blank until the decision is recorded) |

**Sign-off is blocked, not approved.** T3-05 is open; the release gate is not reached by
this record alone. The decision fields are deliberately empty and no row above asserts that the
release is approved.

## 9. Defects found

No product defect was found in this ticket's scope. The integrated build is clean and the venue
image path migration behaves exactly as documented on a disposable database.

| Id | Row | Observation | Severity | Filed |
|---|---|---|---|---|
| — | — | No defect found during this run | — | — |

The stale "JPEG only end-to-end" sentence in `acceptance-coverage-matrix.md` §1 and §6, already
recorded by 8.6 M-MD-01, remains a documentation correction owned by 8.5. It is not a product
defect and does not affect any Tier 3 row.
