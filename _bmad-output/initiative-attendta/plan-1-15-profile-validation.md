# 1.15 — Shared profile field validation: implementation plan

Prepared 2026-11-15. Ticket: `epic-platform-access` entry 1.15. Covers R1, R5, R37.
Source of the rules: `prd-attendta.md` FR-1 and FR-5, section 9.

## The rule set

| Field | Rule | Where it is enforced |
|---|---|---|
| Course | `BSCS` or `BSIT` | View model attribute; service normalizes legacy labels |
| Year level | 1–4 | View model range; the form offers 1–4 |
| Section | A–F | View model attribute; service upper-cases |
| Student number | exactly eight ASCII digits | View model attribute |
| Mobile | empty, or exactly eleven ASCII digits | View model attribute |
| Password | at least eight characters with a letter and a digit | View model attribute |

Trim surrounding whitespace before validating. Never strip internal characters.

## The decision that shapes the work

**Enforce on the form, normalize in the service. Do not reject at the service boundary.**

The integration suite deliberately posts values like `Section = "RACE"`, `"ROLLBACK"`, `"NEW-B"` and `Course = "Competitor course"` to prove concurrency and rollback behaviour. A service-boundary rejection would fail those tests for the wrong reason and hide the race they exist to prove. The form is the user's door; the service keeps its current normalize-and-uniqueness job.

## Files to change

1. `ASI.Basecode.Resources/Constants/ProfileOptions.cs`
   - `YearLevels` becomes `{ 1, 2, 3, 4 }` (currently 1–5).
   - Add `const string` patterns so the attributes and any future check read one source:
     `StudentNumberPattern = "^[0-9]{8}$"`, `MobileNumberPattern = "^[0-9]{11}$"`,
     `SectionPattern = "^[A-Fa-f]$"`, `PasswordPattern = "^(?=.*[A-Za-z])(?=.*[0-9]).{8,}$"`.
   - Add `NormalizeSection(string?)` that trims and upper-cases, beside `NormalizeCourse`.
2. `ASI.Basecode.Services/ServiceModels/AccountRequestViewModel.cs`
   - Student number: add `[RegularExpression(ProfileOptions.StudentNumberPattern, ErrorMessage = "Enter an 8-digit student number.")]`.
   - Year level: `[Range(1, 4, ErrorMessage = "Choose a year level from 1 to 4.")]`.
   - Section: add `[RegularExpression(ProfileOptions.SectionPattern, ErrorMessage = "Choose a section from A to F.")]`.
   - Mobile: add `[RegularExpression(ProfileOptions.MobileNumberPattern, ErrorMessage = "Enter an 11-digit mobile number without spaces or dashes.")]`.
   - Password: add `[RegularExpression(ProfileOptions.PasswordPattern, ErrorMessage = "Use at least 8 characters with a letter and a digit.")]`.
3. `ASI.Basecode.Services/ServiceModels/MemberViewModel.cs` — the same four attributes minus password.
4. `ASI.Basecode.Services/Services/AccountRequestService.cs` — in the normalize path, `model.Section = ProfileOptions.NormalizeSection(model.Section)`.
5. `ASI.Basecode.Services/Services/MemberService.cs` — the same section normalization.
6. `ASI.Basecode.WebApp/Views/AccountRequest/Request.cshtml` and `Views/Member/_Form.cshtml` — confirm the controls carry `inputmode`, `maxlength` and the section select; the year select must read `ProfileOptions.YearLevels`.

## Fixture inventory (the bulk of the work)

Form-posting tests fail model validation until these change:

