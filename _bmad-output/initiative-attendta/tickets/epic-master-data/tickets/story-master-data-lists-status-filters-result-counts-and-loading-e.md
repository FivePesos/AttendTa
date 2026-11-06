---
id: 10
type: story
title: "Master Data lists: status filters, result counts, and loading/error states"
parent: epic-master-data
covers: [R7]
after: [9, 1.13]
hitl: false
risk: low
tracker_id: "127"
remote: "https://github.com/rainyDayssss/AttendTa/issues/127"
tracker_status: backlog
---

# Master Data lists: status filters, result counts, and loading/error states

## Description

Finish existing GitHub #127 only: result counts shipped in #132 and status filters in #134. Preserve those real integrations and finish meaningful submitting/loading feedback and shell-preserving error/retry states on Member, Event Type and Venue lists. Use the 1.13 shared conventions and impeccable for implementation, critique and polish; do not rebuild the filters or require an AJAX conversion merely to display a loading state.

## Requirements

Verify: An Officer sees the shipped filters and counts still working across pagination; a delayed or failed list interaction has truthful feedback and a usable retry inside the shell, and no records versus no matches remain distinct. Check keyboard and phone-width behavior manually.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-master-data/epic-master-data.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Existing issue #127 is reused; #132/#134 shipped work remains historical acceptance evidence.
- Frontend only: use impeccable for implementation and rendered critique, then polish before review. Build/CSS/manual smoke only; full regression and acceptance belong to the finale.
