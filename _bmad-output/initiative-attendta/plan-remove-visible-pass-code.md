---
title: 'Remove misleading pass-code tab'
type: 'refactor'
ticket: ''
created: '2026-11-18'
status: 'in-progress'
baseline_revision: '6cae2f4fd182a0c7ce1ce2360cb4b1bd7d2412b2'
route: 'oneshot'
route_source: 'auto'
risk: 'medium'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The visible Pass code tab asks for a code students cannot access, although camera scanning and manual lookup already cover the real workflows.

**Approach:** Remove the visible tab and field while keeping a hidden scanner form for the camera adapter. Retain QR validation, antiforgery, manual lookup, keyboard navigation, and no-JavaScript manual fallback. Update current camera guidance and tests; preserve unrelated uncommitted certificate-table work.

</frozen-after-approval>

## Implementation Notes

Focused oneshot change. `Views/CheckIn/Index.cshtml` must retain a hidden `data-scan-form` with its generated action and eventId; `wwwroot/js/features/scanner-camera.js` depends on that form. Remove the unused form-submit adapter from `scanner.js`, update camera-denied copy, `Styles/features/check-in.css`, `PRODUCT.md` and `docs/demo-quickstart.md`. Add tests for two-tab keyboard navigation, decoded scan posting and hidden form/no-JavaScript fallback. Do not expose tokens, change endpoint semantics, or commit unrelated pre-existing modifications.

## Plan Change Log

## Review Triage Log

- Independent review identified missing rendered markup coverage and stale scanner/demo instructions; addressed with a SQL-backed rendered-page test and updated copy. A missing hidden form would still make camera controls inert; the rendered-page regression test guards the required form and its action, event ID and antiforgery field.

## Verification

- `npm run css:build --prefix ASI.Basecode.WebApp` and `npm run test:js --prefix ASI.Basecode.WebApp` — pass (55 JavaScript tests).
- Release solution build with an isolated artifacts directory — succeeds, zero warnings or errors.
- Focused SQL-backed CheckIn tests — 26 passed, including rendered HTML, manual lookup and scan service coverage. The first attempt from a temporary output directory had two host failures because the test host could not locate the solution; using an output directory under the repository resolved this.
- Desktop/mobile visual inspection, real-camera verification and Impeccable critique/polish remain outstanding. The running demo uses the previous build and was not interrupted.
