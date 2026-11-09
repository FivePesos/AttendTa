# Formal review — block 3, the register's commit column

Content: the diff of `_bmad-output/initiative-attendta/demo-mock-register.md` on PR #216, baseline
`2d05a3580645e91aac43770caebe473224ac8f5a`. Class: docs (a planning record that defines the
verification gate's checklist). Lenses: adversarial, edge-case-hunter, structure, prose (on top of
structure). Overlap between lenses is signal, not duplication.

Every commit claim below was checked against git in this worktree.

## Adversarial

| # | Location | Finding | Fix | Consequence |
|---|---|---|---|---|
| A1 | `demo-mock-register.md:100` (M-13a) | `c0c0935` made the summary real but left the demo `validAttendeeCount` and `attendanceRate` behind `hasDemoAttendance ? … : report.*` until `04157b3` deleted the file. Confirmed at `c0c0935`. | `removed (c0c0935; valid-count and rate residue 04157b3)` | A gate inspecting 7.1's tree finds two of the row's three figures still demo-computed. |
| A2 | `:91` (M-06) | `ff06123` deleted the demo lookup services, but `DemoCheckInRegistration.cs` — the DI wiring — survived until `6a82840`. Confirmed. | `removed (ff06123; registration file 6a82840)` | A DI-level sweep reports a live demo provider after the row is marked removed. |
| A3 | `:94` (M-09) | `0abc4f8` removed the Completed branch; `DemoEventStateOverlay` survived until `7f6ae22` for M-04's flag. The 8.11 walkthrough says so; the register does not. | `removed (0abc4f8; overlay class 7f6ae22)` | The register and its own gate evidence disagree on M-09. |
| A4 | `:103`, `:105` (M-14a, M-15a) | The classes named in the Surface cells were deleted by the sibling row's commit: `DemoAccountAccessCommands` by `f232933` (M-14b), `DemoMembershipAccessCommands` by `1853ad9` (M-15b). Both confirmed alive at the recorded sha. | `removed (8230045; class deleted with M-14b, f232933)` and the M-15b equivalent | Resolving the named class at the recorded sha finds it alive — the strongest signal a row is wrong. |
| A5 | `:86` (M-01) | The Surface cell describes the store as at 1.14; after 6.9 it held only pass tokens, which `:139` says. | `the demo store (pass tokens; issued certificate documents until 6.9)` | A gate diffing the cell against `git show 149791b` looks for code that is not there. |
| A6 | `:101` (M-13b) | `removed (975c08b; residue 04157b3)` is the only State value not of the form `removed (<sha>)`. | Keep the cell to one sha; move the residue to the report section and amend rule 11. | Any grep anchored on the single-sha form silently skips the row with the most complicated history. |
| A7 | `:429-432` | The residue paragraph names only the `CourseBreakdown`/`YearBreakdown` fallback, but `975c08b` also left the `hasDemoAttendance` overrides for the valid count, rate and no-show figure. | Name all five leftovers. | A gate that trusts the paragraph stops looking after the two breakdowns. |
| A8 | `:432` | "the residue is recorded here" now points at a fact also in the M-13b cell. | Keep one copy. | The two copies drift on the next edit. |
| A9 | `:71-73` (rule 11) | Rule 11 says one ticket deletes the classes; six rows in this diff are exceptions to it. | Amend rule 11 to define the residue case. | A reviewer applying rule 11 literally rejects the rows this diff corrected. |
| A10 | `:7-9` | "the register is now the only place the ids appear" is false across `_bmad-output` — the ids appear in the delivery queue, deferred work, tickets and walkthroughs. | "no product source file outside this register carries an id." | A repo-wide search returns hits and the register looks wrong about itself. |
| A11 | `:7-9` | "Each demo class carried its id in a comment … the last of those comments went with 8.11" — the five comments 8.11 removed were on the real replacements and assets, not on demo classes; the last demo class went with 8.13 (`149791b`). | Say "each demo class, and each real class that replaced one". | The paragraph reads as off by a ticket. |
| A12 | `:7-9` | The only historical claim in a sha-anchored file that names no sha. | Cite `89b362b`. | The claim that establishes the file's authority is the one a reader cannot verify. |
| A13 | `:7-9`, `:71-73` | The seam is gone, but the rules and "adds the next id" still read as live policy, and the intro deleted the tagging convention without moving it into rule 11. | Add a status line: all rows removed, file is historical; rules apply only if demo data returns. | The next author either re-adds demo code untagged or treats the register as an open backlog. |
| A14 | `story-final-backend-hardening-and-cross-module-integration-cleanup-plan.md:105,107,165,187` | Those lines still say M-01 is `removed (005f899)` and instruct "replace it with the merge commit when the pull request lands" — the opposite of the deleting-commit convention. Confirmed. | Mark the lines superseded. | Two authorities disagree on M-01, and the plan's instruction would reintroduce the defect this diff fixes. |
| A15 | `delivery-queue.md:49` | "Every register row except M-01 is `removed`" — now false. Confirmed at line 49. | "Every register row is `removed`." | A gate cross-checking the register against the phase authority finds a contradiction on the one row this PR is about. |

