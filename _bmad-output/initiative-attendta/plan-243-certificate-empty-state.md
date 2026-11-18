---
title: 'Clarify filtered certificate empty states'
type: 'bugfix'
ticket: ''
created: '2026-11-18'
status: 'built'
baseline_revision: '64b594be764c2d400f0bef1c25adc2bf25fb59ba'
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

**Problem:** A certificate search or filter with no matches incorrectly says no issued certificates or reviews exist.

**Approach:** Show a clear no-match message and recovery guidance for filtered empty results, preserving true-empty and no-completed-event messages, event/tab context, filters and all issuance behavior. The user confirmed certificate ownership and approved the wording fix. Preserve existing uncommitted work.

</frozen-after-approval>

## Implementation Notes

Focused oneshot. In `ASI.Basecode.WebApp/Views/CertificateLedger/Index.cshtml`, use the same normalized search and filter options as the ledger and prioritize no completed events. Add rendered SQL-backed coverage to `tests/ASI.Basecode.IntegrationTests/CertificateLedgerFlowTests.cs` for issued search, eligibility search/filter, true-empty and no-completed-event states. Do not change commands, persistence, ownership, or redirect semantics.

The fixture already had an issued certificate, so true-empty verification uses a separate empty completed event. The first test run exposed that fixture assumption; after correction the focused suite passed.

## Plan Change Log

## Review Triage Log

- Medium, patched: Issued Registry has only Search, so its no-match guidance no longer refers to filters.
- Medium, patched: shared result-count helper ignores `filter=all` and recognizes unrelated keys; use the ledger's normalized read options to identify criteria actually applied by the ledger. Result-count wording for a crafted `filter=all` URL remains a pre-existing separate issue.
- Low, patched: added selected-event, active-tab and Reset assertions.
- Low, patched: appended the owner-approved resolution to the sweep evidence; earlier entry remains historical.

## Verification

- Release solution build, JavaScript suite, and focused certificate ledger integration tests pass.
- Verify selected event, tab and Reset remain available; document browser/device limitations.
- Release build, 58 JavaScript tests, and the focused SQL-backed ledger flow passed, including actual desktop/phone PDF-preview paths in the existing test. The initial focused run failed only because its new true-empty assertion used a fixture that already had an issued record; a separate empty event corrected the test setup.
- After quick-review patches, Release rebuild and the focused SQL-backed ledger flow passed again; the new assertions confirm event/tab context and legacy filter behavior.
