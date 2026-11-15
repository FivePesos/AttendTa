---
id: 15
type: story
title: "Shared profile field validation across signup and member edit"
parent: epic-platform-access
covers: [R1, R5, R37]
after: []
hitl: false
risk: medium
tracker_id: "238"
remote: "https://github.com/rainyDayssss/AttendTa/issues/238"
---

# Shared profile field validation across signup and member edit

## Description

Enforce one profile rule set on the Account Request form, the Member editor and their services: course BSCS or BSIT, year level 1 to 4, section A to F, student number exactly eight ASCII digits, mobile empty or exactly eleven ASCII digits, and a password of at least eight characters with a letter and a digit. Put the shared choices and patterns in ProfileOptions so signup, member editing and the list filters read one source. Trim surrounding whitespace, reject internal spaces and punctuation, never silently strip characters, and keep identifiers as strings so leading zeroes survive. Enforce the rules on the form through model annotations and normalize in the service; do not reject arbitrary values at the service boundary, because the concurrency tests post them on purpose. Keep field errors and safe input on failure while clearing passwords. Leave the database schema unchanged.

## Requirements

Verify: Signup and the Member editor accept BSCS/BSIT, years 1-4, sections A-F, an eight-digit student number, an empty or eleven-digit mobile and a valid password; it refuses year 0 and 5, a seven- or nine-digit student number, a dashed or spaced mobile, a section outside A-F, an unknown course and a weak password, with the field error on the right control and the input retained. A crafted POST and a direct service call are refused by the same rules, and the existing uniqueness checks still work.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-platform-access/epic-platform-access.md
- docs/handoffs/ui-ux-backend-alignment-handoff.md
- docs/ui-ux-implementation-plan.md

## Notes

- Legacy pending requests: approval runs member creation and therefore the new rules. Inventory affected records read-only before enforcing them and decide the correction path; never reset the database or coerce stored values.
- No schema change: the columns already hold the target values and the ERD stays as approved (handoff 3.5).
- Phase 6 of the UI plan; the shared constants land here and serve signup, member editing and the list filters.
