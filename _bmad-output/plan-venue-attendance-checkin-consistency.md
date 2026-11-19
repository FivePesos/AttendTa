---
title: 'Venue and attendance interface consistency'
type: 'bugfix'
ticket: ''
created: '2026-11-19'
status: 'in-review'
baseline_revision: '3549124fec1ce3bc7fc961191deb52ed376f81f1'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: ['DESIGN.md', 'PRODUCT.md']
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Venue photo selection lacks a preview, and personal attendance rows and recorded check-in actions do not consistently align with the rest of the application. Scrollbars also appear too prominent to the user.

**Approach:** Reuse established photo and table patterns across create/edit and server/live-rendered states. Retain native scrollbars: hiding them would make independently scrollable regions harder to discover and use; assess actual overflow during visual checks rather than changing global scrolling speculatively.

## Boundaries & Constraints

**Always:** Preserve current permission checks, file validation, private storage, scan/invalidation behavior, keyboard access, focus, natural scrolling, and responsive labelled rows. Match the existing design system; ensure both dialog and standalone forms work.

**Never:** Hide scrollbars, introduce global thin/custom scrollbar styling, change backend business rules or text-length limits, change the check-in log heading or scope, alter unrelated tables, or treat a local preview as proof that a file passed server validation. Do not implement during plan approval.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected behavior | Error handling |
|---|---|---|---|
| Create venue | Select JPEG/PNG in dialog or full page | Selected image preview appears with meaningful alt text | Required photo and server validation remain authoritative |
| Edit venue | Select replacement, then change or clear selection | Preview reflects current selection; saved photo remains identifiable | Clearing removes the new preview, not the saved photo |
| Validation rerender | Create/edit with a selected photo fails validation | Show available saved photo and explain that the selected file must be chosen again, even if the saved photo is unservable | Preserve errors and required indicators; no stale preview |
| Connection / uncertain save | Dialog retains its form and selected photo | Keep the selection and preview; do not say it was lost | Retain existing uncertainty and duplicate-save guidance |
| Check-in log | Initial row, successful live scan or repeat scan | Columns and action styles match; live scans retain their existing immediate Invalidate button | Existing server rules continue to reject invalid actions; reloaded states remain unchanged |
| Short viewport / zoom | Page, sidebar, dialog, or nested list overflows | Content remains reachable with native scroll affordance | No hidden or clipped controls |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Views/Venue/_Form.cshtml` and `Controllers/VenueController.cs` — shared photo input and saved photo; add a separate preview host and create-failure reselection guidance on both invalid model and domain-error responses. `Create.cshtml` and `Edit.cshtml` currently lack the preview script.
- `ASI.Basecode.WebApp/Views/Event/_Form.cshtml` and `wwwroot/js/features/record-dialog.js` — established preview host and delegated object-URL handling for both dialog and standalone forms; preserve revocation and dialog cleanup. Venue index uses `_RecordDialog`.
- `ASI.Basecode.WebApp/Views/MyEvents/_AttendanceHistoryPartial.cshtml` — Recorded cell has `numeric` but lacks `cell-date`; shared date alignment is in `Styles/app.css`.
- `ASI.Basecode.WebApp/Views/CheckIn/Index.cshtml` — recorded log action header lacks shared actions sizing; existing rows have `.row-actions`. Manual lookup and log cells lack responsive `data-label` values.
- `ASI.Basecode.WebApp/wwwroot/js/features/scanner.js` — `appendLog` creates an Invalidate button for success and repeat outcomes regardless of eligibility. Trace scan response and authoritative eligibility before changing presentation; do not change server rules or log scope.
- `ASI.Basecode.WebApp/Styles/app.css` and `Styles/features/record-dialog.css` — shared row, mobile and preview styles; source CSS is built into `wwwroot/css/app.css`. Document, rail, dialogs and nested lists can scroll independently.
- `ASI.Basecode.WebApp/tests/table-layout.test.js`, `tests/site.test.js`, `tests/ASI.Basecode.IntegrationTests/VenuePhotoFlowTests.cs` and `CheckInFlowTests.cs` — existing testing patterns and flow coverage; verify actual coverage before extending.

## Tasks & Acceptance

**Execution:**
- [ ] `Views/Venue/_Form.cshtml`, `Views/Venue/Create.cshtml`, `Views/Venue/Edit.cshtml`, `Controllers/VenueController.cs` — reuse event preview host and script on dialog and standalone paths, retaining a separately labelled current photo and existing upload constraints. After failed creation with a selected photo, explain that it must be selected again, preserving required-field indication and validation on invalid-model and domain-error paths.
- [ ] `Views/MyEvents/_AttendanceHistoryPartial.cshtml` — apply the existing date-cell convention to Recorded, visibly label nonempty invalidation reasons, and use `scope="row"` on the event header if mobile labels and styling remain correct; do not change date formatting or attendance data.
- [ ] `Views/CheckIn/Index.cshtml`, `wwwroot/js/features/scanner.js` — apply existing compact action-column/row conventions and matching mobile cell labels to server-rendered and live rows. Keep the previous immediate Invalidate button and its server-side eligibility checks, as requested; do not change the server contract. Preserve hooks and values. Label manual lookup cells as well.
- [ ] `Styles/app.css`, `Styles/features/record-dialog.css` — change only if existing shared styles cannot meet the checked layouts; if changed, regenerate `wwwroot/css/app.css`. Leave native scrollbars alone.
- [ ] `ASI.Basecode.WebApp/tests/` and `tests/ASI.Basecode.IntegrationTests/` — add focused regression coverage for selected/replaced/cleared previews, create/edit validation rerenders including unavailable current photos, and connection failure retention. Check attendance date, reason and mobile row-header semantics. Verify live/reloaded action eligibility and mobile labels; exercise a repeat scan and certificate-issued state, the empty-to-first-row transition, and the invalidate dialog's correct participant and registration ID. Retain existing venue and check-in flow coverage.
- [ ] Inspect rendered desktop, mobile and 200% zoom states, including long reasons/descriptions, unbroken words and line breaks in the affected attendance table, event type table and venue details dialog; if a layout defect is reproduced, propose a targeted fix without modifying limits or data. Run required UI critique and polish unless the repository's documented demo/integration exception applies, and run the Impeccable detector after UI edits. Record any unavailable browser or infrastructure checks.

**Acceptance Criteria:**
- Given a venue form, when an accepted image is selected, then a distinct preview appears in both dialog and standalone create/edit flows while existing server rules remain unchanged.
- Given a selected venue photo, when it is replaced or cleared, then the preview matches the current selection and any saved photo stays identifiable; a validation rerender explains reselection, while a retained selection after a connection failure is not falsely described as lost.
- Given a populated My Attendance table, when shown on desktop or mobile, then each date aligns with its heading or mobile label and other columns remain legible.
- Given recorded check-ins, when a row loads or is added live, then action placement, styling and mobile labels are consistent, the live Invalidate button appears immediately as before, and it targets the correct participant; the server retains final eligibility checks.
- Given content exceeding a viewport or dialog, when using keyboard, mouse or touch, then all content remains reachable without hidden scrollbars or clipped controls.

## Implementation Notes

The check-in log initially loads at most 25 recent records and can grow as live rows are added; the user expressly declined changing its label or scope. Shared details dialog values already wrap anywhere and preserve line breaks (`Styles/app.css`), and event-type descriptions use a measured Read more disclosure; verify actual long-text rendering rather than assuming limits prevent layout problems. The separate legacy `Views/Event/Cover.cshtml` also lacks a selected-file preview; its use and scope are unconfirmed and outside this plan.

Implementation: the scan response does not expose invalidation eligibility. Live rows currently say to refresh rather than guessing; parity with the reloaded action needs approval for a server-response contract change. Desktop and 390px checks exposed and fixed row-header typography and mobile indentation. Actual browser 200% zoom remains unverified; the 720px viewport is only an approximation. The integration suite passed 696 tests with one skipped. In isolated demo testing, two unintended venue changes were made to the temporary demo database; no existing user database was modified.

User decision: restore the prior immediate Invalidate button on live scan rows, retaining improved styling and the existing server-side checks; do not alter the server response. This supersedes the temporary refresh-message decision above.

Verification: restored live action checked in desktop/mobile browsers using a synthetic scan; dialog showed the correct participant and ID. Additional integration tests cover rendered venue validation and keyboard reachability in a short mobile dialog. The review's mobile long-title overlap was fixed and rechecked at 390px with no overlap or horizontal overflow. Actual browser zoom at 200%, and complete keyboard/touch checks for every separate scroll region, remain unverified. The isolated demo server was stopped; its temporary database and files remain.

## Plan Change Log

## Review Triage Log

- **medium / patch** — The attendance row header's fixed height overrides the mobile auto-height rule. A long event title overlaps the next field, confirmed in the mobile long-text screenshot; restore automatic height at the mobile breakpoint.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — solution builds.
- From `ASI.Basecode.WebApp`: `npm run test:js` — JavaScript tests pass; run `npm run css:build` if source CSS changes.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` — service tests pass; if the documented zero-tests runner issue occurs, run the freshly built Release test host directly at `tests/ASI.Basecode.Services.Tests/bin/Release/net10.0/ASI.Basecode.Services.Tests.exe`.
- With local SQL configured as documented in `AGENTS.md`: `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` — integration suite passes.

**Visual and accessibility checks:** Compare selected photo and saved photo, file replacement/clearing, failed submission, and keyboard access in both form routes. Compare desktop and <=640px attendance and check-in rows, including a fresh scan, reload, invalidated and certificate-issued states. At 200% zoom and short viewport height, inspect page, rail, mobile navigation, dialogs and nested lists for scroll reachability, visible focus and clipped controls. Browser/OS scrollbar appearance varies; no visual behavior has yet been verified in a live browser.
