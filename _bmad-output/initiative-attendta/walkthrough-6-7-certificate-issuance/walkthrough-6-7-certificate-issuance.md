# Walkthrough — 6.7 Transactional certificate issuance pipeline and audit

Target: commits `ee822e7..957766a` on `feature/67-certificate-issuance-pipeline`
Ticket: [story-transactional-certificate-issuance-pipeline-and-audit.md](../tickets/epic-certificates/tickets/story-transactional-certificate-issuance-pipeline-and-audit.md)

- [x] **Block 1 — Intent**
- [x] Block 2 — Broad strokes
- [x] Block 3 — The issuance transaction
- [x] Block 4 — Render and promote outside the locks
- [x] Block 5 — The audit action
- [x] Block 6 — Demo hand-over
- [x] Block 7 — Tests
- [x] Periphery

## Block 1 — Intent

Status: done · unchanged during review

> **Problem:** Issuance is demo-only (M-11a). It renders in memory, publishes to the in-memory store, writes no retained PDF and appends no audit, so an issued certificate cannot be downloaded, survives no restart, and leaves no Activity Log entry.
>
> **Approach:** Implement `CertificateLedgerService.IssueAsync` behind the existing 6.12 contract: pre-check outside locks, render and promote the PDF outside locks, then in one caller-owned transaction lock the owner user and the event, recheck approval, Valid Attendance and Completed, and persist the number, snapshots, issuer, relative path and a `certificate.issued` audit entry. Delete the new file when the commit fails. Hand demo issuance to the real service and keep the demo viewer on the real retained file.

Source: the plan's frozen Intent section, distilled from ticket 6.7. The ticket itself is [story-transactional-certificate-issuance-pipeline-and-audit.md](../tickets/epic-certificates/tickets/story-transactional-certificate-issuance-pipeline-and-audit.md) and the plan is [story-transactional-certificate-issuance-pipeline-and-audit-plan.md](../tickets/epic-certificates/tickets/story-transactional-certificate-issuance-pipeline-and-audit-plan.md).

## Block 2 — Broad strokes

Status: done · unchanged during review

Open these five in order and the mechanism is clear:

1. [CertificateLedgerService.cs:166](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L166) — `IssueAsync`, the orchestrator: pre-checks, render, then the transaction.
2. [CertificateLedgerService.cs:326](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L326) — `IssueInsideTransactionAsync`: locks, recheck, persist, audit.
3. [CertificateLedgerService.cs:386](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L386) — `RenderAndPromoteAsync`: render bytes and move them into `certificates/`.
4. [DemoCertificateLedgerService.cs:41](../../../ASI.Basecode.WebApp/DemoData/DemoCertificateLedgerService.cs#L41) — the demo hand-over: delegates issuance, serves the retained file.
5. [CertificateIssuanceTests.cs:24](../../../tests/ASI.Basecode.IntegrationTests/CertificateIssuanceTests.cs#L24) — the guard suite for every refusal.

## Block 3 — The issuance transaction

Status: done · unchanged during review

Discovery and the cheap refusals run before any lock. The transaction then takes the owner's `users` row, then the event (AD-4 order), re-reads the tracked certificate, and collapses every change that landed during rendering into one `Stale`. On success it writes the number, the three snapshots, the issuer name, the relative path and the issuer/time, saves, appends one audit entry, and saves again.

- [CertificateLedgerService.cs:166](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L166) — pre-checks and the transaction call.
- [CertificateLedgerService.cs:326](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L326) — the locked section.
- [CertificateLedgerService.cs:426](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L426) — `FindReviewAsync`, and the `NotFound` for a row with no certificate id.
- [CertificateLedgerService.cs:517](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L517) — the audit payload; it deliberately omits the path.

## Block 4 — Render and promote outside the locks

Status: done · unchanged during review

Chromium and disk I/O never run under a lock. The renderer returns bytes, the bytes go to the private temporary area, and `PromoteAsync` moves them into `certificates/` under a fresh GUID name with no overwrite. A render, empty-result or promotion failure deletes the temporary file; a failed commit deletes only the promoted key this attempt created, so a retry can never replace an issued PDF.

- [CertificateLedgerService.cs:386](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L386) — `RenderAndPromoteAsync`.
- [CertificateLedgerService.cs:435](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L435) — `DeleteBestEffort`.

## Block 5 — The audit action

Status: done · unchanged during review

One new action, wired through the same four places as every other audited mutation.

- [AuditActions.cs:36](../../../ASI.Basecode.Resources/Constants/AuditActions.cs#L36) — `CertificateIssued`.
- [Audit.resx:38](../../../ASI.Basecode.Resources/Messages/Audit.resx#L38) — the description text.
- [AuditDescriptions.cs:43](../../../ASI.Basecode.Resources/Messages/AuditDescriptions.cs#L43) — the action-to-template switch.
- [ActivityLogWriter.cs:35](../../../ASI.Basecode.Services/Services/ActivityLogWriter.cs#L35) — the `certificates` allow-list entry.

## Block 6 — Demo hand-over

Status: done · changed during review (screenshots added)

The demo provider no longer issues. It delegates the three commands and serves the officer viewer and PDF from the real issued row and its retained file. Only the seeded issued row, which has no retained file, still falls back to the in-memory document until 6.8 removes M-11b. The member history was handed over in the same change so a newly issued certificate still appears there and downloads from its retained file.

- [DemoCertificateLedgerService.cs:41](../../../ASI.Basecode.WebApp/DemoData/DemoCertificateLedgerService.cs#L41) — delegation.
- [DemoCertificateLedgerService.cs:66](../../../ASI.Basecode.WebApp/DemoData/DemoCertificateLedgerService.cs#L66) — the PDF read and its not-found mapping.
- [DemoMyCertificatesService.cs:76](../../../ASI.Basecode.WebApp/DemoData/DemoMyCertificatesService.cs#L76) — the member history read.
- [DemoStore.cs](../../../ASI.Basecode.WebApp/DemoData/DemoStore.cs) — `PublishCertificate` and `GetCertificateNumber` deleted.

## Block 7 — Tests

Status: done · changed during review

Thirteen focused tests with a fake renderer and a real temporary-rooted private store cover every matrix row, including the commit-failure cleanup and the review-added cases: attendance invalidated mid-render, an unknown registration, and a confirmed registration with no review. The end-to-end ledger flow now asserts a persisted row, one retained `%PDF-` file, one audit row, the member history and PDF, the seeded-row fallback, and a missing file returning 404.

- [CertificateIssuanceTests.cs:24](../../../tests/ASI.Basecode.IntegrationTests/CertificateIssuanceTests.cs#L24) — the new suite.
- [CertificateLedgerFlowTests.cs:18](../../../tests/ASI.Basecode.IntegrationTests/CertificateLedgerFlowTests.cs#L18) — the demo end-to-end, renamed to its persisted behaviour.
- [CertificateReviewDecisionTests.cs:236](../../../tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs#L236) — the factory updated for the new constructor.

## Periphery

Status: done · changed during review

- [demo-mock-register.md:96](../demo-mock-register.md#L96) — M-11a marked removed at `58cffca`; M-11b text corrected to the retained-file read.
- [story-transactional-certificate-issuance-pipeline-and-audit-plan.md](../tickets/epic-certificates/tickets/story-transactional-certificate-issuance-pipeline-and-audit-plan.md) — implementation notes, review triage and design notes.
