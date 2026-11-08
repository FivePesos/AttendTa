---
title: 'Check-in refactor sweep'
type: 'refactor'
ticket: '9'
created: '2026-11-08'
status: 'built'
baseline_revision: '596f09ccdc606b185e62a7c93fe27a13043d9a45'
route: 'full'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: []
review_loop_iteration: 0
context:
  - '_bmad-output/initiative-attendta/deferred-work.md'
  - '_bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The finale sweep for check-in still has concrete leftovers: two dead screen-model members (`IsCompleted`, `CanChangeWindow`), a dead pass field (`GateCode`), an unused attendance read (`GetValidAttendedRegistrationIdsAsync`), a redundant published-page guard, a verbatim-duplicated management actor guard in two services, a duplicated published/open-window pre-check in the scan and manual paths, and two member-history endpoints missing the no-store header their sibling has. No token logging or schema drift remains.

**Approach:** Delete the dead members and their only consumers, extract one shared management actor guard and one shared check-in-window pre-check, and align the two endpoints' cache headers. No behavior, message, route, contract or schema change.

## Boundaries & Constraints

**Always:** Preserve every user-visible message, route, JSON outcome shape, lock order, transaction boundary and audit payload. Actor still comes only from `ICurrentActor`. Keep the build at zero warnings and zero errors. Keep the demo seam (`DemoData/`, `--demo-pass-token`) untouched.

**Never:** Delete or alter DemoData or `DemoStore` (8.13 owns the seam), add features, change refusal wording, restyle frontend surfaces (8.12), touch the schema or EF mappings, or write to ticket files.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Scan, unpublished event | event not Published | `Closed` with "Check-in is not available for this event." | unchanged |
| Scan, closed window | Published, window closed | `Closed` with "...Open it before scanning." | unchanged |
| Manual check-in, closed window | Published, window closed | `Closed` with "...Open it before checking in." | unchanged |
| Non-management actor | member role or no actor | `DomainException(Forbidden, "Access denied.")` | unchanged |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/ServiceModels/CheckInScreenViewModel.cs:18-19` — `IsCompleted` and `CanChangeWindow`; neither is ever assigned.
- `ASI.Basecode.WebApp/Views/CheckIn/Index.cshtml:34-37,48` — only reader of `IsCompleted`; only reader of `CanChangeWindow`.
- `ASI.Basecode.Services/ServiceModels/MyEventsViewModels.cs:143` — `PassViewModel.GateCode`, never assigned.
- `ASI.Basecode.WebApp/Controllers/PassesController.cs:113` — only other `GateCode` reference.
- `ASI.Basecode.Data/Interfaces/IAttendanceRepository.cs:35-39`; `ASI.Basecode.Data/Repositories/AttendanceRepository.cs:72-83` — `GetValidAttendedRegistrationIdsAsync`, no caller.
- `ASI.Basecode.Services/Services/CheckInService.cs:33-43,63,77-86` — empty-page guard (keep: the count and page reads can disagree), constant `CanChangeWindow`, duplicated pre-check.
- `ASI.Basecode.Services/Services/CheckInLookupService.cs:80-90` — duplicated pre-check.
- `ASI.Basecode.Services/Services/AttendanceRecordingService.cs:217-242`; `AttendanceInvalidationService.cs:127-152` — identical `RequireManagement()`.
- `ASI.Basecode.WebApp/Controllers/MyEventsController.cs:44-54` — `Registrations`/`Attendance` lack the `[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]` that `Certificates` has at `:58`.
- Reuse `ICurrentActor`, `DomainException`, `OutcomeCode`, `RoleCodes`; do not introduce new dependencies.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.Services/ServiceModels/CheckInScreenViewModel.cs` — delete `IsCompleted` and `CanChangeWindow` — dead members.
- [ ] `ASI.Basecode.WebApp/Views/CheckIn/Index.cshtml` — delete the `IsCompleted` branch; pass `CanChange = true` to `_CheckInControl` — the published-only selector always allows the toggle.
- [ ] `ASI.Basecode.Services/ServiceModels/MyEventsViewModels.cs` — delete `GateCode` — never assigned.
- [ ] `ASI.Basecode.WebApp/Controllers/PassesController.cs` — delete the `GateCode` copy — follows the model.
- [ ] `ASI.Basecode.Data/Interfaces/IAttendanceRepository.cs` and `ASI.Basecode.Data/Repositories/AttendanceRepository.cs` — delete `GetValidAttendedRegistrationIdsAsync` and the then-unused `Data.Queries` using — no caller.
- [ ] `ASI.Basecode.Services/Services/ManagementActorGuard.cs` (new) — internal static `RequireUserId(ICurrentActor)` returning the user id or throwing `DomainException(Forbidden, "Access denied.")` — one guard.
- [ ] `ASI.Basecode.Services/Services/AttendanceRecordingService.cs` and `AttendanceInvalidationService.cs` — replace each private `RequireManagement()` with the shared guard — remove 52 duplicated lines.
- [ ] `ASI.Basecode.Services/Services/CheckInWindowGuard.cs` (new) — internal static `ClosedOutcomeAsync(IEventRepository, int eventId, string windowMessage, CancellationToken)` returning the closed outcome or `null` — one pre-check.
- [ ] `ASI.Basecode.Services/Services/CheckInService.cs` — keep the empty-page guard (the count and page reads can disagree), drop the duplicate published/open-window pre-check and call the shared guard with the existing messages — remove duplication without exposing an empty-list index.
- [ ] `ASI.Basecode.Services/Services/CheckInLookupService.cs` — call the shared pre-check with the existing messages — remove duplication.
- [ ] `ASI.Basecode.WebApp/Controllers/MyEventsController.cs` — add the no-store attribute to `Registrations` and `Attendance` — deferred-work item.

