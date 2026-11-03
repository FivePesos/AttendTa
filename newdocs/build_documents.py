"""Rebuild the revised MVP stories, sequence views, schema, and WBS PDF.

Run from the repository root: python newdocs/build_documents.py
Requires reportlab for PDF generation. Originals in docs/ are read-only inputs.
"""
from pathlib import Path
import re
import html
import textwrap

ROOT = Path(__file__).resolve().parent.parent
OUT = Path(__file__).resolve().parent
BASE = ROOT / 'docs'

HEADER = '''Version: 2.1 — UC-aligned reduced MVP
Requirements baseline: `../docs/UC-Project-FL (1).pdf`, plus the explicit refinements agreed in the scope review.
Stated original submission deadline: October 7, 2026 (historical; not a new delivery promise).

These revised copies supersede the broader workflow descriptions for this revised baseline only. Original files in `docs/` remain unchanged. The original specification v1.3 describes the previous scope and must not override these agreed refinements. Ten core business tables are retained; SQL Server lookup tables implement the readable DBML enums.
'''

RULES = '''## Shared rules and agreed refinements

- One organization; ASP.NET Core MVC, SQL Server, EF Core database-first; UTC `datetime2(3)` storage and explicitly labelled organization-local date/time display.
- UC explicitly lists registration statuses; draft/published/cancelled/completed event states are agreed implementation refinements. Reaching a deadline does not change a published event's status.
- Publishing opens signup. Signup is allowed only while published and `now < registration_deadline`; publication must occur before the deadline. Require deadline no later than event start.
- The deadline stops new signups only. Existing pending reviews and officer-triggered waitlist promotion remain possible until `now < starts_at`. Member cancellation is allowed before start without valid attendance, even after the signup deadline. All these operations also require a published event and current authorization.
- At event start, stop reviews, confirmations, promotions and member cancellations. Pending/waitlisted rows retain their status as non-actionable history with a visible “Not confirmed before cutoff” explanation; they cannot check in and are not no-shows. No scheduled closeout or mass status update is required.
- Waitlist order is ascending `reviewed_at`, then `registration_id`, among currently eligible reviewed waitlisted requests. Existing eligible waitlisted requests take priority over newly approved requests. Promotion is an explicit officer/admin action, never triggered automatically by a cancellation or deactivation.
- Only confirmed registrations occupy seats. Recheck capacity, cutoffs and eligibility transactionally under concurrent reviews/promotions. One registration per member/event; cancellation or decline is final for this release, with no reapplication.
- Draft fields are editable. After publication, only title, description and cover are editable; schedule, venue, type, limit and deadline are locked. Completed/cancelled events are read-only. Covers remain mandatory; replace rather than remove a published cover.
- Manual check-in is independent of signup deadlines and may open early; opening/closing again is allowed while published. Completion closes check-in. No attendance-duration tracking or offline officer synchronization.
- Before an event starts, loss of account/member eligibility cancels future pending/waitlisted registrations and confirmed registrations without valid attendance, preserving reasons and audit history. Release seats without automatic promotion. For started/completed events preserve registration/attendance history; current ineligibility still blocks new check-in. Reactivation does not restore cancelled registrations.
- Ordinary event cancellation requires no valid attendance or issued certificate and an event not already completed/cancelled. Require a reason, cancel active registrations, invalidate QR credentials and close check-in atomically; never promote during cancellation. Completion is an explicit officer action at/after scheduled start when the event has concluded.
- One attendance row per registration. Invalidation preserves the original row and logs, removes its contribution to reports and blocks unissued eligibility/issuance. Re-check-in after invalidation is deferred; show that limitation before correction. Issued certificates block ordinary attendance invalidation.
- Certificate eligibility requires valid attendance, event completion and officer/admin approval. Issue one retained landscape A4 PDF per registration using one fixed template; preserve number, recipient/event snapshots, issuer and timestamp. No template editor, bulk issuance, email, revocation or correction/reissue.
- User profile pictures are not included. Venue photos are mandatory at creation. Event covers are mandatory before publication, optional while drafting. One current image per venue/event; JPEG/PNG, maximum 2 MiB, with content validation and generated filenames. Officers/admins manage these images within lifecycle rules. Required images cannot be removed without replacement.
- Images and private PDFs use a configurable persistent server-local root outside the web/deployment directory, not Supabase or browser localStorage. SQL stores relative paths only. Validate path containment, permission and file content; retain old images until replacement commits. Files must survive redeployment and be backed up with SQL Server. Certificates are served through authorized backend streams, never public static files. File and SQL writes are not a shared transaction: clean up/reconcile orphaned files on database failure and never mark issuance successful after file failure.
- Reports use academic signup snapshots. Completed-event no-show = confirmed registration without valid attendance. Attendance rate = confirmed registrations with valid attendance / confirmed registrations × 100; zero denominator = N/A. Pending/waitlisted/cancelled registrations are excluded. Course/year distribution defaults to attendees. Full-dataset totals never depend on a displayed page.
- No general archive/restore or hard deletes. Use active/inactive master records, account deactivation, event lifecycle states and registration cancellation. Preserve identifiers, references and historical rows.
- Fixed 25-row server-side pagination with Previous/Next, necessary search/filters, authorized counts, stable ordering with ID tie-breaker, retained query state and empty/out-of-range handling.
- Enforce password hashing, current account/role checks, ownership, antiforgery protection for mutations, unique constraints and append-only audit writes without credentials/tokens. Protect the last active admin. Management roles do not bypass participation eligibility/capacity; ordinary role permissions allow reviewing/promoting own participation or approving/issuing own certificate, with the same validation and audit requirements.
- Applicant outcome communication remains an explicit open decision: submission acknowledgment and officer-managed offline communication for the demo, with no claimed public lookup, tracking-token service or outgoing email. This does not block retrying login after an externally communicated approval.

## Deferred

Known-password changes (US-04); reopening declined certificate decisions; confirmed-participant course/year distribution; advanced log filtering and field-comparison UI; general archive/restore; automatic waitlist promotion and scheduled registration closeout; event reapplication; separate manual registration opening; operational edits after publication; self-service profile-detail editing (avatar changes remain); outgoing email/Brevo; email verification; forgotten-password/reset tokens; self-service email changes; dedicated session/device tables and individual token revocation; custom permission editors; CSV/PDF report exports; image galleries/cropping; certificate template editors/bulk issuance/revocation/reissue; in-system QR printing; attendance duration and re-check-in after invalidation.
'''

