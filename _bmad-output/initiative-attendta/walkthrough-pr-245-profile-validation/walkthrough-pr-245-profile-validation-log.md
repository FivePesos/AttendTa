# Walkthrough log — PR #245 / issue #238

- Target: GitHub issue rainyDayssss/AttendTa#238 "Shared profile field validation across signup and member edit"
- Implementation: PR #245, merge commit df09f00, branch feature/1-15-profile-validation (commits 3e883be, 5dbca04, f765173)
- Diff base: 08cdf10
- Date: 2026-11-15

## Activity

- review opened
- target identified: PR #245 / merge df09f00, diff base 08cdf10
- ticket read: issue #238
- plan read: _bmad-output/initiative-attendta/plan-1-15-profile-validation.md

### Entry 2 — Orientation — target and sources

- Session: unavailable · Timestamp: 2026-11-15T20:37:34+08:00
- Action: read issue #238, plan-1-15-profile-validation.md, and the PR #245 diff (base 08cdf10, head df09f00, 15 files).
- Result: target is a merged PR; scope is constants + two view models + two services + two views + tests + bruno + deferred-work.
- Evidence: git diff --stat 08cdf10..df09f00.
- Open: none.

### Entry 3 — Orientation — findings from the read

- Session: unavailable · Timestamp: 2026-11-15T20:37:34+08:00
- Action: compared the shipped rules against the ticket text.
- Result: four observations, all open. (a) Mobile pattern is `^09[0-9]{9}$`, stricter than the ticket's "exactly eleven ASCII digits"; commit 5dbca04 says 12345678901 passed, so the tightening is deliberate but the ticket body was not updated. (b) The course pattern is still the inline literal `"^(BSCS|BSIT)$"` in both view models rather than a ProfileOptions constant, although the ticket asks for shared patterns in ProfileOptions. (c) The form path validates the raw post, so a padded student number is refused by ModelState even though the ticket says trim before validating; only the service path trims first (Normalize at AccountRequestService.SubmitAsync before Validate). (d) MemberService validates inside the transaction after taking the user lock — already recorded in deferred-work.md.
- Evidence: ProfileOptions.cs, AccountRequestViewModel.cs, MemberViewModel.cs, AccountRequestService.cs:66-67, deferred-work.md.
- Open: (a) (b) (c) (d).

### Entry 4 — Block 1 Intent — second opinion (fresh subagent, no review context, session ses_e20c7d793ffeTado0ZNJGD1uvy)

- Session: unavailable · Timestamp: 2026-11-15T20:44:00+08:00
- Action: asked a clean subagent whether the merged change matches the ticket text, rule by rule.
- Result: confirmed three mismatches. (1) The service boundary DOES reject out-of-rule course and section via Validator.TryValidateObject, inverting the plan's central decision "do not reject at the service boundary"; the concurrency fixtures were made compliant instead, so the race tests still pass. (2) The form path validates the raw post, so the service's trim is unreachable there. (3) Mobile is stricter than the ticket (09 prefix). New: NormalizeCourse maps long legacy labels before validation, so a crafted POST of "BS COMPUTER SCIENCE" is accepted and stored as BSCS; the course regex and the year range are literals rather than ProfileOptions-derived; the member editor has no password field, so the password half of the ticket does not apply to it.
- Evidence: subagent report; AccountRequestViewModel.cs:30, MemberViewModel.cs:28, AccountRequestService.cs:286-296, MemberService.cs:240-250, ProfileOptions.cs:10-27, AccountRequestController.cs:136, MemberController.cs:147.
- Open: reviewer to decide whether each mismatch is accepted, fixed or deferred. No block accepted yet.

### Entry 5 — Block 1 Intent — formal review (bmad-code-review, four lenses: blind-hunter, edge-case-hunter, verification-gap, intent-alignment)

- Session: unavailable · Timestamp: 2026-11-15T21:30:00+08:00
- Action: ran the formal review on diff 08cdf10..df09f00; triaged 40 raw findings into 17 deduplicated ones.
- Result: 4 decisions resolved by the user (inventory + correction affordance; keep the 09 rule and amend the docs; drop maxlength; assert a specific Invalid outcome on legacy approval). 16 patches applied: ProfileOptions gained CoursePattern and Min/MaxYearLevel; dead StringLength annotations removed; password capped at 128; maxlength removed; section selects normalised and given a placeholder; legacy approval, selected-section, year-filter, leading-zero and unknown-course tests added; shared ProfileFixtures helper and ProfileOptionsTests guard; inventory SQL script; handoff, UI plan and database-setup docs amended; issue #238 body amended. 1 defer (validate-after-lock, already recorded), 4 rejected.
- Evidence: build Debug and Release clean; service suite 345/345; integration suite 516/516 with ATTENDTA_TEST_SQL on LocalDB master. Findings in plan-1-15-profile-validation.md under `## Code Review`.
- Open: block 1 still not accepted by the user.

