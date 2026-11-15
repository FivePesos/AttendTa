---
id: 16
type: story
title: "Membership request bulk approval"
parent: epic-platform-access
covers: [R2, R33, R37]
after: [7]
hitl: false
risk: high
tracker_id: "239"
remote: "https://github.com/rainyDayssss/AttendTa/issues/239"
---

# Membership request bulk approval

## Description

Build the shared bulk-selection surface once and use it as the membership-request tracer: eligible-pending-row checkboxes, a selected count, Select all on this page, Deselect all, a confirmation naming the selected people and the consequence, and a per-record result panel. Then wire it to bounded bulk approval of pending Account Requests, reusing AccountRequestService.ApproveAsync: accept a bounded, deduplicated list of ids, re-read status, actor and the email and student-number uniqueness at execution time, and keep the existing caller-owned transaction, member creation, password-hash move and audit. Selection clears on page, tab, search or filter change. Decline stays individual.

## Requirements

Verify: Approving a mixed selection creates the approved Members and reports each refusal with its reason; a stale, already-decided, duplicate, or now-taken email or student number is refused and changes nothing; a repeat submit creates no duplicate account; the selection does not survive a page, tab or filter change; decline remains individual and unchanged; the selection, confirmation and result components are the ones 3.20 and 6.14 reuse.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-platform-access/epic-platform-access.md
- docs/handoffs/ui-ux-backend-alignment-handoff.md
- docs/ui-ux-implementation-plan.md

## Notes

- Phase 4 of the UI plan, first of the three bulk surfaces. This entry owns the shared selection/confirmation/result components so 3.20 and 6.14 do not each invent one.
- Partial success with per-record results is the batch semantic; never wrap the existing transaction-owning command in an unreviewed nested transaction.
