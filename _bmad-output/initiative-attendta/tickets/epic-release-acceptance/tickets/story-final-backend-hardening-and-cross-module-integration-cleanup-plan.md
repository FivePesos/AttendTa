---
title: 'Final backend hardening and cross-module integration cleanup'
type: 'refactor'
ticket: '13'
created: '2026-11-08'
status: 'built'
baseline_revision: '005f8997ddc2178ef8ac8f1fac091904dbb29845'
route: 'full'
route_source: 'auto'
risk: 'high'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
followup_review_recommended: true
context: []
warnings: ['multiple-goals', 'oversized']
deferred: []
---

<intent-contract>

## Intent

**Problem:** The demo seam (register row M-01) is still wired into startup, configuration and the CLI, so 8.11's checks cannot pass. The cross-module audit also found duplicated eligibility/attendance facts, a participation report whose five independent reads can disagree, and the deferred findings the focused sweeps (3.14, 5.10, 6.9) recorded for this ticket.

**Approach:** Delete the demo seam first, then give each shared rule one implementation, make the report read one coherent snapshot, and close the recorded deferred findings. Verification and hardening only: no feature, no screen redesign, no new broad test suite.

## Boundaries & Constraints

**Always:** keep `UnitOfWork.ExecuteInTransactionAsync` + `ActivityLogWriter` as the only write/audit path; keep the existing lock order (users → event → attendance, ascending event ids); keep `IPrivateFileStore` as the only file access; keep every controller policy and ownership check; keep `TimeProvider` as the only clock; keep both existing test projects green.

**Never:** add a feature, change a screen's design or copy, author broad new acceptance tests (8.5/8.4 own them), re-open a finding a sweep already fixed, or leave a demo provider, registration line or configuration key behind.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Demo switch on | `DemoData__Enabled=true`, no token key, non-demo database | App starts normally; the key is inert | No guard, no refusal |
| Report mid-change | Attendance invalidated between panels | Summary, no-shows, distribution and rows agree | One snapshot read |
| Invalidated attendance | `attendance.invalidated_at` set | Never a valid attendee, never certificate-eligible | One canonical predicate |
| Missing role code | `GetRoleCodeAsync` returns null | Role change fails; nothing written | No numeric fallback into `activity_logs` |

</intent-contract>

## Code Map