# id, title, actor, goal, acceptance criteria
STORIES = [
('01','Request an account','student','request access for membership verification',[
'Collect student number, full name, email, course, positive year level, section, optional mobile, password and confirmation; hash passwords and never retain confirmation.',
'Create a pending request only; prevent duplicate pending email/student number and existing account/member identities, including inactive identities.',
'Show acknowledgment without promising email or public tracking; allow a separate new account request after rejection while retaining prior requests. Event reapplication is a different, deferred workflow.']),
('02','Review account requests','officer/admin','approve valid organization members',[
'List/view requests without exposing hashes. Approval atomically creates an active member-role user and profile, links created_user_id and records reviewer/time plus audit.',
'Transfer the hash and clear it from the finalized request. Rejection requires a reason, clears the hash and creates no user/profile.',
'Applicants cannot select privileged roles; profiles are created only by approval.']),
('03','Log in and log out','approved user','access only authorized functionality',[
'Validate email/password using hashing; deny pending applicants/inactive accounts. Replace placeholder login and reversible password storage.',
'Use signed expiring cookie authentication; check current account/role on protected requests. Logout clears the cookie; no individual server-side token revocation is promised.']),
('05','Manage roles and account access','administrator','assign responsibility and control access',[
'Assign fixed member/officer/admin roles, deactivate/reactivate with reasons, preserve linked profiles and protect the last active admin.',
'Block existing/new access on deactivation; apply the shared future-registration cancellation rules, preserving past/in-progress history. Do not automatically promote or restore cancelled seats.']),
('06','View my profile','member','see my membership information',[
'Show own account/member details; retrieve current name/email from users and academic/member details from members.',
'Profile-detail changes are handled by officers. Profile-picture uploads are not included; password changes are deferred.']),
('07','Maintain members','officer/admin','keep approved membership information accurate',[
'List/view/edit approved members, full name, academic/contact details, membership type and active/inactive state; no independent Add Member action.',
'Validate student-number uniqueness; require correction reasons and relevant old/new audit values. Email changes are outside this MVP.',
'Apply shared loss-of-eligibility rules without rewriting academic signup snapshots or issued PDFs; membership inactivity does not silently deactivate login.']),
('08','Manage event types','officer/admin','classify events consistently',[
'Create/edit unique nonblank names and descriptions; activate/deactivate without archive/delete controls.',
'Only active types are selectable for draft events; retain historical references and uniqueness across inactive records.']),
('09','Manage venues','officer/admin','maintain suitable event locations',[
'Maintain room name, positive capacity, location, availability notes, contact person, required photo and active state.',
'Reject creation without a valid photo. Permit replacement, not removal without replacement. Preserve event references on deactivation.',
'Prevent venue capacity edits below limits of linked draft/published events. No automated booking-conflict detection.']),
('10','Manage event and venue images','officer/admin','recognize events and venues',[
'Officers/admins manage required venue photos and event covers. Draft covers may be absent; publication requires one. User profile-picture uploads are not included.',
'Use one shared JPEG/PNG upload service, maximum 2 MiB, validated content and generated filenames. Allow draft-cover removal only while optional.',
'Persist relative paths locally; enforce shared storage, replacement cleanup, authorization and event-edit rules.']),
('11','Create and publish events','officer/admin','announce activities and accept signups',[
'Create editable drafts with title, description, type, venue, start, limit and deadline. Validate active masters, positive limit within venue capacity and deadline no later than start.',
'Require a valid cover and future deadline at publication; save actor/time and audit. Publishing immediately opens signup until deadline; hide drafts from members.']),
('12','Show registration availability','member','understand whether I can register',[
'Published events display Open before deadline and Closed at/after the exact deadline, with labelled local date/time. No separate Open Registration control or database fields.',
'Server rejects late submissions even if the form was opened earlier. The event remains published when signup closes.']),
('13','Edit event details','officer/admin','correct descriptions without changing commitments',[
'Edit all draft fields. After publication permit only title, description and cover replacement; lock schedule, venue, type, participant limit and deadline.',
'Completed/cancelled records are read-only; keep required cover and log changes. Issued certificate snapshots never change.']),
('14','Cancel or complete events','officer/admin','record the actual event outcome',[
'Cancel eligible drafts/published events with reason; atomically cancel active registrations, invalidate tokens, close check-in and log. Preserve existing cancellation reasons.',
'Valid attendance, completed state or an issued certificate blocks cancellation. Affected members can still identify previously published cancelled events.',
'Complete published events explicitly at/after start once concluded; close check-in and enable valid-attendee certificate review. Elapsed time alone does not complete an event.']),
('15','Browse and register','active member','request participation in an event',[
'Browse published events/details/covers and submit only before deadline with active account/member eligibility.',
'Create pending registration plus course/year/section snapshots and submission time. One row per member/event including cancelled rows; no event reapplication.',
'Full events may accept pending requests for review/waitlisting with a no-guaranteed-seat warning; no overlapping-event restriction.']),
('16','Review registrations','officer/admin','assign reviewed requests to seats or the waitlist',[
'Before event start, Approve or Decline existing pending requests even after signup deadline; require a published event and eligible participant.',
'Approve to confirmed only with capacity and no prior eligible waitlist; otherwise waitlisted. Decline to cancelled with reason.',
'Record reviewer/time and recheck constraints transactionally. Ordinary role permissions apply, including own participation, with audit. Only confirmed registrations receive unpredictable QR tokens and occupy seats.']),
('17','Cancel my registration','participant','withdraw before the event starts',[
'Allow owner cancellation of pending/confirmed/waitlisted rows while published, before start and without valid attendance, including after signup deadline.',
'Warn cancellation is final for this release. Save reason/time/actor, invalidate token and release the seat; no reapplication and no automatic promotion.']),
('18','Promote the next waitlisted participant','officer/admin','fill an available seat fairly',[
'Explicit action selects earliest currently eligible reviewed waitlisted row ordered by reviewed_at then registration_id across the full queue.',
'Require published event, time before start, free capacity and eligible participant. Ordinary role permissions apply without a self-promotion prohibition. Recheck transactionally; keep original review, set confirmation actor/time and token, and audit officer action.',
'No eligible candidate/free seat means no promotion. Member sees confirmed status and QR on next visit; no outgoing notification.']),
('19','Enforce processing cutoffs','organizer','prevent late participation changes',[
'New signup stops at deadline; reviews, confirmations, officer promotions and member cancellations stop at exact event start, enforced on every server action.',
'Retain unresolved pending/waitlisted statuses; show Not confirmed before cutoff and disable actions. No automatic status rewriting or scheduled job.',
'Unconfirmed participants cannot check in and are excluded from no-shows; check-in opening is independent.']),
('20','Access my event QR','confirmed participant','present my attendance credential',[
'Authorize owner and generate event-registration QR PNG from unpredictable token on demand; permit display, screenshot and PNG download without retained QR files.',
'Saved copies work without member internet but officer validation is online. Cancelled/ineligible/unconfirmed rows have no usable QR; displaying QR never records attendance.']),
('21','Control and record check-in','officer/admin','record arrivals accurately',[
'Select published event and manually open/close/reopen check-in, including early opening. Completed/cancelled events reject check-in.',
'QR scan automatically records after selected-event, confirmed status, active account/member and duplicate checks; show name/student number for identity verification.',
'Manual name/student-number search requires explicit Check in and the same validation. Record actor/time/method atomically with audit, prevent concurrent duplicates, and show original time on repeated valid scan.']),
('22','Correct attendance','officer/admin','exclude mistaken check-ins without losing history',[
'Invalidate with confirmation, reason, actor/time, preserve original attendance and audit. No duration-based invalidation.',
'Exclude invalidated rows from reports and eligibility; set affected unissued eligibility to not eligible with correction reason. Issuance always rechecks attendance.',
'Block invalidation if certificate issued. Re-check-in after invalidation is deferred and warned before correction.']),
('23','View my attendance','member','see my participation record',[
'Show authorized personal event/check-in details, including corrected records clearly labelled.',
'Members cannot mark attendance or invalidate it; preserve history after profile and eligibility changes.']),
('24','Review certificate eligibility','officer/admin','approve certificates for valid completed participation',[
'Create pending-approval rows for valid attendees of completed events only; approve eligible or decline not eligible with a reason and reviewer/time.',
'Recheck attendance/completion and current role; approval of own participation follows the same rules and audit. Warn before decline that declined decisions cannot be reopened in this release; never change issued decisions.',
'No-shows receive no certificate records; no duration requirement.']),
('25','Issue and download PDF certificates','authorized manager or member','retain proof of completed participation',[
'Officer/admin issues one eligible certificate at a time with one fixed landscape A4 template, embedded fonts, unique number, recipient/event snapshots and issuer/time; ordinary role permissions also apply to own certificates with audit.',
'Retain private PDF at certificates.file_path; prevent duplicate concurrent issuance and recheck valid attendance/completion/approval at final commit. Clean up/reconcile files if commit fails.',
'Mark issued only after file and metadata succeed. Active account owner or authorized management can view/download via backend application/pdf stream; membership inactivity alone does not revoke historical certificates.',
'Later edits do not rewrite issued PDFs. No email, public static PDFs, bulk issuance, template editor, revocation or correction/reissue.']),
('26','View participation reports','officer/admin','evaluate turnout',[
'Show registration statuses, valid attendees, completed-event no-shows, attendance rate and course/year distributions using the shared formulas and signup snapshots.',
'Show actual-attendee course/year distribution only, plus participant list, confirmed count, attendee count, rate and no-shows; no confirmed-participant distribution. Pending/waitlisted history is not expected attendance. Cancelled events are shown separately.',
'Full-dataset summaries are independent of pagination. No CSV/PDF report export.']),
('27','Retain historical records','authorized manager','preserve reliable participation history',[
'Use inactive master/account states, event lifecycle and registration cancellation; no general archive/restore or business-record hard deletes.',
'Preserve historical references, unique identities, snapshots, attendance and issued PDFs. Reactivation does not recreate cancelled participation.']),
('28','Review activity history','administrator','trace important decisions',[
'Use one read-only list showing actor, action, affected record, timestamp and reason/short description newest first. Retain relevant old/new values in storage, but defer advanced filtering and a field-by-field comparison UI. No log edit/delete controls.',
'Log mutations consistently with their database transactions; omit passwords, hashes, QR tokens and physical filesystem paths.']),
('29','Browse paginated lists','authorized user','find records efficiently',[
'Use fixed 25-row server-side pages with Previous/Next and necessary search/filters for requests/accounts, masters/events, registrations/search, attendance, certificates, reports and logs.',
'Filter authorization/ownership before counts and slicing; use stable sort plus ID tie-breaker. Preserve query state, reset after filter changes, normalize invalid/out-of-range pages and show empty state.',
'Counts cannot disclose unauthorized data; report totals and promotion selection use full applicable datasets, never just visible page rows.']),
]

