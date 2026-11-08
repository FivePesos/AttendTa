# Review log: Authorized certificate streaming download endpoint (6.8)

Target: PR #207, commits `124ef29` and `f5ba1b9` on `feature/68-authorized-certificate-download-endpoint` in F:\AttendTaBMAD

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

Log entry:
Use format below. Omit empty fields. In Result, give each finding's
disposition: fixed, accepted as-is, deferred, or open. When a
disposition changes, append a new entry; do not edit earlier ones.
Record the actual session ID supplied by the environment and an ISO
8601 timestamp with local timezone for each entry. If the session ID
is unavailable, write "unavailable" rather than inventing one.

Token budget: up to 150 tokens per entry.
The smaller the better, but do not sacrifice precision.
-->

## 1 — Preparation — narrative and log created

Session: unavailable · Timestamp: 2026-11-08T00:00:00+08:00

- Action: Read the story 6.8 ticket and its approved plan, the PR #207 description, and the two-commit diff (`2d31909..f5ba1b9`, 12 files).
- Result: Narrative to be drafted with 6 blocks: intent, broad strokes, service authorization and streaming, controller and demo removal, tests, periphery. No block visited, accepted, or marked complete.
- Evidence: [narrative](walkthrough-certificates-6-8.md), [plan](../tickets/epic-certificates/tickets/story-authorized-certificate-streaming-download-endpoint-plan.md).
- Open: Block 1 awaits the reviewer.

## 2 — Move: Second opinion (Block 1)

Session: unavailable · Timestamp: 2026-11-08T00:20:00+08:00

- Action: Fresh subagent with no review context read the ticket, plan, epic R6 context and the `2d31909..f5ba1b9` diff, and judged whether the intent names the right problem and every expected refusal.
- Result: Sound with caveats. Plan intent (`plan.md:24`) is honest and scoped; the ticket Description (`story-...endpoint.md:19`) states mechanism, not the demo-only gap. I/O matrix has no explicit row for an unissued row (registration exists, review pending/declined) though the code and one test cover the 404. Ticket `:41` says "the screen should not change visibly" while `plan.md:26,102` and `demo-mock-register.md:546` accept the seeded row's officer viewer/PDF becoming a 404.
- Evidence: [narrative](walkthrough-certificates-6-8.md), [plan](../tickets/epic-certificates/tickets/story-authorized-certificate-streaming-download-endpoint-plan.md).
- Open: Block 1 still awaits the reviewer; the three caveats are unadjudicated.

## 3 — Move: Formal review (Block 1)

Session: unavailable · Timestamp: 2026-11-08T00:45:00+08:00

- Action: Ran `bmad-code-review` over `2d31909..f5ba1b9` with four lenses (blind-hunter, edge-case-hunter, verification-gap, intent-alignment); none failed. Triaged 22 raw findings.
- Result: 0 high, 1 medium, 7 low; 8 patch, 0 defer, 0 decision-needed, 13 rejected. The medium is a verification gap: no test writes an issued row whose `file_path` is outside `certificates/`, so the service-level prefix refusal can be removed without CI noticing.
- Result: Intent-alignment found no defect; it recorded that the intent is written at the HTTP surface while the decision logic lives in the service, and that tests observe only HTTP. Two intent caveats from the earlier second opinion remain unadjudicated (unissued-row matrix row; ticket `:41` "screen should not change visibly").
- Evidence: plan `## Code Review` section; [narrative](walkthrough-certificates-6-8.md).
- Open: patch handling awaits the reviewer; Block 1 still unaccepted.

## 4 — Patches applied (formal review)

Session: unavailable · Timestamp: 2026-11-08T01:05:00+08:00

- Action: Reviewer chose "apply every patch". Applied all 8: a tampered `file_path` (`uploads/venues/not-a-certificate.png`) asserted 404 for officer, owner and viewer then restored; `no-store` asserted on the owner PDF; the prefix comment narrowed to an area hint; the register's Known limits now names the ledger's dead View PDF link; the M-12 row notes it owns the last in-memory document path; the `DemoSeedService` summary reworded to this run's guard; the plan Code Map line numbers refreshed; the switch-off branch asserted in `CertificateReviewDecisionTests`.
- Result: `dotnet build` clean, 0 warnings. Services 310/310. Focused integration `CertificateLedger|CertificateReviewDecision|CertificateIssuance` 26/26. All 8 patch items checked off in the plan's `## Code Review`.
- Evidence: plan `## Code Review`; working tree modified, not committed.
- Open: Block 1 still unaccepted; tree is dirty.

## 5 — Second-opinion caveats settled (Block 1)

Session: unavailable · Timestamp: 2026-11-08T01:20:00+08:00

- Action: Reviewer asked to settle the two intent caveats before accepting Block 1. Reworded the ticket Description to open with the demo-only gap, and its Delivery note to name the seeded row's controlled 404 as the one visible change.
- Result: Ticket, plan and register now state the same problem and the same visible change. No code touched. The third second-opinion caveat (no explicit I/O-matrix row for an unissued row) was not in the reviewer's list and remains open.
- Evidence: [story-authorized-certificate-streaming-download-endpoint.md](../tickets/epic-certificates/tickets/story-authorized-certificate-streaming-download-endpoint.md), [narrative](walkthrough-certificates-6-8.md).
- Open: Block 1 acceptance; the ticket edit is uncommitted.

