---
title: 'Whole-app consistency sweep: one implementation, one vocabulary'
type: 'refactor'
ticket: '17'
created: '2026-11-18'
status: 'in-progress'
baseline_revision: '1f486ff77e9963e0a020d36437c8c85f5ccd1380'
route: 'full'
route_source: 'auto'
risk: 'high'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
context:
  - 'DESIGN.md'
  - 'PRODUCT.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The integrated application needs one current, evidence-backed consistency sweep across its roles, pages and shared interaction patterns. Earlier work addressed individual screens, not this complete acceptance scope.

**Approach:** Walk the current application against the established design system, record findings, and fix demonstrated inconsistency or duplication without changing business rules. Run rendered critique and polish plus the complete build and test suites; record remaining limitations honestly.

## Boundaries & Constraints

**Always:** Preserve roles, authorization, antiforgery, filtering, ordering, state, accessibility and business semantics. Coordinate changes in Auth, Transactions, Reports and Administration with their owners. Use disposable test data and preserve the existing local database. Test 390px, 200% zoom, keyboard, focus, long content, empty and error states, and relevant permitted screens for Administrator, Officer and Member. Follow root `DESIGN.md`, use Impeccable during UI implementation, and keep `PRODUCT.md` current. The owner directs that #243 proceed before #242; final integrated verification follows rather than gates this work.

**Never:** Add features, change database schema or business rules, reset the existing database, fabricate a passing walkthrough, or remove intentionally distinct behavior merely because two code paths resemble each other. Treat earlier evidence as historical, not a current pass.

**Decision:** Proceed with approximately 1 GB free on C:; stop and report if restore, SQL Server or browser checks need more space.