RULES = RULES.replace('Course/year distribution defaults to attendees.', 'Show actual-attendee course/year distribution only; confirmed counts and participant lists remain.')
RULES = RULES.replace('self-service profile-detail editing (avatar changes remain)', 'self-service profile-detail editing; user profile-picture uploads')
RULES = RULES.replace('Fixed 25-row server-side pagination with Previous/Next,', 'Pagination is included in necessary functional list tasks and their estimates, with no separate SHR module/task or pagination estimate. Use fixed 25-row server-side pagination with Previous/Next,')
RULES += '''\n## Screen consolidation and delivery ownership\n\nUC traceability retains its five categories: Authentication, Master Data, Transactions, Reports and Administration. For task assignment, the WBS instead uses six familiar functional headings and five owner packages: Member 1 owns Authentication plus Administration; Member 2 Master Data; Member 3 Events and Registration; Member 4 Attendance and QR Codes; Member 5 Certificates and Reports. These are delivery groupings, not new project scope. Each slice includes database/query work, business logic, UI, authorization/validation, audit integration and tests.\n\nMember 1 leads authorization/audit infrastructure; each owner integrates checks and audit in their own slices. Member 2 leads shared image storage/upload and pagination components; other owners integrate them. Member 3 owns seat allocation, token creation on confirmation and registration changes invoked by Member 1 account deactivation or Member 2 membership inactivity. Member 4 owns QR rendering/scanning and attendance correction, coordinating certificate consequences with Member 5. Member 5 owns PDF generation/private downloads using shared storage conventions.\n\nMy Events combines personal registration status, permitted cancellation, QR, attendance and issued-certificate download in one list/detail journey (US-15, US-17, US-20, US-23 and member side of US-25). Member 3 owns its shell; Members 4/5 contribute their sections. Event-attendee management combines attendance, correction, eligibility and issuance (US-21/22/24 and management side of US-25); Member 4 owns the attendance shell, Member 5 contributes eligibility/issuance. Stories are acceptance slices, not separate screen commitments. Use a basic activity-log viewer.\n\nAgree schema/service contracts together before parallel implementation; use reviewed database scripts/scaffolding updates rather than competing schema edits. Integrate small slices continuously. Member 3 has the highest workflow risk; estimates must verify workload balance, not assume equal effort from package counts. Every owner tests their slice; all five run the end-to-end journey and deployment checks. Shared work is estimated once plus integrations.\n\nUS-04 is reserved and deferred; remaining story IDs are unchanged. No additional table or schema removal is needed for deferring password changes; password_hash remains essential for secure login.\n'''
stories = '# AttendTa — Revised MVP User Stories\n\n' + HEADER + '\n## Actors\n\nApplicant; member; officer (also an eligible member); administrator; system validation. Administrators need a member profile only to participate. Fixed roles never bypass capacity or personal ownership rules.\n\n'
for sid, title, actor, goal, criteria in STORIES:
    stories += f'## US-{sid} — {title}\n\n**As a {actor}, I want to {goal}.**\n\nAcceptance criteria:\n' + ''.join(f'- {c}\n' for c in criteria) + '\n'
stories += RULES + '\n## Deployment and acceptance\n\nCreate the first active administrator using supplied credentials and a password hash, without hard-coded defaults. Seed fixed lookup values. Include persistent-disk permissions/backups, database constraints, integration, security checks and deployment testing. Estimates remain TBD, not inferred from task counts.\n\nDemo: request → approve → login → create venue with photo → draft event with cover → publish → signup → confirm → QR check-in → complete → approve eligibility → issue/download PDF → reports/logs. Demonstrate cancellation and explicit waitlist promotion separately, plus deadline/start boundary rejection.\n'
(OUT / 'AttendTa-User-Stories.md').write_text(stories, encoding='utf-8')

