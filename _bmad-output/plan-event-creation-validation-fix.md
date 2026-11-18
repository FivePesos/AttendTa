---
title: 'Fix event creation validation'
type: 'bugfix'
ticket: ''
created: '2026-11-19'
status: 'built'
baseline_revision: '9ce223a05787e7231ea28046c81f604726c91008'
route: 'oneshot'
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

**Problem:** Event creation duplicates field errors and permits dates before today in Manila.

**Approach:** Show field-specific errors only beside their controls, restrict creation date pickers to today or later in Manila and enforce the same rule on the server, allowing earlier times today while keeping the deadline-before-start rule.

</frozen-after-approval>

## Implementation Notes

The changes are limited to the existing event editor and its creation guard, with focused regression tests. Existing edits remain usable. Picker minimums refresh on focus using the browser clock; server validation remains authoritative.

## Plan Change Log

## Review Triage Log

- Hidden published fields: unconfirmed; the controller restores and clears validation errors for locked fields before rendering. Other hidden string fields have no ordinary binding validation.
- Picker minimum crossing Manila midnight: patched with focus-time refresh and JavaScript coverage.
- UI critique: clarified Manila-date hints and marked invalid cover inputs for focus. Rendered browser checks were not performed.
## Verification

- `dotnet build ASI.Basecode.sln --configuration Release`
- Run focused service and event integration tests where dependencies are available.
- Run relevant JavaScript tests and inspect the rendered editor when feasible.

Release build and CSS build passed; 415 service tests, 59 JavaScript tests, and the focused SQL-backed creation and event-draft tests passed. The complete SQL integration suite timed out after ten minutes; no failures appeared in its partial log.
