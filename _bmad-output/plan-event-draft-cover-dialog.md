---
title: 'Event cover in the event editor'
type: 'feature'
ticket: ''
created: '2026-11-16'
status: 'in-progress'
baseline_revision: '28a6086a59bcb17f4c24a87f05b9b21e99098732'
route: 'full'
route_source: 'auto'
risk: 'high'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
context: ['DESIGN.md', 'PRODUCT.md']
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Creating a draft and managing its cover currently require separate screens, unlike venue creation.

**Approach:** Put cover selection in the event editor using the venue form's familiar image input and preview pattern. The create and edit dialogs and their full-page fallbacks submit event details and cover changes together. Existing draft and published events can replace their cover; only drafts can remove one. Redirect existing cover links to the event editor instead of the separate cover page.

## Boundaries & Constraints

**Always:** Cover is optional on drafts and required for publication. Preserve management authorization, antiforgery, JPEG/PNG content validation, the 2 MiB limit, private storage, event guards, and accessibility. Failed combined saves must not silently create an incomplete draft or lose an existing cover.

**Never:** Expose private file paths, weaken publication or event-state rules, or change unrelated event workflows.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected behavior | Error handling |
|---|---|---|---|
| Create | Valid event and valid optional cover | Save draft with its cover in one action | No error |
| No cover | Valid event without a cover | Save a coverless draft | Publication remains blocked until a cover exists |
| Edit cover | Draft or published event with replacement, or draft with explicit removal | Save details and cover changes together; unchanged cover stays intact | Reject removal on published or terminal events and contradictory remove/replace requests |
| Invalid image or event | Oversized, invalid image, or invalid event fields | Keep editor open and retain other input; do not create a partial event | Show actionable errors and prompt file reselection when necessary |
| Save failure | Storage or database failure | No false success; protect existing data and clean staged files safely | Show an appropriate failure; preserve retry path |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Views/Event/_Editor.cshtml`, `_Form.cshtml`, `Create.cshtml`, `Edit.cshtml` — shared editor and full-page fallback; add multipart image field consistently with `Views/Venue/_Form.cshtml` and `_Editor.cshtml`.
- `ASI.Basecode.WebApp/wwwroot/js/features/record-dialog.js` — existing FormData submission and image preview; preserve dirty-state, focus, error and file reselection behavior.
- `ASI.Basecode.WebApp/Controllers/EventController.cs` — create/edit actions, UTC conversion, validation re-render and dialog responses; accept uploaded file only at web boundary.
- `ASI.Basecode.Services/Interfaces/IEventLifecycleService.cs`, `Services/EventLifecycleService.cs` — create and edit transaction boundaries; stage files using existing private store, preserve locking and cover audit behavior; avoid a second independent upload after creating the event.
- `ASI.Basecode.WebApp/Storage/PrivateFileStore.cs` — existing server-side file validation and private storage; reuse, do not bypass.
- `ASI.Basecode.WebApp/Views/Event/Index.cshtml`, `Details.cshtml`, `Cover.cshtml` — existing cover navigation; send editable events to the editor, preserving read-only terminal event behavior and compatible existing POST endpoints.
- `tests/ASI.Basecode.Services.Tests/EventLifecycleServiceTests.cs`, `EventCoverServiceTests.cs`, `tests/ASI.Basecode.IntegrationTests/EventDraftFlowTests.cs`, `EventCoverFlowTests.cs`, `PublishedEventEditFlowTests.cs` — regression patterns and coverage.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.Services/Interfaces/IEventLifecycleService.cs`, `ASI.Basecode.Services/Services/EventLifecycleService.cs` — combine image staging and event persistence; revalidate state under ordered locks, avoid deleting a concurrent cover, and make cleanup commit-aware, including uncertain or nested transaction outcomes. Preserve authorization and path-free audit.
- [ ] `ASI.Basecode.WebApp/Controllers/EventController.cs`, `ASI.Basecode.Services/ServiceModels/EventViewModel.cs` — bind file, populate server-derived cover presence on every edit render, restore authoritative published locked fields after errors, and report failures without misleading success.
- [ ] `ASI.Basecode.WebApp/Views/Event/` and `ASI.Basecode.WebApp/wwwroot/js/features/record-dialog.js` — provide accessible venue-consistent image controls, preview and multipart submission on dialog and fallback forms; ensure fallback preview works outside the dialog. Route editable cover navigation to the editor, while preserving terminal read-only access and compatible legacy POSTs.
- [ ] `tests/ASI.Basecode.Services.Tests/`, `tests/ASI.Basecode.IntegrationTests/` — cover the matrix, access control, existing state guards and failure cleanup.
- [ ] `PRODUCT.md`, `DESIGN.md` — update only if the established editor contract changes.

**Acceptance Criteria:**
- Given an officer opens the create dialog, when they select an image, then they can preview it and save the draft without visiting a separate cover page.
- Given a keyboard or mobile user, when they operate the editor, then controls remain labelled, usable, and consistent with venue creation.
- Given a visitor uses the standalone create form, when they submit the same valid inputs, then it behaves consistently with the dialog.
- Given a draft or published event, when management saves a replacement in either editor, then details and cover change together; draft-only removal and publication requirements remain enforced.
- Given another request changes the event or cover during editing, when the save is processed, then state guards are rechecked under lock and no unrelated cover is deleted or overwritten by a no-cover edit.
- Given a failed validation, storage operation, or database operation, when the editor returns, then it gives actionable feedback, preserves entered fields and the existing cover, and does not falsely report success.

## Implementation Notes

- Implementation has not begun. The implementer identified a conservative transaction-aware approach but could not build or run tests because C: has no free space. The failed build may have produced partial generated output.
- Resumed after verifying approximately 5 GB free on C:.
- Implemented optional cover selection and preview in create/edit and standalone forms, atomic event and cover updates, draft-only removal, stale-cover checks, safe audit values, and regression tests. A Release build and 388 service tests passed; the implementer reports eight focused integration tests passed. The full integration run exceeded four minutes and is being retried with a longer allowance.
- Independent Impeccable critique could not start because the Terra subscription-sharing limit was reached. Rendered desktop/mobile checks and polish remain incomplete. Uncertain transaction outcomes can leave unreferenced files in private storage rather than risk deleting a referenced image.
- Combined edits now write one event audit entry, with path-free cover facts only when the cover changes. Focused integration coverage exercises draft replacement/removal, published replacement/removal rejection, authentication, antiforgery, validation, stale covers, failed SQL writes, and an outer transaction rollback. An uncertain commit is also simulated by the service tests; files are intentionally retained when the outcome is uncertain.
- Release build, 388 service tests, and eight focused integration tests passed. Full integration verification and rendered desktop/mobile critique and polish remain outstanding; do not treat the plan as complete until those checks are performed.

## Plan Change Log

- Plan review: added explicit server-derived cover state, fallback preview, compatibility routing, concurrency and commit-aware cleanup, published-field rehydration, and UI critique/polish verification to avoid partial saves and lost images.

## Review Triage Log

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — succeeds.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` — service tests pass; use the documented direct test host if zero tests run.
- `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` — focused integration coverage passes with `ATTENDTA_TEST_SQL` configured.
- Run the repository's CSS build when views/styles change, and the Impeccable detector once on changed UI files.
- Run Impeccable critique and polish on the changed surface before review; this unticketed change has no documented demo/integration exception.

**Manual checks:** Inspect create/edit dialog and fallback at desktop and mobile widths; check preview, validation, keyboard use, cancellation and reselection.
