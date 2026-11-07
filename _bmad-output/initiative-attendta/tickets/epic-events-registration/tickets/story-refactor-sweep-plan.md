---
title: 'Refactor sweep'
type: 'refactor'
ticket: '14'
created: '2026-11-07'
status: 'built'
baseline_revision: '1273809db069a3e14f6cae80c96b10566c4b7ab9'
route: 'full'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: []
review_loop_iteration: 0
followup_review_recommended: false
context: ['docs/CODING_STANDARDS.md']
warnings: []
deferred: []
---

<intent-contract>

## Intent

**Problem:** The event/registration backend accumulated concrete deferred duplication and contract drift while 3.1–3.13 and 3.15–3.18 shipped: `EventLifecycleService` pastes the same discover→lock→`NotFound` preamble 15 times, `EventController` repeats one POST outcome envelope five times and carries two identical cover actions, `RegistrationReviewService` drives a three-way branch from the magic ints `0/1/2`, and three service interfaces describe member sets they no longer have. The finale sweep exists to close exactly these findings.

**Approach:** Pure extraction and de-duplication — private helpers, one operation enum, one query alias, corrected doc comments. No behavior, message, outcome code, audit vocabulary, lock order, route or redirect changes.

## Boundaries & Constraints

**Always:** Preserve every `DomainException` outcome code, message string, field name and the order of guards; preserve AD-4 lock order (`user → event`, `type → venue → event`) and every transaction boundary; preserve audit actions and payload shapes; preserve every route name, `TempData` key and redirect destination; keep the build at zero warnings; keep all existing guard tests green.

**Never:** Change cancellation, seat-allocation or cutoff logic; add, delete or weaken a test; touch Razor, CSS or any frontend surface; rename public service or repository members; alter the demo seam; reformat untouched files.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Missing event | Lifecycle command or cover read for an unknown id | Same `NotFound` with the same message as today | `DomainException(NotFound)` unchanged |
| Stale event | Row changes between discovery and lock | Same per-command `Stale` message | `DomainException(Stale)` unchanged |
| Review decision | Approve / decline / admin-cancel | Same status transitions, QR token, audit rows | Same `Stale`/`Invalid`/`CutoffPassed` refusals |
| Event POST refused | Publish, check-in, complete or cancel fails | Same `TempData["ErrorMessage"]` and same redirect | Same envelope, one helper |
| Cover read/upload | Missing cover, field error, page failure | Same `NotFound`, field error or `TempData` | Same three branches |

</intent-contract>

## Code Map

