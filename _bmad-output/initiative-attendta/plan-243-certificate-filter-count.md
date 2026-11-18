---
title: 'Align certificate filtered result count'
type: 'bugfix'
ticket: ''
created: '2026-11-18'
status: 'built'
baseline_revision: '29c1adaa38bc99412b0b3b61f2f2be4bb9987c21'
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

**Problem:** A legacy certificate eligibility URL with `filter=all` filters results but the count omits “matching.”

**Approach:** Align count wording with the applied certificate filter, without changing filter results or other surfaces. Preserve retired issued-tab filters and all existing uncommitted work.

</frozen-after-approval>

## Implementation Notes

Focused oneshot: adjust only `ASI.Basecode.WebApp/Models/ListPagingLabels.cs`; add regression assertions in `ListPresentationTests.cs` and `CertificateLedgerFlowTests.cs` for eligibility `filter=all`, retired issued filters and existing status=all behavior. Do not change query semantics.

## Plan Change Log

## Review Triage Log

- Medium, patched: certificate result labels now derive criteria from the same normalized options as the ledger, so unrelated parameters and capitalized query keys are handled consistently.
- Low, patched: added end-to-end assertions for the retired issued filter, alongside the helper tests for other surfaces and `status=all`.

## Verification

- Build Release solution in isolated output and run focused ListPresentation and CertificateLedgerFlow SQL-backed tests.
- Confirm other pages and issued-tab retired filter behavior remain unchanged.
- Release build and 20 focused SQL-backed integration tests passed, including certificate `filter=all`, retired issued filters, and non-certificate `all` behavior.
- After review fixes, the Release build and all 20 focused tests passed again; the rendered flow also covers a capitalized filter key and an issued-tab retired filter.
