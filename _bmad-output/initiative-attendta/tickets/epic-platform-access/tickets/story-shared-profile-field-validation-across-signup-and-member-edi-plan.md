---
title: 'Shared profile field validation across signup and member edit'
type: 'feature'
ticket: '15'
created: '2026-11-15'
status: 'built'
baseline_revision: '22a1896366cf72c7504ab97ef65a70785527801d'
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

**Problem:** Signup and the Member editor accept any course, year level, section, student number, mobile and password, so one person can be stored as `BSIS`, year 5, section `3A`, `2024-001`, `555-0111` and the list filters disagree with the forms.

**Approach:** One rule set in `ProfileOptions` — course BSCS/BSIT, year 1–4, section A–F, student number exactly 8 ASCII digits, mobile empty or exactly 11 ASCII digits, password ≥8 characters with a letter and a digit. Enforce it on the form through model annotations, trim and upper-case in the service, and read the same constants from both forms and the list filters.

## Boundaries & Constraints

**Always:** One source of truth for the rules. Trim surrounding whitespace, never strip internal characters, never coerce a stored value. Identifiers stay strings so leading zeroes survive. A refused field shows its error on that control and keeps the safe input; passwords are cleared. Uniqueness checks keep working. No schema change.

**Never:** No schema or EF mapping change. No new ad-hoc allow-list check inside a service. No loosening of a rule to make a fixture pass. No database reset or reseed. No hard deletes. No PR against `dev`.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Valid signup | BSCS, year 2, section A, `23000123`, `09171234567`, `Applicant#2026` | Pending request created, no account | No error expected |
| Empty mobile | `MobileNumber` blank | Accepted, stored as NULL | No error expected |
| Year out of range | `YearLevel` 0 or 5 | Form redisplays | Error on YearLevel, input retained |
| Student number length | `1234567` or `123456789` | Form redisplays | Error on StudentNumber, input retained |
| Mobile shape | `0917-123-4567` or `0917123456` | Form redisplays | Error on MobileNumber, input retained |
| Section outside A–F | `G` or `3A` | Form redisplays | Error on Section, input retained |
| Unknown course | `BSIS` | Form redisplays | Error on Course, input retained |
| Weak password | `abcdefgh` or `short1` | Form redisplays | Error on Password, both passwords cleared |
| Crafted POST | Invalid values posted directly | Same field errors, no row written | ModelState invalid, 200 redisplay |
| Direct service call | Invalid value on the view model | `DomainException(Invalid)` naming the field | Thrown before any write |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Resources/Constants/ProfileOptions.cs` — `Courses` L7 (BSCS/BSIT), `NormalizeCourse` L10–15, `YearLevels` L18 (today 1–5), `Sections` L21 (A–F). No `NormalizeSection`, no patterns.
- `ASI.Basecode.Services/ServiceModels/AccountRequestViewModel.cs` — `StudentNumber` L11, `Course` L27 (already `^(BSCS|BSIT)$`), `YearLevel` L32 (`Range(1, int.MaxValue)`), `Section` L37, `MobileNumber` L42, `Password` L46, `ConfirmPassword` L51. Only Required/StringLength on the rest.
- `ASI.Basecode.Services/ServiceModels/MemberViewModel.cs` — `StudentNumber` L18, `Course` L25, `YearLevel` L29 (non-nullable `int`), `Section` L32, `MobileNumber` L36.
- `ASI.Basecode.Services/Services/AccountRequestService.cs` — `Normalize` L276–284 then `Validate` L286–289 (`Validator.TryValidateObject`, `validateAllProperties: true`) from `SubmitAsync` L72–73. `ApproveAsync` L167–175 feeds request values into `MemberViewModel`, so approval re-validates.
- `ASI.Basecode.Services/Services/MemberService.cs` — `Normalize` L227–238, `Validate` L240–243, called at L85–86 (create) and L136/L158 (update).
- `ASI.Basecode.WebApp/Views/AccountRequest/Request.cshtml` — year select from `ProfileOptions.YearLevels` L43–50; section is free text + datalist L54–64; student number L13–17 and mobile L65–69 lack `inputmode`/`maxlength`.
- `ASI.Basecode.WebApp/Views/Member/_Form.cshtml` — year L45–52, section free text + datalist L56–66, student number L15–19, mobile L73–77.
- `ASI.Basecode.WebApp/Models/ListToolbarViewModel.cs` L13–14 — course/year filters already read `ProfileOptions`; no change.
- Fixtures: `AccountRequestDecisionTests.cs:546` and `MemberManagementTests.cs:508` (`UniqueStudentNumber`), `AccountRequestSubmissionTests.cs:25,26,374`, `MasterDataTransactionTests.cs:70,98,277,376`, `AccountRequestServiceTests.cs:92,110,128,147,249,264,446,461`, `bruno/Members - Edit.bru:15,18`.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.Resources/Constants/ProfileOptions.cs` — `YearLevels` becomes `{1,2,3,4}`; add `StudentNumberPattern` `^[0-9]{8}$`, `MobileNumberPattern` `^[0-9]{11}$`, `SectionPattern` `^[A-Fa-f]$`, `PasswordPattern` `^(?=.*[A-Za-z])(?=.*[0-9]).{8,}$`; add `NormalizeSection` (trim, upper-invariant, blank→null) beside `NormalizeCourse`. The list filters in `ListToolbarViewModel` read `YearLevels`, so they follow automatically.
- [ ] `ASI.Basecode.Services/ServiceModels/AccountRequestViewModel.cs` — annotate the five fields with the patterns; `Range(1, 4, ErrorMessage = "Choose a year level from 1 to 4.")`.
- [ ] `ASI.Basecode.Services/ServiceModels/MemberViewModel.cs` — the same four, minus password.
- [ ] `ASI.Basecode.Services/Services/AccountRequestService.cs` and `MemberService.cs` — replace the bare `Section?.Trim()` with `ProfileOptions.NormalizeSection(...)` in both `Normalize` methods.
- [ ] `ASI.Basecode.WebApp/Views/AccountRequest/Request.cshtml` and `Views/Member/_Form.cshtml` — section becomes a `<select>` over `ProfileOptions.Sections` with an off-list preserved option (mirroring the year control); add `inputmode="numeric" maxlength="8"` to student number and `inputmode="numeric" maxlength="11"` to mobile. Reuse existing `field-control` classes so no new CSS is needed.
- [ ] Test fixtures — make every view-model fixture compliant: 8-digit student numbers starting with `2` via a static `Interlocked` counter based at `20000000` (real ids look like `23807613`, `22771903`, `21945028`; do not imply a `23` prefix), 11-digit mobiles, A–F sections. The five that must change: `MemberManagementTests.cs:124` `Section = "NEW-B"` → `"B"` (asserted at :144), `:125` `MobileNumber = "555-0111"` → `"09171234567"` (asserted at :145), `:275` `Section = "ROLLBACK"` → `"A"`, `MasterDataTransactionTests.cs:98` `MobileNumber = "555-0100"` → `"09171234567"`, and `:277,376` `UniqueName("SN")` → the 8-digit counter. Leave entity-only seeds (`RACE`, `Competitor course`, `Original course`) untouched. Assert the new year-level message where `YearLevel = 0` is already a rejection case.
- [ ] `tests/ASI.Basecode.Services.Tests/AccountRequestServiceTests.cs` and `AccountRequestSubmissionTests.cs` — add one rejection case per new rule (7- and 9-digit student number, dashed mobile, section `G`, year 5, weak password) plus a crafted-POST case asserting the error sits on the right control and the input is retained.
- [ ] `bruno/Members - Edit.bru` — student number 8 digits, section `B`.

