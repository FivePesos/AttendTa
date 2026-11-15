---
title: 'Registration review and dossier bulk approval'
type: 'feature'
ticket: '20'
created: '2026-11-16'
status: 'built'
baseline_revision: '0c758f72e45b7312b06e8ce871a4416b89a9887e'
route: 'full'
route_source: 'auto'
risk: 'high'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
followup_review_recommended: true
context:
  - 'AGENTS.md'
  - 'DESIGN.md'
  - 'docs/handoffs/ui-ux-backend-alignment-handoff.md'
  - 'docs/ui-ux-implementation-plan.md'
warnings: ['oversized']
deferred: []
---

<intent-contract>

## Intent

**Problem:** Management approves pending Registrations one dialog per person on the review queue and on the event dossier roster, so a full page of signups costs a page of dialogs. Phase 4 of the UI plan needs the same bounded batch approval the membership queue already has, without weakening any seat, cutoff, waitlist or audit rule.

**Approach:** Reuse the shared bulk surface ticket 1.16 already built (`_BulkSelectionBar`, `_BulkConfirmDialog`, `_BulkResultPanel`, `bulk-selection.js`, `bulk-actions.css`) on both registration surfaces, and wire it to a bounded `RegistrationReviewService.ApproveManyAsync` that runs the existing single-record approval path once per deduplicated id, so each record keeps its own transaction, locks, capacity and waitlist decisions and its own audit entry.

## Boundaries & Constraints

**Always:** Actor only from `ICurrentActor` through the existing `RequireManagement`; `Policies.Management` plus antiforgery on the POST. Bound the batch to `BulkApproval.MaxBatchSize`, deduplicate in first-seen order, and return exactly one result row per distinct id, naming the actual outcome — Confirmed, Waitlisted, or refused with its reason. Process ids in the review queue's own order (`SubmittedAt`, then `RegistrationId`) so capacity goes to the earliest signup. Every id goes through the existing approval path unchanged: discover identity, lock user then event, re-read and recheck status, actor, account/membership activity, event status, start cutoff, seat and eligible-waitlist priority, one audit entry per committed change. A refusal must never roll back another record. Selection is per page and ephemeral: no storage, cleared on load and bfcache restore, and every page, event, search or filter change is a full navigation that re-renders with nothing ticked. Individual Approve, Decline and Cancel stay available and unchanged.

**Never:** No outer or nested transaction around the loop; no second approval code path; no raw SQL status update; no schema change. No client-supplied actor, role, eligibility, capacity or outcome, and no client-supplied name trusted in the result panel. No bulk decline, no bulk cancellation, no bulk promotion. Do not change the single-record `ApproveAsync` signature or its guards, and do not touch the Account Request or Certificate Ledger surfaces beyond the shared components' backward-compatible extension.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Mixed selection, seats short | 3 pending, limit 1 | Earliest confirmed with a 64-hex QR; the other two waitlisted; 3 audit rows | 200 with per-record results |
| Stale / already decided | id decided by someone else | Refused with the existing stale message; no write, no audit | 200 result row |
| Duplicate ids | `[7,7,8]` | 7 and 8 decided once each; two result rows | No error |
| Wrong event | dossier batch carrying an id of another event | Refused naming the event mismatch; nothing changes | 200 result row |
| Repeat submit | same ids posted twice | Second submit refuses each as stale; no duplicate status change or audit | 200 result rows |
| Empty or over-bound | no ids, or 26 ids | Nothing decided; the notice names the requirement or the 25 limit | 200 notice |
| Past cutoff / unpublished | event started or not Published | Each id refused with the existing cutoff or event-status message | 200 result rows |
| Forbidden actor | anonymous or member POST | Policy redirect/403; the service also refuses with `Forbidden` | Redirect or 403 |
| Missing antiforgery | POST without a token | 400 | 400 |
| Page / filter change | rows ticked, then tab, page, event or search changes | Fresh page, nothing ticked, bulk action disabled | No error |

</intent-contract>

## Code Map

