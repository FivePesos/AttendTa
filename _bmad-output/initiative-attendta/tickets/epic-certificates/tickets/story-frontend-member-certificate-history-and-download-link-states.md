---
id: 13
type: story
title: "Frontend member certificate history and download-link states"
parent: epic-certificates
covers: [R2, R6]
after: [3.17]
hitl: false
risk: low
tracker_id: "146"
remote: "https://github.com/rainyDayssss/AttendTa/issues/146"
tracker_status: backlog
---

# Frontend member certificate history and download-link states

## Description

Fill 3.17's _CertificateLinkPartial/history contract with the member's pending/eligible/declined/issued/unavailable certificate presentation and an issued-only View/Download affordance. Use 1.13 isolated fixtures until 6.9 supplies the real owner query and 6.8 streams retained files; mark sample links visibly. Own only the certificate partial/scoped assets, not the My Events controller, pass or attendance partial. Use impeccable for implementation, critique and polish.

## Requirements

Verify: A Member switches to Certificates and distinguishes review versus issued states, with clear empty/unavailable feedback and no misleading official download for an unissued record. Keyboard, phone-width and independent history navigation work.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-certificates/epic-certificates.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md

## Notes

- Frontend only: impeccable implementation, rendered critique and polish; build/CSS and a short tab/link/keyboard smoke. Real file authorization and retention cases run at the finale.
