# Review log: PR #225 — Tier 3 verification record

Target: PR #225, branch `feature/81-tier-3-manual-integration-regression` → `dev`, commit `bb5b6b5`, two added files (384 lines, no deletions).

<!--
Append-only record of outcomes/decisions.
Main purpose: persisted context for resuming or analyzing
the review process. You're writing this log for consumption
by another LLM session.

Record decisions and their reasons, user constraints, unresolved
findings, and references needed to recover evidence. Link to large
results rather than copying them here.
This is NOT a transcript or internal deliberation.
The review narrative (the human-facing file) owns review blocks
and their statuses.
-->

## 1 — Orientation — target identified

Session: unavailable · Timestamp: 2026-11-12T10:22:47+08:00

- Action: Read PR #225 description, ticket 8.8 / #81, the added plan file and the added record; diffed the branch against `origin/dev`.
- Result: Documentation-only change. Two files added: `_bmad-output/initiative-attendta/tier-3-verification-record.md` (267 lines) and the ticket plan (117 lines). No product, Razor, CSS, schema, test or CI file. PR explicitly does not close #81.
- Evidence: `git diff --stat origin/dev...origin/feature/81-tier-3-manual-integration-regression`; PR body tier-3 matrix table.
- Open: Whether the record's claims hold at the commit it is being merged into.

## 2 — Block 1 (Intent) — second opinion

Session: unavailable · Timestamp: 2026-11-12T10:28:00+08:00

- Action: Fresh subagent with no review context, asked whether the delivered record matches the plan's frozen intent and whether that scope is right for a Tier 3 release gate.
- Result: Scope right; execution right-but-incomplete. (a) "the two rows nothing else covers" is inflated — T3-03 itself concedes only the allow-list guard is new, since prefix/idempotence/over-length are already asserted in `VenueImagePathMigrationTests`. (b) T3-07's result word `BLANK` is not in the §2 legend (PASS/FAIL/BLOCKED/REUSED/NOT RUN), while the plan's own acceptance criterion says every row carries a result. (c) The plan's Problem statement names 8.7 as existing evidence; it did not exist on the branch. Verified sound: the guard's `AttendTa_IT[_]%` escape and the five-file, all-docs `fa9ae82..84b3f54` diff.
- Evidence: `_bmad-output/initiative-attendta/tier-3-verification-record.md:30` (legend), `:69` (T3-07), `:5` (opening claim), `:65` (T3-03 concession); `database/migrate-venue-image-paths.sql:11`.
- Open: Whether to patch (a) and (b) before merge. (c) is also factually resolved on `dev` by PR #226, which landed the 8.7 record on the same tested commit.

## 3 — Block 1 (Intent) — formal review

Session: unavailable · Timestamp: 2026-11-12T10:34:00+08:00

- Action: `bmad-review` on the branch diff, content class docs. Lenses: adversarial (20 findings), edge-case hunter (7), editorial structure (13), editorial prose (14). Verification-gap skipped — it is `applies_to: code`. Full report: `formal-review.md` in this folder.
- Result: 54 raw findings, triaged into 7 gate-affecting, 10 accuracy, and editorial. Headline: T3-04 is stale on arrival — PR #226 merged the 8.7 record on the same tested commit `84b3f54`, so under the record's own §3 rule T3-04 is REUSED, not BLOCKED. Also: T3-06/T3-07 use result values outside the §2 legend while §8 claims every row carries a result; §3 cites `execution-manifest-fa9ae82.md` and `manual-acceptance-checklist.md` at commits that do not contain them (verified — both `git cat-file -e` calls fail; the files were added by `21c8b1f` and `211a1f2`); BLOCKED is used for locally runnable rows; four 8.6 PASS rows are unmapped; §6 records no observation queries and never tests the 492-character pass boundary.
- Evidence: `formal-review.md`; `_bmad-output/initiative-attendta/tier-3-verification-record.md:66`; `_bmad-output/initiative-attendta/demo-walkthrough-evidence.md:23`.
- Open: Nothing was patched — this is a review, not an edit. The reviewer decides what to fix before merge.

