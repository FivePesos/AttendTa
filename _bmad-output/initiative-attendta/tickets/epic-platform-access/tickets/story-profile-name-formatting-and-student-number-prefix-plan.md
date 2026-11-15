---
title: 'Profile name formatting and student-number prefix'
type: 'feature'
ticket: '17'
created: '2026-11-15'
status: 'built'
baseline_revision: 'f765173d1d9d953e6a3ed7cbfdbfdbceb8b57646'
route: 'oneshot'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: []
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** A student number can start with any digit, and a name typed in capitals is stored and shown in capitals — `SETH PINCA` stays `SETH PINCA` on every screen.

**Approach:** Tighten the student-number pattern to `^2[0-9]{7}$` with a message that says so. Normalise a name on save the way the section is normalised today: collapse repeated spaces, capitalise the first letter of every word, lower-case the rest, keep initials (`A.`), hyphens and apostrophes. Store the result, so no display formatter and no backfill are needed; rows written before this change keep whatever they hold.

</frozen-after-approval>

## Implementation Notes

Second commit on `feature/1-17-profile-name-formatting`, stacked on 1.15 because it edits `ProfileOptions.StudentNumberPattern`, which 1.15 introduced.

`NormalizeName` goes beside `NormalizeSection` in `ProfileOptions`: split on whitespace with `StringSplitOptions.RemoveEmptyEntries`, then walk the string capitalising the first character of each word and lower-casing the rest, treating space, `-` and `'` as word boundaries. That yields `Seth Pinca`, `John Paul A. Singco`, `Maria Dela Cruz`, `Anne-Marie Lopez`, `O'Neil Santos`, and accepts `McDonald` becoming `Mcdonald`.

Both `Normalize` methods call it for `FullName` in place of the bare `Trim()`: `AccountRequestService.cs:279` and `MemberService.cs:229`. That covers every write — a new request, a member edit (`user.Name` at `MemberService.cs:175`) and approval (`Name = request.FullName` at `AccountRequestService.cs:156`, already normalised at submission). No other code assigns `users.name`.

The student-number message becomes "Enter an 8-digit student number starting with 2." on both view models. Existing fixtures already start with `2`.

The review found one write the rule did not cover: `ApproveAsync` set `users.name` straight from `request.FullName` (`AccountRequestService.cs:156`). A request created before this change would carry its original capitals into the new account. Now wrapped in `NormalizeName`.

Known limitation, accepted: an intentional internal capital is lost, so `McDonald` becomes `Mcdonald`. Roman-numeral suffixes are handled — `JOHN PAUL II` stays `John Paul II` — and `Jr.` is unaffected. A token containing a digit is unaffected in practice (`ANA 3RD` → `Ana 3rd`). All three behaviours are pinned by test cases.

## Review Triage Log

| Finding | Verdict | Evidence |
|---|---|---|
| `ApproveAsync` wrote `users.name` from the raw request value, bypassing the formatter for a request created before this change | medium | Real: `AccountRequestService.cs:156`. Unreachable today (0 pending requests) but it is the one name write outside the rule. Fixed by wrapping it in `NormalizeName`. |
| Roman-numeral suffixes render as `Ii` | low | Real, and fixed on request: a six-entry suffix list (`II`, `III`, `IV`, `V`, `VI`) keeps them upper-case, so `JOHN PAUL II` stays `John Paul II` while `Jr.` is untouched. Pinned by test cases. |

### Formal review, 2026-11-15

Four lenses (blind-hunter, edge-case-hunter, verification-gap, intent-alignment). 4 patched, 12 rejected.

| Finding | Verdict | Evidence |
|---|---|---|
| The demo seed still writes `UC-2026-0101`, which the new pattern rejects | false | `dev` carries the fixed seed (`23184502`, `BSIT`, `09170000101`) and this branch does not touch `database/`. The lens read a working copy that predates the #237 merge. |
| `.` is not a word boundary, so `J.R.R. TOLKIEN` becomes `J.r.r. Tolkien` | low | Real. Patched: `.` now starts a word, and `J.R.R. TOLKIEN` is a test case. |
| The member-edit path has no eight-digit non-`2` case, so the tightened rule is unpinned there | medium | Real gap. Patched: `31234567` added to `ProfileValueOutsideTheSharedRulesIsRejectedOnTheMemberEditPath`. |
| The approval-time normalisation is untested — reverting it leaves the suite green | medium | Real gap. Patched: `ApprovalStoresTheNameInTitleCase` seeds a mixed-case request and asserts the created account. |
| `NormalizeName` can return null into a NOT NULL `users.name` | false | `account_requests.full_name` is NOT NULL (`attendta-mvp-schema.sql:137,152`) and `[Required]` blocks blanks, so approval never receives a name that normalises to null. |
| Names are normalised on write but not read, so legacy rows keep capitals and are rewritten on the next save | false | That is the owner's chosen option B, recorded in the ticket and in this plan's intent. |
| The doc comment says leading zeroes survive while the pattern demands a leading `2` | low | Real contradiction. Patched: the comment now says the column keeps its shape. |
| No positive case proving `20000000` is accepted | false | The happy-path fixtures (`20230123`) and the counter base both prove the accept side. |
| No direct unit test of `NormalizeName` | false | It is exercised through `SubmitAsync` by a seven-case theory, which is where the behaviour matters. |
| A possessive renders as `O'Brien'S` | low | Rejected: names do not carry possessives, and the fix needs lookahead the rule does not describe. |
| A hyphen-only name survives `[Required]` | low | Rejected: absurd input, and closing it needs a content check beyond this ticket. |
| Non-ASCII letters are mangled by invariant casing | false | `char.ToUpperInvariant` handles accented letters correctly; invariant casing is the intended behaviour. |
| The audit snapshot records the unnormalised name | false | `MemberService` snapshots `user.Name`, which is the normalised value. |
| Legacy student numbers make members uneditable | false | Inventory of `AttendTaMVP_Test`: 0 offenders; `dev`'s seed is compliant. |
| No test pins the new student-number message text | low | Rejected: the pattern is pinned, and the suite does not assert most attribute messages. |

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — expected: build succeeds with no new warnings.
- `& "tests\ASI.Basecode.Services.Tests\bin\Debug\net10.0\ASI.Basecode.Services.Tests.exe"` — expected: all pass.
- `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` with `ATTENDTA_TEST_SQL` pointing at a local `master` — expected: all pass.