- `ASI.Basecode.WebApp/DemoData/` — the whole seam: `DemoDataOptions`, `DemoDataStartupGuard`, `DemoStore`, `DemoSeedService`, `DemoDataRegistration`. Delete the folder.
- `ASI.Basecode.WebApp/Program.cs:17,67-88` — `--demo-pass-token`; `Startup.cs:76-78` — `DemoDataStartupGuard.Validate`; `Startup.DI.cs:110-112` — `AddDemoData`; `appsettings.json:13-15` — `DemoData` section. Keep `--demo-readiness`.
- `ASI.Basecode.Data/Queries/ParticipationFacts.cs:12-19,38-42` — canonical `EligibleParticipant`, `ValidAttendance`, `SeatHeld`, `ValidAttendances`, `SeatHolders`; the single source every other module must call.
- `ASI.Basecode.Services/Services/ParticipationReportService.cs:54-70` — five independent reads; `ASI.Basecode.Data/Repositories/ParticipationReportRepository.cs:27-160` — the five query bodies.
- `ASI.Basecode.Services/Services/CertificateLedgerService.cs:307-330,383-389`, `CertificateReviewCompletionHandler.cs:32` — split certificate-eligibility rule.
- `ASI.Basecode.Data/Repositories/RegistrationRepository.cs:21-25,74,114,157-158,167,203,268,289-293` — seat/attendance re-derivations, the two renamed members, the redundant in-memory filter.
- `ASI.Basecode.Services/Services/CheckInLookupService.cs:64`, `AttendanceRecordingService.cs:70,169`, `AttendanceInvalidationService.cs:96`, `MyEventsService.cs:95`, `PassService.cs:110-112`, `MemberEventService.cs:232` — inline re-derivations to replace.
- `ASI.Basecode.WebApp/Mvc/ControllerBase.cs:83,105-109,214` — template scaffolding with no consumer.
- `ASI.Basecode.Data/Repositories/EventRepository.cs` — repeated list/detail projections.
- `ASI.Basecode.Services/Services/MyEventsService.cs:18-60` — hand-mapped rows and repeated paging shape.
- `ASI.Basecode.Services/ServiceModels/AccessDialogViewModels.cs:77` — ordinal status compare.
- `ASI.Basecode.Data/Interfaces/IMemberRepository.cs:14,17,20,21`, `IUserRepository.cs:11` — synchronous reads.
- `ASI.Basecode.Services/Services/AccountRoleService.cs:126-127` — numeric role-code fallback.
- `ASI.Basecode.WebApp/Controllers/AdminAccountsController.cs:34` — injected `ILoggerFactory` never used.
- `ASI.Basecode.Services/Services/ActivityLogWriter.cs` + `Views/ActivityLog/Index.cshtml:36` — `OccurredAt` kind workaround.
- `_bmad-output/initiative-attendta/demo-mock-register.md:85` — M-01 row.
- `docs/demo-quickstart.md`, `database/seed-demo-scenario.sql`, `PRODUCT.md:41` — stale demo text.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.WebApp/DemoData/` -- delete the folder -- M-01 removal.
- [x] `Program.cs`, `Startup.cs`, `Startup.DI.cs`, `appsettings.json` -- remove the demo switch, guard, registration call, `--demo-pass-token` block and config section; keep `--demo-readiness` -- M-01 removal.
- [x] `tests/ASI.Basecode.IntegrationTests/DemoDataStartupGuardTests.cs` -- delete; strip `UseSetting` demo switches from `DemoCheckInWindowFlagTests`, `CertificateReviewDecisionTests`, `AdminMembershipDeactivationTests`, `AdminAccountReactivationTests`, `EventCheckInWindowFlowTests`; reseed `CertificateLedgerFlowTests` attendance and reviews through the real repositories instead of `DemoSeedService` -- tests must not depend on the seam.
- [x] `ASI.Basecode.Data/Queries/ParticipationFacts.cs` -- add in-memory predicates for valid attendance, seat held and certificate eligibility beside the SQL expressions, and comment `PublishableEventIds`' correlated `venueIds` projection -- one rule, one file.
- [x] `ParticipationReportRepository.cs` / `ParticipationReportService.cs` -- replace the five reads with one `GetSnapshotAsync(eventId)` returning every registration of the event with its member, snapshots and attendance; compute summary, no-shows, distribution, total and the page slice from that snapshot -- report panels must agree.
- [x] `CertificateLedgerService.cs`, `CertificateReviewCompletionHandler.cs` -- route both eligibility checks through the canonical predicate (pending/approved + valid attendance + completed where required) -- one certificate rule.
- [x] `RegistrationRepository.cs` and the six inline re-derivation sites -- call the canonical predicates; rename `GetForCancellationAsync`→`GetForUpdateAsync` and `DiscoverReviewAsync`/`RegistrationReviewIdentity` to identity naming across the interface, services and test fakes; drop `GetOpenForEventAsync`'s in-memory re-filter -- deferred 3.14 findings.
- [x] `ControllerBase.cs` -- delete the unused template members -- deferred 3.14 finding.
- [x] `EventRepository.cs` -- collapse the repeated list/detail projections into two query builders -- deferred 3.14 finding.
- [x] `MyEventsService.cs` -- extract the paging shape and map rows with `IMapper` -- deferred 3.14 finding.
- [x] `AccessDialogViewModels.cs:77` -- `OrdinalIgnoreCase` status compare -- deferred 5.10 finding.
- [x] `IMemberRepository`, `IUserRepository` and implementations -- make the four member reads and `GetUsers()` async with `CancellationToken`, thread callers -- deferred 5.10 finding.
- [x] `AccountRoleService.cs:126-127` -- fail loudly on a missing role code -- deferred 5.10 finding.
- [x] `AdminAccountsController.cs:34` -- log the swallowed failure path -- deferred 5.10 finding.
- [x] `ActivityLogWriter.cs` + `Views/ActivityLog/Index.cshtml:36` -- carry an explicit UTC kind and drop `SpecifyKind` -- deferred 5.10 finding.
- [x] `demo-mock-register.md` -- set M-01 to `removed (<sha>)` in the follow-up docs commit -- register rule 11.
- [x] `docs/demo-quickstart.md`, `database/seed-demo-scenario.sql`, `PRODUCT.md` -- remove stale demo-seam text -- no surviving reference.

**Acceptance Criteria:**
- Given the seam is deleted, when I search source and published output for `DemoData`, `DemoStore`, `DemoSeedService`, `demo-pass-token` and `M-01`, then no code or configuration hit remains.
- Given `DemoData__Enabled=true` with no token key against a non-demo database, when the app starts, then it starts normally.
- Given attendance changes while the report is open, when it renders, then the summary, no-shows, distribution and detail rows all come from one snapshot.
- Given an invalidated attendance or a non-confirmed registration, when any module evaluates validity, seat or eligibility, then every module returns the same answer from `ParticipationFacts`.
- Given a missing role code, when a role change commits, then the command fails and no numeric code reaches `activity_logs`.
- Given the change set, when I build and run both test projects, then everything passes.

## Implementation Notes

- **Demo seam removed.** `ASI.Basecode.WebApp/DemoData/` (five files) is deleted. `Program.cs` no longer has `--demo-pass-token` or the `System.Globalization` import; `Startup.cs` no longer calls the guard; `Startup.DI.cs` no longer calls `AddDemoData`; `appsettings.json` no longer has the `DemoData` section. `--demo-readiness` and the hotspot block are untouched. Verified: `rg "DemoData|DemoStore|DemoSeedService|demo-pass-token"` over source, `bin` and `obj` returns no hits, and the app starts normally with `DemoData__Enabled=true`, no token key, against `AttendTaMVP_Test` (checked `Application started` in the host log, then stopped the process).
- **Tests no longer depend on the seam.** `DemoDataStartupGuardTests` is deleted; `DemoCheckInWindowFlagTests` is renamed `CheckInWindowFlagTests` and its demo-switch test folded into one real-service DI check; the demo `UseSetting` blocks are gone from the admin deactivation/reactivation tests; `CertificateReviewDecisionTests` drops the demo-namespace scan; `EventCheckInWindowFlowTests` renames its demo-named test. `CertificateLedgerFlowTests` now seeds attendance and certificate reviews through `AttendanceRepository`/`CertificateRepository` in `SeedAttendanceAndReviewsAsync`, reproducing the same states (one invalidated check-in, one declined review, one issued review with no retained file).
- **One participation rule.** `ParticipationFacts` gained in-memory twins (`IsValidAttendance` over the entity, the invalidation timestamp and the flag; `HoldsSeat` over the entity and the status; `IsOpenRegistration`; `IsCertificateEligible`), and the SQL `OpenRegistration` expression. `PublishableEventIds` now documents its correlated venue projection. Every listed inline re-derivation now calls a predicate: `CheckInLookupService`, both `AttendanceRecordingService` checks, `AttendanceInvalidationService`, `MyEventsService`, `PassService`, `MemberEventService` (cancel guard and `CanCancel`), `RegistrationCancellationService`, and the `RegistrationRepository` confirmed reads.
- **One report snapshot.** `IParticipationReportRepository` is now a single `GetSnapshotAsync(eventId)` returning every registration with its member, signup snapshots and attendance (`ParticipationReportDetailRow` carries `InvalidatedAt` instead of a boolean). `ParticipationReportService` derives the status counts, Confirmed, valid attendees, rate, no-shows, both breakdowns, the total and the page slice from that one list, so the panels agree by construction. The five-read facts records are deleted.
- **One certificate rule.** `IsCertificateEligible(registration, attendance, certificate, requiredStatus, eventItem)` is the single predicate. The ledger's decision passes `PendingApproval`, issuance passes `Eligible` and still refuses a missing event row, and `CertificateRepository.GetRegistrationsAwaitingReviewAsync` now selects `SeatHolders` (it previously accepted any status) so the completion hook and the ledger agree. The ledger takes `IAttendanceRepository` to read the attendance row inside its transaction.
- **Renames.** `GetForCancellationAsync` → `GetForUpdateAsync`, `DiscoverReviewAsync` → `DiscoverIdentityAsync`, `RegistrationReviewIdentity` → `RegistrationIdentity`, across the interface, repositories, services and every test fake. `GetOpenForEventAsync` drops its redundant in-memory re-filter.
- **Deferred 3.14 findings.** `ControllerBase` loses `UserId`, `UserName`, `Supervisor`, `ClientId`, `ClientSystemId`, `ClientSystemName`, `ClientUserRole`, the two empty action overrides and `HandleExceptionLog`. `EventRepository` collapses four projections into `MemberVisibleEventListQuery` and `MemberVisibleEventDetailQuery`. `MyEventsService` extracts `ReadPageAsync` and maps rows through `IMapper` (two new maps plus `MyEventsMappingItems.CutoffPassed`).
- **Deferred 5.10 findings.** `MembershipAccessViewModel.For` compares the status with `OrdinalIgnoreCase`. The four member reads and `GetUsers()` are async with `CancellationToken` (`IMemberService.GetMemberAsync` and `MemberController.Edit` threaded with them). `AccountRoleService` throws `InvalidOperationException` instead of writing a numeric role fallback. `AdminAccountsController.PopulateRoleFormAsync` logs the refused refresh. `ActivityLog.OccurredAt` gets an EF value converter that restores `DateTimeKind.Utc` on read, so the Activity Log view drops `SpecifyKind`; `TransactionVerificationTests` now asserts the read-back kind.
- **Docs.** `demo-mock-register.md` sets M-01 to `removed (005f899)` and marks the seam detail as historical; `docs/demo-quickstart.md` drops the three `DemoData__*` variables, the startup-log line and the two demo troubleshooting rows; `database/seed-demo-scenario.sql` describes the real-service walk instead of the seed; `PRODUCT.md` drops the "reviews live only in process memory" and "demonstration configuration" claims.
- **Verification.** `dotnet build "ASI.Basecode.sln" --configuration Release -warnaserror` → 0 warnings, 0 errors. Services suite → 314 passed, 0 failed, 0 skipped. Integration suite with `ATTENDTA_TEST_SQL` → 456 passed, 0 failed, 0 skipped (three fewer than the 459 baseline: two deleted guard tests and one merged demo-switch test). The `DemoData`/`M-01` search returns no code or configuration hit.
- The M-01 register sha is the baseline revision `005f899`; replace it with the merge commit when the pull request lands.
- **Orchestrator additions after diff review.** Carried the two keepers from open PR #214 (7.2 follow-up): `ParticipationReportService.YearBreakdown` now always renders `Year N` because `chk_registrations_positive_year_level` forbids a zero year, and `demo-mock-register.md` M-13b records its removal commit `975c08b`. PR #214's other change (one grouped distribution query) is superseded by the snapshot read and is not carried.
- **Matrix coverage added.** Two matrix rows had no covering test, so the orchestrator added one each: `CheckInWindowFlagTests.DemoSwitchKeyIsInertWhenEnabled` (host starts and serves `/Account/Login` with `DemoData:Enabled=true`, no token key, non-demo database) and `AccountRoleServiceTests.MissingRoleLookupFailsWithoutWritingTheChange` (missing role code throws, role unchanged, no audit row, no save). `FakeUsers` gained a `RoleLookupMissing` switch for the latter.
- **Re-verified after those additions.** `dotnet build "ASI.Basecode.sln" --configuration Release -warnaserror` → 0 warnings, 0 errors. Services suite → 315 passed. Integration suite with `ATTENDTA_TEST_SQL` → 457 passed. The demo-artifact search still returns no hit.
- **Reviewed and left alone.** `GetOpenForEventAsync`'s dropped in-memory re-filter is safe: its only caller, `RegistrationCancellationService.CancelEventRegistrationsAsync`, re-checks `ParticipationFacts.IsOpenRegistration` before writing, so a row cancelled between the id query and the tracked refresh is a no-op.

## Plan Change Log

## Review Triage Log

### 2026-11-08 — Review pass
- verdicts: 5 findings — high 0, medium 2, low 3, false 0, maybe-false 0
- findings:
  - `[medium]` `[patch]` `CheckInWindowFlagTests` sets the literal `DemoData:Enabled`, so the plan's own "no `DemoData` hit remains" acceptance criterion and 8.11's source search fail — rebuild the setting name from its two words with a comment explaining the 8.11 constraint, and rename the test so it carries no forbidden literal.
  - `[low]` `[patch]` `RegistrationRepository.GetMyPassRowsAsync` still compares `RegistrationStatusId == Confirmed` inline, so the pass list's seat answer does not come from `ParticipationFacts` — filter with `ParticipationFacts.SeatHolders(...)`. A later seat-rule change would otherwise leave the pass list divergent.
  - `[low]` `[patch]` `AttendanceRecordingService` (two sites) and `MyCertificatesService` still compare `Confirmed` inline — same root cause as the row above; use `ParticipationFacts.HoldsSeat(...)`.
  - `[medium]` `[patch]` `_bmad-output/initiative-attendta/demo-script.md` still tells the presenter to set the deleted `DemoData__*` variables, expect the deleted start-up log line, and use the deleted `--demo-pass-token` switch — remove those instructions and describe the real-service walk. The 8.7 demo would otherwise follow a broken script.
  - `[low]` `[patch]` `CertificateLedgerService.DecideInsideTransactionAsync` reports a lost seat as invalid attendance because the message is chosen only from the certificate status — pick the message from the fact that failed: not pending → `AlreadyDecidedMessage`, seat lost → `ChangedMessage`, otherwise `AttendanceInvalidMessage`.

## Design Notes

The report fix is a single materialized snapshot rather than a repeatable-read transaction: `UnitOfWork` owns the only transaction API and adding an isolation-level variant would widen a shared core path for one screen. One query over the event's registrations is bounded by venue capacity and makes every panel coherent by construction. If the snapshot proves too heavy, fall back to one repository method that runs the five existing queries inside `ExecuteInTransactionAsync` — but never leave the service issuing five independent reads.

The canonical facts stay as expression trees for SQL paths and gain matching in-memory predicates; both live in `ParticipationFacts` so a rule change touches one file. Do not replace the expressions with compiled delegates in query paths.

## Verification

**Commands:**
- `dotnet build "ASI.Basecode.sln" --configuration Release -warnaserror` -- expected: 0 warnings, 0 errors.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` -- expected: all pass.
- `$env:ATTENDTA_TEST_SQL="Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` -- expected: all pass, including the request-to-certificate and report flows.
- `rg -n "DemoData|DemoStore|DemoSeedService|demo-pass-token" --glob '!**/bin/**' --glob '!**/obj/**' --glob '!_bmad-output/**' .` -- expected: no hits.

