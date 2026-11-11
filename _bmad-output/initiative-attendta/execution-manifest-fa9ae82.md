# Execution manifest — 8.4 whole suite on SQL Server

Status: executed, all green. This is execution evidence, reused by 8.6–8.8. Any change under
`ASI.Basecode.*/` or `tests/` after this commit invalidates it and requires a new run; documentation
and evidence-only commits do not.

| Field | Value |
|---|---|
| Ticket | 8.4 / #77 — Whole IntegrationTests suite execution on SQL Server |
| Branch | `feature/8.4-integration-suite-execution` |
| Tested commit | `fa9ae82a1ea97e83e924c5b2ef2217dcd3bff1ec` (`upstream/dev` tip) |
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
| `npm --prefix ASI.Basecode.WebApp run css:build` | 0 | Tailwind build succeeded | 0.4 s |
| `npm --prefix ASI.Basecode.WebApp run test:js` | 0 | 5 tests, 5 passed, 0 failed | 0.1 s |

## Scope covered

- Seat overbooking and promotion races, duplicate submission/decision, duplicate attendance and
  certificate issuance, and last-admin concurrency all ran inside the 479-test SQL suite with zero
  duplicates and zero overbooking.
- The seven important cases added by 8.5 are inside the 479; the case-to-test mapping is in
  `acceptance-coverage-matrix.md`, section 5.
- PDF checks ran with the real Playwright renderer and Poppler.
- No test project, harness, source, schema or CI change is part of this record.

## Notes and non-defects

- The integration log contains 13 EF `fail:` lines and 9 SQL error 334 entries produced by
  deliberately created test-only triggers. The triggers raise 51044 (`TR_TestFailVenueUpdate`),
  51052 (`TR_TestFailEventCoverUpdate`), 51053 (`TR_TestFailEventPublicationAudit`) and 51991 (the
  readiness trigger); error 334 is the doomed-transaction error SQL Server reports after those
  statements fail. These are the negative-path assertions the tests create on purpose; every
  affected test passed.
- The npm commands were run from `ASI.Basecode.WebApp`; the `--prefix` form above is equivalent and
  is what CI uses. No dependency-install step is recorded.
- Six EF `Model.Validation[20601]` warnings about database-generated defaults with no sentinel value
  are non-failing.
- `ASI.Basecode.WebApp/wwwroot/css/app.css` appeared modified after `css:build` only because of a
  CRLF stat-cache mismatch; the content is byte-identical to HEAD and the file is not committed.
- No environment blockers: LocalDB, Chromium and Poppler were all present.
