---
title: 'Consistent attendance reason column'
type: 'bugfix'
ticket: ''
created: '2026-11-20'
status: 'built'
baseline_revision: 'f00a23fc0e1d4708f7432e74f629a86d923c6386'
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

**Problem:** My Attendance puts invalidation reasons under State while comparable tables have a dedicated Reason column.

**Approach:** Add a Reason column and reuse the shared reason display, including its empty, short, and expandable long-text states. Retain attendance state, ownership, pagination, and mobile table behavior.

</frozen-after-approval>

## Implementation Notes

Oneshot: a small Razor view and corresponding tests; no data or business-rule change.
The view uses the shared reason partial only for invalidated attendance, with its dash for other rows. Updated layout and SQL-backed rendering tests.

## Plan Change Log

## Review Triage Log

- Quick review: required UI critique/polish was not evidenced. Source-based design and detector assessments were run; the detector was clean, but browser inspection and a complete visual polish remain unverified because the browser was disconnected. Source critique also noted pre-existing missing recourse guidance and empty-state messaging, outside this narrow display change.
## Verification

- Run JavaScript tests and a Release build; run relevant My Events integration tests if local SQL is available.
- Inspect desktop and mobile when a visible browser is available. Record any visual verification gap.
- Release integration project build passed; 71 JavaScript tests passed; the SQL-backed MyEventsAttendanceQueryTests passed. Screenshot-based desktop/mobile inspection unavailable because the browser window was not visible; design detector reported no findings.
