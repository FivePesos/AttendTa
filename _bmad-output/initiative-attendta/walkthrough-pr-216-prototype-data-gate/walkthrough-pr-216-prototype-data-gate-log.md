# Review log: PR #216 — remove prototype data and prove complete real integrations

Target: https://github.com/rainyDayssss/AttendTa/pull/216 (branch `feature/152-remove-prototype-data-and-prove-real-integrations`, base `dev`, baseline `2d05a3580645e91aac43770caebe473224ac8f5a`)

## Block status

| Block | Area | Status |
| ----- | ---- | ------ |
| 1 | Intent | accepted |
| 2 | Broad strokes | accepted |
| 3 | Slices | accepted |
| 4 | Periphery | accepted |

## 1 — Session — walkthrough opened

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Created the narrative and this log for the walkthrough review of PR #216.
- Result: Both files initialized; blocks 1–4 registered as pending. No content reviewed yet.
- Evidence: [narrative](walkthrough-pr-216-prototype-data-gate.md)
- Open: Awaiting block content from the orchestrator.

## 2 — Block 1 (intent) — presented

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Added Block 1 to the narrative: ticket #152's Description quoted verbatim, plus provenance (ticket and plan) and the release-gate timing. Listed the review moves once at the top of the narrative.
- Result: Block 1 presented, awaiting review. No findings recorded; nothing examined or changed.
- Evidence: [narrative](walkthrough-pr-216-prototype-data-gate.md); [story-remove-prototype-data-and-prove-complete-real-integrations.md](../tickets/epic-release-acceptance/tickets/story-remove-prototype-data-and-prove-complete-real-integrations.md); [plan](../tickets/epic-release-acceptance/tickets/story-remove-prototype-data-and-prove-complete-real-integrations-plan.md).
- Open: Block 1 not yet accepted. Blocks 2–4 pending.

