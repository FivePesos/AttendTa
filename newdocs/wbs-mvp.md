# AttendTa — Vertical-Slice Delivery WBS

Markdown companion to `wbs-mvp.pdf`; generated from the same content. Each description is one continuous paragraph without PDF layout line breaks. Copy a rendered description, or enter cell-edit mode in Excel before pasting an individual description. Raw Markdown table rows contain pipe separators, not Excel tab separators.

**Predecessors:** module-based task references from Depends On / Handoff. These are integration prerequisites, not a fully estimated finish-to-start schedule. Non-task setup and service contracts remain in the handoff notes; “OR” is an alternative. Agree contracts early to permit parallel work. Task actors identify application access; owners identify development assignments. Story IDs remain unchanged in the separate user-story document.

### Five owner packages; six functional headings

Revised MVP v2.1 | User profile pictures removed; other agreed scope unchanged. Original submission deadline: October 7, 2026 (historical). Owners are assignment placeholders. Estimates remain TBD. Password changes remain deferred without a reserved WBS number.

Task labels use module code — application actor: task, numbered consecutively within each prefix in displayed order. Owner identifies the teammate building it, not access permission. Member/participant actions are also available to officers/admins with eligible member profiles under the same ownership rules. System means server enforcement, not a new privileged login. Pagination is included in necessary lists, with no separate SHR module/task.

| OWNER | PACKAGE | END-TO-END RESPONSIBILITY |
| --- | --- | --- |
| Member 1 | Accounts and Administration | Authentication + Administration; authorization/audit infrastructure and admin setup. |
| Member 2 | Membership and Master Data | Members, types, venues/photos; shared event/venue image storage and pagination components. |
| Member 3 | Events and Registration | Events/covers, signup, review, cancellation, capacity, waitlist, cutoffs; My Events shell. |
| Member 4 | Attendance and QR | QR render/download/scan, manual fallback, controls, correction, attendance sections and attendee screen shell. |
| Member 5 | Certificates and Reports | Eligibility, fixed-template PDF issuance/private downloads, participation reports and certificate screen sections. |

Definition of done: every slice includes its database/query changes, service logic, MVC screens, validation/authorization, audit integration and tests. Deliver a demonstrable result, not isolated frontend/backend layers. Members review schema/service contracts together first.

Member 3 carries the highest lifecycle/concurrency risk. Estimate remaining work before assuming equal workload. Member 2 builds shared components; each consumer integrates them. All five own incremental integration and final journey testing. Historical retention and necessary list pagination are included within the functional tasks, not separate modules or estimates.

### Authentication — Member 1

Concise delivery summaries; complete acceptance criteria remain in AttendTa-User-Stories.md and AttendTa-System-Specification.md. Each slice includes database-to-screen behavior and tests. Agree handoff contracts early; estimates include integration without double-counting shared screens.

| TASK / ACTOR | DESCRIPTION / DEMONSTRABLE RESULT | OWNER | PREDECESSORS | DEPENDS ON / HANDOFF | UC MAPPING | EST. MD |
| --- | --- | --- | --- | --- | --- | --- |
| AUTH-01 — Student: Request an account | Submit student identity, academic details and password for approval. Validate duplicate identities, securely hash the password and create a pending request; allow a new account request after rejection. | Member 1 | — (see handoff) | Shared schema; account validation/hash service. | Authentication | TBD |
| AUTH-02 — Officer/Admin: Review account requests | Approve or reject pending account requests and record the decision. Approval atomically creates an active member-role account and linked profile; rejection requires a reason. Clear the finalized request password hash. Use 25-row pagination for list results. | Member 1 | AUTH-01 | AUTH-01; agreed user/member contract with Member 2. | Authentication | TBD |
| AUTH-03 — All Accounts: Log in and log out | Authenticate active accounts using email/password and enforce current role permissions. Use signed, expiring authentication; logout clears the browser cookie and inactive accounts cannot access protected functions. | Member 1 | Setup admin OR AUTH-02 | Setup admin or AUTH-02; role/access helpers. | Authentication | TBD |

