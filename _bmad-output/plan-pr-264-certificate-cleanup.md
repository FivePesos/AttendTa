---
title: 'Protect hotspot certificate files on failure'
type: 'bugfix'
ticket: ''
created: '2026-11-16'
status: 'built'
baseline_revision: 'c9a0fd14b72637f7d656ef0a5046ef2141e27fc3'
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

**Problem:** A failed hotspot certificate command can delete files created or replaced by another process.

**Approach:** Prevent overwrites and preserve potentially shared output on failure; verify successful generation and collision behavior.

</frozen-after-approval>

## Implementation Notes

Small, focused fix and regression coverage; use the oneshot route. Preserve partial output on failure rather than deleting paths that another process may have replaced.

Files are created exclusively; private-key files use restrictive Unix permissions at creation. A failed run warns that partial output may remain and a new output directory should be used.

## Plan Change Log

## Review Triage Log

- Medium, patched: the concurrent subprocess test could pass without exercising the post-check collision; added a deterministic exclusive-creation collision test.

## Verification

- Build the integration test project and run the hotspot certificate tests.