## 3 — Block 1 (intent) — accepted

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Marked Block 1 accepted in the narrative at the reviewer's direction.
- Result: Block 1 accepted. No findings raised against the intent.
- Evidence: [narrative](walkthrough-pr-216-prototype-data-gate.md#block-1--intent)
- Open: Blocks 2–4 pending.

## 4 — Block 2 (broad strokes) — presented

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Added Block 2 to the narrative: five entry points in reading order (verification record, register, the flag test, a representative comment-only file, the plan), plus a closing note that the change is verify-only.
- Result: Block 2 presented, awaiting review. No findings recorded.
- Evidence: [narrative](walkthrough-pr-216-prototype-data-gate.md#block-2--broad-strokes); [verification record](../walkthrough-8-11-remove-prototype-data/walkthrough-8-11-remove-prototype-data.md); [register](../demo-mock-register.md); [CheckInWindowFlagTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CheckInWindowFlagTests.cs); [CheckInService.cs](../../../ASI.Basecode.Services/Services/CheckInService.cs); [plan](../tickets/epic-release-acceptance/tickets/story-remove-prototype-data-and-prove-complete-real-integrations-plan.md).
- Open: Block 2 not yet accepted. Blocks 3–4 pending.

## 5 — Block 2 (broad strokes) — move: Draw

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Drew the block-2 diagram: the four gate checks and the residue that blocked the search. Hand-authored SVG, viewable in a browser or an editor preview.
- Result: Diagram saved and linked from the end of Block 2 in the narrative. No new claims beyond the block's text; block status unchanged.
- Evidence: [block-2-gate-checks.svg](block-2-gate-checks.svg); [narrative](walkthrough-pr-216-prototype-data-gate.md#block-2--broad-strokes)
- Open: Block 2 not yet accepted. Blocks 3–4 pending.

### Test move — block 2 (2026-11-09)

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

Worktree at PR head `2005351`.

- Action: Ran the gate's own checks independently of the PR's record.
- Search, register ids (`rg -n "M-0[1-9]|M-1[0-5][ab]?"` over Data/Services/Resources/WebApp, excluding bin, obj, node_modules): 0 hits.
- Search, seam names (`rg -n "DemoData|DemoStore|DemoSeedService|DemoDataRegistration|demo-pass-token"` over the repo, excluding `_bmad-output`, bin, obj, node_modules, .git): 1 hit — `tests/ASI.Basecode.IntegrationTests/CheckInWindowFlagTests.cs:73`, the inertness test.
- `ASI.Basecode.WebApp/DemoData/`: absent.
- Published output (`dotnet publish -c Release -o <temp>`): both searches 0 hits; no `DemoData` folder.
- Register commits: all 21 recorded SHAs resolve to commits. The 12 rows naming a demo class were confirmed to delete that file in the recorded commit (M-01, M-02, M-03, M-04, M-05, M-06, M-07, M-11b, M-12, M-13c, M-14b, M-15b). The remaining 9 rows were checked for commit existence only.
- M-01 correction confirmed: `DemoDataOptions.cs` exists at `005f899` and does not exist at `149791b`.
- `dotnet build ASI.Basecode.sln -warnaserror`: 0 warnings, 0 errors.
- `dotnet test ... --filter FullyQualifiedName~RetiredSeamSwitchKeyIsInertWhenEnabled` with `ATTENDTA_TEST_SQL` on LocalDB: 1 passed, 0 failed (59s).
- Published build started from its publish directory with `DemoData__Enabled=true`, no `DemoData__TokenKey`, `AttendTaMVP_Test` (not `AttendTa_Demo`), Production: `GET /Account/Login` returned 200.
- Result: Every check the gate names passes on the PR head. Not re-run: the full services and integration suites, and the 51-check end-to-end run. Still unverified by this move: the 9 register rows whose commit names no demo class, and the end-to-end flows.
- Evidence: [narrative](walkthrough-pr-216-prototype-data-gate.md#block-2--broad-strokes); [CheckInWindowFlagTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CheckInWindowFlagTests.cs#L73); [register](../demo-mock-register.md).
- Open: Block 2 not yet accepted. Blocks 3–4 pending.

### Second opinion — block 2 (2026-11-09)

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

Asked of a fresh subagent with no review context: "Does this record actually prove what it claims, or does it ask the reader to take something on trust?"

- Requirement 1 (register rows): mostly sufficient. The hole is M-08, whose commit `d3edd37` deletes no demo class — it adds the real query and replaces the empty body, and `DemoMyEventsService.cs` had already gone in `6668425` (M-02). The record's own table concedes this, so the status line's "every register row … with the commit that deleted its demo code" is imprecise for M-08.
- Requirement 2 (seam gone): sufficient and independently checkable. The startup evidence is thin — env vars plus a 200, no console capture — but cheap to redo and backed by the CI test.
- Requirement 3 (real flows): not sufficient for a reader who was not there. The driver was temporary and uncommitted, credentials ephemeral, the database dropped, and the report row gives no figures.
- Weakest claim named: the log's statement that every check is enumerated with request, expected result and observed value. In fact only the nine first-pass failures are enumerated; the 42 passes exist as a count. Also notes `pass-qr-download` failed three consecutive runs and is still scored a pass.
- Orchestrator confirmed the M-08 point: `git show --stat d3edd37` shows additions only, no demo-class deletion.
- Result: Record proves requirement 2 outright, requirement 1 with one imprecise row, and requirement 3 only for someone who was present.
- Open: Block 2 not yet accepted. Blocks 3–4 pending.

## 6 — Block 2 (broad strokes) — accepted

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Marked Block 2 accepted and closed the review at Wrap-up at the reviewer's direction.
- Result: Block 2 accepted; review closed. Two blocks walked, two not.
- Evidence: [narrative](walkthrough-pr-216-prototype-data-gate.md#block-2--broad-strokes)
- Open: None. Blocks 3–4 not walked.

## 7 — Blocks 3–4 — not walked

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Recorded that blocks 3 (slices) and 4 (periphery) were not walked — the reviewer chose Wrap-up after block 2.
- Result: Both marked `skipped` in the block-status table. No block content invented; the narrative carries a `## Blocks not walked` note instead.
- Evidence: [narrative](walkthrough-pr-216-prototype-data-gate.md#blocks-not-walked)
- Open: None.

## 8 — Session — review reopened; block 3 presented

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Reviewer reopened the review and asked for block 3. Added `## Block 3 — Slices` to the narrative with four subsections (the five comment files, the test literal, the register's commit column, the evidence record and its log); changed the top status line to reopened; narrowed `## Blocks not walked` to block 4 only.
- Result: Block 3 presented, awaiting review. Block 4 remains `skipped`. Supersedes entries 6–7 and the Outcome section for block 3 only; a fresh Outcome is expected at final close.
- Evidence: [narrative](walkthrough-pr-216-prototype-data-gate.md#block-3--slices); [CheckInService.cs](../../../ASI.Basecode.Services/Services/CheckInService.cs#L14); [CheckInWindowFlagTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CheckInWindowFlagTests.cs#L73); [register](../demo-mock-register.md#L84); [record](../walkthrough-8-11-remove-prototype-data/walkthrough-8-11-remove-prototype-data.md).
- Open: Block 3 not yet accepted. Block 4 not walked.

### Formal review — block 3 (2026-11-09)

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

- Scope: the diff of `demo-mock-register.md` against baseline `2d05a35`; class docs.
- Lenses: adversarial, edge-case-hunter, structure, prose (on top of structure). Skill: bmad-review.
- Results: 15 adversarial, 6 edge-case, 13 structure, 19 prose + 5 minor.
- Verified in this session against git: M-13a's demo overrides survive at `c0c0935`; `DemoCheckInRegistration.cs` alive at `6a82840^`; `DemoAccountAccessCommands.cs` alive at `8230045`; `DemoMembershipAccessCommands.cs` alive at `48e0231`; the M-01 Surface cell contradicts the store detail at line 139; the two plan files still say `005f899` and instruct replacing it with the merge commit; `delivery-queue.md:49` still says "Every register row except M-01 is removed".
- Reports: [block-3-formal-review.md](block-3-formal-review.md) and [block-3-formal-review-findings.json](block-3-formal-review-findings.json). The skill's `output_format` is `both`, so the full markdown report and the raw JSON array were written to those files and a condensed report was presented in chat.
- Result: Findings recorded for triage; none patched by this move.
- Open: Block 3 not yet accepted. Block 4 not walked.

### Follow-through (2026-11-09)

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

- Decision: reviewer chose to fix the register and the two cross-document lines, then commit and push.
- Register edits: M-06 `removed (ff06123; registration file 6a82840)`; M-09 `removed (0abc4f8; overlay class 7f6ae22)`; M-13a `removed (c0c0935; valid-count and rate residue 04157b3)`; M-14a `removed (8230045; class deleted with M-14b, f232933)`; M-15a `removed (48e0231; class deleted with M-15b, 1853ad9)`; M-13b cell reworded to `breakdown residue 04157b3`.
- Rule 11 amended to define the `removed (<sha>; <what> <sha>)` form.
- M-01 Surface cell now says pass tokens, with issued certificate documents until 6.9.
- Introduction: scoped the id claim to product source, named the real classes the last five comments sat on, cited `89b362b`, and added a status line stating all 21 rows are removed.
- M-13 detail paragraph now names all five leftovers from `975c08b` and the `c0c0935` residue.
- `delivery-queue.md:49`: "Every register row is `removed`."
- 8.13 plan lines 105, 107, 165, 187: M-01 marked corrected to `149791b`; the "replace it with the merge commit" instruction reversed.
- 8.11 walkthrough M-13b row: evidence now names the leftover overrides as well as the breakdown fallback.
- Commit `0d449e0` on `feature/152-remove-prototype-data-and-prove-real-integrations`; pushed to origin. PR #216 head is now `0d449e0`, 17 files, +973 / −30.
- Result: Register and both cross-document lines corrected and pushed. No CI checks are reported on the branch.
- Evidence: [register](../demo-mock-register.md); [delivery queue](../delivery-queue.md#L49); [plan](../tickets/epic-release-acceptance/tickets/story-remove-prototype-data-and-prove-complete-real-integrations-plan.md); [record](../walkthrough-8-11-remove-prototype-data/walkthrough-8-11-remove-prototype-data.md).
- Open: Block 3 not yet accepted. Block 4 not walked. CI unobserved.

### Verification and merge (2026-11-09)

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

- Services suite: 321 passed, 0 failed, 0 skipped (`--no-build`).
- Integration suite on LocalDB with `ATTENDTA_TEST_SQL`: 460 passed, 0 failed, 0 skipped (1m 24s).
- Register table integrity: rows 90–113 all carry 7 pipes after the edits.
- PR #216 merged into `rainyDayssss/AttendTa` `dev` at 2026-11-09T07:57:56Z by wigglebop25; merge commit `e81a9b3`, which contains `0d449e0` and `2005351`.
- Result: Block 3 accepted and the review closed. No CI checks were configured on the branch, so the two suites above are the verification of record.
- Evidence: [narrative](walkthrough-pr-216-prototype-data-gate.md); [register](../demo-mock-register.md#L90)
- Open: None. Block 4 not walked.

## 9 — Block 4 (periphery) — presented

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Reviewer asked whether all blocks were done; added `## Block 4 — Periphery` to the narrative and removed the `## Blocks not walked` section. Top status line now reads "In progress — block 4 presented after the merge".
- Result: Block 4 presented, awaiting review. One finding recorded: the plan's frontmatter claims `lenses_ran: ['quick']` while its own triage log records the quick lens as not returning and the inspection as a deviation, not a lens result. No build, DI, csproj or schema registration in the PR.
- Evidence: [narrative](walkthrough-pr-216-prototype-data-gate.md#block-4--periphery); [plan](../tickets/epic-release-acceptance/tickets/story-remove-prototype-data-and-prove-complete-real-integrations-plan.md); [record](../walkthrough-8-11-remove-prototype-data/walkthrough-8-11-remove-prototype-data.md); [record log](../walkthrough-8-11-remove-prototype-data/walkthrough-8-11-remove-prototype-data-log.md).
- Open: Block 4 not yet accepted. Supersedes entry 7 and the "block 4 not walked" clauses in entries 8–9 and the Verification and merge entry.

## 10 — Block 4 (periphery) — accepted; review closed

Session: unavailable · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Marked Block 4 accepted and closed the review at the reviewer's direction.
- Result: Block 4 accepted; review closed. All four blocks walked.
- Evidence: [narrative](walkthrough-pr-216-prototype-data-gate.md#block-4--periphery)
- Open: None. One finding left open by choice — the plan's `lenses_ran` frontmatter.

## Outcome

- The gate's checks were independently re-run and pass: searches clean in source and published output, build 0/0, the inertness test passing, the published build serving 200 with the retired switch on.
- Five register rows named a commit that did not remove the class or residue they describe; all corrected, with rule 11 defining the two-part form.
- Two records outside the register contradicted it; both corrected.
- One finding left open by choice: the plan's frontmatter records `lenses_ran: ['quick']` while its triage log says the quick lens did not return.
