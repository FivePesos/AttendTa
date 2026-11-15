---
id: 16
type: story
title: "Integrated verification after the four feature tickets"
parent: epic-release-acceptance
after: [1.15, 1.16, 3.20, 6.14]
hitl: true
risk: medium
tracker_id: "242"
remote: "https://github.com/rainyDayssss/AttendTa/issues/242"
---

# Integrated verification after the four feature tickets

## Description

Run the whole suite and walk the four changed surfaces on the integrated build once 1.15, 1.16, 3.20 and 6.14 have merged: the full IntegrationTests suite on SQL Server, the service suite and the JavaScript tests, then sign in as Administrator, Officer and Member and exercise signup validation, the membership request queue, the registration review queue and the certificate eligibility screen. Record the commit, the command output and what was seen. A finding goes back to the ticket that owns it; nothing is fixed here.

## Requirements

Verify: The full IntegrationTests, service and JavaScript suites pass on one commit, and each of the four surfaces is walked in the role that owns it with the result recorded. Any failure names the owning ticket.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-release-acceptance/epic-release-acceptance.md
- _bmad-output/initiative-attendta/plan-1-15-profile-validation.md

## Notes

- Reopens the scope of the closed 8.4 for the current code. The tracker closed 8.4 on 2026-11-11 and the code has changed since, so its evidence no longer covers this build.
- A person runs this. No new automated tests are authored here; 8.5 already mapped the important cases.
