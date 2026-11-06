---
id: 11
type: story
title: "Frontend door scanner manual lookup and attendance dialogs"
parent: epic-check-in
covers: [R3, R4, R5, R6, R8, R9]
after: [1.13]
hitl: false
risk: medium
tracker_id: "138"
remote: "https://github.com/rainyDayssss/AttendTa/issues/138"
tracker_status: backlog
---

# Frontend door scanner manual lookup and attendance dialogs

## Description

Build DESIGN screen 14 using the 1.13 shell/contracts: event selection, open/paused control, camera surface, persistent success/duplicate/invalid/unavailable outcomes, manual name/student-number lookup with explicit Check in, and invalidation reason/confirmation. Prototype outcomes use isolated Development-only fixtures and never record real attendance. Prepare separate camera/manual adapters so 4.4 and 4.5 can wire them without racing on one script; 4.2/4.3/4.6 supply real commands later. Serve all assets locally. Use impeccable for implementation, critique and polish.

## Requirements

Verify: An Officer can inspect camera-denied, closed, successful, duplicate, invalid and manual-lookup scenarios on desktop and 390px width. Search/view alone causes no write, fixture outcomes are visibly simulations, and controls work by keyboard with readable persistent feedback.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Frontend only: load impeccable for implementation, critique the rendered scanner and polish before review.
- Development checks: build/CSS and focused keyboard/result-state smoke; real camera, refusal/concurrency and hotspot tests belong to 4.4 and the finale, not a large mock-only test suite.
