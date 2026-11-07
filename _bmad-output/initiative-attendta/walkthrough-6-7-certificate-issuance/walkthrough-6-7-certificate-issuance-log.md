# Review log: 6.7 certificate issuance

Target: commits ee822e7..957766a (BMAD 6.7)

## 1 — Orientation — target and narrative created

Session: unavailable · Timestamp: 2026-11-07T22:40:00+08:00

- Action: Reviewed target commits ee822e7..957766a (BMAD 6.7) against ticket 6.7 and its plan; created the narrative with eight blocks.
- Result: Target is the real transactional certificate issuance pipeline; narrative blocks written, all unvisited except Intent.
- Evidence: `_bmad-output/initiative-attendta/tickets/epic-certificates/tickets/story-transactional-certificate-issuance-pipeline-and-audit-plan.md`; `git log --oneline 052e737..HEAD`.

## 2 — Block 1 — Second opinion

Session: unavailable · Timestamp: 2026-11-07T22:52:00+08:00

- Action: Asked a fresh subagent, with no review context, whether the change delivers the stated intent.
- Result: Fit sound. It confirmed the render/promote-outside-locks and locked-recheck shape. It flagged one scope extension: the `DemoMyCertificatesService` M-12 hand-over, which the plan change log justifies under register rule 12. Its "most likely wrong" point is the concurrent duplicate: the pre-check sits outside the locks, so a racing duplicate renders and promotes before the in-transaction refusal, then deletes the file. End state still matches the matrix (no file, no audit, existing bytes unchanged); only wasted work differs.
- Evidence: diff lines 127-205, 223-244, 621-727; plan change log entry on the M-12 hand-over.
- Open: the true concurrent-issuance proof (CERT-01) is assigned by the ticket to 8.5 authoring and 8.4 execution; no concurrency test exists in this change.

## 3 — Block 2 — Test

Session: unavailable · Timestamp: 2026-11-07T23:05:00+08:00

- Action: Ran the certificate suites against LocalDB: CertificateIssuanceTests, CertificateLedgerFlowTests and CertificateReviewDecisionTests.
- Result: 23 of 23 passed in 17s.
- Evidence: `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --filter "FullyQualifiedName~CertificateIssuanceTests|FullyQualifiedName~CertificateLedgerFlowTests|FullyQualifiedName~CertificateReviewDecisionTests"`.
- Open: the missing-retained-file 404 and the concurrent duplicate race have no test. Both were reasoned from `PrivateFileStore.OpenReadAsync` and the lock order, not executed.

## 4 — Block 3 — Formal review (bmad-code-review)

Session: unavailable · Timestamp: 2026-11-07T23:35:00+08:00

- Action: Ran the four-lens formal review over the 6.7 change: blind hunter, edge-case hunter, verification gap and intent alignment.
- Result: 1 decision-needed, 10 patch, 0 defer, 25 rejected. The owner deferred the demo-off viewer/PDF gap to 6.8. All 10 patches applied: member-history, seeded-row and missing-file tests; mid-render attendance and NotFound tests; temporary-file assertions; broader retained-file exception mapping; declined-review message; seed number year; Directory.Exists guard.
- Evidence: plan `## Code Review`; `deferred-work.md`; 13/13 issuance, 1/1 ledger flow, 13/13 ledger+decisions, 310/310 services.
- Open: none from this review; the concurrency race stays with 8.5/8.4.

## 5 — Block 5 — Test

Session: unavailable · Timestamp: 2026-11-07T23:48:00+08:00

- Action: Ran the audit happy-path and audit-rollback tests.
- Result: 2 of 2 passed. The happy path asserts one `certificate.issued` row with the actor, entity id and no `path` in the payload; the rollback test makes the audit writer throw and asserts the certificate row is unchanged and the promoted file deleted.
- Evidence: `dotnet test --filter "FullyQualifiedName~IssueWritesNumberSnapshotsIssuerPathAndOneAuditRow|FullyQualifiedName~CommitFailureDeletesTheNewFileAndLeavesTheRowUnchanged"`.
- Open: the allow-list rejection for a wrong entity type and the description text are not covered by a test, only by inspection.

## 6 — Block 6 — Demo

Session: unavailable · Timestamp: 2026-11-08T00:02:00+08:00

- Action: Ran the real demo flow through the app host with `DemoData:Enabled=true` and real Chromium: seeded event, live completion, decline with reason, approve, issue, officer viewer and PDF, member history and PDF, then deleted the retained file and re-requested both PDFs.
- Result: Passed. The ledger populated from real rows; issuance wrote the retained PDF and one `certificate.issued` audit row; the officer and the owning member both downloaded the retained bytes; the seeded row served the in-memory fallback; the missing file returned 404 on both endpoints.
- Evidence: [ledger desktop](attendta-6-12-ledger-desktop.png), [issue dialog](attendta-6-12-issue-desktop.png), [viewer phone](attendta-6-12-certificate-viewer-phone.png), [ledger phone](attendta-6-12-ledger-phone.png), [decline phone](attendta-6-12-decline-phone.png); `CertificateLedgerFlowTests`.
- Open: none.

## 7 — Wrap-up — review complete

Session: unavailable · Timestamp: 2026-11-08T00:15:00+08:00

- Action: Closed the walkthrough; all eight blocks accepted.
- Result: Review complete. Proposed follow-through: commit the walkthrough notes and screenshots, push `feature/67-certificate-issuance-pipeline`, and open the PR to `dev`.
- Evidence: narrative; commits `ee822e7`, `957766a`, `467975e`.
- Open: the PR is not created; awaiting the owner's confirmation.

## 8 — Wrap-up — follow-through complete

Session: unavailable · Timestamp: 2026-11-08T00:22:00+08:00

- Action: Committed the walkthrough notes and screenshots (`3e5b84b`), pushed `feature/67-certificate-issuance-pipeline`, and opened the PR.
- Result: PR #206 is open against `dev` and mergeable; CI pending.
- Evidence: https://github.com/rainyDayssss/AttendTa/pull/206
- Open: peer approval and green CI before merge.
