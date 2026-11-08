---
title: 'Administration backend refactor sweep'
type: 'refactor'
ticket: '10'
created: '2026-11-08'
status: 'built'
baseline_revision: '108ac583f19a11622653c02567707f543cd3fffe'
route: 'full'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The finale sweep found concrete duplication and dead code in the Administration backend: three services carry the same reason and actor guards, the access viewmodels expose members nothing uses, `Startup.DI.cs` registers two concrete services nobody resolves, and `ActivityLogWriter` repeats fully-qualified constants and names its clock unlike the module.

**Approach:** Extract the duplicated guards into one internal helper, delete the dead members and redundant registrations, and apply mechanical using/naming cleanup in the audit writer. No behavior, message, outcome code, transaction, lock order or audit payload changes.

## Boundaries & Constraints

**Always:** Preserve every message, `OutcomeCode`, `ModelState` field key, transaction boundary, lock order (`attendta:last-admin` before the `users` row), audit action/entity/payload, and the `IActivityLogWriter` and `IRegistrationCancellationService` contracts. The role form keeps its `Reason` key; the dialogs keep `reason`.

**Never:** No schema, no view/Razor/CSS change, no controller behavior change, no repository signature change, no `<Nullable>` enablement, no change to the audit guard chain or the cancellation path. Tests are limited to the human-directed `AccessDialogServiceTests`.

</frozen-after-approval>

## Code Map

- `Services/Services/AccountRoleService.cs:20,160-199`, `AccountAccessService.cs:22,161-200`, `MembershipAccessService.cs:24,157-198` — three copies of `ReasonMaxLength`, `RequiredReason` (keys `nameof(...Reason)` vs `"reason"`) and the actor guard (`RequireAdministratorActor` ×2, `RequireManagementActor` ×1).
- `Services/ServiceModels/AccessDialogViewModels.cs:36,66-67,73` — uncalled `Empty` factories; settable `IsActive` whose comment still describes the removed demo overlay. `AccountSummaryViewModel.cs:12-13` — settable `IsActive`, never assigned after construction.
- `WebApp/Startup.DI.cs:44,46` — concrete registrations; only the interfaces at `:45,47` are resolved.
- `Services/Services/ActivityLogWriter.cs:16,24-35` — `clock` parameter; twelve fully-qualified `ASI.Basecode.Resources.Constants.AuditActions.*` references.
- `Services/Services/AccessDialogService.cs:42-44` — unlocked `CountActiveAdministratorsAsync` display read, unexplained; the write path re-checks under the lock (`AccountAccessService.cs:96-103`).
- Reuse `DomainException`, `OutcomeCode`, `RoleCodes`, `ICurrentActor`. Do not touch the audit guard ordering, `RegistrationCancellationService`, repositories, controllers, views or tests.

## Tasks & Acceptance

**Execution:**
- [x] `Services/Services/AdministrationCommandGuards.cs` (new) — `internal static` `ReasonMaxLength`, `RequiredReason(reason, fieldName)`, `RequireAdministrator(ICurrentActor)`, `RequireManagement(ICurrentActor)` with today's exact messages and codes — one home for three copies.
- [x] `Services/Services/AccountRoleService.cs` — delete the private guards, call the helper with `nameof(ChangeAccountRoleViewModel.Reason)` — keeps the role form key.
- [x] `Services/Services/AccountAccessService.cs`, `MembershipAccessService.cs` — delete the private guards, call the helper with `"reason"` — keeps the dialog key.
- [x] `Services/ServiceModels/AccessDialogViewModels.cs`, `AccountSummaryViewModel.cs` — remove both `Empty` factories, make `IsActive` `init`, drop the stale overlay comment — deletes dead members.
- [x] `WebApp/Startup.DI.cs` — remove the two concrete registrations — interfaces are the only resolution points.
- [x] `Services/Services/ActivityLogWriter.cs` — add `using ASI.Basecode.Resources.Constants;`, shorten the twelve references, rename `clock` to `timeProvider` — mechanical consistency.
- [x] `Services/Services/AccessDialogService.cs` — one comment on the unlocked count: best-effort display read, re-checked under the lock — records the deliberate choice.

**Acceptance Criteria:**
- Given the extracted guards, when any command gets a blank/over-long reason or a disallowed actor, then message, outcome code and field key are identical to `108ac58`.
- Given the cleaned viewmodels and DI block, when the solution builds and the access screens load, then no `Empty` member, concrete resolution or post-construction `IsActive` assignment is needed.
- Given the audit writer edit, when any audited command runs, then the same `activity_logs` row is written with the same action, entity and payload.

## Implementation Notes

- The actor guard is factored once more than the plan names: `RequireAdministrator` and
  `RequireManagement` both delegate the authentication/`UserId` part to a private
  `RequireAuthenticatedActor`, which still throws `Forbidden` / "Access denied." for an
  unauthenticated actor, a throwing `UserId` and a non-positive id. Role comparison happens after,
  so the observable exception is unchanged for every case.
- `RequireAdministrator` is called for its return value and also for its side effect in
  `AccountRoleService.GetRoleChangeAsync`, matching the old `RequireAdministratorActor()` call.
- The `For` factories on both dialog viewmodels were already uncalled but stay: the plan only
  removes `Empty`, and `For` is the documented composition helper.