- `AccountRequestDecisionTests.cs:546` — `UniqueStudentNumber()` returns `2026-{guid}`. Make it eight digits, e.g. a static counter starting at `23000000` incremented with `Interlocked`.
- `MemberManagementTests.cs:508` — `UniqueStudentNumber()` returns `CORRECTED-{guid}`. Same treatment.
- Literals to convert to eight digits, keeping each distinct: `2023-01234`, `2023-00001`, `2023-08888`, `2023-09999`, `2024-0009`, `IT-10001`, `S-0001`, `SN-7`, `UI-APPLICANT`, and the trim case `"  2023-01234  "`.
- Section literals that reach a form: `"NEW-B"`, `"RACE"`, `"ROLLBACK"`, `"DemoData"`. Check each one — if it posts through the view model it must become A–F; if it calls the service directly it must stay.
- `Course = "BSIS"`, `"Computer Science"`, `"Computing"`, `"Original course"`, `"Competitor course"` — same test: form or service.
- `YearLevel = 0` is a deliberate rejection case; keep it and assert the new message.

## Order

1. Constants and view models. Build.
2. Section normalization in both services. Build.
3. Fixtures, one test class at a time, running that class after each change.
4. Full service suite, then the integration suite with `ATTENDTA_TEST_SQL` pointing at a local `master`.
5. Regenerate CSS only if a view changed.

## Verification

```powershell
dotnet build ASI.Basecode.sln --configuration Release
node --test ASI.Basecode.WebApp/tests/*.test.js
& "tests\ASI.Basecode.Services.Tests\bin\Debug\net10.0\ASI.Basecode.Services.Tests.exe"
dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none
```

Acceptance is the ticket's Verify line: signup and the Member editor accept BSCS/BSIT, years 1–4, sections A–F, an eight-digit student number, an empty or eleven-digit mobile and a valid password; each invalid case is refused with the error on the right control and the input retained.

## Not in scope

No schema change. No correction screen for legacy values — Q3 is closed by reset. No service-boundary rejection of arbitrary sections or courses.

## Code Review

### 2026-11-15

**Summary:** 4 decision-needed, 12 patch, 1 defer, 4 rejected — shared profile field validation (PR #245, diff `08cdf10..df09f00`).

- [x] [Review][Patch] Pre-change rows cannot pass the new rules and no inventory or correction path exists (decided: option 1 — inventory the affected rows read-only, then add a correction affordance before enforcing) — approval feeds stored request values through `MemberViewModel` validation (`AccountRequestService.cs:167-175`, `MemberService.cs:85-86`), so a pending request holding `2023-01234` or section `3A` can never be approved; editing a member whose stored student number, mobile, section or year level predates the rules is refused until the officer rewrites that data, and the section escape hatch only displays it. The ticket's Notes required a read-only inventory and a chosen correction path; this plan instead assumed Q3 was "closed by reset", which the ticket forbids. Options: 1. Inventory the affected rows read-only, then add a correction affordance before enforcing (the ticket's own instruction). 2. Exempt an unchanged stored value on the member edit path and normalize stored values when approving. 3. Repair the data with a script, then enforce. Recommended: 1, with 2's unchanged-value exemption so an unrelated edit is not blocked — the ticket names the inventory and it is the only option that shows the operator what will break.
- [x] [Review][Patch] Mobile requires a `09` prefix, contradicting the plan and the cited handoff (decided: option 1 — keep the `09` rule and amend the ticket and handoff) — `MobileNumberPattern` is `^09[0-9]{9}$` (`ProfileOptions.cs:27`), while `docs/handoffs/ui-ux-backend-alignment-handoff.md:65` says verbatim "Do not require a mobile prefix such as `09`; that was not requested", and the field message differs from the handoff's intended copy at `:58`. Options: 1. Keep the `09` rule and amend the ticket and handoff. 2. Revert to `^[0-9]{11}$` and restore the handoff message. Recommended: 1 — a Philippine mobile number starts with 09 and the tightening is deliberate (commit `5dbca04`); the documents are what is out of date.
- [x] [Review][Patch] `maxlength` silently truncates pasted values (decided: option 1 — drop `maxlength`, keep `inputmode`, let the regex refuse) — a pasted nine-digit student number becomes eight digits and is accepted as a different number; a pasted dashed mobile is truncated and then refused (`Views/AccountRequest/Request.cshtml:15,69`, `Views/Member/_Form.cshtml:17,78`). The ticket says never silently strip characters. Options: 1. Drop `maxlength`, keep `inputmode`, let the regex refuse. 2. Keep `maxlength` and add a paste handler that refuses instead of truncating. Recommended: 1 — one attribute removed, and the refusal comes from the rule that already exists.
- [x] [Review][Patch] No test approves a pre-change pending request (decided: option 2 — assert a specific `Invalid` outcome and field) — every approval fixture is in-rule, so the break above has no failing test (`AccountRequestDecisionTests.cs`, `AccountRequestQueueTests.cs`). Options: 1. Assert approval succeeds after the chosen correction path. 2. Assert a specific `Invalid` outcome and field. 3. Leave it to the chosen implementation. Recommended: 1 if the correction path normalizes stored values, otherwise 2 — the test should pin whichever behaviour the decision above selects.

