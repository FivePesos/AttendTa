---
id: 14
type: story
title: "Venue centered record editor with photo and validation feedback"
parent: epic-master-data
covers: [R4, R5, R6]
after: [12]
hitl: false
risk: medium
tracker_id: "148"
remote: "https://github.com/rainyDayssss/AttendTa/issues/148"
tracker_status: backlog
---

# Venue centered record editor with photo and validation feedback

## Description

Apply 2.12's reusable record dialog to Venue create/edit using the existing real service and private photo store. Keep the current photo, upload preview, required/format guidance, capacity guard errors and separate status action; retain entered values on failed save. Own only Venue presentation files and its scoped assets. Use impeccable for implementation, critique and polish.

## Requirements

Verify: An Officer creates/edits a Venue inside the centered dialog, sees its existing or selected photo, corrects an invalid capacity/upload without a blank panel and can cancel by keyboard. The dialog and footer fit phone width.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-master-data/epic-master-data.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Frontend only: use impeccable for implementation, rendered critique and polish. Build/CSS plus focused save/error/keyboard smoke; reuse existing file-store tests, no new broad upload suite.
