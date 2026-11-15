---
id: 17
type: story
title: "Whole-app consistency sweep: one implementation, one vocabulary"
parent: epic-release-acceptance
covers: [R37]
after: [16]
hitl: true
risk: medium
tracker_id: "243"
remote: "https://github.com/rainyDayssss/AttendTa/issues/243"
---

# Whole-app consistency sweep: one implementation, one vocabulary

## Description

Walk the entire application as Administrator, Officer and Member — every screen, not only the four changed ones: sign-in and the membership request, the member feed and event detail, My Events and its tabs, the pass, check-in and the scanner, the event dossier and roster, registration review, master data, the certificate ledger and viewer, the participation report, accounts, the activity log and the profile. Judge each against DESIGN.md and PRODUCT.md for hierarchy, spacing, typography, control sizes, status vocabulary, empty and error states, keyboard operation, focus, 200% zoom and a 390px width. Then remove what is duplicated or inconsistent: a second pager, a second dialog guard, a second list query path, two spellings of one status, two spacings of one control. Prefer deleting a duplicate over abstracting it. Run the Impeccable critique and polish on what changes, and keep DESIGN.md and PRODUCT.md true to what ships. No new feature, no schema change, no rule change.

## Requirements

Verify: Every screen in the application is walked in all three roles and its findings are recorded; no second implementation of the pager, the dialog guard or the list query remains; the screens share one control, label and status vocabulary; empty, error, long-content, keyboard and 390px states are checked; critique findings are fixed or recorded with a reason; the build, the CSS build and all three suites pass.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-release-acceptance/epic-release-acceptance.md
- docs/DESIGN.md
- PRODUCT.md
- _bmad-output/initiative-attendta/plan-1-15-profile-validation.md

## Notes

- Replaces the closed 8.12 for this build: 8.12 ran on 2026-11-10, before the UI consistency work and the four feature tickets.
- Impeccable is required here for implementation, rendered critique and polish. This is the one whole-app pass, not a per-ticket review.
