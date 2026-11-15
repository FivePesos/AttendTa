---
title: 'Membership request bulk approval'
type: 'feature'
ticket: '16'
created: '2026-11-15'
status: 'built'
baseline_revision: 'a8a4e30df88eed02609337e7a40dd678c3911bc8'
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
warnings: ['oversized']
deferred: []
---

<intent-contract>

## Intent

**Problem:** Management approves pending Account Requests one dialog per person and the queue has no selection surface. Phase 4 of the UI plan needs one shared selection/confirmation/result component that 3.20 and 6.14 reuse instead of each inventing one.

**Approach:** Build that surface once — eligible-row checkboxes, selected count, Select all on this page, Deselect all, a confirmation naming the selected people and the consequence, and a per-record result panel — then wire it to a bounded `ApproveManyAsync` that runs the existing `ApproveAsync` once per deduplicated id, so each record keeps its own transaction, member creation, hash move and audit.

## Boundaries & Constraints

**Always:** Actor only from `ICurrentActor` via `RequireManagementActor`; `Policies.Management` plus antiforgery on the POST. Bound the batch to 25 ids, deduplicate in first-seen order, and return one result row per selected id. Every id goes through `ApproveAsync` unchanged: own transaction, lock order `account_requests` → `users`, email and student-number uniqueness re-checked at execution time, one audit entry per success. A refusal must never roll back another record. Selection is per page and ephemeral: no `sessionStorage`, nothing carried across page, tab, search, sort or filter change, cleared on load and on bfcache restore. Decline stays individual. Reuse `task-dialog`, `button`, `badge`, `notice`, `data-dialog-*`, `_Pager`, `_ListToolbar`, `ListNavigation`; rebuild and commit `wwwroot/css/app.css` when styles change.

**Never:** No outer or nested transaction around the loop; no second approval path; no raw SQL status update; no schema change; no new controller. No client-supplied actor, role or outcome, and no client-supplied name trusted in the result panel. No bulk decline, no bulk issuance, no edits to the Registration Review or Certificate Ledger surfaces. No toastr and no new JS framework.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Mixed selection | 3 pending; one student number taken | 2 approved (account, member, audit each), 1 refused with the uniqueness message; pending page re-renders with the result panel | 200 with per-record results |
| Stale / already decided | id decided by someone else | Refused "This account request has already been decided."; no writes | 200 result row |
| Duplicate ids | `[7,7,8]` | 7 and 8 approved once each; two result rows | No error |
| Repeat submit | same ids posted twice | Second submit refuses each as stale; no duplicate account, member or audit | 200 result rows |
| Empty or over-bound | no ids, or 26 ids | Nothing decided; notice names the requirement or the 25 limit | 200 notice |
| Forbidden actor | anonymous or member POST | Policy redirect/403; service also refuses with `Forbidden` | Redirect or 403 |
| Missing antiforgery | POST without token | 400 | 400 |
| Filter change | rows selected, then tab/search/sort/page changes | Fresh page, nothing selected, bulk action disabled | No error |

</intent-contract>

## Code Map

