---
title: 'Align invalidated check-in action labels'
type: 'bugfix'
ticket: ''
created: '2026-11-19'
status: 'built'
baseline_revision: '95adec1c71981efb23e41d8d2e3c711959442ef1'
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

**Problem:** The manual lookup action cell repeats Invalidated while the recorded check-in action cell says No actions available.

**Approach:** Match the invalidated lookup action label to the recorded table; preserve existing checked-in and check-in action behavior and the status badge.

</frozen-after-approval>

## Implementation Notes

Small presentation-only change with a focused regression test.

Updated the lookup action label and tested the conditional. Solution build and focused JavaScript tests passed; quick review found no issues. Running UI not independently inspected.

## Plan Change Log

## Review Triage Log

## Verification

- `node --test tests/table-layout.test.js` from `ASI.Basecode.WebApp` — pass.
- Build the solution and refresh the local demo to confirm the displayed label.