### Master Data — Member 2

Concise delivery summaries; complete acceptance criteria remain in AttendTa-User-Stories.md and AttendTa-System-Specification.md. Each slice includes database-to-screen behavior and tests. Agree handoff contracts early; estimates include integration without double-counting shared screens.

| TASK / ACTOR | DESCRIPTION / DEMONSTRABLE RESULT | OWNER | PREDECESSORS | DEPENDS ON / HANDOFF | UC MAPPING | EST. MD |
| --- | --- | --- | --- | --- | --- | --- |
| MD-01 — Member: View my profile | Display the member’s account, academic and membership details. Officers handle profile corrections; profile-picture uploads are not included and password changes remain deferred. | Member 2 | AUTH-03, AUTH-02 | AUTH-03, AUTH-02; linked user/member query. | Master Data | TBD |
| MD-02 — Officer/Admin: Maintain members | Maintain approved member profiles, academic/contact details, membership type and active status. Validate student-number corrections, record reasons and apply future-registration rules when membership becomes inactive without rewriting historical participation. Use 25-row pagination for list results. | Member 2 | AUTH-02, AUTH-03 | AUTH-02, AUTH-03; Member 3 cancellation service for inactivity. | Master Data | TBD |
| MD-03 — Officer/Admin: Manage event types | Create and edit unique event-type names and descriptions. Activate or deactivate types, allow only active types for new selections and preserve historical event references. Use 25-row pagination for list results. | Member 2 | AUTH-03 | AUTH-03; unique names and active-state validation. | Master Data | TBD |
| MD-04 — Officer/Admin: Manage venues | Maintain venue details, capacity, active status and a required photo. Validate capacity changes against linked draft/published events and preserve historical references; required photos may be replaced, not removed without replacement. Use 25-row pagination for list results. | Member 2 | AUTH-03, MD-05 | AUTH-03, MD-05; capacity contract with Member 3. | Master Data | TBD |
| MD-05 — Officer/Admin: Manage event and venue images | Provide officer/admin JPEG/PNG uploads up to 2 MiB for required venue photos and event covers required before publication. Validate permissions/content, use persistent local paths and safely replace files; no user profile pictures. | Member 2 | AUTH-03 | AUTH-03; persistent-storage config; event integration by Member 3. | Master Data; Transactions (covers) | TBD |

### Events and Registration — Member 3

Concise delivery summaries; complete acceptance criteria remain in AttendTa-User-Stories.md and AttendTa-System-Specification.md. Each slice includes database-to-screen behavior and tests. Agree handoff contracts early; estimates include integration without double-counting shared screens.

