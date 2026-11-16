---
title: 'Fix member edit submission'
type: 'bugfix'
ticket: ''
created: '2026-11-16'
status: 'built'
baseline_revision: 'e17de42d5dd9ee77a1c7c2b62ec9b579e61eeb98'
route: 'oneshot'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Member edit submits to the GET-only member-specific URL, returning HTTP 405 and leaving changes unsaved. The standalone editor may have the same routing issue.

**Approach:** Point both member edit forms to the existing POST action and add browser regression coverage for modal save, validation and unsaved-change handling. Preserve existing membership rules; defaulting new membership types is a separate proposed decision.

</frozen-after-approval>

## Implementation Notes

Oneshot: a small routing correction and a focused browser regression test. The existing Playwright/LocalDB test harness can host the reproduction. The temporary browser experiment proved that POST /Member/Edit saves and returns field errors.

Changed both editor forms to clear the ambient route ID, and added a Chromium integration test for saving, validation, discard protection and the standalone form action. Release build succeeded and all 16 member-management tests passed. The user supplied an independent review from a separate session reporting no findings. A separate session supplied a degraded single-context UI critique and polish: no new markup findings, detector clean, no browser inspection or snapshot; it noted the pre-existing lack of read-only email explanation in the modal. The Chromium regression independently verified modal save, validation and discard behavior.

## Plan Change Log

## Review Triage Log

- Independent quick review: no findings reported by the separate review session.

## Verification

- `dotnet build ASI.Basecode.sln --configuration Release` succeeds.
- Run the member integration and browser regression tests using the isolated LocalDB fixture; verify valid saves persist and invalid fields produce errors.
