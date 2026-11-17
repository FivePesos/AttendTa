---
title: 'Issue 243 report cards and member validation'
type: 'bugfix'
ticket: ''
created: '2026-11-17'
status: 'in-review'
baseline_revision: '33291d9298fe96ebfda661fe997b4a2f1d5ba170'
route: 'full'
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

**Problem:** Participation report cards are uneven and their displayed numbers need verification against their source; member editing repeats a field error above Email.

**Approach:** Correct the layout and any demonstrated statistic mismatch while preserving documented rules; show field errors only at their respective fields in both editing modes.

## Boundaries & Constraints

**Always:** Preserve existing work in the original worktree. Use the separate branch based on latest upstream/dev. Verify desktop and mobile rendered screens, add regression tests, reference issue #243 as a partial fix, and retain model-level errors and conflict recovery.

**Never:** Claim the broader issue is complete, change CI workflows, invent aggregates, change business rules without verified evidence, or touch unrelated application features.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Completed event | Confirmed, valid and invalidated check-ins, other statuses | Counts, rate and no-shows match documented source facts | Zero denominator displays N/A |
| Incomplete or cancelled event | Provisional or cancelled state | Final-only metrics retain their documented states | No misleading final figures |
| Filter or pagination | A subset of detail rows | Event-wide cards are clearly scoped and match full event snapshot | No silently misleading scope |
| Invalid member edit | Invalid or duplicate student number in dialog or standalone form | Error appears beneath Student Number only | Model-level failures and conflict recovery remain visible |
| Responsive rendering | Desktop and 390px mobile | Summary cards align without overflow; field errors stay associated | Other form behaviors remain usable |

</frozen-after-approval>

## Code Map

- `docs/AttendTa-System-Specification.md` section 15 defines confirmed seats, valid attendance, no-shows and rate; pagination never changes aggregates.
- `ASI.Basecode.Services/Services/ParticipationReportService.cs` computes summary from an event-wide snapshot; filters affect detail rows. `ASI.Basecode.Data/Repositories/ParticipationReportRepository.cs` supplies the facts. Verify any apparent mismatch before changing semantics.
- `ASI.Basecode.WebApp/Views/Reports/Participation.cshtml` renders four cards and participant details; `ASI.Basecode.WebApp/Styles/features/reports.css` controls grid, while `Styles/app.css` defines `.panel + .panel` margins which can compete with card styles.
- `ASI.Basecode.WebApp/Views/Member/_Editor.cshtml` uses an All summary above Email; `Edit.cshtml` uses ModelOnly; `_Form.cshtml` already supplies per-field messages. Check unobtrusive client validation and server errors in both routes, preserving the model-level error summary and stale-edit recovery.
- `tests/ASI.Basecode.Services.Tests/ParticipationReportServiceTests.cs`, `tests/ASI.Basecode.IntegrationTests/ParticipationReportRepositoryTests.cs` and `MemberManagementTests.cs` contain existing report and browser/HTTP coverage to extend.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.WebApp/Styles/features/reports.css` and `ASI.Basecode.WebApp/Views/Reports/Participation.cshtml` — correct card alignment and make data scope clear if necessary, using the existing design system.
- [ ] `ASI.Basecode.WebApp/Views/Member/_Editor.cshtml`, `Edit.cshtml`, and shared validation code if needed — isolate field and model-level errors without degrading recovery or other editors.
- [ ] `tests/ASI.Basecode.Services.Tests/ParticipationReportServiceTests.cs` and integration/browser tests — cover verified formulas, card rendering/scope, and field-specific errors in both edit modes.
- [ ] Run Impeccable critique and polish on affected rendered surfaces, including desktop and mobile, and record any limitations.

**Acceptance Criteria:**
- Given a completed event with mixed registration and attendance states, when its report is displayed, then card figures agree with the canonical full-event facts and the prescribed formulas.
- Given filtered or paged details, when the report is displayed, then the relationship between cards and detail rows is clear and no summary changes solely because of pagination.
- Given an invalid student number, when either editor validates it, then its error appears only under Student Number, while model-level errors remain discoverable.
- Given desktop or 390px width, when the affected pages are viewed, then card boundaries align and both editors remain legible and usable.

## Implementation Notes

- Confirmed the documented full-event formulas against the service and a SQL-backed browser fixture; no calculation change was justified. The report now identifies summary/filter scope.
- Corrected the shared-panel CSS specificity conflict and kept model-level errors in the member dialog while routing field errors to their fields. Focus moves to the first invalid field when no model-level error exists.
- Release build, CSS build, 407 service tests, 51 JavaScript tests, focused SQL/browser report and member tests passed. Screenshots at 1440px and 390px were inspected for the report, dialog and standalone editor.
- OpenAI reviewers inspected the report and member editor screenshots and ran the mechanical detector (no findings). The breakdown population wording was clarified. A follow-up browser test verifies Tab and Shift+Tab reach the dialog actions after multiple errors at 390px and 1440px. Actual screen-reader speech remains unverified; shared 36px touch fields are a pre-existing wider-system concern. Critique snapshot recorded; polish cannot close it while screen-reader verification is outstanding.
- Full integration suite passed 647 tests in 609.287 seconds with no failures, after an earlier 600-second timeout. The suite ran before the final keyboard-test addition; that addition passed separately in all three browser cases. SQL/browser report tests (3) and duplicate-field HTTP tests (2) also passed after their changes.

## Plan Change Log

## Review Triage Log

- Quick review: two findings. Required critique/polish incomplete: valid process gap; remain in review. Duplicate student-number response rendering untested: valid; added and passed HTTP regression tests for both editor modes.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — succeeds.
- Run relevant service and integration tests, including browser regression coverage, following `AGENTS.md` and the repository test harness.
- Run the CSS build and Impeccable detector for changed UI targets.

**Manual checks:**
- Inspect report and both member editors at desktop and 390px; exercise invalid student number and relevant empty/non-final report states.