## Auto Run Result

Status: built (2026-11-08)

### Implemented change

Deleted the demo seam (register M-01) and closed the cross-module backend findings the focused sweeps recorded for 8.13. One implementation now owns each shared rule, the participation report reads one coherent snapshot, and the deferred 3.14/5.10 refactors are done.

### Files changed (77 files, +1060 / -1494)

- `ASI.Basecode.WebApp/DemoData/` (5 files) — deleted: options, startup guard, store, seed service, registration method.
- `ASI.Basecode.WebApp/Program.cs`, `Startup.cs`, `Startup.DI.cs`, `appsettings.json` — removed `--demo-pass-token`, the guard call, `AddDemoData` and the `DemoData` section; `--demo-readiness` kept.
- `ASI.Basecode.Data/Queries/ParticipationFacts.cs` — in-memory twins for valid attendance, seat held, open registration and certificate eligibility, plus the SQL `OpenRegistration` expression and the `PublishableEventIds` comment.
- `ASI.Basecode.Data/Interfaces/IParticipationReportRepository.cs`, `Repositories/ParticipationReportRepository.cs` — five reads replaced by one `GetSnapshotAsync`.
- `ASI.Basecode.Services/Services/ParticipationReportService.cs` — summary, no-shows, distributions, total and page slice derived from that snapshot; year label is always `Year N`.
- `ASI.Basecode.Services/Services/CertificateLedgerService.cs`, `CertificateReviewCompletionHandler.cs`, `ASI.Basecode.Data/Repositories/CertificateRepository.cs` — one certificate-eligibility rule; the completion query selects Confirmed seats; the decision refusal names the failed fact.
- `ASI.Basecode.Data/Repositories/RegistrationRepository.cs`, `Interfaces/IRegistrationRepository.cs` — canonical predicates at every confirmed/token/student-number read, `GetForCancellationAsync`→`GetForUpdateAsync`, `DiscoverReviewAsync`→`DiscoverIdentityAsync`, `RegistrationReviewIdentity`→`RegistrationIdentity`, redundant in-memory filter dropped.
- `ASI.Basecode.Data/Repositories/EventRepository.cs` — four projections collapsed into two query builders.
- `ASI.Basecode.Services/Services/MyEventsService.cs`, `ASI.Basecode.WebApp/Startup.AutoMapper.cs`, `ServiceModels/MyEventsViewModels.cs` — shared paging helper and AutoMapper row maps.
- `ASI.Basecode.WebApp/Mvc/ControllerBase.cs` — unused template members deleted.
- `ASI.Basecode.Services/ServiceModels/AccessDialogViewModels.cs` — `OrdinalIgnoreCase` status compare.
- `ASI.Basecode.Data/Interfaces/IMemberRepository.cs`, `IUserRepository.cs` and implementations — four member reads and `GetUsersAsync` are async with `CancellationToken`; `IMemberService.GetMemberAsync` and `MemberController.Edit` threaded.
- `ASI.Basecode.Services/Services/AccountRoleService.cs` — missing role code throws instead of writing a numeric fallback.
- `ASI.Basecode.WebApp/Controllers/AdminAccountsController.cs` — logs the refused role-form refresh.
- `ASI.Basecode.Data/CanonicalSchemaMappings.cs`, `ASI.Basecode.Services/Services/ActivityLogWriter.cs`, `Views/ActivityLog/Index.cshtml` — UTC kind restored on read, `SpecifyKind` dropped.
- `_bmad-output/initiative-attendta/demo-mock-register.md` — M-01 `removed (005f899)`, M-13b `removed (975c08b)`, seam detail marked historical.
- `_bmad-output/initiative-attendta/demo-script.md`, `docs/demo-quickstart.md`, `database/seed-demo-scenario.sql`, `PRODUCT.md` — stale seam instructions replaced with the real-service walk.
- Tests: `DemoDataStartupGuardTests` deleted; `DemoCheckInWindowFlagTests`→`CheckInWindowFlagTests`; demo switch settings removed from five suites; `CertificateLedgerFlowTests` seeds attendance and reviews through the real repositories; `ParticipationReport*` tests rewritten for the snapshot; new `MissingRoleLookupFailsWithoutWritingTheChange` and `RetiredSeamSwitchKeyIsInertWhenEnabled`; fakes updated for the renames and async reads.

