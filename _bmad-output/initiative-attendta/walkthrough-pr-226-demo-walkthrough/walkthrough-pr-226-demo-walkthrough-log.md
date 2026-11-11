# Review log — PR #226 demo walkthrough

- 2026-11-12 — Session opened. Target identified as PR #226, "docs(acceptance): [BMAD 8.7] record the continuous 26-feature demo walkthrough". Docs-only: 2 new files, +477/-0.
- 2026-11-12 — Orientation: ticket read, PR description read.

## Blocks

| Block | Title | Status |
|-------|-------|--------|
| 1 | Intent | done |
| 2 | Broad strokes | done |
| 3 | The 68 journey rows | done |
| 4 | The FR-1..FR-36 mapping | done |
| 5 | The 26 functional-ID mapping | done |
| 6 | Blocked rows and the no-workaround claim | done |
| 7 | The plan file | done |
| 8 | Periphery | done |

## 1 — Block 1 Intent — orientation

- Action: Read the ticket, the PR description and the plan's frozen intent; identified the target as docs-only.
- Result: Intent recorded as the plan's verbatim Problem/Approach. Block 1 presented, awaiting the reviewer's acceptance.
- Evidence: PR #226 (2 new files, +477/-0); ticket id 7 / issue #80.
- Open: Block 1 not yet accepted by the reviewer.

## 2 — Block 1 accepted / Block 2 presented — review

- Action: Reviewer accepted Block 1. Presented Block 2 (broad strokes, five entry points).
- Result: Block 1 marked done in the narrative. No finding raised on Block 1.
- Evidence: PR head `f50d8d5` checked out locally as branch `pr-226` so the record's file references resolve.
- Open: Block 2 not yet accepted.

## 3 — Block 2 — draw and test

- Action: Drew the record's mechanism (one run → journey table → three projections) as Mermaid in `record-mechanism.md`. Wrote `check-record.ps1` and ran it against the record.
- Result: All structural claims held — 68 journey rows contiguous; 51 distinct J-references all resolve; 36 FR rows and 26 ID rows each exactly once; 7 blocked rows each with an owner. One finding: the plan says "6 blocked rows" in Implementation Notes and "7" in Auto Run Result; the record has 7. Disposition: open (plan prose only, no row missing).
- Evidence: `record-mechanism.md`, `check-record.ps1` output in this folder.
- Open: whether the reviewer wants the plan's stale "6" corrected in this PR.

## 4 — Block 2 — drive

- Action: Provisioned a clean `AttendTaMVP_Test` (reset, schema, admin, event types, demo scenario), ran the Production build on :5080, and the reviewer drove seven record claims in a browser.
- Result: All seven confirmed — J1 field set and acknowledgment, J54 generic refusal, J5 admin home, J2 queue and approve dialog, J64 officer denied on the audit log, J17 deadline refusal, and note 2's absent pass for seeded registrations. No new finding. J2's row names differ from the record because Ava Mendoza was journey data, not seed data.
- Evidence: build 0 warnings / 0 errors; `/Account/Login` and `/AccountRequest/Request` returned 200; seeded state 1 admin / 1 officer / 8 members.
- Open: environment difference recorded — .NET SDK 10.0.401 here versus 10.0.403 in the record.
- Note: the app is still running on :5080 for the rest of the review.

## 5 — Block 2 accepted / Block 3 presented — review

- Action: Reviewer accepted Block 2 after the drive. Presented Block 3 (the 68 journey rows, grouped into eight phases, with four rows called out).
- Result: No finding on Block 2. Block 3 presented for review.
- Evidence: record lines 86-153 hold J1..J68.
- Open: Block 3 not yet accepted.

## 6 — Block 3 — formal review (bmad-review, 4 lenses)

- Action: Ran bmad-review over the PR diff with adversarial, edge-case-hunter, structure and prose lenses; verified the consequential findings against the controllers and services.
- Result: 158 raw observations, triaged to 6 factual errors, 6 substantive gaps, 3 structural points. Six claims dismissed as false after verification, including J35's message (matches `CheckInController.cs:146-147`) and J2's pending count (header is pre-journey). Verified the 8.4 manifest citation is sound: `git diff --stat fa9ae82 84b3f54` excluding `_bmad-output` and `docs` is empty.
- Evidence: `formal-review.md`; `AccountController.cs:123`; `AccountRoleService.cs:75`; `CheckInController.cs:146-147`.
- Open: whether the six factual errors are fixed in this PR or deferred to a follow-up.

## 7 — Block 3 — six fixes applied

- Action: Applied the six factual corrections to the record and the plan in the working tree: blocked-row count 6→7, stale-note count two→three, J65 logout redirect `/`→`/Account/Login`, phantom "evidence field"→"observed-outcome cell" in two places, removed the `browser` header field from the plan's task list, "same patch level"→"same SDK version".
- Result: `check-record.ps1` reports all checks passed; plan and record agree on 7 blocked rows. The two substantive gaps (FR-8 replace, §11 filled-vs-evidenced) were left open as evaluator decisions.
- Evidence: `git diff --stat` — 2 files, 9 insertions, 8 deletions on branch `pr-226`.
- Open: the working tree has uncommitted fixes; the reviewer has not yet said whether to commit.

## 8 — Block 3 accepted / Block 4 presented — review

- Action: Reviewer accepted Block 3. Presented Block 4 (the FR-1..FR-36 mapping).
- Result: No new finding. Block 4 presented.
- Evidence: record §5, 36 rows.
- Open: Block 4 not yet accepted.

## 9 — Blocks 4-8 — walkthrough

- Action: Reviewer accepted Block 4 and asked for Blocks 5-8 in one pass. Presented the 26-ID mapping, the blocked rows and no-workaround statement, the plan file, and the periphery.
- Result: No new finding beyond those already triaged in `formal-review.md`. Block 5-8 presented together at the reviewer's request.
- Evidence: record §6-§11; plan §Intent-§Auto Run Result.
- Open: Blocks 5-8 not yet accepted.

## 10 — Blocks 5-8 accepted — review closed

- Action: Reviewer accepted Blocks 5 to 8. All eight blocks accepted; review closed.
- Result: Review complete. Six factual fixes applied and uncommitted on `pr-226`. Two substantive gaps left as evaluator decisions (FR-8 replace, §11 filled-vs-evidenced).
- Evidence: narrative `Outcome` section; `formal-review.md`; `explained-in-plain-english.md` (a plain-language explanation of the PR written at the reviewer's request for their instructor).
- Open: commit the six fixes; decide FR-8 and §11; the record's human sign-off block is still blank.

## Findings
