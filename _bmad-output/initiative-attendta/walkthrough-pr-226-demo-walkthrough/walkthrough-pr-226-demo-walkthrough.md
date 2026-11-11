# Walkthrough — PR #226, the continuous 26-feature demo walkthrough record

This review covers PR #226, "docs(acceptance): [BMAD 8.7] record the continuous 26-feature demo walkthrough", for ticket [story-full-functional-demo-script-walkthrough-across-all-26-featur.md](../tickets/epic-release-acceptance/tickets/story-full-functional-demo-script-walkthrough-across-all-26-featur.md) (id 7, tracker issue #80, BMAD 8.7). The change is documentation-only: two new files, 477 lines added and nothing deleted, with no product code touched. The evidence lives in [demo-walkthrough-evidence.md](../demo-walkthrough-evidence.md), and the companion plan is [story-full-functional-demo-script-walkthrough-across-all-26-featur-plan.md](../tickets/epic-release-acceptance/tickets/story-full-functional-demo-script-walkthrough-across-all-26-featur-plan.md). Because nothing ships to users, the review is about whether the record is accurate, complete, and honest about what was and was not exercised.

## Blocks

### Block 1 — Intent

**Status:** done — accepted by the reviewer 2026-11-12

Verbatim from the plan's frozen intent section in [the plan file](../tickets/epic-release-acceptance/tickets/story-full-functional-demo-script-walkthrough-across-all-26-featur-plan.md):

> **Problem:** SM-2 / R2 requires all 26 functional IDs plus FR-35 and FR-36 demonstrated end to end on the integrated build. The only evidence is 8.6's per-case checklist (isolated cases, no continuous journey) and 8.4's automated run; no record shows one connected request-to-certificate/report lifecycle, and FR-35/FR-36 have no demo evidence at all.
>
> **Approach:** Execute the demo script's beats as one continuous session on a freshly seeded disposable database — public request, approval, new-member sign-in, master data, event create/publish, registration, pass, check-in, completion, certificate review/issue/download, report, access changes, activity log — then write one walkthrough record that maps FR-1..FR-36 and every one of the 26 functional IDs to the journey step that demonstrates it or to the 8.6/8.4 evidence it reuses. Host/phone-only rows stay BLOCKED.

The ticket itself states the requirement in a single line — "An evaluator executes the full 26-feature-ID user journey demonstration script on the integrated dev build and verifies that every functional requirement from FR-1 through FR-36 succeeds without manual workarounds" — so the block above is the plan's fuller statement of the same intent.

What this is not: a product change. The record is the deliverable; nothing in the app, the schema, the views or the tests moves.

### Block 2 — Broad strokes

**Status:** done — accepted by the reviewer 2026-11-12

The change is one long evidence document plus the plan that produced it. The document has a fixed skeleton: a header that pins the run to a commit and an environment, a legend that defines what each result word means, the journey table that is the actual evidence, and then three cross-reference tables that reorganise the same evidence by requirement id. The plan file is the working record around it — intent, decisions, tasks, and the review findings that were patched before the PR opened.

Five places to open first:

1. [The record header](../demo-walkthrough-evidence.md) — pins the run to commit `84b3f54`, a Release build, a disposable database and a Production app on port 5080, and names the three evidence files it reuses instead of repeating.
2. [The journey table](../demo-walkthrough-evidence.md) — 68 rows, J1 to J68, each one a role, an action, an observed outcome and a result; this is the only place the lifecycle is actually described.
3. [The FR-1..FR-36 table](../demo-walkthrough-evidence.md) — the same evidence indexed by requirement, so an evaluator can find the row that proves FR-n without reading the journey.
4. [The 26 functional-ID table](../demo-walkthrough-evidence.md) — the same again indexed by the 26 IDs the acceptance matrix groups the requirements into.
5. [The blocked rows](../demo-walkthrough-evidence.md) — the seven things this run could not do, each naming the missing input and the file that owns it.

Read in that order, the mechanism is clear: the journey is executed once, and the two index tables are projections of it. Nothing in the document is a second run.

The mechanism is drawn in [record-mechanism.md](record-mechanism.md): one execution feeds the 68 journey rows, and the FR table, the ID table and the blocked table are three projections of those rows, with the reused 8.4/8.6/8.12 evidence cited rather than re-run.

The structural claims were also checked mechanically rather than taken on trust — [check-record.ps1](check-record.ps1) re-derives them from the file. Every claim held: 68 contiguous journey rows, 51 distinct journey references that all resolve, 36 FR rows and 26 functional-ID rows each appearing exactly once, and 7 blocked rows each naming an owning evidence file. One claim did not hold, and it is in the plan rather than the record — see the finding below.

