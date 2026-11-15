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
