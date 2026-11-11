# AttendTa full functional demo walkthrough — request to certificate/report

Ticket 8.7 / #80. Executed 2026-11-11 as **one continuous journey** on a freshly seeded
disposable database, from the anonymous membership request through approval, sign-in, master
data, event creation and publication, registration, review, pass, check-in, invalidation,
completion, certificate review, issuance, PDF download, participation report, access changes and
the activity log. This file is the **execution record** for SM-2 / R2: every FR-1..FR-36 and every
one of the 26 functional IDs appears exactly once with a result.

It is not a replacement for the evidence it cites: `manual-acceptance-checklist.md` (8.6) owns the
per-case manual results, `execution-manifest-fa9ae82.md` (8.4) owns the combined automated run, and
`demo-script.md` (8.15) owns the presenter beats. This file records one connected lifecycle and the
FR-to-step mapping that none of those files carries.

The evaluator decision at the end is deliberately blank. The record is complete when every row
carries a result; the ticket is done only when the human evaluator signs it.

## 1. Record header

| Field | Value |
|---|---|
| Ticket | 8.7 / #80 — Full functional demo script walkthrough across all 26 feature IDs |
| Tested commit | `84b3f54e58d127f993d55c45a9287dc9b267dda5` (`dev` tip, PR #224 merge) |
| Build | `dotnet build ASI.Basecode.sln --configuration Release` — 0 warnings, 0 errors |
| CSS build | `npm run css:build` in `ASI.Basecode.WebApp` — Tailwind 4.3.4, completed |
| Date | 2026-11-11 |
| Operator | AI agent (subagent), executing under the ticket plan; human evaluator sign-off pending |
| OS | Windows 11 Pro, build 10.0.26200 |
| Runtime | .NET SDK 10.0.403, Node 24.12.0 |
| Database | SQL Server LocalDB `MSSQLLocalDB`, disposable `AttendTaMVP_Test`, reset from `database/reset-local-mvp-test-db.sql` and seeded with `attendta-mvp-schema.sql`, `seed-local-admin.sql`, `seed-event-types.sql`, `seed-demo-scenario.sql` |
| App process | `dotnet run --project ASI.Basecode.WebApp --no-build --configuration Release`, `ASPNETCORE_ENVIRONMENT=Production`, `http://localhost:5080` |
| Private store | Throw-away `Common__SetupRoot` outside `wwwroot` and the deployment tree, with `keys`, `logs`, `temporary`, `uploads/venues`, `uploads/events`, `certificates` |
| Driver | Node 24 HTTP client with a real per-role cookie jar and the real antiforgery tokens: every state change is a real MVC form POST (urlencoded or multipart), and every read is a real GET. No browser automation, no fixtures, no direct SQL writes. Rendered-UI, keyboard and 390px evidence is 8.6's UI-01/UI-02 and is cited, not repeated. |
| Accounts | The three seeded accounts from `demo-script.md` §2.5 plus the member created by the journey, all with locally generated Argon2id hashes. No password, hash, cookie, QR token or private path appears in this file. |
| Automated evidence reused | `execution-manifest-fa9ae82.md` (8.4: 321 service tests, 479 integration tests, 0 failed, 0 skipped, commit `fa9ae82`) |
| Manual evidence reused | `manual-acceptance-checklist.md` (8.6: UI-01, UI-02, CERT-03, M-ADM-01, M-ATT-02, M-CERT-02a/b, M-MD-01, and the five BLOCKED rows) |
| Walkthrough evidence reused | `story-final-frontend-walkthrough-targeted-refactor-and-impeccable.md` §"Walkthrough evidence — Session G" and §"Keyboard/dialog evidence" (8.12) |

The 8.4 manifest was produced on `fa9ae82`. The tested commit here is `84b3f54`, which differs
only by documentation and evidence commits (the 8.6 record and the PR #224 merge). The manifest's
own rule is that documentation and evidence-only commits do not invalidate it, so it is cited
unchanged and no suite was re-run for this ticket. The manifest records .NET SDK 10.0.403, the
same patch level used here.

## 2. Result legend

| Result | Meaning |
|---|---|
| **PASS** | Executed on the tested commit and environment above; the evidence field names the observation. |
| **FAIL** | Executed; the observed result contradicts the expected outcome. The row names the defect. |
| **BLOCKED** | Not executable here. The row names the exact missing input and the evidence file that owns it. A closed issue, a merged tool or a planning document is never substituted. |
| **REUSED** | Covered by the cited automated suite or walkthrough; not re-run for this ticket. |
| **Composite** | A value such as `PASS (pass-code path)` or `PASS (history) / BLOCKED (camera)` states the part demonstrated here first, then the part that is REUSED or BLOCKED, with the citation or the §7 row naming the remainder. |

The journey rows are **demonstrations on one integrated build**, not concurrency proofs. The
automated same-kind races in `acceptance-coverage-matrix.md` §5 and §7 own the proof; this record
does not claim to reproduce a sub-second interleaving by hand.

## 3. Provisioning

1. `sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -i database/reset-local-mvp-test-db.sql` — dropped and
   recreated only the disposable `AttendTaMVP_Test`.
2. `sqlcmd ... -d AttendTaMVP_Test -i database/attendta-mvp-schema.sql` — canonical schema applied.
3. Three Argon2id hashes generated with the built app's `--hash-password` mode; only the PHC hash
   reaches stdout. The plaintext secrets and the hashes live outside the repository and never
   appear here.
4. `seed-local-admin.sql`, `seed-event-types.sql`, `seed-demo-scenario.sql` run from one wrapper
   `.sql` file outside the repository, so PowerShell never splits the hashes.
5. Venue photos and event covers copied into the private setup root under the names the seed uses.
6. App started in `Production` on `http://localhost:5080`; `/Account/Login` returned 200 before the
   journey began.

Seeded starting state confirmed before the journey: 1 Administrator, 1 Officer, 8 member accounts,
6 event types, 2 venues, 3 events (Leadership Summit 2026 started and published; Community Outreach
Drive cancelled; Founders' Week Assembly upcoming and published), 8 confirmed + 3 cancelled + 3
confirmed + 1 pending registrations, and 1 pending account request for Camille Torres.

## 4. The journey

Roles: **Anon** = anonymous public form, **Admin** = Maria Clara Dizon, **Officer** = Rafael Santos,
**Member** = Ava Mendoza (created by the journey), **Liza** = the seeded member who holds the
issued certificate.

| # | Beat | Role | Action | Observed outcome | Result |
|---|---|---|---|---|---|
| J1 | FR-1 | Anon | `GET /AccountRequest/Request`, then `POST` the form | Form exposes StudentNumber, FullName, Email, Course, YearLevel, Section, MobileNumber, Password, ConfirmPassword and **no role field**; POST → 302 `/AccountRequest/Submitted`; acknowledgment reads "Your membership request is pending review."; the stored request is status 1 with an Argon2id hash | PASS |
| J2 | A2 / FR-2 | Admin | `GET /AccountRequest` | Queue shows 2 pending (Camille Torres, Ava Mendoza); the approve dialog summary reads "Approve Ava Mendoza (UC-2026-0111)?"; the approve form posts to `/AccountRequest/Approve` | PASS |
| J3 | A2 / FR-2 | Admin | Approve Ava's request | 302 to the queue with "Account request approved."; the request's hash is cleared; a new account (role 3, active) and its member profile (UC-2026-0111, BS Information Technology, year 2, section B) exist and the account hash is Argon2id | PASS |
| J4 | FR-3 | Member | Sign in with the email and **the password typed on the public form** | 302 `/Home/Index`; the session is issued | PASS |
| J5 | A1 | Admin | `GET /Home/Index` | "Welcome back, Maria Clara Dizon"; the "Run the organization" and "Records and requests" cards, including Accounts and Activity log | PASS |
| J6 | A3 | Admin | `GET /Member?search=Ava` | Ava Mendoza, UC-2026-0111, BS Information Technology, year 2, section B, mobile 0917-000-0111, Active, with Edit and Deactivate actions | PASS |
| J7 | A4 / FR-6 | Admin | `GET /EventType/Create`, then `POST` | Dialog loads Type name and Description with Cancel/Save; save → "Event type added successfully."; "Workshop" is listed and selectable | PASS |
| J8 | A4 / FR-7 | Admin | `GET /Venue/Create`, then `POST` a real JPEG | Dialog loads Room name, Capacity, Location, Availability notes, Contact person and a photo input accepting `image/png,image/jpeg`; save → "Venue added successfully."; `GET /Venue/Photo?id=3` returns 200 `image/jpeg`, 170,275 bytes | PASS |
| J9 | A4 / FR-5 | Admin | `GET /Member/Edit/{id}`, then `POST` | Editor loads full name, student number, course, year level (1–5), section, mobile, membership type; save → "Member updated successfully."; the mobile number changed to 0917-000-0112 | PASS |
| J10 | A5 / FR-9 | Admin | `GET /Event/Create`, then `POST` a draft | The new Workshop type and Innovation Lab venue are selectable; save → "Draft event created."; the event is status 1 (draft) with no cover | PASS |
| J11 | A5 / FR-8 | Admin | `POST /Event/UploadCover?id=4` with a real JPEG | "Event cover updated."; `image_path` becomes an area-prefixed `uploads/events/<guid>.jpg` | PASS |
| J12 | A5 / FR-10 | Admin | `POST /Event/Publish` | "Event published."; status 2 (published); `published_by` is the real administrator id | PASS |
| J13 | M1 | Member | `GET /Home/Index` | "Welcome back, Ava Mendoza"; the member cards Browse events, My Passes, My Events, Profile; no Accounts or Activity log | PASS |
| J14 | M2 | Member | `GET /Events` | Three events listed (Leadership Summit 2026, Founders' Week Assembly, Peer Tutoring Session) with four images and Signup open / Signup closed badges; the cancelled Community Outreach Drive is hidden from a member it does not affect | PASS |
| J15 | M3 | Liza | `GET /MemberEvents/Details?id=3`, `?id=1`, `?id=2` | `?id=3` shows the Confirmed panel with "Cancel my registration"; `?id=1` shows the confirmed state with signup closed; `?id=2` returns 404 — see §8 note 1 | PASS (with note) |
| J16 | M3 | Member | `GET /MemberEvents/Details?id=4` | Signup open and a "Submit registration" control | PASS |
| J17 | FR-15 | Member | `POST /MemberEvents/Register?id=1` (deadline passed) | 302 to `/Events` with "Signup for this event is closed."; the registration count for that member and event is 0 before and after | PASS |
| J18 | M4 / FR-14 | Member | `POST /MemberEvents/Register?id=4` | "Registration submitted. Your request is pending review."; the detail panel changes to Pending review; the row is status 1 with the course/year/section snapshot BS Information Technology / 2 / B and no QR token yet | PASS |
| J19 | M5 | Member | `GET /MyEvents/Registrations` | Lists Peer Tutoring Session as Pending review with View event | PASS |
| J20 | M9 / FR-4 | Member | `GET /Profile` | Identity strip, Active badge, the academic record (course BS Information Technology, year level 2, section B — the values submitted on the public form and recorded in J6), the membership details, mobile 0917-000-0112 and View my passes | PASS |
| J21 | FR-18 | Liza | `POST /MemberEvents/Cancel?id=3` | "Your registration was cancelled. This cannot be undone."; the row is status 4 and its QR token is cleared | PASS |
| J22 | FR-14 | Liza | `POST /MemberEvents/Register?id=3` again | "You already have a registration for this event." — no reapplication | PASS |
| J23 | O1 | Officer | `GET /Home/Index` | "Welcome back, Rafael Santos"; Check-in, Registration review, Certificates, Reports cards; no Accounts or Activity log | PASS |
| J24 | O2 | Officer | `GET /Event/1`, `/Event/2`, `/Event/3` | All 200 with metadata, capacity and a status-aware roster; event 1 and 3 offer Complete event and the check-in control; the cancelled event 2 offers no completion control | PASS |
| J25 | O3 | Officer | `POST /Event/Complete` on event 3 (before start) | "An event cannot be completed before it starts."; status stays 2 | PASS |
| J26 | O3 | Officer | `POST /Event/Complete` on event 2 (cancelled) | "Only a published event can be completed."; status stays 3 | PASS |
| J27 | O4 | Officer | `GET /RegistrationReview/Index` | Daniel Navarro and Ava Mendoza are pending; each row offers Approve and a Decline form with a required reason | PASS |
| J28 | O4 / FR-16 | Officer | Approve Ava's registration | 302 with "Registration updated."; the row is status 2 (confirmed) and a canonical 64-hex QR token now exists | PASS |
| J29 | O4 / FR-16 | Officer | Decline Daniel's registration with a reason | 302 with "Registration updated."; the row is status 4 (cancelled) and the reason is stored | PASS |
| J30 | M8 / FR-21 | Member | `GET /Passes` | The Peer Tutoring Session pass card shows the confirmed seat, the QR image and "Download pass"; the cancelled registration has no pass | PASS |
| J31 | M8 / FR-21 | Member | `GET /Passes/Qr/16` and `?download=true` | 200 `image/png`, 496 bytes; download is `attachment; filename=door-pass-16.png` | PASS |
| J32 | FR-22 | Officer | `POST /Event/OpenCheckIn` on event 4 | "Check-in opened."; `is_check_in_open = 1` | PASS |
| J33 | O5 | Officer | `GET /CheckIn?eventId=4` | The scanner offers Camera scan, Pass code and Manual lookup; the pass-code input is present | PASS |
| J34 | O5 / FR-23 | Officer | `POST /CheckIn/Scan` with that pass's token | "Ava Mendoza checked in."; the outcome panel shows the participant and "Check-in recorded"; exactly one attendance row exists | PASS |
| J35 | O5 / FR-23 | Officer | Submit the same token again | "Ava Mendoza was already checked in at 2026-11-11 20:16 Asia/Manila." — the original time; still exactly one attendance row | PASS |
| J36 | O6 / FR-24 | Officer | `GET /CheckIn?eventId=4&q=Ava` | Ava is found with "Checked in"; the recorded check-ins table shows the row; searching alone added no attendance row | PASS |
| J37 | M6a / FR-26 | Member | `GET /MyEvents/Attendance` | Peer Tutoring Session, 2026-11-11 20:16 Asia/Manila, method QR code, Valid | PASS |
| J38 | O7 / FR-25 | Officer | `POST /CheckIn/Invalidate` with a reason | "Attendance invalidated."; the recorded row shows INVALIDATED with the reason; the attendance row is kept | PASS |
| J39 | O7 / FR-25 | Officer | Re-scan the same token | "This attendance was invalidated and cannot be checked in again."; no second row | PASS |
| J40 | M6b / FR-26 | Member | `GET /MyEvents/Attendance` again | The record is marked Invalidated with the correction reason | PASS |
| J41 | FR-22 / FR-24 | Officer | Open check-in on event 1, then manual check-in Liza and Mark | "Check-in opened."; "Liza Fernandez checked in." and "Mark Villanueva checked in."; each records one attendance row with method 2 (manual search) | PASS |
| J42 | O3 / FR-13 | Officer | `POST /Event/Complete` on event 1 | "Event completed."; status 4; check-in closed; exactly 2 certificate reviews created for the 2 valid attendees; the roster is read-only | PASS |
| J43 | O8 | Officer | `GET /Certificates?eventId=1` | Liza Fernandez and Mark Villanueva both show Pending review / Not issued with an Issue control | PASS |
| J44 | O8 / FR-27 | Officer | Approve Liza's review | "Certificate eligibility approved."; the review is status 2 (approved, not issued) | PASS |
| J45 | O8 / FR-27 | Officer | Decline Mark's review with a reason | "Certificate eligibility declined."; status 3; the row shows "Decision final" and the reason is stored | PASS |
| J46 | O9 / FR-28 | Officer | Issue Liza's certificate | "Certificate issued. View it in the Issued Registry."; `AT-2026-000001`; status 4; the retained file is `certificates/<guid>.pdf`; snapshots are Liza Fernandez / Leadership Summit 2026 / the event start / Rafael Santos | PASS |
| J47 | O9 / FR-29 | Officer | Issued Registry, Viewer, PDF and download | The registry lists `AT-2026-000001` with View PDF; `/Certificates/Viewer/1` returns 200 and frames `/Certificates/Pdf/1`; the PDF is 200 `application/pdf`, 422,591 bytes, `Cache-Control: no-store,no-cache`; the download is `attachment; filename=AT-2026-000001.pdf` and byte-identical | PASS |
| J48 | M7 / FR-29 | Liza | `GET /MyEvents/Certificates` and the member PDF route | Issued Leadership Summit 2026 certificate with `AT-2026-000001`, View PDF and Download PDF; the member PDF is 200 `application/pdf`, 422,591 bytes, with the same attachment filename | PASS |
| J49 | M3 | Liza | `GET /MemberEvents/Details?id=1` after completion | The event shows the Completed state and no cancellation control | PASS |
| J50 | O10 / FR-30 | Officer | `GET /Reports/Participation?eventId=1` | Confirmed 8, Valid attendees 2, Attendance rate 25%, No-shows 6; the detail rows name each participant, status, attendance and check-in time | PASS |
| J51 | O10 / FR-30 | Officer | `GET /Reports/Participation?eventId=2` | Confirmed 0, Valid attendees 0, Attendance rate N/A, No-shows "—"; the 3 cancelled registrations are listed and excluded from the rate | PASS |
| J52 | A6 / FR-32 | Admin | `GET /admin/accounts` and the account access dialog | Ava's row is listed; the preview names the account and lists the affected upcoming Peer Tutoring Session registration; the reason is required | PASS |
| J53 | A6 / FR-32 | Admin | Deactivate Ava's account with a reason | "Account deactivated."; `is_active = 0`; the upcoming confirmed registration is cancelled (status 4) and its QR token is cleared | PASS |
| J54 | A6 / FR-32 | Member | Reuse the existing session, then sign in again | The next request 302s to `/Account/Login?ReturnUrl=%2FMyEvents%2FCertificates`; a fresh sign-in returns the generic "Incorrect email or password." | PASS |
| J55 | A6 / FR-32 | Admin | Reactivate Ava's account | "Account reactivated."; `is_active = 1`; the member signs in again (302 `/Home/Index`); the cancelled registration is **not** restored | PASS |
| J56 | A6 / FR-32 | Admin | Attempt to deactivate the only administrator | "The last active Administrator cannot be deactivated."; the administrator stays active | PASS |
| J57 | A6 / FR-5 | Admin | Deactivate and reactivate Liza's membership | "Membership deactivated." → membership status 2; the issued certificate `AT-2026-000001` is preserved; "Membership reactivated." → membership status 1 | PASS |
| J58 | A7 / FR-33, FR-34 | Admin | `GET /admin/auditlogs` | Newest first, 34 rows, "Page 1 of 2"; page 1 and page 2 together show `account_request.approved`, `member.created`, `event.published`, `event.check_in_opened`, `event.completed`, `attendance.recorded`, `attendance.invalidated`, `registration.approved`, `registration.declined`, `certificate.approved`, `certificate.declined`, `certificate.issued`, `user.deactivated`, `user.reactivated`, `member.membership_deactivated` and `member.membership_reactivated` | PASS |
| J59 | FR-31 | Admin | Change a seeded member's role to Officer, then back to Member | "Role updated successfully." both times; `role_id` 2 then 3; the account list reflects the new role | PASS |
| J60 | FR-11 | Admin | `GET /Event/Edit?id=4`, then `POST` a new description | "Event updated."; the event stays published; the description changed; event type, venue, start, deadline and limit travel as locked hidden values | PASS |
| J61 | FR-6 | Admin | `POST /EventType/ToggleActive` twice | "Event type status updated."; `is_active` 0 then 1 | PASS |
| J62 | FR-8 | Admin | `POST /Venue/Edit` with a replacement JPEG | "Venue updated successfully."; the served photo changed from 170,275 to 179,839 bytes and the stored path is a new area-prefixed file | PASS |
| J63 | FR-19 | Officer | Administratively cancel a confirmed registration on the completed event | Refused with "Event status changed. Refresh and try again."; the registration stays confirmed | PASS (refusal observed; exact start boundary REUSED) |
| J64 | FR-34 | Officer | `GET /admin/auditlogs` | 302 to `/Account/AccessDenied?ReturnUrl=%2Fadmin%2Fauditlogs` | PASS |
| J65 | FR-3 | Member | `POST /Account/Logout`, then `GET /Home/Index` | 302 to `/`; the next request 302s to `/Account/Login?ReturnUrl=%2FHome%2FIndex`; the `AttendTa.Auth` cookie is gone | PASS |
| J66 | FR-35 | Operator | Re-run `seed-local-admin.sql` on the seeded database, then run it with no variables | 1 active Administrator before and after; exactly 1 `user.provisioned` audit row before and after; the stored hash is Argon2id and no plaintext is stored; with no variables sqlcmd refuses with "'AdminName' scripting variable not defined." and exit 1 | PASS |
| J67 | FR-36 | Anon | Submit 26 real requests through the public form | 26 × 302 to the acknowledgment page; pending requests rise to 27 | PASS |
| J68 | FR-36 | Admin | Page, filter and empty-state the request queue | Page 1 shows 25 rows, "Page 1 of 2", Next only; page 2 shows 2 rows, "Page 2 of 2", Previous only; `page=0` shows page 1; `page=99` shows page 2; `search=zzzzz` shows "No pending account requests found." with no pager; `/Venue` shows 3 rows with no pager | PASS |

## 5. FR-1..FR-36

| FR | Requirement | Journey step / reused evidence | Result |
|---|---|---|---|
| FR-1 | Submit account request | J1 | PASS |
| FR-2 | Review account requests | J2, J3 | PASS |
| FR-3 | Log in and log out | J4, J54, J65 | PASS |
| FR-4 | View own profile | J20 | PASS |
| FR-5 | Maintain member profiles | J9, J57 | PASS |
| FR-6 | Maintain event types | J7, J61 | PASS |
| FR-7 | Maintain venues | J8 | PASS |
| FR-8 | Upload and replace record images | J11, J62; invalid/oversized content REUSED — `VenuePhotoFlowTests`, `EventCoverFlowTests`, `PrivateFileStoreTests` (8.4) | PASS |
| FR-9 | Create and edit draft events | J10 (draft create); the draft-edit part REUSED — `EventDraftFlowTests` (8.4) | PASS (create) / REUSED (draft edit) |
| FR-10 | Publish event | J12 | PASS |
| FR-11 | Edit published event | J60 | PASS |
| FR-12 | Cancel event | REUSED — `EventCancellationFlowTests` (8.4). Supporting observations only: J26 (completion refused on an already-cancelled event) and §8 note 1 (the affected member sees "Cancelled" in My Events); the journey did not cancel an event itself | REUSED |
| FR-13 | Complete event | J42 | PASS |
| FR-14 | Browse and register | J18, J22 | PASS |
| FR-15 | Enforce signup deadline | J17 | PASS |
| FR-16 | Review and administratively cancel registrations | J28, J29 | PASS |
| FR-17 | Promote from waitlist | REUSED — `RegistrationPromotionTests` (8.4); the journey contained no full event, so no waitlist existed to promote | REUSED |
| FR-18 | Member cancels own registration | J21 | PASS |
| FR-19 | Enforce event start cutoff | The start-cutoff clause is REUSED — `MemberEventDeadlineFlowTests`, `EventCompletionFlowTests` (8.4); the journey never observed "This event has already started." because `RegistrationReviewService` checks `EventStatusId != Published` first. J63 is the post-completion refusal only, with the confirmed row unchanged | REUSED (start cutoff) / PASS (post-completion refusal) |
| FR-20 | My Events | J19, J37, J48 | PASS |
| FR-21 | View and download QR pass | J30, J31 | PASS |
| FR-22 | Control check-in window | J32, J42 | PASS |
| FR-23 | Check in by QR scan | J33–J35 (pass-code path); camera scanning is **BLOCKED** — see §7 ATT-03 | PASS (pass-code path) |
| FR-24 | Manual check-in fallback | J36, J41 | PASS |
| FR-25 | Invalidate attendance record | J38, J39 | PASS |
| FR-26 | View own attendance | J37, J40 | PASS |
| FR-27 | Review certificate eligibility | J44, J45 | PASS |
| FR-28 | Issue certificate | J46 | PASS |
| FR-29 | Download certificate | J47, J48 | PASS |
| FR-30 | View event participation report | J50, J51 | PASS |
| FR-31 | Manage roles | J59 | PASS |
| FR-32 | Deactivate and reactivate access | J52–J57 | PASS |
| FR-33 | Record activity log | J58 | PASS |
| FR-34 | View activity log | J58, J64 | PASS |
| FR-35 | Initial administrator setup | J66 | PASS |
| FR-36 | Fixed pagination | J67, J68 | PASS |

## 6. The 26 functional IDs

| ID | Demonstrated by | Result |
|---|---|---|
| AUTH-01 | J1 (public request), J2–J3 (reviewed approval) | PASS |
| AUTH-02 | J2 (review), J54 (generic refusal), J64 (wrong-role refusal); anonymous refusal, another member's data and antiforgery variants REUSED — `AuthenticationFlowTests`, `AccountRequestDecisionTests`, `AdminAccountsListTests` (8.4) | PASS |
| AUTH-03 | J4 (real email sign-in), J54 (inactive refusal), J65 (logout) | PASS |
| MD-01 | J20 (own read-only profile) | PASS |
| MD-02 | J9 (permitted member correction) | PASS |
| MD-03 | J7, J61 (type create and toggle) | PASS |
| MD-04 | J8 (venue create with required photo) | PASS |
| MD-05 | J11, J62 (cover upload, photo replacement) | PASS |
| EVT-01 | J10, J12 (draft then publish) | PASS |
| EVT-02 | J17 (deadline rejection) | PASS |
| EVT-03 | J60 (published edit with locked fields) | PASS |
| EVT-04 | J25, J26 (completion refusals), J42 (completion) | PASS |
| REG-01 | J18 (pending signup with snapshot) | PASS |
| REG-02 | J28, J29 (approve and decline with reason) | PASS |
| REG-03 | J21 (self-cancellation) | PASS |
| REG-04 | REUSED — `RegistrationPromotionTests` (8.4) | REUSED |
| REG-05 | J17 (deadline rejection); the start-cutoff clause is REUSED — `MemberEventDeadlineFlowTests`, `EventCompletionFlowTests` (8.4); J63 is the post-completion refusal only | REUSED (start cutoff) / PASS (deadline + post-completion refusal) |
| QR-01 | J30, J31 (owner-authorized token-only PNG) | PASS |
| ATT-01 | J32–J36 (window control, scan, duplicate, manual) | PASS |
| ATT-02 | J38, J39 (invalidation and blocked re-check-in) | PASS |
| ATT-03 | J37, J40 (owner-scoped history); camera scanning **BLOCKED** — see §7 ATT-03 | PASS (history) / BLOCKED (camera) |
| CERT-01 | J44, J45 (approve and final decline) | PASS |
| CERT-02 | J46–J48 (one-at-a-time issuance, retained PDF, authorized download) | PASS |
| REP-01 | J50, J51 (selected-event totals, rate, no-shows, N/A) | PASS |
| ADM-01 | J52–J57, J59 (roles, access and membership cascades, last-admin safety); attended-data survival REUSED — `manual-acceptance-checklist.md` §5 M-ADM-01 (8.6) | PASS |
| ADM-02 | J58, J64 (administrator-only newest-first viewer); append-only storage and audit atomicity REUSED — `TransactionAuditTests` (8.4) | PASS |

## 7. Blocked rows

These are host, phone and human inputs that no local run can supply. Each row names the missing
input and the evidence file that owns it. No PASS is inferred for the blocked part.

| Case | Blocked part | Missing input | Owning evidence |
|---|---|---|---|
| ATT-03 | Camera scan on the real hotspot HTTPS origin | A real demo host with the hotspot HTTPS binding, its generated root CA installed on a real officer phone, and a phone on the hotspot subnet | `manual-acceptance-checklist.md` §5 ATT-03 (BLOCKED); `docs/hotspot-https-evidence.md` |
| OPS-01 | Demo-host readiness under the runtime identity | A provisioned demo host, a dedicated Windows service identity and a dedicated SQL login | `manual-acceptance-checklist.md` §5 OPS-01 (BLOCKED); `docs/demo-host-readiness-evidence.md` |
| OPS-02 | Backup, restart/redeploy and restore to a separate target | A real host, an approved backup destination, an approved restore target, a maintenance window and an approver | `manual-acceptance-checklist.md` §5 OPS-02 (BLOCKED); `docs/storage-survival-evidence.md` |
| M-AUD-01 | Column-level `GRANT`/`DENY` on `activity_logs` under the runtime SQL identity | A provisioned runtime SQL login on a demo host | `manual-acceptance-checklist.md` §5 M-AUD-01 (BLOCKED) |
| M-ADM-01 (mixed race) | The demote-vs-deactivate race between the last two administrators | Two live sessions producing the required sub-second interleaving; the two same-kind races are automated, the mixed pair is not reproducible by hand | `manual-acceptance-checklist.md` §5 M-ADM-01 (mixed race BLOCKED; the sequential attended-data survival there is PASS) |
| M-AUTH-01 | The 8-hour, non-sliding idle expiry | An 8-hour idle observation on a real browser session | `manual-acceptance-checklist.md` §5 M-AUTH-01 (BLOCKED; cookie flags recorded there) |
| CERT-03 signatory | Human approval of the certificate signatory name, title and signature | The human's confirmation of the approved signatory | `manual-acceptance-checklist.md` §5 CERT-03 (signatory sub-field BLOCKED) |

The rendered-UI, keyboard, focus and 390px checks are **not** re-run here: 8.6's UI-01 and UI-02
own them and are cited unchanged. This record adds the continuous journey and the FR mapping, not a
second rendering audit.

## 8. Notes and discrepancies

1. **`GET /MemberEvents/Details?id=2` returns 404.** A cancelled event has no member detail page, even
   for an affected member. FR-12's visibility requirement is still met: the affected member's
   `GET /MyEvents/Registrations` lists "Community Outreach Drive — Cancelled". `demo-script.md` M3's
   "`?id=2` shows the cancelled state" is stale for the detail route. Recorded as a documentation
   note, not a product defect.
2. **Seeded confirmed registrations carry no QR token.** `seed-demo-scenario.sql` inserts confirmed
   rows without `qr_token`, so the seeded Leadership Summit and Founders' Week passes do not render;
   the pass service correctly treats a missing token as no pass. Passes exist only for registrations
   confirmed through the app, which is why M8/J30–J31 use the journey's own registration.
   `demo-script.md` M8's "Active Founders' Week and Peer Tutoring cards" is stale for the seeded
   rows. Recorded as a demo-data note, not a product defect.
3. **`demo-script.md` §5's "the token is read from the pass page" is stale.**
   `Views/MyEvents/_PassPartial.cshtml` renders only the QR image and the download link, so the pass
   page never exposes the token. The journey read the registration's own `qr_token` read-only from
   the registration row to feed the real scanner endpoint, and the value is not reproduced here.
   Recorded as a documentation note, not a product defect.
4. **The certificate leg runs on the seeded started event.** Publication requires a signup deadline
   in the future and completion requires a started event, so a single newly published event cannot
   reach issuance in one sitting. The journey therefore creates and publishes Peer Tutoring Session
   for the registration, pass and check-in legs, and completes the seeded Leadership Summit for the
   certificate and report legs. This is the plan's recorded decision, not a workaround.
5. **Two check-in methods were exercised.** The pass-code scan (FR-23) on the new event and the
   manual lookup (FR-24) on the started event. The camera path is ATT-03 and stays BLOCKED.
6. **The attendance rate reflects the journey.** Leadership Summit reports 2 valid attendees of 8
   confirmed (25%) because the journey checked in Liza and Mark before completing it. The demo
   script's "0 valid, 0%" figure describes the untouched seed.

## 9. No-workaround statement

Every state change in this record was produced by a real MVC form POST against the running
Production build: the public request, the approval, every editor save, the cover and photo uploads,
publish, registration, review, check-in window, scan, manual check-in, invalidation, completion,
certificate decision, issuance, role change, deactivation and reactivation. No fixture, no seeded
or mocked substitute for a service call, no browser-storage workaround, no direct SQL state edit and
no fake issued outcome was used. SQL was read-only: it supplied the pass's own `qr_token` from the
registration row so the real scanner endpoint could be exercised, and it verified the resulting rows.
The token is not reproduced here. The only SQL that wrote anything was the ticket's own provisioning
and the FR-35 re-run of the administrator seed, both of which are part of the evidence. No
destructive script was run against `AttendTa_Demo`, `AsiBasecodeDB` or any shared database.

## 10. Defects

No product defect was found on `84b3f54`. Three documentation/data-state discrepancies are recorded
in §8 as notes, not defects.

| Id | Row | Observation | Severity | Filed |
|---|---|---|---|---|
| — | — | No product defect found during this run | — | — |

## 11. Sign-off block

| Field | Value |
|---|---|
| Record complete | Yes — every FR-1..FR-36 and every one of the 26 functional IDs appears exactly once in §5/§6 with a result, and every blocked part is listed in §7 with its missing input |
| Operator | AI agent (subagent), 2026-11-11 |
| Human evaluator | — (blank, to be filled by the human) |
| Evaluator decision | — (blank: PASS / FAIL / retest) |
| Date | — |
| Affected retests | — |