## 4 — Block 1 (Intent) — second opinion on the triage

Session: unavailable · Timestamp: 2026-11-12T10:40:00+08:00

- Action: Fresh subagent with no review context asked to judge the seven Tier 1 items and the headline.
- Result: Headline upheld. The finding is right but the label understates it: merged after PR #226, the record tells the human signer the journey is unproven while a same-commit record proves it. Triage revised — 1.1 (T3-04) and the BLOCKED-misuse item stay Tier 1; the §6 item narrowed to the untested 492→500 pass boundary and the absence of a valid legacy row, because "a partial commit would pass" is wrong (the recorded checksum and len_sum would change) and assertion 4's snapshot query is in fact recorded; the legend, §3 commit-citation, environment and unmapped-8.6-rows items demoted to Tier 2/Tier 3. `formal-review.md` rewritten to match.
- Evidence: `formal-review.md` (revised); `_bmad-output/initiative-attendta/tier-3-verification-record.md` §6 assertion 4.
- Open: Whether the reviewer patches the two Tier 1 items before merge.

## 5 — Block 2 (Broad strokes) — draw

Session: unavailable · Timestamp: 2026-11-12T10:46:00+08:00

- Action: Drew the record's mechanism as a Mermaid flowchart plus a plain-text rendering in session. Source saved as `record-mechanism.md` in this folder.
- Result: The diagram makes the reuse rule visible as a single decision node: a cited source either passes its own validity test at `84b3f54` and feeds the matrix, or it becomes a blocker. It also shows the missing edge — §3 has no 8.7 source, so T3-04 sits on the blocked branch even though PR #226's record was executed on the same commit. Two rows use result words the legend does not define.
- Evidence: `record-mechanism.md`; `_bmad-output/initiative-attendta/tier-3-verification-record.md` §2, §3, §4, §7, §8.
- Open: Block 2 not yet accepted.

## 6 — Block 2 (Broad strokes) — test

Session: unavailable · Timestamp: 2026-11-12T10:52:00+08:00

- Action: Re-ran T3-02's four commands and rebuilt T3-03 from scratch on three disposable databases, on the same environment the record names (SDK 10.0.403, Node v24.12.0, Windows build 26200). Added the two assertions the record omits: a 492-character legacy row, and a valid legacy row alongside the failing one.
- Result: Every recorded observation reproduced. Build 0 warnings / 0 errors; `css:build` exit 0 with no content change (the `git status` M is line-ending normalisation only, `git diff` is empty); `test:js` 5/5; service tests 321/321; prefix, idempotence, guard exit 1, and over-length abort exit 1 reporting venue 2 at 508 characters all matched. Both added assertions pass: 492 → 500 migrates, and after the abort the valid legacy row is still unprefixed, so the rollback is real and the `> 500` boundary is correct. The §6 gap is an evidence gap, not a defect.
- Evidence: structural counts re-derived — §3 has 8 sources, §4 has 7 rows, §7 has 8 blockers, and 8.7 is cited only at `:229`, never as a §3 source.
- Open: The 479 integration suite was not re-run (the manifest rule holds). Host, phone and 8-hour rows remain untestable here.

## 7 — Block 3 (§3 evidence reuse) — test

Session: unavailable · Timestamp: 2026-11-12T10:57:00+08:00