- `ASI.Basecode.Resources/Constants/BulkApproval.cs` (new) — `MaxBatchSize = 25`, read by service and controller.
- `Data/Models/AccountRequestSummary.cs` (new) and `Data/Interfaces/IAccountRequestRepository.cs`, `Data/Repositories/AccountRequestRepository.cs` — read-only `GetSummariesAsync(ids, ct)` with `AsNoTracking`, styled after `GetPageByStatusAsync`; names are display only.
- `Services/ServiceModels/AccountRequestApprovalOutcome.cs` (new) and `Services/Interfaces/IAccountRequestService.cs`, `Services/Services/AccountRequestService.cs` — add `ApproveManyAsync`; leave `ApproveAsync` (L123–193) untouched. `UnitOfWork.ExecuteInTransactionAsync` (L48–86) rolls back and clears the whole outer transaction on any inner failure, so the loop must not open one.
- `WebApp/Models/BulkSelectionViewModel.cs`, `BulkResultViewModel.cs` (new) — one selection VM feeds bar and dialog; the result VM is surface-agnostic (`Title`, counts, `Items` of `(Label, Succeeded, Message)`).
- `WebApp/Views/Shared/_BulkSelectionBar.cshtml`, `_BulkConfirmDialog.cshtml`, `_BulkResultPanel.cshtml` (new) — bar with `role="group"`, native `<dialog class="task-dialog">` with `asp-all-route-data`, result panel with `role="status"`.
- `WebApp/wwwroot/js/features/bulk-selection.js` (new) — scoped to `[data-bulk-root]`; exposes `window.AttendTaBulkSelection` for the node test; opens the dialog itself because `site.js`'s delegated `[data-dialog-open]` handler would show it before the ids are written.
- `WebApp/Styles/features/bulk-actions.css` (new) and `Styles/app.css` import — `.bulk-bar`, `.bulk-count`, `.select-column`, `.bulk-result*`; plain rules, no `@apply`.
- `WebApp/Controllers/AccountRequestController.cs` — add `ApproveSelected(int[] ids, …)` mirroring `Approve`/`DecisionError` (L65–110): re-render pending `Index` with `ViewBag.BulkResult` or `ViewBag.BulkError` and `ViewData["PagerPath"]`.
- `WebApp/Views/AccountRequest/Index.cshtml` — selection column, bar, result panel, `ViewData["Feature"] = "bulk-selection"`, shared dialog; empty-state `colspan` 5 → 6.
- `tests/ASI.Basecode.Services.Tests/AccountRequestServiceTests.cs` — extend `Harness` fakes with `GetSummariesAsync`; assert one transaction per id via `FakeUnitOfWork.TransactionCount`.
- `tests/ASI.Basecode.IntegrationTests/AccountRequestDecisionTests.cs` — reuse `CreateService`, `SeedRequestAsync`, `SignInAsync`, `GetLayoutAntiforgeryTokenAsync`.
- `WebApp/tests/bulk-selection.test.js` (new) — `vm` + fake-DOM style of `dialog-guard.test.js`.

## Tasks & Acceptance

**Execution:**
- [x] `Resources/Constants/BulkApproval.cs` — add the 25-id limit — one number shared by service and controller.
- [x] `Data/Models/AccountRequestSummary.cs`, `Data/Interfaces/IAccountRequestRepository.cs`, `Data/Repositories/AccountRequestRepository.cs` — add the read-only summary lookup — the result panel names people server-side.
- [x] `Services/ServiceModels/AccountRequestApprovalOutcome.cs`, `Services/Interfaces/IAccountRequestService.cs`, `Services/Services/AccountRequestService.cs` — add `ApproveManyAsync`: validate the actor once, reject empty and over-bound lists, resolve names in one read, deduplicate, call `ApproveAsync` per id catching only `DomainException`.
- [x] `WebApp/Models/BulkSelectionViewModel.cs`, `BulkResultViewModel.cs` and `WebApp/Views/Shared/_BulkSelectionBar.cshtml`, `_BulkConfirmDialog.cshtml`, `_BulkResultPanel.cshtml` — add the reusable view models and components with `data-bulk-*` hooks and live-region semantics.
- [x] `WebApp/wwwroot/js/features/bulk-selection.js` and `WebApp/Styles/features/bulk-actions.css`, `Styles/app.css` — add selection behaviour, feature styles and the import; rebuild `wwwroot/css/app.css`.
- [x] `WebApp/Controllers/AccountRequestController.cs` and `WebApp/Views/AccountRequest/Index.cshtml` — add the antiforgery `ApproveSelected` POST and wire the column, bar, result panel and dialog into the queue; keep individual Approve/Decline intact.
- [x] `tests/ASI.Basecode.Services.Tests/AccountRequestServiceTests.cs` — cover mixed, stale, duplicate-id, empty, over-bound, forbidden and repeat-submit batches.
- [x] `tests/ASI.Basecode.IntegrationTests/AccountRequestDecisionTests.cs` and `WebApp/tests/bulk-selection.test.js` — prove partial success, exact audit counts, no duplicate on repeat submit, the browser POST with and without antiforgery, and the selection JS.

**Acceptance Criteria:**
- Given three pending requests where one student number is already taken, when Management approves all three from the queue, then the page shows two approved members and one refusal naming that person and the uniqueness reason, and exactly two accounts, two members and two audit rows exist.
- Given a request decided by someone else after the page rendered, when it is approved in a batch, then its result row says it was already decided and it gains no account, member or audit row.
- Given the same batch is submitted twice, when the second submit completes, then every row is refused as stale and no duplicate account, member or audit row exists.
- Given 26 ids, when submitted, then nothing is decided and the page states the 25-request limit.
- Given rows are selected and the user changes tab, search, sort or page, when the new page renders, then no row is selected and the bulk action is disabled.
- Given a member or anonymous caller posts the bulk endpoint, when the request is evaluated, then it is refused before any decision, and a POST without an antiforgery token returns 400.
- Given the queue page for Management, when it renders, then the bar, confirmation dialog and result panel come from the three shared partials with no account-request-specific markup inside them, and Decline is still a single-record action.