- [x] [Review][Patch] Course and year rules are literals, not ProfileOptions [ASI.Basecode.Services/ServiceModels/AccountRequestViewModel.cs:30,36]
- [x] [Review][Patch] No test that the member editor renders the stored section selected [tests/ASI.Basecode.IntegrationTests/MemberManagementTests.cs:420-430]
- [x] [Review][Patch] No test that the year filter offers 1–4 [tests/ASI.Basecode.IntegrationTests/ListPresentationTests.cs:85]
- [x] [Review][Patch] The member-edit form test is weaker than the signup one — page-wide message grep, vacuous mobile row, one field of four [tests/ASI.Basecode.IntegrationTests/MemberManagementTests.cs:331,361,476-489]
- [x] [Review][Patch] Section select renders a duplicate off-list option for a lower-case stored value [ASI.Basecode.WebApp/Views/Member/_Form.cshtml:61]
- [x] [Review][Patch] Dead `[StringLength]` annotations with contradictory messages [ASI.Basecode.Services/ServiceModels/AccountRequestViewModel.cs:14,42,47]
- [x] [Review][Patch] Password has no upper bound [ASI.Basecode.Resources/Constants/ProfileOptions.cs:33]
- [x] [Review][Patch] Member form control polish — selectable year 0, no Section placeholder, two selection mechanisms, signup inputs without the shared control class [ASI.Basecode.WebApp/Views/Member/_Form.cshtml:45-67]
- [x] [Review][Patch] Docs stale — year 1–5 / positive year level / no backend validation [docs/ui-ux-implementation-plan.md:274,279-283]
- [x] [Review][Patch] Test fixture hygiene — four copies of the student-number seed helper, implicit eight-digit contract, no constants-consistency guard [tests/ASI.Basecode.IntegrationTests/MemberManagementTests.cs:606]
- [x] [Review][Patch] Intent properties stated but untested — leading zeroes, plausible wrong course [tests/ASI.Basecode.Services.Tests/AccountRequestServiceTests.cs]
- [x] [Review][Patch] The walkthrough narrative misdescribes the mechanism — it says the services do not reject out-of-rule values; both validate the annotations and do reject [_bmad-output/initiative-attendta/walkthrough-pr-245-profile-validation/walkthrough-pr-245-profile-validation.md]

- [x] [Review][Defer] Service validation runs after the row lock [ASI.Basecode.Services/Services/MemberService.cs:136,158] — deferred: pre-existing ordering, already recorded in `deferred-work.md` by this change.

**Rejected:**

- `false` — ConfirmPassword carries no rule: the requirement is a valid password with a matching confirmation; a weak pair is already refused on the Password control.
- `low` — `ListReadOptionsFactory` accepts year 1–10 while the filter offers 1–4: the dropdown cannot produce 5, and a hand-typed `?year=5` only returns legacy rows, so no user is harmed.
- `low` — Bruno edit request still cannot succeed: already recorded in `deferred-work.md` as a request that cannot run at all (no antiforgery token, stale port); the payload details change nothing.
- `false` — Section case widened then rewritten: the plan states the upper-casing and a test pins it; normalization is not stripping.
