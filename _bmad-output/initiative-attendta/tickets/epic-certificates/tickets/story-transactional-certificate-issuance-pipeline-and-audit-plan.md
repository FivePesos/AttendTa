---
title: 'Transactional certificate issuance pipeline and audit'
type: 'feature'
ticket: '7'
created: '2026-11-07'
status: 'built'
baseline_revision: '052e737c817fc2282d77d9808679c5ad3d975f85'
route: 'full'
route_source: 'auto'
risk: 'high'
review: 'quick'
review_source: 'pinned'
lenses_ran: [quick]
review_loop_iteration: 0
context:
  - '_bmad-output/initiative-attendta/demo-mock-register.md'
  - '_bmad-output/initiative-attendta/finale-test-cases.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Issuance is demo-only (M-11a). It renders in memory, publishes to the in-memory store, writes no retained PDF and appends no audit, so an issued certificate cannot be downloaded, survives no restart, and leaves no Activity Log entry.

**Approach:** Implement `CertificateLedgerService.IssueAsync` behind the existing 6.12 contract: pre-check outside locks, render and promote the PDF outside locks, then in one caller-owned transaction lock the owner user and the event, recheck approval, Valid Attendance and Completed, and persist the number, snapshots, issuer, relative path and a `certificate.issued` audit entry. Delete the new file when the commit fails. Hand demo issuance to the real service and keep the demo viewer on the real retained file.

## Boundaries & Constraints

**Always:** Reuse `ICertificateLedgerService`, `CertificateLedgerController` and the views; Management authorization; render and file I/O outside every lock; one caller-owned transaction; lock the owner `users` row then the event (AD-4); recheck `eligible`, Valid Attendance and Completed inside the transaction; append the audit before the save; relative `file_path` only; keep the demo screen populated and unchanged.

