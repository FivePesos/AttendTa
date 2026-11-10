# AttendTa acceptance coverage matrix

Ticket 8.5 / issue #78. Authored 2026-11-10 against `dev` after 8.10 (#83, PR #219).

This is a **coverage map, not execution evidence**. Every case below starts as **not executed**.
8.4 (#77) runs the combined automated suite on one integrated commit; 8.6 (#79) executes and
records the manual remainder; 8.7 (#80) demonstrates the 26 functional IDs; 8.8 (#81) signs off.

Rules applied:

- **Automated** means a test that exists in `tests/ASI.Basecode.IntegrationTests` or
  `tests/ASI.Basecode.Services.Tests` today, including the cases added by 8.5.
- **Manual** means a concrete 8.6 case with the steps written out. A manual case is never a
  substitute for a test that could reasonably exist; it is the remainder after the automated
  inventory.
- **Residual** means explicitly not automated, with the owning case named. No SPEC §20 scenario
  is silently dropped.
- No new test project and no prototype-only suite was introduced. The two existing projects and
  the existing SQL harness (`ATTENDTA_TEST_SQL`, one `AttendTa_IT_<guid>` database per class) are
  reused.

## 1. Finale automated/integration cases

| Case | Automated coverage | Added by 8.5 | Manual remainder |
|---|---|---|---|
| AUTH-01 | `AccountRequestSubmissionTests`, `AccountRequestDecisionTests`, `AccountServiceTests`, `AuthenticationFlowTests`, `AdminSeedTests`, `PasswordHasherCliTests`, `Argon2PasswordHasherTests` — hash-only storage, one approval outcome under parallel decisions, generic sign-in failure, no privileged public creation, non-persistent HttpOnly cookie | — | M-AUTH-01: observe the 8-hour non-sliding expiry on a real browser session (cookie ticket length is not asserted by a test) |
| AUTH-02 | `AuthenticationFlowTests`, `AccountRequestDecisionTests`, `AccountRequestQueueTests`, `AdminAccountsListTests`, `AdminMembershipReactivationTests` — anonymous/wrong-role refusal, another member's data, antiforgery variants, stale session after deactivate/demote | — | — |
| MD-01 | `VenuePhotoFlowTests`, `VenueTransactionPhotoTests`, `VenueImagePathMigrationTests`, `PrivateFileStoreTests`, `MasterDataTransactionTests`, `MemberManagementTests`, `ActiveReferenceQueryTests`, `EventCoverFlowTests` — 2 MiB JPEG by content, failed replacement keeps the old file, uniqueness/reason/history, active-reference guards | — | M-MD-01: upload a real PNG through the venue dialog (only JPEG is accepted end-to-end today) |
| REG-01 | `MemberEventFlowTests` (all six classes), `MemberSelfCancellationTests`, `RegistrationCancellationTests`, `RegistrationPromotionTests`, `RegistrationReviewTests`, `RegistrationStatusDisplayTests`, `MemberEventServiceTests` — concurrent final-seat approvals, concurrent promotions, exact cutoffs, cancelled token invalid, no automatic promotion, no re-registration | `RegistrationsForSeparateOverlappingEventsAreBothAccepted` | — |
| ATT-01 | `CheckInFlowTests`, `CheckInLookupFlowTests` — one attendance row and one audit entry per valid scan, repeat returns the original time, identity only after a valid check-in | `ParallelDuplicateScansWriteOneAttendanceRowAndOneAuditEntry` | — |
| ATT-02 | `CheckInFlowTests`, `CheckInLookupFlowTests`, `CheckInWindowFlagTests`, `AttendanceInvalidationFlowTests`, `PassDownloadTests`, `MyEventsAttendanceQueryTests`, `EventCheckInWindowFlowTests` — unknown/wrong-event/cancelled/unconfirmed/inactive/invalidated tokens, closed and unpublished windows, refusals insert nothing, lookup alone records nothing | `WaitlistedRegistrationIsInvalidAndWritesNothing` | M-ATT-02: scan a token while an invalidation is in flight on another session (the sequential guard is automated; the interleaving is not) |
| EVT-01 | `EventCompletionFlowTests`, `EventCompletionReviewTests`, `EventCancellationFlowTests`, `EventLifecycleServiceTests` — completion closes check-in, atomic review creation, hook failure rolls back event/reviews/audit | `CompleteExactlyAtStartIsAllowed`, `CompleteJustBeforeStartIsRefusedWithoutChanges` | — |
| ADM-01 | `AdminAccountDeactivationTests`, `AdminAccountReactivationTests`, `AdminMembershipDeactivationTests`, `AdminMembershipReactivationTests`, `AdminRoleAssignmentTests`, `DeactivationPreviewTests`, `AccessDialogServiceTests`, `AccountRoleServiceTests` — parallel deactivations and parallel demotions each leave one active admin, preview/reason, cascade, session ends, reactivation restores no seats | — | M-ADM-01: deactivate an account that has attended registrations and confirm attendance/certificate rows survive; race a demotion against a deactivation of the last two admins |
| CERT-01 | `CertificateIssuanceTests`, `CertificateReviewDecisionTests`, `CertificateLedgerFlowTests`, `AttendanceInvalidationFlowTests`, `ParticipationFactsTests` — required reason, final decline, sequential duplicate refused, invalidation blocked after issue | `ConcurrentIssuesWriteOneCertificateOneFileAndOneAuditRow` | — |
| CERT-02 | `CertificateIssuanceTests`, `CertificateLedgerFlowTests`, `CertificatePdfRendererTests`, `PrivateFileStoreTests`, `MemberManagementTests`, `StaticAssetTests` — renderer/audit/commit failure leaves no false issued row, retained bytes equal served bytes, member-edit snapshot immutability, authorized id-only download, missing file 404, traversal refused | — | M-CERT-02a: force a private-file-store failure during issuance; M-CERT-02b: edit the event after issuance and re-check the retained PDF and snapshots |
| REP-01 | `ParticipationReportRepositoryTests`, `ParticipationReportServiceTests`, `ParticipationFactsTests`, `PagingTests` — full-dataset totals, zero denominator, corrected attendance, inactive history, snapshot distributions | `MixedStatusTotalsAreIndependentOfTheRequestedPage` | — |
| AUD-01 | `TransactionAuditTests`, `TransactionVerificationTests`, `ActivityLogViewerTests`, `CanonicalSchemaTests`, `AdminSeedTests` — business/audit atomicity, unsafe field-name rejection, append-only trigger, administrator-only newest-first viewer | — | M-AUD-01: verify column-level `GRANT`/`DENY` on `activity_logs` under the runtime SQL identity (host provisioning, shared with OPS-01) |
| MOCK-01 | Owned and closed by 8.11 (#152) and 8.13 (#151); `demo-mock-register.md` records every row as `removed` | — | — |

## 2. SPEC §20 acceptance bullets

| SPEC §20 bullet | Automated | Manual |
|---|---|---|
| Account duplicates, rejection/resubmission, atomic approval, inactive/current-role access | `AccountRequestSubmissionTests`, `AccountRequestDecisionTests`, `AuthenticationFlowTests` | — |
| Mandatory venue photo and publication cover, invalid/oversized files, replacement/SQL failures, safe private download, no profile-picture workflow | `VenuePhotoFlowTests`, `VenueTransactionPhotoTests`, `EventCoverFlowTests`, `PrivateFileStoreTests` | M-MD-01 (PNG) |
| Deadline/start boundaries, stale forms, publication-open signup, final cancellation/no reapplication, retained cutoff history | `MemberEventDeadlineFlowTests`, `EventPublicationFlowTests`, `MemberSelfCancellationTests`, `RegistrationReviewTests` | — |
| Concurrent final-seat approvals/promotions, full-queue fairness, no automatic promotion | `RegistrationReviewTests`, `RegistrationPromotionTests`, `RegistrationCancellationTests`, `AdminMembershipDeactivationTests` | — |
| QR wrong-event/unknown/ineligible/duplicate scans, valid-scan identity, manual fallback, early/reopened check-in, correction restrictions, no re-check-in | `CheckInFlowTests` (incl. the parallel duplicate), `CheckInLookupFlowTests`, `AttendanceInvalidationFlowTests`, `EventCheckInWindowFlowTests`, `PassDownloadTests` | ATT-03 (camera on the real HTTPS origin) |
| Successful registration for separate events with overlapping times | `MemberEventFlowTests.RegistrationsForSeparateOverlappingEventsAreBothAccepted` | — |
| Completion, eligibility, concurrent issuance/correction, immutable PDF downloads, unauthorized access, missing-file handling | `EventCompletionFlowTests`, `CertificateIssuanceTests` (incl. the concurrent issue), `CertificateLedgerFlowTests`, `CertificatePdfRendererTests` | M-CERT-02a, M-CERT-02b |
| Full-dataset reports independent of paging, zero denominator, corrected attendance, inactive history, restricted counts | `ParticipationReportRepositoryTests` (incl. the 30-row mixed-status case), `ParticipationReportServiceTests`, `ParticipationFactsTests` | — |
| Initial administrator setup, persistent storage across redeployment, backups/restores, build/review/integration testing, demonstration | `AdminSeedTests`, `PasswordHasherCliTests`, `CanonicalSchemaTests` | OPS-02 (backup/restore), 8.4 (combined run), 8.7 (demo) |

## 3. Functional ID groups

| IDs | Automated | Manual |
|---|---|---|
| AUTH-01–03 | `AccountRequestSubmissionTests`, `AccountRequestDecisionTests`, `AccountRequestQueueTests`, `AuthenticationFlowTests` | — |
| MD-01–05 | `MemberManagementTests`, `MemberPagedListTests`, `EventTypeManagementTests`, `EventTypeManagementFlowTests`, `VenuePagedListTests`, `VenuePhotoFlowTests`, `ActiveReferenceQueryTests` | M-MD-01 |
| EVT-01–04 | `EventDraftFlowTests`, `EventPublicationFlowTests`, `PublishedEventEditFlowTests`, `EventCancellationFlowTests`, `EventCompletionFlowTests`, `EventCompletionReviewTests`, `MemberEventBrowsingFlowTests` | — |
| REG-01–05 | `MemberEventRegistrationFlowTests`, `MemberEventDeadlineFlowTests`, `RegistrationReviewTests`, `RegistrationPromotionTests`, `RegistrationCancellationTests`, `MemberSelfCancellationTests` | — |
| QR-01 | `PassDownloadTests`, `CheckInFlowTests` | ATT-03 |
| ATT-01–03 | `CheckInFlowTests`, `CheckInLookupFlowTests`, `AttendanceInvalidationFlowTests`, `MyEventsAttendanceQueryTests`, `EventCheckInWindowFlowTests` | ATT-03, M-ATT-02 |
| CERT-01–02 | `CertificateReviewDecisionTests`, `CertificateIssuanceTests`, `CertificateLedgerFlowTests`, `CertificatePdfRendererTests` | CERT-03, M-CERT-02a, M-CERT-02b |
| REP-01 | `ParticipationReportRepositoryTests`, `ParticipationReportServiceTests` | — |
| ADM-01–02 | `AdminAccountsListTests`, `AdminRoleAssignmentTests`, `AdminAccountDeactivationTests`, `AdminAccountReactivationTests`, `AdminMembershipDeactivationTests`, `AdminMembershipReactivationTests`, `ActivityLogViewerTests` | M-ADM-01, M-AUD-01 |
| FR-35–36 | `AdminSeedTests`, `PasswordHasherCliTests`, `PagingTests`, `MemberPagedListTests`, `VenuePagedListTests`, `ParticipationReportServiceTests` | — |

## 4. Manual and device cases

These stay manual by design; they need a real browser, a real phone on the trusted HTTPS
origin, or the demo host.

| Case | Owner | Steps summary |
|---|---|---|
| UI-01 | 8.6 | Public request/login, member events/details/passes/history/profile, officer and admin navigation against real data; no dead links or prototype-only role switching |
| UI-02 | 8.6 | Keyboard-only forms, status tabs, centered editor and reason dialogs; one validation error, cancel and reopen; 200% zoom at 1440/1024/768/390px |
| ATT-03 | 8.6 | Real hotspot HTTPS URL in phone Chrome: grant/deny camera, scan a valid pass and a duplicate, then search/select/check in manually |
| CERT-03 | 8.6 | Issue and view/download a real retained landscape A4 PDF; check fonts, branding, signatory, recipient/event/date/number |
| OPS-01 | 8.6 | Demo-host readiness script under the runtime identity, including server-level privilege and audit column-write checks |
| OPS-02 | 8.6 | Back up SQL plus SetupRoot together, restart/redeploy, restore to a separate disposable target and compare rows/checksums/keys |

## 5. Cases added by 8.5

| Test | Class | Proves | Case |
|---|---|---|---|
| `ParallelDuplicateScansWriteOneAttendanceRowAndOneAuditEntry` | `CheckInFlowTests` | Two scans of the same token on two SQL connections, gated before the `users` lock, produce exactly one attendance row and one audit entry; the second answers `Repeat` with the first scan's timestamp | ATT-01, SM-4 |
| `WaitlistedRegistrationIsInvalidAndWritesNothing` | `CheckInFlowTests` | A waitlisted registration's token is refused through both the scanner and the engine, with no attendance row and no audit entry | ATT-02 |
| `ConcurrentIssuesWriteOneCertificateOneFileAndOneAuditRow` | `CertificateIssuanceTests` | Two issuances gated inside the renderer produce exactly one issued certificate, one retained file, no temporary file and one audit row; the loser is refused as `Duplicate` | CERT-01, SM-4 |
| `CompleteExactlyAtStartIsAllowed` | `EventCompletionServiceTests` | Completion on the start tick is accepted and closes check-in | EVT-01 |
| `CompleteJustBeforeStartIsRefusedWithoutChanges` | `EventCompletionServiceTests` | Completion one millisecond before start (the `datetime2(3)` limit) is refused with no state change and no audit row | EVT-01 |
| `MixedStatusTotalsAreIndependentOfTheRequestedPage` | `ParticipationReportRepositoryTests` | 30 registrations across all statuses: totals, no-shows and rate are identical on page 1, page 2 and the default page | REP-01 |
| `RegistrationsForSeparateOverlappingEventsAreBothAccepted` | `MemberEventRegistrationFlowTests` | The same member can register for two published events scheduled at the same time | SPEC §20, REG-01 |

## 6. Residual gaps and their owners

Nothing below is dropped; each is assigned to a concrete case.

| Residual | Owner | Why not automated |
|---|---|---|
| M-AUTH-01 — 8-hour, non-sliding cookie expiry | 8.6 | Needs a real browser session over time; the cookie flags themselves are automated |
| M-MD-01 — PNG upload through the venue dialog | 8.6 | The store accepts only JPEG end-to-end today; the content-sniffing rules are automated |
| M-ATT-02 — scan racing an in-flight invalidation | 8.6 | Two live operator sessions; the sequential guard and the certificate transitions are automated |
| M-ADM-01 — attended data survives account deactivation; demote-vs-deactivate race | 8.6 | The two same-kind races are automated; the mixed pair and attended-data preservation are not |
| M-CERT-02a — private-file-store failure during issuance | 8.6 | Renderer, audit and commit failures are automated; the store layer is not |
| M-CERT-02b — event edited after issuance | 8.6 | Member-edit snapshot immutability is automated; the event-edit equivalent is not |
| M-AUD-01 — `activity_logs` column permissions under the runtime identity | 8.6 / OPS-01 | Host provisioning, not application code; the append-only trigger is automated |
| ATT-03, CERT-03, OPS-01, OPS-02, UI-01, UI-02 | 8.6 | Device, host and human-judgement evidence |

## 7. SM-4 concurrency proof

SM-4 requires scripted SQL Server concurrency tests showing zero overbooked events, zero duplicate
attendance records and zero duplicate certificates.

| SM-4 clause | Test | Connections |
|---|---|---|
| Zero overbooked events | `RegistrationReviewTests.ConcurrentApprovalsSerializeTheLastSeatAcrossSqlConnections` | Blocker transaction plus two service contexts |
| Zero overbooked events (promotion) | `RegistrationPromotionTests` concurrent promotion + approval, concurrent promotions | Two service contexts plus a held event lock |
| Zero duplicate attendance records | `CheckInFlowTests.ParallelDuplicateScansWriteOneAttendanceRowAndOneAuditEntry` | Two gated service contexts |
| Zero duplicate certificates | `CertificateIssuanceTests.ConcurrentIssuesWriteOneCertificateOneFileAndOneAuditRow` | Two gated service contexts |
| Zero duplicate registrations | `MemberEventRegistrationPersistenceTests` parallel duplicate registration | Two service contexts |

## 8. Out of scope for 8.5

- Combined full-suite execution and its manifest: 8.4 (#77).
- Manual/device/host evidence: 8.6–8.8.
- Presentation, Razor, CSS and JS: 8.12 (#153) already closed; this ticket is test-only.