# Derive a revised schema without touching the original.
schema = (BASE / 'AttendTa-MVP.dbml').read_text(encoding='utf-8')
user_start = schema.index('Table users {')
user_end = schema.index('Table account_requests {')
schema = schema[:user_start] + re.sub(r'^  image_path .*\n', '', schema[user_start:user_end], flags=re.M) + schema[user_end:]
schema = re.sub(r'^  (?:deleted_by|deleted_at|deletion_reason|registration_opened_by|registration_opened_at) .*\n', '', schema, flags=re.M)
schema = re.sub(r'^Ref: .*\.(?:deleted_by|registration_opened_by) > .*\n', '', schema, flags=re.M)
schema = schema.replace('apply publication/archive visibility first', 'apply publication visibility first')
schema = schema.replace('apply publication/archive visibility', 'apply publication visibility')
schema = schema.replace('Table venues {', "Table venues {\n  Note: 'A validated venue photo is required at creation; replacement is permitted, removal without replacement is not.'")
start = schema.index('Table venues {'); end = schema.index('Table events {')
schema = schema[:start] + schema[start:end].replace('  image_path nvarchar(500)', "  image_path nvarchar(500) [not null, note: 'Required relative local image path.']") + schema[end:]
schema = schema.replace('Table events {', "Table events {\n  Note: 'Publishing opens signup before the deadline and requires a cover. After publication only title, description and cover may change. At deadline signup closes without changing event status.'")
schema = schema.replace("    `capacity > 0` [name: 'chk_venues_positive_capacity']", "    `capacity > 0` [name: 'chk_venues_positive_capacity']\n    `LEN(LTRIM(RTRIM(image_path))) > 0` [name: 'chk_venues_required_photo']")
schema = schema.replace("    `participant_limit > 0` [name: 'chk_events_positive_participant_limit']", "    `participant_limit > 0` [name: 'chk_events_positive_participant_limit']\n    // Physical SQL Server enum values are lookup FKs; implement the status/cover rule in the service or a suitable trigger.\n    // Published/completed events require a nonblank existing validated cover; cancelled drafts may have no image.")
schema = schema.replace('Table registrations {', "Table registrations {\n  Note: 'One row per member/event, no reapplication. Officer-triggered promotion only; queue: reviewed_at ASC, registration_id ASC. At start pending/waitlisted remain non-actionable history, not no-shows. Capacity/cutoff/eligibility checks require transactional server enforcement.'")
schema = schema.replace('(event_id, status, submitted_at, registration_id) // Review/queue ordering; display pages do not drive promotion.', '(event_id, status, reviewed_at, registration_id) // Reviewed waitlist order; promotion is officer-triggered.')
schema = schema.replace('  image_path nvarchar(500)\n\n  published_by', "  image_path nvarchar(500) [note: 'Optional for draft only; validated nonblank cover required for published/completed events.']\n\n  published_by")
schema = schema.replace('  Note: \'Issued certificates require', "  Note: 'One fixed PDF template; no bulk issuance, revocation or reissue. Issued certificates require")
schema = '// Revised MVP v2.1; UC baseline plus agreed refinements. Originals remain in docs/.\n// Password changes and reopening declined certificate decisions are deferred; password_hash remains for login.\n// No archive/delete fields or manual registration-opening fields.\n// No user image field; mandatory venue photos; event covers required before publishing.\n// See AttendTa-User-Stories.md shared rules for application/transaction constraints.\n' + schema
(OUT / 'AttendTa-MVP.dbml').write_text(schema, encoding='utf-8')

# Each sequence is shared source for editable Mermaid and offline static SVG.
DIAGRAMS = []
def diagram(slug, title, refs, actors, messages, note):
    assert len(actors) <= 5 and len(messages) <= 12
    DIAGRAMS.append((slug,title,refs,actors,messages,note))

diagram('pagination','Browse authorized lists','US-29', ['Authorized user','ASP.NET Core','SQL Server'],[
(0,1,'Request page and filters'),(1,2,'Count authorized filtered rows'),(2,1,'Matching count'),(1,2,'Read stable 25-row page'),(2,1,'Page rows'),(1,0,'Render rows and Previous/Next')],
'Fixed 25 rows. Apply role/owner filters before count/slice and ID tie-breaker before paging. Full report totals and waitlist selection are independent of pages. Reset on changed filters and handle empty/out-of-range pages.')
diagram('approval','Account request and approval','US-01–03',['Applicant','Officer / Admin','ASP.NET Core','SQL Server'],[
(0,2,'Submit details and password'),(2,3,'Validate duplicates; save hash'),(3,2,'Pending request saved'),(2,0,'Acknowledge submission'),(1,2,'Approve request'),(2,3,'Atomic user/member approval + audit'),(3,2,'Approval committed; request hash cleared'),(2,1,'Approval recorded'),(0,2,'Later login with email/password'),(2,3,'Read active account and hash'),(3,2,'Account credentials and role'),(2,0,'Validate hash; issue signed cookie')],
'Rejection instead records reason and clears request hash without creating an account. Applicant receives no automated email/public tracking; outcome communication is officer-managed offline for the demo. No login bypass or reversible credential storage.')
diagram('images','Upload or replace event/venue image','US-09–11',['Officer / Admin','ASP.NET Core','Local Filesystem','SQL Server'],[
(0,1,'Submit JPEG/PNG image'),(1,3,'Check management role and event lifecycle'),(3,1,'Record and current path'),(1,2,'Validate content; write unique file'),(2,1,'New relative path'),(1,3,'Commit image_path and audit'),(3,1,'Path update committed'),(1,2,'Remove old file after commit'),(1,0,'Display current image')],
'Maximum 2 MiB. User profile-picture uploads are not included. Venue photo mandatory at creation; draft cover optional but required before publication. Required images cannot be removed without replacement. Lifecycle restrictions apply. On failed SQL save clean up/reconcile the new file and retain old path/file. Persistent local root outside web/deployment directory; no physical paths sent to clients.')
diagram('registration','Publish, register and approve','US-11–16',['Member','Officer / Admin','ASP.NET Core','SQL Server'],[
(1,2,'Publish draft with required cover'),(2,3,'Validate masters, cover, future deadline'),(3,2,'Publish and audit committed'),(2,1,'Published; signup now open'),(0,2,'Submit signup before deadline'),(2,3,'Validate; save pending + snapshots'),(3,2,'Pending registration saved'),(2,0,'Pending review'),(1,2,'Approve before event start'),(2,3,'Atomic eligibility/capacity/queue check'),(3,2,'Confirmed + token + audit committed'),(2,1,'Confirmation result')],
'Available-seat path without earlier eligible waitlist. Otherwise approval produces waitlisted. Review may occur after signup deadline but before start. No separate manual registration opening. Only descriptive edits/cover replacement after publish; operational fields locked. Ordinary role permissions also apply to own participation; audit all decisions. No arbitrary status editing.')
diagram('cancellation','Cancel a registration','US-17',['Member','ASP.NET Core','SQL Server'],[
(0,1,'Confirm final cancellation'),(1,2,'Check owner, cutoff, attendance'),(2,1,'Cancellation permitted'),(1,2,'Cancel + invalidate token + audit'),(2,1,'Seat released; commit complete'),(1,0,'Cancelled; no reapplication')],
'Allowed while event published and before start without valid attendance, including after signup deadline. Seat release does not automatically promote anyone. Officer explicitly runs the separate promotion action.')
diagram('waitlist','Officer-triggered waitlist promotion','US-18',['Officer / Admin','ASP.NET Core','SQL Server'],[
(0,1,'Promote next eligible participant'),(1,2,'Check published event and start cutoff'),(2,1,'Event permits promotion'),(1,2,'Atomic capacity + full eligible queue check'),(2,1,'Select reviewed_at then ID'),(1,2,'Confirm + token + actor/time + audit'),(2,1,'Commit seat allocation'),(1,0,'Participant confirmed')],
'Select earliest eligible reviewed waitlisted request across the entire queue, not the visible page. Preserve original reviewer. All checks and updates belong to one seat-allocation transaction. If no candidate or no capacity, do not mutate. No automatic promotion or emails; member sees new QR/status on next authorized visit.')
diagram('cutoff','Reject signup at the deadline','US-12, US-19',['Member','ASP.NET Core','SQL Server'],[
(0,1,'Submit previously opened signup form'),(1,2,'Read event deadline and current state'),(2,1,'Published event; deadline reached'),(1,0,'Reject: registration closed')],
'At now >= deadline new signup is rejected server-side; event stays published. Existing reviews/promotions/cancellations may continue until start. At now >= start reject those too; pending/waitlisted remain visibly non-actionable history, excluded from no-shows. Check-in has its own manual gate. Store UTC; display labelled local time. No scheduled status rewrite.')
diagram('checkin','Saved QR and attendance check-in','US-20–23',['Member','Officer / Admin','ASP.NET Core','SQL Server'],[
(0,2,'Request own confirmed event QR'),(2,3,'Authorize owner; read token'),(3,2,'Confirmed eligible registration'),(2,0,'Generate PNG for display/download'),(1,2,'Open event check-in'),(2,3,'Save check-in state + audit'),(0,1,'Present saved QR at venue'),(1,2,'Scan QR for selected event'),(2,3,'Atomic eligibility + duplicate check'),(2,3,'Insert attendance + audit'),(3,2,'Attendance committed'),(2,1,'Success with participant identity')],
'Officer requires internet; member may use screenshot offline. Validate event match, open check-in, active account/member and confirmed registration; unique constraint prevents simultaneous duplicate scans. Manual search requires explicit Check in with same service. Invalidated attendance cannot be rechecked in this release; issued certificates block invalidation.')
diagram('certificate','Complete, approve and issue PDF','US-14, US-24–25',['Officer / Admin','ASP.NET Core','SQL Server','Local Filesystem'],[
(0,1,'Complete concluded event at/after start'),(1,2,'Close check-in; complete; audit'),(1,2,'Create pending valid-attendee reviews'),(0,1,'Approve certificate eligibility'),(1,2,'Validate attendance; save reviewer + audit'),(0,1,'Issue one eligible certificate'),(1,2,'Prepare unique number and snapshots'),(2,1,'Issuance data'),(1,3,'Generate fixed-template private PDF'),(3,1,'Saved relative file path'),(1,2,'Recheck; commit issued metadata + audit'),(1,0,'Issued only after successful commit')],
'One fixed landscape A4 template with embedded fonts. Eligibility review and issuance share event-attendee management. Ordinary role permissions permit own-certificate decisions with the same checks/audit. Declined decisions cannot reopen. No bulk issuance, email, revocation or reissue. Prevent concurrent duplicate issuance; final attendance/eligibility recheck must serialize with correction. File and SQL writes are not one transaction: never mark issued on file failure; clean up/reconcile orphan PDF after failed commit. Retain immutable PDF and snapshots.')
diagram('download','Authorized certificate download','US-25',['Member','ASP.NET Core','SQL Server','Local Filesystem'],[
(0,1,'View/download issued certificate'),(1,2,'Check active account, ownership, issuance'),(2,1,'Authorized certificate and file_path'),(1,3,'Read private PDF within storage root'),(3,1,'Retained PDF bytes'),(1,0,'Stream application/pdf')],
'Authorized management uses equivalent permission checks. Membership inactivity alone does not revoke historical certificates; inactive accounts cannot authenticate. Reject path traversal; never expose PDFs through static middleware or physical paths. Later profile/event edits cannot overwrite issued content.')

