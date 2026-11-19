---
title: 'Place check-in invalidation reason under outcome'
type: 'bugfix'
ticket: ''
created: '2026-11-19'
status: 'built'
baseline_revision: '1c9ae073f1db616970da5eacb25b79fbbe03c0d7'
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

**Problem:** The check-in invalidation reason appears without a label under Action, where it looks misplaced and can stretch the action column.

**Approach:** Show the reason, clearly labeled, with the Invalidated outcome and leave the action column without a misleading reason. Preserve existing behavior and the complete reason.

</frozen-after-approval>

## Implementation Notes

Small Razor presentation fix; retain server-side eligibility and mobile labels. Check long reasons wrap without clipping, and keep certificate-issued and active action states intact.

Moved the labelled reason into Outcome; the invalidated Action cell is empty. Build and all 67 JavaScript tests passed. Browser rendering for this change has not been verified.

## Plan Change Log

## Review Triage Log

## Verification

- `npm run test:js` in `ASI.Basecode.WebApp` — pass.
- `dotnet build ASI.Basecode.sln --configuration Release` — pass.
- Inspect an invalidated row and a long reason at desktop and mobile widths when a browser is available.
