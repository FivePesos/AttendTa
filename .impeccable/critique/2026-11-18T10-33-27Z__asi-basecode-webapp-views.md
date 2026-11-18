---
target: changed tables and certificate history
total_score: 21
max_score: 32
na_heuristics: 8,10
p0_count: 0
p1_count: 0
target_identity: "file:F:\\AttendTaBakcend\\issue-243-consistency\\ASI.Basecode.WebApp\\Views"
timestamp: 2026-11-18T10-33-27Z
slug: asi-basecode-webapp-views
---
# Critique: changed tables and certificate history

Method: dual-agent; both reviewers lacked browser access. Parent independently confirmed the two empty-state findings using the running demo; screenshot and overlay unavailable.

## Design health
Eight evidence-backed heuristics scored 21/32: status 3, real-world match 3, control 3, consistency 2, prevention 3, recognition 3, efficiency 2, error recovery 2. Minimalist design and help were not scored due to insufficient evidence. This is not a whole-app rating.

## Strengths
Visible actions, text status labels, decision context, and actionable missing-certificate guidance support the operational workflow.

## Findings
- P2: Event Types search empty state incorrectly claims no event types exist and suggests creating one. Independently reproduced; subsequently corrected in the working tree and covered by an integration test.
- P2: Issued Registry search empty state claims no certificates have been issued although the event has one. Independently reproduced; certificate-owner coordination required before changing copy.
- P3: Account Requests uses Decline for the action and Rejected for status; terminology requires Auth-owner coordination.

## Detection and limitations
Markup detector: zero findings across seven targeted view files. Reviewers had no browser connection; no overlay was injected. Parent browser snapshots verified the two empty states. User separately reported mobile and 200% zoom acceptable. Actual screenshot, touch, screen-reader and full keyboard/error-state checks remain open.
