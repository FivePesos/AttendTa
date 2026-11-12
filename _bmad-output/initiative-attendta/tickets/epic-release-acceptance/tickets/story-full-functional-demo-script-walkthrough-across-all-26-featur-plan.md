---
title: 'Full functional demo script walkthrough across all 26 feature IDs'
type: 'chore'
ticket: '7'
created: '2026-11-11'
status: 'built'
baseline_revision: '84b3f54e58d127f993d55c45a9287dc9b267dda5'
route: 'full'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
followup_review_recommended: true
context:
  - '_bmad-output/initiative-attendta/demo-script.md'
  - '_bmad-output/initiative-attendta/manual-acceptance-checklist.md'
  - '_bmad-output/initiative-attendta/acceptance-coverage-matrix.md'
  - '_bmad-output/initiative-attendta/finale-test-cases.md'
warnings: []
deferred: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** SM-2 / R2 requires all 26 functional IDs plus FR-35 and FR-36 demonstrated end to end on the integrated build. The only evidence is 8.6's per-case checklist (isolated cases, no continuous journey) and 8.4's automated run; no record shows one connected request-to-certificate/report lifecycle, and FR-35/FR-36 have no demo evidence at all.

**Approach:** Execute the demo script's beats as one continuous session on a freshly seeded disposable database — public request, approval, new-member sign-in, master data, event create/publish, registration, pass, check-in, completion, certificate review/issue/download, report, access changes, activity log — then write one walkthrough record that maps FR-1..FR-36 and every one of the 26 functional IDs to the journey step that demonstrates it or to the 8.6/8.4 evidence it reuses. Host/phone-only rows stay BLOCKED.

## Decisions

- 2026-11-11 — the record lives at `_bmad-output/initiative-attendta/demo-walkthrough-evidence.md`, beside `demo-script.md` and the 8.6 checklist.
- 2026-11-11 — the certificate/report leg runs on the seeded started event (Leadership Summit), because a newly published event cannot be completed before its start; the new member's own registration demonstrates signup, review, pass and check-in on the event created during the journey.
- 2026-11-11 — FR-36's multi-page proof uses 26 real account requests submitted through the public form after the journey. Real form submissions are data, not fixtures; SQL inserts are not used.

## Boundaries & Constraints

**Always:** Evidence only — no product, Razor, CSS, schema or test change. Drive real forms and real services; SQL is read-only for verification. Record tested commit, environment and date. Every FR-1..FR-36 and every one of the 26 functional IDs appears exactly once with a result. Cite 8.6's checklist, 8.4's manifest and 8.12's walkthrough instead of repeating them. BLOCKED rows name the missing input and the owning evidence file. No secrets, hashes, tokens or private paths in the record.

**Never:** Fixtures, browser-storage workarounds, direct SQL state editing, or a fake issued outcome. Build a second screen or change presentation. Close the ticket without the human sign-off. Reset or run destructive scripts against `AttendTa_Demo`, `AsiBasecodeDB` or any shared database.

</frozen-after-approval>

## Code Map