- `ASI.Basecode.Resources/Constants/BulkApproval.cs` — keep `MaxBatchSize = 25`; add registration-specific empty and over-bound notices beside the account-request ones so neither surface shows the other's wording.
- `ASI.Basecode.Data/Interfaces/IRegistrationRepository.cs` — add `GetSummariesAsync(IReadOnlyCollection<int> ids, ct)` and a `RegistrationSummary` record beside `RegistrationReviewRow` (L120): `RegistrationId, EventId, EventTitle, MemberName, StudentNumber, SubmittedAt, Status`. No status filter, so an already-decided id still resolves its name.
- `ASI.Basecode.Data/Repositories/RegistrationRepository.cs` — implement it `AsNoTracking` by reshaping `ReviewQueue`'s joins (L249–270) without the status predicate; read only.
- `ASI.Basecode.Services/Services/RegistrationReviewService.cs` — `ExecuteAsync` (L81) returns the resulting `RegistrationStatusCode`; `ApproveAsync` (L41) awaits and discards it so its signature and guards are untouched. Add `ApproveManyAsync(ids, eventId, ct)` that validates the actor once, rejects empty and over-bound lists, deduplicates, resolves identities in one read, orders by `SubmittedAt` then id, and calls the approval path per id catching only `DomainException`. The event scope check (`eventId.HasValue && identity.EventId != eventId`) refuses before the transaction opens.
- `ASI.Basecode.Services/ServiceModels/RegistrationApprovalOutcome.cs` (new) — id, server-resolved name/student number/event title, a `Confirmed | Waitlisted | Refused` result and the message.
- `ASI.Basecode.Services/Interfaces/IRegistrationReviewService.cs` — declare `ApproveManyAsync`.
- `ASI.Basecode.WebApp/Models/BulkResultViewModel.cs` — add optional `Status` and `Tone` to `BulkResultItem` so the panel can say Confirmed and Waitlisted distinctly; positional defaults keep the account-request call site compiling.
- `ASI.Basecode.WebApp/Views/Shared/_BulkResultPanel.cshtml` — render `Status`/`Tone` when present, else the existing success/failure label and badge.
- `ASI.Basecode.WebApp/Models/RegistrationBulkResult.cs` (new) — one mapping from outcomes to `BulkResultViewModel` (`badge-approved` / `badge-pending` / `badge-rejected`), used by both controllers.
- `ASI.Basecode.WebApp/Controllers/RegistrationReviewController.cs` — add the antiforgery `ApproveSelected(int[] ids, string status, int? eventId, int? page)` mirroring `AccountRequestController.ApproveSelected` (L95–140): re-render `Index` with `ViewBag.BulkResult`/`ViewBag.BulkError`, `ViewData["ListPath"]`, `ViewData["PagerPath"]`.
- `ASI.Basecode.WebApp/Controllers/EventController.cs` — extract `Details`' body (L65–82) into a private `DossierAsync` and add `ApproveSelected(int[] ids, int eventId, string status, int? page)` that re-renders `Details`; the class-level `[AutoValidateAntiforgeryToken]` (L26) already covers it.
- `ASI.Basecode.WebApp/Views/RegistrationReview/Index.cshtml` — `ViewData["Feature"] = "bulk-selection"`, `[data-bulk-root]`, select column only when a pending row is selectable, bar, dialog, result panel; empty-state `colspan` 4 → 5.
- `ASI.Basecode.WebApp/Views/Event/Details.cshtml` — same on the pending roster tab, gated on `Model.CanReview && Model.Status == "pending"`; add a `Scripts` section for `bulk-selection.js` because `ViewData["Feature"]` already carries `record-dialog`; empty-state `colspan` 3 → 4.
- `ASI.Basecode.WebApp/Styles/features/bulk-actions.css` — reuse as is; only extend if the dossier's `table-sticky` header breaks the 44px target, then rebuild `wwwroot/css/app.css`.
- Adding a member to `IRegistrationRepository` breaks six fakes, all mechanical: `Services.Tests/RegistrationReviewTests.cs` L306, `MemberEventServiceTests.cs` L962, `EventLifecycleServiceTests.cs` L1208, `EventCoverServiceTests.cs` L422, `IntegrationTests/RegistrationCancellationTests.cs` L355 and `MemberEventFlowTests.cs` L888. The two integration wrappers must delegate.
- `tests/ASI.Basecode.Services.Tests/RegistrationReviewTests.cs` — the single-registration `ReviewHarness` (L212) cannot express a batch; add a `BatchHarness` with N registrations, a counting `IUnitOfWork`, and `GetSummariesAsync` on `FakeRegistrations` (L306).
- `tests/ASI.Basecode.IntegrationTests/RegistrationReviewTests.cs` — reuse `SetupAsync`, `AddRegistrationAsync`, `Service` (L485–503) and the browser helpers in `BrowserQueuePaginatesAndFiltersAndEnforcesPolicyAndAntiforgery` (L410).

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Resources/Constants/BulkApproval.cs` — add the two registration notices — so the registration surfaces never show account-request wording.
- [x] `ASI.Basecode.Data/Interfaces/IRegistrationRepository.cs` and `ASI.Basecode.Data/Repositories/RegistrationRepository.cs` — add the read-only summary lookup — the result panel names people server-side after the row has left the pending page.
- [x] the six `IRegistrationRepository` fakes listed in the Code Map — implement `GetSummariesAsync` (`NotSupportedException` in the four service fakes, delegate in the two integration wrappers) — the interface change must not break the existing suites.
- [x] `ASI.Basecode.Services/ServiceModels/RegistrationApprovalOutcome.cs`, `ASI.Basecode.Services/Interfaces/IRegistrationReviewService.cs`, `ASI.Basecode.Services/Services/RegistrationReviewService.cs` — add `ApproveManyAsync` over the existing approval path — partial success with one transaction and one audit per committed record.
- [x] `ASI.Basecode.WebApp/Models/BulkResultViewModel.cs`, `Views/Shared/_BulkResultPanel.cshtml`, `ASI.Basecode.WebApp/Models/RegistrationBulkResult.cs` — extend the shared panel with an optional outcome label and map registration outcomes once — Confirmed and Waitlisted must not read as the same word.
- [x] `ASI.Basecode.WebApp/Controllers/RegistrationReviewController.cs` and `ASI.Basecode.WebApp/Views/RegistrationReview/Index.cshtml` — add the antiforgery bulk POST and the selection column, bar, dialog and result panel — the review queue gains the batch without losing its individual actions.
- [x] `ASI.Basecode.WebApp/Controllers/EventController.cs` and `ASI.Basecode.WebApp/Views/Event/Details.cshtml` — add the dossier bulk POST over the extracted `DossierAsync` and the same surface on the pending roster tab — the second surface reuses the same use case.
- [x] `tests/ASI.Basecode.Services.Tests/RegistrationReviewTests.cs` — cover mixed, stale, duplicate-id, wrong-event, empty, over-bound, forbidden and repeat-submit batches, and assert one transaction per id.
- [x] `tests/ASI.Basecode.IntegrationTests/RegistrationReviewTests.cs` — prove partial success with exact audit counts, no duplicate effect on repeat submit, the wrong-event refusal, and the browser POST on both surfaces with and without antiforgery.

**Acceptance Criteria:**
- Given three pending registrations on a published event with one free seat, when Management approves all three from the review queue, then the result panel names each participant as Confirmed, Waitlisted or refused, exactly one registration is Confirmed with a 64-hex QR token, and exactly three approval audit rows exist.
- Given a registration decided by someone else after the page rendered, when it is approved in a batch, then its result row states the status changed and that registration gains no status change and no audit row.
- Given the dossier posts a batch containing a registration of a different event, when it is submitted, then that id is refused naming the mismatch and its registration is unchanged.
- Given the same batch is submitted twice, when the second submit completes, then every row is refused as stale and no registration gains a second status change or audit row.
- Given 26 ids or no ids, when submitted, then nothing is decided and the page states the 25-registration limit or that a selection is required.
- Given rows are ticked and the user changes tab, page, event filter or search, when the new page renders, then no row is ticked and the bulk action is disabled.
- Given a member or anonymous caller posts either bulk endpoint, when the request is evaluated, then it is refused before any decision, and a POST without an antiforgery token returns 400.
- Given both surfaces, when they render, then each shows the shared bar, confirmation dialog and result panel with no surface-specific markup inside them, and individual Approve, Decline and Cancel still work.

## Implementation Notes

- `RegistrationReviewService.ExecuteAsync` now returns the resulting `RegistrationStatusCode`. `ApproveAsync` awaits and discards it, so the single-record signature and every guard are unchanged; `ApproveManyAsync` uses it to distinguish Confirmed from Waitlisted without re-reading the row.
- `ApproveManyAsync` validates the actor once, refuses empty and over-bound lists with the registration-specific notices, deduplicates in first-seen order, resolves display identity in one `GetSummariesAsync` read, orders by `SubmittedAt` then `RegistrationId`, and calls `ExecuteAsync` per id catching only `DomainException`. No outer transaction; the approval path keeps its own.
- The dossier's event scope is checked against the resolved summary before the transaction opens, so a foreign id costs no lock and no write. An unknown id falls through to the approval path's own `NotFound` refusal.
- `RegistrationRepository.GetSummariesAsync` is the review queue's join without the status predicate, `AsNoTracking`, read only.
- The shared result panel gained optional `Status`/`Tone` on `BulkResultItem` (positional defaults), so the account-request call site is untouched. `RegistrationBulkResult` is the single mapping both controllers use: `badge-approved` for Confirmed, `badge-pending` for Waitlisted, `badge-rejected` for refused.
- The dossier POST re-renders `Details` (200 with per-record results) instead of redirecting, so the result panel needs no TempData round trip. `DossierAsync` now sets `PagerPath`/`ListPath`, and the roster tab links pass `listPath`, so a POST re-render keeps every link on the GET dossier route.
- The dossier keeps `ViewData["Feature"] = "record-dialog"` and loads `bulk-selection.js` through a `Scripts` section; the review queue owns the feature slot.
- The service-test `FakeRegistrations` implements `GetSummariesAsync` functionally because the new `BatchHarness` needs it; the other three service fakes throw `NotSupportedException` and the two integration wrappers delegate.

### Review fixes (2026-11-16)

- `DossierAsync` sets `ViewData["PagerPath"]`/`["ListPath"]` to `Url.Action(nameof(Details))` with no route values. `_Pager` and `ListNavigation.Url` append their own query string, so a path that already carried `?id=…` produced `?id=3?status=…`, bound `id` as `"3?status=…"`, fell back to 0 and 404'd every dossier page. `ApproveSelected`'s event parameter is renamed `id` so the POST query carries the dossier's own route key and both helpers copy it forward.
- The dossier bulk dialog's route values now carry `search` and `pageSize` from the request, so a batch taken from a searched roster re-renders the same roster. The review queue already carried `search`.
- `RegistrationReviewController.ApproveSelected` normalizes the posted status to the queue's four keys before it reaches `ViewBag.Status`, the page read and the re-render, so `status=foo` renders pending rows with the pending key and cannot re-post itself.
- The integration tests assert every dossier link is one bare path plus at most one query string, that a searched dossier's dialog keeps `search`, and that an unknown status falls back to `pending`.

### Design gate (2026-11-16)

`/impeccable critique` ran degraded (no sub-agent tool in this session) with the bundled detector returning no findings, then `/impeccable polish` cleared its priority issues:

- The confirmation dialog's selected-record list was labelled "Selected requests" on a registration surface. `BulkSelectionViewModel` gained `SelectionListLabel` and `RecordNoun` (defaults keep the membership wording), and both registration surfaces pass "Selected registrations"/"registration".
- The batch result now leads the list block on both surfaces, matching Membership Account Requests, instead of sitting below the toolbar.
- The review queue's consequence says "its event's live capacity" (the queue spans events); the dossier keeps "this event's".
- The dossier's bulk wrapper indentation matches its siblings.

No style change was needed, so `wwwroot/css/app.css` was not rebuilt. The critique snapshot is `.impeccable/critique/2026-11-15T18-03-35Z__app-views-registrationreview-index-cshtml-5ae7faab.md`; the polish edits changed the target bytes, which closes that backlog by the tool's own rule.

## Plan Change Log

- 2026-11-16 — Dossier bulk POST re-renders `Details` with `ViewBag.BulkResult`/`ViewBag.BulkError` rather than redirecting through TempData. The cookie TempData provider round-trips complex values as `JsonElement`, so a structured result panel cannot survive a redirect; re-rendering also matches the review queue and the plan's "re-renders `Details`" wording.
- 2026-11-16 — `DossierAsync` sets `PagerPath`/`ListPath` and the roster tab links pass `listPath`. Not in the Code Map, but without it a POST re-render would emit pager and tab links pointing at `/Event/ApproveSelected`.
- 2026-11-16 — The integration surface test asserts the visible bulk controls (`data-bulk-open`, `bulk-approve-dialog`, `data-bulk-row`, `select-column`) are absent on a decided tab rather than `data-bulk-root`. The root wrapper is inert without selectable rows, and this matches the account-request precedent.
- 2026-11-16 — Review fix: `PagerPath`/`ListPath` are bare paths and the dossier POST parameter is `id`, not `eventId`. The earlier `Url.Action(nameof(Details), new { id })` produced a path with a query string that the pager and tab helper then appended to again.
- 2026-11-16 — Polish: `BulkSelectionViewModel.SelectionListLabel`/`RecordNoun` are new optional members with membership-compatible defaults, so the shared dialog can speak each surface's noun without a second partial.

## Review Triage Log

### 2026-11-16 — Review pass
- verdicts: 4 findings — high 1, medium 2, low 1, false 0, maybe-false 0
- findings:
  - `[high]` `[patch]` The dossier's status tabs, pager and Reset links 404 on every load. `DossierAsync` set `PagerPath`/`ListPath` to `Url.Action(nameof(Details), new { id })`, which resolves to `/Event/Details?id=3` because the literal route outranks `Event/{id:int}` (proved by `RegistrationPromotionTests.cs:360` asserting that exact shape from the same link generator), and both `_Pager.cshtml:27` and `ListNavigation.cs:39` append `query.ToQueryString()` with its leading `?`, producing `/Event/Details?id=3?status=…` and `/Event/Details?id=3?id=3&status=…`; `id` bound as `"3?…"`, fell back to 0 and `GetEventAsync(0)` returned null. Fixed: both values are now the bare `Url.Action(nameof(Details))`, `ApproveSelected`'s parameter is renamed `eventId` → `id` so the posted query carries `id=`, and new integration assertions require every dossier link to be one bare path plus at most one `?`.
  - `[medium]` `[patch]` A bulk approve taken from a searched dossier re-rendered the unfiltered roster — the dialog's `RouteValues` carried only `eventId`, `status` and `page`, while `ListReadOptionsFactory` derives `Search` from the request, so the reader was shown rows they never selected. Fixed: the dossier builds a `bulkRoute` dictionary carrying `id`, `status`, `page` plus `search` and `pageSize` when present, the same shape the review queue uses.
  - `[low]` `[patch]` `ApproveSelected` passed the raw posted `status` into `ViewBag.Status` and `GetPageAsync`, so a hand-edited `status=foo` rendered pending rows with no tab marked `aria-current`, suppressed the waitlist Promote block and re-posted itself, because `RegistrationReviewService.ParseStatus` silently maps unknown values to Pending. Fixed: a `NormalizeStatus` matching `AccountRequestController`'s four keys now drives the page read and the re-render.
  - `[medium]` `[patch]` The required per-ticket `/impeccable` critique and polish gate had not run, and ticket 3.20 is in neither the demo nor the integration phase of `delivery-queue.md`, so the AGENTS.md exception did not apply. Fixed: `/impeccable critique` scored 33/40 with the bundled detector returning 0 findings, and `/impeccable polish` added `SelectionListLabel`/`RecordNoun` to `BulkSelectionViewModel` (the dialog's list was labelled with the membership surface's noun), moved the result panel above the list block on both surfaces, made the queue's consequence sentence read "its event's" for a cross-event batch, and fixed the dossier wrapper indentation. No style changed, so `app.css` was not rebuilt.

## Design Notes

Per-record transactions are the point, not an optimisation: the approval path already owns its transaction, so a loop yields partial success with one audit per committed record. `UnitOfWork.ExecuteInTransactionAsync` keeps a `depth` counter and clears the whole change tracker on any inner failure, so wrapping the loop would turn one refused id into a failed batch.

Ordering is a business rule: capacity and waitlist priority are decided per record against live counts, so the batch walks ids in the queue's own order (`SubmittedAt`, then `RegistrationId`) rather than the order the browser posted them, or a later signup could take the last seat from an earlier one.

The result panel is fed by a server-side summary read taken before any decision, because an approved row has already left the pending page and a client can post any label it likes.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — expected: 0 warnings, 0 errors.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` — expected: all pass; on `Zero tests ran` (exit 5) run `& "tests\ASI.Basecode.Services.Tests\bin\Release\net10.0\ASI.Basecode.Services.Tests.exe"`.
- With `$env:ATTENDTA_TEST_SQL` set to a local SQL Server `master` connection, `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` — expected: all pass, including the new bulk and browser cases.
- `npm --prefix ASI.Basecode.WebApp run css:build` then `git diff --exit-code -- ASI.Basecode.WebApp/wwwroot/css/app.css` — expected: committed CSS matches the build.
- `npm --prefix ASI.Basecode.WebApp run test:js` — expected: all node tests pass.

