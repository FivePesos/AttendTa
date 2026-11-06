---
id: 4
type: story
title: "Create certificate reviews in event completion transaction"
parent: epic-certificates
covers: [R1]
after: [1.2, 1.3, 3.12]
hitl: false
risk: medium
tracker_id: "64"
remote: "https://github.com/rainyDayssss/AttendTa/issues/64"
tracker_status: backlog
---

# Create certificate reviews in event completion transaction

## Description

Implements IEventCompletionHandler, which epic-events-registration's completion transaction (3.12) declares and calls inside its own transaction, inserting through CertificateRepository a pending_approval certificate review for each Registration with Valid Attendance (ParticipationFacts, AD-5) under the event lock completion already holds, with created_by set to the completing officer and no file or PDF work, delivering R1.

## Requirements

Verify: Completing an event creates pending_approval review rows for every Registration with Valid Attendance and none for absent or invalidated attendance, all in the completion's database transaction, and a completion that fails rolls back every review.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-certificates/epic-certificates.md
- _bmad-output/initiative-attendta/delivery-queue.md
- _bmad-output/initiative-attendta/finale-test-cases.md

## Delivery (2026-11-06)

- Integration consumes the existing completion hook (3.12) and mapped attendance facts; it does not build a second completion screen and does not wait for the scanner UI or recording engine.
- Development check: build plus a focused completion-to-review smoke; existing CI unchanged. Missing important test authoring is 8.5 (#78), combined final execution 8.4 (#77), manual 8.6–8.8.
- Backend-only: no Impeccable requirement; frontend design lives only in the separate slices 6.12/6.13/7.4 and finale 8.12.
- The pending-only review rule and the completion-transaction rollback stay final acceptance even while test authoring is deferred.
