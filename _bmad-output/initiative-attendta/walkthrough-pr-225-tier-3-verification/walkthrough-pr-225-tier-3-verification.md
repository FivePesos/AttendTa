# Walkthrough — PR #225, the Tier 3 verification record

This review covers PR #225, "docs(acceptance): [BMAD 8.8] record the Tier 3 matrix with the build and migration checks passing and sign-off blocked", for ticket [story-tier-3-manual-system-integration-and-regression-verification.md](../tickets/epic-release-acceptance/tickets/story-tier-3-manual-system-integration-and-regression-verification.md) (id 8, tracker issue #81, BMAD 8.8). It is a documentation-only change: two new files, 384 lines added, nothing deleted, and no product code, Razor, CSS, schema, test or CI file touched.

The deliverable is [tier-3-verification-record.md](../tier-3-verification-record.md), the release-gate decision record for Tier 3. The companion plan is [the ticket plan](../tickets/epic-release-acceptance/tickets/story-tier-3-manual-system-integration-and-regression-verification-plan.md). Because nothing ships to users, the review is about whether the record is accurate, complete and honest about what was and was not exercised — and, above all, whether it correctly refuses to approve a release it cannot approve.

## Blocks

### Block 1 — Intent

**Status:** done — accepted by the reviewer 2026-11-12, after a second opinion and a formal four-lens review

Verbatim from the frozen intent section of [the ticket plan](../tickets/epic-release-acceptance/tickets/story-tier-3-manual-system-integration-and-regression-verification-plan.md):