**Never:** A second screen, controller or contract; schema or migration changes; bulk issuance, revocation or reissue; a certificate lock AD-4 does not require; audit outside the transaction; hard-coded actor ids; a caller-supplied path; replacing an issued PDF on retry; the 6.8 owner-or-Management download endpoint.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Issue | `eligible`, valid attendance, completed event | `issued` with number, snapshots, issuer, path; one `certificate.issued` audit; retained PDF | No error |
| Duplicate | already `issued` | No render, no file, no audit; existing path and bytes unchanged | `Duplicate` |
| Not approved | `pending_approval` or `not_eligible` | Nothing written | `Invalid` |
| Invalidated attendance | `invalidated_at` set | Nothing written | `Stale` |
| Changed during render | decision or invalidation lands mid-render | Nothing written; the new file is deleted | `Stale` |
| Render failure | renderer throws or returns non-PDF | Nothing written; temporary file deleted | `Invalid` |
| Commit failure | SQL failure after promote | New file deleted; row unchanged | Propagates |
| Non-management caller | member role | Refused | `Forbidden` |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/Services/CertificateLedgerService.cs:25-33,136-172,300` — add `ICertificatePdfRenderer` and `IPrivateFileStore` to the primary constructor; replace the `IssueAsync` stub; keep `GetViewerAsync`/`GetPdfAsync` unavailable for 6.8.
- `ASI.Basecode.Services/Services/CertificateLedgerService.cs:174-242` — `DecideInsideTransactionAsync` is the transaction idiom to copy: discover outside, `ExecuteInTransactionAsync`, `LockUserAsync` then `LockEventAsync`, tracked re-read, recheck, mutate, `SaveChangesAsync`, `audit.Append`, `SaveChangesAsync`.
- `ASI.Basecode.Data/Interfaces/ICertificateRepository.cs:28` — `GetByRegistrationIdAsync` returns the tracked row; no new repository method is needed.
- `ASI.Basecode.Data/Interfaces/IRegistrationRepository.cs:94` — `RegistrationReviewIdentity(UserId, MemberId, EventId)` from `DiscoverReviewAsync`; `HasValidAttendanceAsync`.
- `ASI.Basecode.Data/Interfaces/IUserRepository.cs:21` — `GetByIdAsync` supplies the recipient name (`identity.UserId`) and the issuer name (`actorUserId`).
- `ASI.Basecode.Data/Interfaces/IEventRepository.cs:12-13` — `GetEventAsync` for the snapshot; `LockEventAsync` returns the locked `Event` for the Completed recheck.
- `ASI.Basecode.Services/Interfaces/IPrivateFileStore.cs:20-32` — `SaveAsync(Temporary)` → `PromoteAsync(Certificates)` → relative key; `Delete(key)` for cleanup; certificates require `.pdf` and `%PDF-`.
- `ASI.Basecode.Services/Interfaces/ICertificatePdfRenderer.cs:12` — `RenderSnapshotAsync(CertificatePdfSnapshot)`; snapshot is `(RecipientName, EventTitle, EventStartsAt, IssuedAt, CertificateNumber)`.
- `ASI.Basecode.Resources/Constants/AuditActions.cs:34-35`, `Messages/AuditDescriptions.cs:41-42`, `Messages/Audit.resx`, `Services/Services/ActivityLogWriter.cs:35` — add `certificate.issued`, its description and the `certificates` allow-list entry.
- `ASI.Basecode.WebApp/DemoData/DemoCertificateLedgerService.cs:49-111,113-176` — delete the demo issuance and the store overlay; delegate `IssueAsync` to the real service; read the real row and `file_path` for viewer/PDF.
- `ASI.Basecode.WebApp/DemoData/DemoStore.cs:63-79,97-103` — delete `PublishCertificate` and `GetCertificateNumber`; keep the seeded document members used by the fallback.
- `ASI.Basecode.WebApp/DemoData/DemoCertificateDocuments.cs:13-25` — remains the fallback for the seeded row that has no retained file.
- `ASI.Basecode.WebApp/DemoData/DemoSeedService.cs:267-284` — the seeded issued row still seeds the in-memory document; no change to its SQL seeding.
- `tests/ASI.Basecode.IntegrationTests/CertificateLedgerFlowTests.cs:159-188` — the demo flow now persists a file and an extra audit row; update those assertions.
- `tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs:236-329` — copy the `Service(...)` factory and `SeedAsync` fixture shape for the new test class.
- `tests/ASI.Basecode.IntegrationTests/PrivateFileStoreTests.cs:132-148` — copy `CreateStore()` for a real store rooted in a temp `Common:SetupRoot`.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Resources/Constants/AuditActions.cs`, `Messages/Audit.resx`, `Messages/AuditDescriptions.cs`, `ASI.Basecode.Services/Services/ActivityLogWriter.cs` -- add `CertificateIssued = "certificate.issued"`, its resource description, and the `certificates` allow-list entry -- issuance must be auditable.
- [x] `ASI.Basecode.Services/Services/CertificateLedgerService.cs` -- inject the renderer and file store; implement `IssueAsync` as discover → pre-check → render → temp/promote → transaction (locks, recheck, mutate, audit) → delete on failure -- the real pipeline.
- [x] `ASI.Basecode.WebApp/DemoData/DemoCertificateLedgerService.cs` -- delegate `IssueAsync` to the real service; remove the store overlay and the demo participation checks; read the real row and `file_path` in `GetViewerAsync`/`GetPdfAsync`, falling back to the in-memory document only when `FilePath` is null -- M-11a removal with no visible change.
- [x] `ASI.Basecode.WebApp/DemoData/DemoStore.cs` -- delete `PublishCertificate` and `GetCertificateNumber`; update the class comment -- no demo issuance path remains.
- [x] `ASI.Basecode.WebApp/DemoData/DemoMyCertificatesService.cs` -- hand the M-12 member read over to the real issued row and retained `file_path`, keeping the seeded in-memory document only when `FilePath` is null -- register rule 12.
- [x] `tests/ASI.Basecode.IntegrationTests/CertificateIssuanceTests.cs` -- new fixture with a fake renderer and a real temp-rooted `PrivateFileStore`: issue, duplicate, not approved, invalidated, render failure, non-PDF render, changed-during-render, non-management, and file/audit assertions -- the focused guard.
- [x] `tests/ASI.Basecode.IntegrationTests/CertificateLedgerFlowTests.cs` -- assert one persisted file and the `certificate.issued` audit row instead of "no files" -- keep the demo end-to-end true.
- [x] `_bmad-output/initiative-attendta/demo-mock-register.md` -- set M-11a `removed` and correct the M-11b text to the real-file read -- register rule 12.

