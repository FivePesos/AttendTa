---
title: 'Shared profile field validation across signup and member edit'
type: 'feature'
ticket: '15'
created: '2026-11-15'
status: 'draft'
baseline_revision: '22a1896366cf72c7504ab97ef65a70785527801d'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: ''
review_source: ''
lenses_ran: []
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
- [ ] Test fixtures — make every view-model fixture compliant: 8-digit student numbers via a static `Interlocked` counter from `23000000`, 11-digit mobiles, A–F sections. Leave entity-only seeds (`RACE`, `Competitor course`, `Original course`) untouched. Assert the new year-level message where `YearLevel = 0` is already a rejection case.
- [ ] `tests/ASI.Basecode.Services.Tests/AccountRequestServiceTests.cs` and `AccountRequestSubmissionTests.cs` — add one rejection case per new rule (7- and 9-digit student number, dashed mobile, section `G`, year 5, weak password) plus a crafted-POST case asserting the error sits on the right control and the input is retained.
- [ ] `bruno/Members - Edit.bru` — student number 8 digits, section `B`.

**Acceptance Criteria:**
- Given the signup form, when BSCS/BSIT, year 1–4, section A–F, an eight-digit student number, an empty or eleven-digit mobile and a valid password are submitted, then a pending request is created.
- Given either form, when an invalid value is submitted, then the error renders on that control, the safe input is retained and passwords are cleared.
- Given a crafted POST bypassing the browser controls, when it carries invalid values, then the same field errors appear and nothing is written.
- Given an existing Member, when the editor saves a compliant profile, then the change is audited and the uniqueness checks still refuse a duplicate student number.

## Implementation Notes

## Plan Change Log

## Review Triage Log

## Design Notes

The service already validates the view model with `Validator.TryValidateObject(validateAllProperties: true)`, so the annotations are the single enforcement point for both the form and a direct service call; no extra service check is added. `RegularExpression` treats null and empty as valid, which is exactly the "empty mobile is allowed" rule. `SectionPattern` accepts lower case and the service upper-cases it, so `b` and `B` both land as `B`.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — expected: build succeeds with no new warnings.
- `node --test ASI.Basecode.WebApp/tests/*.test.js` — expected: all pass.
- `& "tests\ASI.Basecode.Services.Tests\bin\Debug\net10.0\ASI.Basecode.Services.Tests.exe"` — expected: all pass.
- `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` with `ATTENDTA_TEST_SQL` pointing at a local `master` — expected: all pass.
