---
title: 'Prevent misleading unchanged member saves'
type: 'bugfix'
ticket: ''
created: '2026-11-17'
status: 'built'
baseline_revision: '989b62aa9ed529fcb0ea4d2fed01ae6c411429de'
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

**Problem:** Saving an unchanged member currently reports an update, changes modification metadata and writes an audit entry even though the operator made no effective change.

**Approach:** Distinguish a genuine no-op from an actual member edit in both the shared dialog and standalone form. Prevent no-op writes and false audit entries at the service boundary, and give honest feedback while retaining normal edit and validation behavior. Investigate related member-edit defects and report any out-of-scope issues.

## Boundaries & Constraints

**Always:** Preserve management authorization, antiforgery, existing validation, transactional audit, student-number correction reason, uniqueness and member status controls. Compare against freshly loaded persisted data under the existing lock. Treat semantically equivalent values after existing normalization as unchanged; do not silently rewrite legacy storage on a no-op.

**Never:** Change membership rules, broaden supported legacy data or add unrelated features. Do not claim stale edit conflicts are solved without a concurrency mechanism.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| No change | Valid submitted values match normalized current record | Explain no changes; no metadata update, audit row or database update | Preserve existing authorization and existence checks |
| Normalized equivalent | Whitespace or a supported course alias normalizes to current value | Same no-op outcome; preserve raw stored values | Do not treat a real name/field change as unchanged |
| Actual edit | One or more editable fields differ | Persist change and one truthful audit event; show update success | Existing validations, uniqueness, correction reason and atomic rollback remain |
| Invalid or missing member | Invalid model, missing record or forbidden actor | Existing appropriate error; never report update | Preserve input and avoid writes |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/Services/MemberService.cs` — `UpdateMemberAsync` normalizes, acquires the user lock, validates then unconditionally mutates and audits; `ToViewModel` normalizes course. Compare editable values under the lock before mutation; distinguish outcomes.
- `ASI.Basecode.Services/Interfaces/IMemberService.cs` and implementations/callers — update contract and test doubles consistently if an explicit outcome is required.
- `ASI.Basecode.WebApp/Controllers/MemberController.cs` — `Edit` currently reports success for every service return; preserve JSON dialog and standalone flows but distinguish no-op.
- `ASI.Basecode.WebApp/Views/Member/_Form.cshtml`, `_Editor.cshtml`, and `ASI.Basecode.WebApp/wwwroot/js/features/record-dialog.js` — existing editor, server re-render and dialog mechanisms; avoid weakening validation or breaking accessible error/focus behavior.
- `tests/ASI.Basecode.IntegrationTests/MemberManagementTests.cs` and member controller/service tests — existing real edits, correction reasons, audit rollback and locking; extend rather than replace.
- `ASI.Basecode.Resources/Constants/ProfileOptions.cs` — existing normalization and supported legacy aliases; apply consistently without writing storage on no-op.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Services/Services/MemberService.cs` and `ASI.Basecode.Services/Interfaces/IMemberService.cs` — implement normalized no-op detection within the existing transaction and expose a clear result; update affected consumers and test doubles.
- [x] `ASI.Basecode.WebApp/Controllers/MemberController.cs` and member editor views/scripts where needed — communicate no changes without misleading success for dialog and standalone paths, retaining input and normal success.
- [x] `tests/ASI.Basecode.IntegrationTests/MemberManagementTests.cs` and focused service/controller/JavaScript tests — cover each matrix case, metadata/audit invariants and dialog/full-page behavior.
- [x] Run scoped UI critique and polish; record related findings without broadening the change. Actual screen-reader announcements and visual screenshots remain unverified.

**Acceptance Criteria:**
- Given an unchanged valid member, when Save is selected in the dialog or standalone editor, then the result states no changes were made and neither member/user modification metadata nor audit history is changed.
- Given a supported legacy course alias or other normalized-equivalent input, when submitted without an effective change, then stored bytes and audit history remain unchanged.
- Given an actual valid edit, when saved, then the change persists and exactly the appropriate existing audit event is recorded.
- Given invalid, duplicate, forbidden or missing data, when submitted, then the established error behavior remains and no partial changes are recorded.

## Implementation Notes

Related observations to investigate without treating them as established bugs: legacy values outside current validation may block editing; stale open forms may overwrite later edits. Report evidence and avoid adding a concurrency feature without an explicit request.

## Plan Change Log

## Review Triage Log

- Investigation fixes: revalidation normalizes before model validation; raw equivalent student number is preserved on unrelated edits; edit-specific locked refresh preserves shared repository behavior and refuses to discard unsaved changes. Focused regressions pass.
- UI critique: no-change notice could be obscured behind mobile header and may not be announced; mobile visibility and focus association added and covered in browser tests. Actual screen-reader announcement remains unverified.
- Quick review: medium, patched — normalized membership type could be lost during a validation re-render because ModelState retained the raw attempted select value; it now uses the normalized value and standalone/dialog regression cases passed.

## Verification

- `dotnet build ASI.Basecode.sln --configuration Release`
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release`
- `$env:ATTENDTA_TEST_SQL='Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True'; dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -class ASI.Basecode.IntegrationTests.MemberManagementTests -parallelMode none`
- Run focused service, member integration and applicable JavaScript tests once after the build; add exact commands before execution.
- Inspect member dialog and standalone flows, no-op, edit, invalid data and keyboard/focus behavior on desktop and mobile.
- Final Release build passed, 0 warnings/errors; service tests 405/405; JavaScript tests 51/51; focused LocalDB MemberManagementTests 24/24, including desktop/mobile browser notice checks. Full SQL suite not rerun. Screen-reader verification remains outstanding.
- After the quick-review patch: Release build passed with zero warnings/errors; focused member integration tests passed 24/24; CSS build passed. The change was limited to the member controller and its regression test; service and JavaScript suites passed immediately before that patch.
- Scoped polish inspected source and headless browser geometry/focus tests at 390px and 1440px; visual screenshots, zoom and actual assistive-technology announcements remain unverified.