md = '# AttendTa — Revised MVP Sequence Diagrams\n\n' + HEADER + '\nOpen `AttendTa-Sequence-Diagrams.html` for offline static SVG views. The Mermaid below is editable source. Successful paths are shown; adjacent notes describe guards and failure handling. SQL mutations include audit entries. Routine acknowledgments are omitted.\n\n'
for slug,title,refs,actors,messages,note in DIAGRAMS:
    md += f'## {title}\n\n**Stories:** {refs}\n\n```mermaid\nsequenceDiagram\n    autonumber\n'
    for i,actor in enumerate(actors): md += f'    participant P{i} as {actor}\n'
    for a,b,label in messages:
        response = any(w in label.lower() for w in ('committed','matching count','page rows','relative path','pdf bytes','issuance data','account credentials','event permits','pending request saved','current path'))
        md += f'    P{a}{"-->>" if response else "->>"}P{b}: {label}\n'
    md += f'```\n\n{note}\n\n'
md += '## Shared policy reference\n\nSee `AttendTa-User-Stories.md` and `AttendTa-System-Specification.md` for acceptance criteria, authority, cutoffs, storage and deferred features. These are workflow views, not separate screens: personal registration/QR/attendance/download use My Events, and certificate review/issuance use event-attendee management. US-04 (Change Password) is deferred. Reports show attendee-only course/year distribution; logs use a basic read-only viewer.\n'
(OUT / 'AttendTa-Sequence-Diagrams.md').write_text(md, encoding='utf-8')

