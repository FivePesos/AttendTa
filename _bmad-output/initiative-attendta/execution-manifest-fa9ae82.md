# Execution manifest — 8.4 whole suite on SQL Server

Status: executed, all green. This is execution evidence, reused by 8.6–8.8; a code change after
this commit invalidates it and requires a new run.

| Field | Value |
|---|---|
| Ticket | 8.4 / #77 — Whole IntegrationTests suite execution on SQL Server |
| Branch | `feature/8.4-integration-suite-execution` |
| Tested commit | `fa9ae82a1ea97e83e924c5b2ef2217dcd3bff1ec` (origin/dev tip) |
| Date | 2026-11-11 |
| OS | Windows 11 Pro 24H2, build 10.0.26100 |
| Runtime | .NET SDK 10.0.403, Node v24.6.0 |
| Database | SQL Server LocalDB `MSSQLLocalDB`, master connection via `ATTENDTA_TEST_SQL` |
| PDF tooling | Poppler 23.13.0 (`pdftotext`, `pdfinfo`, `pdffonts` on PATH) |
| Browser | Playwright chromium-1228 / chromium-1248 |

## Commands and results

| Command | Exit | Result | Duration |
|---|---|---|---|
| `dotnet build "ASI.Basecode.sln" --configuration Release` | 0 | Build succeeded, 0 warnings, 0 errors | 8.1 s |
| `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` | 0 | 321 passed, 0 failed, 0 skipped | 5.3 s |
| `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` | 0 | 479 total, 479 passed, 0 failed, 0 skipped, 0 not run | 413.6 s |
| `npm run css:build` | 0 | Tailwind build succeeded | 0.4 s |
| `npm run test:js` | 0 | 5 tests, 5 passed, 0 failed | 0.1 s |

## Scope covered

- Seat overbooking and promotion races, duplicate submission/decision, duplicate attendance and
  certificate issuance, and last-admin concurrency all ran inside the 479-test SQL suite with zero
  duplicates and zero overbooking.
- PDF checks ran with the real Playwright renderer and Poppler.
- No test project, harness, source, schema or CI change is part of this record.

## Notes and non-defects

- The integration log contains 13 EF `fail:` lines and 9 SQL error 334 entries from deliberately
  created test-only triggers (`TR_TestFailVenueUpdate`, `TR_TestFailEventPublicationAudit`,
  `TR_TestFailEventCoverUpdate`, and the readiness trigger). These are the negative-path assertions
  the tests create on purpose; every affected test passed.
- Six EF `Model.Validation[20601]` warnings about database-generated defaults with no sentinel value
  are non-failing.
- `ASI.Basecode.WebApp/wwwroot/css/app.css` appeared modified after `css:build` only because of a
  CRLF stat-cache mismatch; the content is byte-identical to HEAD and the file is not committed.
- No environment blockers: LocalDB, Chromium and Poppler were all present.
