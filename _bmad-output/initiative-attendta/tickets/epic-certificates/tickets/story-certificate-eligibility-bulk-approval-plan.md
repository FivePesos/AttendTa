---
title: 'Certificate eligibility bulk approval'
type: 'feature'
ticket: '14'
created: '2026-11-16'
status: 'built'
baseline_revision: '0c758f72e45b7312b06e8ce871a4416b89a9887e'
route: 'full'
route_source: 'auto'
risk: 'high'
review: 'quick'
review_source: 'pinned'
lenses_ran: [quick]
review_loop_iteration: 0
followup_review_recommended: false
context: []
warnings: [oversized]
deferred:
  - summary: >-
      The AGENTS.md UI review gate (impeccable critique and polish) was not run for this
      UI-facing change.
    evidence: |-
      AGENTS.md requires /impeccable critique and /impeccable polish for any ticket that touches
      Razor or CSS, and ticket 6.14 is phase "ui" so the demo/integration exemption does not apply.
      The critique command is an interactive workflow that ends by asking the user what to improve,
      which an unattended build cannot complete. The surface adds no new visual language: the
      selection bar, confirmation dialog, result panel, select column and bulk-actions CSS are the
      1.16 components already critiqued and polished. Settle it by running
      `/impeccable critique` on ASI.Basecode.WebApp/Views/CertificateLedger/Index.cshtml and
      applying `/impeccable polish` before merge.
    location: >-
      ASI.Basecode.WebApp/Views/CertificateLedger/Index.cshtml
    severity: medium (unverified)
---

<intent-contract>

## Intent

**Problem:** An officer approves certificate eligibility one row at a time, so clearing a completed event's pending reviews costs one dialog per member. 6.14 adds bounded bulk approval of pending Certificate Reviews on the eligibility screen; declines and PDF issuance stay individual.

**Approach:** Add `ApproveManyAsync` to `ICertificateLedgerService`, which runs the existing `DecideAsync(id, approve: true, reason: null)` once per deduplicated id in its own transaction and returns one per-record outcome. Wire it to the shared bulk selection surface 1.16 already built for account requests.

## Boundaries & Constraints

**Always:** Approval marks eligibility only and renders no PDF. Each record's review status, Valid Attendance, confirmed seat and completed event are re-read inside its own transaction under the existing `users`-then-`event` lock order, so a repeat submit or concurrent invalidation is refused, not duplicated. Exactly one `certificate.approved` audit entry per successful change. Every selected id produces exactly one result row carrying the server-resolved member label. The batch is bounded to `BulkApproval.MaxBatchSize` (25) and deduplicated in first-seen order. Selection is per page and ephemeral.

**Never:** No bulk decline, no bulk issuance, no PDF render or file I/O on this path. No change to `DecideAsync`, `IssueAsync` or the individual row actions. No client-supplied label trusted. No outer transaction wrapping the batch.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| MIXED_SELECTION | 3 pending + 1 declined id | 3 approved; declined reported "This review has already been decided." | Partial success, no rollback of approved rows |
| EMPTY_SELECTION | no ids | No decision, ledger re-renders | Notice "Select at least one certificate review to approve." |
| OVER_BOUND | 26 ids | No decision, ledger re-renders | Notice naming the 25-record bound |
| REPEAT_SUBMIT | same pending id twice | One decision, one audit entry, one result row | Second occurrence deduplicated, not refused |
| ISSUED_OR_DECIDED | review `Issued` or `NotEligible` | Refused with its reason | Result row marked not approved |
| ATTENDANCE_INVALIDATED | invalidated before commit | Refused with the attendance message | Result row marked not approved |

</intent-contract>

## Code Map