css = '''body{margin:0;background:#f5f5f5;color:#2d3142;font-family:Arial,sans-serif}main{max-width:1280px;margin:auto;padding:40px 24px}h1{font:40px Georgia,serif}h2{font-size:24px}p{line-height:1.6;max-width:1040px}nav{display:flex;gap:12px;flex-wrap:wrap;margin:24px 0}a{color:#2e5aa8}nav a{padding:8px;border:1px solid #bfc0c0;text-decoration:none}section{border-top:1px solid #bfc0c0;padding:24px 0}.figure{overflow-x:auto}svg{display:block;width:100%;min-width:960px;height:auto}.detail{border-left:3px solid #bfc0c0;padding-left:16px;color:#4f5d75;font-size:14px}.eyebrow,footer{font:12px monospace;color:#4f5d75}footer{padding:24px 0}@media print{body{background:white}main{padding:0}nav{display:none}section{break-before:page}svg{min-width:0}.detail{font-size:10px}}'''
page = '<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>AttendTa — Revised MVP Sequences</title><style>'+css+'</style></head><body><main><div class="eyebrow">ATTENDTA / REVISED MVP V2.1</div><h1>From request to participation</h1><p>UC baseline plus agreed refinements. Ten workflow views, not separate screens; ten business tables. My Events combines personal registration, QR, attendance and certificate downloads. Event-attendee management combines eligibility and issuance. Password changes and reopening declined certificate decisions are deferred. Reports use attendee-only distribution; logs use a basic viewer. Original submission deadline: October 7, 2026 (historical). Offline static SVG, existing neutral/orange palette and local Arial/Georgia/monospace fallbacks retained.</p><nav aria-label="Diagram navigation">'
page += ''.join(f'<a href="#{s}">{html.escape(t)}</a>' for s,t,*_ in DIAGRAMS) + '</nav>'
for slug,title,refs,actors,messages,note in DIAGRAMS:
    height=240+len(messages)*72
    xs=[120+i*(960//(len(actors)-1)) for i in range(len(actors))]
    svg=f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1200 {height}" role="img" aria-labelledby="{slug}-title {slug}-desc"><title id="{slug}-title">{html.escape(title)}</title><desc id="{slug}-desc">{html.escape(title)} showing authorized actors, application validation and durable data changes in time order.</desc><defs>'
    for kind,color in [('arrow','#4f5d75'),('accent','#eb6c36'),('link','#2e5aa8')]:
        svg+=f'<marker id="{slug}-{kind}" markerWidth="8" markerHeight="6" refX="7" refY="3" orient="auto"><polygon points="0 0,8 3,0 6" fill="{color}"/></marker>'
    svg+=f'</defs><rect width="1200" height="{height}" fill="#f5f5f5"/>'
    for x in xs: svg+=f'<line x1="{x}" y1="88" x2="{x}" y2="{height-80}" stroke="#bfc0c0" stroke-dasharray="4,4"/>'
    for idx,(a,b,label) in enumerate(messages):
        y=160+idx*72; x1,x2=xs[a],xs[b]; mid=(x1+x2)//2
        color='#eb6c36' if idx==len(messages)-1 else '#4f5d75'; kind='accent' if idx==len(messages)-1 else 'arrow'
        dash=' stroke-dasharray="5,4"' if a>b else ''
        svg+=f'<rect x="{x2-4}" y="{y-4}" width="8" height="24" fill="#ececec" stroke="#bfc0c0"/><line x1="{x1}" y1="{y}" x2="{x2}" y2="{y}" stroke="{color}" stroke-width="1.2"{dash} marker-end="url(#{slug}-{kind})"/>'
        lines=textwrap.wrap(f'{idx+1}. {label}',width=max(24,(abs(x2-x1)-24)//7))
        maskw=min(abs(x2-x1)-24, max(len(l) for l in lines)*7+16)
        svg+=f'<rect x="{mid-maskw/2:g}" y="{y-8-len(lines)*16}" width="{maskw}" height="{len(lines)*16}" fill="#f5f5f5"/>'
        for j,line in enumerate(lines): svg+=f'<text x="{mid}" y="{y-12-(len(lines)-1-j)*16}" text-anchor="middle" font-family="monospace" font-size="12" fill="{color}">{html.escape(line)}</text>'
    for x,actor in zip(xs,actors): svg+=f'<rect x="{x-100}" y="24" width="200" height="64" rx="4" fill="white" stroke="#2d3142"/><text x="{x}" y="60" text-anchor="middle" font-family="Arial,sans-serif" font-weight="600" font-size="16" fill="#2d3142">{html.escape(actor)}</text>'
    svg+=f'<line x1="24" y1="{height-64}" x2="1176" y2="{height-64}" stroke="#bfc0c0"/><text x="24" y="{height-40}" font-size="12" font-family="monospace" fill="#4f5d75">TIME FLOWS DOWN · REQUESTS / ACTIONS · DASHED RETURNS · ORANGE KEY OUTCOME</text></svg>'
    page+=f'<section id="{slug}"><div class="eyebrow">{refs}</div><h2>{html.escape(title)}</h2><div class="figure">{svg}</div><p class="detail">{html.escape(note)}</p></section>'
page+='<footer>AttendTa · UC baseline + agreed refinements · v2.1 · Full rules: AttendTa-User-Stories.md · No external runtime dependencies</footer></main></body></html>'
(OUT / 'AttendTa-Sequence-Diagrams.html').write_text(page, encoding='utf-8')

# The PDF is generated from the same story acceptance criteria, grouped as WBS.
from reportlab.lib import colors
from reportlab.lib.pagesizes import A4, landscape
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont

fontroot=Path('C:/Windows/Fonts')
pdfmetrics.registerFont(TTFont('AttendSans',str(fontroot/'arial.ttf')))
pdfmetrics.registerFont(TTFont('AttendSansBold',str(fontroot/'arialbd.ttf')))
pdfmetrics.registerFontFamily('AttendSans',normal='AttendSans',bold='AttendSansBold')
styles=getSampleStyleSheet()
styles.add(ParagraphStyle(name='CellText',fontName='AttendSans',fontSize=8,leading=11,spaceAfter=0))
styles.add(ParagraphStyle(name='SmallText',fontName='AttendSans',fontSize=9,leading=13,spaceAfter=7))
styles.add(ParagraphStyle(name='BrandTitle',fontName='AttendSansBold',fontSize=22,leading=27,textColor=colors.HexColor('#8c3735'),spaceAfter=12))
styles.add(ParagraphStyle(name='BrandHeading',fontName='AttendSansBold',fontSize=14,leading=18,textColor=colors.HexColor('#8c3735'),spaceAfter=10))
def p(text,style='CellText'): return Paragraph(html.escape(text),styles[style])
# WBS summaries stay concise; STORIES retains the full acceptance criteria.
WBS_DESCRIPTIONS = {
    '01': 'Submit student identity, academic details and password for approval. Validate duplicate identities, securely hash the password and create a pending request; allow a new account request after rejection.',
    '02': 'Approve or reject pending account requests and record the decision. Approval atomically creates an active member-role account and linked profile; rejection requires a reason. Clear the finalized request password hash.',
    '03': 'Authenticate active accounts using email/password and enforce current role permissions. Use signed, expiring authentication; logout clears the browser cookie and inactive accounts cannot access protected functions.',
    '05': 'Assign fixed roles and deactivate/reactivate accounts with reasons. Block inactive access, apply future-registration cancellation rules, preserve history and protect the last active administrator; do not automatically restore seats or promote participants.',
    '06': 'Display the member’s account, academic and membership details. Officers handle profile corrections; profile-picture uploads are not included and password changes remain deferred.',
    '07': 'Maintain approved member profiles, academic/contact details, membership type and active status. Validate student-number corrections, record reasons and apply future-registration rules when membership becomes inactive without rewriting historical participation.',
    '08': 'Create and edit unique event-type names and descriptions. Activate or deactivate types, allow only active types for new selections and preserve historical event references.',
    '09': 'Maintain venue details, capacity, active status and a required photo. Validate capacity changes against linked draft/published events and preserve historical references; required photos may be replaced, not removed without replacement.',
    '10': 'Provide officer/admin JPEG/PNG uploads up to 2 MiB for required venue photos and event covers required before publication. Validate permissions/content, use persistent local paths and safely replace files; no user profile pictures.',
    '11': 'Create event drafts with schedule, venue, type, capacity, deadline and cover. Validate master records, capacity and dates; publication requires a cover and future deadline, makes the event visible and immediately opens signup.',
    '12': 'Display signup as open before the registration deadline and closed at or after it, using labelled local time. Reject late submissions server-side without changing the published event status.',
    '13': 'Allow all validated draft edits. After publication, permit only title, description and cover replacement; lock operational details, retain required images and make completed/cancelled events read-only.',
    '14': 'Cancel eligible events with a reason, cancel active registrations and invalidate QR access without promotion. Block cancellation after valid attendance or issuance; explicitly completing a concluded event closes check-in and enables certificate review.',
    '15': 'Browse published events and submit one pending request per member/event before the deadline. Validate eligibility, capture academic snapshots and allow requests for full events with a waitlist warning; no reapplication after cancellation.',
    '16': 'Approve or decline pending requests before event start, including after signup closes. Confirm only with capacity and waitlist priority respected; otherwise waitlist. Record review details and generate QR credentials only on confirmation.',
    '17': 'Allow members to cancel before event start without valid attendance, even after signup closes. Require confirmation, record the cancellation, invalidate the QR and release any seat; no reapplication or automatic promotion.',
    '18': 'Let officers explicitly promote the next eligible reviewed waitlisted participant before event start. Apply review-time/ID priority, recheck capacity transactionally and record confirmation with QR access; never promote automatically.',
    '19': 'Stop new signup at the deadline and reviews, promotions and member cancellations at event start. Retain unresolved pending/waitlisted requests as non-actionable history, exclude them from no-shows and enforce cutoffs server-side.',
    '20': 'Display or download the confirmed participant’s event-specific QR as an on-demand PNG. Saved copies work offline for members, but officers validate eligibility online; cancelled or unconfirmed registrations have no usable QR.',
    '21': 'Manually control check-in for published events and record arrivals through QR scanning or explicit manual lookup/check-in. Validate event, confirmation and eligibility; timestamp attendance, prevent duplicates and display participant identity.',
    '22': 'Invalidate mistaken attendance with a reason while preserving its history. Exclude it from reports and unissued eligibility; block corrections after certificate issuance and do not permit re-check-in after invalidation.',
    '23': 'Show personal attendance and check-in details within My Events, clearly identifying corrected records. Restrict access to authorized personal information; members cannot record or invalidate their own attendance.',
    '24': 'Review valid attendees of completed events and approve or decline certificate eligibility with reviewer/time and a decline reason. Recheck attendance and permissions; declined decisions cannot reopen, and issued decisions cannot change.',
    '25': 'Generate and retain one fixed-template PDF per approved eligible participant, preserving unique number, snapshots and issuer details. Prevent duplicate issuance, handle file/database failures and provide private authorized downloads without rewriting issued PDFs.',
    '26': 'Show participants/statuses, confirmed and attendee counts, attendance rate, completed-event no-shows and attendee course/year distribution. Use signup snapshots and full-dataset totals; exclude unconfirmed/cancelled requests from expected attendance and defer report exports.',
    '28': 'Provide a basic read-only activity list showing actor, action, affected record, time and reason. Preserve append-only audit history and relevant stored changes without credentials/tokens; defer advanced filters and field-comparison screens.',
}
groups=[('Authentication','Member 1',['01','02','03']),('Master Data','Member 2',['06','07','08','09','10']),('Events and Registration','Member 3',['11','12','13','14','15','16','17','18','19']),('Attendance and QR Codes','Member 4',['20','21','22','23']),('Certificates and Reports','Member 5',['24','25','26']),('Administration','Member 1',['05','28'])]
dependencies={
    '01':'Shared schema; account validation/hash service.',
    '02':'US-01; agreed user/member contract with Member 2.',
    '03':'Setup admin or US-02; role/access helpers.',
    '06':'US-03, US-02; linked user/member query.',
    '07':'US-02/03; Member 3 cancellation service for inactivity.',
    '08':'US-03; unique names and active-state validation.',
    '09':'US-03/10; capacity contract with Member 3.',
    '10':'US-03; persistent-storage config; event integration by Member 3.',
    '11':'US-08/09/10; active masters and required cover.',
    '12':'US-11; shared server clock/timezone convention.',
    '13':'US-11; shared upload service and event edit guards.',
    '14':'US-11/16; attendance and certificate guard contracts (Members 4/5).',
    '15':'US-02/07/11/12; eligibility and unique member/event pair.',
    '16':'US-15; transactional capacity/queue service; token contract with Member 4.',
    '17':'US-15/16; attendance guard from Member 4.',
    '18':'US-16/17; full-queue transactional allocation.',
    '19':'US-12/16/17/18; server time guards.',
    '20':'US-16/18; token contract; My Events shell from Member 3.',
    '21':'US-11/16/20; event control and attendance contracts.',
    '22':'US-21; issued-state/eligibility contract with Member 5.',
    '23':'US-21/22; My Events shell from Member 3.',
    '24':'US-14/21/22; valid-attendee query; attendee screen from Member 4.',
    '25':'US-24; shared storage; Member 3/4 screen integrations.',
    '26':'US-14/15/21/22; snapshots and attendance definitions.',
    '05':'US-03; Member 3 future-registration cancellation service.',
    '28':'Audit contract and events from all slice owners.',
}
# Presentation IDs are independent of story IDs and sequential in display order
# within each prefix. Deferred work does not reserve a numbered WBS slot.
WBS_CODES = {'01':'AUTH-01','02':'AUTH-02','03':'AUTH-03',
             '05':'ADM-01','06':'MD-01','07':'MD-02','08':'MD-03','09':'MD-04','10':'MD-05',
             '11':'EVT-01','12':'EVT-02','13':'EVT-03','14':'EVT-04',
             '15':'REG-01','16':'REG-02','17':'REG-03','18':'REG-04','19':'REG-05',
             '20':'QR-01','21':'ATT-01','22':'ATT-02','23':'ATT-03',
             '24':'CERT-01','25':'CERT-02','26':'REP-01','28':'ADM-02'}
WBS_ACTORS = {'01':'Student','02':'Officer/Admin','03':'All Accounts','05':'Admin',
              '06':'Member','07':'Officer/Admin','08':'Officer/Admin','09':'Officer/Admin',
              '10':'Officer/Admin',
              '11':'Officer/Admin','12':'Member; System','13':'Officer/Admin','14':'Officer/Admin',
              '15':'Member','16':'Officer/Admin','17':'Member','18':'Officer/Admin','19':'System',
              '20':'Confirmed Participant','21':'Officer/Admin','22':'Officer/Admin','23':'Member',
              '24':'Officer/Admin','25':'Officer/Admin (issue); Owner/Management (download)',
              '26':'Officer/Admin','28':'Admin'}
PAGINATED = {'02','05','07','08','09','11','15','16','17','21','22','23','24','25','26','28'}

def wbs_references(text):
    text = text.replace('US-27 retention', 'Historical retention').replace('US-29 pagination', 'List pagination')
    text = text.replace('US-04 is reserved and deferred; remaining story IDs are unchanged.', 'Password change remains deferred without a reserved WBS number; story IDs in the user-story document remain unchanged.')
    text = text.replace('(US-04)', '').replace('US-04 password changes', 'Password changes')
    def convert(match):
        return ', '.join(WBS_CODES[n] for n in match.group(1).split('/'))
    return re.sub(r'US-(\d{2}(?:/\d{2})*)', convert, text)

def wbs_predecessors(handoff):
    refs = list(dict.fromkeys(re.findall(r'\b(?:AUTH|MD|EVT|REG|ATT|QR|CERT|REP|ADM)-\d{2}\b', handoff)))
    if handoff.startswith('Setup admin or AUTH-02'):
        return 'Setup admin OR AUTH-02'
    return ', '.join(refs) if refs else '— (see handoff)'

flow=[p('AttendTa — Vertical-Slice Delivery WBS','BrandTitle'),p('Five owner packages; six functional headings','BrandHeading'),p('Revised MVP v2.1 | User profile pictures removed; other agreed scope unchanged. Original submission deadline: October 7, 2026 (historical). Owners are assignment placeholders. Estimates remain TBD. Password changes remain deferred without a reserved WBS number.','SmallText'),p('Task labels use module code — application actor: task, numbered consecutively within each prefix in displayed order. Owner identifies the teammate building it, not access permission. Member/participant actions are also available to officers/admins with eligible member profiles under the same ownership rules. System means server enforcement, not a new privileged login. Pagination is included in necessary lists, with no separate SHR module/task.','SmallText')]
owner_rows=[[p(x) for x in ['OWNER','PACKAGE','END-TO-END RESPONSIBILITY']]]
packages=[('Member 1','Accounts and Administration','Authentication + Administration; authorization/audit infrastructure and admin setup.'),('Member 2','Membership and Master Data','Members, types, venues/photos; shared event/venue image storage and pagination components.'),('Member 3','Events and Registration','Events/covers, signup, review, cancellation, capacity, waitlist, cutoffs; My Events shell.'),('Member 4','Attendance and QR','QR render/download/scan, manual fallback, controls, correction, attendance sections and attendee screen shell.'),('Member 5','Certificates and Reports','Eligibility, fixed-template PDF issuance/private downloads, participation reports and certificate screen sections.')]
for owner,package,scope in packages: owner_rows.append([p(owner),p(package),p(scope)])
ownership_table=Table(owner_rows,colWidths=[70,180,508],repeatRows=1,hAlign='LEFT')
ownership_table.setStyle(TableStyle([('GRID',(0,0),(-1,-1),.4,colors.HexColor('#b4a7a7')),('BACKGROUND',(0,0),(-1,0),colors.HexColor('#eedbda')),('VALIGN',(0,0),(-1,-1),'TOP'),('TOPPADDING',(0,0),(-1,-1),8),('BOTTOMPADDING',(0,0),(-1,-1),8)]))
flow += [ownership_table,Spacer(1,12),p('Definition of done: every slice includes its database/query changes, service logic, MVC screens, validation/authorization, audit integration and tests. Deliver a demonstrable result, not isolated frontend/backend layers. Members review schema/service contracts together first.','SmallText'),p('Member 3 carries the highest lifecycle/concurrency risk. Estimate remaining work before assuming equal workload. Member 2 builds shared components; each consumer integrates them. All five own incremental integration and final journey testing. Historical retention and necessary list pagination are included within the functional tasks, not separate modules or estimates.','SmallText')]
for gi,(name,owner,indices) in enumerate(groups):
    flow.append(PageBreak())
    flow += [p('AttendTa — Work Breakdown Structure','BrandTitle'),p(f'{name} — {owner}','BrandHeading'),p('Concise delivery summaries; complete acceptance criteria remain in AttendTa-User-Stories.md and AttendTa-System-Specification.md. Each slice includes database-to-screen behavior and tests. Agree handoff contracts early; estimates include integration without double-counting shared screens.','SmallText')]
    rows=[[p(t) for t in ['TASK / ACTOR','DESCRIPTION / DEMONSTRABLE RESULT','OWNER','PREDECESSORS','DEPENDS ON / HANDOFF','UC MAPPING','EST. MD']]]
    for i in indices:
        sid,title,actor,goal,criteria=next(story for story in STORIES if story[0] == i)
        mapping='Authentication' if sid in ('01','02','03') else 'Administration' if sid in ('05','28') else 'Reports' if sid=='26' else 'Master Data' if sid in ('06','07','08','09','10') else 'Transactions'
        if sid=='10': mapping='Master Data; Transactions (covers)'
        handoff = wbs_references(dependencies[sid])
        description = WBS_DESCRIPTIONS[sid]
        if sid in PAGINATED:
            description += ' Use 25-row pagination for list results.' if sid!='26' else ' Paginate detail rows at 25; retain full-dataset totals.'
        rows.append([p(f'{WBS_CODES[sid]} — {WBS_ACTORS[sid]}: {title}'),p(description),p(owner),p(wbs_predecessors(handoff)),p(handoff),p(mapping),p('TBD')])
    table=Table(rows,colWidths=[148,235,48,90,125,72,40],repeatRows=1,hAlign='LEFT')
    table.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,0),colors.HexColor('#eedbda')),('GRID',(0,0),(-1,-1),.4,colors.HexColor('#b4a7a7')),('VALIGN',(0,0),(-1,-1),'TOP'),('LEFTPADDING',(0,0),(-1,-1),7),('RIGHTPADDING',(0,0),(-1,-1),7),('TOPPADDING',(0,0),(-1,-1),8),('BOTTOMPADDING',(0,0),(-1,-1),8),('ROWBACKGROUNDS',(0,1),(-1,-1),[colors.white,colors.HexColor('#faf3f3')])]))
    flow.append(table)