## 6 — Block 1 accepted; Block 2 presented

Session: unavailable · Timestamp: 2026-11-08T01:30:00+08:00

- Action: Committed the ticket reword as `c228863`. Reviewer accepted Block 1 (intent). Block 1 checked in the narrative; Block 2 (broad strokes) marked current.
- Result: Block 2 presented with five entry points: `GetPdfAsync`, `RequireIssuedDocumentAsync`, `ReadRetainedAsync`, the controller's per-action policy, and the demo registration. Suggested moves: Draw and Test.
- Evidence: [narrative](walkthrough-certificates-6-8.md).
- Open: Block 2 awaits the reviewer; tree clean.

## 7 — Move: Test (Block 2)

Session: unavailable · Timestamp: 2026-11-08T01:55:00+08:00

- Action: Ran the full integration suite (424/424) and the services suite (310/310). Mapped every I/O-matrix row to its assertion. Then found the new tampered-key test was not discriminating: with the prefix clause removed it still passed, because the venue file did not exist and `FileNotFoundException` mapped to the same 404.
- Result: Wrote a real venue PNG to `<SetupRoot>/uploads/venues/not-a-certificate.png` before tampering the row. Mutation-verified: with the `certificates/` clause removed the request returns 200 OK; restored, 404. Committed as `e206178`. Focused suite 26/26 after the change.
- Result: Not covered, deliberately — the demo switch-off state end-to-end (the DI test covers registration only; a second host costs file watchers), the member-history route (6.9), and concurrent downloads.
- Evidence: [narrative](walkthrough-certificates-6-8.md), `tests/ASI.Basecode.IntegrationTests/CertificateLedgerFlowTests.cs:261`.
- Open: Block 2 still unaccepted.

## 8 — Block 2 accepted; Block 3 presented

Session: unavailable · Timestamp: 2026-11-08T02:05:00+08:00

- Action: Reviewer accepted Block 2 (broad strokes) after the Test move. Block 2 checked; Block 3 (service authorization and streaming) marked current.
- Result: Block 3 covers the `RequireManagement` split, the owner check from `DiscoverReviewAsync`, the shared issued-and-retained gate, and the contained read. Suggested moves: Drive and Second opinion.
- Evidence: [narrative](walkthrough-certificates-6-8.md).
- Open: Block 3 awaits the reviewer; tree clean except the untracked walkthrough folder.

## 9 — Move: Drive (Block 3) — app running

Session: unavailable · Timestamp: 2026-11-08T02:20:00+08:00

- Action: Started the WebApp on http://localhost:5080 against `AttendTaMVP_Test` with `DemoData:Enabled=true` and a local token key. Set a local walkthrough password on users 1 (admin), 3 and 4 (members) only; no plaintext password or hash written to the repo.
- Result: Demo seed wrote 8 attendance rows and 8 certificate reviews on event 1 (Published, started). Registration 1 is Issued with no `file_path` — the controlled 404. Registrations 2/4/5/6/7 are pending, 3/8 not eligible. Verified: `/Account/Login` 200, admin sign-in works, `/Certificates` 200 (empty until the event is completed), `/Event/1` 200 with an enabled Complete button.
- Evidence: local run only; no files changed by the drive.
- Open: reviewer drives complete → approve → issue → viewer/download → owner read → seeded-row 404; Block 3 still unaccepted.

## 10 — Drive confirmed; Block 3 accepted; Block 4 presented

Session: unavailable · Timestamp: 2026-11-08T02:40:00+08:00

- Action: Reviewer drove the flow and confirmed it works: completed event 1, approved and issued a review, opened the viewer, downloaded the PDF, read the same certificate as its owner, and got the identical 404 as a non-owner and for an unknown id.
- Result: Block 3 (service authorization and streaming) accepted. Block 4 (controller and demo removal) marked current and presented: the policy moved from the class to the five management actions, `Pdf` left authenticated-only, the demo provider deleted, and the registration line removed.
- Evidence: [narrative](walkthrough-certificates-6-8.md); app still running on http://localhost:5080.
- Open: Block 4 awaits the reviewer.

## 11 — Move: Demo (Block 4)

Session: unavailable · Timestamp: 2026-11-08T02:55:00+08:00

- Action: Exercised the running app directly. Officer viewer for the seeded row (`/Certificates/Viewer/1`) returned 404 with the "Certificate not found." alert, captured as a screenshot. Owner read (`/Certificates/Pdf/2` as joshua.reyes) returned 200, `application/pdf`, `Cache-Control: no-store,no-cache`, 446,093 bytes starting `%PDF-`. Non-owner liza.fernandez got 404 for id 2 and for 999999, both carrying the same alert; the bodies differ only by the id in the page's retry link.
- Result: The controller split and the demo removal behave as described. The seeded row is the controlled 404; the real ledger serves the owner in the demo switch-on state. Switch-off was not re-driven (the DI test covers the registration; a second host costs file watchers).
- Evidence: [transcript](walkthrough-certificates-6-8/demo-transcript.md), [seeded-row 404](walkthrough-certificates-6-8/demo-seeded-row-404.jpeg), [owner certificate](walkthrough-certificates-6-8/demo-owner-certificate.pdf).
- Open: Block 4 still unaccepted.