| TASK / ACTOR | DESCRIPTION / DEMONSTRABLE RESULT | OWNER | PREDECESSORS | DEPENDS ON / HANDOFF | UC MAPPING | EST. MD |
| --- | --- | --- | --- | --- | --- | --- |
| EVT-01 — Officer/Admin: Create and publish events | Create event drafts with schedule, venue, type, capacity, deadline and cover. Validate master records, capacity and dates; publication requires a cover and future deadline, makes the event visible and immediately opens signup. Use 25-row pagination for list results. | Member 3 | MD-03, MD-04, MD-05 | MD-03, MD-04, MD-05; active masters and required cover. | Transactions | TBD |
| EVT-02 — Member; System: Show registration availability | Display signup as open before the registration deadline and closed at or after it, using labelled local time. Reject late submissions server-side without changing the published event status. | Member 3 | EVT-01 | EVT-01; shared server clock/timezone convention. | Transactions | TBD |
| EVT-03 — Officer/Admin: Edit event details | Allow all validated draft edits. After publication, permit only title, description and cover replacement; lock operational details, retain required images and make completed/cancelled events read-only. | Member 3 | EVT-01 | EVT-01; shared upload service and event edit guards. | Transactions | TBD |
| EVT-04 — Officer/Admin: Cancel or complete events | Cancel eligible events with a reason, cancel active registrations and invalidate QR access without promotion. Block cancellation after valid attendance or issuance; explicitly completing a concluded event closes check-in and enables certificate review. | Member 3 | EVT-01, REG-02 | EVT-01, REG-02; attendance and certificate guard contracts (Members 4/5). | Transactions | TBD |
| REG-01 — Member: Browse and register | Browse published events and submit one pending request per member/event before the deadline. Validate eligibility, capture academic snapshots and allow requests for full events with a waitlist warning; no reapplication after cancellation. Use 25-row pagination for list results. | Member 3 | AUTH-02, MD-02, EVT-01, EVT-02 | AUTH-02, MD-02, EVT-01, EVT-02; eligibility and unique member/event pair. | Transactions | TBD |
| REG-02 — Officer/Admin: Review registrations | Approve or decline pending requests before event start, including after signup closes. Confirm only with capacity and waitlist priority respected; otherwise waitlist. Record review details and generate QR credentials only on confirmation. Use 25-row pagination for list results. | Member 3 | REG-01 | REG-01; transactional capacity/queue service; token contract with Member 4. | Transactions | TBD |
| REG-03 — Member: Cancel my registration | Allow members to cancel before event start without valid attendance, even after signup closes. Require confirmation, record the cancellation, invalidate the QR and release any seat; no reapplication or automatic promotion. Use 25-row pagination for list results. | Member 3 | REG-01, REG-02 | REG-01, REG-02; attendance guard from Member 4. | Transactions | TBD |
| REG-04 — Officer/Admin: Promote the next waitlisted participant | Let officers explicitly promote the next eligible reviewed waitlisted participant before event start. Apply review-time/ID priority, recheck capacity transactionally and record confirmation with QR access; never promote automatically. | Member 3 | REG-02, REG-03 | REG-02, REG-03; full-queue transactional allocation. | Transactions | TBD |
| REG-05 — System: Enforce processing cutoffs | Stop new signup at the deadline and reviews, promotions and member cancellations at event start. Retain unresolved pending/waitlisted requests as non-actionable history, exclude them from no-shows and enforce cutoffs server-side. | Member 3 | EVT-02, REG-02, REG-03, REG-04 | EVT-02, REG-02, REG-03, REG-04; server time guards. | Transactions | TBD |

### Attendance and QR Codes — Member 4

Concise delivery summaries; complete acceptance criteria remain in AttendTa-User-Stories.md and AttendTa-System-Specification.md. Each slice includes database-to-screen behavior and tests. Agree handoff contracts early; estimates include integration without double-counting shared screens.

| TASK / ACTOR | DESCRIPTION / DEMONSTRABLE RESULT | OWNER | PREDECESSORS | DEPENDS ON / HANDOFF | UC MAPPING | EST. MD |
| --- | --- | --- | --- | --- | --- | --- |
| QR-01 — Confirmed Participant: Access my event QR | Display or download the confirmed participant’s event-specific QR as an on-demand PNG. Saved copies work offline for members, but officers validate eligibility online; cancelled or unconfirmed registrations have no usable QR. | Member 4 | REG-02, REG-04 | REG-02, REG-04; token contract; My Events shell from Member 3. | Transactions | TBD |
| ATT-01 — Officer/Admin: Control and record check-in | Manually control check-in for published events and record arrivals through QR scanning or explicit manual lookup/check-in. Validate event, confirmation and eligibility; timestamp attendance, prevent duplicates and display participant identity. Use 25-row pagination for list results. | Member 4 | EVT-01, REG-02, QR-01 | EVT-01, REG-02, QR-01; event control and attendance contracts. | Transactions | TBD |
| ATT-02 — Officer/Admin: Correct attendance | Invalidate mistaken attendance with a reason while preserving its history. Exclude it from reports and unissued eligibility; block corrections after certificate issuance and do not permit re-check-in after invalidation. Use 25-row pagination for list results. | Member 4 | ATT-01 | ATT-01; issued-state/eligibility contract with Member 5. | Transactions | TBD |
| ATT-03 — Member: View my attendance | Show personal attendance and check-in details within My Events, clearly identifying corrected records. Restrict access to authorized personal information; members cannot record or invalidate their own attendance. Use 25-row pagination for list results. | Member 4 | ATT-01, ATT-02 | ATT-01, ATT-02; My Events shell from Member 3. | Transactions | TBD |

