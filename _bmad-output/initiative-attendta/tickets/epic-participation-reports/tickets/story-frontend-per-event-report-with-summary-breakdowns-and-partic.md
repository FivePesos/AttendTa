---
id: 4
type: story
title: "Frontend per-event report with summary breakdowns and participant table"
parent: epic-participation-reports
covers: [R1, R2]
after: [1.13]
hitl: false
risk: medium
tracker_id: "140"
remote: "https://github.com/rainyDayssss/AttendTa/issues/140"
tracker_status: backlog
---

# Frontend per-event report with summary breakdowns and participant table

## Description

Build DESIGN screen 18 from 1.13 contracts: event selector, status/Confirmed/Valid attendee summary, completed-only no-shows, course/year breakdowns and a paged detail table with accessible values. Use one coherent explicitly labelled fixture dataset until 7.1–7.3 supply real queries; do not combine real counts with fake attendance or invent global metrics/exports. Include empty, zero-denominator N/A, invalidated, cancelled and long-content scenarios. Own the report surface/scoped assets. Use impeccable for implementation, critique and polish.

## Requirements

Verify: An Officer can select a sample event and inspect populated/empty/zero-denominator/completed/cancelled report states. Summary totals do not change with detail paging, charts have equivalent values, and the layout works on desktop, keyboard and phone width without page overflow.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-participation-reports/epic-participation-reports.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Frontend only: use impeccable for implementation, rendered critique and polish. Build/CSS and focused selector/pager/keyboard smoke; formula automation and full regression belong to the finale.