## Edge-case hunter

| # | Location | Trigger | Guard | Consequence |
|---|---|---|---|---|
| E1 | `:101`, rule 11 `:72` | A row's demo code outlives the commit named in its State cell | Add the residue form to rule 11 | No rule says which of the two commits removed M-13b. |
| E2 | `:429-432` | The demo store holds attendance for the event | Name the no-show fallback too | An auditor believes only the breakdown fallback survived. |
| E3 | `:81-82` and lettered rows | A surface split into lettered rows shares one demo class | State that the later letter's commit deletes the shared class | Checking an earlier letter's commit finds the class still present. |
| E4 | `:101` | The State cell says `975c08b` removed M-13b | The prose says `975c08b` left a fallback and `04157b3` deleted it | Rule 11 is false for M-13b. |
| E5 | `:103`, `:105` | The cited commits deleted the classes named in the rows | `8230045` and `48e0231` modify; `f232933` and `1853ad9` delete | A reader finds the class still present at the recorded sha. |
| E6 | `:7-9` | "The last demo-class id comments went with 8.11" | `149791b` (8.13) deleted the last demo class; `89b362b` (8.11) removed ids from real classes | The header implies demo classes survived past 8.13. |

## Editorial structure

| Pass | Original | Revised | Effect |
|---|---|---|---|
| MOVE-add | Opening (lines 1–9) then 12 rules before the first row | Status line at the top: all 21 rows removed, last removal `149791b`, 8.11 verified `89b362b` | The reader's question is answered 1,659 words below the fold. |
| CUT | M-01 "Switch and guard" (~100 words) | Two lines naming the guard class and call site | Rule 2 already states the same behavior. |
| QUESTION | M-01 "Store" present tense | Rewrite to final state; the store was deleted by 8.13 | `:287` already says the store was removed, so the document contradicts itself. |
| CONDENSE | M-01 and M-03 detail in present tense | Past tense, or one "as of 8.13" frame | Sections are snapshotted at different moments; removed rows read as live. |
| MERGE | M-13 opener and "Removal." paragraph | One paragraph, keeping the new residue sentence | Near-verbatim duplication; this PR added a third restatement. |
| MOVE | "Known limits of the demo" (~207 words) | To `demo-script.md`, keeping the seeded-row 404 limit | Every row is removed; these describe the delivered app. |
| CONDENSE | 12 rules, 874 words | Past-tense one-liners, keeping the numbers | Rules 3, 10 and 12 are cross-referenced, so the numbers must stay. |
| MOVE | Section order | Register order, or an id→section index | The table is sorted by id; the sections are not. |
| MOVE | M-13b State cell | Residue to a footnote; amend rule 11 | One row violates the document's own schema. |
| CUT | "adds the next id (M-16…)" and M-10a's implementer instruction | — | Moot instructions to tickets that have all run. |
| CONDENSE | Introduction's search-workflow sentence | One clause | Explains a mechanism that no longer exists, where the status belongs. |
| — | Section headings | Name the removal ticket in every heading or none | Three of nine carry it. |
| PRESERVE | M-04's 7-char sha, the `> **Removed.**` callouts, the note under the table | — | The short sha matches every other cell; the callouts break 6,279 words of tables. |