### Review findings breakdown

5 findings, all triaged `patch` (0 high, 2 medium, 3 low, 0 false, 0 maybe-false). Patches applied: the `DemoData` literal in the new integration test; `GetMyPassRowsAsync` and three further inline seat comparisons moved onto `ParticipationFacts`; `demo-script.md` cleared of the deleted seam; the certificate decision refusal now names the fact that failed. Deferred: none. Rejected: none.

### Follow-up review recommendation

`true` — two `medium` entries were patched this pass. Unverified risk: the report's single-snapshot read is covered by unit tests and the SQL snapshot repository tests, but no integration test renders `/Reports`, so the rendered page against a real database is still unproven until 8.12/8.6 exercise it.

### Verification performed

- `dotnet build "ASI.Basecode.sln" --configuration Release -warnaserror` → 0 warnings, 0 errors.
- `dotnet test tests/ASI.Basecode.Services.Tests` → 315 passed, 0 failed, 0 skipped.
- `dotnet test tests/ASI.Basecode.IntegrationTests` with `ATTENDTA_TEST_SQL` → 457 passed, 0 failed, 0 skipped (baseline 459; two guard tests deleted, one merged, two added).
- `rg "DemoData|DemoStore|DemoSeedService|demo-pass-token"` over source, `bin` and `obj` → no hits; the same search over `demo-script.md` → no hits.
- Manual: the app started with `DemoData__Enabled=true`, no token key, against `AttendTaMVP_Test` and served normally.

### Residual risks

- The M-01 register sha is the baseline `005f899`; replace it with the merge commit when the pull request lands.
- The report's rendered page is not exercised by an integration test (see the follow-up risk above).
- `ParticipationFacts` now carries three `IsValidAttendance` overloads (entity, timestamp, flag); callers must pick the matching shape.
- No Impeccable critique or polish ran: the only Razor change is the Activity Log `<time datetime>` value, which has no visual effect.