- `ActivityLogWriter` line endings and the CRLF file style were preserved by the mechanical replace.
- Human-directed addition (2026-11-08): `tests/ASI.Basecode.Services.Tests/AccessDialogServiceTests.cs`
  covers the dialog service directly, which had no unit test before. It asserts the two not-found
  outcomes, the last-active-Administrator flag (alone, with another admin, inactive, non-admin), the
  active/inactive preview gating, the linked-user preview for membership, case-insensitive status,
  and field mapping. This supersedes the plan's original "no new tests" boundary at the human's
  direction.
- Correction to the note above: the `For` factories *are* used by `Views/AdminAccounts/Index.cshtml`
  and `Views/Member/Index.cshtml`; they stay because the list rows need them, not only as a helper.

## Verification Results

- `dotnet build` — Build succeeded, `0 Warning(s)`, `0 Error(s)`.
- `dotnet test tests/ASI.Basecode.Services.Tests` — 304 passed, 0 failed (includes
  `AccountRoleServiceTests` blank/over-long/forbidden cases asserting `Field` is
  `nameof(ChangeAccountRoleViewModel.Reason)`).
- `dotnet test tests/ASI.Basecode.IntegrationTests --filter "FullyQualifiedName~Admin|FullyQualifiedName~DeactivationPreview|FullyQualifiedName~ActivityLogViewer"`
  — 63 passed, 0 failed.
- `dotnet test tests/ASI.Basecode.Services.Tests --filter "FullyQualifiedName~AccessDialogServiceTests"`
  — 11 passed, 0 failed (new suite).

## Plan Change Log

## Review Triage Log

Review pass 1 — quick lens, 2026-11-08. Verdicts: high 0, medium 0, low 1, false 8, maybe-false 0.

| # | Finding | Verdict | Route | Evidence |
|---|---------|---------|-------|----------|
| 1 | `RequireManagement` is a tenth guard while nine other modules keep private copies; "one home" untrue | false | — | The frozen intent scopes the sweep to the three Administration command services, and the helper's comment says "Administration access and role commands". The other guards belong to other modules and are outside the named scope; no behavior or developer harm at the cited location. |
| 2 | `AccessDialogService` uses `OrdinalIgnoreCase` while `MembershipAccessViewModel.For` uses ordinal `== "active"`; the new test pins only the service | low | defer | Verified both derivations differ. `MemberRepository` emits lowercase status, so they agree today; the `For` factory predates this sweep. Recorded in `deferred-work.md`. |
| 3 | Fake declares `Task<AccountListItem?>` against a `Task<AccountListItem>` interface, hiding that the real repository never returns null | false | — | `AccountAdministrationRepository.GetAccountAsync:54-57` returns `null` for `accountId <= 0` and projects through the join otherwise, so the nullable fake matches real behavior. The Data project disables nullable annotations; the signature is unchanged. |
| 4 | No test proves the dialog read does not take the last-administrator lock | false | — | `FakeAccounts.AcquireLastAdministratorLockAsync` throws `NotSupportedException`, so any lock acquisition on the dialog read path would fail `AccessDialogServiceTests` immediately. The guard already exists. |
| 5 | `For` omits `IsLastActiveAdministrator`/`AffectedRegistrations`, so the row partial renders an incomplete dialog | false | — | `Views/AdminAccounts/_AccessActions.cshtml` reads only `Model.IsActive` and `Model.AccountId`; the dialog itself is fetched from `IAccessDialogService` on open. No bad outcome at the cited location. |
| 6 | `OccurredAt` kind is unchanged while the writer is called "fully cleaned" | false | — | The plan's task scopes the writer edit to the using, the twelve references and the `clock` rename, and Design Notes explicitly defer the `OccurredAt` kind fix. The finding itself records no behavior claim. |
| 7 | No test asserts that resolving the concrete services fails after the DI removal | false | — | Removing a registration cannot break a resolution that does not exist: controllers resolve only the interfaces, and no `GetRequiredService` for the concrete types exists. No demonstrated bad outcome. |
| 8 | No test pins the `"reason"` field key in the two access services | false | — | The code passes the literal `"reason"` at both call sites; the absence of a test is not a defect, and the finding states the code is correct. |
| 9 | `RequireAdministrator` splits the old combined `userId <= 0 || role` check | false | — | The finding's own trace shows identical observable behavior for every case; recorded as checked-and-clean. |

No patch, bad_plan or intent_gap entries. No loopback.

## Design Notes

The helper takes `fieldName` because the forms differ: the role form binds a model property (`Reason`), the dialogs post a raw `reason` field. Passing the key preserves `ModelState` instead of "standardizing" it into a regression.

`IsActive` becomes `init` because the demo overlay that mutated it was deleted in 5.5/5.7; the command writes the entity and the dialog is rebuilt from a fresh read. The unlocked last-Administrator count stays and is only documented: making it authoritative would put the app lock on a read path, which the architecture reserves for writes.

Deferred, not agreed findings for this ticket: sync non-cancellable member reads, the numeric role-code fallback, the missing `LogWarning` in `AdminAccountsController`, and the `OccurredAt` kind fix in the view. Each changes behavior or crosses a layer; they stay recorded for 8.13.

## Verification

**Commands:**
- `dotnet build` — expected: `0 Warning(s)`, `0 Error(s)`.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — expected: all pass, including `AccountRoleServiceTests` blank/over-long/forbidden cases.
- `$env:ATTENDTA_TEST_SQL = "Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --filter "FullyQualifiedName~Admin|FullyQualifiedName~DeactivationPreview|FullyQualifiedName~ActivityLogViewer"` — expected: all pass (access/cascade smoke).