**Acceptance Criteria:**
- Given an eligible review with valid attendance on a completed event, when an Officer issues it, then the row is `issued` with a unique number, recipient/event/start snapshots, issuer name, a relative `certificates/...` path, issuer and time, and exactly one `certificate.issued` audit row.
- Given the issued row, when the retained file is read through the private store, then it starts with `%PDF-`.
- Given an already issued review, when issuance runs again, then it is refused as `Duplicate`, the stored path and bytes are unchanged, and no second audit row exists.
- Given a pending or not-eligible review, when issuance runs, then it is refused as `Invalid` with no file and no audit.
- Given invalidated attendance, when issuance runs, then it is refused as `Stale` with no file and no audit.
- Given a renderer failure, when issuance runs, then no file remains under `certificates/` and the row is unchanged.
- Given a member caller, when issuance runs, then it is refused as `Forbidden`.
- Given `DemoData:Enabled=true`, when the ledger opens after issuing, then the row appears in the Issued Registry and its PDF downloads unchanged.

## Implementation Notes

- `CertificateLedgerService` gained `ICertificatePdfRenderer` and `IPrivateFileStore` as the last two
  primary-constructor parameters. `IssueAsync` discovers and pre-checks untracked, renders bytes into
  the private temporary area, promotes them to `certificates/`, then runs one caller-owned
  transaction that locks the owner `users` row and the event before the tracked re-read. The
  `DecideInsideTransactionAsync` idiom is reused verbatim: mutate, save, append the audit, save.
- Pre-check refusals: `Issued` → `Duplicate`, any other non-`Eligible` status or a non-confirmed
  registration or a non-completed event → `Invalid`, invalidated attendance → `Stale`. The
  in-transaction recheck collapses every change that landed during rendering into one `Stale`.
- Rendering and promotion are outside the transaction, so no lock is held during Chromium or disk
  I/O. A renderer exception, an empty result or a non-PDF promotion becomes `Invalid` and deletes the
  temporary file; a commit failure deletes only the promoted key this attempt created.
- The audit payload carries `EventId`, `RegistrationId`, `CertificateNumber`, `IssuedBy` and
  `IssuedAt`; the writer's sensitive-name filter rejects any property containing `path`, so the
  relative key is never logged.
- `DemoCertificateLedgerService` now only delegates the three commands and serves the officer viewer
  and PDF from the real issued row. `DemoMyCertificatesService` (M-12) was handed over in the same
  change so a newly issued certificate still appears in the member history and downloads from its
  retained file; only the seeded row without `file_path` uses `DemoCertificateDocuments`.
- `DemoStore` keeps `IsCertificateIssued`, `TryGetIssuedCertificate`, `SeedIssuedCertificate` and
  `AttachCertificateDocument`; only the demo issuance methods were deleted.
- Review pass 1 patches: stale demo-overlay docs and M-11a labels corrected; the ledger flow test
  renamed to its persisted behaviour; a queue row without a certificate id now refuses as `NotFound`;
  a retained file missing on disk now refuses as `NotFound` instead of an unhandled 500.

## Plan Change Log

- Added the `DemoMyCertificatesService` hand-over to the execution list. The plan's frozen "keep the
  demo screen populated and unchanged" and register rule 12 require it: after 6.7 real issuance no
  longer writes a `DemoStore` issue, so the M-12 member history and PDF would otherwise show a newly
  issued certificate as unavailable.