- `_bmad-output/initiative-attendta/demo-walkthrough-evidence.md` (new) — the deliverable record.
- `_bmad-output/initiative-attendta/demo-script.md` — pre-flight §2, accounts §2.5, starting state §3, beats A1–A7/O1–O10/M1–M9, 390px checks §7; read-only.
- `_bmad-output/initiative-attendta/manual-acceptance-checklist.md` — 8.6 execution record to reuse: UI-01, UI-02, CERT-03, M-ADM-01, M-ATT-02, M-CERT-02a/b, M-MD-01, and the five BLOCKED rows; read-only.
- `_bmad-output/initiative-attendta/acceptance-coverage-matrix.md` §1/§3/§5 — case→test map and the 26-ID grouping; read-only.
- `_bmad-output/initiative-attendta/execution-manifest-fa9ae82.md` — 8.4 run to cite (321 service, 479 integration); read-only.
- `_bmad-output/initiative-attendta/prd-attendta/prd-attendta.md` §4 FR-1..FR-36 and §7.1 — the mapping authority; read-only.
- `database/reset-local-mvp-test-db.sql`, `attendta-mvp-schema.sql`, `seed-local-admin.sql`, `seed-event-types.sql`, `seed-demo-scenario.sql` — disposable `AttendTaMVP_Test` fixture.
- Route surface (read-only): `AccountRequestController` (`RequestForm`, `SubmitRequest`, `Submitted`, `Approve`, `Reject`), `AccountController` (`Login`, `Logout`), `EventController` (`Create`, `Cover`/`UploadCover`, `Publish`, `OpenCheckIn`, `Complete`), `MemberEventsController` (`Register`), `PassesController` (`Qr`), `RegistrationReviewController` (`Approve`), `CheckInController` (`Scan`, `CheckIn`, `Invalidate`), `CertificateLedgerController` (`Approve`, `Issue`, `Pdf`), `ReportsController` (`Participation`), `AdminAccountsController` (`Deactivate`, `ChangeRole`), `ActivityLogController` (`/admin/auditlogs`), `MyEventsController`, `ProfileController`, `MemberController`, `EventTypeController`, `VenueController`.
- `ASI.Basecode.WebApp/Views/AccountRequest/Request.cshtml` — public fields (StudentNumber, FullName, Email, Course, YearLevel, Section, MobileNumber, Password, ConfirmPassword); there is no role field.

## Tasks & Acceptance

**Execution:**
- [x] `demo-walkthrough-evidence.md` — author the header (ticket, tested commit, build, date, operator, OS/runtime, database, app process, private store, reused evidence), the result legend, and the journey table skeleton.
- [x] Same file — provision: reset `AttendTaMVP_Test`, apply `attendta-mvp-schema.sql`, generate three Argon2id hashes with `--hash-password`, run `seed-local-admin.sql`, `seed-event-types.sql`, `seed-demo-scenario.sql`, copy the private images, start the app Production on `http://localhost:5080`.
- [x] Same file — execute the public request beat (FR-1), admin approval (FR-2), and sign-in as the newly created member (FR-3); confirm the approved account is Member role with the submitted course/year/section.
- [x] Same file — execute A1–A7: home, request queue, member search, event-type/venue/member editors, draft event + cover + publish, access preview/deactivate, activity log.
- [x] Same file — execute M1–M9: home, browse, event detail, signup, My Events, attendance, certificates, passes (QR + PNG), profile.
- [x] Same file — execute O1–O10: dossier, completion refusals, registration review, check-in window, pass-code scan and duplicate, manual lookup, invalidation, certificate review/issue/PDF, report.
- [x] Same file — FR-35: run `seed-local-admin.sql` on the fresh database, record one active Administrator with an Argon2id hash, no default credentials, the re-run behaviour, and that the public form offers no role.
- [x] Same file — FR-36: submit 26 real requests through the public form, then record 25 rows per page, Previous/Next, page 2, invalid page → page 1, out-of-range → last page, hidden navigation on a single-page list, and an empty state.
- [x] Same file — write the FR-1..FR-36 table and the 26-functional-ID table, each row naming the journey step or the reused 8.6/8.4/8.12 evidence; add the blocked rows, the no-workaround statement, the defects table and the blank sign-off block.

**Acceptance Criteria:**
- Given the freshly seeded disposable database, when the evaluator submits the public request form with valid details, then the acknowledgment page appears and the request appears in the admin queue.
- Given a pending request, when the administrator approves it, then the applicant can sign in with the password typed on the form and the profile shows the submitted course, year level and section.
- Given the administrator saves a draft event and uploads a cover, when they publish it, then the member feed lists it and the member's registration shows as pending review.
- Given a confirmed registration, when the officer opens check-in and submits that pass's token, then the outcome names the participant and the recorded check-ins table shows the row; a second submission returns the original time.
- Given valid attendance on a started published event, when the officer completes it, then a certificate review exists for that participant, and after approval and issuance the member's certificates page offers the PDF and the report counts the valid attendee.
- Given the journey ran through real forms, when the record is read, then no step relies on a fixture, browser storage, SQL state editing or a fake issued outcome.
- Given the finished record, when an evaluator opens it, then FR-1..FR-36 and all 26 functional IDs appear exactly once with a result.
- Given a host- or phone-only requirement, when it cannot run here, then its row is BLOCKED with the missing input named and no PASS inferred.

