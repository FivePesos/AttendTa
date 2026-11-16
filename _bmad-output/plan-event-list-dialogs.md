---
title: 'Event list cancellation and cover dialogs'
type: 'feature'
ticket: ''
created: '2026-11-16'
status: 'built'
baseline_revision: '465319cad138b06b3c137ebd61bf39394254532b'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** From the Events list, cancelling an event or viewing a completed/cancelled event cover navigates away to a separate page, inconsistent with existing dialogs.

**Approach:** Open an event cancellation dialog and a read-only cover preview from their existing row-menu actions, keeping the current list context. Retain standalone routes as accessible fallbacks.

## Boundaries & Constraints

**Always:** Preserve management authorization, antiforgery, service-owned cancellation rules and audit behavior. Clearly show the targeted event, irreversible consequences, required reason, validation and recovery. Preserve current list filters and pagination, protect unsaved input, return focus to the visible menu trigger, and handle absent or unavailable images. Reuse existing dialog conventions and the private authorized image endpoint.

**Never:** Auto-cancel, alter event data just to test the UI, expose storage paths, remove standalone fallback routes, or change publishing or check-in flows.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Cancellation | Draft or published row; valid reason | Dialog names event; explicit confirmation cancels once and returns to same list context | Preserve existing authorization and business validation |
| Retry | Blank reason or service rejects a stale event | No cancellation; show actionable error and retain reason and target where still appropriate | Handle missing or terminal event safely |
| Cover | Completed or cancelled row with a cover | Dialog shows event name and authorized cover | Show a useful unavailable state if image cannot load |
| No cover | Completed or cancelled row with no cover | Dialog explains no cover is available | Never display a broken image |
| Fallback | JavaScript or dialog support unavailable | Existing standalone cancellation and cover pages remain usable | Do not leave dead controls |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Views/Event/Index.cshtml` — row menus, cancel and cover links, shared dialog shell; use unique IDs or a carefully scoped shared dialog.
- `ASI.Basecode.WebApp/Views/Event/Details.cshtml` — existing cancellation dialog and retry conventions, including reason and consequence; keep dossier behavior intact.
- `ASI.Basecode.WebApp/Views/Event/Cancel.cshtml` and `Cover.cshtml` — standalone fallback views; retain routes and behavior.
- `ASI.Basecode.WebApp/Controllers/EventController.cs` — `Cancel`, `CancelFromDossier`, `Cover`, and `Index`; implement an explicit list-safe retry path rather than redirecting to dossier or silently dropping service failures.
- `ASI.Basecode.Services/Services/EventLifecycleService.cs` — `GetPageAsync` maps list rows without explicitly setting `HasCover`, whereas `GetEventAsync` does; derive availability safely without N+1 queries or exposing paths.
- `ASI.Basecode.WebApp/wwwroot/js/site.js` — existing delegated dialogs, popover focus return, unsaved-change guard, whitespace validation and retry behavior. Keep shared handlers and avoid conflicting or stacked dialogs.
- `tests/ASI.Basecode.IntegrationTests/EventCancellationFlowTests.cs`, `EventDraftFlowTests.cs`, and `ASI.Basecode.WebApp/tests/site.test.js` — existing cancellation/fallback and dialog behavior coverage.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.WebApp/Views/Event/Index.cshtml` and relevant dialog partials — add cancellation and cover preview with event-specific identity, fallback navigation and appropriate empty/error states.
- [ ] `ASI.Basecode.WebApp/Controllers/EventController.cs` and service/viewmodel mapping as needed — preserve list state and reopen cancellation with its reason and error when validation or the service refuses; keep existing endpoints and permission boundaries.
- [ ] `ASI.Basecode.WebApp/wwwroot/js/site.js` or a narrowly scoped feature module — integrate dialogs with existing focus, dismissal, retries, and image failure handling without duplicating shared dialog listeners.
- [ ] `tests/ASI.Basecode.IntegrationTests/EventCancellationFlowTests.cs`, relevant cover integration tests, and `ASI.Basecode.WebApp/tests/` — cover normal/fallback routes, state and error handling, authorization, missing cover, and focus/unsaved behavior.
- [ ] Update design/product context only if implementation materially changes the documented contract; run required UI critique and polish on the changed surface.

**Acceptance Criteria:**
- Given a draft or published event in a filtered or paged list, when Cancel is chosen, then a modal identifies the event, explains consequences and requires a reason; cancellation happens only on confirmation.
- Given a rejected cancellation, when the list returns, then the relevant dialog displays the error and retains the submitted reason and context without modifying the event.
- Given a completed or cancelled event, when View cover is chosen, then a modal shows the image or a clear unavailable/no-cover message without leaving the list.
- Given JavaScript is unavailable, when either action is selected, then its existing full-page fallback remains usable.
- Given a modal is dismissed, when it closes, then focus returns to its visible row-menu trigger and edited input is protected from accidental dismissal.

## Implementation Notes

The list now opens event-specific cover previews and a shared cancellation dialog, retaining full-page fallback links. Cancellation preserves the local return URL, restores rejected input, and keeps authorization and antiforgery checks. The list projection includes a cover-presence flag without disclosing storage keys.

Verification so far: 49 JavaScript tests passed, and 20 selected LocalDB integration tests passed, including cancellation and cover flows. The integration-test project builds in a separate output directory. The full solution build remains blocked by the running preview locking Release assemblies; the branch also has the previously identified unrelated `User.UserId` service-test compilation error. An attempted build with `UseAppHost=false` additionally failed because xUnit requires an app host; that flag must not be used for the solution build.

With user approval, updated the stale login test to use `User.Email`. After restarting the preview from isolated build output, the full Release solution build passed without warnings or errors.

At the initial verification checkpoint, the desktop browser displayed the updated list, but automated clicks failed with `UnknownVizError`; interactive dialog, keyboard, mobile, and no-JavaScript browser checks remained unverified. Final review, critique and polish were still pending at that point.

Final verification: full Release solution build passed with zero warnings or errors; 51 JavaScript tests and 20 focused LocalDB integration tests passed. Quick code review and source-based UI critique were completed and actionable findings patched. Rendered desktop/mobile and screen-reader checks remain outstanding because browser automation could not click the controls.

## Plan Change Log

## Review Triage Log

- Medium, patched: missing Popover API could throw before opening a dialog; the handler now calls `hidePopover` only when available, with a JavaScript regression test.
- Medium, patched: a stale or missing event may no longer have a row-menu invoker; focus now falls back to the Events list region, with a JavaScript regression test.
- Low, patched: a new integration test used a fixed password; it now generates a unique password.
- Medium, addressed in part: independent source-based design critique and detector were run; cover content inset, cancellation description and image-failure recovery were corrected. Browser automation failed, so rendered desktop/mobile critique and polish remain unverified.

## Verification

**Commands:**
- `npm run test:js` in `ASI.Basecode.WebApp` — JavaScript tests pass; rebuild CSS if styles change.
- `dotnet build ASI.Basecode.sln --configuration Release` — build; report the already known baseline test compilation issue if still present, without altering unrelated work.
- With `ATTENDTA_TEST_SQL` set to the LocalDB master connection, run the Release integration test host for the affected event cancellation and cover flow classes; tests create their own disposable databases.
- Exercise list dialogs on desktop and mobile, including keyboard dismissal, focus restore, validation retries, missing covers and fallback pages. Do not submit cancellation against existing demo records.
