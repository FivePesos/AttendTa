# AttendTa recorded manual SPEC §20 acceptance checklist

Ticket 8.6 / #79. Authored and executed 2026-11-11 against the disposable LocalDB database
`AttendTaMVP_Test` and a local build of the tested commit below. This is the **execution record**
for the manual remainder that `finale-test-cases.md` and `acceptance-coverage-matrix.md` left
open. It is not a replacement for either: the matrix stays the coverage map, the 8.4 manifest
stays the automated run, and this file records what a human or an agent actually observed.

The evaluator decision at the end is deliberately blank. The record is complete when every row
below carries a result; the ticket is done only when the human evaluator signs it.

## 1. Record header

| Field | Value |
|---|---|
| Ticket | 8.6 / #79 — Recorded manual SPEC §20 acceptance checklist verification |
| Tested commit | `d8046bf6fe41643f06d2cac64560e115161cb821` (`dev` tip, PR #223 merge) |
| Build | `dotnet build ASI.Basecode.sln --configuration Release` — 0 warnings, 0 errors |
| Date | 2026-11-11 |
| Operator | AI agent (subagent), executing under the ticket plan; human evaluator sign-off pending |
| OS | Windows 11 Pro, build 10.0.26100 |
| Runtime | .NET SDK 10.0.401 |
| Database | SQL Server LocalDB `MSSQLLocalDB`, disposable `AttendTaMVP_Test`, reset from `database/reset-local-mvp-test-db.sql` and seeded with `attendta-mvp-schema.sql`, `seed-local-admin.sql`, `seed-event-types.sql`, `seed-demo-scenario.sql` |
| App process | `dotnet run --project ASI.Basecode.WebApp --no-build --configuration Release`, `ASPNETCORE_ENVIRONMENT=Production`, `http://localhost:5080` (second instance `http://localhost:5091` for M-CERT-02a) |
| Private store | throw-away `Common__SetupRoot` under the session temp directory, outside `wwwroot` and the deployment tree, with `keys`, `logs`, `temporary`, `uploads/venues`, `uploads/events`, `certificates` |
| Browser | Playwright 1.57.0, Chromium 143 headless (build v1200); real browser contexts, real cookie jar, real form posts |
| PDF tooling | Poppler `pdftotext` 4.00 (Git for Windows build) plus raw PDF structure inspection |
| Accounts | The three seeded accounts from `demo-script.md` §2.5, with locally generated Argon2id hashes. No password, hash, cookie or token is reproduced in this file. |
| Automated evidence reused | `execution-manifest-fa9ae82.md` (8.4: 321 service, 479 integration, commit `fa9ae82`); `demo-mock-register.md` (all 21 rows `removed`, last `149791b`) |
| Walkthrough evidence reused | `story-final-frontend-walkthrough-targeted-refactor-and-impeccable.md` §"Walkthrough evidence — Session G" and §"Keyboard/dialog evidence" (8.12) |

The 8.4 manifest was produced on `fa9ae82`; the tested commit here is `d8046bf`, which differs
only by documentation and evidence commits (`970781b`, `ec15038`, PR #223 merge). The manifest's
own rule is that documentation and evidence-only commits do not invalidate it, so it is cited
unchanged and no suite was re-run for this ticket. The manifest records .NET SDK 10.0.403; this
run used 10.0.401 on the same OS build, so the cited automated results come from a slightly
different SDK patch level while the source under test is the same.

## 2. Result legend

| Result | Meaning |
|---|---|
| **PASS** | Executed on the tested commit and environment above; the evidence field names the observation. |
| **FAIL** | Executed; the observed result contradicts the expected outcome. The row names the defect. |
| **BLOCKED** | Not executable here. The row names the exact missing input and the evidence file that owns it. A closed issue, a merged tool or a planning document is never substituted. |
| **REUSED** | Covered by the cited automated suite or walkthrough; not re-run for this ticket. |
| **PREPARED** | Authored with steps and expected outcome, not yet run. No row below is left PREPARED at the end of this record. |

Race rows are **demonstrations, not proofs**: a human (or an agent) driving two sessions cannot
produce the sub-second interleaving that proves a concurrency property. The automated same-kind
races cited in the matrix §5 and §7 own the proof.

## 3. SPEC §20 acceptance bullets

SPEC §20 (`docs/AttendTa-System-Specification.md` lines 377–393) has nine acceptance bullets plus
the pending-signatory statement. Each bullet appears once, with its automated citation and the
manual rows that carry the remainder.

| # | SPEC §20 bullet | Automated citation (8.4 manifest) | Manual rows | Result |
|---|---|---|---|---|
| B1 | Account duplicates, rejection/resubmission, atomic approval and inactive/current-role access checks | `AccountRequestSubmissionTests`, `AccountRequestDecisionTests`, `AuthenticationFlowTests`, `AdminSeedTests` — inside the 479 | M-AUTH-01, M-ADM-01 | PASS (automated REUSED; manual rows below) |
| B2 | Mandatory venue photo and publication cover, invalid/oversized files, replacement/SQL failures and safe private download; no user profile-picture workflow | `VenuePhotoFlowTests`, `VenueTransactionPhotoTests`, `EventCoverFlowTests`, `PrivateFileStoreTests` — inside the 479 | M-MD-01 | PASS (automated REUSED; M-MD-01 PASS — matrix PNG premise stale, no defect) |
| B3 | Deadline/start boundaries including stale forms, publication-open signup, final cancellation/no reapplication and retained unresolved cutoff history | `MemberEventDeadlineFlowTests`, `EventPublicationFlowTests`, `MemberSelfCancellationTests`, `RegistrationReviewTests` — inside the 479 | — (UI-01 exercises the signup path) | PASS (automated REUSED) |
| B4 | Concurrent final-seat approvals/promotions, full-queue fairness and no automatic promotion on cancellation/deactivation | `RegistrationReviewTests`, `RegistrationPromotionTests`, `RegistrationCancellationTests`, `AdminMembershipDeactivationTests` — inside the 479 | M-ADM-01 | PASS (automated REUSED; M-ADM-01 PASS) |
| B5 | QR wrong-event/unknown/ineligible/duplicate scans, valid-scan identity, manual fallback, early/reopened check-in, correction restrictions and no re-check-in | `CheckInFlowTests` (incl. `ParallelDuplicateScansWriteOneAttendanceRowAndOneAuditEntry`), `CheckInLookupFlowTests`, `AttendanceInvalidationFlowTests`, `EventCheckInWindowFlowTests`, `PassDownloadTests` — inside the 479 | ATT-03, M-ATT-02 | BLOCKED (ATT-03 camera on the real HTTPS origin); M-ATT-02 PASS as demonstration |
| B6 | Successful registration for separate events whose scheduled times overlap; overlapping registrations are not blocked | `MemberEventRegistrationFlowTests.RegistrationsForSeparateOverlappingEventsAreBothAccepted` — inside the 479 | — | PASS (automated REUSED) |
| B7 | Completion, eligibility, concurrent issuance/correction, immutable PDF downloads, unauthorized access and missing-file handling | `EventCompletionFlowTests`, `CertificateIssuanceTests` (incl. `ConcurrentIssuesWriteOneCertificateOneFileAndOneAuditRow`), `CertificateLedgerFlowTests`, `CertificatePdfRendererTests` — inside the 479 | CERT-03, M-CERT-02a, M-CERT-02b | PASS (automated REUSED; CERT-03 PASS with the signatory sub-field BLOCKED, §5) |
| B8 | Full-dataset reports independent of paging, zero denominator, corrected attendance, inactive historical records and restricted counts | `ParticipationReportRepositoryTests` (incl. `MixedStatusTotalsAreIndependentOfTheRequestedPage`), `ParticipationReportServiceTests`, `ParticipationFactsTests` — inside the 479 | — | PASS (automated REUSED) |
| B9 | Initial administrator setup, persistent storage across redeployment, backups/restores, build/review/integration testing and demonstration | `AdminSeedTests`, `PasswordHasherCliTests`, `CanonicalSchemaTests`; the 8.4 manifest; 8.7 demo | OPS-01, OPS-02 | BLOCKED (OPS-01, OPS-02 — no real host, no restore target); build and integration testing PASS via the 8.4 manifest |
| B10 | Authorized signatory name/title/signature remain pending and must be supplied before official certificate issuance | `CertificateSignatoryOptions` validation (approved constants) — `CertificatePdfRendererTests` | CERT-03 signatory sub-field | BLOCKED — the human has not confirmed the signatory; the configured values are recorded in §5 |

## 4. Automated coverage index (matrix §1 and §5)

Every matrix §1 case maps to its test classes and to the 8.4 manifest line. The manifest records
one combined run — **321 service tests, 479 integration tests, 0 failed, 0 skipped** — so the
"manifest line" for each case is that run; the case-to-test mapping is matrix §5.

| Case | Test classes (matrix §1) | Manifest line | Result |
|---|---|---|---|
| AUTH-01 | `AccountRequestSubmissionTests`, `AccountRequestDecisionTests`, `AccountServiceTests`, `AuthenticationFlowTests`, `AdminSeedTests`, `PasswordHasherCliTests`, `Argon2PasswordHasherTests` | 479 integration + 321 service, all passed | REUSED |
| AUTH-02 | `AuthenticationFlowTests`, `AccountRequestDecisionTests`, `AccountRequestQueueTests`, `AdminAccountsListTests`, `AdminMembershipReactivationTests` | same run | REUSED |
| MD-01 | `VenuePhotoFlowTests`, `VenueTransactionPhotoTests`, `VenueImagePathMigrationTests`, `PrivateFileStoreTests`, `MasterDataTransactionTests`, `MemberManagementTests`, `ActiveReferenceQueryTests`, `EventCoverFlowTests` | same run | REUSED |
| REG-01 | `MemberEventFlowTests` (all six classes), `MemberSelfCancellationTests`, `RegistrationCancellationTests`, `RegistrationPromotionTests`, `RegistrationReviewTests`, `RegistrationStatusDisplayTests`, `MemberEventServiceTests` | same run | REUSED |
| ATT-01 | `CheckInFlowTests`, `CheckInLookupFlowTests` incl. `ParallelDuplicateScansWriteOneAttendanceRowAndOneAuditEntry` | same run | REUSED |
| ATT-02 | `CheckInFlowTests`, `CheckInLookupFlowTests`, `CheckInWindowFlagTests`, `AttendanceInvalidationFlowTests`, `PassDownloadTests`, `MyEventsAttendanceQueryTests`, `EventCheckInWindowFlowTests` incl. `WaitlistedRegistrationIsInvalidAndWritesNothing` | same run | REUSED |
| EVT-01 | `EventCompletionFlowTests`, `EventCompletionReviewTests`, `EventCancellationFlowTests`, `EventLifecycleServiceTests` incl. `CompleteExactlyAtStartIsAllowed`, `CompleteJustBeforeStartIsRefusedWithoutChanges` | same run | REUSED |
| ADM-01 | `AdminAccountDeactivationTests`, `AdminAccountReactivationTests`, `AdminMembershipDeactivationTests`, `AdminMembershipReactivationTests`, `AdminRoleAssignmentTests`, `DeactivationPreviewTests`, `AccessDialogServiceTests`, `AccountRoleServiceTests` | same run | REUSED |
| CERT-01 | `CertificateIssuanceTests`, `CertificateReviewDecisionTests`, `CertificateLedgerFlowTests`, `AttendanceInvalidationFlowTests`, `ParticipationFactsTests` incl. `ConcurrentIssuesWriteOneCertificateOneFileAndOneAuditRow` | same run | REUSED |
| CERT-02 | `CertificateIssuanceTests`, `CertificateLedgerFlowTests`, `CertificatePdfRendererTests`, `PrivateFileStoreTests`, `MemberManagementTests`, `StaticAssetTests` | same run | REUSED |
| REP-01 | `ParticipationReportRepositoryTests`, `ParticipationReportServiceTests`, `ParticipationFactsTests`, `PagingTests` incl. `MixedStatusTotalsAreIndependentOfTheRequestedPage` | same run | REUSED |
| AUD-01 | `TransactionAuditTests`, `TransactionVerificationTests`, `ActivityLogViewerTests`, `CanonicalSchemaTests`, `AdminSeedTests` | same run | REUSED |
| MOCK-01 | Owned and closed by 8.11 (#152, `89b362b`) and 8.13 (#151, `149791b`) | `demo-mock-register.md`: all 21 rows `removed`, last `149791b` | REUSED |

Matrix §5 cases added by 8.5 — `ParallelDuplicateScansWriteOneAttendanceRowAndOneAuditEntry`,
`WaitlistedRegistrationIsInvalidAndWritesNothing`,
`ConcurrentIssuesWriteOneCertificateOneFileAndOneAuditRow`, `CompleteExactlyAtStartIsAllowed`,
`CompleteJustBeforeStartIsRefusedWithoutChanges`,
`MixedStatusTotalsAreIndependentOfTheRequestedPage`,
`RegistrationsForSeparateOverlappingEventsAreBothAccepted` — are all inside the same 479-test run
and are cited above, not re-run.

## 5. Manual and device cases

Each row carries id, requirement, preconditions, numbered steps, expected outcome, evidence and
result. Rows are ordered by case id.

### ATT-03 — camera scan on the real hotspot HTTPS origin

| Field | Value |
|---|---|
| Requirement | SPEC §20 bullet 5; `finale-test-cases.md` ATT-03; `docs/hotspot-https.md` |
| Preconditions | Real demo host with the hotspot HTTPS binding, root CA installed on a real officer phone, a valid printed pass, the app reachable from the hotspot subnet |
| Steps | 1. Open `https://<hotspot-ip>:5443/Account/Login` in phone Chrome. 2. Sign in as the officer. 3. Grant camera permission and scan a valid pass. 4. Scan the same pass again. 5. Deny camera permission and use manual lookup. 6. Search, select and check in manually. 7. Disconnect the host from the internet and repeat 3 and 6. |
| Expected outcome | No certificate warning; camera works; a duplicate scan reports the original check-in time; camera denial leaves manual lookup usable; results persist after the redirect; lookup alone never inserts; no CDN or internet dependency |
| Evidence | **BLOCKED.** Missing input: a real demo host with the hotspot HTTPS binding, its generated root CA, and a real officer phone. `docs/hotspot-https-evidence.md` rows 1–12 are all `BLOCKED` for the same reason (no host name, no hotspot adapter address, no certificate thumbprint, no phone). A closed #75 is not host evidence. |
| Result | **BLOCKED** |

### CERT-03 — retained landscape A4 PDF, fonts, branding, signatory

| Field | Value |
|---|---|
| Requirement | SPEC §20 bullet 7 and the pending-signatory statement; `finale-test-cases.md` CERT-03 |
| Preconditions | A completed event with a checked-in, approved attendee; local Chromium for the Playwright renderer; Poppler for text extraction |
| Steps | 1. Sign in as the officer, open `/Certificates?eventId=1`. 2. Approve the pending review through the dialog. 3. Issue the certificate through the dialog. 4. Open the Issued Registry and View PDF. 5. Download the PDF. 6. Extract the text with `pdftotext -layout` and inspect the PDF structure. 7. Confirm no sample or development artefact is presented as official output. |
| Expected outcome | One retained landscape A4 PDF; embedded local fonts; University of Cebu branding; configured signatory; recipient, event, event start, issue time and certificate number; stable retained download; no navigation or link annotations inside the PDF |
| Evidence | Executed on `d8046bf`, `http://localhost:5080`, Production, seeded `AttendTaMVP_Test`, headless Chromium 143, Poppler 4.00. Viewer `/Certificates/Viewer/1` returned 200 and framed `/Certificates/Pdf/1`; the PDF response was `200 application/pdf`, 447,096 bytes, `Cache-Control: no-store,no-cache`, inline; the download was byte-identical with `Content-Disposition: attachment; filename=AT-2026-000001.pdf`. `pdftotext -layout` extracted: `UNIVERSITY OF CEBU`, `STUDENT ORGANIZATION COUNCIL — ATTENDTA`, `CERTIFICATE OF PARTICIPATION`, `This certificate is presented to`, `Liza Fernandez`, `in recognition of participation in`, `Leadership Summit 2026`, `EVENT START — MANILA TIME 2026-11-08 15:43 Asia/Manila`, `DATE ISSUED — MANILA TIME 2026-11-11 15:47 Asia/Manila`, `Christian Kyle Ducay`, `Organization President`, `Certificate number AT-2026-000001`. Structure: one page, `MediaBox 0 0 841.92 594.96` (A4 landscape), four embedded font files (`Cinzel-Bold`, `ArialMT`, `Arial-BoldMT`, `CourierNewPS-BoldMT`), four images (logo, signature, branding), zero `/Annots` or `/Link`. The SQL snapshot row carried `AT-2026-000001`, recipient `Liza Fernandez`, event `Leadership Summit 2026`, event start `2026-11-08 07:43:48.923`, issuer `Rafael Santos`, file `certificates/<guid>.pdf`. A second issue on a later fixture produced `AT-2026-000021` with the same template and the correct event title and start. No sample or watermarked render appeared. |
| Signatory sub-field | Name `Christian Kyle Ducay`, title `Organization President`, signature source `wwwroot/img/christian-kyle-ducay-signature.png` (packaged asset, validated by `CertificateSignatoryOptions` against the approved constants). **Human approval of these values: NOT RECORDED.** SPEC §20 says the signatory "remains pending and must be supplied before official certificate issuance", so this sub-field is **BLOCKED** until the human confirms the name, title and signature as the approved signatory. |
| Result | **PASS** for the PDF content, branding, fonts, snapshots and download; the signatory sub-field is **BLOCKED** on human approval |

### OPS-01 — demo-host readiness under the runtime identity

| Field | Value |
|---|---|
| Requirement | SPEC §20 bullet 9; `finale-test-cases.md` OPS-01; `docs/demo-host-readiness-evidence.md` |
| Preconditions | A provisioned demo host, a dedicated Windows service identity, a dedicated SQL login, `AttendTa_Demo`, a staged `PLAYWRIGHT_BROWSERS_PATH`, an approved disconnection method |
| Steps | 1. Run `--demo-readiness` as the service identity. 2. Confirm the database connect and `DB_NAME()`. 3. Inspect effective server permissions. 4. Inspect table and column-level `UPDATE`/`DELETE` denial on `activity_logs`. 5. Run the rollback probe. 6. Inspect the six private areas and `logs/<yyyyMM>`. 7. Launch staged headless Chromium while disconnected and render a PDF. 8. Request `http://127.0.0.1:<port>/Account/Login` without following redirects. 9. Record the Argon2 benchmark. |
| Expected outcome | Every readiness row verified on the recorded host, or an explicit blocker |
| Evidence | **BLOCKED.** Missing input: a real demo host, a dedicated service identity and a dedicated SQL login. `docs/demo-host-readiness-evidence.md` rows 1–10 are `BLOCKED` (no host, no `AttendTa_Demo`, no runtime login, no staged browser, no disconnection method). A closed #74 is not evidence. The local machine is not the demo host. |
| Result | **BLOCKED** |

### OPS-02 — backup, restart/redeploy and restore to a separate disposable target

| Field | Value |
|---|---|
| Requirement | SPEC §20 bullet 9; `finale-test-cases.md` OPS-02; `docs/storage-survival-evidence.md`; `docs/storage-backup-restore.md` |
| Preconditions | The demo host, an approved backup destination, an approved empty restore target, a maintenance window and an approver |
| Steps | 1. Stop every writer and capture the baseline (row counts, certificate status counts, reference counts, tree manifest, key count, digests). 2. Take a `COPY_ONLY` SQL backup with `CHECKSUM` and `VERIFYONLY`. 3. Capture the complete private tree and compare it with the source. 4. Restart with the same root and database and re-check. 5. Redeploy excluding the setup root and re-check. 6. Restore to a new database and root with `WITH MOVE` and `CHECKDB`. 7. Compare rows, references, keys and digests. 8. Confirm isolated access and decryption, then restore the original host state. |
| Expected outcome | Images, certificates and keys persist; the restored bundle is coherent; no shared or production database is reset or overwritten |
| Evidence | **BLOCKED.** Missing input: a real host, an approved backup destination, an approved restore target and an operator/approver. `docs/storage-survival-evidence.md` records every stage `BLOCKED`; its local section explicitly says the helper proves fixtures only and is not host acceptance. No backup, restore, stop, restart or redeploy was performed for this ticket, and none was performed against `AttendTa_Demo` or `AsiBasecodeDB`. |
| Result | **BLOCKED** |

### UI-01 — role, route, 200 and dead-link walkthrough against real data

| Field | Value |
|---|---|
| Requirement | SPEC §20 (integrated release gate); `finale-test-cases.md` UI-01; `demo-script.md` §4–§6 beats |
| Preconditions | Seeded `AttendTaMVP_Test`, Production build, three signed-in roles |
| Steps | 1. Sign in as Administrator, Officer and Member in separate browser contexts. 2. Open every role route from `demo-script.md` §4–§6. 3. Record the status, final URL and page heading. 4. Harvest every same-origin `href` from the visited screens and request each one. 5. Request management routes as the member and member routes as the officer. 6. Request a member route anonymously. |
| Expected outcome | Every required route is reachable in the correct role with 200 and its own heading; no dead link; no prototype-only role switching; wrong-role and anonymous requests are refused |
| Evidence | Executed on `d8046bf`, `http://localhost:5080`, Production. Administrator: `/Home/Index` ("Welcome back, Maria Clara Dizon"), `/AccountRequest`, `/Member`, `/EventType`, `/Venue`, `/Event`, `/admin/accounts`, `/admin/auditlogs`, `/RegistrationReview/Index`, `/Certificates`, `/Reports/Participation`, `/Event/Details?id=1` — all 200 with their own headings. Officer: `/Home/Index` ("Welcome back, Rafael Santos"), `/Event`, `/Event/Details?id=1`, `/RegistrationReview/Index`, `/CheckIn`, `/Certificates`, `/Reports/Participation?eventId=1` — all 200. Member: `/Home/Index` ("Welcome back, Liza Fernandez"), `/Events`, `/MemberEvents/Details?id=3`, `/MyEvents/Registrations`, `/MyEvents/Attendance`, `/MyEvents/Certificates`, `/Passes`, `/Profile` — all 200. Dead-link crawl: 78 distinct same-origin links requested; every non-200 was an intentional 302 index redirect (`/MyEvents`, `/MyEvents/Registrations`, `/MyEvents/Attendance`, `/MyEvents/Certificates`, `/Passes/Index`, `/Profile/Index`) or a POST-only endpoint; **no dead link**. Role separation: member `/CheckIn`, `/Member`, `/admin/accounts` → `/Account/AccessDenied`; officer `/Passes`, `/Profile` → `/Account/AccessDenied`; anonymous `/Member` → `/Account/Login?ReturnUrl=%2FMember`. No page script errors were raised. The 8.12 Session G dead-end crawl (90 links, no dead ends) is cited, not repeated. |
| Result | **PASS** |

### UI-02 — keyboard, dialog focus, validation and 200% zoom at 1440/1024/768/390

| Field | Value |
|---|---|
| Requirement | SPEC §20 (integrated release gate); `finale-test-cases.md` UI-02 |
| Preconditions | The same seeded build; real key presses in a real browser |
| Steps | 1. Tab through `/Account/Login` and record focus order and the visible focus ring. 2. Submit the empty form with Enter and read the labelled errors. 3. Open the member record dialog from the list with the keyboard. 4. Record whether the dialog is centred, inside the viewport and holds focus; Tab twelve times and confirm focus never leaves the dialog for another page control. 5. Press Escape and confirm focus returns to the invoking control. 6. On `/CheckIn?eventId=3`, reach the three status tabs by keyboard: focus "Camera scan", Tab to the next tab, press ArrowRight, then Enter on "Manual lookup". 7. Open the invalidate-attendance reason dialog from a recorded check-in: confirm focus lands on the reason field, Tab through it, submit with the reason empty, press Escape, reopen and press Cancel, then reopen again. 8. Load every role route at 1440, 1024, 768 and 390 and record document-level horizontal overflow. 9. Emulate 200% zoom (720 CSS px at device scale 2, the 1440/2 CSS viewport) on the Administrator and Member routes; the officer routes are covered at 390px, which is narrower than the 616px 200%-zoom viewport. |
| Expected outcome | Visible focus; contained and restored dialog focus; labelled errors; status tabs reachable and switchable by keyboard with a visible focus ring; the reason dialog refuses an empty reason and returns focus to its trigger on Escape and Cancel; no page-level horizontal overflow at any width or at 200% zoom; dense tables may scroll inside their own region |
| Evidence | Executed on `d8046bf`, `http://localhost:5080`. Tab order on `/Account/Login`: skip link → Email → Password → Sign in → links, each with a computed `outline: solid 2px`. Empty submit produced `Email is required.` and `Password is required.` with `aria-invalid="true"` on Password. The member record dialog opened centred (centre offset < 40px), fully inside the viewport, with focus inside (`INPUT`); twelve Tab presses never left the dialog; Escape closed it and returned focus to the invoking `Edit` button. Status tabs on `/CheckIn?eventId=3`: three `role="tab"` controls; the focused tab computed `outline: rgb(29, 78, 216) solid 2px`; ArrowRight moved focus to "Pass code" and set `aria-selected="true"` on it; Enter on "Manual lookup" set its `aria-selected="true"` and revealed `#panel-manual`. Reason dialog (invalidate attendance, opened from a recorded check-in): opened centred at x=480 with width 480 in a 1440 viewport, focus on `#invalidate-reason`; a 14-step Tab trail cycled close → Cancel → browser chrome → close → reason → submit → close, never landing on a page control outside the dialog; an empty submit was refused with `Please fill out this field.`; Escape closed it and returned focus to the trigger (`data-dialog-open="invalidate-dialog"`); Cancel closed it and returned focus to the same trigger; reopening showed an empty reason. Document-level overflow (`scrollWidth − clientWidth`) was **0** on every route at 1440, 1024, 768 and 390 (Administrator: Home, Member, Event, Venue, EventType, admin/accounts, admin/auditlogs, AccountRequest, Event/Details, Certificates, Reports/Participation, CheckIn; Member: Events, MemberEvents/Details, MyEvents ×3, Passes, Profile; Officer: CheckIn, Event/Details) and **0** at the 200% zoom viewport on the Administrator and Member routes. The 8.12 Session G width matrix and its keyboard/dialog evidence are cited for the geometry harness and the row-menu focus cases; this row adds the actual 200% zoom check that Session G recorded as unverified, plus the status-tab and reason-dialog checks. |
| Result | **PASS** |

### M-ADM-01 — attended data survives deactivation; demote-vs-deactivate race

| Field | Value |
|---|---|
| Requirement | SPEC §20 bullets 1 and 4; matrix §1 ADM-01 and §6; `finale-test-cases.md` ADM-01 |
| Preconditions | A member account with an issued certificate and an attendance row; Administrator and Member sessions |
| Steps | 1. Sign in as the member and open `/MyEvents/Attendance` and `/MyEvents/Certificates`. 2. As Administrator open `/admin/accounts`, request `DeactivationPreview/3` and read the affected registrations. 3. Deactivate the account with a reason. 4. Reuse the member session and request `/MyEvents/Certificates`. 5. Sign in again with the member credentials. 6. As the officer, open the Issued Registry and download the PDF. 7. Reactivate the account. 8. Sign in again and re-open Attendance and Certificates. |
| Expected outcome | The preview lists the affected registrations; deactivation ends the session and refuses sign-in generically; attendance and certificate rows survive; the officer can still view and download the issued PDF; reactivation restores login access only and restores no seat |
| Evidence | Executed on `d8046bf`, `http://localhost:5080`, Production. Preview: `Deactivation impact preview — Liza Fernandez · liza.fernandez@uc.edu.ph — Deactivating this account cancels these registrations. Founders' Week Assembly 2026-11-13 15:43 Asia/Manila`. Deactivate → 302 to `/admin/accounts`; the member's existing session was refused on its next request (`/MyEvents/Certificates` → `/Account/Login?ReturnUrl=%2FMyEvents%2FCertificates`); a fresh sign-in returned the generic `Incorrect email or password.`; the officer's Issued Registry still listed `Liza Fernandez … ISSUED AT-2026-000001` and `GET /Certificates/Pdf/1` returned 200 with 447,096 bytes. Reactivate → 302; the member signed in again and `/MyEvents/Attendance` still showed `Leadership Summit 2026 2026-11-11 15:46 Asia/Manila Manual search VALID` and `/MyEvents/Certificates` still showed `ISSUED AT-2026-000001`. SQL after the cycle: `users.is_active = 1`, the attendance row untouched, the certificate row `status 4`, file present. The audit log recorded `user.deactivated`, `registration.cancelled` (the upcoming Founders' Week seat, with the reason) and `user.reactivated`, in order. The demote-vs-deactivate race of the last two administrators is **BLOCKED as not reproducible manually** — two live sessions cannot produce the required interleaving; the automated same-kind races `AdminAccountDeactivationTests` / `AdminRoleAssignmentTests` (parallel deactivations and parallel demotions each leave one active admin) own that proof. |
| Result | **PASS** for the sequential attended-data survival; the mixed race is **BLOCKED** with the automated races cited |

### M-ATT-02 — scan racing an in-flight invalidation (demonstration)

| Field | Value |
|---|---|
| Requirement | SPEC §20 bullet 5; matrix §1 ATT-02 and §6 |
| Preconditions | Two officer sessions on the same event with an open check-in window and a recorded attendance row |
| Steps | 1. Open `/CheckIn?eventId=3` in session A and locate the recorded check-in. 2. Hold the invalidation dialog open in session A. 3. In session B submit a duplicate scan of the same registration's token. 4. Submit the invalidation in session A at the same moment. 5. Read the outcome panel and the recorded check-ins table. |
| Expected outcome | The sequential guard is automated; the interleaving is recorded as a demonstration, not proof |
| Evidence | **Demonstration, not proof.** Executed on `d8046bf`, `http://localhost:5080`. Session A held the invalidation form for registration 13; session B posted `POST /CheckIn/Scan` with that registration's stored `qr_token` while session A posted `POST /CheckIn/Invalidate` with the reason `Pass could not be verified at the door.` Both returned 302 to `/CheckIn/Index?eventId=3`. The recorded check-ins table then showed `Mark Villanueva … INVALIDATED Pass could not be verified at the door.`; SQL showed the attendance row still present with `invalidated_at` set and exactly one new `attendance.invalidated` audit row — no second attendance row and no re-check-in. The observed interleaving was invalidation-then-scan; a sub-second scan-then-invalidate ordering was not reproducible by hand. The automated `AttendanceInvalidationFlowTests` and `CheckInFlowTests` (parallel duplicate scans) own the proof. |
| Result | **PASS** as a demonstration; the proof is REUSED from the automated suite |

### M-AUD-01 — `activity_logs` column permissions under the runtime SQL identity

| Field | Value |
|---|---|
| Requirement | SPEC §20 bullet 9; matrix §1 AUD-01 and §6; shared with OPS-01 |
| Preconditions | A provisioned demo host with a dedicated SQL login for the application |
| Steps | 1. Connect as the runtime login. 2. Query effective table and column permissions on `activity_logs`. 3. Attempt an `UPDATE` and a `DELETE` through the actual runtime identity. 4. Confirm the append-only trigger and the administrator-only newest-first viewer still behave. |
| Expected outcome | Table-level `UPDATE`/`DELETE` and every column-level `UPDATE` on `activity_logs` are absent for the runtime login; the append-only trigger refuses mutation; the viewer is administrator-only and newest-first |
| Evidence | **BLOCKED.** Missing input: a provisioned runtime SQL login on a demo host. `docs/demo-host-readiness-evidence.md` check 3 is `BLOCKED` for exactly this reason. The append-only trigger and the administrator-only newest-first viewer are automated (`TransactionAuditTests`, `ActivityLogViewerTests`) and were observed working in this run (the Activity Log rendered newest first with the real actions above), but the column-level `GRANT`/`DENY` check needs the host identity. |
| Result | **BLOCKED** |

### M-AUTH-01 — 8-hour, non-sliding cookie expiry

| Field | Value |
|---|---|
| Requirement | SPEC §20 bullet 1; matrix §1 AUTH-01 and §6 |
| Preconditions | A real browser session signed in through the real form |
| Steps | 1. Sign in as the officer. 2. Inspect the `AttendTa.Auth` cookie flags. 3. Record the ticket lifetime. 4. Leave the session idle for 8 hours and re-request a management route. 5. Confirm the session ends at 8 hours and does not slide. |
| Expected outcome | `AttendTa.Auth` is `HttpOnly`, `SameSite=Lax`, a session cookie (no `Expires`), with an 8-hour ticket; the session ends after 8 hours and does not slide |
| Evidence | Executed on `d8046bf`, `http://localhost:5080`. Cookie `AttendTa.Auth`: `domain=localhost`, `path=/`, `httpOnly=true`, `secure=false` (plain HTTP origin), `sameSite=Lax`, `expires=session`. The antiforgery cookie is `HttpOnly`, `SameSite=Strict`, session-scoped. The encrypted ticket is not decodable without the Data Protection keys, so the 8-hour lifetime is not readable from the cookie in this run. **The 8-hour wait is incomplete**: BLOCKED on an 8-hour idle observation. The flags above are recorded as partial evidence; the automated `AuthenticationFlowTests` own the non-persistent HttpOnly cookie assertion. |
| Result | **BLOCKED** — flags recorded as partial evidence; 8-hour expiry not observed |

### M-CERT-02a — private-file-store failure during issuance

| Field | Value |
|---|---|
| Requirement | SPEC §20 bullet 7; matrix §1 CERT-02 and §6 |
| Preconditions | A second app instance on a spare port with a fresh `Common__SetupRoot` whose `certificates` folder denies writes; a completed event with an approved review |
| Steps | 1. Start the second instance on `http://localhost:5091` with the fresh setup root. 2. Check in two participants of a future event, complete it after its start, and approve both reviews. 3. Deny write access on `<SetupRoot>/certificates` for the runtime identity and confirm the denial with a probe write. 4. Issue one certificate through the dialog. 5. Assert no issued row, no retained file and no `certificate.issued` audit row, and a clean retryable state. 6. Restore the ACL and retry the issue. |
| Expected outcome | No false issued row on failure; no retained file; no audit row; the review stays approved and retryable; the retry succeeds after the store is restored |
| Evidence | Executed on `d8046bf`, second instance `http://localhost:5091`, Production, same disposable database, fresh setup root. Fixture: event 11 (`Acceptance Store Failure Event`), two confirmed registrations (23, 24) checked in, event completed after its start, both reviews approved (`APPROVED — Not issued`). ACL: `icacls <SetupRoot>\certificates /deny <user>:(OI)(CI)(W)`; probe write returned `EPERM` (write denied). Issue → the row stayed `APPROVED / Not issued`, the flash read `The certificate could not be rendered. Please try again.`, SQL showed **no** certificate row for registration 24, **no** `certificate.issued` audit row, and the `certificates` folder gained no file; the `temporary` folder held zero files. After `/remove:d` and a full-control grant (probe write allowed), the same review (registration 24) was retried and issued `AT-2026-000024` with `certificates/4bc185639d50492e982f64dcb2eaa0b5.pdf` (446,040 bytes) and one `certificate.issued` audit row; the other approved review (registration 23) issued `AT-2026-000023`. The same failure-then-issue pair was observed on event 10 (registration 22 stayed `APPROVED / Not issued`, registration 21 issued `AT-2026-000021`). The certificate number is `AT-<year>-<registrationId:D6>` (`ASI.Basecode.Services/Services/CertificateLedgerService.cs:240`), so the retry of registration 24 is `AT-2026-000024` and registration 23 is `AT-2026-000023`. The retry was re-run on the same commit with a fresh throw-away setup root after the original root was discarded. |
| Result | **PASS** |

### M-CERT-02b — edit the event after issuance

| Field | Value |
|---|---|
| Requirement | SPEC §20 bullet 7; matrix §1 CERT-02 and §6 |
| Preconditions | An issued certificate on a completed event; a published event that can still be edited |
| Steps | 1. Open `/Event/Edit?id=1` for the completed event. 2. Attempt the edit. 3. Open a published event's editor and confirm editing still works. 4. Re-open the Issued Registry and the retained PDF. |
| Expected outcome | A completed event cannot be edited; the retained PDF and its snapshots are unchanged; a published event stays editable |
| Evidence | Executed on `d8046bf`, `http://localhost:5080`. `/Event/Edit?id=1` (Completed) → 302 to `/Event` with `Only draft or published events can be edited.`; the Events list offered no Edit link for event 1. `/Event/Edit?id=3` (Published) → 200 with the editor form. The Issued Registry still showed `Liza Fernandez … ISSUED AT-2026-000001`, and the SQL snapshot row still carried `Leadership Summit 2026`, `2026-11-08 07:43:48.923` and `Rafael Santos` after the attempt. The member history still listed the issued certificate with View and Download. The automated `MemberManagementTests` snapshot-immutability case is cited for the member-edit equivalent. |
| Result | **PASS** |

### M-MD-01 — PNG through the venue dialog (premise re-check)

| Field | Value |
|---|---|
| Requirement | SPEC §20 bullet 2; matrix §1 MD-01 and §6 |
| Preconditions | An Administrator session; a real PNG file |
| Steps | 1. Re-check the matrix premise "the store accepts only JPEG end-to-end today" against `PrivateFileStore.ValidateSignature`, `NormalizeExtension`, `VenueController.Photo` and `Views/Venue/_Form.cshtml`. 2. Open `/Venue` → New Venue, fill the fields, attach a real PNG, save. 3. Confirm the record and its photo. 4. Open the venue record dialog, replace the photo with a different PNG, save. 5. Confirm the served photo changed. 6. Re-open the dialog and save without a photo. 7. Confirm the previous photo is retained and no error is shown. |
| Expected outcome | PNG is accepted end-to-end; the photo is served as `image/png`; replacement swaps the file; a missing photo is refused on create and keeps the current photo on edit |
| Evidence | Executed on `d8046bf`, `http://localhost:5080`. **The matrix premise is stale, not a defect.** `PrivateFileStore.NormalizeExtension` admits `.jpg`, `.jpeg` and `.png` for venue and event images; `ValidateSignature` checks the PNG magic number for `.png`; `VenueController.Photo` maps `.png` to `image/png`; `Views/Venue/_Form.cshtml` sets `accept="image/png,image/jpeg"`. Create: `New Venue` dialog fields (`RoomName`, `Capacity`, `Location`, `AvailabilityNotes`, `ContactPerson`, `photo`) accepted a real PNG; `Venue added successfully.`; the row appeared and `GET /Venue/Photo?id=3` returned `200 image/png`. Replace: the record dialog (`/Venue/Edit`, `accept="image/png,image/jpeg"`) accepted a second PNG and the served bytes changed (`changed=true`). Keep current photo: re-opening the dialog and saving without a photo produced `venue.edited` and the previous photo was still served. That is the designed edit behaviour, not a defect: `VenueController.Edit` sets `PhotoRequired = false`, `VenueService.UpdateVenueAsync` assigns `ImagePath` only when a new file exists (`if (newPath != null)`), and `_Form.cshtml` labels the field "Select a new JPEG or PNG photo up to 2 MiB to replace the current one". The required-photo guard applies to create (`/Venue/Create`, `PhotoRequired = true`), and the schema's non-empty `image_path` stays satisfied by the retained file. |
| Result | **PASS** — PNG create, replace and keep-current-photo all behave as designed. Matrix note: the "PNG not accepted end-to-end" premise in `acceptance-coverage-matrix.md` §1 and §6 is stale and should be corrected against `d8046bf`. |

## 6. Residual and functional-ID cross-reference

Every matrix §6 residual appears above exactly once. Matrix §3 groups (functional IDs) are covered
by the §1 automated index plus these rows: AUTH-01–03 → M-AUTH-01, M-ADM-01; MD-01–05 → M-MD-01;
EVT-01–04 → automated; REG-01–05 → automated; QR-01 → ATT-03, M-ATT-02; ATT-01–03 → ATT-03,
M-ATT-02; CERT-01–02 → CERT-03, M-CERT-02a, M-CERT-02b; REP-01 → automated; ADM-01–02 →
M-ADM-01, M-AUD-01; FR-35–36 → automated.

| Matrix §6 residual | Row above | Result |
|---|---|---|
| M-AUTH-01 — 8-hour, non-sliding cookie expiry | M-AUTH-01 | BLOCKED (flags recorded) |
| M-MD-01 — PNG upload through the venue dialog | M-MD-01 | PASS (premise stale) |
| M-ATT-02 — scan racing an in-flight invalidation | M-ATT-02 | PASS as demonstration |
| M-ADM-01 — attended data survives deactivation; demote-vs-deactivate race | M-ADM-01 | PASS (race BLOCKED) |
| M-CERT-02a — private-file-store failure during issuance | M-CERT-02a | PASS |
| M-CERT-02b — event edited after issuance | M-CERT-02b | PASS |
| M-AUD-01 — `activity_logs` column permissions under the runtime identity | M-AUD-01 | BLOCKED |
| ATT-03, CERT-03, OPS-01, OPS-02, UI-01, UI-02 | ATT-03, CERT-03, OPS-01, OPS-02, UI-01, UI-02 | ATT-03 BLOCKED; CERT-03 PASS (signatory BLOCKED); OPS-01 BLOCKED; OPS-02 BLOCKED; UI-01 PASS; UI-02 PASS |

## 7. Summary

| Result | Count | Rows |
|---|---|---|
| PASS | 8 | UI-01, UI-02, CERT-03 (content), M-ADM-01 (sequential), M-ATT-02 (demonstration), M-CERT-02a, M-CERT-02b, M-MD-01 |
| BLOCKED | 5 | ATT-03, OPS-01, OPS-02, M-AUD-01, M-AUTH-01 |
| BLOCKED sub-fields | 2 | CERT-03 signatory approval; M-ADM-01 mixed race |
| FAIL | 0 | — |
| Defects | 0 | §9 — no product defect; one stale coverage-map premise noted |
| REUSED | 13 case ids; 7 of 9 SPEC bullets | §4; §3 B1–B4 and B6–B8 wholly REUSED, B5 and B9 BLOCKED, B10 signatory BLOCKED |

The release gate is not reached by this record alone: ATT-03, OPS-01, OPS-02, M-AUD-01 and the
8-hour M-AUTH-01 observation are host, phone and time inputs that no local run can supply.

## 8. Sign-off block

| Field | Value |
|---|---|
| Record complete | Yes — every SPEC §20 bullet and every matrix §1, §4 and §6 case appears once with a result |
| Operator | AI agent (subagent), 2026-11-11 |
| Human evaluator | — (blank, to be filled by the human) |
| Evaluator decision | — (blank: PASS / FAIL / retest) |
| Date | — |
| Affected retests | — |

## 9. Defects found

No product defect was found. One coverage-map premise is stale and is recorded as a note, not a
defect: `acceptance-coverage-matrix.md` §1 and §6 say the venue store accepts only JPEG
end-to-end, but `PrivateFileStore.NormalizeExtension` admits `.png` and `ValidateSignature`
checks the PNG magic number, and a real PNG created, replaced and re-served through the venue
dialog on `d8046bf`. Correcting that sentence is 8.5's record, not this ticket's.

| Id | Row | Observation | Severity | Filed |
|---|---|---|---|---|
| — | — | No defect found during this run | — | — |