### Certificates and Reports — Member 5

Concise delivery summaries; complete acceptance criteria remain in AttendTa-User-Stories.md and AttendTa-System-Specification.md. Each slice includes database-to-screen behavior and tests. Agree handoff contracts early; estimates include integration without double-counting shared screens.

| TASK / ACTOR | DESCRIPTION / DEMONSTRABLE RESULT | OWNER | PREDECESSORS | DEPENDS ON / HANDOFF | UC MAPPING | EST. MD |
| --- | --- | --- | --- | --- | --- | --- |
| CERT-01 — Officer/Admin: Review certificate eligibility | Review valid attendees of completed events and approve or decline certificate eligibility with reviewer/time and a decline reason. Recheck attendance and permissions; declined decisions cannot reopen, and issued decisions cannot change. Use 25-row pagination for list results. | Member 5 | EVT-04, ATT-01, ATT-02 | EVT-04, ATT-01, ATT-02; valid-attendee query; attendee screen from Member 4. | Transactions | TBD |
| CERT-02 — Officer/Admin (issue); Owner/Management (download): Issue and download PDF certificates | Generate and retain one fixed-template PDF per approved eligible participant, preserving unique number, snapshots and issuer details. Prevent duplicate issuance, handle file/database failures and provide private authorized downloads without rewriting issued PDFs. Use 25-row pagination for list results. | Member 5 | CERT-01 | CERT-01; shared storage; Member 3/4 screen integrations. | Transactions | TBD |
| REP-01 — Officer/Admin: View participation reports | Show participants/statuses, confirmed and attendee counts, attendance rate, completed-event no-shows and attendee course/year distribution. Use signup snapshots and full-dataset totals; exclude unconfirmed/cancelled requests from expected attendance and defer report exports. Paginate detail rows at 25; retain full-dataset totals. | Member 5 | EVT-04, REG-01, ATT-01, ATT-02 | EVT-04, REG-01, ATT-01, ATT-02; snapshots and attendance definitions. | Reports | TBD |

### Administration — Member 1

Concise delivery summaries; complete acceptance criteria remain in AttendTa-User-Stories.md and AttendTa-System-Specification.md. Each slice includes database-to-screen behavior and tests. Agree handoff contracts early; estimates include integration without double-counting shared screens.

| TASK / ACTOR | DESCRIPTION / DEMONSTRABLE RESULT | OWNER | PREDECESSORS | DEPENDS ON / HANDOFF | UC MAPPING | EST. MD |
| --- | --- | --- | --- | --- | --- | --- |
| ADM-01 — Admin: Manage roles and account access | Assign fixed roles and deactivate/reactivate accounts with reasons. Block inactive access, apply future-registration cancellation rules, preserve history and protect the last active administrator; do not automatically restore seats or promote participants. Use 25-row pagination for list results. | Member 1 | AUTH-03 | AUTH-03; Member 3 future-registration cancellation service. | Administration | TBD |
| ADM-02 — Admin: Review activity history | Provide a basic read-only activity list showing actor, action, affected record, time and reason. Preserve append-only audit history and relevant stored changes without credentials/tokens; defer advanced filters and field-comparison screens. Use 25-row pagination for list results. | Member 1 | — (see handoff) | Audit contract and events from all slice owners. | Administration | TBD |

## Shared Rules and Estimation

### Cross-cutting delivery work — no additional functional module

Historical retention is included in each slice; Member 1 coordinates identity/access rules. Pagination is included in applicable list tasks at 25 rows per page with Previous/Next; Member 2 provides reusable helpers and each owner integrates/tests their lists, with no SHR task or separate pagination estimate. Setup/deployment: Member 1 leads lookup/admin setup, Member 2 storage/backups, each owner supplies constraints/indexes and all five perform integration/security/boundary tests and demonstration. Estimates TBD; avoid double-counting common services.