**Acceptance Criteria:**
- Given the solution, when it builds, then there are zero warnings and zero errors.
- Given the removed members, when searching the repo, then `CanChangeWindow`, `GateCode` and `GetValidAttendedRegistrationIdsAsync` have no matches.
- Given a management actor, when scanning or manually checking in, then every message and outcome is identical to the baseline.
- Given a member actor, when calling the recording or invalidation service, then the same `Forbidden` "Access denied." is thrown.
- Given `MyEvents/Registrations` and `MyEvents/Attendance`, when inspected, then both carry the no-store cache attribute.

## Implementation Notes

- Implemented as planned, with two verification corrections found at run time.
- The integration project is also `xunit.v3` MTP with `OutputType=Exe`, so `dotnet test` reports "Zero tests ran" and exits 5. Run the test host exe directly instead.
- The whole-tree `git grep` matches two frozen historical plan documents; scope it to `*.cs`/`*.cshtml` so only source is judged.
- `DemoData/` and `--demo-pass-token` were left untouched (8.13 owns the seam).
- `AdministrationCommandGuards.RequireManagement` in the Administration module is functionally identical to the new `ManagementActorGuard`; left separate to avoid coupling modules.
- The diff excludes the pre-existing `AGENTS.md` edit and the untracked `_bmad/custom/*.toml` files; both predate this build and are not part of the change.

## Plan Change Log

## Review Triage Log

- **Finding:** `CheckIn/Index.cshtml` `CanChange = true` is a user-visible behaviour change because `CanChangeWindow` was never assigned. **Verdict:** false. The baseline assigned `CanChangeWindow = true` in `CheckInService.GetScreenAsync` — the diff removes that exact line — so `_CheckInControl` already rendered the toggle; the literal preserves behaviour. The "assigned nowhere" claim grepped the post-change tree.
- **Finding:** removing the `published.Count == 0` guard lets `SelectEvent` index `published[0]` on an empty list. **Verdict:** medium. Verified: `CountPublishedAsync` and `GetPublishedPageAsync` are separate reads, so a concurrent complete/cancel of the last published event yields `total > 0` with an empty page and `published[0]` throws a 500. Patch: restore the guard.
- **Finding:** `CheckInWindowGuard.ClosedOutcomeAsync` returns `null` from a non-nullable `Task<CheckInOutcomeViewModel>`. **Verdict:** low. Verified against `docs/CODING_STANDARDS.md:26` (represent optional values explicitly). Patch: file-local `#nullable enable` and `Task<CheckInOutcomeViewModel?>`.
- **Finding:** the acceptance criterion "no matches" is unmet because the identifiers remain in historical plan documents. **Verdict:** false. No source reference remains; the hits are frozen planning records, not the removed members, and the criterion's subject is the code.

## Verification

**Commands:**
- `dotnet build "ASI.Basecode.sln" -v q --nologo` — expected: Build succeeded, 0 Warning(s), 0 Error(s)
- `& "tests\ASI.Basecode.Services.Tests\bin\Debug\net10.0\ASI.Basecode.Services.Tests.exe"` — expected: Total 315, Failed 0
- `$env:ATTENDTA_TEST_SQL="Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; & "tests\ASI.Basecode.IntegrationTests\bin\Debug\net10.0\ASI.Basecode.IntegrationTests.exe"` — expected: Total 423, Failed 0 (plain `dotnet test` reports "Zero tests ran" — known MTP trap)
- `git grep -n "CanChangeWindow\|GateCode\|GetValidAttendedRegistrationIdsAsync" -- "*.cs" "*.cshtml"` — expected: no matches