> **Problem:** Tier 3 is the release gate: the whole team signs off cross-feature compatibility, the integrated build and database migrations on the shared `dev` build before `dev` goes to `main`. No Tier 3 record exists. 8.4 (#77) produced the automated manifest, 8.6 (#79) the manual checklist, 8.7 (#80) the demo walkthrough and 8.2/8.3 (#75/#76) the environment evidence — but nothing states, for one exact commit, which rows are proven, which are still blocked, and that the human decision is outstanding.
>
> **Approach:** Write one Tier 3 verification record that cites the existing evidence with its validity rule, executes only the rows nothing else covers (the integrated build and database migration checks), marks every unmet row BLOCKED with its missing input named, and leaves the team-lead/tester decision blank for the human. No second checklist, no second screen, no product change.

The ticket states the same requirement in one line — "The team lead and testers complete the Tier 3 manual test matrix on the integrated dev build and sign off that all cross-feature test cases and database migration checks pass" — and adds that unresolved critical cases, remaining fixtures or unverified environment rows block sign-off.

What this is not: a product change. The deliverable is a decision record; nothing in the app, the schema, the views or the tests moves.

### Block 2 — Broad strokes

**Status:** done — accepted by the reviewer 2026-11-12, after re-running both executed rows

The change is one decision record plus the plan that produced it. The record has a fixed skeleton: a header that pins the run to a commit and an environment, a legend that defines what each result word means, a reuse table that decides for each earlier ticket whether its evidence still holds at this commit, the matrix that is the actual gate, the two rows executed here with their commands and output, and then the blockers and the blank sign-off. The plan is the working record around it — intent, boundaries, tasks, and the review findings patched before the PR opened.

Five places to open first:

1. [The record header](../tier-3-verification-record.md) — pins the whole record to commit `84b3f54`, one workstation, one SDK and Node version, and states plainly that this is not the demo host.
2. [§3 Evidence reuse and validity](../tier-3-verification-record.md) — eight sources, each with its own commit and a verdict on whether it still holds at `84b3f54`; this table is what licenses reusing 8.4, 8.6, 8.2 and 8.3 instead of re-running them.
3. [§4 The Tier 3 matrix](../tier-3-verification-record.md) — seven rows, T3-01 to T3-07, each with a result and the evidence that produced it; this is the gate.
4. [§5 and §6](../tier-3-verification-record.md) — the only two rows executed for this ticket: the integrated build with its four commands, and the venue image path migration with its four assertions.
5. [§7 Blockers and §8 Sign-off](../tier-3-verification-record.md) — eight blockers, each with an owner and the action that would unblock it, followed by a sign-off block whose decision fields are deliberately empty.

Read in that order the mechanism is clear: one commit is tested for two things, and everything else is either cited under a stated validity rule or named as blocked. Nothing in the document is a second run, and no row claims a PASS it did not observe.

### Block 3 — Evidence reuse and the validity rules

**Status:** done — accepted by the reviewer 2026-11-12, after re-deriving all eight verdicts

[§3](../tier-3-verification-record.md) is what makes this a decision record rather than a third checklist. For each earlier ticket it names the source file, the commit that source was produced on, and a verdict on whether that source may still be reused at `84b3f54`. The rule underneath is the 8.4 manifest's own: a change under `ASI.Basecode.*/` or `tests/` invalidates the automated suite, and documentation or evidence commits do not. Because the verdict is per source, the record can reuse 8.4's 479 tests without re-running them and still refuse to treat 8.2's blocked rows as host evidence.

The rows come in three kinds, and the distinction between them is the substance of the section:

- **Reusable execution evidence.** The 8.4 manifest is valid because `git diff --stat fa9ae82..84b3f54` touches five files, all under `_bmad-output/`. I re-derived that diff and it is exactly five documentation files, so not re-running the 479 is correct rather than convenient.
- **Reusable as a blocked record.** 8.2, 8.14 and 8.3 are valid precisely because each records that nothing could be executed. The section's own line — a closed issue, a merged tool or a planning document is never substituted — is what stops a closed #75 from quietly becoming host evidence.
- **Reusable as a definition only.** 8.5's coverage matrix is valid as the case-id list, not as execution evidence, and its stale "JPEG only" premise is handed back to 8.5 rather than counted against Tier 3. That is an unusually honest row: the record declines to inherit a claim it knows is wrong.

What the table has no row for is 8.7. PR #226 merged [demo-walkthrough-evidence.md](../demo-walkthrough-evidence.md) onto `dev` after this record was written, and it was executed on the same `84b3f54`. It is the fourth kind of source — reusable execution evidence for the journey T3-04 currently calls blocked — and it is absent from the table entirely.

Two smaller things the table gets wrong: the "Commit / date" column names the commit each source was *produced on*, but neither `execution-manifest-fa9ae82.md` nor `manual-acceptance-checklist.md` exists at the commit it is cited with, so a reader who checks out that commit finds no file. And the section never reconciles this run's environment (build 10.0.26200, SDK 10.0.403) with the reused evidence's (10.0.26100, SDK 10.0.401), even though 8.6 discloses the same difference in its own text.

### Block 4 — The Tier 3 matrix

**Status:** done — accepted by the reviewer 2026-11-12, after re-deriving all seven results

[§4](../tier-3-verification-record.md) is the gate itself: seven rows, one per Tier 3 obligation, each carrying a result word and the evidence that produced it. The shape is deliberate — two rows were executed for this ticket, one is reused from earlier tickets, two are blocked, one needed no work, and one is reserved for the human. Nothing in the matrix is a claim the rest of the document does not support.

The rows, and what decides each:

- **T3-01 cross-feature compatibility — REUSED.** 8.6's route crawl and keyboard/zoom pass, plus the 8.4 suite's concurrency cases. The reuse is legitimate under §3.
- **T3-02 integrated build — PASS.** Executed here; I re-ran it and it holds.
- **T3-03 database migrations — PASS.** Executed here; I rebuilt it from an empty database and it holds, including the two assertions the record omits.
- **T3-04 request-to-certificate journey — BLOCKED.** The one row whose blocker has since dissolved: 8.7's record exists on `dev` at the same commit.
- **T3-05 environment, device and host — BLOCKED.** Seven rows, of which five genuinely need a host, a phone or a human, and two (M-AUTH-01's 8-hour wait, M-ADM-01's scripted race) are local actions parked as external blockers.
- **T3-06 affected retests — PASS (none required).** Correct in substance: no source or test change landed, so nothing needs retesting. The result word is not in the legend.
- **T3-07 human decision — BLANK.** Correctly reserved, and also not in the legend.

Two structural notes. The matrix is where the legend mismatch becomes visible: four of the seven rows use PASS, REUSED or BLOCKED, and three use a qualified PASS, a composite and a blank. And the matrix is the only place a reader learns the gate is blocked — that fact should be at the top of the document, not 1,400 words in.

### Block 5 — The two executed rows: build and migration

**Status:** done — accepted by the reviewer 2026-11-12, after re-running both

[§5](../tier-3-verification-record.md) and [§6](../tier-3-verification-record.md) are the only new evidence in the whole change. Everything else is cited or blocked. §5 is four commands with their exit codes and observed output; §6 is one disposable database and four ordered assertions against the venue image path migration. I re-ran all of it on the same environment the record names, and every recorded observation reproduced.

The build section is unremarkable and that is the point: Release build clean, CSS build clean, five JavaScript tests, 321 service tests. The 479-test integration suite is deliberately absent, and §5 says why — the manifest's own invalidation rule still holds, so re-running it would prove nothing.

The migration section is the part worth reading, because it is the only place the record does something no earlier ticket did. Assertion 1 prefixes legacy `venues/` keys, assertion 2 proves the rerun is a no-op, assertion 3 proves the allow-list guard refuses a database outside `AttendTaMVP_Test` / `AttendTa_IT_*` / `AttendTa_Demo`, and assertion 4 proves an over-length path aborts before any row changes. The prefix, idempotence and over-length behaviours are already automated inside the 479 by `VenueImagePathMigrationTests`; the guard is the one genuinely new observation, and the record says so rather than claiming all four.

Two gaps, both about what the record shows rather than what the migration does. Assertions 1 and 2 state their results without recording the query that produced them, so the check cannot be re-run from the document. And assertion 4's database has no *valid* legacy row left besides the failing one, so "no rows changed" cannot distinguish a real rollback from a migration that had nothing to do. I built both cases: a 492-character key migrates to exactly 500, and with a valid legacy row present the abort leaves it untouched. The migration is correct; the record just does not demonstrate it.

The section also carries two small reproducibility defects. The over-length insert hard-codes `created_by = 1` after capturing the real id, and the guard probe's database name is regenerated per run, so its cleanup only ever drops the current value.

### Block 6 — Blockers and the sign-off block

**Status:** done — accepted by the reviewer 2026-11-12

[§7](../tier-3-verification-record.md) is the most operationally useful part of the document. Every blocked row is broken out into the missing input, the evidence file that owns it, the ticket that owns that file, and the action that would unblock it. A reader who wants the gate closed can work down that table and know exactly who to ask. Eight rows, and the discipline is consistent: no row says "pending" or "TBD", and no row substitutes a closed issue for a real input.

Three of the eight deserve scrutiny:

- The 8.7 demo-journey row is now dead. PR #226 delivered that record at the same commit, so the row describes work that is already done.
- M-AUTH-01 and M-ADM-01 are not external blockers. One is an eight-hour idle wait and the other is a scripted race; both are local actions, and the table's own "unblocking action" column says so. Filing them beside "needs a demo host" overstates what remains.
- The M-ADM-01 row is the most honest entry in the document: it offers two ways out — script the mixed race as a SQL-gated test, or record a human decision that the two same-kind automated races are sufficient — rather than pretending the gap will close itself.

[§8](../tier-3-verification-record.md) is the part that matters most for the release. All five decision fields are blank, the record says sign-off is blocked rather than approved, and it says so twice. That is the correct output for a gate that cannot be closed, and it is the reason this PR does not close #81.

One inconsistency sits between the two sections: §8's "Record complete" field says yes because every row carries a result, but three rows use result words the legend does not define. The claim is true in spirit and false by the document's own vocabulary.

[§9](../tier-3-verification-record.md) records no product defect, which is consistent with everything I re-ran: the build is clean and the migration behaves as documented.

### Block 7 — Periphery

**Status:** done — accepted by the reviewer 2026-11-12

- [The ticket plan](../tickets/epic-release-acceptance/tickets/story-tier-3-manual-system-integration-and-regression-verification-plan.md) — the working record around the deliverable: frozen intent, boundaries, code map, tasks, the quick-lens triage log, and verification. Its frontmatter pins `baseline_revision: 84b3f54`, `route: full` and `risk: high`.
- The ticket file itself is untouched — `tracker_status: backlog` and #81 stays open, which is consistent with a PR that records a blocked gate rather than a closed one.
- No delivery-queue change was needed; [the queue](../delivery-queue.md) already describes 8.8 as "Human Tier 3 acceptance; retest only missing/affected cases", which is what the record does.
- No CI, schema, Razor, CSS or test file is touched. The diff is two markdown files, 384 lines, no deletions.
- The record is not linked from any index. The 8.7 record was added the same way, so this matches house practice rather than diverging from it.

## Outcome

The record is accurate about what it did and honest about what it could not do. Both executed rows reproduce exactly on the environment the record names, all eight §3 verdicts hold against their sources, and the sign-off block correctly refuses to approve a release it cannot approve.

The one substantive defect is that T3-04 went stale between authorship and merge: PR #226 landed the 8.7 demo record on the same tested commit, so the journey the gate calls unproven is in fact proven. The rest is citation hygiene, two result words outside the legend, and two migration assertions the record omits but the migration already satisfies.

Nothing in the change is a product defect. The diff is two markdown files.

**Patched after the review.** Commit `1f5e8d9` on the PR branch reclassifies T3-04 as REUSED, adds the 8.7 source to §3, defines the two result words T3-06 and T3-07 were already using, corrects the four imprecise citations, and drops the dead §7 blocker row.

Left for the author: the opening paragraph still says the record executes "the database migration checks" when only the allow-list guard is new, and the editorial pass — gate status at the top, condensed §4 cells, §5/§6 as an appendix, US spelling — was not applied.