**Manual checks (if no CLI):**
- On the pending review tab, ticking rows updates the count, the header box shows the partial state, and Approve selected is disabled at zero.
- After a mixed batch the panel names every selected participant with their own outcome, and the confirmed rows have left the pending table.
- The same bar and panel appear on the dossier's Pending Review tab only, and not on Confirmed, Waitlisted, Cancelled or a started event.

**Results (2026-11-16):**
- `dotnet build ASI.Basecode.sln --configuration Release` — succeeded, 0 warnings, 0 errors.
- Service tests — 376 passed, 0 failed. Includes 10 new batch cases (mixed/capacity, stale, duplicate ids, wrong event, repeat submit, empty, over-bound, forbidden, start cutoff, unpublished event) with a counting unit of work asserting one transaction per id.
- Integration tests — 530 passed, 0 failed, 0 skipped with `ATTENDTA_TEST_SQL` on LocalDB master. New cases cover exact audit counts for a mixed batch, the stale refusal, repeat submit with no duplicate rows, the dossier wrong-event refusal, both endpoints with/without antiforgery and for anonymous/member/officer, where the bulk surface renders, the dossier's link shape, a searched dossier keeping its search, and `status=foo` falling back to pending.
- `npm --prefix ASI.Basecode.WebApp run css:build` — no change to `wwwroot/css/app.css`; no CSS extension was needed.
- `npm --prefix ASI.Basecode.WebApp run test:js` — 35 passed, 0 failed.
- The build and both suites were re-run in an isolated worktree at the commit, because the shared working tree also held another session's in-flight issue-241 certificate work that did not compile.

