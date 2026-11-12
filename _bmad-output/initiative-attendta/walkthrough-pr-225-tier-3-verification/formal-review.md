# Formal review — PR #225

Content: the branch diff `origin/dev...feature/81-tier-3-manual-integration-regression` (`bb5b6b5`), class **docs** — two added markdown files, 384 lines, no deletions.

Lenses run: **adversarial**, **edge-case hunter**, **editorial structure**, **editorial prose** (on top of structure). Verification-gap is `applies_to: code` and did not apply. Style guide: Microsoft Writing Style Guide. Reader type: humans.

Raw lens output: 20 adversarial, 7 edge-case, 13 structure, 14 prose. Triaged below, then revised after an independent second opinion. Overlap between lenses is noted rather than deduped.

---

## Tier 1 — affects whether the record can gate

### 1.1 T3-04 is stale on arrival
[§4 T3-04](../tier-3-verification-record.md) files the request-to-certificate/report journey as **BLOCKED** because "no 8.7 record exists in `_bmad-output/initiative-attendta/` at `84b3f54`". True when it was written. PR #226 has since merged [demo-walkthrough-evidence.md](../demo-walkthrough-evidence.md) into `dev`, and its header pins it to **the same tested commit, `84b3f54`**, on the same OS, SDK and Node versions, as the 8.7 continuous journey with 36 FR rows and 26 functional-ID rows. Its seven blocked parts are exactly the seven T3-05 already carries; nothing in the journey itself is blocked.

Under the record's own §3 rule a documentation-only commit does not invalidate evidence, so T3-04 is now **REUSED**, not BLOCKED. Merged after #226, the release-gate record tells the human signer the journey is unproven while a same-commit record proves it. That is the record's whole job, so it is not a technicality. Fix: reclassify T3-04, add the 8.7 source to §3, and drop the first §7 blocker row.

### 1.2 BLOCKED is used for rows that are locally runnable
The legend defines BLOCKED as "Not executable here". T3-04's own unblocking action is "run the demo beats on a seeded build"; M-AUTH-01 is an 8-hour idle wait; M-ADM-01's action is a scripted SQL-gated race. All three are local actions, and 8.7 ran the first of them locally on the same day. T3-05's claim that its seven rows need "inputs no local run can supply" is false for two of them. *(adversarial 5, 6)*
Fix: split T3-05 into external-input rows and locally runnable rows, and state why the latter were not run.

### 1.3 §6 never proves the migration's pass boundary
Assertion 4 exercises failing lengths only (493 → 508). The exact pass boundary, 492 → 500, is never run, so an off-by-one in the `> 500` guard would pass both the record and `VenueImagePathMigrationTests`. Separately, assertion 4's database has no *valid* legacy row left besides the failing one, so "no rows changed" proves less than it reads: it cannot distinguish a correct rollback from a migration that never had work to do. *(edge-case 2, 3)*
Fix: add a 492-character row that must migrate to exactly 500, and a second valid `venues/` row that must still be unprefixed after the abort.

---

## Tier 2 — traceability and accuracy

