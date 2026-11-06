---
id: 10
type: story
title: "Frontend member pass cards and QR download states"
parent: epic-check-in
covers: [R1, R2]
after: [3.17]
hitl: false
risk: low
tracker_id: "144"
remote: "https://github.com/rainyDayssss/AttendTa/issues/144"
tracker_status: backlog
---

# Frontend member pass cards and QR download states

## Description

Fill 3.17's _PassPartial with the DESIGN screen 13 pass-card treatment, active/past sections, event and holder identity, intact QR quiet zone, and available/unavailable download states. Use 1.13 fixtures only where the real 4.1 renderer is missing; sample QR images are visibly non-credentials and never accepted by real check-in. Own the pass partial/scoped assets, not MyEventsController or the host card. Use impeccable for implementation, critique and polish.

## Requirements

Verify: A Member reaches My Passes, sees readable one/two-column cards and full square QR placeholders, and can distinguish Confirmed versus cancelled/unconfirmed/unavailable states without a misleading usable pass. The real PNG/ownership behavior remains open in 4.1.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Frontend only: impeccable implementation, rendered critique and polish. Build/CSS plus phone/keyboard smoke only; do not implement a second credential format or an in-app print workflow.
