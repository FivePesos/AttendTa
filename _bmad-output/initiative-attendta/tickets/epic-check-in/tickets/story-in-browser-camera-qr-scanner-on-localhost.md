---
id: 4
type: story
title: "In-browser camera QR scanner on localhost"
parent: epic-check-in
covers: [R4]
after: [2, 3, 11]
hitl: true
risk: high
tracker_id: "47"
remote: "https://github.com/rainyDayssss/AttendTa/issues/47"
tracker_status: backlog
---

# In-browser camera QR scanner on localhost

## Description

Adds the Management door scanner page where an Officer points the laptop camera at a Confirmed Member's Pass; the scanner page also runs on the hotspot HTTPS origin in epic-release-acceptance entry 2 (no hard-coded localhost), while this entry verifies it on localhost. A browser QR decoder library chosen in this entry is served from wwwroot, with no runtime CDN (AD-14). The page reads the laptop camera and posts each decoded token to the entry 3 endpoint with the RequestVerificationToken header read from the layout meta tag (AD-8), then shows the JSON outcome and message (AD-9). Scan payloads are not logged (Consistency: Diagnostics) (delivering the camera scan interaction of R4).

## Requirements

Verify: An Officer opens the scanner on localhost on a laptop webcam, points it at a Confirmed Member's QR Pass, and sees name and student number on success; presenting the same pass again shows the original check-in time; the browser network log shows no request to any host other than localhost.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md
- docs/DESIGN.md
- _bmad-output/initiative-attendta/delivery-queue.md
- _bmad-output/initiative-attendta/finale-test-cases.md

## Delivery (2026-11-06)

- Frontend-facing integration: the scanner screen and its separate camera adapter are already delivered by 4.11. This issue wires the real camera adapter to the engine (4.3), does not rebuild the screen, and does not gate the manual adapter 4.5.
- Use impeccable for implementation, then a rendered critique and polish of the wired scanner before review.
- Development check: build/CSS plus one real scan, one duplicate and the camera-denied fallback; existing CI unchanged. The full matrix and the phone/hotspot run are 8.5 (#78) and 8.6–8.8; the full suite runs once at 8.4 (#77).
- Final rules unchanged: no hard-coded localhost origin, assets served locally with no runtime CDN, antiforgery header read from the layout meta tag, and no scan payload in logs.
