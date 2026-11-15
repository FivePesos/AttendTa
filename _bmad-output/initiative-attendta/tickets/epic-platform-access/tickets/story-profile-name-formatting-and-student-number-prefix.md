---
id: 17
type: story
title: "Profile name formatting and student-number prefix"
parent: epic-platform-access
covers: [R1, R5]
after: [15]
hitl: false
risk: low
tracker_id: "244"
remote: "https://github.com/rainyDayssss/AttendTa/issues/244"
---

# Profile name formatting and student-number prefix

## Description

Two profile-field follow-ups raised while the owner exercised the 1.15 signup form.

First, a student number must begin with `2`, so the shared pattern becomes `^2[0-9]{7}$` and the field message says so. The eight-digit rule, the string type and the uniqueness checks stay as 1.15 left them.

Second, a name typed in capitals must be stored the way it should read: `SETH PINCA` becomes `Seth Pinca` and `JOHN PAUL A. SINGCO` becomes `John Paul A. Singco`. Normalise on save, the way the section is normalised today, so every screen is right without a formatter and no migration is needed. The rule: split on spaces and hyphens, collapse repeated spaces, upper-case a token that is a single letter with or without a period, capitalise the first letter of every other token and lower-case the rest, and rejoin with the original separators. Small connector words such as `de`, `dela` and `van` are capitalised like any other token, so `MARIA DELA CRUZ` becomes `Maria Dela Cruz`.

## Requirements

Verify: a student number starting with `0`, `1` or `3` is refused on the student-number field with the input retained, and `2`-prefixed numbers are accepted. A member saved with an all-capitals name is stored and displayed as title case in the members list, the account-request queue, the profile page and the member editor; `John Paul A. Singco`, `Maria Dela Cruz` and `Anne-Marie Lopez` keep their initials, particles and hyphens; a name already correctly cased is unchanged. Historical registration and certificate name snapshots are untouched.

## References

- parent - _bmad-output/initiative-attendta/tickets/epic-platform-access/epic-platform-access.md
- 1.15 - _bmad-output/initiative-attendta/tickets/epic-platform-access/tickets/story-shared-profile-field-validation-across-signup-and-member-edi.md
- docs/handoffs/ui-ux-backend-alignment-handoff.md

## Notes

- Raised 2026-11-15 in the 1.15 walkthrough, after the owner submitted the signup form with sample values.
- Normalising on save is the owner's choice: the stored value becomes title case, so no display formatter is needed and no backfill is required. Rows written before this change keep whatever they hold.
- Accepted trade-off: an intentional internal capital such as `McDonald` becomes `Mcdonald`. A dictionary of exceptions is deliberately out of scope.
- Historical snapshots (`registrations.course_snapshot`, `section_snapshot`, `certificates.recipient_name_snapshot`) must never be reformatted; they record what was true when the row was written.
- The mobile rule changed separately in 1.15 to require a `09` prefix; this entry does not touch it.
- If the two rules are wanted separately, split the student-number prefix into its own entry; they share only `ProfileOptions`.
