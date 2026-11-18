# Whole-app consistency sweep #243 — working evidence

## Baseline and environment

- Revision: `1f486ff77e9963e0a020d36437c8c85f5ccd1380`; Windows; .NET SDK 10.0.401.
- Approximately 3.30 GB free on C: at start. No existing database was reset or modified.
- A separate disposable `AttendTa_IT_243Demo20261118` database and private setup root were prepared after the owner approved them. The app is served at `http://localhost:5089`; role-based rendered inspection is not established by this inventory.
- This is a source inventory, not a rendered walkthrough or acceptance pass. Earlier evidence is historical only.

## Screen and role/state inventory

| Surface | Relevant audience | Current verification |
|---|---|---|
| Sign-in and membership request | Anonymous | Not rendered; normal, invalid, pending and error states unchecked |
| Campus events feed/detail | Member and eligible management users | Not rendered; empty, long content and registration states unchecked |
| My Events registrations, attendance and certificates; passes and PDF viewer | Member and linked management users | Not rendered; all tabs, empty/error and document states unchecked |
| Check-in and scanner | Management | Officer signed in and inspected Check-in; user provisionally approved the screen. The closed-event message, empty log, camera/pass-code/manual tabs and pass-code panel were observed. Actual camera, submitted scans, manual check-in, errors, mobile, zoom, focus and screen-reader checks remain incomplete. |
| Event list, dossier and roster; registration review | Management | Officer opened Registration review: search, status and event filters, one pending registration and a single accurate pagination footer were present. Filter actions, decisions, mobile and keyboard states remain unchecked; event dossier/roster remain unchecked. |
| Members, event classifications and venues | Management | Administrator opened Members: search/course/year/status controls and a one-page-at-a-time footer were present; details, editor, other roles and device/keyboard states unchecked. Classifications and venues loaded; their behavior remains unchecked. |
| Certificate ledger and viewer; participation reports | Management | Administrator opened the report and selected a published event: summary showed 8 confirmed and final-only figures as “Not final”; scope text and pagination were present. Other event states, filter behavior, roles and device/keyboard states remain unchecked. Certificate ledger loaded; review and viewer remain unchecked. |
| Membership requests, accounts, activity log | Applicable management roles | Not rendered; permission boundaries and decisions unchecked |
| Profile | Member | Not rendered; edit and error states unchecked |

For every row, desktop, 390px, 200% zoom, keyboard/focus, touch targets and actual screen-reader announcements remain unverified. Role permissions are not inferred from view names.

The owner provisionally accepted the Organization Members and Participation & Attendance Report screens based on their inspection; this does not replace the outstanding required checks or mark either surface fully passed.

## Source inventory and findings

- Shared list UI includes `_ListToolbar`, `_Pager` and `_ResultCount`; per-screen search labels, filters, Reset context, footer accuracy and status vocabulary still require the rendered walk.
- Count/normalize/fetch paging appears in account administration, request queue, activity log, events, event types, member events, members, registration review and venues; certificate ledger, personal certificates and reports also page filtered in-memory collections. Report snapshot semantics must remain distinct. This inventory is not a proof that all query paths are equivalent.
- Member and account-request repositories both implement the same BSCS/BSIT legacy-course alias expression. Translation and complete filtering semantics have not yet been tested together; no consolidation is presumed safe.
- Dialog guard, asynchronous editor baselines, busy/failure behavior, PDF cleanup and focus restoration remain to be audited.
- The account-request queue repeats range arithmetic instead of delegating to `PagedResult`; its last-item multiplication uses 32-bit arithmetic and can overflow for large totals.
- Source search found repeated `Manila time` in the certificate template, an `Asia/Manila` accessible name in the request queue, and a visible `Asia/Manila` suffix in `wwwroot/js/features/scanner.js`. The user confirmed certificate and check-in owner agreement; the non-visible accessible name remains unchanged pending Auth coordination.
- Administrator Event types inspection found that navigation and controls say “Event types” while the page heading said “Event Classifications.” The heading and document title now use the navigation vocabulary; the updated page loaded after restart. Responsive and visual verification remain outstanding.
- The account-request service normalizes the requested page against the filtered status count before fetching at its offset. Its behavior under concurrent count changes and its displayed rows have not been verified here.
- The record editor establishes its dirty baseline only after asynchronous loading, preserves failed-save content and has its own loading abort, preview cleanup, busy lock and focus handling. Those lifecycle details are not assumed equivalent to other dialogs. Existing JavaScript guard tests passed, but a full dialog audit was not completed.

## Changes and verification in this session

- Account-request queue range metadata now delegates to the canonical `PagedResult`, avoiding 32-bit overflow in the last-item calculation. New service tests cover empty, clamped and maximum-count metadata; they do not establish row-to-range agreement on a real database.
- `dotnet build ASI.Basecode.sln --configuration Release`: passed, zero warnings and errors.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release`: 411 passed, none skipped.
- `npm run test:js --prefix ASI.Basecode.WebApp`: 51 passed.
- `npm run css:build --prefix ASI.Basecode.WebApp`: passed after `npm ci`; the initial attempt failed because local dependencies were absent. `npm ci` reported three high-severity dependency advisories; they were not investigated or fixed as part of this sweep.
- Full SQL integration suite: 656 passed, zero failed or skipped, using disposable test databases and a local master connection. The runner emitted expected error logs from failure-path tests; its final result was successful. Rendered walkthrough, critique, polish, detector and timezone changes remain incomplete.
- After the Event types wording change, Release build passed and the focused `EventTypeManagementFlowTests` SQL integration test passed (1/1). The initial rebuild encountered a lock from the running demo; stopping the demo resolved it. The full suites above predate the wording change and must be rerun before completion.
- After certificate-owner agreement and the user's instruction to show only date and time, both repeated timezone labels were removed from newly generated certificates. The PDF renderer tests pass (7/7), including unchanged converted timestamps, the one-page layout and signatory; existing issued PDFs are not regenerated. Visual certificate critique and polish remain outstanding.
- After check-in owner agreement, the scanner's outcome and newly appended log rows no longer display the timezone suffix. A JavaScript regression checks the same Manila-local date and time on both surfaces; all 52 JavaScript tests passed. Rendered scanner inspection is still pending.

## Blockers and remaining work

- Obtain owner coordination for remaining Auth, Transactions, Reports and Administration changes. The user confirmed certificate and check-in owner agreement for the timezone wording changes.
- Walk the prepared isolated demo with the disposable role accounts; add safe fixtures for any missing completed-event or other scenarios without touching existing databases.
- Run rendered Impeccable critique and polish, including screen-reader speech and touch-target verification; rerun verification after subsequent changes. Any unavailable check keeps acceptance incomplete.