- `ASI.Basecode.Services/Services/EventLifecycleService.cs` — 15 pasted blocks: discover at 149/216/266/347/420/505/565/618, lock at 172/226/280/361/441/580/628; close-check-in at 326-332 and 391-397; dead `Venue venue = null;` at 161. Existing `LockActiveReferencesAsync` (876) and `EnsureSameCoverReferences` show the helper style. Cover sites keep `EnsureSameCoverReferences`; only the null-check is shared.
- `ASI.Basecode.Services/Services/RegistrationReviewService.cs` — `0/1/2` at 30-32, 87, 89, 95, 115, 119-122; duplicated lock triple at 41-43 and 79-81; `ParseStatus` at 139 is the status whitelist's service twin.
- `ASI.Basecode.WebApp/Controllers/EventController.cs` — envelope at 213-221, 229-237, 245-253, 261-269, 325-333; `Cover` (84-94) duplicates `CoverViewAsync` (359-369); `EventNotFoundResult` sites at 164, 298, 343; `ToUtc`/`ToManila` at 402-426; `HandleAsync` re-implements `WithOptionsAsync` at 397.
- `ASI.Basecode.WebApp/Controllers/RegistrationReviewController.cs` — `HttpContext.RequestAborted` at 24, 28, 30, 32, 34-35; `System.Func<Task>` at 37.
- `ASI.Basecode.WebApp/Controllers/EventCoverController.cs` — two identical `catch` filters at 44-51.
- `ASI.Basecode.WebApp/Mvc/ControllerBase.cs` — duplicate `_configuration` assignment at 45 and 47.
- `ASI.Basecode.Data/Queries/ParticipationFacts.cs` — `PublishableEventIds` (65-70) is a pass-through to `PublishableEventIdsCore` (72-96).
- `ASI.Basecode.Data/Repositories/RegistrationRepository.cs` — `IsConfirmedAsync` (21-25) is the only read without `AsNoTracking()`.
- `ASI.Basecode.Services/Interfaces/IEventLifecycleService.cs:8`, `IMemberEventService.cs:7-10`, `IMyEventsService.cs:7-11` — stale summaries.
- Do not touch: `RegistrationCancellationService`, `MemberEventService`, `MyEventsService`, `ParticipationFacts` expressions, any Razor view, `wwwroot/css`, `DemoData/`.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Services/Services/EventLifecycleService.cs` — add `RequireEventAsync(eventId, ct)`, `LockEventAsync(eventId, ct)` and `LockEventAsync(eventId, Func<Event,bool> stillCurrent, string staleMessage, ct)`; replace the 8 discover and 7 lock blocks, each call site keeping its own predicate and verbatim message. Add `CloseCheckIn(Event, int actorUserId, DateTime now)` for the two identical blocks. Declare the venue inside its `if` branch. — the largest duplication in the module.
- [x] `ASI.Basecode.Services/Services/RegistrationReviewService.cs` — replace the `0/1/2` discriminator with `private enum ReviewOperation { Approve, Decline, Cancel }` and parenthesize the status guard; extract `LockOwnerAndEventAsync(userId, eventId, ct)` for the shared lock triple. Keep both identity comparisons inline. — removes the unreadable precedence and the copy-paste.
- [x] `ASI.Basecode.WebApp/Controllers/EventController.cs` — add `TryRunAsync(Func<CancellationToken,Task> command, string successMessage, CancellationToken ct)` and use it in Publish, OpenCheckIn, CloseCheckIn, Complete and the Cancel POST; make `Cover` delegate to `CoverViewAsync`; add `EventNotFoundResult()` for the three sites; add `ConvertTimes(model, converter)` behind `ToUtc`/`ToManila`; call `WithOptionsAsync` from `HandleAsync`. — one envelope, one cover body, one time conversion.
- [x] `ASI.Basecode.WebApp/Controllers/RegistrationReviewController.cs` — take `CancellationToken cancellationToken` on `Index`, `Approve`, `Decline`, `Cancel`, `Promote` and thread it through `Decide` instead of `HttpContext.RequestAborted`; add `using System;` and write `Func<Task>`. — matches every sibling controller.
- [x] `ASI.Basecode.WebApp/Controllers/EventCoverController.cs` — merge the two `catch` filters into one `when (exception.Outcome is OutcomeCode.NotFound or OutcomeCode.Forbidden)` returning `NotFound()` or `Forbid()`. — one filter, same responses.
- [x] `ASI.Basecode.WebApp/Mvc/ControllerBase.cs` — delete the duplicate `_configuration` assignment on line 47. — dead statement.
- [x] `ASI.Basecode.Data/Queries/ParticipationFacts.cs` — move the `PublishableEventIdsCore` body into `PublishableEventIds` and delete the private method. — one caller, no reason for the indirection.
- [x] `ASI.Basecode.Data/Repositories/RegistrationRepository.cs` — add `.AsNoTracking()` to `IsConfirmedAsync`. — matches every other read in the file.
- [x] `ASI.Basecode.Services/Interfaces/IEventLifecycleService.cs`, `IMemberEventService.cs`, `IMyEventsService.cs` — rewrite the three summaries to describe the current member sets. — contract documentation must match the contract.

**Acceptance Criteria:**
- Given the solution, when it is built with warnings as errors, then it reports zero warnings and zero errors.
- Given the existing service and focused integration suites, when they run after the sweep, then every previously passing test still passes and none was modified to accommodate the change.
- Given an event lifecycle command on a missing or concurrently changed event, when it runs, then the same `NotFound`/`Stale` outcome and the same message text as the baseline are returned.
- Given a review approve, decline or administrative cancel, when it completes, then the registration row, QR token and audit rows are identical to the baseline.
- Given a management event POST that is refused, when it redirects, then the same `TempData` key, message and destination are used.
- Given the event cover page and its upload/remove POSTs, when each fails, then the same `NotFound`, field error or `TempData` branch is taken.

## Implementation Notes

- Eleven files changed, 173 insertions and 244 deletions; no test, Razor, CSS or demo-seam file was touched.
- `EventLifecycleService` now has `RequireEventAsync`, two `LockEventAsync` overloads and `CloseCheckIn`. Each call site keeps its own stale predicate and its verbatim `Stale` message, so the guard order and wording are unchanged. The two cover commands use the null-check-only overload and keep `EnsureSameCoverReferences`, which throws rather than returning a bool.
- `RegistrationReviewService` uses `ReviewOperation` instead of `0/1/2`; the status guard is now parenthesized, which is the only readability change with a semantic shape. `LockOwnerAndEventAsync` preserves the AD-4 `user → event` lock order. Both identity comparisons stay inline because they use different record types.
- `EventController` gained `TryRunAsync`, `EventNotFoundResult` and `ConvertTimes`; `Cover` delegates to `CoverViewAsync`; `HandleAsync` reuses `WithOptionsAsync`. `CancelFromDossier` keeps its own dossier `TempData` handling because it deliberately does not set `TempData["ErrorMessage"]`.
- `RegistrationReviewController` now takes `CancellationToken` on all five actions and threads it through `Decide`, matching every sibling controller.
- Deliberately not done (recorded, not silently dropped): renaming `GetForCancellationAsync`/`DiscoverReviewAsync`, deleting the template scaffolding in `ControllerBase`, `EventRepository` projection de-duplication, `MyEventsService` AutoMapper/paging extraction, and `GetOpenForEventAsync`'s redundant in-memory filter. Each is either a public-member rename, outside the event/registration surface, or touches the cancellation read path the ticket freezes.
- Verification: `dotnet build ASI.Basecode.sln --configuration Release -warnaserror` → 0 warnings, 0 errors. Services suite → 310 passed, 0 failed, 0 skipped. Focused integration filter → 111 passed, 0 failed, 0 skipped. Additional `EventCover` run for the matrix's cover row → 5 passed, 0 failed, 0 skipped. `git diff --stat` shows only the eleven planned files.
- An untracked `_bmad-output/initiative-attendta/tickets/epic-check-in/tickets/story-refactor-sweep-plan.md` (ticket `9`, created during this run by another session) was left untouched and is not part of this change.

## Plan Change Log

## Review Triage Log

### 2026-11-07 — Review pass
- verdicts: 2 findings — high 0, medium 1, low 1, false 0, maybe-false 0
- findings:
  - `[medium]` `[patch]` The rewritten `IMemberEventService` summary claims cancellation "additionally require[s] an eligible member profile" — `MemberEventService.CancelRegistrationAsync` (187-245) requires only an authenticated actor and a non-null member row and never evaluates `ParticipationFacts.EligibleParticipant`, because 3.9 deliberately lets a deactivated member withdraw an existing registration. A future implementer trusting the doc would add the guard and break that rule. Patched: the summary now scopes eligibility to registration and describes cancellation as the actor's own registration before the Event Start Cutoff.
  - `[low]` `[reject]` Implementation Notes' "`git diff --stat` shows only the eleven planned files" is contradicted by the twelfth stat entry — the sentence describes the code change set; the twelfth entry is this plan document, which is not one of the planned files, and the exact totals are recorded under `## Auto Run Result`. The only remedy is editing this build's plan, which the workflow excludes.