- `Services/Services/CertificateLedgerService.cs` -- `DecideAsync` 164-182 and 299-372 own the transaction, lock order and single audit entry; `ApproveManyAsync` sits beside it. `RequireManagement()` 582 is the actor gate.
- `Services/Interfaces/ICertificateLedgerService.cs` -- add the batch method.
- `Services/ServiceModels/AccountRequestApprovalOutcome.cs` -- the outcome shape to mirror.
- `Data/Interfaces/ICertificateRepository.cs` -- add the summary read beside `GetReviewQueueAsync` 43.
- `Data/Repositories/CertificateRepository.cs` -- 46-74 already joins `registrations` → `members` → `users` for `user.Name` and `member.StudentNumber`; copy that projection.
- `Data/Models/AccountRequestSummary.cs` -- the display-only model to mirror.
- `Resources/Constants/BulkApproval.cs` -- `MaxBatchSize` 10 plus account-request-only messages; add noun-based helpers, do not reuse the request wording.
- `WebApp/Controllers/CertificateLedgerController.cs` -- add `ApproveSelected` beside `Approve` 83-91; reuse the `Command` redirect shape 98-110.
- `WebApp/Models/BulkSelectionViewModel.cs`, `BulkResultViewModel.cs` -- reuse unchanged.
- `WebApp/Views/CertificateLedger/Index.cshtml` -- eligibility table 50-116, dialogs 123-155. `ViewData["Feature"]` is single-valued (`certificate-ledger`), so load the bulk script from `@section Scripts`.
- `WebApp/Views/Shared/_BulkSelectionBar.cshtml`, `_BulkConfirmDialog.cshtml`, `_BulkResultPanel.cshtml` -- shared partials, reuse unchanged.
- `WebApp/Views/AccountRequest/Index.cshtml` 26-54, 108-208 -- the reference wiring for route values, `data-bulk-root`, checkbox column and result panel.
- `WebApp/wwwroot/js/features/bulk-selection.js` -- reuse unchanged; hooks are `[data-bulk-root]`, `[data-bulk-row]`, `[data-bulk-label]`, `[data-bulk-dialog-ids]`.
- `Data/Queries/ListReadOptions.cs` -- scoped from the request query string, so POST route values restore search/filter/page on re-render.
- `tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs` -- `SeedAsync()`/`Service(...)` harness to extend.

## Tasks & Acceptance

**Execution:**
- [ ] `Resources/Constants/BulkApproval.cs` -- add `EmptySelectionMessageFor(string noun)` and `OverBoundMessageFor(string noun)` built from `MaxBatchSize`; leave existing members untouched -- one bound, one wording source per surface.
- [ ] `Data/Models/CertificateReviewSummary.cs` -- new display-only model (`RegistrationId`, `MemberName`, `StudentNumber`) -- the panel names people from the server, never from the posted form.
- [ ] `Data/Interfaces/ICertificateRepository.cs` + `Data/Repositories/CertificateRepository.cs` -- add `GetDecisionSummariesAsync(IReadOnlyCollection<int> registrationIds, ct)` projecting the member name and student number -- one read resolves every label before any decision.
- [ ] `Services/ServiceModels/CertificateApprovalOutcome.cs` -- new per-record outcome (`RegistrationId`, `MemberName`, `StudentNumber`, `Succeeded`, `Message`).
- [ ] `Services/Interfaces/ICertificateLedgerService.cs` -- add `Task<IReadOnlyList<CertificateApprovalOutcome>> ApproveManyAsync(IReadOnlyCollection<int> registrationIds, CancellationToken ct = default)`.
- [ ] `Services/Services/CertificateLedgerService.cs` -- implement it: `RequireManagement()` once, refuse empty and over-bound with the noun helpers, dedupe first-seen, read summaries once, loop `DecideAsync(id, true, null, ct)` catching only `DomainException` per record -- each record keeps its own transaction, lock order and audit entry; cancellation and unexpected faults surface.
- [ ] `WebApp/Controllers/CertificateLedgerController.cs` -- add `[HttpPost, ValidateAntiForgeryToken] [Authorize(Policy = Policies.Management)] ApproveSelected(int[] ids, int? eventId, string tab, string search, string filter, int? page)`; refuse empty/over-bound before the service, map outcomes to `BulkResultViewModel("Approval results", …, "Approved", "Not approved")`, then re-render `Index` from `GetPageAsync` -- a batch returns the reader to their page with a per-record report.
- [ ] `WebApp/Views/CertificateLedger/Index.cshtml` -- eligibility tab only: wrap the table in `data-bulk-root data-bulk-noun="review"`; add a `select-column` checkbox column (`data-bulk-all` in the head; `data-bulk-row` and `data-bulk-label="@row.MemberName (@row.StudentNumber)"` on pending rows only); render `_BulkSelectionBar` when a pending row exists, `_BulkConfirmDialog` with route values `eventId`/`tab`/`search`/`filter`/`page` and the consequence "Each selected review is approved on its own. Issuing the PDF stays a separate, individual step.", `_BulkResultPanel` when `ViewBag.BulkResult` is set, and an error notice when `ViewBag.BulkError` is set; leave the individual Approve/Decline/Issue buttons untouched; load `~/js/features/bulk-selection.js` from `@section Scripts`.
- [ ] `tests/ASI.Basecode.Services.Tests/CertificateLedgerBulkApprovalTests.cs` -- new tests for the empty, over-bound and deduplicated-first-seen rules with a fake `ICurrentActor` (officer) and `TimeProvider` -- the request-shape guards are provable without SQL.
- [ ] `tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs` -- extend with mixed-selection, already-decided, repeat-submit and no-PDF cases -- the acceptance matrix is only honest against SQL.

