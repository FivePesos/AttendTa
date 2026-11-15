# Walkthrough — Shared profile field validation (issue #238, PR #245)

Status: Blocks: 1-7 accepted. Review complete 2026-11-15.

## Block 1 — Intent ✓

Accepted 2026-11-15.

# Shared profile field validation across signup and member edit

## Description

Enforce one profile rule set on the Account Request form, the Member editor and their services: course BSCS or BSIT, year level 1 to 4, section A to F, student number exactly eight ASCII digits, mobile empty or exactly eleven ASCII digits, and a password of at least eight characters with a letter and a digit. Put the shared choices and patterns in ProfileOptions so signup, member editing and the list filters read one source. Trim surrounding whitespace, reject internal spaces and punctuation, never silently strip characters, and keep identifiers as strings so leading zeroes survive. Enforce the rules on the form through model annotations and normalize in the service; do not reject arbitrary values at the service boundary, because the concurrency tests post them on purpose. Keep field errors and safe input on failure while clearing passwords. Leave the database schema unchanged.

## Requirements

Verify: Signup and the Member editor accept BSCS/BSIT, years 1-4, sections A-F, an eight-digit student number, an empty or eleven-digit mobile and a valid password; it refuses year 0 and 5, a seven- or nine-digit student number, a dashed or spaced mobile, a section outside A-F, an unknown course and a weak password, with the field error on the right control and the input retained. A crafted POST and a direct service call are refused by the same rules, and the existing uniqueness checks still work.

Source: issue #238 body, pasted verbatim.

## Block 2 — Broad strokes ✓

Accepted 2026-11-15. Diagram: [block-2-mechanism.md](block-2-mechanism.md).

Five places, in reading order. Open them in this order and the mechanism is complete.

1. [ProfileOptions.cs](../../../ASI.Basecode.Resources/Constants/ProfileOptions.cs) — the single source: the program list, year levels 1–4, sections A–C, four regex patterns and the section normalizer.
2. [AccountRequestViewModel.cs](../../../ASI.Basecode.Services/ServiceModels/AccountRequestViewModel.cs) — the signup annotations that consume those constants; this is the refusal point.
3. [MemberViewModel.cs](../../../ASI.Basecode.Services/ServiceModels/MemberViewModel.cs) — the same set for the member editor, minus the password rule.
4. [Request.cshtml](../../../ASI.Basecode.WebApp/Views/AccountRequest/Request.cshtml) and [_Form.cshtml](../../../ASI.Basecode.WebApp/Views/Member/_Form.cshtml) — the controls that make an out-of-rule value hard to type in the first place.
5. [AccountRequestService.cs](../../../ASI.Basecode.Services/Services/AccountRequestService.cs) and [MemberService.cs](../../../ASI.Basecode.Services/Services/MemberService.cs) — the one-line change that upper-cases the section on the way in.

In one line: the rules live as constants, the view models carry them as annotations, MVC enforces them on the form post, and the services keep their existing job of tidying and checking uniqueness — they do not become a second gatekeeper.

## Block 3 — One rule set, two view models ✓

Accepted 2026-11-15 after a test run (44 + 14 + 22 tests green).

The rule set is now data rather than four copies of a string literal: `ProfileOptions` owns the patterns and both view models reference them by name. That shared reference is the whole point of the ticket — signup and the member editor cannot drift apart.

- Student number `^[0-9]{8}$`, mobile `^09[0-9]{9}$`, section `^[A-Ca-c]$`, password `^(?=.*[A-Za-z])(?=.*[0-9]).{8,}$` — [ProfileOptions.cs](../../../ASI.Basecode.Resources/Constants/ProfileOptions.cs)
- Year levels drop from 1–5 to 1–4 in the same file, which is also what the list filters read.
- Signup applies all four patterns plus the password rule — [AccountRequestViewModel.cs:13](../../../ASI.Basecode.Services/ServiceModels/AccountRequestViewModel.cs)
- The member editor applies the same set minus password — [MemberViewModel.cs:20](../../../ASI.Basecode.Services/ServiceModels/MemberViewModel.cs)
- `[Range(1, 4)]` replaces `[Range(1, int.MaxValue)]` on both, so year 0 and year 5 now fail with "Choose a year level from 1 to 4."

Because `RegularExpressionAttribute` treats null and empty as valid, an empty mobile still passes — that is the "optional" half of the rule, and it is why no separate conditional check was needed.

## Block 4 — The forms make the rules hard to break by typing ✓

