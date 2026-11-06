---
id: 18
type: story
title: "Frontend member event feed and detail visual alignment"
parent: epic-events-registration
covers: [R6, R7, R10, R11, R15]
after: [1.13, 10]
hitl: false
risk: low
tracker_id: "142"
remote: "https://github.com/rainyDayssss/AttendTa/issues/142"
tracker_status: backlog
---

# Frontend member event feed and detail visual alignment

## Description

Align the existing real member feed/detail screens to DESIGN screens 11–12: cover hierarchy, metadata, registration-status panel, truthful capacity/deadline messages and cancellation dialog. Preserve the ready registration/cancellation services and RegistrationStatusDisplay shipped by #133; no backend rule rewrite. Own only MemberEvents presentation and scoped assets, not the My Events host. Use impeccable for implementation, critique and polish.

## Requirements

Verify: A Member browses events, opens a readable responsive detail page and sees consistent Pending/Confirmed/Waitlisted/Cancelled/cutoff states, with working existing registration/cancellation behavior and no unrelated mock replacing a ready real source.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-events-registration/epic-events-registration.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Frontend only: use impeccable for implementation and critique, then polish. Build/CSS and a short real-flow smoke; reuse existing boundary tests rather than rewriting them.