## 12 — Block 4 accepted; Block 5 presented

Session: unavailable · Timestamp: 2026-11-08T03:05:00+08:00

- Action: Reviewer accepted Block 4 (controller and demo removal) after the Demo move. Block 4 checked; Block 5 (tests) marked current and its body rewritten to include the tampered-key test and the switch-off assertion.
- Result: Block 5 lists ten pinned behaviours and the two-state DI test. Suggested moves: Draw and Second opinion.
- Evidence: [narrative](walkthrough-certificates-6-8.md).
- Open: Block 5 awaits the reviewer.

## 13 — Moves: Second opinion and Draw (Block 5)

Session: unavailable · Timestamp: 2026-11-08T03:20:00+08:00

- Action: A fresh subagent with no review context read the two test files, the service, the controller and the plan matrix, and named what the suite does not pin. Drew the two gate chains as Mermaid and rendered them in the Review pane.
- Result: Seven gaps raised. The three that matter: served bytes are compared only to each other, never to the retained file on disk, so an in-memory re-render would stay green (`CertificateLedgerFlowTests.cs:182,186,254`); no member ever requests `/Certificates/Viewer`, so removing both the policy and `RequireManagement` would stay green; the identical not-found is compared by status only, so a distinct non-owner message would pass. The rest are low: admin-role coverage, PDF-signature refusal, switch-off end-to-end, and an eligible row with a `file_path`.
- Result: No patches applied; the reviewer has not adjudicated these.
- Evidence: [diagram](walkthrough-certificates-6-8/block-5-tests.md), [narrative](walkthrough-certificates-6-8.md).
- Open: Block 5 still unaccepted; the seven gaps are unadjudicated.

## 14 — Second-opinion gaps 1–3 closed

Session: unavailable · Timestamp: 2026-11-08T03:40:00+08:00

- Action: Reviewer asked to close the three gaps that mattered. Added: served bytes compared to the retained file on disk for both the officer and the owner read; a member request to `/Certificates/Viewer/{id}` asserted as an AccessDenied redirect; the not-found alert block compared between a non-owner and an unknown id with only the retry-link id normalised.
- Result: First attempt compared whole bodies and failed on the per-request antiforgery token; switched to the alert block. Focused suite 26/26. Committed as `46a0de1`.
- Result: Gaps 4–7 (admin role, PDF-signature refusal, switch-off end-to-end, eligible row) left open and unadjudicated.
- Evidence: `tests/ASI.Basecode.IntegrationTests/CertificateLedgerFlowTests.cs`.
- Open: Block 5 still unaccepted.

## 15 — Block 5 accepted; Block 6 presented

Session: unavailable · Timestamp: 2026-11-08T03:50:00+08:00

- Action: Reviewer accepted Block 5 (tests) after the second opinion, the diagram, and closing gaps 1–3. Block 5 checked; Block 6 (periphery) marked current and its body rewritten to include the review's doc changes.
- Result: Block 6 lists the plan, the reworded ticket, the register's three edits, the DI comment and the M-12 relabels, plus what was deliberately left untouched. Suggested move: Wrap-up.
- Evidence: [narrative](walkthrough-certificates-6-8.md).
- Open: Block 6 awaits the reviewer; last block.

## 16 — Wrap-up: review complete, push offered

Session: unavailable · Timestamp: 2026-11-08T04:00:00+08:00

- Action: Reviewer accepted Block 6 and asked for the wrap-up. All six blocks checked; narrative status set to complete.
- Result: Branch is `ahead 4` of `origin/feature/68-authorized-certificate-download-endpoint`, which is the PR #207 head repo (`Wynnwrath/AttendTav2`). PR is OPEN and MERGEABLE; CI on the old head was green. Offered to push the four review commits and to commit the walkthrough folder.
- Evidence: [narrative](walkthrough-certificates-6-8.md); PR https://github.com/rainyDayssss/AttendTa/pull/207.
- Open: push decision; the walkthrough folder is still untracked, as the 2.9 review left its own.

## 17 — Pushed; PR #207 updated

Session: unavailable · Timestamp: 2026-11-08T04:10:00+08:00

- Action: Pushed `f5ba1b9..46a0de1` to `origin/feature/68-authorized-certificate-download-endpoint`.
- Result: PR #207 now lists all six commits, remains OPEN and MERGEABLE, and the `build-and-test` CI check is QUEUED on the new head (run 42396377445).
- Evidence: https://github.com/rainyDayssss/AttendTa/pull/207.
- Open: CI result on the new head; PR review and merge. Walkthrough folder left untracked, as the 2.9 review left its own.
