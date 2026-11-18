---
title: 'Home management groups layout'
type: 'refactor'
ticket: ''
created: '2026-11-18'
status: 'in-progress'
baseline_revision: '57902010ca5ee6f0c9a7fae6fead4fc367c58677'
route: 'oneshot'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Management Home's vertically stacked destination groups require unnecessary scrolling on wider screens.

**Approach:** Place Manage records and Run operations side by side where space allows, keeping their existing vertical links and stacking the groups at smaller sizes. Member Home remains one group. Preserve role-specific links, navigation, keyboard order and existing uncommitted work.

</frozen-after-approval>

## Implementation Notes

Focused oneshot. Add a management-only grouping wrapper in `ASI.Basecode.WebApp/Views/Home/Index.cshtml`, and a responsive grid in `ASI.Basecode.WebApp/Styles/features/home.css`; retain existing link order and section semantics. Update `PRODUCT.md` and `DESIGN.md` to document the owner-approved Home exception. Add focused layout tests and retain existing SQL-backed role/link tests in `AuthenticationFlowTests`. Rebuild CSS without discarding existing certificate/table changes.

The 1024px breakpoint leaves each column at least half the available management content width; the existing mobile link style and focus order remain intact. The demo was restarted against the same isolated database to inspect the desktop layout.

## Plan Change Log

## Review Triage Log

- Medium, open: required rendered critique/polish and narrow-width inspection remain incomplete; the browser screenshot facility previously reported no visible window. Member role/link rendering passed the SQL-backed authentication tests, but an actual member viewport and touch device were not inspected.
- Low, patched: removed obsolete adjacent-section margin rules and updated the layout regression to check both source and generated CSS.

## Verification

- Build Release solution in an isolated artifacts directory; run CSS build and JavaScript tests.
- Run focused `AuthenticationFlowTests` against disposable LocalDB if available.
- Inspect management Home at desktop and narrow widths and confirm member Home remains unchanged; record visual or device limitations.
- Results: isolated Release build and CSS build passed; 58 JavaScript tests and 53 SQL-backed authentication/role integration tests passed. Desktop browser measurements showed two equal-width groups on the same row with no document overflow. Home detector reported no findings. Real mobile/zoom and screenshot visual inspection remain unverified; required rendered critique and polish remain open.
