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
- Follow-up check-in change `5790201` removed the misleading visible pass-code tab while retaining QR scanning and manual lookup. The restarted demo showed the two intended tabs. The Release build, 55 JavaScript tests and 26 focused SQL-backed check-in tests passed; real-camera, mobile and accessibility verification remain open.
- Registration review resumed as Administrator against the isolated demo: the initial pending queue showed 11 results with a 1–5 of 11 footer; searching for a nonexistent participant produced a Reset link and “0 matching registrations,” and Reset returned to the pending queue. This is a partial check only; status/event filters, pagination, decisions, mobile and keyboard checks are not yet established.
- Registration Review pagination follow-up: with five rows per page, Next advanced from “1–5 of 11” to “6–10 of 11” and page 2 of 3. A combined status/event filter check was interrupted by navigation to another page, so its result is not established; no decision was submitted.
- Registration Review follow-up in a separate browser tab: Confirmed plus Demo 243 Event 05 produced one row with the selected event and status, and the footer showed 1–1 of 1. Reset returned to Pending and retained the selected event. An attempt to open Decline failed with a browser `UnknownVizError`; no dialog or decision was verified and no decision was submitted.
- At the current desktop browser width (1614px), Members, Event types, Venues, Events, Account requests and Accounts each rendered five data rows matching their 1–5 footer and had no document-level horizontal overflow. This is not a mobile, accessibility, visual or full filter/decision pass.
- A certificate for Issue243Member and Demo 243 Event 02 was issued through the normal workflow in the isolated demo database. The PDF endpoint returned a valid PDF; the owner reported the Issued Registry PDF preview looked good. This is provisional approval of that sample, not a mobile, accessibility or full certificate-workflow pass.
- The owner also provisionally approved Issue243Member's My Events → Certificates page after inspecting it. Mobile, accessibility, and other certificate states remain unverified.
- Follow-up verification: 58 JavaScript tests and six focused SQL-backed certificate ledger/personal-certificate integration tests passed. `git diff --check` found no whitespace errors. These results do not establish the outstanding mobile, 200% zoom, or rendered critique/polish checks.
- 2026-11-18 critique/polish follow-up: two independent source/markup assessments found no detector findings but could not access the browser. The parent reproduced two misleading search-empty states in browser snapshots. The Event Types search-empty message now distinguishes no matches from an empty database, with an SQL-backed integration regression; Release build, 58 JavaScript tests and the focused Event Type test passed. The Issued Registry search-empty state still says no certificates exist even where a search merely finds none; coordinate with the certificate owner. Account-request Decline/Rejected wording requires Auth-owner coordination. No screenshot or overlay was available; actual touch, screen-reader and full keyboard/error-state polish remain open. The owner separately reported mobile and 200% zoom looked fine on the screens inspected. See `.impeccable/critique/2026-11-18T10-33-27Z__asi-basecode-webapp-views.md`.
- Certificate owner confirmed approval: the Issued Registry and Eligibility Review now distinguish filtered no-match results from genuinely empty states. Release build, 58 JavaScript tests and the focused SQL-backed certificate ledger flow passed, including event/tab/Reset context and desktop/phone preview coverage. A crafted `filter=all` URL can still make the shared result count omit “matching”; the list's behavior is unchanged and that separate wording edge case remains open.
- Follow-up: certificate result counts now derive applied criteria from the same normalized options as the ledger, including `filter=all` and case-insensitive query keys. Retired issued-tab filters and other pages retain their behavior. Release build and 20 focused SQL-backed integration tests passed.
- Owner-approved table spacing refinement: Members unchanged; Event Types gives description more width, Venues separates right-aligned Capacity from Status, Events and Accounts use compact action columns, and account requests, certificates and Registration Review use decision-sized action columns. Registration submission date/time align to their headings. The action widths are capped by 30% at intermediate widths to avoid squeezing data more than before. Release build, CSS build and 57 JavaScript tests passed; desktop DOM measurements confirm the changed columns. Screenshot capture failed because the browser reported its window was not visible, so visual polish, 390px, 200% zoom and touch checks remain open. The detector's two Inter warnings conflict with the project's explicitly pinned font and were not applied.

## Blockers and remaining work

- Obtain owner coordination for remaining Auth, Transactions, Reports and Administration changes. The user confirmed certificate and check-in owner agreement for the timezone wording changes.
- Walk the prepared isolated demo with the disposable role accounts; add safe fixtures for any missing completed-event or other scenarios without touching existing databases.
- Run rendered Impeccable critique and polish, including screen-reader speech and touch-target verification; rerun verification after subsequent changes. Any unavailable check keeps acceptance incomplete.