## Implementation Notes

Executed 2026-11-11 on `84b3f54` against a freshly seeded `AttendTaMVP_Test`, Production build on
`http://localhost:5080`. The record is `_bmad-output/initiative-attendta/demo-walkthrough-evidence.md`
(68 journey rows, 36 FR rows, 26 functional-ID rows, 7 blocked rows, 0 defects).

- The journey member is Ava Mendoza (`UC-2026-0111`), created by the public request and approval;
  the seeded member Liza Fernandez holds the issued certificate because the certificate leg needs a
  started event.
- The new event is Peer Tutoring Session (Workshop type, Innovation Lab venue), created, covered and
  published in A5; it carries the registration, pass and check-in legs.
- The certificate and report legs complete the seeded Leadership Summit: manual check-in of Liza and
  Mark, completion, approve/decline, issuance of `AT-2026-000001`, PDF and report.
- Two check-in methods ran: pass-code scan on the new event and manual lookup on the started event.
- FR-17 (promotion) is REUSED because the journey had no full event; FR-19's exact start boundary is
  REUSED and only the post-completion refusal was observed.
- Three demo-script and demo-data discrepancies are recorded as notes, not defects: `MemberEvents/Details?id=2`
  returns 404 for a cancelled event, the seeded confirmed registrations carry no QR token, and
  `demo-script.md` §5's "the token is read from the pass page" is stale.
- Verification: `dotnet build ASI.Basecode.sln --configuration Release` 0 warnings / 0 errors; a
  coverage script confirmed 36 FR rows and 26 functional-ID rows with no missing or duplicate ids;
  `git diff --check` clean. The record and this plan are the only files added.

## Plan Change Log

## Review Triage Log

### 2026-11-11 — Review pass
- verdicts: 9 findings — high 0, medium 4, low 5, false 0, maybe-false 0
- findings:
  - `[medium]` `[patch]` FR-12 read PASS though the journey never cancelled an event; J26 is a completion refusal on an already-cancelled event and J15's `?id=2` is a 404 — verified against `MemberEventsController.Details` and PRD FR-12; the row now reads REUSED with `EventCancellationFlowTests` (8.4) and keeps J26/note 1 as supporting observations.
  - `[medium]` `[patch]` FR-19 and REG-05 cited J63, whose refusal is the event-status guard, not the start cutoff — verified in `RegistrationReviewService` (status check at line 89 before `StartCutoffPassed` at 91, message "This event has already started."); both rows now mark the start-cutoff clause REUSED (`MemberEventDeadlineFlowTests`, `EventCompletionFlowTests`) and keep J63 as the post-completion refusal.
  - `[medium]` `[patch]` ATT-03 carried two different results (PASS in §6, BLOCKED in §7) while §11 claimed one result per ID — real against the plan's "exactly once" criterion; §6 now carries one composite result, §7 stays the blocked-part detail, and §11 names §5/§6 and §7 explicitly.
  - `[medium]` `[patch]` §9 said SQL was read "for verification" while it also supplied the pass's `qr_token`, and `demo-script.md` §5's "token is read from the pass page" is stale — verified in `Views/MyEvents/_PassPartial.cshtml` (QR image and download link only); §8 note 3 records the stale expectation and §9 now says the read supplied the pass's own token.
  - `[low]` `[patch]` The blocked table dropped 8.6's M-ADM-01 mixed-race sub-field and ADM-01 did not cite 8.6 for attended-data survival — verified against `manual-acceptance-checklist.md` §5 M-ADM-01 and §7's summary; the row is added and ADM-01 cites it.
  - `[low]` `[patch]` J20 did not name the submitted course/year/section the second acceptance criterion needs — the row now names them and cross-references J6.
  - `[low]` `[patch]` FR-9 cited only the draft create while the requirement is create and edit — the draft-edit part is now REUSED to `EventDraftFlowTests` (8.4).
  - `[low]` `[patch]` AUTH-02 and ADM-02 read PASS without citing the automated evidence for clauses the journey did not run — `AuthenticationFlowTests`, `AccountRequestDecisionTests`, `AdminAccountsListTests` and `TransactionAuditTests` are now cited.
  - `[low]` `[patch]` The legend defined four values while the tables used composites — one Composite legend row now defines the form and its citation requirement.

