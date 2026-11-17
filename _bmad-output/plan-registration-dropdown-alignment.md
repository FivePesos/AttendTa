---
title: 'Registration review dropdown alignment'
type: 'bugfix'
ticket: ''
created: '2026-11-17'
status: 'built'
baseline_revision: '0c8ca609751e41dc63340652cd239c08c6989912'
route: 'oneshot'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The registration review filter dropdown arrows appear misaligned.

**Approach:** Correct the dropdown presentation with the smallest appropriate shared styling change, preserving native interaction and the existing design. Use focused verification.

</frozen-after-approval>

## Implementation Notes

Oneshot: a small CSS adjustment and generated stylesheet; shared toolbar selects are rendered by `Views/Shared/_ListToolbar.cshtml` and styled in `Styles/app.css`.

Styled shared list toolbar selects with a centered chevron and text clearance, retaining native appearance in forced-colors mode. CSS build and 51 JavaScript tests passed; browser computed styles confirm both filters receive the rule. Screenshot unavailable because the browser window was not visible; mobile, zoom, and forced-colors rendering remain unchecked. Focused critique found no confirmed priority issue; long labels and existing touch-target sizing warrant visual follow-up.

Increased toolbar filter preferred width from 160px to 190px to offset the room reserved for the chevron; fields can still wrap or shrink on narrow screens.

## Plan Change Log

## Review Triage Log

- Low, patched: the additional arrow padding reduced room for selected labels. Increased the preferred filter width; arbitrary long event names can still be clipped at narrow widths, as before.

## Verification

- Build CSS using `npm --prefix ASI.Basecode.WebApp run css:build` and check the resulting diff.
- Inspect registration review filters at desktop and narrow sizes if a browser is available; preserve keyboard operation.