**Acceptance Criteria:**
- Given a completed event with three pending reviews and one declined review, when an officer approves all four, then the three pending become Eligible with reviewer and time set, the declined row is unchanged, and the panel shows three approved and one "This review has already been decided." refusal.
- Given a bulk approval succeeds, when certificates and activity log are read, then no row is `Issued`, no `certificates/` file exists, and each approved review has exactly one `certificate.approved` entry.
- Given a review is already `Issued` or `NotEligible`, when it is in a bulk approval, then it is refused with its reason and its status, reviewer and audit history are unchanged.
- Given the same pending id is submitted twice in one batch, when the batch runs, then the review is decided once with exactly one result row and one audit entry.
- Given no ids or more than 25 ids are posted, when the form submits, then no decision is made and the ledger re-renders with the matching notice.
- Given the reader changes page, event, search or filter, when the new page loads, then no row stays selected and the bar reads "0 selected".
- Given a pending row, when the officer uses its own Approve, Decline or Issue action, then each behaves exactly as before.

## Implementation Notes

- `ApproveManyAsync` loops the existing `DecideAsync` instead of extracting a shared inner method: each record keeps its own transaction, `users`-then-`event` lock order and single audit entry, so one refusal cannot roll back the rest of the batch.
- Shared-file dependency declared: `Views/Shared/_EventSelector.cshtml` now falls back to `ViewData["ListPath"]` when `FormAction` is unset. The file is owned by 1.13, and the fallback is additive and inert when `ListPath` is null; without it the selector would submit a GET to the POST-only `ApproveSelected` action after a batch re-render. `_ListToolbar` already uses the same convention.
- `Views/Shared/_BulkConfirmDialog.cshtml` and `Models/BulkSelectionViewModel.cs` gained an init-only `SelectionListLabel` (default `"Selected requests"`) so the certificate surface's dialog list announces "Selected reviews". The default keeps every existing surface unchanged.
- The controller re-renders rather than redirecting, matching the 1.16 account-request batch, so the per-record result panel survives without TempData.

## Plan Change Log

## Review Triage Log

### 2026-11-16 — Review pass
- verdicts: 7 findings — high 0, medium 1, low 5, false 1, maybe-false 0
- findings:
  - `[low]` `[patch]` Empty-selection and over-bound notices read "review"/"reviews" instead of the matrix's "certificate review"/"certificate reviews" — patched: both controller call sites and the service now pass `"certificate review"`, and the unit-test assertions updated.
  - `[false]` `[reject]` Claim that the mixed-selection criterion and the `ISSUED_OR_DECIDED` row cannot be produced by the surface, so "the result panel can only ever report successes" — refuted: a pending row whose attendance is invalidated or whose review is decided between page load and submit is selectable and produces a refusal, which `BulkApprovalRefusesInvalidatedAttendancePerRecordAndLeavesTheReviewsPending` and the mixed-selection test prove. Only pending rows carry a checkbox by design; the ticket requires the refusal to be reported, not that an already-decided row be selectable. The only remaining fix would be rewording the plan's own acceptance criterion, which triage rejects.
  - `[low]` `[patch]` Undeclared change to `_EventSelector.cshtml`, a file owned by 1.13 — patched: the dependency and its reason are now recorded under Implementation Notes, and the change is additive and inert when `ListPath` is null.
  - `[medium]` `[defer]` The AGENTS.md UI review gate (impeccable critique and polish) was not run for this UI-facing change — deferred: the critique command is interactive and cannot complete in an unattended run, and the surface composes 1.16's already-critiqued components; the human action is recorded in the plan's `deferred` list.
  - `[low]` `[patch]` `PRODUCT.md` officer certificate ledger bullet omits bounded bulk approval — patched: the bullet now names bounded bulk approval of eligibility reviews with issuance staying individual.
  - `[low]` `[patch]` The confirmation dialog's selected-items list announces "Selected requests" on the certificate surface — patched: `BulkSelectionViewModel.SelectionListLabel` (default "Selected requests") is rendered by the partial and set to "Selected reviews" by the ledger view.
  - `[low]` `[patch]` `ApproveSelected` binds `search` and `filter` but never reads them, so the signature advertises state the action does not use — patched: both parameters removed; the re-render still restores search and filter through the request query string and `ListReadOptions`.

## Design Notes

`ApproveManyAsync` calls `DecideAsync` rather than extracting a shared inner method: `DecideAsync` already opens its own transaction, takes the `users`-then-`event` locks, re-reads status, seat and attendance, writes one audit entry and commits. An outer batch transaction would hold locks across records and turn one refusal into a whole-batch rollback, which the ticket forbids. The cost is one `DiscoverIdentityAsync` per record, the same read the individual action already pays.