## Editorial prose

| Pass | Original | Revised | Changes |
|---|---|---|---|
| prose | "the seed" / "the start-up seed" / "the scenario seed" (L423, L436, L461) | Name the two services distinctly | Three labels for two services, two of them stating opposite contents. |
| prose | Table header "Missing backend today" | "Missing backend before removal" | "today" is false for all 22 rows. |
| prose | L173–175 and L187–188 | Keep the concrete version; cut the first to the real column | The same rendering fact twice in 14 lines. |
| prose | M-10a row: "Replace the creation call/adapter with the completion hook…" | Cut the instruction | An instruction sits on a row removed by 6.4. |
| prose | "**Removal (4.3, 4.5, 4.6).**" (~200 words) | Three paragraphs, one per ticket | One paragraph carries three removals plus a fourth topic. |
| prose | "The contracts are in `ASI.Basecode.Services`; all demo code is in …" | "The demo code lived in …; the rows below name the real replacements" | The lead-in promises demo code but four rows name real services. |
| prose | "Removed in `d3edd37`; the sha is recorded here in the follow-up docs commit…" | "Removed in `d3edd37`." | Explains a process the reader no longer needs. |
| prose | "Fewer rows degrade gracefully: … The seed writes only the attendance and certificate rows…" | Keep the first sentence | Repeats the clause 12 lines above. |
| prose | "ties on the lowest event id"; the 70-word comma-spliced sentence | "ties broken by the lowest event id"; three sentences | "ties on" is not idiomatic; the sentence splices four outcomes. |
| prose | "**Read composition and states (as handed over).**" | Split into "What the demo provider did" and "What the real service does" | The paragraph switches tense mid-sentence with no marker. |
| prose | "Each runs in one transaction…" | "Each command runs in one transaction…" | "Each" has no singular antecedent. |
| prose | "the explicit Check in recorded `student_number`…" | "the explicit **Check in** action recorded … when the search text equaled the student number" | "Check in" reads as a verb; "the query" has no antecedent. |
| prose | "**Scanner follow-up (#138, after PR #170).**" | One sentence, or move it out | Describes delivered UI, not demo data; its "shared store" clause contradicts the store's removal. |
| prose | "A surface that several tickets make real is split…" | "A surface made real by several tickets is split…" | Word order forces a re-read of a sentence governing the table. |
| prose | Five citation shapes (`4.5 (commit ff06123)`, `Removed in d3edd37`, `8.13 (149791b) deleted`, …) | One form: "removed by 4.5 (`ff06123`)" | A verifier grepping for a sha cannot rely on a pattern. |
| prose | "controlled 404" / "controlled not-found" / "a controlled 404" | "a controlled not-found response" | One response named three ways. |
| prose | "at startup" vs "at start-up"; "Start-up seed" | "startup" throughout | Both spellings for one concept. |
| prose | Lists without a serial comma | Match the document's other lists | List punctuation is inconsistent. |
| prose | "Member certificate history and download links - `IMyCertificatesService`" | Em dash | The only hyphen where every other row uses an em dash. |

Five further minor fixes: "its only remaining consumer after M-11b" (unclear whose), slash-joined
alternatives (`M-05/M-06/M-07`, `label/badge`, `GetViewerAsync/GetPdfAsync`), "the Event list" vs
the table's "Officer event list", and a stray double blank line before `## M-08`.
