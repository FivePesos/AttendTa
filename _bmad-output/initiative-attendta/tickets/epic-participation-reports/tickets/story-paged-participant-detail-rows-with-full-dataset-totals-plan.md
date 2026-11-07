---
title: 'Paged participant detail rows with full-dataset totals'
type: 'feature'
ticket: '3'
created: '2026-11-07'
status: 'built'
baseline_revision: '911f69a1a4eadf7995f843f4b464a726567d9103'
route: 'full'
route_source: 'auto'
risk: 'low'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
followup_review_recommended: false
context:
  - 'AGENTS.md'
  - '_bmad-output/initiative-attendta/tickets/epic-participation-reports/tickets/story-paged-participant-detail-rows-with-full-dataset-totals.md'
  - '_bmad-output/initiative-attendta/tickets/epic-participation-reports/epic-participation-reports.md'
  - '_bmad-output/initiative-attendta/tickets/epic-participation-reports/tickets/story-completed-event-no-shows-and-signup-snapshot-attendee-distri-plan.md'
warnings: []
deferred: []
---

<intent-contract>

## Intent

**Problem:** The report's participant table still comes from the demo overlay (M-13c), which reads every registration in memory and slices the page. Management needs the same FR-30 detail rows from canonical SQL, paged 25 at a time with stable boundaries, while the summary totals and attendee distributions keep covering the full dataset.

**Approach:** Add a real counted/paged detail read to `ParticipationReportRepository`, ordered by participant name then `registration_id`, and map it in `ParticipationReportService` behind the existing `IParticipationReportService` contract. Delete the demo report provider and its registration so the real service answers the report in both switch states.

## Boundaries & Constraints

**Always:** Reads only (AD-2). Every registration of the event appears (all four statuses). Order by participant name, then `registration_id` (AD-13). Totals, no-shows and distributions stay full-dataset and never depend on the page. Check-in time is the stored UTC `recorded_at`; the view already formats it for Asia/Manila. Attendance labels stay exactly as today: Checked in, Invalidated, No-show, Not checked in, and "—" for a non-Confirmed row with no attendance. Keep the route, view model, Razor output and `IParticipationReportService` signature unchanged.

**Never:** Mutate participation records; change the view, CSS or visible labels; add a second report screen; re-derive a canonical fact; add a PM override; keep or re-create demo report state.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Completed event, mixed attendance | Confirmed rows with valid, invalidated and no attendance | Valid = Checked in/attended; invalidated = Invalidated/not attended; none = No-show | No error |
| Non-final event | Published event, Confirmed row without attendance | "Not checked in", no no-show | No error |
| Non-Confirmed row | Pending/waitlisted/cancelled with no attendance | Attendance "—" with no badge | No error |
| Page boundary | 26+ registrations, page 1 then page 2 | 25 rows then the remainder; same order, no row repeated or skipped | No error |
| Name tie | Two registrations with the same participant name | Ordered by `registration_id` across the boundary | No error |
| Invalid page | `page` null, 0, negative or past the last page | Page 1, or the last page; totals unchanged | No error |
| Empty event | Event with no registrations | Empty table, `TotalCount` 0, no pager | No error |

</intent-contract>

## Code Map

