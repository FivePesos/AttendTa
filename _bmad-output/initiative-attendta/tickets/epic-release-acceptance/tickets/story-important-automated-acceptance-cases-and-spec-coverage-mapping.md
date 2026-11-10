---
id: 5
type: story
title: "Important automated acceptance cases and SPEC coverage mapping"
parent: epic-release-acceptance
covers: [R1]
after: [10, 1.8, 2.9, 3.14, 4.9, 5.10, 6.11, 7.3]
hitl: false
risk: medium
tracker_id: "78"
remote: "https://github.com/rainyDayssss/AttendTa/issues/78"
tracker_status: backlog
---

# Important automated acceptance cases and SPEC coverage mapping

## Description

Author only the missing important automated acceptance cases in the two existing test projects, reusing current auth, seat/cutoff, cancellation, audit and PDF coverage. Use finale-test-cases.md for ownership/antiforgery, duplicate/refused attendance, completion rollback, access cascades/last admin, duplicate issuance and invalidation races, retained/authorized files and report formulas. Do not automate every cosmetic state or duplicate an existing passing case. Map every remaining SPEC §20 scenario to an existing automated case or a concrete manual case in 8.6; no scenario is silently removed. 8.4 owns the final combined full-suite execution.

## Requirements

Verify: The acceptance matrix identifies an existing or added automated test for each high-risk case and a manual case for every remaining SPEC §20 scenario; targeted new cases pass and real-SQL race checks prove SM-4. No new test project or large prototype-only suite is introduced.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-release-acceptance/epic-release-acceptance.md
- _bmad-output/initiative-attendta/delivery-queue.md
- _bmad-output/initiative-attendta/finale-test-cases.md

## Delivery (2026-11-06)

- Test-only authoring of the missing important cases; it does not build a second screen or change presentation, and prototype-only coverage does not close the issue.
- #78 is not "automate every cosmetic scenario": full SPEC §20 coverage stays through the automated/manual mapping, with the remainder assigned to concrete 8.6 manual cases.
- CI now does install Chromium; use the PDF test double for SQL race/lock tests and the real renderer for PDF checks. Existing CI unchanged.
- Missing important test authoring is this entry (#78); combined final execution is 8.4 (#77), manual 8.6–8.8.
- Test-only: no Impeccable requirement; frontend design lives only in the separate slices 6.12/6.13/7.4 and finale 8.12.
- The high-risk lifecycle/storage/race rules stay final acceptance; no new test project or large prototype-only suite.

## Delivery (2026-11-10)

Test-only authoring in the two existing projects plus the coverage map. No production code, Razor,
CSS, JS, schema or CI change.

- Added seven targeted cases: the parallel duplicate scan (`CheckInFlowTests`), the waitlisted
  token refusal (`CheckInFlowTests`), the concurrent double issue (`CertificateIssuanceTests`),
  the completion start boundary in both directions (`EventCompletionServiceTests`), the 30-row
  mixed-status report totals (`ParticipationReportRepositoryTests`), and overlapping-event
  registrations (`MemberEventRegistrationFlowTests`).
- SM-4 is now proved for all three clauses: zero overbooked events (existing), zero duplicate
  attendance records and zero duplicate certificates (both new, on two gated SQL connections).
- `_bmad-output/initiative-attendta/acceptance-coverage-matrix.md` maps every finale case, every
  SPEC §20 bullet and every functional ID group to an existing or added automated test, and names
  the manual remainder with its owning 8.6 case. Nothing is silently dropped.
- Verification: `dotnet build ASI.Basecode.sln --configuration Release` succeeded with 0 warnings
  and 0 errors. The five edited classes ran green against LocalDB
  (`CheckInFlowTests` 12, `CertificateIssuanceTests` 14, `EventCompletionServiceTests` 10,
  `ParticipationReportRepositoryTests` 3, `MemberEventRegistrationFlowTests` 8).
- The combined full-suite execution and its manifest remain 8.4 (#77); the manual cases remain
  8.6 (#79).
