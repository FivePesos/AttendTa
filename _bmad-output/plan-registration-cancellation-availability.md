---
title: 'Registration cancellation availability'
type: 'bugfix'
ticket: ''
created: '2026-11-20'
status: 'in-progress'
baseline_revision: '00e0c9bfeccd873098b064e885efe8372ceafb5c'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
context: ['DESIGN.md', 'PRODUCT.md']
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Registration Review offers cancellation for historical or otherwise ineligible records, only to reject the action after the officer tries it. Event dossier rows can also offer cancellation despite valid attendance.

**Approach:** Keep registration history visible and show cancellation only where the current event, registration and attendance facts permit it. For confirmed and waitlisted rows, show only “Cancellation unavailable” in the row and explain every applicable blocker within Show details. Keep rows compact on both management surfaces.

## Boundaries & Constraints

**Always:** Preserve the authoritative transactional cancellation checks, permissions, status and audit behavior, search, pagination, existing registration records, and available actions. Use the same availability semantics across Registration Review and the event dossier. Availability is advisory: handle changes between rendering and submission with the existing server rejection.

**Never:** Delete or change historical registrations, relax cutoff or valid-attendance safeguards, or implement the upcoming/past filter in this step. Do not change the demo database while testing.

## I/O & Edge-Case Matrix