**Decision:** Remove visible Manila-time, Asia/Manila and Philippine-time wording from UI surfaces and the certificate template, leaving only the date and time; keep all existing UTC storage and Manila-time conversions. Do not alter or regenerate previously issued certificates.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| List pagination | Empty, filtered, last page, large counts | One accurate accessible footer; filters and stable ordering remain intact | Clamp before fetching or refetch after clamping; assert rows and range together |
| Dialog dismissal | Dirty, busy, validation failure, asynchronous loading | Shared guard preserves input, focus and each surface's correct baseline | Busy actions cannot dismiss; failed saves retain input |
| Responsive screens | 390px, 200% zoom, long text, keyboard | Controls and content remain reachable without obscuring information | Errors and empty states remain understandable |
| Role permissions | Administrator, Officer and Member | Each sees allowed screens and appropriate status vocabulary | Unauthorized operations remain denied |
| Timezone labels | Labels in screens or newly rendered certificates | Manila-local values stay unchanged and visible timezone labels are removed | Retained issued PDFs are never regenerated or modified |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Views/`, `Styles/` and `wwwroot/js/` — existing screens and shared UI; use the established workspace design, no redesign.
- `ASI.Basecode.WebApp/Views/Certificates/CertificateTemplate.cshtml` — event and issuance labels include “Manila time”; adjust display wording for newly generated certificates without changing dates, formatting, signatory, or retained issued PDFs. `Views/AccountRequest/Index.cshtml` includes an Asia/Manila accessible label; preserve accessible context when reducing repetition.
- `ASI.Basecode.WebApp/wwwroot/js/site.js` — existing shared dialog dirty-state guard; `features/record-dialog.js`, `certificate-ledger.js`, `bulk-selection.js` and `certificate-preview.js` have different async loading, close, cleanup and focus needs.
- `ASI.Basecode.Services/ServiceModels/PagedResult.cs` and `AccountRequestQueueViewModel.cs` — canonical paging metadata and a wrapper that repeats range arithmetic; confirm paths and cover extreme/empty cases before changing.
- `ASI.Basecode.Data/Repositories/MemberRepository.cs` and `AccountRequestRepository.cs` — repeated course-alias filtering; retain exact query semantics and database-side filtering if consolidation is justified.
- `ASI.Basecode.Services/Services/ParticipationReportService.cs` — report uses a single event-wide snapshot; preserve it. Report card alignment and member-editor validation from PR #272 are already merged.
- `ASI.Basecode.WebApp/tests/dialog-guard.test.js`, `site.test.js`, `bulk-selection.test.js`; `tests/ASI.Basecode.Services.Tests/PagingTests.cs`; `tests/ASI.Basecode.IntegrationTests/CompactListFlowTests.cs`, `MemberManagementTests.cs` — existing regression patterns.
- `_bmad-output/plan-gh-243-report-member-validation.md` — prior partial evidence; screen-reader speech and mobile touch-target concerns need current verification.

## Tasks & Acceptance

**Execution:**
- [ ] `_bmad-output/initiative-attendta/whole-app-consistency-243.md` — first inventory permitted screens, role/state coverage and list-query, control, label and status vocabularies; record current revision, environment, observations, fixes and explicit blocked items without secrets.
- [ ] `ASI.Basecode.WebApp/Views/`, `Styles/`, `wwwroot/js/` — walk sign-in, membership request, member feed/detail, every My Events tab and pass, check-in/scanner, dossier/roster, registration review, master data, certificate ledger and viewer, reports, accounts, activity log and profile. Check each applicable list for labelled search, its own filters, Reset retaining event/history context, one accurate footer, meaningful details and narrow-screen stacking. Verify screen-reader announcements and mobile touch targets; record any specific blocker. Apply Impeccable while fixing demonstrated inconsistencies in owning files, after required ownership coordination.
- [ ] `ASI.Basecode.WebApp/Views/Certificates/CertificateTemplate.cshtml` and affected views/scripts — remove visible timezone wording and verify accessible labels and unchanged Manila-local values; coordinate certificate-template edits with its owner.
- [ ] `ASI.Basecode.Services/ServiceModels/AccountRequestQueueViewModel.cs`, `PagedResult.cs` and paging tests — verify and consolidate duplicated range calculations where semantics match; cover overflow, empty and clamped pages and ensure displayed rows match the effective page.
- [ ] `ASI.Basecode.Data/Repositories/`, `ASI.Basecode.Services/Services/` and focused integration tests — inventory list-query paths across the walked screens and remove genuinely equivalent duplicates, including course alias filtering if justified; preserve translation, ordering, filtering, paging and intentional snapshot differences.
- [ ] `ASI.Basecode.WebApp/wwwroot/js/site.js`, `features/record-dialog.js` and dialog tests — audit and eliminate only genuinely duplicated guard behavior; preserve async baselines, failed submissions, busy states, PDF cleanup and focus restoration.
- [ ] Existing service, integration and JavaScript test projects — alongside each change, add targeted regressions for its behavior and relevant matrix edge cases.
- [ ] `DESIGN.md`, `PRODUCT.md` and evidence record — after fixes, align documentation with shipped behavior, record review findings and explicit reasons for any unresolved items; then run the rendered critique, polish and final verification below.

**Acceptance Criteria:**
- Given each role and permitted screen, including each My Events tab and certificate viewer, when normal and relevant edge states are walked, then observations and findings are recorded without inferring passes from old evidence.
- Given the shared list and dialog paths, when filters, pages, focus or dirty forms are exercised, then the matrix behaviors hold, R37 list controls behave as specified, and no verified equivalent duplicate implementation remains.
- Given screens across roles, when controls, labels and statuses are compared, then vocabulary is consistent and intentional differences are recorded.
- Given a UI date or a newly rendered certificate, when a UTC timestamp is displayed, then the Manila-local value is unchanged and visible timezone labels are removed; previously issued PDFs remain unchanged.
- Given the rendered application, when viewed at 390px and 200% zoom and operated by keyboard, then controls, labels, states and focus remain usable and consistent with `DESIGN.md`.
- Given the finished changes, when critique, polish, build, CSS build and all three suites run, then all required checks pass; any blocker is recorded and keeps acceptance incomplete unless the owner changes the requirement. No new feature, rule or schema change is introduced.

## Implementation Notes

- 2026-11-18: Account-request queue range metadata delegates to `PagedResult`; service tests cover empty, clamped and large totals. The detailed source inventory and remaining gaps are recorded in `../../../whole-app-consistency-243.md`.
- Release build and CSS build passed; 411 service, 51 JavaScript and 656 SQL integration tests passed. These results precede any later edits and must be repeated after further changes.
- The full role walkthrough, screen-reader inspection, critique, polish, timezone wording and owner coordination remain incomplete. Existing local database was not reset or modified. Dependency installation reported three high-severity npm advisories; not yet assessed.

## Plan Change Log

- Owner requested removing the shared Philippine-time note too; show only the date and time while retaining the existing conversion and issued PDFs.

## Review Triage Log

## Design Notes

The application is an operational console; preserve its existing neutral workspace identity. Shared code should replace only equivalent implementations, not abstract away intentional differences in timing, focus, ordering or database consistency.

## Verification

- `dotnet build ASI.Basecode.sln --configuration Release` — succeeds.
- `npm run css:build --prefix ASI.Basecode.WebApp` and `npm run test:js --prefix ASI.Basecode.WebApp` — CSS builds and JavaScript tests pass.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` — service suite passes; use the built Release test host if this reports zero tests.
- With `ATTENDTA_TEST_SQL` set to the local master connection, `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` — full integration suite passes using its own disposable databases.
- Run Impeccable critique, polish and detector on changed surfaces; inspect every relevant role/screen at desktop, 390px and 200% zoom, recording keyboard, empty/error and long-content results.
