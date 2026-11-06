---
id: 11
type: story
title: "Frontend account and membership access dialogs"
parent: epic-administration
covers: [R1, R2, R3, R4, R6]
after: [3, 1.13, 2.13]
hitl: false
risk: medium
tracker_id: "149"
remote: "https://github.com/rainyDayssss/AttendTa/issues/149"
tracker_status: backlog
---

# Frontend account and membership access dialogs

## Description

Complete account/member access presentation using the real #54 impact preview and existing account/role lists. Reuse 2.12/2.13's centered-editor action seam; clearly distinguish account access from membership eligibility. Provide reason, consequence, blocked last-admin and reactivation states through the 1.13 prototype mechanism until 5.4–5.7 are real. Missing mutations are conspicuous simulations, never real changes or session workarounds. Own administration/status partials, not the Member editor or shared dialog primitive. Use impeccable for implementation, critique and polish.

## Requirements

Verify: An authorized operator previews upcoming affected registrations, sees the required-reason/last-admin explanations and can inspect deactivate/reactivate states without any fake real-record mutation. Member and account restrictions remain distinct on desktop, keyboard and phone layouts.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-administration/epic-administration.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Frontend only: use impeccable for implementation, rendered critique and polish. Build/CSS plus reason/preview/keyboard smoke, not a new broad admin suite.
- The backend account and membership mutation tickets serialize their shared administration source changes; this is a code-boundary dependency, not a permanent human/module assignment.