flow.append(PageBreak()); flow.append(p('Shared Rules and Estimation','BrandTitle'))
flow.append(p('Cross-cutting delivery work — no additional functional module','BrandHeading'))
flow.append(p('Historical retention is included in each slice; Member 1 coordinates identity/access rules. Pagination is included in applicable list tasks at 25 rows per page with Previous/Next; Member 2 provides reusable helpers and each owner integrates/tests their lists, with no SHR task or separate pagination estimate. Setup/deployment: Member 1 leads lookup/admin setup, Member 2 storage/backups, each owner supplies constraints/indexes and all five perform integration/security/boundary tests and demonstration. Estimates TBD; avoid double-counting common services.','SmallText'))
for line in RULES.splitlines():
    line = wbs_references(line)
    if line.startswith('## '): flow.append(p(line[3:],'BrandHeading'))
    elif line.startswith('- '): flow.append(p(line[2:].replace('`',''),'SmallText'))
    elif line.strip(): flow.append(p(line,'SmallText'))
flow.append(p('Estimation and authority','BrandHeading'))
flow.append(p('All MD and total MM estimates remain TBD. Estimate remaining database, UI, services, storage, integration, testing and deployment work; avoid double-counting shared services. State working days/month before converting MD to MM. UC is the requirements baseline; added QR/image/PDF details and lifecycle policies are agreed refinements, not claims that UC explicitly mandates them. The prior specification and November 12 target describe the older baseline.','SmallText'))
def footer(canvas,doc):
    canvas.setFont('AttendSans',8); canvas.setFillColor(colors.HexColor('#666666'))
    canvas.drawString(24,18,'AttendTa | MVP v2.1 | Vertical slices | Five owners | Estimates pending')
    canvas.drawRightString(818,18,f'Page {doc.page}')