### Shared rules and agreed refinements

One organization; ASP.NET Core MVC, SQL Server, EF Core database-first; UTC datetime2(3) storage and explicitly labelled organization-local date/time display.

UC explicitly lists registration statuses; draft/published/cancelled/completed event states are agreed implementation refinements. Reaching a deadline does not change a published event's status.

Publishing opens signup. Signup is allowed only while published and now < registration_deadline; publication must occur before the deadline. Require deadline no later than event start.

The deadline stops new signups only. Existing pending reviews and officer-triggered waitlist promotion remain possible until now < starts_at. Member cancellation is allowed before start without valid attendance, even after the signup deadline. All these operations also require a published event and current authorization.

At event start, stop reviews, confirmations, promotions and member cancellations. Pending/waitlisted rows retain their status as non-actionable history with a visible “Not confirmed before cutoff” explanation; they cannot check in and are not no-shows. No scheduled closeout or mass status update is required.

Waitlist order is ascending reviewed_at, then registration_id, among currently eligible reviewed waitlisted requests. Existing eligible waitlisted requests take priority over newly approved requests. Promotion is an explicit officer/admin action, never triggered automatically by a cancellation or deactivation.

Only confirmed registrations occupy seats. Recheck capacity, cutoffs and eligibility transactionally under concurrent reviews/promotions. One registration per member/event; cancellation or decline is final for this release, with no reapplication.

Draft fields are editable. After publication, only title, description and cover are editable; schedule, venue, type, limit and deadline are locked. Completed/cancelled events are read-only. Covers remain mandatory; replace rather than remove a published cover.

Manual check-in is independent of signup deadlines and may open early; opening/closing again is allowed while published. Completion closes check-in. No attendance-duration tracking or offline officer synchronization.

Before an event starts, loss of account/member eligibility cancels future pending/waitlisted registrations and confirmed registrations without valid attendance, preserving reasons and audit history. Release seats without automatic promotion. For started/completed events preserve registration/attendance history; current ineligibility still blocks new check-in. Reactivation does not restore cancelled registrations.

Ordinary event cancellation requires no valid attendance or issued certificate and an event not already completed/cancelled. Require a reason, cancel active registrations, invalidate QR credentials and close check-in atomically; never promote during cancellation. Completion is an explicit officer action at/after scheduled start when the event has concluded.

One attendance row per registration. Invalidation preserves the original row and logs, removes its contribution to reports and blocks unissued eligibility/issuance. Re-check-in after invalidation is deferred; show that limitation before correction. Issued certificates block ordinary attendance invalidation.

Certificate eligibility requires valid attendance, event completion and officer/admin approval. Issue one retained landscape A4 PDF per registration using one fixed template; preserve number, recipient/event snapshots, issuer and timestamp. No template editor, bulk issuance, email, revocation or correction/reissue.

User profile pictures are not included. Venue photos are mandatory at creation. Event covers are mandatory before publication, optional while drafting. One current image per venue/event; JPEG/PNG, maximum 2 MiB, with content validation and generated filenames. Officers/admins manage these images within lifecycle rules. Required images cannot be removed without replacement.

Images and private PDFs use a configurable persistent server-local root outside the web/deployment directory, not Supabase or browser localStorage. SQL stores relative paths only. Validate path containment, permission and file content; retain old images until replacement commits. Files must survive redeployment and be backed up with SQL Server. Certificates are served through authorized backend streams, never public static files. File and SQL writes are not a shared transaction: clean up/reconcile orphaned files on database failure and never mark issuance successful after file failure.

Reports use academic signup snapshots. Completed-event no-show = confirmed registration without valid attendance. Attendance rate = confirmed registrations with valid attendance / confirmed registrations × 100; zero denominator = N/A. Pending/waitlisted/cancelled registrations are excluded. Show actual-attendee course/year distribution only; confirmed counts and participant lists remain. Full-dataset totals never depend on a displayed page.