## Implementation Notes

Built the shared bulk surface as three partials plus one JS module and one CSS feature file, and wired it to `AccountRequestService.ApproveManyAsync`. `ApproveAsync` was not touched: the batch loop calls it once per deduplicated id, so each record keeps its own transaction, lock order, uniqueness re-check, member creation, hash move and audit. The service test asserting `FakeUnitOfWork.TransactionCount == 3` for a three-id batch is the proof that no outer transaction was introduced.

Two adjustments made after the implementing subagent returned: the outcome carries the server-resolved `StudentNumber` so the result panel distinguishes two applicants who share a name (the controller composes `Name (student number)`), and `BulkSelectionViewModel.SubmittingLabel` replaced a hard-coded "Approving…" so 3.20 and 6.14 can reuse the dialog unchanged.

Surprise: a leftover integration-test host from the subagent's run held the Release output DLLs open and failed the first solution build with MSB3027. Killing that process cleared it; the build then succeeded with 0 warnings.

## Plan Change Log

## Review Triage Log

### 2026-11-15 — Review pass
- verdicts: 4 findings — high 0, medium 2, low 1, false 1, maybe-false 0
- findings:
  - `[medium]` `[patch]` After a bulk POST the page is rendered at `/AccountRequest/ApproveSelected`, so a second bulk submit makes the filter tabs, sort link, search form and Reset point at the POST-only action (405 on GET) — verified in `list-controls.js:34-50` (writes `uiReturnUrl` from `window.location` on every POST) and `ListNavigation.cs:33-38` (takes the path from `uiReturnUrl`/`request.Path`). Fixed: `ViewData["ListPath"]` set in `ApproveSelected` and `DecisionError`, passed as `ListNavigation.Url`'s `targetPath` by the tabs, the sort link and `_ListToolbar`'s form action and Reset.
  - `[low]` `[patch]` On tabs with no selectable rows the bar's "Select all on this page" and "Deselect all" stayed enabled and inert — verified against `_BulkSelectionBar.cshtml` and `bulk-selection.js` `sync()`, which disabled only the bulk action and the header box. Fixed: `sync()` disables both page actions when the page has no selectable rows, covered by two node tests.
  - `[medium]` `[patch]` The row checkbox hit area was the 16px box inside a 44px cell, short of the 44px touch target `DESIGN.md` requires — verified in `bulk-actions.css` (`.select-column input { width:16px }`, cell padding left as the only surrounding space). Fixed: header and row checkboxes wrapped in a 44×44 `<label class="select-target">`, column padding zeroed, visual box kept at 16px.
  - `[false]` `[reject]` The result panel cannot distinguish two applicants who share a name — refuted: the reviewed diff already builds each row with `Label(outcome)` (`AccountRequestController`), which appends the server-resolved student number, and `AccountRequestApprovalOutcome.StudentNumber` is populated by `ApproveManyAsync`.

## Design Notes

Per-record transactions are the point: `ApproveAsync` already owns its transaction, so calling it in a loop yields partial success with one audit per committed record and no nesting hazard. A batch-level transaction would turn one bad row into a failed batch.