Selection needs no explicit clear-on-change code: page, event, search and filter are full navigations, and `bulk-selection.js` already clears on load and on bfcache restore.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` -- expected: succeeds with no new warnings.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` -- expected: all pass. If it reports `Zero tests ran` and exits 5, run `& "tests\ASI.Basecode.Services.Tests\bin\Debug\net10.0\ASI.Basecode.Services.Tests.exe"`.
- `node ASI.Basecode.WebApp/tests/bulk-selection.test.js` -- expected: the shared selection tests pass.
- `$env:ATTENDTA_TEST_SQL="Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` -- expected: the extended certificate decision cases pass.

## Auto Run Result

**Summary:** Bounded bulk approval of pending Certificate Reviews on the eligibility screen. `CertificateLedgerService.ApproveManyAsync` deduplicates the posted ids, resolves every display label in one read, then runs the existing `DecideAsync` once per record so each approval keeps its own transaction, `users`-then-`event` lock order, re-read of status/attendance/completion and single `certificate.approved` audit entry. No PDF is rendered; declines and issuance stay individual. The eligibility tab gains the shared 1.16 bulk selection bar, confirmation dialog and per-record result panel, bounded to 25 ids, and a batch re-renders the reader's event, tab, search, filter and page.

**Files changed:**
- `ASI.Basecode.Resources/Constants/BulkApproval.cs` — noun-based empty and over-bound notices built from `MaxBatchSize`.
- `ASI.Basecode.Data/Models/CertificateReviewSummary.cs` — new display-only identity for one review.
- `ASI.Basecode.Data/Interfaces/ICertificateRepository.cs`, `ASI.Basecode.Data/Repositories/CertificateRepository.cs` — `GetDecisionSummariesAsync` resolves names in one untracked read.
- `ASI.Basecode.Services/ServiceModels/CertificateApprovalOutcome.cs` — new per-record outcome.
- `ASI.Basecode.Services/Interfaces/ICertificateLedgerService.cs`, `ASI.Basecode.Services/Services/CertificateLedgerService.cs` — `ApproveManyAsync`.
- `ASI.Basecode.WebApp/Controllers/CertificateLedgerController.cs` — `ApproveSelected` POST plus a shared `RenderLedger` for GET and POST.
- `ASI.Basecode.WebApp/Models/BulkSelectionViewModel.cs`, `ASI.Basecode.WebApp/Views/Shared/_BulkConfirmDialog.cshtml` — `SelectionListLabel` so the dialog list names the right noun.
- `ASI.Basecode.WebApp/Views/CertificateLedger/Index.cshtml` — selection bar, checkbox column, confirmation dialog, result panel and error notice on the eligibility tab only.
- `ASI.Basecode.WebApp/Views/Shared/_EventSelector.cshtml` — `ListPath` fallback so the selector keeps pointing at the GET route after a re-render.
- `PRODUCT.md` — certificate ledger bullet names bounded bulk approval.
- `tests/ASI.Basecode.Services.Tests/CertificateLedgerBulkApprovalTests.cs` — new guard, dedupe and partial-success tests.
- `tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs` — mixed selection, already-decided, repeat submit, invalidated attendance and no-PDF cases.

**Review findings:** 7 findings — 5 patched (all `low`), 1 deferred (`medium`), 1 rejected (`false`). Patched: the notice noun, the `_EventSelector` dependency record, the `PRODUCT.md` bullet, the dialog list label and the unused `search`/`filter` parameters. Deferred: the interactive impeccable critique/polish gate, recorded in `deferred` with the human action. Rejected: the claim that refusals are unreachable from the surface — a row invalidated or decided between load and submit is selectable and is reported per record.

**Follow-up review recommendation:** false — no `high` entry was patched and no two `medium` entries were patched (patched: high 0, medium 0, low 5).

**Verification:**
- `dotnet build ASI.Basecode.sln --configuration Release` — succeeded, 0 warnings, 0 errors.
- Services tests — 371 passed, including the 6 new `CertificateLedgerBulkApprovalTests`.
- `node ASI.Basecode.WebApp/tests/bulk-selection.test.js` — 11 passed.
- Integration suite on SQL Server — 530 passed, including the 5 new certificate bulk cases and the full pre-existing suite.

**Residual risks:** the impeccable critique/polish gate is deferred to the human reviewer; `_EventSelector` is a 1.13-owned file touched additively, so a future 1.13 edit to that form may conflict; the "no `certificates/` file" guarantee is proven by throwing renderer and file-store fakes plus null `FilePath`/`CertificateNumber`/`IssuedAt`, not by scanning a real setup root.
