---
id: 1
type: story
title: "On-demand QR Pass PNG rendering and download"
parent: epic-check-in
covers: [R1, R2]
after: [3.7, 3.13, 10]
hitl: true
risk: medium
tracker_id: "44"
remote: "https://github.com/rainyDayssss/AttendTa/issues/44"
tracker_status: backlog
---

# On-demand QR Pass PNG rendering and download

## Description

Adds server-side QR PNG rendering in ASI.Basecode.WebApp/Rendering with no System.Drawing, chooses and records the server QR library (net10.0-compatible, referenced from WebApp only), and adds a download action that checks ownership in Services and streams the PNG generated in memory, encoding only the 64-character token. The download control renders through e4's _PassPartial inside e3's My Events; e4 adds no action to MyEventsController (delivering R2 and the pass presentation part of R1).

## Requirements

Verify: A logged-in Member opens My Events for a Confirmed Registration, clicks Download PNG, and receives a PNG that decodes to their 64-character lowercase hex token; downloading another Member's pass, an unknown registration id, or a cancelled Registration returns the same 403 Forbidden response and message.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md
- _bmad-output/initiative-attendta/delivery-queue.md
- _bmad-output/initiative-attendta/finale-test-cases.md

## Delivery (2026-11-06)

- Frontend already delivered by 4.10 (pass cards, QR quiet zone and download states). This issue stays open for the real on-demand PNG renderer and the ownership-checked download; the renderer and ownership rules adapt to 4.10's partial and add no second pass UI or credential format.
- Development check: build plus one focused download/ownership smoke (own pass renders, another member's pass returns the same 403); existing CI unchanged. Broad automation goes to 8.5 (#78), the full suite runs once at 8.4 (#77), final manual acceptance is 8.6–8.8.
- Final rules unchanged: no System.Drawing, the PNG encodes only the 64-character token, and unknown, cancelled and foreign registrations share one indistinguishable refusal.
