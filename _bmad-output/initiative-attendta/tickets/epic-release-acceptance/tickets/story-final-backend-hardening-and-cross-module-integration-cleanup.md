---
id: 13
type: story
title: "Final backend hardening and cross-module integration cleanup"
parent: epic-release-acceptance
after: [3.14, 4.9, 5.10, 6.11, 7.3]
hitl: false
risk: high
tracker_id: "151"
remote: "https://github.com/rainyDayssss/AttendTa/issues/151"
tracker_status: backlog
---

# Final backend hardening and cross-module integration cleanup

## Description

Resolve concrete cross-module backend findings after the focused module sweeps: confirm one caller-owned transaction/audit path, lock order, canonical facts, cancellation/hook registration, authorization/ownership, time guards and safe private file failure/cleanup. Include report read coherence and source registration. No feature expansion or frontend presentation work; do not duplicate the module sweeps or author exhaustive tests here. Hand the stable real-provider wiring to 8.11.

## Requirements

Verify: Build and the focused request-to-certificate/report smoke succeed on real data; each agreed backend finding is fixed or an explicit release blocker, and there is one implementation of shared cancellation, eligibility and storage rules. 8.5/8.4 own the final important tests.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-release-acceptance/epic-release-acceptance.md
- _bmad-output/initiative-attendta/delivery-queue.md
- _bmad-output/initiative-attendta/finale-test-cases.md
