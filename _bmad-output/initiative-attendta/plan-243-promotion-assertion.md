---
title: 'Align promotion test with registration date markup'
type: 'chore'
ticket: ''
created: '2026-11-18'
status: 'built'
baseline_revision: '81f502998437a5de9b5cc802c0e113b0d475be34'
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

**Problem:** A promotion integration test expects the pre-refinement CSS class and fails before exercising promotion.

**Approach:** Align the test with each rendered surface while preserving checks of date, time, filtering, promotion and audit behavior. Do not change application behavior or unrelated work.

</frozen-after-approval>

## Implementation Notes

Focused oneshot: `RegistrationPromotionTests.FilteredRosterNamesAndPromotesTheEarliestEligibleCandidateFromTheFullQueue` covers both dossier and Registration Review; only the latter received `cell-date`. Keep the existing date/time assertions with appropriate class per surface. Build in isolated output and run the test class; request independent review as approved by the user.

The first build targeted the running demo's output and failed due to locked DLLs; a separate output built successfully without stopping the demo.

## Plan Change Log

## Review Triage Log

- Verification-record finding: patched by recording the isolated Release build and focused results below.
- Historical sweep evidence finding: patched by appending a follow-up; the original failed run remains accurately documented.
- Dossier alignment: deferred, a pre-existing separate UI surface outside the test-only intent; documented in the sweep evidence. The assertion intentionally reflects the current surface until separately changed.
- Unrelated research: preserved untracked and excluded from the commit; its inclusion in the review diff is not a change made by this task.

## Verification

- Release build and focused SQL-backed `RegistrationPromotionTests`, including both theory cases.
- Full integration rerun for final acceptance if practical.
- Isolated Release solution build passed with zero warnings/errors. The SQL-backed `RegistrationPromotionTests` class passed 24/24, including both filtered-roster theory cases.
