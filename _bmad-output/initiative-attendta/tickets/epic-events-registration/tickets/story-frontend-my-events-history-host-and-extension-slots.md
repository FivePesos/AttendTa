---
id: 17
type: story
title: "Frontend My Events history host and extension slots"
parent: epic-events-registration
covers: [R12, R15]
after: [1.13]
hitl: false
risk: medium
tracker_id: "137"
remote: "https://github.com/rainyDayssss/AttendTa/issues/137"
tracker_status: backlog
---

# Frontend My Events history host and extension slots

## Description

Build the member My Events/Participation History shell with registration, attendance and certificate tabs, accurate status presentation and separate pager state. Use 1.13's contracts and existing queries when available; otherwise use its explicit Development-only fixtures. Establish one MyEventsController/card host and named _PassPartial, attendance and _CertificateLinkPartial slots with empty handoff partials; 4.10/4.12/6.13 own their partials and data contracts without rewriting this host. 3.13 owns the real registrations query. Use impeccable for implementation, critique and polish.

## Requirements

Verify: A Member can navigate registrations/history, switch the three tabs and inspect populated/empty/cutoff scenarios without dead ends. The host slot map lets pass, attendance and certificate UI build independently; mock-backed content is conspicuous and does not claim real attendance or issuance.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-events-registration/epic-events-registration.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Frontend only: impeccable implementation, rendered critique and polish; build/CSS plus tab/pager/keyboard smoke only. Functional completion and ownership checks belong to 3.13 and finale tests.
