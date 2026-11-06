---
id: 13
type: story
title: "Member centered record editor using the shared dialog"
parent: epic-master-data
covers: [R2, R6]
after: [12]
hitl: false
risk: medium
tracker_id: "147"
remote: "https://github.com/rainyDayssss/AttendTa/issues/147"
tracker_status: backlog
---

# Member centered record editor using the shared dialog

## Description

Apply 2.12's reusable record dialog to Member edit using the current real Member service. Preserve read-only email, academic fields, membership type, student-number correction reason and errors; do not add standalone creation or implement membership-status mutations. Reserve the agreed access/status action seam for 5.11 without editing its partial. Use impeccable for implementation, critique and polish.

## Requirements

Verify: An Officer edits a Member in a centered dialog, sees a required correction reason and inline server errors, cancels without saving and returns focus to the original row. Phone-width content and actions remain reachable.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-master-data/epic-master-data.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Frontend only: use impeccable for implementation, rendered critique and polish. Keep development verification to build/CSS and focused save/error/keyboard checks; defer broad automation to the finale.