- Action: Re-derived all eight §3 verdicts against their sources — file presence at the cited commit, last-touch commit, and the counts each verdict asserts.
- Result: All eight verdicts are substantively correct. 8.4's 321/479 and five-file docs diff hold; 8.6's 8 PASS / 5 BLOCKED / 0 FAIL holds; 8.5's stale "JPEG only" premise is at `acceptance-coverage-matrix.md:102`; `finale-test-cases.md:81,83` really does assign step 5 to 8.7 and step 6 to 8.8; hotspot has 12 numbered rows all BLOCKED; host-readiness has 10 numbered checks all BLOCKED; storage carries 42 BLOCKED markers. Citation defects, not verdict defects: the 8.4 manifest is absent at `fa9ae82` (added by `970781b`), the 8.6 checklist absent at `d8046bf` (added by `211a1f2`), the 8.14 host file was last touched by `fb783f1` not the cited `da3661b`, and `demo-script.md` is cited by bare filename when its path is `_bmad-output/initiative-attendta/demo-script.md`. No 8.7 row exists.
- Evidence: `git cat-file -e` per row; `git log -1 -- <path>` per row; row counts above.
- Open: Block 3 not yet accepted.

## 8 — Block 4 (§4 Tier 3 matrix) — test

Session: unavailable · Timestamp: 2026-11-12T11:01:00+08:00

- Action: Re-derived each of the seven row results against §3, §5, §6 and the sources the evidence cells cite.
- Result: Four rows exactly right (T3-02, T3-03, T3-06, T3-07). T3-04 is wrong — 8.7's record exists on `dev` at the same `84b3f54`, so the row is REUSED, not BLOCKED. T3-01's verdict is licensed but two details overstate the source: 8.6 records headings for Administrator routes only (`manual-acceptance-checklist.md:160` says "all 200" for Officer and Member), and the PRD's SM-4 is overbooked events, duplicate attendance and duplicate certificates only (`prd-attendta.md:547`) — the record adds "zero duplicate registrations". T3-05's seven-row count is right, but its justification is wrong for two rows: M-AUTH-01 is an 8-hour wait and M-ADM-01 a scripted race, both local actions.
- Evidence: `manual-acceptance-checklist.md:160`; `prd-attendta.md:547`; `tier-3-verification-record.md:63-69`.
- Open: Block 4 not yet accepted.

## 9 — Wrap-up — patch applied and artifacts committed

Session: unavailable · Timestamp: 2026-11-12T11:04:00+08:00

- Action: Reviewer chose the T3-04 + legend + citations bundle and a separate branch for the artifacts. Patched `tier-3-verification-record.md` and committed it as `1f5e8d9` on `feature/81-tier-3-manual-integration-regression`.
- Result: T3-04 is REUSED citing the 8.7 record at `84b3f54`; §3 gains the 8.7 row, a "Tested commit (record commit)" column, `970781b`/`211a1f2` for the two files that postdate their tested commits, `fb783f1` for the 8.14 host file, and the full `_bmad-output/initiative-attendta/demo-script.md` path; §2 defines `PASS (none required)` and `BLANK`; §7 drops to seven blockers; §8 names T3-05 as the only open row. Left undone by choice: the opening paragraph still says "the database migration checks" when only the guard is new, and the editorial pass.
- Evidence: commit `1f5e8d9`; `git diff` 10 insertions, 8 deletions, one file.
- Open: Artifacts branch `docs/225-review-record` to be pushed; PR branch push pending reviewer confirmation.

## 10 — Close-out — both PRs open, review complete

Session: unavailable · Timestamp: 2026-11-12T11:05:00+08:00

- Action: Pushed both branches, rewrote the PR #225 description to match `1f5e8d9`, and opened PR #228 for the artifacts branch. Corrected the timestamps in entries 2–9, which had been estimated ahead of the real clock.
- Result: PR #225 carries 2 commits and 2 files; PR #228 is open against `dev`. Merge #225 first — #228 is stacked on `1f5e8d9`, so its diff currently shows 6 files and collapses to the 4 artifacts once #225 lands. Review complete: seven blocks, all accepted by the reviewer.
- Evidence: `https://github.com/rainyDayssss/AttendTa/pull/225`, `https://github.com/rainyDayssss/AttendTa/pull/228`.
- Open: Not the review — the ticket. #81 stays open on T3-05's seven rows; M-ADM-01 (scripted race) and M-AUTH-01 (8-hour idle) are local work, the other five need the demo host.