### Entry 6 — Block 1 Intent — accepted

- Session: unavailable · Timestamp: 2026-11-15T21:40:00+08:00
- Action: user accepted block 1 after the second opinion and the formal review.
- Result: block 1 closed; block 2 presented. Tree dirty with the review patches, uncommitted; user has not asked for a commit.
- Open: blocks 2-7 unvisited.

### Entry 7 — Block 2 Broad strokes — draw

- Session: unavailable · Timestamp: 2026-11-15T21:52:00+08:00
- Action: drew the mechanism as a Mermaid flowchart, saved beside the narrative.
- Result: one rule set in ProfileOptions feeding both view models and both forms; ModelState as the form refusal; both services validating the same annotations; approval of a legacy row as the one path with no in-app correction.
- Evidence: block-2-mechanism.md (renders in the VS Code markdown preview).
- Open: block 2 not accepted yet.

### Entry 8 — Block 2 Broad strokes — accepted

- Session: unavailable · Timestamp: 2026-11-15T22:05:00+08:00
- Action: user accepted the mechanism drawing.
- Result: block 2 closed; block 3 (the rule set and the two view models) presented.
- Open: blocks 3-7 unvisited; tree still dirty.

### Entry 9 — Block 3 One rule set, two view models — test

- Session: unavailable · Timestamp: 2026-11-15T22:20:00+08:00
- Action: ran the service suite class AccountRequestServiceTests (44 passed), and the integration classes MemberManagementTests (14), ProfileOptionsTests + ListPresentationTests (22), against LocalDB master.
- Result: both view models refuse the same values with the same field names; the constants guard passes; no failures.
- Unverified: the browser-side messages (only the server text is asserted) and the two form dropdowns (only the list toolbar is asserted).
- Evidence: tests/ASI.Basecode.IntegrationTests/ListPresentationTests.cs:72, ProfileOptionsTests.cs, MemberManagementTests.cs:329.
- Open: block 3 not accepted.

### Entry 10 — Block 3 One rule set, two view models — accepted

- Session: unavailable · Timestamp: 2026-11-15T22:35:00+08:00
- Action: user accepted after the test run.
- Result: block 3 closed; block 4 (the two forms) presented.
- Open: blocks 4-7 unvisited; tree still dirty.

### Entry 11 — Block 4 The forms — product owner edits during the drive

- Session: unavailable · Timestamp: 2026-11-15T23:10:00+08:00
- Action: user asked for membership type Regular + Developer only, and sections A–C only; confirmed the mobile rule stays 09-prefixed with eleven digits and refuses on submit rather than blocking keystrokes.
- Result: ProfileOptions.MembershipTypes = {Regular, Developer}; Sections = {A, B, C}; SectionPattern = ^[A-Ca-c]$; messages say "A to C"; guard test, inventory SQL, handoff and UI plan updated; issue #238 body to be amended. Service suite 345 and 51 integration tests green.
- Open: block 4 not accepted; membership-type change is outside ticket #238.

### Entry 12 — Block 4 The forms — accepted

- Session: unavailable · Timestamp: 2026-11-15T23:25:00+08:00
- Action: user accepted after driving the forms and making the section/membership edits.
- Result: block 4 closed; block 5 (service normalization) presented.
- Open: blocks 5-7 unvisited; three uncommitted change sets in the tree.

### Entry 13 — Block 5 Service normalization — accepted

- Session: unavailable · Timestamp: 2026-11-15T23:45:00+08:00
- Action: user asked for a plain-English restatement of the block, then accepted it.
- Result: block 5 closed; block 6 (tests and fixtures) presented.
- Open: blocks 6-7 unvisited; three uncommitted change sets.

### Entry 14 — Block 6 Tests — accepted

- Session: unavailable · Timestamp: 2026-11-15T23:58:00+08:00
- Action: user accepted the test slice without further questions.
- Result: block 6 closed; block 7 (periphery) presented.
- Open: block 7 unvisited; three uncommitted change sets.

### Entry 15 — Block 7 Periphery — accepted; review complete

- Session: unavailable · Timestamp: 2026-11-16T00:10:00+08:00
- Action: user accepted the last block.
- Result: all seven blocks accepted; review closed. Follow-through proposed: two commits on a branch, push, PR into dev, stop the app.
- Open: three uncommitted change sets; issue #238 still open remotely.