**Acceptance Criteria:**
- Given the signup form, when BSCS/BSIT, year 1–4, section A–F, an eight-digit student number, an empty or eleven-digit mobile and a valid password are submitted, then a pending request is created.
- Given either form, when an invalid value is submitted, then the error renders on that control, the safe input is retained and passwords are cleared.
- Given a crafted POST bypassing the browser controls, when it carries invalid values, then the same field errors appear and nothing is written.
- Given an existing Member, when the editor saves a compliant profile, then the change is audited and the uniqueness checks still refuse a duplicate student number.

## Implementation Notes

- 2026-11-15, during the walkthrough: the owner asked for a Philippine mobile rule, so `MobileNumberPattern` became `^09[0-9]{9}$`, the field message became "Enter an 11-digit mobile number starting with 09.", and two rejection cases (an 11-digit number not starting with 09, and an `08` number) were added to the service theory. Committed as `5dbca04`. 342 service tests and 503 integration tests pass; every fixture already used a `09` number.

## Plan Change Log

## Review Triage Log

Quick lens, 2026-11-15. 12 findings, no `intent_gap` or `bad_plan`; one patch, two deferrals, nine rejected.

| # | Finding | Verdict | Route | Evidence |
|---|---|---|---|---|
| 1 | `MemberService.UpdateMemberAsync` validates at L158 inside the transaction, after `LockUserAsync`, so a crafted POST takes the row lock before the annotation refuses it | low | defer | Real: `Normalize` L136, `Validate` L158 inside `ExecuteInTransactionAsync`. Nothing is mutated and `MemberController.Edit` rejects on `ModelState` first. The ordering predates this ticket — the old `Required` rules failed at the same point. |
| 2 | `_Form.cshtml` section select has no empty option and renders a raw off-list stored value the annotations would reject | false | reject | `database/attendta-mvp-schema.sql:157` makes `members.section` NOT NULL and the canonical seed uses A–F, so a blank section cannot reach the editor. `SectionPattern` is `^[A-Fa-f]$`, so a stored lowercase `b` is accepted, not rejected. The missing empty option mirrors the year control in the same file, which is what the plan specified. |
| 3 | `Request.cshtml` year select lacks `class="field-control"` while the new section select has it | low | patch | Real: the diff added the class to section and left the sibling year select bare. `Styles/app.css:653-656` styles `.field select` and `.field-control` in one rule block, so the patch is visually a no-op. Patched. |
| 4 | The crafted-POST test's `value="G" selected` assertion depends on Razor attribute order and does not assert the absence of a second selected option | low | reject | The assertion does pass only because Razor writes attributes in source order, but the view guards the off-list option with `!Sections.Contains(Model.Section)`, so a second selected option is not reachable. Making the test parse the select and count `selected` attributes is more than a direct correction. |
| 5 | `InvalidFieldIsRejectedWithItsFieldName` was not extended, and its `Section = null` case exercises `Required`, not the pattern | false | reject | By design: `RegularExpression` treats null and empty as valid so `Required` owns emptiness. The new rules are covered by `ProfileValueOutsideTheSharedRulesIsRejectedWithItsFieldName` (`G`, `3A`, 7- and 9-digit numbers, dashed and 10-digit mobiles) and `WeakPasswordIsRejectedOnThePasswordField`. |
| 6 | `MasterDataTransactionTests` inline `MemberViewModel` has no `MobileNumber` and was left out of the sweep | false | reject | A null mobile is compliant: the rule is "empty, or exactly 11 ASCII digits", and `RegularExpression` passes null. Nothing to change. |
| 7 | Four `Interlocked` counters all start at `20000000`, so numbers collide across test classes | false | reject | Each test class creates and drops its own `AttendTa_IT_<guid>` database (`SqlServerDatabaseFixture`), so the counters never share a keyspace. The finding itself states it is safe today. |
| 8 | `bruno/Members - Edit.bru` cannot run: `auth: none`, no antiforgery token, stale port | low | defer | Real but pre-existing: the file has no sign-in step and no `__RequestVerificationToken`, and points at `https://localhost:51629`. This ticket only made the payload shape-compliant. |
| 9 | `PasswordPattern` `.{8,}` rejects a password containing a newline | low | reject | Real but unreachable: the signup field is a single-line `<input type="password">`, which cannot carry a newline, and refusing control characters in a password is desirable. Widening the pattern is more than a direct correction. |
| 10 | `SectionPattern` accepts lowercase but `NormalizeSection` upper-cases, so the lowercase branch is dead on the service path | false | reject | The lowercase branch is load-bearing on the MVC path, which validates the raw posted value before the service normalizes — the finding says so itself. No bad outcome. |
| 11 | The off-list preserved option renders the raw model value with no length bound in the view | false | reject | `StringLength(50)` bounds it and `members.section` holds A–F. Refusing an out-of-set value and showing the error on submit is the intended behaviour, not a defect. |
| 12 | A whitespace-only mobile (`"   "`) is refused by MVC before `Normalize` can turn it into NULL | low | reject | Real but uncommon: the form returns "Enter an 11-digit mobile number" on its own control. The service path the plan's matrix and test cover stores NULL. Closing it needs either a weakened shared pattern or model-binding machinery — more than a direct correction. |

