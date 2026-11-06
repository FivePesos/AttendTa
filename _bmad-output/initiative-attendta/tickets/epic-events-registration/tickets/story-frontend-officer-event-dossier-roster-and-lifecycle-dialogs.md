---
id: 16
type: story
title: "Frontend officer event dossier roster and lifecycle dialogs"
parent: epic-events-registration
covers: [R1, R2, R3, R4, R5, R8, R9, R14, R15]
after: [1.13]
hitl: false
risk: medium
tracker_id: "141"
remote: "https://github.com/rainyDayssss/AttendTa/issues/141"
tracker_status: backlog
---

# Frontend officer event dossier roster and lifecycle dialogs

## Description

Complete the officer event-list/create/detail composition and registration roster using the ready event, review, promotion and cancellation services. Replace the rough Event ID filter workflow with contextual event navigation. Present the completion and check-in-window controls through the 1.13 prototype contract until 3.12/4.2 are real; never simulate a successful real write. Own Event/RegistrationReview presentation and scoped assets, not My Events. Use impeccable for implementation, critique and polish against DESIGN screens 8–10.

## Requirements

Verify: An Officer navigates Events to a dossier with metadata, capacity and status-aware roster/actions, uses ready real review/cancel actions, and can inspect completion/open-paused prototype states with a clear fixture notice. Required reason, validation and blocked-action states remain usable on desktop and phone width.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-events-registration/epic-events-registration.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Frontend only: load impeccable for implementation, critique rendered results and polish before review.
- Development checks: build/CSS and one primary/blocked/manual interaction; no new exhaustive lifecycle test suite. Missing completion/toggle behavior remains accountable to 3.12/4.2.