## Review Triage Log

**Pass 1 (quick)** — high 0, medium 1, low 6, false 0, maybe-false 0.

| Finding | Verdict | Route | Evidence / action |
|---|---|---|---|
| `demo-mock-register.md:96` — M-11a reads `removed (pending commit)` instead of a commit | low | patch | Verified: the row must carry the removing commit for 8.11's check. Set to `removed (ee822e7)` after the implementation commit. |
| `CertificateLedgerService.cs:63-67,105` — XML docs still describe the demo overlay | low | patch | Verified: `DemoCertificateLedgerService.GetPageAsync` now delegates straight to `inner`, so the overlay text is stale. Reworded both summaries to the real paged read. |
| `DemoCertificatesRegistration.cs:7-8` — header comment still says the provider overlays the issued state and lists M-11a | low | patch | Verified: the overlay was deleted. Reworded to the remaining M-11b viewer/PDF role. |
| `DemoSeedService.cs:21,112,267` — seeded document still labelled M-11a | low | patch | Verified: after 6.7 that document is the remaining M-11b state. Relabelled the summary, log line and seed comment. |
| `CertificateLedgerFlowTests.cs:18` — test name says "WithoutPersistingCertificateRows" while the body asserts persistence | low | patch | Verified: the assertions now require an issued row, a retained file and an audit row. Renamed to `OfficerCanReviewIssueAndDownloadTheRetainedCertificateWithAnAuditRow`. |
| `CertificateLedgerService.cs:191-205` — a confirmed registration with no certificate row is refused as `Invalid` | low | patch | Verified: `GetReviewQueueAsync` left-joins the certificate, so `FindReviewAsync` returns a row with a null `CertificateId` and the pre-check falls through to "Approve this review". `IssueAsync` now treats a row without a certificate id as `NotFound`, matching the decision path. |
| `DemoCertificateLedgerService.cs:74-80`, `DemoMyCertificatesService.cs:89-95` — a missing retained file throws `FileNotFoundException` and becomes an unhandled 500 | medium | patch | Verified: `PrivateFileStore.OpenReadAsync` throws for a missing file and the controller catches only `DomainException`; CERT-02 exercises a missing file. Both providers now map the file-open failure to `DomainException(NotFound)`. |

## Design Notes

- The number `AT-{issuedAt:yyyy}-{registrationId:D6}` is unique because `registration_id` is unique; the filtered unique index is the backstop and `SaveChangesAsync` already maps SQL 2601/2627 to `Duplicate`.
- Rendering and promotion happen before the transaction so no lock is held during Chromium or disk I/O (AD-4). The transaction rechecks every fact, so a decision or invalidation that lands mid-render is refused as `Stale` and the new file is deleted.
- Retry never replaces an issued PDF: `PromoteAsync` writes a new GUID name with `overwrite: false`; a successful row keeps its own path, and a failed commit deletes only the file it created.
- The demo provider reads the real `file_path` for issued rows. Only the seeded issued row, which has no retained file, still falls back to the in-memory document until 6.8 removes M-11b.
- The audit payload carries event, registration, number, issuer and time only — never the path, because the writer rejects property names containing `path`.

## Verification

**Commands:**
- `dotnet build` -- expected: clean.
- `$env:ATTENDTA_TEST_SQL = "Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --filter "FullyQualifiedName~CertificateIssuanceTests"` -- expected: all pass.
- `$env:ATTENDTA_TEST_SQL = "Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --filter "FullyQualifiedName~CertificateLedger|FullyQualifiedName~CertificateReviewDecision"` -- expected: all pass.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` -- expected: green.

**Manual checks:**
- With `DemoData__Enabled=true`, complete the seeded event, approve one review, issue it, open the Issued Registry, and view and download the PDF; confirm one file under `<SetupRoot>/certificates` and one `certificate.issued` row in the Activity Log.
