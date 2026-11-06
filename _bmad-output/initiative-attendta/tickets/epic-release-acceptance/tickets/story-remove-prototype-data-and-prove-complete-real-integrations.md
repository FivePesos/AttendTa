---
id: 11
type: story
title: "Remove prototype data and prove complete real integrations"
parent: epic-release-acceptance
covers: [R1, R2, R3]
after: [13, 2.10, 2.11]
hitl: false
risk: high
tracker_id: "152"
remote: "https://github.com/rainyDayssss/AttendTa/issues/152"
tracker_status: backlog
---

# Remove prototype data and prove complete real integrations

## Description

Use 1.13's fixture register to replace/remove every required mock-backed source and simulated mutation with its accountable real implementation. Verify My Events/passes/attendance, scanner, completion, certificate review/issue/download, access changes and reports against one coherent database. Remove prototype routes/providers from runtime wiring, keep only isolated test fixtures, and refuse any release configuration enabling prototype mode. This is a functional integration gate, not a frontend redesign or a claim that the final test suite already passed.

## Requirements

Verify: The fixture register has no unresolved required surface, release startup cannot enable fixture providers, and a real member goes from approved account through registration/pass/check-in/completion to a retained certificate and matching report/audit, with no manual SQL state workaround.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-release-acceptance/epic-release-acceptance.md
- _bmad-output/initiative-attendta/delivery-queue.md
- _bmad-output/initiative-attendta/finale-test-cases.md
