---
title: 'Fix PR 284 activity log integration test'
type: 'bugfix'
ticket: ''
created: '2026-11-20'
status: 'built'
baseline_revision: '7cb94de5fda61157968ee38f855cac04b5323e22'
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

**Problem:** PR 284 CI fails because its activity log integration test expects an actor type label that the updated viewer no longer displays.

**Approach:** Align the integration assertions with the intended actor presentation while preserving coverage for user and system entries, paging, ordering and access control.

</frozen-after-approval>

## Implementation Notes

One-shot: the correction is confined to an existing integration test.
Removed obsolete actor-type assertions and scoped actor-name checks to the matching rows. Release build and targeted SQL integration test passed.
The 30 activity-log reader service tests also passed. Quick review found no issues.

## Plan Change Log

## Review Triage Log

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — build succeeds.
- Run `ActivityLogViewerTests` against disposable LocalDB using the integration test executable — the test passes.
