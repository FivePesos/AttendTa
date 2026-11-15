# Review log: 1.17 profile name formatting and student-number prefix

Target: commit 58b99cf on feature/1-17-profile-name-formatting, PR #246

## 1 - Orientation - target identified

Session: unavailable - Timestamp: 2026-11-15T20:38:24.7388267+08:00

- Action: Identified the target as commit 58b99cf and read ticket 1.17 (issue #244) plus the build plan.
- Result: Intent block pasted verbatim from the ticket. Five blocks planned: intent, broad strokes, the name formatter, the student-number prefix, periphery.
- Evidence: _bmad-output/initiative-attendta/tickets/epic-platform-access/tickets/story-profile-name-formatting-and-student-number-prefix.md; gh issue 244.

## 2 - Intent - formal review

Session: unavailable - Timestamp: 2026-11-15T20:56:25.2011550+08:00

- Action: Ran bmad-code-review with four lenses over the one-commit diff. Note: the first diff was generated two-dot and showed the planning docs as deletions; regenerated three-dot to match what GitHub shows.
- Result: 4 patched, 0 deferred, 12 rejected. Patched: a member-path case for an eight-digit non-2 number, an approval-name test, dotted initials as a word boundary, and a doc comment that contradicted the pattern. Rejected the demo-seed claim (dev carries the compliant seed; the lens read a stale working copy), the nullability worry (full_name is NOT NULL) and the legacy-data concerns (0 offenders).
- Evidence: commit 18c47ca; 352 service tests and 510 integration tests pass.

## 3 - Broad strokes - test

Session: unavailable - Timestamp: 2026-11-15T21:04:13.0632605+08:00

- Action: Ran the two classes that pin the three write sites (AccountRequestServiceTests 51, MemberManagementTests 14), then mutated the approval site back to request.FullName to prove the new test is not vacuous.
- Result: All pass. The mutation produced Expected "Seth Pinca" / Actual "SETH PINCA", so the test genuinely catches the regression. Line restored and the tree confirmed identical to the commit; a BOM added by Set-Content was removed.
- Evidence: ASI.Basecode.Services/Services/AccountRequestService.cs:156; tests/ASI.Basecode.Services.Tests/AccountRequestServiceTests.cs.

## 4 - The name formatter - block accepted

Session: unavailable - Timestamp: 2026-11-15T21:38:54.1793517+08:00

- Action: Corrected a claim made during review: ANA 3RD becomes Ana 3rd, not 3Rd, so only the roman-numeral case was a real limitation. On the owner's call, added a six-entry suffix list (II, III, IV, V, VI) so JOHN PAUL II keeps its capitals while Jr. is untouched.
- Result: Block accepted. 355 service tests and 510 integration tests pass. Commit f25f62d.
- Evidence: ASI.Basecode.Resources/Constants/ProfileOptions.cs.

## 5 - The student-number prefix - test

Session: unavailable - Timestamp: 2026-11-15T21:47:33.1560623+08:00

- Action: Parsed the pattern out of ProfileOptions.cs and ran eleven values through .NET regex, then ran the two classes that pin it.
- Result: Accepts 2-prefixed eight-digit numbers including the 20000000 and 29999999 boundaries; rejects a 3, 1 or 0 prefix, seven and nine digits, and a non-digit. Empty passes the pattern and Required is the gate. 54 service and 14 integration tests pass.
- Evidence: ASI.Basecode.Resources/Constants/ProfileOptions.cs:24.