Accepted 2026-11-15 after the drive. During the drive the product owner narrowed sections to A–C and membership types to Regular/Developer; both are outside ticket #238.

The annotations are the refusal; the controls are the prevention. Student number and mobile gained `inputmode="numeric"` and a `maxlength` that matches the rule, so a phone keyboard offers digits and the field cannot hold a ninth digit.

The larger change is the section field: a free-text input with a `<datalist>` of suggestions became a real `<select>` built from `ProfileOptions.Sections`, in both forms.

- Signup section select — [Request.cshtml:55](../../../ASI.Basecode.WebApp/Views/AccountRequest/Request.cshtml)
- Member editor section select — [_Form.cshtml:56](../../../ASI.Basecode.WebApp/Views/Member/_Form.cshtml)

Both selects keep a legacy escape hatch: if the stored section is outside A–C, it is appended as a selected option. Editing an old record therefore shows the value that is actually stored instead of silently snapping the form to the first letter.

## Block 5 — Normalize in the service, do not reject ✓

Accepted 2026-11-15.

One line changes in each service: `Section` goes through `ProfileOptions.NormalizeSection` instead of a bare `Trim()`. It trims, upper-cases and collapses a blank to null.

- [AccountRequestService.cs:282](../../../ASI.Basecode.Services/Services/AccountRequestService.cs)
- [MemberService.cs:232](../../../ASI.Basecode.Services/Services/MemberService.cs)
- The normalizer itself — [ProfileOptions.cs](../../../ASI.Basecode.Resources/Constants/ProfileOptions.cs)

What the services do *not* do is carry a second rule set. Both call `Validator.TryValidateObject` over the same annotated view models, so a direct service call is refused by exactly the rules the form uses — that is the "refused by the same rules" half of the ticket. The concurrency and rollback tests used to post `Section = "RACE"` / `"ROLLBACK"` to prove locking; those fixtures now post in-rule values, so they still prove the lock and the rollback without arguing with the annotations. Because the section normalizer runs before validation, a lower-case `b` from a direct caller is stored as `B` rather than refused.

## Block 6 — Tests: compliant fixtures, new refusal cases ✓

Accepted 2026-11-15.

Most of the diff is fixture repair, because any test that posts a form now fails model validation before it reaches the behaviour it was written for.

- The student-number helpers changed shape: `2026-{guid}` and `CORRECTED-{guid}` became an `Interlocked` counter starting at 20000000, and the dashed literals (`2023-01234`, `2023-09999`, …) became eight digits. One worked example — [AccountRequestDecisionTests.cs:547](../../../tests/ASI.Basecode.IntegrationTests/AccountRequestDecisionTests.cs).
- New refusal coverage on the service path: a theory over student number, mobile and section values, year 5, and three weak passwords, each asserting the outcome, the field name and that nothing was saved — [AccountRequestServiceTests.cs](../../../tests/ASI.Basecode.Services.Tests/AccountRequestServiceTests.cs).
- New refusal coverage on the member path: the same theory over `UpdateMemberAsync`, a year-5 case, and a lower-case section that must store upper-cased — [MemberManagementTests.cs:329](../../../tests/ASI.Basecode.IntegrationTests/MemberManagementTests.cs).
- The end-to-end case is a crafted POST to the signup form with five bad values at once, asserting each error lands on its own control, the safe input is retained and the password is not echoed back — [AccountRequestSubmissionTests.cs:263](../../../tests/ASI.Basecode.IntegrationTests/AccountRequestSubmissionTests.cs).

## Block 7 — Periphery ✓

Accepted 2026-11-15.

- Two deferrals recorded rather than fixed: member validation runs inside the transaction after the user lock is taken, and the Bruno edit request still cannot run (no antiforgery token, stale port) — [deferred-work.md](../../../_bmad-output/initiative-attendta/deferred-work.md).
- The Bruno payload was made shape-compliant (`20240001`, section `B`) — [Members - Edit.bru](../../../bruno/Members%20-%20Edit.bru).
- No schema change and no migration: the columns already hold the target values.

## Outcome

The review found 17 deduplicated issues; 4 decisions were made, 16 patches applied, 1 deferred, 4 rejected. Build clean in Debug and Release; 345 service tests and 516 integration tests passed before the product owner's later edits, and 345 plus 51 affected integration tests passed after. The product owner then narrowed membership types to Regular/Developer and sections to A–C, which are outside ticket #238 and still uncommitted.