The result panel is fed by a server-side name lookup, not the posted form: a client can post any label, and an approved row has already left the pending page when the panel renders. Catch only `DomainException` per id; cancellation and unexpected faults must surface rather than be reported as per-row refusals.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — expected: 0 warnings, 0 errors.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` — expected: all pass; on `Zero tests ran` (exit 5) run `& "tests\ASI.Basecode.Services.Tests\bin\Release\net10.0\ASI.Basecode.Services.Tests.exe"`.
- With `$env:ATTENDTA_TEST_SQL` set to a local SQL Server `master` connection, `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` — expected: all pass, including the new bulk and browser cases.
- `npm --prefix ASI.Basecode.WebApp run css:build` then `git diff --exit-code -- ASI.Basecode.WebApp/wwwroot/css/app.css` — expected: committed CSS matches the build.
- `npm --prefix ASI.Basecode.WebApp run test:js` — expected: all node tests pass.

**Manual checks (if no CLI):**
- On the pending tab, selecting rows updates the count, the header checkbox shows the partial state, and Approve selected is disabled at zero.
- After a mixed batch the result panel names every selected person and the approved rows have left the pending table.

## Auto Run Result

**Summary:** Membership request bulk approval (ticket 1.16, issue #239). The shared bulk surface — selection bar, confirmation dialog, per-record result panel, one JS module, one CSS feature file — is built once for reuse by 3.20 and 6.14, and wired to a bounded `ApproveManyAsync` that runs the existing `ApproveAsync` once per deduplicated id inside its own transaction. Decline is unchanged and individual.

**Files changed:**
- `ASI.Basecode.Resources/Constants/BulkApproval.cs` — the 25-id bound and the two notices, shared by service and controller.
- `ASI.Basecode.Data/Models/AccountRequestSummary.cs` — display-only identity read model.
- `ASI.Basecode.Data/Interfaces/IAccountRequestRepository.cs`, `Repositories/AccountRequestRepository.cs` — read-only `GetSummariesAsync`.
- `ASI.Basecode.Services/ServiceModels/AccountRequestApprovalOutcome.cs` — one result row per decided id.
- `ASI.Basecode.Services/Interfaces/IAccountRequestService.cs`, `Services/AccountRequestService.cs` — `ApproveManyAsync`; `ApproveAsync` untouched.
- `ASI.Basecode.WebApp/Models/BulkSelectionViewModel.cs`, `BulkResultViewModel.cs` — reusable selection and result view models.
- `ASI.Basecode.WebApp/Views/Shared/_BulkSelectionBar.cshtml`, `_BulkConfirmDialog.cshtml`, `_BulkResultPanel.cshtml` — the three shared components.
- `ASI.Basecode.WebApp/wwwroot/js/features/bulk-selection.js` — selection state, count, tri-state header, dialog population, bfcache reset.
- `ASI.Basecode.WebApp/Styles/features/bulk-actions.css`, `Styles/app.css`, `wwwroot/css/app.css` — feature styles, import and rebuilt output.
- `ASI.Basecode.WebApp/Controllers/AccountRequestController.cs` — antiforgery `ApproveSelected` POST and the list-path fix.
- `ASI.Basecode.WebApp/Views/AccountRequest/Index.cshtml` — selection column, bar, result panel, dialog and list-path links.
- `ASI.Basecode.WebApp/Views/Shared/_ListToolbar.cshtml`, `Models/ListNavigation.cs` — optional `targetPath` so a POST re-render keeps GET links.
- `tests/ASI.Basecode.Services.Tests/AccountRequestServiceTests.cs` — 8 batch tests, one transaction per id.
- `tests/ASI.Basecode.IntegrationTests/AccountRequestDecisionTests.cs` — 5 endpoint and browser tests.
- `ASI.Basecode.WebApp/tests/bulk-selection.test.js` — 10 node tests.

**Review findings:** 4 reported — 2 medium patched, 1 low patched, 1 rejected as false. No deferred items. Patched: the POST-URL list links (medium), the inert page actions on tabs with no selectable rows (low), and the 16px checkbox hit area (medium). Rejected: the same-name result-panel claim, refuted because the reviewed diff already composes the label from the server-resolved name and student number.

**Follow-up review recommendation:** `true`. Two medium entries were patched on this first pass. Unverified risk: the list-path fix assumes every link on the re-rendered queue page flows through `ListNavigation.Url` or `ViewData["ListPath"]`; the pager uses `PagerPath` and the auto-fit reload is disabled in fixed paging mode, but no browser test performs two consecutive bulk submits and then follows a tab or sort link.

**Verification performed:**
- `dotnet build ASI.Basecode.sln --configuration Release` — 0 warnings, 0 errors.
- Services test project — 365 passed, 0 failed.
- Integration suite (`-parallelMode none`, LocalDB) — 524 passed, 0 failed, 0 skipped.
- `npm --prefix ASI.Basecode.WebApp run css:build` — deterministic; committed `wwwroot/css/app.css` matches the build.
- `npm --prefix ASI.Basecode.WebApp run test:js` — 34 passed.

**Residual risks:** the bulk POST re-renders in place rather than redirecting, so a browser refresh re-posts the batch (safe: every row returns stale and no duplicate is created, but the panel is replaced by refusals). The bar renders on every tab and is inert where no row is selectable. `AccountRequestApprovalOutcome` carries no outcome code, only the message, so a future surface that needs to branch on the refusal reason would have to add one.
