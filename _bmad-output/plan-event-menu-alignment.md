---
title: 'Align event row menu actions'
type: 'bugfix'
ticket: ''
created: '2026-11-16'
status: 'built'
baseline_revision: '1bd428703acf5b4e87b413c09a874b018335e1c9'
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

**Problem:** Show details is centered while other actions are left-aligned in the Events row menu.

**Approach:** Center all row-menu action text to match Show details without changing action behavior or keyboard access.

</frozen-after-approval>

## Implementation Notes

Oneshot: shared row-menu actions are flex containers; use centered alignment consistently for links and buttons. Rebuild the committed CSS and check the live Events menu.

User selected centered rather than left alignment. Changed the shared row-menu rule and rebuilt the shipped CSS; all 46 JavaScript tests passed. The shared rule also applies to other row menus. Browser inspection was interrupted while the user was navigating.

## Plan Change Log

## Review Triage Log

Quick review found no code defects. Live visual inspection remains unverified; the user's active browser session was not interrupted.

## Verification

**Commands:**
- `npm run css:build` and `npm run test:js` in `ASI.Basecode.WebApp`.
- Inspect a live Events row menu and verify action alignment without submitting actions.
