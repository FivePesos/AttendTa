# Walkthrough: 1.17 ΓÇö profile name formatting and student-number prefix

Target: commit `58b99cf` on `feature/1-17-profile-name-formatting`, PR #246.

## Blocks

- [x] 1. Intent ΓÇö done
- [x] 2. Broad strokes ΓÇö done
- [x] 3. The name formatter ΓÇö done
- [x] 4. The student-number prefix ΓÇö done
- [ ] 5. Periphery ΓÇö current

## 1. Intent

> Two profile-field follow-ups raised while the owner exercised the 1.15 signup form.
>
> First, a student number must begin with `2`, so the shared pattern becomes `^2[0-9]{7}$` and the field message says so. The eight-digit rule, the string type and the uniqueness checks stay as 1.15 left them.
>
> Second, a name typed in capitals must be stored the way it should read: `SETH PINCA` becomes `Seth Pinca` and `JOHN PAUL A. SINGCO` becomes `John Paul A. Singco`. Normalise on save, the way the section is normalised today, so every screen is right without a formatter and no migration is needed. The rule: split on spaces and hyphens, collapse repeated spaces, upper-case a token that is a single letter with or without a period, capitalise the first letter of every other token and lower-case the rest, and rejoin with the original separators. Small connector words such as `de`, `dela` and `van` are capitalised like any other token, so `MARIA DELA CRUZ` becomes `Maria Dela Cruz`.

Source: ticket 1.17 (issue #244), Description, pasted verbatim.

The owner chose to normalise **on save** rather than at display, so the stored value is already correct and no screen needs to remember to format it. Rows written before this change keep whatever they hold.

## 2. Broad strokes

Three places carry the change.

1. [ProfileOptions.cs](../../../ASI.Basecode.Resources/Constants/ProfileOptions.cs) ΓÇö the student-number pattern tightens to `^2[0-9]{7}$`, and a new `NormalizeName` sits beside `NormalizeSection`.
2. [AccountRequestService.cs](../../../ASI.Basecode.Services/Services/AccountRequestService.cs) and [MemberService.cs](../../../ASI.Basecode.Services/Services/MemberService.cs) ΓÇö each `Normalize` method calls `NormalizeName` for `FullName` instead of a bare `Trim()`.
3. [AccountRequestService.cs](../../../ASI.Basecode.Services/Services/AccountRequestService.cs) again, at the approval site ΓÇö the name copied onto the new account is normalised too.

The mechanism: every write of a person's name goes through one formatter, so the database holds title case and no reader has to do anything.

## 3. The name formatter

`NormalizeName` collapses repeated whitespace, then walks the string once. Space, `-` and `'` mark the start of a new word; the first character of a word is upper-cased and the rest lower-cased. That single pass gives initials, hyphens and apostrophes for free, without a list of special cases.

| Typed | Stored |
|---|---|
| `SETH PINCA` | Seth Pinca |
| `JOHN PAUL A. SINGCO` | John Paul A. Singco |
| `maria dela cruz` | Maria Dela Cruz |
| `ANNE-MARIE LOPEZ` | Anne-Marie Lopez |
| `o'neil santos` | O'Neil Santos |
| `  ana   reyes  ` | Ana Reyes |

Two consequences are accepted rather than fixed. An intentional internal capital is lost, so `McDonald` becomes `Mcdonald`. A token containing a digit is unaffected in practice: `ANA 3RD` becomes `Ana 3rd`, which is how it should read. Roman-numeral suffixes are handled by a six-entry list, so `JOHN PAUL II` stays `John Paul II` and `III` stays `III`, while `Jr.` is untouched.

## 4. The student-number prefix

One line in [ProfileOptions.cs](../../../ASI.Basecode.Resources/Constants/ProfileOptions.cs): `^[0-9]{8}$` becomes `^2[0-9]{7}$`, because real UC ids are a two-digit year prefix plus six digits. The field message on both view models now reads "Enter an 8-digit student number starting with 2."

Nothing else moved. Every fixture that passes through a view model already used a number starting with `2`; entity-only seeds such as `SN-7` and `FLOW-IT-1001` bypass the rule and were left alone. The existing rejection cases for seven digits, nine digits and a non-digit still hold, and two were added: an eight-digit number starting with `3` on the signup path, and the same on the member edit path.

## 5. Periphery

- [AccountRequestServiceTests.cs](../../../tests/ASI.Basecode.Services.Tests/AccountRequestServiceTests.cs) ΓÇö a six-case theory for the formatter, plus the `31234567` rejection case.
- [MemberManagementTests.cs](../../../tests/ASI.Basecode.IntegrationTests/MemberManagementTests.cs) ΓÇö a test proving the member edit path stores the title-cased name, and one stale assertion updated: `After lock release` is now stored as `After Lock Release`, which is the rule working.
