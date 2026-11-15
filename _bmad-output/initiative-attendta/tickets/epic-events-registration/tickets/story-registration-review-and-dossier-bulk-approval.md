---
id: 20
type: story
title: "Registration review and dossier bulk approval"
parent: epic-events-registration
covers: [R16, R33, R37]
after: [7, 1.16]
hitl: false
risk: high
tracker_id: "240"
remote: "https://github.com/rainyDayssss/AttendTa/issues/240"
---

# Registration review and dossier bulk approval

## Description

Add bounded bulk approval of pending Registrations on the review screen and the event dossier roster, reusing RegistrationReviewService.ApproveAsync. Accept a bounded, deduplicated list of ids and the selected event context; re-read status, actor, account and membership activity and event facts at execution time; keep the existing transaction, locking, capacity, cutoff, waitlist-priority and audit rules; return a per-record result naming the actual outcome (confirmed, waitlisted or refused with the reason). Selection is per page, shows a count, clears on page, event, search or filter change, and never implies a bulk command that does not exist. Individual approval stays available.

## Requirements

Verify: Approving a mixed selection reports each participant's actual outcome and audits one entry per successful change; a stale, already-decided, duplicate, over-capacity, past-cutoff or wrong-event id is refused with its reason and changes nothing; a repeat submit creates no duplicate effect; a selection does not survive a page or filter change; individual approval still works.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-events-registration/epic-events-registration.md
- docs/handoffs/ui-ux-backend-alignment-handoff.md
- docs/ui-ux-implementation-plan.md

## Notes

- Phase 4 of the UI plan. Partial success with per-record results is the recommended batch semantic; never wrap the existing transaction-owning command in an unreviewed nested transaction.
- Do not add checkboxes that imply a working bulk command before this entry ships.
- Restored 2026-11-15 after the phase-entry withdrawal; the project owner kept bulk approval in scope.
