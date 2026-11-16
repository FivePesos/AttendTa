---
title: 'Route legacy cover errors to the event editor'
type: 'bugfix'
ticket: ''
created: '2026-11-16'
status: 'built'
baseline_revision: '0f42dd7b5361e5422495c890a3cdbd7066d7b7c2'
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

**Problem:** Failed legacy cover uploads or removals can reopen a separate editable cover page even though cover management now belongs in the event editor.

**Approach:** Route failed legacy operations for editable events to the event editor with a visible actionable error; retain legacy mutation endpoints, authorization and antiforgery, preserve terminal-event read-only viewing and missing-event behavior. Do not retire endpoints without confirming their consumers.

</frozen-after-approval>

## Implementation Notes

Oneshot: a focused controller error-handling adjustment and regression coverage.

Legacy validation and domain errors now redirect to the editor for editable events or the read-only cover view for terminal events, carrying the error via TempData. Release build and all four EventCoverFlowTests passed.

## Plan Change Log

## Review Triage Log

Quick independent review: no findings.

## Verification

- Build Release and run focused EventCoverFlowTests against a disposable LocalDB fixture, including invalid uploads, removal rejection and terminal read-only coverage.
- Verify legacy errors remain visible after redirect and successful legacy commands still work.
