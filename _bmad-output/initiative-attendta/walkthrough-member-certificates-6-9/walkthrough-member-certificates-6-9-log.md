# Review log: Real member certificate history (6.9)

Target: PR #210, head commit `787fd61` on `feature/69-real-member-certificate-links` (base `dev`) in F:\AttendTaBMAD

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

Session: unavailable · Timestamp: 2026-11-08T11:32:12+08:00

- Action: Identified target as PR #210 (`787fd61`, `feature/69-real-member-certificate-links` → `dev`), read the ticket (id 9, tracker #69, covers R2) and its built plan, then wrote the narrative with Block 1 only.
- Result: Narrative holds the plan's Intent quoted verbatim with a source note; Blocks 2–4 are placeholders. No block visited or accepted. Plan's single low review finding (namespace comparison scans the wrong assembly in `CertificateReviewDecisionTests`) is recorded there, not yet adjudicated in this review.
- Evidence: [narrative](walkthrough-member-certificates-6-9.md), [plan](../tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md#L112).
- Open: Block 1 awaits the reviewer.

## 2 — Move: Second opinion (Block 1)

Session: unavailable · Timestamp: 2026-11-08T11:35:26+08:00

- Action: Fresh subagent with no review context judged whether the diff delivers the stated intent.
- Result: Intent and diff line up. `Startup.DI.cs` registers `IMyCertificatesService` → `MyCertificatesService` unconditionally; `DemoDataRegistration` registers only `DemoStore` and `DemoSeedService`; owner scope is `m.UserId == userId` in `RegistrationRepository`; PDF delegates to `CertificateLedgerService.GetPdfAsync` (owner-or-management, Issued row, `certificates/` key); `CertificateLedgerFlowTests` covers demo-on 200 `%PDF-`, `CertificateReviewDecisionTests` covers demo-off DI.
- Result: Five findings, no new defects. (1) low — the rewritten test's `DoesNotContain` checks compare Services-assembly types to the WebApp demo namespace, so they are vacuous, and only one switch state is built; already in the plan's triage log, open. (2) info — the page skips cancelled events, unreachable in practice. (3) info — the seeded issued row now shows Unavailable with no number or link, matching the frozen matrix. (4) scope clean — no dangling references to deleted types outside `_bmad-output` docs. (5) switch-off has DI-level coverage only; remaining matrix rows deferred to 8.5 (#78).
- Evidence: [narrative](walkthrough-member-certificates-6-9.md), [plan triage log](../tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md#L112).
- Open: Block 1 not yet accepted by the reviewer.

## 3 — Block 1 accepted; Block 2 presented

Session: unavailable · Timestamp: 2026-11-08T11:36:04+08:00

- Action: Reviewer accepted Block 1 (intent) after the second opinion. Block 1 checked in the narrative; Block 2 (broad strokes) marked current and its body written.
- Result: Block 2 presents four entry points: `GetPageAsync` (actor-scoped page and state), `GetPdfAsync` (member guard plus ledger delegation), the unconditional DI registration, and the demo side reduced to store and seed with no document. No finding adjudicated; the low test finding stays open.
- Evidence: [narrative](walkthrough-member-certificates-6-9.md), [plan](../tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md#L112).
- Open: Block 2 awaits the reviewer.

## 4 — Housekeeping — walkthrough folder committed

Session: unavailable · Timestamp: 2026-11-08T11:40:16+08:00

- Action: Committed the untracked walkthrough folder on the PR branch.
- Result: Commit `263ee51` — "docs(walkthrough): [BMAD 6.9] start the member certificate history walkthrough". Tree clean.
- Evidence: [narrative](walkthrough-member-certificates-6-9.md), [log](walkthrough-member-certificates-6-9-log.md).
- Open: Block 2 still unaccepted.

## 5 — Move: Test (Block 2)

Session: unavailable · Timestamp: 2026-11-08T11:40:16+08:00

- Action: `dotnet build` — 0 warnings, 0 errors. `dotnet test tests/ASI.Basecode.Services.Tests` — 310/310. `dotnet test tests/ASI.Basecode.IntegrationTests` with `ATTENDTA_TEST_SQL` set to LocalDB `master` — 424/424 in 1m39s. Focused runs via `--filter` and `--treenode-filter` both matched zero tests under the Microsoft.Testing.Platform runner, so the full-suite result is the evidence.
- Result: `CertificateLedgerFlowTests` proves, for this change: the owner's history page carries the issued certificate number (`:198`); the member PDF route returns 200 `%PDF-` (`:199-202`); the shared `Certificates/Pdf` route admits the owner with the same bytes and an attachment name (`:206-213`); an inactive membership does not block the owner (`:215-222`); a non-owner member and an unknown id both get 404 (`:226-229`); the seeded issued row with no retained file is a controlled officer 404 (`:189-192`).
- Result: Not covered, deferred to 8.5 (#78) per the plan: unissued-review row shows no link, issued-without-file renders Unavailable, empty history, non-member 403, and switch-off beyond DI resolution. The plan's low finding (vacuous `DoesNotContain` assertions) was re-confirmed by the second opinion; the green suite exercises it neither way. Finding stays open.
- Evidence: [CertificateLedgerFlowTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CertificateLedgerFlowTests.cs#L198), [plan](../tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md#L112).
- Open: Block 2 not yet accepted by the reviewer at the time of this entry.

## 6 — Block 2 accepted; Block 3 presented

Session: unavailable · Timestamp: 2026-11-08T11:51:05+08:00

- Action: Reviewer accepted Block 2 (broad strokes) after the Test move. Block 2 checked; Block 3 (the page query) marked current and written as the first slice.
- Result: Block 3 traces `GetPageAsync`: owned event ids, cancelled-event skip, the actor's own confirmed registration, status-to-state map, `HasDocument` demotion of an issued row without a file, then ordering and 25-row paging. Suggested move: Drive.
- Evidence: [narrative](walkthrough-member-certificates-6-9.md).
- Open: Block 3 awaits the reviewer.

## 7 — Move: Drive (Block 3)

Session: unavailable · Timestamp: 2026-11-08T11:51:05+08:00

- Action: Started the app on http://localhost:5199 against `AttendTaMVP_Test` with `DemoData:Enabled` and `DemoData__AllowDatabase` set; reset four member passwords to a local walkthrough password in that disposable database only. Reviewer drove as Liza.
- Result: Both of Liza's rows show Unavailable with no links (issued-without-file row confirmed). Requesting another member's registration id returns "This certificate is unavailable or you do not have access to it." Angela's row is Declined and Mark's is Pending, both with no link. This closes four deferred manual rows: issued-without-file, declined, pending, and the other-owner refusal message.
- Result: Still unverified: empty history, non-member 403, and the switch-off state.
- Evidence: local run only; no files changed by the drive. [narrative](walkthrough-member-certificates-6-9.md).
- Open: Block 3 not yet accepted by the reviewer.

## 8 — Move: Formal review (Block 3)

Session: unavailable · Timestamp: 2026-11-08T12:01:48+08:00

- Action: Ran `bmad-code-review` over the plan-baseline diff `e757812..HEAD` (15 files) with four lenses: blind-hunter, edge-case-hunter, verification-gap, intent-alignment. None failed. 62 raw findings, 46 rejected, six deduplicated findings written to the plan's new `## Code Review` section.
- Result: 2 decision-needed, 3 patch, 1 defer. F1 medium decision-needed — an issued certificate with no retained file renders as "Not issued" with its number hidden, because the service demotes it to Unavailable and the unchanged partial prints "Not issued". F2 medium patch — the rewritten DI test's `DoesNotContain` assertions scan the Services assembly against the WebApp demo namespace, so they can never fail, and only the switch-off host is built. F3 medium decision-needed — no test exercises the rendered member surface (Issued links, unissued rows, issued-without-file, member-route identical 404, switch-off tab). F4 low patch — `CertificateId` carries the registration id with no comment at the assignment. F5 low patch — the plan's task line still says `removed (pending commit)` while the register records `d16feed`. F6 low deferred — `DemoStore.GetPassToken` has no consumer; pre-existing.
- Result: Notable rejections — the cancelled-event skip is unreachable because cancellation is refused once a certificate is issued and cancels every registration; the N+1 reads are a documented trade; the controller's catch renders an error notice, not an empty tab.
- Evidence: [plan Code Review](../tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md), [narrative](walkthrough-member-certificates-6-9.md).
- Open: F1–F5 await the reviewer's disposition; Block 3 not yet accepted.

## 9 — Patches applied; decisions deferred (Block 3)

Session: unavailable · Timestamp: 2026-11-08T12:10:04+08:00

- Action: Reviewer resolved both decision-needed findings as defers and chose to apply every patch. F1 deferred — the frozen matrix defines the issued-without-file row as Unavailable with no links, and the copy sits in a partial the ticket forbids touching; revisit in 8.5/8.12. F3 deferred — the 8.5 (#78) deferral was approved 2026-11-08 and the rows were driven manually in this walkthrough. F6 stayed deferred. Patched: F2 — `CertificateReviewDecisionTests` now scans `typeof(DemoDataRegistration).Assembly` for both `DoesNotContain` assertions; F4 — comment at the `CertificateId` assignment naming it the registration id and PDF route value; F5 — plan task line now `removed (d16feed)`.
- Result: `dotnet build` clean, 0 warnings, 0 errors; integration 424/424 with `ATTENDTA_TEST_SQL` set. The running app was stopped to unlock the build output and restarted on http://localhost:5199. Plan's `## Code Review` marks both decisions deferred with reasons and the three patches done; `deferred-work.md` gained both entries.
- Evidence: [plan Code Review](../tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md), [narrative](walkthrough-member-certificates-6-9.md).
- Open: Block 3 remains open; patches uncommitted.

## 10 — Block 3 accepted; Block 4 presented

Session: unavailable · Timestamp: 2026-11-08T12:11:32+08:00

- Action: Reviewer accepted Block 3 (the page query) after the Drive and Formal review moves. Block 3 checked; Block 4 (the PDF read) marked current and written; the periphery placeholder renumbered to Block 5.
- Result: Block 4 covers the thin delegation, the member guard, the ledger's owner re-resolution and `certificates/` key requirement, the shared not-found branch, the controller's 404 mapping and single message, and the retained-file read. Suggested moves: Drive and Second opinion.
- Evidence: [narrative](walkthrough-member-certificates-6-9.md).
- Open: Block 4 awaits the reviewer; the F2/F4/F5 patches are still uncommitted.

## 11 — Move: Draw (Block 4)

Session: unavailable · Timestamp: 2026-11-08T12:12:48+08:00

- Action: Authored an SVG of the PDF request path and showed it in the host's Review pane preview.
- Result: The diagram shows the controller's member policy, the service's member guard, the ledger's owner resolution and issued-row requirement, the retained-file read, and the single refusal shape that makes another member's id and an unknown id indistinguishable.
- Evidence: [block-4-pdf-read.svg](block-4-pdf-read.svg), [narrative](walkthrough-member-certificates-6-9.md).
- Open: Block 4 not yet accepted.

## 12 — Block 4 accepted; Block 5 presented

Session: unavailable · Timestamp: 2026-11-08T12:16:53+08:00

- Action: Reviewer accepted Block 4 (the PDF read) after the Draw move. Block 4 checked; Block 5 (periphery) marked current and written.
- Result: Block 5 lists `PRODUCT.md:42`, the register's M-12 row at `:98` and removal detail at `:488`, the plan, and this walkthrough pair. Suggested move: Wrap-up; last block.
- Evidence: [narrative](walkthrough-member-certificates-6-9.md).
- Open: Block 5 awaits the reviewer; the F2/F4/F5 patches remain uncommitted.

## 13 — Wrap-up: review complete, committed and pushed

Session: unavailable · Timestamp: 2026-11-08T12:18:55+08:00

- Action: Reviewer accepted Block 5 and chose the wrap-up follow-through. Committed the work in two commits on the PR branch: `3722b98` "fix(certificates): [BMAD 6.9] make the DI exclusion assertions real" and `f4283ad` "docs(walkthrough): [BMAD 6.9] finish the member certificate history walkthrough". Pushed to `feature/69-real-member-certificate-links`.
- Result: PR #210 updated; its head is now `f4283ad`. CI for the new head was queued at the time of closing. The app on http://localhost:5199 was left running. All five blocks accepted; review complete.
- Evidence: [narrative](walkthrough-member-certificates-6-9.md); https://github.com/rainyDayssss/AttendTa/pull/210.
- Open: CI result on `f4283ad`; PR review and merge.
