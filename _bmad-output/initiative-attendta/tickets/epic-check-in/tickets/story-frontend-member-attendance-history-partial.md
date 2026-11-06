---
id: 12
type: story
title: "Frontend member attendance history partial"
parent: epic-check-in
covers: [R1, R7, R9]
after: [3.17]
hitl: false
risk: low
tracker_id: "145"
remote: "https://github.com/rainyDayssss/AttendTa/issues/145"
tracker_status: backlog
---

# Frontend member attendance history partial

## Description

Fill only the attendance-history slot agreed by 3.17, using a dedicated partial and independent page parameter. Show valid/invalidated records, event details and Asia/Manila times with no member check-in/correction actions. Use the agreed fixture provider until 4.7 supplies real owner-scoped attendance; do not edit the pass or certificate partials or host model unilaterally. Use impeccable for implementation, critique and polish.

## Requirements

Verify: A Member switches to Attendance History, sees valid/invalidated/empty examples and uses its pager independently of registrations at desktop and phone widths. Fixture data is labelled; no self-check-in or correction control exists.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Frontend only: use impeccable for implementation, rendered critique and polish. Build/CSS plus tab/pager/keyboard smoke; real ownership/attendance verification stays with 4.7 and finale cases.
