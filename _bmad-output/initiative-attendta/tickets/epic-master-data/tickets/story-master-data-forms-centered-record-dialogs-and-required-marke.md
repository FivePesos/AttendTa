---
id: 11
type: story
title: "Master Data forms: centered record dialogs and required markers"
parent: epic-master-data
covers: [R2, R3, R4, R5, R6]
after: [13, 14]
hitl: false
risk: low
tracker_id: "128"
remote: "https://github.com/rainyDayssss/AttendTa/issues/128"
tracker_status: backlog
---

# Master Data forms: centered record dialogs and required markers

## Description

Retain existing GitHub #128 as the acceptance umbrella for centered Member, Event Type and Venue editors. Required markers, fieldsets and constrained forms already shipped in #132; implementation is split into 2.12–2.14. Verify the complete original visual scope rather than claiming that the Event Type tracer alone finishes #128. Use impeccable to critique all three rendered editors and polish remaining UI findings.

## Requirements

Verify: All three editors satisfy docs/DESIGN.md: centered dialogs, keyboard focus entering/contained/restored, separate status versus Cancel/Save actions, retained validation and Venue photo preview. Close #128 only after 2.12–2.14 are complete and this combined walkthrough passes.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-master-data/epic-master-data.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Existing issue #128 is reused, not closed or weakened by the split. Frontend only: impeccable implementation for any fixes, critique and polish; manual acceptance rather than a duplicate automated suite.