## Design Notes

One record file, not an edit of `demo-script.md`: the script is the presenter's instructions, the checklist is 8.6's per-case evidence, and 8.7 needs a single place that shows the continuous journey and the FR-to-step mapping. The certificate leg uses the seeded started event because publication requires a future signup deadline and completion requires a started event, so a single new event cannot reach issuance in one sitting. FR-36's 26 requests are created through the real public form, which is data entry, not a fixture: the prohibition targets seeded or mocked state that substitutes for service behaviour. BLOCKED stays the honest result for camera scanning, host readiness and restore, whose evidence files are still BLOCKED; a closed issue or merged tool is not host evidence.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — expected: 0 warnings, 0 errors.
- Coverage check over `_bmad-output/initiative-attendta/demo-walkthrough-evidence.md`: every `FR-1`..`FR-36` and each of AUTH-01–03, MD-01–05, EVT-01–04, REG-01–05, QR-01, ATT-01–03, CERT-01–02, REP-01, ADM-01–02 appears — expected: no missing id, no duplicate result row.
- `git diff --stat` — expected: only the new record and this plan file.
- `git diff --check` — expected: clean.

**Manual checks (if no CLI):**
- Open the record: every journey row has the role, the action, the observed outcome and a result; every BLOCKED row names its missing input.

## Auto Run Result

**Implemented change.** One new evidence record, `_bmad-output/initiative-attendta/demo-walkthrough-evidence.md`,
executed 2026-11-11 on `84b3f54` against a freshly seeded disposable `AttendTaMVP_Test` and a
Production build on `http://localhost:5080`. It carries 68 journey rows (J1–J68) forming one
continuous request-to-certificate/report lifecycle, the FR-1..FR-36 table, the 26-functional-ID
table, 7 blocked rows, three stale-demo-script notes, the no-workaround statement, a zero-defect
table and a blank human sign-off block.

**Files changed.**
- `_bmad-output/initiative-attendta/demo-walkthrough-evidence.md` — new execution record and FR/ID mapping.
- `_bmad-output/initiative-attendta/tickets/epic-release-acceptance/tickets/story-full-functional-demo-script-walkthrough-across-all-26-featur-plan.md` — this plan (status, tasks, implementation notes, triage log).

**Review findings breakdown.** 9 findings: 0 high, 4 medium, 5 low, 0 false, 0 maybe-false.
9 patched, 0 deferred, 0 rejected. Patched: FR-12 downgraded to REUSED; FR-19/REG-05 start-cutoff
clause marked REUSED; ATT-03 given one composite result; the SQL `qr_token` read and the stale
`demo-script.md` §5 expectation recorded; the M-ADM-01 mixed-race blocked row added and cited;
J20 named the submitted academic values; FR-9's draft-edit part marked REUSED; AUTH-02/ADM-02
cited their automated evidence; the legend gained a Composite row.

**Follow-up review recommendation: true.** Patched counts by verdict: 0 high, 4 medium, 5 low. The
unverified risk: every PASS in the record is the executing agent's testimony about a live run that
a reviewer cannot re-observe, so the record's accuracy rests on the database state it left behind
and the cited automated suites, not on an independent replay.

**Verification performed.**
- `dotnet build ASI.Basecode.sln --configuration Release` — 0 warnings, 0 errors.
- Coverage check over the record — 36 FR rows, 26 functional-ID rows, no missing or duplicate ids.
- `git diff --cached --check` — clean; `git diff --cached --stat` — only the record and this plan.
- Database cross-check of the journey's end state: 34 `activity_logs` rows, 28 `account_requests`
  (27 pending + the approved journey request), 2 certificate rows (one issued, one declined),
  11 `users` and 16 `registrations` — consistent with J58, J66–J68 and the journey.

**Residual risks.** ATT-03 camera scanning, OPS-01, OPS-02, M-AUD-01, M-AUTH-01, the M-ADM-01
mixed race and the CERT-03 signatory stay BLOCKED on host, phone, time and human inputs; the
ticket cannot close until the human evaluator signs the record.