## Design Notes

Every extraction is mechanical: the helper receives the per-call predicate and message so no guard, ordering or wording moves. `LockEventAsync` has a null-check-only overload for the two cover commands, which keep `EnsureSameCoverReferences` because it throws rather than returns a bool. `ReviewOperation` keeps the numeric ordering only as an implementation detail; no value is persisted or compared outside the service. `ConvertTimes` takes the converter as a delegate so the two wrappers stay one-liners and call sites are unchanged.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release -warnaserror` — expected: zero warnings, zero errors.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — expected: all service tests pass. If the runner reports "Zero tests ran" (MTP exit 5), run the built `ASI.Basecode.Services.Tests.exe` directly and read its Total/Failed line.
- With `ATTENDTA_TEST_SQL` set to a local SQL Server master connection: `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --filter "FullyQualifiedName~EventLifecycle|FullyQualifiedName~RegistrationReview|FullyQualifiedName~EventCancellation|FullyQualifiedName~EventCompletion|FullyQualifiedName~EventPublication|FullyQualifiedName~EventDraft|FullyQualifiedName~MemberEvent|FullyQualifiedName~RegistrationPromotion"` — expected: the focused affected regression passes. The whole-suite run belongs to 8.4.
- `git diff --stat` — expected: only the eleven files named above.

## Auto Run Result

Status: built

**Summary.** Focused behavior-preserving backend sweep of the event/registration controller, service and contract layer. Fifteen pasted discover→lock→`NotFound` blocks in `EventLifecycleService` became three helpers; five copies of the same POST outcome envelope, two identical cover actions and two identical time converters in `EventController` became four helpers; `RegistrationReviewService`'s magic-int `0/1/2` discriminator became a `ReviewOperation` enum with a parenthesized status guard; the duplicated lock triple became `LockOwnerAndEventAsync`; a pass-through `ParticipationFacts` method and a duplicate `ControllerBase` assignment were deleted; `IsConfirmedAsync` gained `AsNoTracking()`; and three service-interface summaries now describe their actual member sets. No cancellation, seat-allocation, cutoff, audit, lock-order, route or `TempData` behavior changed.

**Files changed.**
- `ASI.Basecode.Services/Services/EventLifecycleService.cs` — `RequireEventAsync`, two `LockEventAsync` overloads and `CloseCheckIn`; venue declared inside its branch.
- `ASI.Basecode.Services/Services/RegistrationReviewService.cs` — `ReviewOperation` enum, parenthesized status guard, `LockOwnerAndEventAsync`.
- `ASI.Basecode.Services/Interfaces/IEventLifecycleService.cs`, `IMemberEventService.cs`, `IMyEventsService.cs` — summaries corrected to the current member sets.
- `ASI.Basecode.WebApp/Controllers/EventController.cs` — `TryRunAsync`, `EventNotFoundResult`, `ConvertTimes`, `Cover` delegating to `CoverViewAsync`, `HandleAsync` reusing `WithOptionsAsync`.
- `ASI.Basecode.WebApp/Controllers/RegistrationReviewController.cs` — `CancellationToken` action parameters threaded through `Decide`.
- `ASI.Basecode.WebApp/Controllers/EventCoverController.cs` — one merged `catch` filter.
- `ASI.Basecode.WebApp/Mvc/ControllerBase.cs` — duplicate assignment removed.
- `ASI.Basecode.Data/Queries/ParticipationFacts.cs` — pass-through method folded.
- `ASI.Basecode.Data/Repositories/RegistrationRepository.cs` — `AsNoTracking()` on `IsConfirmedAsync`.
- `_bmad-output/initiative-attendta/tickets/epic-events-registration/tickets/story-refactor-sweep-plan.md` — this plan.

**Review findings.** 2 reported: 1 patched (medium — the `IMemberEventService` summary misstated the cancellation precondition), 1 rejected (low — the Implementation Notes' eleven-file claim; its only remedy is editing this build's plan). 0 deferred.

**Follow-up review recommendation.** false — one patched entry at `medium`, below the two-medium threshold, and no patched `high`.

**Verification.**
- `dotnet build ASI.Basecode.sln --configuration Release -warnaserror` → 0 warnings, 0 errors (run before and after the review patch).
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --configuration Release --no-build` → 310 passed, 0 failed, 0 skipped.
- Focused integration run with `ATTENDTA_TEST_SQL` at LocalDB `master` over the plan's filter plus `EventCover` → 115 passed, 0 failed, 0 skipped.
- Matrix audit: all five matrix rows are covered by tests that ran and passed — missing/stale event by the lifecycle and draft-flow classes, review decisions by the review and promotion classes, refused event POSTs by the publication/cancellation/completion HTTP flows, and cover read/upload by the cover flow and cover service classes.
- `git diff HEAD --stat` → 12 files, 279 insertions, 244 deletions (11 code files plus this plan).

**Residual risks.** The whole integration suite was not run; 8.4 owns that combined run. An untracked `_bmad-output/initiative-attendta/tickets/epic-check-in/tickets/story-refactor-sweep-plan.md` (ticket `9`) was present during the run from another session and was left untouched and uncommitted.
