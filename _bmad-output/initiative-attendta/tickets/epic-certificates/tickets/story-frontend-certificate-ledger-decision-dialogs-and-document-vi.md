---
id: 12
type: story
title: "Frontend certificate ledger decision dialogs and document viewer"
parent: epic-certificates
covers: [R4, R5, R6, R7, R8]
after: [1.13, 2, 3]
hitl: false
risk: medium
tracker_id: "139"
remote: "https://github.com/rainyDayssss/AttendTa/issues/139"
tracker_status: backlog
---

# Frontend certificate ledger decision dialogs and document viewer

## Description

Build DESIGN screens 16–17 from the existing fixed template/renderer and 1.13 contracts: completed-event selector, Eligibility Review/Issued Registry tabs, approve/decline/issue dialogs with reason/consequence feedback, and a quiet PDF view/download surface. Use explicitly labelled fixture reviews/outcomes until 6.4–6.8 are wired; a sample document is never presented as an officially issued retained certificate. Do not reimplement the renderer, invent signatories, enable bulk issuance or claim success before commit. Own certificate presentation/scoped assets, not the member-history partial. Use impeccable for implementation, critique and polish.

## Requirements

Verify: An Officer can inspect pending/eligible/declined/issued/failure states and the landscape A4 sample viewer, with coherent actions and reason dialogs on desktop and phone width. Prototype decisions do not write real records; the existing configured template is reused.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-certificates/epic-certificates.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Frontend only: use impeccable for implementation, rendered critique and polish. Build/CSS and focused tabs/dialog/viewer smoke; do not build a large suite over temporary fixture data.
- Real review/issue/download acceptance remains in #64–#68; the signatory configured by #63/PR102 is reused, not a new blocker.