- `ASI.Basecode.Data/Interfaces/IParticipationReportRepository.cs` — add `CountDetailRowsAsync(eventId, ct)` and `GetDetailPageAsync(eventId, skip, take, ct)` plus a `ParticipationReportDetailRow` record (registration id, member name, student number, course snapshot, year snapshot, status, `CheckedInAt`, `IsAttendanceInvalidated`). `CheckedInAt` null means no attendance row.
- `ASI.Basecode.Data/Repositories/ParticipationReportRepository.cs` — count `Registration` rows for the event; page them with `Skip`/`Take`, joining `Member` and `User` for name/student number and left-joining `Attendance` (its `registration_id` is UNIQUE). `AsNoTracking`, no writes. Project to an anonymous type, materialize, then map to the record so EF cannot fail on record construction.
- `ASI.Basecode.Services/Services/ParticipationReportService.cs` — read the count, normalize the page with `PageRequest`, read one page, and map rows with the existing status/attendance label rules. Keep the summary/no-show/distribution reads unchanged.
- `ASI.Basecode.Services/Interfaces/IParticipationReportService.cs` — update the XML doc: 7.1–7.3 are all real; no demo provider remains.
- `ASI.Basecode.WebApp/DemoData/DemoParticipationReportService.cs` — delete (M-13c overlay and the report-side demo attendance merge).
- `ASI.Basecode.WebApp/DemoData/DemoReportsRegistration.cs` — delete; remove `DemoReportsRegistration.AddDemoReports(services)` from `DemoDataRegistration.cs` and refresh its comment.
- `ASI.Basecode.WebApp/DemoData/DemoStore.cs` — update the `MarkCompleted` comment: the fixture now serves the certificate ledger demo/tests, not the report overlay.
- `ASI.Basecode.WebApp/Controllers/ReportsController.cs` — update the class comment: the real service answers the report.
- `_bmad-output/initiative-attendta/demo-mock-register.md` — set M-13c to `removed (<commit>)`; rewrite the M-13 section so all three rows are removed and the real reads are described; fix the M-01 completed-state fixture wording.
- `_bmad-output/initiative-attendta/demo-script.md` — correct the known-limit line that says seeded store check-ins still appear on the report.
- `tests/ASI.Basecode.Services.Tests/ParticipationReportServiceTests.cs` — extend the fake repository with the two new methods; assert paging, ordering pass-through, row labels and page normalization.
- `tests/ASI.Basecode.IntegrationTests/ParticipationReportRepositoryTests.cs` — add a focused SQL smoke: 26 registrations page 25+1, stable order across the boundary, invalidated attendance flagged, snapshot course/student number returned.
- `tests/ASI.Basecode.IntegrationTests/DemoParticipationReportOverlayTests.cs` — delete with the provider.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Data/Interfaces/IParticipationReportRepository.cs` — add the count/page methods and detail record — gives the service a real paged source.
- [x] `ASI.Basecode.Data/Repositories/ParticipationReportRepository.cs` — implement the counted, ordered SQL page — stable boundaries and full-dataset count.
- [x] `ASI.Basecode.Services/Services/ParticipationReportService.cs` — read and map the page with the existing label rules — keeps the visible contract identical.
- [x] `ASI.Basecode.Services/Interfaces/IParticipationReportService.cs`, `ASI.Basecode.WebApp/Controllers/ReportsController.cs` — refresh comments — no stale demo wording.
- [x] `ASI.Basecode.WebApp/DemoData/DemoParticipationReportService.cs`, `DemoReportsRegistration.cs`, `DemoDataRegistration.cs`, `DemoStore.cs` — delete the provider and its registration; update the fixture comment — M-13c is gone.
- [x] `_bmad-output/initiative-attendta/demo-mock-register.md`, `demo-script.md` — record the removal and correct the stale report claim.
- [x] `tests/ASI.Basecode.Services.Tests/ParticipationReportServiceTests.cs`, `tests/ASI.Basecode.IntegrationTests/ParticipationReportRepositoryTests.cs` — cover the matrix rows; delete `DemoParticipationReportOverlayTests.cs`.

**Acceptance Criteria:**
- Given a completed event with more than 25 registrations, when page 1 and page 2 load, then each shows 25 and the remainder in the same name-then-registration-id order with no row repeated or skipped.
- Given a completed event, when the report loads, then each row shows name, student number, snapshot course, registration status, attendance result and the stored check-in time formatted for Manila.
- Given any detail page, when it loads, then the summary counts, no-shows and course/year breakdowns equal the full-dataset values.
- Given the demo switch on or off, when the report loads, then the real `ParticipationReportService` answers it and no demo report provider is registered.

## Implementation Notes

- `GetDetailPageAsync` joins `Registration` → `Member` → `User` for the name and student number and
  left-joins `Attendance` on the UNIQUE `registration_id`. It projects an anonymous type, materializes
  with `Skip`/`Take`, then maps to `ParticipationReportDetailRow`, so EF never constructs the record
  inside the query.
- `ParticipationReportService` reads the count first, normalizes the page with `PageRequest`, and
  reads exactly one page; an empty event skips the page read. The summary, no-show and distribution
  reads are unchanged, so they never depend on the page.
- The label rules moved verbatim from the deleted overlay into `ParticipationReportService.ToRow` and
  `Attendance`: Checked in, Invalidated, No-show, Not checked in, and "—" with no badge for a
  non-Confirmed row with no attendance. `Attended` is true only for a valid check-in.
- `DemoParticipationReportService`, `DemoReportsRegistration` and `DemoParticipationReportOverlayTests`
  are deleted; `DemoDataRegistration` no longer registers a report provider. `DemoStore.MarkCompleted`
  and `IsCompleted` remain for the certificate-ledger demo and its M-10a tests.
- The integration smoke seeds names descending as registration ids ascend, so a page ordered by name
  is provably not ordered by id; the two lowest ids share the largest name so the tie breaks on
  `registration_id` exactly at the page boundary.

## Plan Change Log

## Review Triage Log

## Design Notes

- The repository counts and pages the same event-scoped `Registration` set; the count is the pager's `TotalCount`, so the summary reads stay independent of the page.
- Attendance is left-joined because `attendance.registration_id` is UNIQUE; a null `CheckedInAt` is the only "no attendance row" signal, and `IsAttendanceInvalidated` distinguishes Invalidated from Checked in.
- Deleting the provider is the ticket's instruction: after 7.3 the demo host's report reflects real SQL attendance only. The seeded demo-store check-ins stay for the certificate ledger and scanner demo, not the report.

## Verification

**Commands:**
- `dotnet build` — 0 warnings, 0 errors.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — 310/310 pass, including the new paging, page-normalization and label tests.
- `$env:ATTENDTA_TEST_SQL = "Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` — 400/400 pass, including the new 26-row paging smoke.
