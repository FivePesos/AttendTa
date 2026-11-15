---
title: 'Certificate eligibility bulk approval'
type: 'feature'
ticket: '14'
created: '2026-11-16'
status: 'in-progress'
baseline_revision: '0c758f72e45b7312b06e8ce871a4416b89a9887e'
route: 'full'
route_source: 'auto'
risk: 'high'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
followup_review_recommended: false
context: []
warnings: [oversized]
deferred: []
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

## Plan Change Log

## Review Triage Log

## Design Notes

`ApproveManyAsync` calls `DecideAsync` rather than extracting a shared inner method: `DecideAsync` already opens its own transaction, takes the `users`-then-`event` locks, re-reads status, seat and attendance, writes one audit entry and commits. An outer batch transaction would hold locks across records and turn one refusal into a whole-batch rollback, which the ticket forbids. The cost is one `DiscoverIdentityAsync` per record, the same read the individual action already pays.

Selection needs no explicit clear-on-change code: page, event, search and filter are full navigations, and `bulk-selection.js` already clears on load and on bfcache restore.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` -- expected: succeeds with no new warnings.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` -- expected: all pass. If it reports `Zero tests ran` and exits 5, run `& "tests\ASI.Basecode.Services.Tests\bin\Debug\net10.0\ASI.Basecode.Services.Tests.exe"`.
- `node ASI.Basecode.WebApp/tests/bulk-selection.test.js` -- expected: the shared selection tests pass.
- `$env:ATTENDTA_TEST_SQL="Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` -- expected: the extended certificate decision cases pass.
