---
id: 14
type: story
title: "Certificate eligibility bulk approval"
parent: epic-certificates
covers: [R27, R33, R37]
after: [6, 12, 1.16]
hitl: false
risk: high
tracker_id: "241"
remote: "https://github.com/rainyDayssss/AttendTa/issues/241"
---

# Certificate eligibility bulk approval

## Description

Add bounded bulk approval of pending Certificate Reviews on the eligibility screen, reusing CertificateLedgerService.DecideAsync. Approval marks eligibility only and issues no PDF; issuance and declines stay individual. Re-read status, Valid Attendance and completion at execution time, serialize against invalidation, keep one audit entry per successful change, and return a per-record result. Selection is per page, clears on page, event, search or filter change, and individual approval stays available.

## Requirements

Verify: Approving a mixed selection marks each eligible review and reports each refusal with its reason; no PDF is generated; an issued or already-decided review is refused; a repeat submit creates no duplicate decision; declines and issuance remain individual and unchanged.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-certificates/epic-certificates.md
- docs/handoffs/ui-ux-backend-alignment-handoff.md
- docs/ui-ux-implementation-plan.md

## Notes

- Phase 4 of the UI plan. No bulk issuance: eligibility approval is a separate step from individual PDF issuance.
- Restored 2026-11-15 after the phase-entry withdrawal; the project owner kept bulk approval in scope.
