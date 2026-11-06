---
id: 13
type: story
title: "Member self profile: bring screen 4 up to DESIGN §6"
parent: epic-check-in
covers: [R1]
after: [10]
hitl: false
risk: low
tracker_id: "129"
remote: "https://github.com/rainyDayssss/AttendTa/issues/129"
tracker_status: backlog
---

# Member self profile: bring screen 4 up to DESIGN §6

## Description

Finish existing GitHub #129 only: #132/#134 already delivered the profile title, grouped read-only panels, badge, dark identity strip, amber notice and inactive-membership explanation. Add the compact bottom action to the usable My Passes route from 4.10, preserving all shipped work and active-account versus membership semantics. This residual passes-link story belongs here as a consumer of the pass screen; it creates no new profile editor. Use impeccable for implementation, critique and polish.

## Requirements

Verify: A Member follows the compact profile action to My Passes without a dead link; profile identity/notices remain correct and read-only. Inactive membership does not imply a blocked login or loss of issued-certificate access. Close #129 only after this rendered flow works.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Existing issue #129 is reused and its shipped #132/#134 acceptance is preserved. Frontend only: impeccable implementation, critique and polish; a short rendered link/keyboard check replaces new broad automation.