| Scenario | State | Expected output | Error handling |
|---|---|---|---|
| Eligible cancellation | Confirmed or waitlisted, published event, before start, no valid attendance | Cancellation action appears in both management surfaces | Server still rechecks upon POST |
| Historical or closed event | Completed, cancelled or draft event, or exact start reached | No cancellation action; compact unavailable label and explanation in Show details | Show details and historical row remain available |
| Valid attendance | Otherwise eligible registration with non-invalidated attendance | No cancellation action; Show details identifies attendance | Keep record unchanged |
| Multiple blockers | Ineligible event and valid attendance | Explain each applicable event and attendance blocker in Show details, consistently on both surfaces | Preserve status and details |
| Invalidated attendance | Otherwise eligible registration with invalidated attendance | Cancellation remains available, consistent with server rule | Recheck on submit |
| Other statuses | Pending or already-cancelled registration | Preserve existing pending review actions and cancelled-row details; do not add a misleading cancellation message | Existing rules remain authoritative |
| Concurrent change | Event, attendance or registration status changes after page render | Server rejects stale action without losing error, row context or entered reason, even if the trigger disappears | Never authorize based on UI flag; no new cancellation audit |
| Mixed paged queue | Rows for multiple events, search and pagination | Each row reflects its own event and attendance; no per-row database requests | Existing ordering and filters unchanged |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/Services/RegistrationReviewService.cs` `ExecuteAsync` owns authoritative eligibility and lock/recheck. `GetPageAsync` returns paged `RegistrationReviewRow` and has injected `TimeProvider`; capture one UTC instant per page and compare stored start timestamps with the same semantics as the command. Preserve `ParticipationFacts.StartCutoffPassed`, including one tick before, at and after the boundary; never compare Manila-formatted text.
- `ASI.Basecode.Data/Interfaces/IRegistrationRepository.cs` `RegistrationReviewRow`, `ASI.Basecode.Data/Repositories/RegistrationRepository.cs` `ReviewQueue`: project event status, start time and canonical `ParticipationFacts.ValidAttendances` existence with a registration-keyed correlated `Any`, not a multiplying join. Avoid N+1, preserve the count query's behavior and cost, and avoid a SQL time comparison that rounds away a tick.
- `ASI.Basecode.WebApp/Views/RegistrationReview/Index.cshtml`: currently shows Cancel for every Confirmed/Waitlisted row. Render only eligible actions and plain-text reason when unavailable while preserving shared details, POST dialog, failure recovery and antiforgery.
- `ASI.Basecode.WebApp/Controllers/EventController.cs`, `ServiceModels/EventDossierViewModel.cs`, and `Views/Event/Details.cshtml`: event facts and roster are loaded separately and use separate clock reads. Use a consistent event snapshot and one captured instant for the dossier's event-level notices/review controls and per-row availability, including when facts change between reads. Preserve pending and promotion actions and do not replace transactional rechecks.
- `ASI.Basecode.WebApp/wwwroot/js/site.js` and both views: test failed-dialog reopening and reason/error retention when a refreshed row's cancellation trigger is gone, including the existing fallback when the row leaves the page.
- `tests/ASI.Basecode.IntegrationTests/RegistrationReviewTests.cs`, `RegistrationPromotionTests.cs`, `EventDossierLayoutTests.cs` and `tests/ASI.Basecode.Services.Tests/RegistrationReviewTests.cs`: exercise existing review rules and rendering, including fixed-clock boundaries and stale writes; add targeted coverage and a bounded-query regression where practical.
- UI follows `DESIGN.md`, shared `row-actions` and responsive table patterns. Run applicable UI critique/polish and report unavailable visual checks.

## Tasks & Acceptance

**Execution:**
- [ ] Extend the paged row projection with event and valid-attendance facts; calculate display availability using the injected clock and existing rules without altering count, sorting or authorization.
- [ ] Update the two management views to show the compact unavailable label in rows and all applicable blockers in Show details in a stable order, preserving pending/cancelled behavior and details. Align dossier event snapshot and instant with row display, including changes between reads.
- [ ] Add automated tests for every matrix case, including mixed events; one tick before, at, and after start; invalidated versus valid attendance; multiple simultaneous blockers; registration/event/attendance stale POST and audit; failure reason retention; and bounded page/count queries.
- [ ] Run required UI critique and polish unless the documented project exception applies; verify desktop/mobile and keyboard when available, and explicitly document anything unverified.

**Acceptance Criteria:**
- Given an eligible confirmed or waitlisted registration, when an officer views either surface, then Cancel registration is available and can still be submitted with a reason.
- Given a past, unpublished or valid-attendance registration, when viewing either surface, then the row shows “Cancellation unavailable”, cancellation is absent, and Show details explains why.
- Given a confirmed or waitlisted row with multiple blockers, when opening Show details on either surface, then each applicable blocker is shown; pending and cancelled rows keep their existing actions and details.
- Given an action that becomes stale after rendering, when submitted, then the existing server checks refuse it without changing the registration.
- Given a rejected stale cancellation whose trigger has disappeared, when the page returns, then the error and entered reason remain accessible with row context or the existing off-page fallback.
- Given mixed records and an active search/page, when the queue renders, then each row's availability is correct without changing the filtered total or order.

## Implementation Notes

The advisory projection and two views are implemented. The existing cancellation command and transaction guards are unchanged. SQL-backed tests exercise eligible, invalidated attendance, and completed-with-valid-attendance rows on both surfaces, stale submission and reason retention; 58 relevant integration tests and 8 dossier/query tests passed. The queue count and page issued two reads for four rows; count-query SQL cost and mixed-event search remain unchecked. Independent review identified stale POST redirect losing search/page size, dossier concurrent event-read consistency, and incomplete stale-dialog tests; these must be addressed before acceptance. Desktop/mobile visual critique and polish remain unfinished. The normal Release solution build was blocked by a running demo server; the Release integration project and its referenced projects built successfully into a separate output directory after freeing temporary build outputs. A later dossier read-order change and CSS wrapping change still need a rebuild and test run.

2026-11-20 follow-up: Build and tests on a separate F: copy passed (419 service, 71 JavaScript, 85 focused integration). Review found that the off-page Registration Review fallback retains the reason but not registration context and the queue promotion form drops search/pageSize. Fix both, add regression tests for stale cancellation and redirect preservation on both surfaces including promotion, and cover event changes between dossier reads. Read the existing code first and preserve uncommitted work. The running demo must not be stopped or its database modified. C: is nearly full: do not build there. For builds and SQL-backed tests use a fresh copy on F:, set TEMP/TMP/DOTNET_CLI_HOME to an F: directory, and in the copy only direct the isolated fixture's generated test database files to F:. Do not delete or overwrite unrelated files. Report incomplete checks explicitly.

Follow-up implementation: the queue fallback now includes the registration ID with the unsaved reason, and the queue promotion form carries search and page size. A full Release solution build and 10 SQL-backed dossier/query tests, including two new queue regression tests, passed from the F: copy. The promotion test checks the rendered form, not a completed promotion POST; dossier redirect/stale-dialog coverage, interleaved event-read coverage, the wider matrix and desktop/mobile/keyboard checks remain unfinished. No demo restart, commit or push.

Additional follow-up: 18 SQL-backed dossier tests passed from an F: copy. They include actual promotion POST redirects from both surfaces, stale cancellation caused by event, attendance, and registration changes on both surfaces with state and audit assertions, and deterministic event-read interleaving in both directions. A desktop browser DOM check found the compact label and details with no page overflow at 1453px; screenshot capture failed because the browser window was not visible. Mobile and keyboard presentation and browser execution of stale-dialog recovery remain unverified. The separate temporary verification server was stopped; the original demo was left running.

Latest combined verification: Release solution build succeeded with no warnings or errors on F:, followed by 422 service tests and 109 focused integration tests passing. Added mixed-event searched queue, all relevant statuses, exact cutoff on both screens, successful dossier cancel, and count SQL checks. The cutoff helper now uses a tested in-memory twin instead of compiling an expression per row. The 71 JavaScript tests passed before this final non-JS change. SQL execution-plan cost, live stale-dialog recovery, mobile and keyboard checks, and critique/polish remain unverified. Independent review was performed before the final test additions and cutoff optimization; those final changes still need review. No commit or push.

2026-11-20 targeted integration follow-up: Added successful promotion POST coverage on both surfaces from a real second page with search and page size, checking redirect criteria, confirmed status and one promotion audit. Added stale cancellation POST coverage for event, attendance and registration changes on both surfaces, checking preserved reason/error/context, redirect criteria, unchanged status and no registration audit; on-page cases also check the missing trigger and reopen markup. Release integration project build and all 16 EventDossierLayoutTests passed on an isolated F: copy with generated LocalDB files on F:. No production defects found by these tests. Interleaved dossier event-read coverage, browser execution of dialog recovery, the wider matrix audit, UI critique/polish and desktop/mobile/keyboard checks remain outstanding. Demo and demo DB were not touched.

2026-11-20 interleaved-read follow-up: Added a test-only review-service gate after the real roster read and before the event read. Both published-to-completed and completed-to-published changes verify the old roster snapshot and the later rendered event state, notice, and cancellation availability agree. The Release integration project built with zero warnings and errors, and all 18 EventDossierLayoutTests passed using the F: copy and isolated F: LocalDB files. Browser execution of dialog recovery, wider matrix audit, UI critique/polish and desktop/mobile/keyboard checks remain outstanding.

2026-11-20 matrix coverage follow-up: Added SQL-backed rendered tests for a searched six-row, two-event, two-page queue with per-row attendance; pending, cancelled, and eligible/ineligible waitlisted and confirmed rows on both surfaces; one tick before, at, and after start through fixed-clock service and both views; and successful dossier cancellation with both audits. The bounded-read test now checks generated searched count SQL has no attendance reference while the page SQL does. The Release integration project built with zero warnings/errors and the two focused integration classes passed all 32 tests on the isolated F: copy. These checks do not measure a SQL execution plan or cost, execute dialog JavaScript in a browser, or replace the remaining full-matrix audit and visual/keyboard review.

## Plan Change Log

- 2026-11-20 dialog recovery follow-up: Auto-fit can navigate before the jQuery-ready dialog handler, consuming one-shot TempData; mark failed queue/dossier recovery and suppress refitting for that page, including the off-page fallback. In a separate F: copy, 72 JavaScript tests and 422 service tests passed. The full solution build did not finish within the time limit (initial restore also hit a full C: NuGet cache); WebApp and service projects did build in the final attempt. New SQL-backed rendering assertions, real-browser reproduction, matrix audit and visual/keyboard checks remain unverified. The demo and its database were not touched.

- 2026-11-20: User requested a compact “Cancellation unavailable” row label, with all reasons moved to Show details to avoid increasing row height. Cancellation rules remain unchanged.

## Review Triage Log

## Verification

- `dotnet build ASI.Basecode.sln --configuration Release`; service tests and the relevant SQL-backed registration/dossier tests against disposable LocalDB; report all failures or skips.
- `npm run test:js` from `ASI.Basecode.WebApp` and CSS build only if styles change.
- Inspect desktop and 390px if a visible browser is available; check a mix of eligible/ineligible rows and stale submission handling. Do not claim unrun visual checks.