| # | Finding | Location | Fix |
|---|---|---|---|
| 2.1 | §3 cites `execution-manifest-fa9ae82.md` at `fa9ae82` and `manual-acceptance-checklist.md` at `d8046bf`; neither file exists at its cited commit (verified — both `git cat-file -e` calls fail; added by `21c8b1f` and `211a1f2`). The column means *tested* commit, but a reader who checks out the cited commit finds nothing. *(adversarial 2, edge-case 6)* | §3 | Relabel the column "Tested commit" and add the file's own commit |
| 2.2 | T3-06 reports `PASS (none required)` and T3-07 reports `BLANK`; §2 defines neither, while §8 claims every row carries a result. *(adversarial 7, edge-case 1, structure)* | §2, §4, §8 | Add both values to the legend or restate the rows |
| 2.3 | §1 records Windows build 10.0.26200 and SDK 10.0.403; the 8.6 checklist records 10.0.26100 and 10.0.401, the 8.4 manifest 10.0.26100 and Node v24.6.0. 8.6 discloses its own difference; the Tier 3 record does not. *(adversarial 4)* | §1, §3 | One sentence stating why the difference is immaterial |
| 2.4 | Triage log says "6 findings: 0 high, 4 medium, 1 low"; the table has five rows and "Routes: 5 patch" totals five. *(adversarial 16, edge-case 7, structure)* | plan, Review Triage Log | Correct the count, or add the missing row |
| 2.5 | Triage finding #4 claims it added the "seventh §7 blocker row"; §7 has eight rows and M-ADM-01 is the eighth. The task bullet still lists seven. | plan | Correct to eighth; add M-ADM-01 to the task bullet |
| 2.6 | T3-01 says UI-01 returned "200 and its own heading" for every role; 8.6 records headings for Administrator routes only. | §4 T3-01 | Say 200 for every role, headings for Administrator routes |
| 2.7 | T3-01 says UI-02 ran 200 % zoom at 1440/1024/768/390; 8.6's body records zoom emulated at the 1440/2 viewport on two roles. | §4 T3-01 | Match the body, not the 8.6 heading |
| 2.8 | SM-4 is quoted as including "zero duplicate registrations"; the PRD's SM-4 is overbooked events, duplicate attendance, duplicate certificates. | §4 T3-01 | Drop the fourth clause |
| 2.9 | T3-04 says both certificates were issued "on a real retained PDF"; 8.6 records a retained PDF for `AT-2026-000024` only. | §4 T3-04 | Name the one with the PDF |
| 2.10 | T3-04's owning file is `finale-test-cases.md` in §4 but `finale-test-cases.md` step 5 *and* `demo-script.md` in §7. | §4 vs §7 | Use one owner in both |
| 2.11 | The §6 fixture embeds `$argon2id$fixture-not-a-real-hash`, against the plan's own "never put hashes in the record" rule. | §6 | Use a non-hash placeholder |
| 2.12 | The over-length insert hard-codes `created_by = 1` after capturing `@u` via `SCOPE_IDENTITY()`; `$probe` is regenerated per run, so cleanup drops only the current value and a re-run leaks a database. | §6 | Reuse `@u`; capture the probe name once |
| 2.13 | The plan claims a pre-existing untracked 8.7 plan file was "left untracked and unmodified"; it is now committed by PR #226, so the claim cannot be checked. | plan | Drop the claim or note its resolution |
| 2.14 | The plan's `context:` and Code Map omit `demo-script.md`; `context:` omits `finale-test-cases.md`. The record relies on both. | plan | Add both |

---

## Tier 3 — editorial and optional

**Structure.** Lead with the gate status — today a reader reaches §4 before learning the gate is blocked. Condense §4's paragraph-length Evidence cells to verdict plus decisive number, with pointers to §5–§7; this also removes duplication with those sections. Move §5 and §6 to an appendix after §8 so the decision chain (matrix → blockers → sign-off) is unbroken. Cut two of the three copies of the stale "JPEG only" sentence. Collapse §9's empty five-column table to one sentence. Condense the §6 "Target:" paragraph, the leftover-database note, and §8's "Record complete" cell, which restate §1 and the opening paragraph. Preserve §2, §3 and §7 — their density is the argument.

**Prose.** US spelling: `behaviour` → `behavior`, `artefact` → `artifact` (the repository runs 571:64 and 201:8 the other way). `200 %` → `200%`. Three comma splices (plan Boundaries, §6 assertion 4, §3 diff note). Three verbless openings ("Fixture, written to…", "Migration, run with…", "Every BLOCKED row above…"). One idea, "documentation-only", written four ways including a slash compound. Four names for one test suite: use "the 479-test integration suite" on first mention, "the 479 suite" after. Fix "this manifest" → "that manifest" in the §3 parenthetical, and "byte-identical to assertion 1" → "byte-identical to those in assertion 1". "Sign off" takes "on".

**Optional.** The matrix maps only UI-01/UI-02, M-CERT-02a and the M-ADM-01 mixed race, leaving 8.6's M-CERT-02b, M-MD-01, M-ATT-02 and sequential M-ADM-01 unmapped. Tier 3 owns cross-feature compatibility, the build and migrations, not the SPEC-20 rows, and 8.7 §6 cites all four — so this is cosmetic once 1.1 is fixed. One line in T3-01 would close it.

---

## Not findings

- The migration guard's `AttendTa_IT[_]%` escape is correct, and `AttendTa_IT_t3mig523f203e` legitimately passes while `AttendTa_ITprobe_*` is refused.
- `git diff --stat fa9ae82..84b3f54` really is five files, all under `_bmad-output/`, so the 8.4 manifest's invalidation rule holds and not re-running the 479 is correct.
- Assertion 4's snapshot query *is* recorded verbatim with before/after tables, and the recorded Msg 50000 distinguishes a guard abort from a truncation — so "a partial commit would pass" is not a valid objection. Assertions 1 and 2 do lack their observation queries.
- The 321/479 counts, the 8.6 PASS/BLOCKED tallies, the 78-link crawl, `AT-2026-000023/24`, the cited test class names and the "JPEG only" premise in the coverage matrix all check out.
- No product defect is implied by any finding. This is a documentation change.