## Design Notes

**Enforcement rule.** The view-model annotations are the single enforcement point. They run through MVC for a form post and through `Validator.TryValidateObject` for a direct service call, so both paths refuse the same values. A fixture that builds a view model must use compliant values. A fixture that inserts entities directly keeps whatever values it needs to prove its race.

`RegularExpression` treats null and empty as valid, which is exactly the "empty mobile is allowed" rule. `SectionPattern` accepts lower case and the service upper-cases it, so `b` and `B` both land as `B`.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — expected: build succeeds with no new warnings.
- `node --test ASI.Basecode.WebApp/tests/*.test.js` — expected: all pass.
- `& "tests\ASI.Basecode.Services.Tests\bin\Debug\net10.0\ASI.Basecode.Services.Tests.exe"` — expected: all pass.
- `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` with `ATTENDTA_TEST_SQL` pointing at a local `master` — expected: all pass.

## Code Review

### 2026-11-15

**Code review complete.** 1 `decision-needed`, 4 `patch`, 1 `defer`, 25 rejected. Lenses: blind-hunter, edge-case-hunter, verification-gap, intent-alignment.

- [x] [Review][Decision] `maxlength` silently truncates a pasted value [ASI.Basecode.WebApp/Views/AccountRequest/Request.cshtml:149,186; Views/Member/_Form.cshtml:19,77] — **resolved 2026-11-15: keep `maxlength` (option a).** The dashed example was the old fixture format (`2023-01234`, `2024-001`), not a user scenario; real ids are plain eight digits (`23806132`), which fit the box exactly. No code change.
- [x] [Review][Patch] The Member editor's rules were unproven at both the service and the form — fixed in `f765173`
- [x] [Review][Patch] `NormalizeSection` upper-casing unproven on the member path — fixed in `f765173`
- [x] [Review][Patch] The student-number message differed between the two view models — fixed in `f765173`
- [x] [Review][Patch] The password rule's accept side was unpinned at the boundary — fixed in `f765173`
- [x] [Review][Defer] `UpdateMemberAsync` validates inside the transaction after taking the user lock [ASI.Basecode.Services/Services/MemberService.cs:158] — deferred: pre-existing ordering, already recorded in `deferred-work.md` during the walkthrough.

**Rejected:** dead `StringLength` attributes (harmless guard); `SectionPattern` lowercase (required by the MVC path); year-5 filter chip gone (intended); `selected` idiom inconsistency (removing it risks the retention tests); duplicated counters (a shared helper is a new file); counter collisions (per-class databases); queue-test change (required — approval re-validates); shared mobile literal (no uniqueness rule); `RACE`/`555-0100` entity seeds (never touch a view model); deferred-work plan path (resolves on the docs branch); Bruno payload edit (correct for when the harness is fixed); no `ProfileOptions` test (covered by services and a live probe); whitespace-only section/mobile (Required owns section, blank mobile is intended); legacy student numbers, year 5 and pre-rule pending requests (inventory: 0 offenders); demo seed (compliant: `23184502`, `09170000101`); signup section placeholder (present); member section placeholder (`section` is NOT NULL); ordinal section comparison (retention, no duplicate); non-nullable year level (no stored year 5); year filter option set (intended); `AuthenticationFlowTests` seeds (entity-only); course pattern not in `ProfileOptions` (owner accepted it in the walkthrough); no section filter in the toolbar (never existed).