No general archive/restore or hard deletes. Use active/inactive master records, account deactivation, event lifecycle states and registration cancellation. Preserve identifiers, references and historical rows.

Pagination is included in necessary functional list tasks and their estimates, with no separate SHR module/task or pagination estimate. Use fixed 25-row server-side pagination with Previous/Next, necessary search/filters, authorized counts, stable ordering with ID tie-breaker, retained query state and empty/out-of-range handling.

Enforce password hashing, current account/role checks, ownership, antiforgery protection for mutations, unique constraints and append-only audit writes without credentials/tokens. Protect the last active admin. Management roles do not bypass participation eligibility/capacity; ordinary role permissions allow reviewing/promoting own participation or approving/issuing own certificate, with the same validation and audit requirements.

Applicant outcome communication remains an explicit open decision: submission acknowledgment and officer-managed offline communication for the demo, with no claimed public lookup, tracking-token service or outgoing email. This does not block retrying login after an externally communicated approval.

### Deferred

Known-password changes ; reopening declined certificate decisions; confirmed-participant course/year distribution; advanced log filtering and field-comparison UI; general archive/restore; automatic waitlist promotion and scheduled registration closeout; event reapplication; separate manual registration opening; operational edits after publication; self-service profile-detail editing; user profile-picture uploads; outgoing email/Brevo; email verification; forgotten-password/reset tokens; self-service email changes; dedicated session/device tables and individual token revocation; custom permission editors; CSV/PDF report exports; image galleries/cropping; certificate template editors/bulk issuance/revocation/reissue; in-system QR printing; attendance duration and re-check-in after invalidation.

### Screen consolidation and delivery ownership

UC traceability retains its five categories: Authentication, Master Data, Transactions, Reports and Administration. For task assignment, the WBS instead uses six familiar functional headings and five owner packages: Member 1 owns Authentication plus Administration; Member 2 Master Data; Member 3 Events and Registration; Member 4 Attendance and QR Codes; Member 5 Certificates and Reports. These are delivery groupings, not new project scope. Each slice includes database/query work, business logic, UI, authorization/validation, audit integration and tests.

Member 1 leads authorization/audit infrastructure; each owner integrates checks and audit in their own slices. Member 2 leads shared image storage/upload and pagination components; other owners integrate them. Member 3 owns seat allocation, token creation on confirmation and registration changes invoked by Member 1 account deactivation or Member 2 membership inactivity. Member 4 owns QR rendering/scanning and attendance correction, coordinating certificate consequences with Member 5. Member 5 owns PDF generation/private downloads using shared storage conventions.

My Events combines personal registration status, permitted cancellation, QR, attendance and issued-certificate download in one list/detail journey (REG-01, REG-03, QR-01, ATT-03 and member side of CERT-02). Member 3 owns its shell; Members 4/5 contribute their sections. Event-attendee management combines attendance, correction, eligibility and issuance (ATT-01, ATT-02, CERT-01 and management side of CERT-02); Member 4 owns the attendance shell, Member 5 contributes eligibility/issuance. Stories are acceptance slices, not separate screen commitments. Use a basic activity-log viewer.

Agree schema/service contracts together before parallel implementation; use reviewed database scripts/scaffolding updates rather than competing schema edits. Integrate small slices continuously. Member 3 has the highest workflow risk; estimates must verify workload balance, not assume equal effort from package counts. Every owner tests their slice; all five run the end-to-end journey and deployment checks. Shared work is estimated once plus integrations.

Password change remains deferred without a reserved WBS number; story IDs in the user-story document remain unchanged. No additional table or schema removal is needed for deferring password changes; password_hash remains essential for secure login.

### Estimation and authority

All MD and total MM estimates remain TBD. Estimate remaining database, UI, services, storage, integration, testing and deployment work; avoid double-counting shared services. State working days/month before converting MD to MM. UC is the requirements baseline; added QR/image/PDF details and lifecycle policies are agreed refinements, not claims that UC explicitly mandates them. The prior specification and November 12 target describe the older baseline.
