---
title: 'Remove duplicate check-in notification'
type: 'bugfix'
ticket: ''
created: '2026-11-20'
status: 'built'
baseline_revision: 'e9808841db2fc9aca8475944049a87eb285864fe'
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

**Problem:** Check-in outcomes are displayed both in the top notice and in the scanner result card.

**Approach:** Keep the detailed result card as the sole check-in outcome notification for manual lookup and scans. Preserve identity, time, errors, accessibility and no-JavaScript behavior. Do not affect attendance invalidation notices or server-side check-in rules.

</frozen-after-approval>

## Implementation Notes

Small, focused controller change with targeted tests. `CheckInController.SetOutcomeMessage` writes both `CheckInOutcome` and `SuccessMessage`/`ErrorMessage`; remove only the latter. Manual and non-AJAX scan render the persisted result through `Views/CheckIn/Index.cshtml`; AJAX camera scans use `scanner.js` and already render the result directly. Invalidation sets separate messages and must remain unchanged. Add HTTP-level regression coverage for success and error paths, no-JavaScript scan and manual, and unchanged invalidation notices when feasible. Existing scanner JavaScript tests cover camera.

When the original event is no longer selected or available, a single top notice identifies the original event instead of dropping the outcome. The result card gains focus after redirects and an in-page link supports navigation without JavaScript. Camera-rendered cards preserve the link target. Browser/screen-reader verification was not run.

## Plan Change Log

## Review Triage Log

- First review: event mismatch could discard feedback and the bottom card was hard to discover; patched with an event-identified fallback, focus and link. Added successful manual and invalidation tests. The original closed-state test also asserts the card-specific class.
- Follow-up review: patched event identification and a stale result link after camera scans. Fallback success/repeat combinations and physical screen-reader behavior remain unverified; existing tests cover fallback errors and manual success.

## Verification

- Build on F: to avoid the full C: drive and the running preview's locked executable; run focused SQL integration tests using isolated generated test databases on F: and the scanner JavaScript suite.
- Verify both notification types and report skipped tests. Do not touch the demo database.
- Result: isolated Release integration project build passed; 28 check-in tests passed before follow-up, 9 focused tests passed after, and 72 JavaScript tests passed. UI detector reported no findings. No demo database changed.