**Finding (low):** the plan states the blocked-row count twice and disagrees with itself. [The plan's Implementation Notes](../tickets/epic-release-acceptance/tickets/story-full-functional-demo-script-walkthrough-across-all-26-featur-plan.md) say "6 blocked rows"; its Auto Run Result says "7"; the record has 7. The number is right in the record and in the PR description, so this is a stale figure in the plan's own prose, not a missing row.

The record's claims were also driven in a real browser rather than only read. I reset the disposable database, seeded it, ran the Production build on port 5080, and the reviewer pushed seven of the record's beats:

| Claim | What the reviewer did | Result |
|---|---|---|
| J1 — the public form has no role field | Opened `/AccountRequest/Request` and submitted it | Confirmed; acknowledgment reads "Your membership request is pending review." |
| J54 — the sign-in refusal is generic | Signed in with a wrong password | Confirmed: "Incorrect email or password." |
| J5 — the admin home | Signed in as administrator | Confirmed; the organization and records cards are present |
| J2 — the request queue and approve dialog | Opened `/AccountRequest` | Confirmed; the dialog names the applicant and student number. The queue showed the seeded request plus the reviewer's own submission, not the record's Ava Mendoza row, because that row was the journey's own data. |
| J64 — officer cannot read the audit log | Officer opened `/admin/auditlogs` | Confirmed; redirected to Access Denied |
| J17 — the signup deadline | Member tried to register for the started event | Confirmed: "Signup for this event is closed." |
| §8 note 2 — seeded registrations carry no QR token | Member opened `/Passes` | Confirmed; no pass card renders for a seeded confirmed registration |

Every behavioural claim that was driven held. Counts and named rows did not reproduce, and should not: the record describes one journey on its own data, while this run started from the seed alone. That is the expected difference between a demonstration and a reproduction, not a discrepancy in the record.

### Block 3 — The 68 journey rows

**Status:** done — accepted by the reviewer 2026-11-12 after the formal review and the six fixes

This is the only part of the record where the lifecycle is actually described. Sixty-eight rows, each a role, an action, an observed outcome and a result. What makes it evidence rather than a list is that the rows are adjacent and the state carries: the registration confirmed at J28 is the pass rendered at J30, the same pass is scanned at J34, the same attendance is invalidated at J38, and the same event is completed at J42 to produce the certificate review at J43. The 8.6 checklist tested those as separate cases; this table tests them as one chain, which is the thing the ticket asked for and the thing no other file shows.

The rows fall into eight phases:

1. J1–J4 — anonymous request, admin approval, and the new member signing in with the password typed on the public form.
2. J5–J12 — admin home, master data, then a draft event, its cover, and publication.
3. J13–J22 — member home, feed, event detail, signup, My Events, profile, self-cancellation and the re-registration refusal.
4. J23–J29 — officer dossier, two completion refusals, and registration review.
5. J30–J41 — pass, QR PNG, check-in window, scan, duplicate scan, manual lookup, member attendance view, invalidation and blocked re-scan.
6. J42–J51 — completion, certificate review, decline, issuance, PDF, member certificate, and the participation report.
7. J52–J59 — account deactivation with its cascade, reactivation, the last-administrator guard, membership toggle, audit log and role change.
8. J60–J68 — published edit, toggles, venue photo replacement, post-completion refusal, officer denied, logout, and the two FR-35/FR-36 rows.

Four rows are worth reading closely, because they are where a record like this usually cheats:

- [J17](../demo-walkthrough-evidence.md) — a refusal recorded as PASS, and it does not stop at the message: it also states the registration count was 0 before and after. A refusal that silently wrote a row would look identical in the UI.
- [J34 and J35](../demo-walkthrough-evidence.md) — the duplicate scan returns the *original* check-in timestamp rather than a fresh one, and the row count stays at one. That is the difference between "the second scan was rejected" and "the second scan was absorbed".
- [J63](../demo-walkthrough-evidence.md) — the honest composite: "PASS (refusal observed; exact start boundary REUSED)". It claims only the post-completion refusal it actually saw and hands the start-cutoff boundary to the automated suite.
- [J67 and J68](../demo-walkthrough-evidence.md) — 26 real requests submitted through the public form to force a second page. The temptation is a SQL insert; the record uses the form, and the plan defends why that is data entry rather than a fixture.

What to look for while reading: the rows that assert an **absence** — "no second row", "count 0 before and after", "searching alone added no attendance row", "no cancellation control". Those are the rows that would quietly pass even if the feature were broken, so they are the ones worth distrusting, and the record states them explicitly rather than leaving them implied.

A formal review ran over this block with four lenses — adversarial, edge-case hunter, structure and prose. The lenses returned 158 raw observations; [formal-review.md](formal-review.md) holds the triaged set after checking each one against the code and the files.

Six factual errors survived, all small and all fixable: the plan says "6 blocked rows" where the record has 7 and the plan's own summary says 7; it says "Two stale demo-script expectations" where the record has three; J65 says logout redirects to `/` when `AccountController.Logout` redirects to `/Account/Login`; the legend and the plan both cite an "evidence field" that does not exist in the journey table; the plan's task list names a `browser` header field the record never has; and the record calls .NET SDK 10.0.403 a "patch level".

Two gaps matter more than the typos. FR-8 asks for upload **and replace** of record images, and the journey replaces a venue photo but never replaces an event cover — the row is marked PASS without that half being exercised or marked REUSED. And §11's "Record complete: Yes" conflates *every row is filled* with *every row is evidenced*, which is the one thing a human signer most needs to be able to tell apart.

The review also dismissed six plausible-sounding findings after checking them, and one dismissal is worth recording because it came back in the record's favour: the 8.4 manifest is cited for a commit seven commits later, and `git diff --stat fa9ae82 84b3f54` excluding `_bmad-output` and `docs` is empty — the range is documentation only, so the REUSED rows are sound.

The six factual errors were fixed in the working tree rather than deferred: the blocked-row count, the stale-note count, J65's logout redirect, the phantom evidence field in two places, the `browser` header field, and the "patch level" wording for the SDK version. [check-record.ps1](check-record.ps1) now reports all checks passing, including the plan and record agreeing on seven blocked rows.

The two substantive gaps were left open deliberately — they are judgement calls for the human signer, not typos. FR-8's missing event-cover replacement and §11's conflation of "filled" with "evidenced" both change what the sign-off means, so they belong in the evaluator's decision rather than in a quiet edit.

### Block 4 — The FR-1..FR-36 mapping

**Status:** done — accepted by the reviewer 2026-11-12

Thirty-six rows, one per requirement, each pointing at the journey step that demonstrates it or at the evidence it reuses. This is the completeness proof: the evaluator's question is not "does the app work" but "is every requirement accounted for", and this table is the answer.

The rows are not uniform, and the differences are the review. Thirty of the thirty-six are a plain PASS against a named journey step. The other six are where the record makes a claim the demo did not fully earn:

- [FR-12](../demo-walkthrough-evidence.md) and [FR-17](../demo-walkthrough-evidence.md) are **REUSED in full** — the journey never cancelled an event and never filled one, so cancellation and waitlist promotion rest entirely on the 8.4 automated suite. Two of thirty-six requirements were not demonstrated at all.
- [FR-19](../demo-walkthrough-evidence.md) is split: the start-cutoff boundary is REUSED, and only the post-completion refusal was observed. The plan's triage log explains why — the status guard fires before the cutoff guard, so the journey could not reach the cutoff message.
- [FR-9](../demo-walkthrough-evidence.md) is split the same way: draft creation was demonstrated, draft editing was not.
- [FR-23](../demo-walkthrough-evidence.md) demonstrates the pass-code path and leaves camera scanning BLOCKED.
- [FR-8](../demo-walkthrough-evidence.md) is the one row the formal review disputed: it is marked PASS, but the requirement is upload **and replace**, and the journey replaced a venue photo rather than an event cover.

The judgement for the evaluator is whether "REUSED" is acceptable evidence for a demo sign-off. The record is explicit that it is not a substitute for the automated suites and says so in the legend; what it does not do is state the consequence — that a signer who accepts this record is accepting the 8.4 suite's authority for two requirements and part of three more.

### Block 5 — The 26 functional-ID mapping

**Status:** done — accepted by the reviewer 2026-11-12

The same evidence again, indexed by the 26 functional IDs that `acceptance-coverage-matrix.md` groups the requirements into — AUTH-01..03, MD-01..05, EVT-01..04, REG-01..05, QR-01, ATT-01..03, CERT-01..02, REP-01, ADM-01..02. Where §5 answers "is every requirement covered", this table answers "is every ID the acceptance matrix names covered", and it is the join between this record and that matrix.

Twenty-four of the twenty-six are a plain PASS. The two exceptions are the same two holes as §5, seen from the other side: [REG-04](../demo-walkthrough-evidence.md) is REUSED in full, and [ATT-03](../demo-walkthrough-evidence.md) is a composite — attendance history PASS, camera scanning BLOCKED. [REG-05](../demo-walkthrough-evidence.md) is also composite, splitting the deadline refusal that was demonstrated from the start cutoff that was not.

What this table adds beyond §5 is the citation of reused evidence for clauses the journey did not run: AUTH-02 hands the anonymous refusal and the antiforgery variants to three automated suites, and ADM-02 hands append-only storage and audit atomicity to `TransactionAuditTests`. Those citations are the difference between an honest composite and an unsupported one.

### Block 6 — Blocked rows and the no-workaround claim

**Status:** done — accepted by the reviewer 2026-11-12

Two sections carry the record's honesty, and they are the two a sceptical evaluator should read first.

The [blocked table](../demo-walkthrough-evidence.md) names seven things this run could not do: camera scanning on a real hotspot HTTPS origin, demo-host readiness, backup and restore, column-level grants on the audit log, the mixed demote-versus-deactivate race between two administrators, the eight-hour idle expiry, and human approval of the certificate signatory. Each row names the missing input and the evidence file that owns it. None of them is quietly folded into a PASS.

The [no-workaround statement](../demo-walkthrough-evidence.md) is the claim the ticket actually turns on: every state change was a real MVC form POST, with no fixture, no browser-storage trick, no direct SQL state edit and no fake issued outcome. It discloses the one exception rather than hiding it — SQL was read to obtain the pass's own `qr_token`, because the pass page renders only the QR image and never exposes the token, and the scanner endpoint needed a real token to exercise.

Two things the formal review raised here. The blocked rows do not say which FR or functional ID each belongs to, so a reader cannot confirm that nothing is silently unrepresented — OPS-01, OPS-02, M-AUD-01, M-AUTH-01 and CERT-03 appear nowhere in §4 to §6. And the token disclosure is bounded but not fully: the record says the value is not reproduced, but not when it was read or how the right row was found.

### Block 7 — The plan file

**Status:** done — accepted by the reviewer 2026-11-12

The plan is the working record around the deliverable: a frozen intent block, decisions, boundaries, a code map, nine task lines, eight acceptance criteria, implementation notes, a nine-finding triage log, design notes, verification commands, and an auto-run summary.

Two parts earn their place. The [triage log](../tickets/epic-release-acceptance/tickets/story-full-functional-demo-script-walkthrough-across-all-26-featur-plan.md) is the audit trail: nine findings raised before the PR opened, each with the file it was verified against and what changed as a result — FR-12 downgraded from PASS to REUSED, FR-19's start cutoff marked REUSED, ATT-03 given one composite result instead of two contradictory ones. And the [auto-run result](../tickets/epic-release-acceptance/tickets/story-full-functional-demo-script-walkthrough-across-all-26-featur-plan.md) contains the most honest sentence in the whole PR: every PASS in the record is the executing agent's testimony about a live run that a reviewer cannot re-observe, so the record's accuracy rests on the database state it left behind and the cited suites, not on an independent replay.

The rest is duplication. Implementation Notes and Auto Run Result restate the same execution summary; the findings breakdown re-lists the nine findings the triage log already carries with more evidence; Verification and Verification performed list the same four commands. None of it is wrong, and a plan is allowed to be redundant — but a reader looking for the decision has to get past the restatement.

### Block 8 — Periphery

**Status:** done — accepted by the reviewer 2026-11-12

There is almost none, which is the point. The PR adds two files and changes nothing else: no build registration, no dependency injection, no schema, no test, no Razor view, no CSS. Nothing needs wiring up and nothing can regress.

The only peripheral concern is that the record cites four pre-existing files it must stay consistent with — `demo-script.md`, `manual-acceptance-checklist.md`, `execution-manifest-fa9ae82.md` and `acceptance-coverage-matrix.md`. Three of the record's own notes are exactly that consistency failing: two places where `demo-script.md` describes behaviour the app does not have, and one where the seeded data does not carry the tokens the script assumes. Those are recorded as notes against the script rather than fixed, which is the right call for this ticket but does leave the script stale.

## Outcome

All eight blocks accepted 2026-11-12.

**Six factual errors were fixed in the working tree** (branch `pr-226`, uncommitted): the plan's blocked-row count 6 to 7, its stale-note count two to three, J65's logout redirect, the phantom evidence field in the legend and the plan's manual check, the `browser` header field in the plan's task list, and "patch level" to "SDK version".

**Two substantive gaps were left open as evaluator decisions:** FR-8's undemonstrated event-cover replacement, and §11's conflation of "every row is filled" with "every row is evidenced".

**What the review added beyond reading the document:** a live drive that reproduced seven of the record's claims in a browser, a mechanical check of the record's own structural claims, and a verification that the 8.4 manifest citation is sound (`git diff --stat fa9ae82 84b3f54` excluding `_bmad-output` and `docs` is empty).

The human sign-off block in the record stays blank. That is the evaluator's to fill, and the ticket cannot close without it.
