---
title: 'Registration and check-in consistency'
type: 'bugfix'
ticket: ''
created: '2026-11-19'
status: 'in-progress'
baseline_revision: '95adec1c71981efb23e41d8d2e3c711959442ef1'
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

**Problem:** Check-in actions and event-roster columns are misaligned, the cancelled/declined roster tab has no count, account requests omit section in the list, and saved registration-decline reasons are not accessible on relevant screens.

**Approach:** Align the affected tables without changing other screens; show the matching filtered roster count and full academic profile; expose saved decline reasons in Show details on both management views and the member's own registration card.

## Boundaries & Constraints

**Always:** Preserve the existing local uncommitted check-in label fix, eligibility, authorization, pagination, filtering, decision behavior, and HTML encoding. Scope padding and alignment to the affected surfaces; keep mobile labels and keyboard/focus behavior. Member reason data must remain owner-scoped.

**Never:** Expose another member's data, invent reasons or treat every cancellation as a decline, change data or status transitions, alter unrelated tables, or change scrollbars.

## I/O & Edge-Case Matrix

| State | Input | Expected behavior | Error handling |
|---|---|---|---|
| Lookup and log | Buttons, checked-in or invalidated rows | Actions and headings align on desktop; mobile remains legible with right-edge breathing room | No change to action availability |
| Event dossier | Any roster tab, zero/multiple pages or search | All columns align and cancelled tab shows actual count using same event/search scope as sibling tab counts | No fabricated count |
| Account requests | Course, year, section or missing legacy section | List mirrors member list's labels and fallback; existing details remain | Missing data is not invented |
| Declined registration | Saved review reason, including long text or markup | Owner and management can open Show details and read encoded full reason | Missing reason is not shown; no other owner's data leaks |
| Other cancellation | Cancelled without decline review reason | Do not mislabel as declined or expose unrelated internal cancellation reasons | Existing status remains unchanged |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Views/CheckIn/Index.cshtml`, `Styles/features/check-in.css`, `Styles/app.css` — lookup action form and text differ from log's row-actions. Reuse scoped flex/spacing conventions, avoid global table changes. Existing local label edits must survive.
- `ASI.Basecode.WebApp/Views/Event/Details.cshtml` — roster tabs and dates; apply `cell-date`, action alignment, and cancelled count. It uses the shared details button.
- `ASI.Basecode.WebApp/Controllers/EventController.cs` `DossierAsync`, `ASI.Basecode.Services/ServiceModels/EventDossierViewModel.cs`, `RegistrationReviewService.GetCountAsync` — add cancelled count using the existing same event/search-scoped count path, including zero and pagination. Existing confirmed count also drives capacity; don't worsen or silently alter its search semantics.
- `ASI.Basecode.WebApp/Views/AccountRequest/Index.cshtml` and `Views/Member/Index.cshtml` — account request already projects section; mirror course/year/section display and fallback. Existing Show details remains.
- `ASI.Basecode.Data/Repositories/RegistrationRepository.cs`, `Data/Interfaces/IRegistrationRepository.cs` — owner-scoped `MyEventRegistrationRow` and management `RegistrationReviewRow` omit review notes. Decline writes `ReviewNotes` in `Services/RegistrationReviewService.cs`; other cancellation causes share Cancelled status. Project only the saved review note for cancelled rows; never infer a decline from cancellation reason alone.
- `ASI.Basecode.Services/Services/MyEventsService.cs`, `ServiceModels/MyEventsViewModels.cs`, `WebApp/Startup.AutoMapper.cs`, `Views/MyEvents/_RegistrationsTab.cshtml` — pass owner-scoped reason to member card and use existing shared details-dialog pattern, not a QR token or another member's record.
- `Views/RegistrationReview/Index.cshtml` and `Views/Event/Details.cshtml` — add saved review reason to each relevant row's existing Show details when present; omit otherwise.
- Tests: `tests/ASI.Basecode.IntegrationTests/{MyEventsRegistrationQueryTests,RegistrationReviewTests,EventDossierLayoutTests,AccountRequestQueueTests,CheckInLookupFlowTests}.cs`; `ASI.Basecode.WebApp/tests/table-layout.test.js`.

## Tasks & Acceptance

**Execution:**
- [ ] `Views/CheckIn/Index.cshtml`, `Styles/features/check-in.css`, `wwwroot/css/app.css` — align lookup text and buttons with action headings and add right spacing to unavailable messages in both tables; preserve stacked mobile layout and regenerate CSS.
- [ ] `Views/Event/Details.cshtml`, `Controllers/EventController.cs`, `ServiceModels/EventDossierViewModel.cs` — align roster dates/actions and add cancelled/declined count via same search/event scope as other badges, not page length.
- [ ] `Views/AccountRequest/Index.cshtml` — show course/year/section and mobile label like Members; retain details and empty/bulk states.
- [ ] `Data/Interfaces/IRegistrationRepository.cs`, `Data/Repositories/RegistrationRepository.cs`, `Services/Services/MyEventsService.cs`, `Services/ServiceModels/MyEventsViewModels.cs`, `WebApp/Startup.AutoMapper.cs` — carry decline review note only where appropriate; preserve actor-based ownership and existing queries' paging.
- [ ] `Views/MyEvents/_RegistrationsTab.cshtml`, `Views/RegistrationReview/Index.cshtml`, `Views/Event/Details.cshtml` — expose nonempty saved review reason through shared Show details with keyboard/focus return and safe Razor encoding; distinguish unrelated cancellations, retain prior actions and status.
- [ ] Relevant tests above — cover all matrix states, non-owner access, HTML-like/long reasons, empty reason, cancellation without review note, full filtered count across pages, and zero results.
- [ ] Browser inspection on desktop/mobile of both check-in tables, each roster state, member/management reason dialogs, and request list. Preserve long-text wrapping, visible focus, and no horizontal overflow. Run applicable UI critique/polish requirements; record exceptions or blockers.

**Acceptance Criteria:**
- Given an invalidated lookup result, when compared with a recorded check-in, then both show aligned no-action text with right padding and retain their existing status.
- Given a roster search or page, when viewing any tab, then headings and values align and all four tab totals reflect the same event/search scope, not the current page size.
- Given a request with a section, when viewing Account Requests, then course, year and section appear as in Members, including mobile labels.
- Given an owner's declined registration with a review reason, when opening Show details in My Registrations or either authorized management screen, then the correct complete reason is visible; another member cannot obtain it.
- Given a cancellation with no decline review note, when viewing details, then no decline reason is invented or displayed.

## Implementation Notes

Existing workspace edits belong to the preceding check-in label fix and must not be reverted. Review notes on historic data require verification; no assumption that a generic cancellation reason is safe to expose.

## Plan Change Log

## Review Triage Log

## Verification

- 2026-11-19: Release solution build passed before the final test additions; a separate Release integration-project build including those additions passed. Rebuilding the normal output during the full integration run was blocked by its running test host.
- JavaScript: 68 passed. Service tests: 415 passed. Targeted SQL-backed MyEvents and dossier tests passed, including owner isolation, long encoded reason, a real decline POST followed by all three reason views, zero/filtered counts and a six-row count across two pages.
- Full integration run: 702 passed, 1 failed, 1 skipped (704 total). The failure was an existing exact CSS-class assertion in RegistrationPromotionTests; updated to match the intentional date alignment and all 24 tests in that class passed afterward. A full rerun is needed. Desktop browser accessibility snapshot showed the cancelled count, but screenshot capture failed because the desktop window was not visible. Mobile, keyboard/focus, and visual critique/polish remain unverified.
- Implementation subagent could not start due to a subscription usage limit; implemented directly. A later independent inspection found no definite functional bug and identified test/visual gaps; the actual-decline test gap was subsequently covered, while visual verification remains open.

- `dotnet build ASI.Basecode.sln --configuration Release` — pass; stop/restart only the local demo server if it locks output.
- From `ASI.Basecode.WebApp`: `npm run css:build` and `npm run test:js` — pass.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` — pass.
- With disposable LocalDB configured, run `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` — pass, note skipped tests.
- Visually check 390px and desktop; check 200% zoom if available; inspect details and restored focus using keyboard. Do not silently claim unrun checks.
