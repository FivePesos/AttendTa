# Review narrative: Remove prototype data and prove complete real integrations (8.11)

Target: PR #216, branch `feature/152-remove-prototype-data-and-prove-real-integrations` against `dev` in `rainyDayssss/AttendTa`, baseline `2d05a3580645e91aac43770caebe473224ac8f5a`

Review status: **In progress — reopened for block 3.**

Review moves: Second opinion, Formal review, Test, Drive, Demo, Draw, Wrap-up.

---

## Blocks

## Block 1 — Intent

- [x] Block 1 — Intent (accepted 2026-11-09)

Source: ticket #152, pasted verbatim.

> Verify only; this ticket changes no code. Confirm every row of `demo-mock-register.md`, M-01 included, is `removed` with its commit; an open row sends the finding back to that row's removing ticket instead of being fixed here. Then prove the demo seam is gone with checks that fail if any part remains: start the published build with DemoData__Enabled=true, no DemoData__TokenKey and a database other than AttendTa_Demo, which the startup guard refuses while the seam exists; and search the source and the published output for every register id, the demo data folder and the DemoData configuration keys. Finally verify My Events, passes, attendance, scanner, completion, certificate review, issue and download, access changes and reports against one coherent database. Keep only isolated test fixtures in the test projects. This is a functional integration gate, not a frontend redesign or a claim that the final test suite already passed.

Where it comes from: [story-remove-prototype-data-and-prove-complete-real-integrations.md](../tickets/epic-release-acceptance/tickets/story-remove-prototype-data-and-prove-complete-real-integrations.md) (Description), with the plan's framing in [the plan](../tickets/epic-release-acceptance/tickets/story-remove-prototype-data-and-prove-complete-real-integrations-plan.md): the seam was deleted by 8.13 (`149791b`) but nothing had proven it, and the gate's own search already failed on five leftover register-id comments, a split literal in a test, and a wrong commit in the register.

Why now: this is the last gate before release acceptance; 8.12 and the demo rehearsal consume its verdict.

## Block 2 — Broad strokes

- [x] Block 2 — Broad strokes (accepted 2026-11-09)

Five entry points, in reading order:

1. [The verification record](../walkthrough-8-11-remove-prototype-data/walkthrough-8-11-remove-prototype-data.md) — the gate's evidence: the 21-row register verdict, the startup check, the two searches, and the end-to-end run.
2. [The demo-mock register](../demo-mock-register.md) — the 21 rows, each now naming the commit that deleted its demo code (table starts at line 84).
3. [CheckInWindowFlagTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CheckInWindowFlagTests.cs) — `RetiredSeamSwitchKeyIsInertWhenEnabled` at line 66, the one place the retired key may appear because it is the check.
4. [CheckInService.cs](../../../ASI.Basecode.Services/Services/CheckInService.cs) — line 14, a representative of the five files whose register-id comment was dropped so the gate's own search passes.
5. [The plan](../tickets/epic-release-acceptance/tickets/story-remove-prototype-data-and-prove-complete-real-integrations-plan.md) — the decisions, the verification command list, and the review triage log.

The change is verify-only, so nothing in the product behaves differently. What the diff actually contains is the evidence record, the register's commit column, one test literal, and five comment lines.

Diagram: [block-2-gate-checks.svg](block-2-gate-checks.svg) — the four checks, the five stale comments and the split literal.

Tested in this review: see the log's Test move entry.

## Blocks not walked

Block 4 (periphery) was not walked: the review closed at block 2 and reopened only for block 3.

## Block 3 — Slices

- [ ] Block 3 — Slices (presented, awaiting review)

### Slice 1 — the five comment files

The register defined each demo id as a deletion marker in a comment on the demo class; when the tickets deleted the classes, five comments outlived their code. The gate searches for the ids, so the comments had to go. Each edit removes only the parenthetical and the sentence is reflowed to read without it.

Worked example: [CheckInService.cs](../../../ASI.Basecode.Services/Services/CheckInService.cs#L14) line 14 — `/// The real door scanner (register M-05, removed by 4.3). The screen composes…` becomes `/// The real door scanner. The screen composes…`.

Same treatment: [CheckInLookupService.cs](../../../ASI.Basecode.Services/Services/CheckInLookupService.cs#L14) line 14 (M-06), [scanner.js](../../../ASI.Basecode.WebApp/wwwroot/js/features/scanner.js#L1) line 1 (M-05), [scanner-camera.js](../../../ASI.Basecode.WebApp/wwwroot/js/features/scanner-camera.js#L1) line 1 (M-05), [passes.css](../../../ASI.Basecode.WebApp/Styles/features/passes.css#L1) line 1 (M-03). `passes.css` is a Tailwind source, so `app.css` was rebuilt; the PR shows no `app.css` diff, which is the expected result.

### Slice 2 — the test literal

The inertness test proves the host starts with the retired switch on. To keep the 8.11 search blind, it assembled the name from two words — which made the search's "one intentional hit" impossible, because the check hid itself. Now [CheckInWindowFlagTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CheckInWindowFlagTests.cs#L73) line 73 reads `const string retiredSection = "DemoData";` and the comment says this test is the one place the retired name may appear, because it is the check.

### Slice 3 — the register's commit column

The register is the gate's checklist. A row that says only `removed` cannot be checked, and a row naming the wrong commit misleads. This slice fills the commit for nine rows, corrects M-01, shortens M-04's full SHA to the same short form, records M-13b's residue, and rewrites the introduction because the code no longer carries the ids. See [demo-mock-register.md](../demo-mock-register.md#L84) line 84.

M-01: `005f899` is the PR #209 merge, which still contained all five seam files; `149791b` is the deletion. Both checked.

M-13b: `975c08b` removed the demo path but left the `CourseBreakdown`/`YearBreakdown` fallback; `04157b3` deleted the file. The row keeps `975c08b` and names the residue.

Open question from the second opinion: M-08's `d3edd37` deletes no demo class — it adds the real query and replaces the empty body, and the demo class went in `6668425` (M-02). The row still reads `removed (d3edd37)` and the record's table says "no demo class remained", so the row is accurate but the record's status line overstates.

### Slice 4 — the evidence record and its log

The record is the gate's product. Five sections: the 21-row verdict table, the startup check, the search table, the end-to-end table, and "what this record does not claim". The log holds the commands, the environment and the nine first-pass failures. See [the record](../walkthrough-8-11-remove-prototype-data/walkthrough-8-11-remove-prototype-data.md) and [its log](../walkthrough-8-11-remove-prototype-data/walkthrough-8-11-remove-prototype-data-log.md).

Thin spots: the end-to-end evidence is prose with no raw output; the log promises every check enumerated but only the failures are; `pass-qr-download` is scored a pass after three 404s.
