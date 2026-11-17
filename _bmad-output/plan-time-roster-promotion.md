---
title: 'Time display and registration roster improvements'
type: 'feature'
ticket: ''
created: '2026-11-17'
status: 'blocked'
baseline_revision: 'f3d7e6820b784ede9e631972fca35eb2c99c611c'
route: 'full'
route_source: 'auto'
risk: 'high'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
followup_review_recommended: false
context: ['{project-root}/DESIGN.md', '{project-root}/.opencode/skills/impeccable/reference/craft-floor.md']
warnings: [multiple-goals]
deferred: []
---

<intent-contract>

## Intent

**Problem:** Time displays use 24-hour formats, roster submission timestamps are hidden, and promotion confirmation does not identify the participant.

**Approach:** Display user-facing times in 12-hour AM/PM Manila time, put submission date and time in adjacent roster columns, and identify and safely confirm the next eligible participant.

## Boundaries & Constraints

**Always:** Preserve UTC storage, Manila conversion, antiforgery, authorization, audit, eligibility, queue ordering, capacity and transaction locking. Use existing accessible table and dialog patterns and preserve mobile labels. Check that the candidate confirmed is the candidate promoted.

**Never:** Change machine-readable ISO form values, logs, persisted data, retained certificates, or promote a replacement when a confirmation is stale. Do not choose a candidate from a filtered or paged roster.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Display | Manila midnight, noon, afternoon | 12-hour times with AM/PM and correct date | Preserve null handling |
| Promotion | Earliest eligible waitlisted member exists | Confirmation names actual candidate; only that candidate may be promoted | Revalidate seat, cutoff and eligibility |
| Stale confirmation | Candidate changes after dialog render | No replacement promoted | Show refresh/retry error |
| Empty queue | No eligible candidate | No misleading named promotion | Explain unavailability |

</intent-contract>

## Code Map

- `ASI.Basecode.WebApp/Formatting/ManilaTimeFormatter.cs` is the UTC-to-Manila display formatter used throughout pages and certificate generation.
- `ASI.Basecode.WebApp/Views/Event/_Form.cshtml` has local wall-clock summaries and ISO datetime-local inputs; preserve input binding and hidden fractional precision.
- `ASI.Basecode.WebApp/wwwroot/js/features/scanner.js` separately formats check-in timestamps.
- `ASI.Basecode.WebApp/Views/Event/Details.cshtml` has dossier roster and promotion dialog; `Views/RegistrationReview/Index.cshtml` is another roster and promotion entry point.
- `ASI.Basecode.WebApp/Controllers/EventController.cs` assembles dossier; `RegistrationReviewController.cs` serves queue and accepts promotion.
- `ASI.Basecode.Services/Services/RegistrationReviewService.cs`, `Interfaces/IRegistrationReviewService.cs`, `ASI.Basecode.Data/Repositories/RegistrationRepository.cs`, and `Data/Interfaces/IRegistrationRepository.cs` implement authoritative full-queue candidate selection and transactional rechecks.
- `tests/ASI.Basecode.IntegrationTests/ManilaTimeFormatterTests.cs`, `RegistrationPromotionTests.cs`, other time-display integration tests, and `tests/ASI.Basecode.Services.Tests/RegistrationReviewTests.cs` cover existing contracts.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.WebApp/Formatting/ManilaTimeFormatter.cs`, `Views/Event/_Form.cshtml`, `wwwroot/js/features/scanner.js` and other user-facing time renderers -- apply consistent 12-hour AM/PM without altering machine values or double conversion.
- [ ] `ASI.Basecode.WebApp/Views/Event/Details.cshtml` and `Views/RegistrationReview/Index.cshtml` -- add adjacent submission date and time columns, responsive labels and correct empty-row spans.
- [ ] `ASI.Basecode.Data/Interfaces/IRegistrationRepository.cs`, `Repositories/RegistrationRepository.cs`, `ASI.Basecode.Services/Interfaces/IRegistrationReviewService.cs`, `Services/RegistrationReviewService.cs` -- expose eligible candidate identity and require the displayed registration ID for promotion; compare on fresh selection and under existing locks.
- [ ] `ASI.Basecode.WebApp/Controllers/EventController.cs`, `Controllers/RegistrationReviewController.cs`, `Views/Event/Details.cshtml`, `Views/RegistrationReview/Index.cshtml` and associated view models -- show candidate name on confirmation at both entry points, handle empty and stale states without unsafe fallback.
- [ ] `tests/ASI.Basecode.IntegrationTests/ManilaTimeFormatterTests.cs`, `RegistrationPromotionTests.cs`, affected time-display tests and `tests/ASI.Basecode.Services.Tests/RegistrationReviewTests.cs` -- cover matrix, both promotion routes, stale candidate, queue eligibility, and 12-hour boundaries.
- [ ] `PRODUCT.md` -- update relevant product behavior to match changed roster and promotion flow.
- [ ] Conduct required impeccable critique and polish of changed UI, respecting `DESIGN.md` and craft floor.

**Acceptance Criteria:**
- Given a displayed timestamp, when the page or scanner renders it, then it shows Manila local time in 12-hour AM/PM format; native machine-readable values remain valid.
- Given a registration roster row, when viewed on desktop or mobile, then submission date and time appear as separate adjacent labelled columns.
- Given an eligible waitlist, when management opens promotion confirmation from either entry point, then it names the actual next eligible participant across the full queue.
- Given a changed candidate after confirmation is shown, when management submits, then no different participant is promoted and a retry message appears.
- Given unchanged eligible candidate and free capacity, when management confirms, then only that candidate is promoted with existing authorization, transaction, and audit guarantees.

## Implementation Notes

Implemented 12-hour time displays, submission columns, and candidate-bound promotion confirmations. Release build and 408 service tests passed. An implementation subagent reported a successful full integration run, but the independent full integration run timed out after 600 seconds and did not finish. Source-based UI critique and focused polish were performed; authenticated browser inspection remains unverified.

## Auto Run Result

Status: blocked
Blocking condition: implementation verification failed. The required full integration test command exceeded its 600-second timeout; desktop/mobile browser inspection was unavailable. No feature commit, push, or pull request was created.

## Plan Change Log

## Review Triage Log

## Verification

- `dotnet build ASI.Basecode.sln --configuration Release` -- succeeds.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` -- succeeds; use documented direct test-host fallback if zero tests run.
- `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` -- succeeds with configured disposable SQL Server; report unavailable dependencies honestly.
- Inspect rendered dossier, queue and promotion confirmation at desktop and mobile widths and run the required design detector.