## Auto Run Result

**Summary:** Registration review and dossier bulk approval (ticket 3.20, issue #240). Both registration surfaces now carry the shared bulk selection bar, confirmation dialog and per-record result panel built for ticket 1.16, wired to a bounded `RegistrationReviewService.ApproveManyAsync` that runs the existing single-record approval path once per deduplicated id in the review queue's own order, so every record keeps its own transaction, locks, capacity and waitlist decision and its own audit entry. Confirmed and Waitlisted are reported as distinct outcomes; a refused id changes nothing.

**Files changed:**
- `ASI.Basecode.Resources/Constants/BulkApproval.cs` — the two registration notices, beside the account-request wording.
- `ASI.Basecode.Data/Interfaces/IRegistrationRepository.cs`, `Repositories/RegistrationRepository.cs` — read-only `GetSummariesAsync` and the `RegistrationSummary` record, the review queue's joins without the status predicate.
- `ASI.Basecode.Services/ServiceModels/RegistrationApprovalOutcome.cs` — one result row per distinct id with a `Confirmed | Waitlisted | Refused` result.
- `ASI.Basecode.Services/Interfaces/IRegistrationReviewService.cs`, `Services/RegistrationReviewService.cs` — `ApproveManyAsync`; `ExecuteAsync` now returns the resulting status while `ApproveAsync`'s signature and guards are untouched.
- `ASI.Basecode.WebApp/Models/BulkResultViewModel.cs`, `RegistrationBulkResult.cs`, `Views/Shared/_BulkResultPanel.cshtml` — optional outcome label and tint on the shared panel, and the one registration outcome mapping.
- `ASI.Basecode.WebApp/Models/BulkSelectionViewModel.cs`, `Views/Shared/_BulkConfirmDialog.cshtml` — `SelectionListLabel` and `RecordNoun` so each surface names its own records, with membership-compatible defaults.
- `ASI.Basecode.WebApp/Controllers/RegistrationReviewController.cs`, `Views/RegistrationReview/Index.cshtml` — antiforgery bulk POST, status normaliser, selection column, bar, dialog and result panel.
- `ASI.Basecode.WebApp/Controllers/EventController.cs`, `Views/Event/Details.cshtml` — `DossierAsync` extracted, dossier bulk POST, the same surface on the pending roster tab, and the link plumbing that keeps the tabs and pager on the GET route.
- `tests/ASI.Basecode.Services.Tests/RegistrationReviewTests.cs` — `BatchHarness` with a counting unit of work and 10 batch cases.
- `tests/ASI.Basecode.IntegrationTests/RegistrationReviewTests.cs` — 6 endpoint and browser cases.
- six `IRegistrationRepository` fakes — the new member, four throwing and two delegating.

**Review findings:** 4 reported — 1 high, 2 medium, 1 low, all patched; 0 deferred, 0 rejected. The high was a regression that broke every dossier load's tabs, pager and Reset (a `Url.Action` result carrying a query string, appended to by both link builders). The mediums were a searched dossier losing its search after a batch and the missing `/impeccable` critique and polish gate (33/40, detector 0 findings). The low was an unnormalised posted `status`.

**Follow-up review recommendation:** `true` — this pass patched a `high` entry. The unverified risk is the patched dossier link plumbing: the fix changed `ApproveSelected`'s parameter name and both `ViewData` values, and the new assertions cover the rendered link shape but not a real browser following those links through a second page of results.

**Verification performed:**
- `dotnet build ASI.Basecode.sln --configuration Release` — 0 warnings, 0 errors.
- Services test project — 376 passed, 0 failed.
- Integration suite (`-parallelMode none`, LocalDB) — 530 passed, 0 failed, 0 skipped.
- `npm --prefix ASI.Basecode.WebApp run css:build` — deterministic; `git diff --exit-code` on `wwwroot/css/app.css` clean.
- `npm --prefix ASI.Basecode.WebApp run test:js` — 35 passed.

**Residual risks:** the dossier bulk POST re-renders in place rather than redirecting, so a browser refresh re-posts the batch (safe — every row returns stale and no duplicate is created — but the panel is replaced by refusals and a GET of `/Event/ApproveSelected` is a 405). On the review queue a started or unpublished event still shows the bulk surface and refuses each row, while the dossier hides it; both match the plan's matrix, but the two surfaces differ. `docs/ui-ux-implementation-plan.md` still lists Phase 4 as not started, because certificate-eligibility bulk approval is outside this ticket. The shared working tree also held another session's in-flight issue-241 certificate work, which does not compile; this commit stages only this ticket's files, so that work stays uncommitted and the two branches will need reconciling before either merges.

**Results after review fixes and design gate (2026-11-16):**
- `dotnet build ASI.Basecode.sln --configuration Release` — succeeded, 0 warnings, 0 errors.
- Focused integration runs (the files this round edited): `RegistrationReviewTests` 51 passed, 0 failed; `AccountRequestDecisionTests` 21 passed, 0 failed, which guards the shared dialog's unchanged membership wording.
- `/impeccable critique` ran degraded (no sub-agent tool exposed) with `impeccable detect --json` returning no findings; `/impeccable polish` cleared all five priority issues. No browser was available, so no rendered inspection was performed.
- `wwwroot/css/app.css` was not rebuilt: the polish changed markup and copy only, no style.
