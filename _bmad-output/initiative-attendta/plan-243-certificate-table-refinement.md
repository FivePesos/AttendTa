---
title: 'My Certificates table consistency'
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

**Problem:** My Certificates separates event timestamps from their names, uses different PDF row controls, and suggests contacting an officer even when no certificate is available yet.

**Approach:** Align timestamps with event names, use shared bordered row controls for available documents, and reserve officer-help guidance for issued documents that are unavailable. Preserve certificate states, access checks, PDF routes and preview behavior.

</frozen-after-approval>

## Implementation Notes

Oneshot: a focused refinement to one existing table and its tests. Preserve tabular figures, responsive stacking, keyboard focus, preview hooks, and 44px touch targets. Follow `DESIGN.md`; use Impeccable critique and polish and inspect the rendered result. The broader #243 PR remains a draft and is not completed by this change.

- The UI now left-aligns both event and issue metadata, uses bordered PDF controls, and reserves officer guidance for issued-but-missing documents. Updated `PRODUCT.md` and regenerated CSS.
- Release build and CSS build passed, 52 JavaScript tests passed, and focused SQL-backed certificate suites passed (5 personal-history tests and 1 ledger flow). Browser tests checked issued rows at 1440px and 390px, 44px mobile controls, horizontal overflow, and PDF preview; screenshots were inspected. The unissued state was checked through an HTTP integration test and the demo DOM.
- Independent design assessment and the source detector (zero findings) were run; live detector overlay, real screen-reader speech and a complete Impeccable critique/polish run remain unverified. The design assessment also identified pre-existing issued-status color and mobile field-indentation inconsistencies outside the three requested refinements; these remain for the broader #243 sweep.

## Plan Change Log

## Review Triage Log

## Verification

- `npm run css:build --prefix ASI.Basecode.WebApp` — builds tracked CSS.
- `npm run test:js --prefix ASI.Basecode.WebApp` — JavaScript suite passes.
- `dotnet build ASI.Basecode.sln --configuration Release` — build succeeds.
- With `ATTENDTA_TEST_SQL` set to local SQL Server master, run the focused `CertificateLedgerFlowTests` integration test class; verify available and unavailable document states and existing PDF behavior.
- Inspect rendered table on desktop and mobile, including available and unavailable document states and keyboard focus. Run Impeccable critique, polish and detector for the changed surface; record any blocked checks.
