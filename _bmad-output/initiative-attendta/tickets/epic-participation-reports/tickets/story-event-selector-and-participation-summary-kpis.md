---
id: 1
type: story
title: "Event selector and participation summary KPIs"
parent: epic-participation-reports
covers: [R1]
after: [4, 1.1, 1.9, 3.7]
hitl: false
risk: medium
tracker_id: "71"
remote: "https://github.com/rainyDayssss/AttendTa/issues/71"
tracker_status: backlog
---

# Event selector and participation summary KPIs

## Description

Wire 7.4's existing report contract to coherent Management-authorized real read queries: selected-event registration status counts, Confirmed count and Valid attendees excluding invalidated attendance. Calculate the canonical ParticipationFacts rate as Valid attendees divided by Confirmed, null at zero; round only for display. Following the existing authoritative SPEC/docs/DESIGN.md presentation scope, show final attendance rate only for completed non-cancelled events, otherwise N/A/not final. No change to the underlying canonical fact or new PM override; no second report screen.

## Requirements

Verify: An Officer selects a real completed event and sees status counts, Confirmed/Valid attendee totals and the percentage rate match the full database dataset, with N/A at zero Confirmed. Published/cancelled events do not present a final rate; changing detail pages does not change the totals.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-participation-reports/epic-participation-reports.md
- _bmad-output/initiative-attendta/delivery-queue.md
- _bmad-output/initiative-attendta/finale-test-cases.md

## Delivery (2026-11-06)

- Builds the real report reads after the 7.4 frontend contract; it does not construct a second report screen or change presentation, and mock figures do not close the functional issue.
- Read queries use canonical mapped attendance and ParticipationFacts and can be built without waiting for the entire scanner/invalidation UI; 8.11 proves the real attendance data end to end.
- Development check: build plus a focused selector/summary smoke; existing CI unchanged. Missing important test authoring is 8.5 (#78), combined final execution 8.4 (#77), manual 8.6–8.8.
- Backend-only: no Impeccable requirement; frontend design lives only in the separate slices 6.12/6.13/7.4 and finale 8.12.
- SPEC v2.1 precedence over the unresolved R30 rate wording is preserved; do not invent a PM override. The no-show/rate rule stays final acceptance even while test authoring is deferred.