doc=SimpleDocTemplate(str(OUT/'wbs-mvp.pdf'),pagesize=landscape(A4),rightMargin=24,leftMargin=24,topMargin=26,bottomMargin=36,title='AttendTa — Revised MVP WBS v2.1',author='AttendTa')
def export_wbs_markdown(flow, destination):
    """Export the actual PDF flow before ReportLab consumes it; no PDF line wraps."""
    def plain(value):
        if isinstance(value, Paragraph):
            return ' '.join(value.getPlainText().split())
        return ' '.join(str(value).split())

    def cell(value):
        return plain(value).replace('|', '\\|')

    lines = ['# AttendTa — Vertical-Slice Delivery WBS', '',
             'Markdown companion to `wbs-mvp.pdf`; generated from the same content. Each description is one continuous paragraph without PDF layout line breaks. Copy a rendered description, or enter cell-edit mode in Excel before pasting an individual description. Raw Markdown table rows contain pipe separators, not Excel tab separators.', '',
             '**Predecessors:** module-based task references from Depends On / Handoff. These are integration prerequisites, not a fully estimated finish-to-start schedule. Non-task setup and service contracts remain in the handoff notes; “OR” is an alternative. Agree contracts early to permit parallel work. Task actors identify application access; owners identify development assignments. Story IDs remain unchanged in the separate user-story document.', '']
    for item in flow:
        if isinstance(item, Paragraph):
            text = plain(item)
            if text == 'AttendTa — Vertical-Slice Delivery WBS' or text == 'AttendTa — Work Breakdown Structure':
                continue
            if item.style.name == 'BrandTitle':
                lines.extend(['## ' + text, ''])
            elif item.style.name == 'BrandHeading':
                lines.extend(['### ' + text, ''])
            else:
                lines.extend([text, ''])
        elif isinstance(item, Table):
            rows = [[cell(v) for v in row] for row in item._cellvalues]
            lines.append('| ' + ' | '.join(rows[0]) + ' |')
            lines.append('| ' + ' | '.join('---' for _ in rows[0]) + ' |')
            for row in rows[1:]:
                lines.append('| ' + ' | '.join(row) + ' |')
            lines.append('')
    destination.write_text('\n'.join(lines), encoding='utf-8')

export_wbs_markdown(flow, OUT / 'wbs-mvp.md')
doc.build(flow,onFirstPage=footer,onLaterPages=footer)
print(f'Generated five revised deliverables and the WBS Markdown companion in {OUT}')
