---
title: 'Clear activity history and meaningful role selection'
type: 'feature'
ticket: ''
created: '2026-11-20'
status: 'in-progress'
baseline_revision: '442ea882281bef30d3036396cb249dd6f59d91d8'
route: 'full'
route_source: 'auto'
risk: 'high'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
context: ['DESIGN.md', 'PRODUCT.md']
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Administrators see technical action and entity identifiers rather than recognizable affected records and recorded changes. The role editor also offers the existing role and reports a no-op as success.

**Approach:** Keep the activity table slim and make its details understandable using existing audit data. Offer only different roles and reject same-role submissions server-side.

## Boundaries & Constraints

**Always:** Keep Occurred, Actor and Action headings and existing Manila time formatting. Show a compact Affected record column and explain once that names reflect current records; retain existing read-only administrator authorization, paging, ordering, action-filter behavior, and audit history. Search also matches friendly action labels. Show old/new only where recorded and provide honest unavailable fallbacks. Preserve role-change transaction, lock order, last-administrator guard, antiforgery, role validation, and audit-on-success semantics.

**Never:** Rewrite historical logs, infer missing historical values, expose arbitrary JSON or secrets, modify schema, alter other modules' audit writing, or change unrelated user work. Do not add new filters or claim a display name is a historical snapshot when it is a live lookup.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Role change | Recorded member → admin | Friendly action and current account name; details show Member → Administrator and reason | Unknown account uses labeled unavailable fallback and ID |
| Event update | Recorded old/new event values | Current event title and only safe, meaningful recorded changes | Missing, malformed or partial JSON shows unavailable; no fabricated prior state |
| Related activity | Registration, attendance or certificate | Identify current participant and event when resolvable; show recorded decision/reason | Missing relation or unknown type/action retains legible fallback and ID |
| Same role | Form or crafted/stale submission selects authoritative current role | Current role not selectable; server refuses rather than claiming success | Inline actionable validation; no write or audit row |
| Other role | Valid role and reason | Existing transactional role change and one audit entry | Existing authorization and last-admin rules remain |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Repositories/ActivityLogReaderRepository.cs`, `Data/Models/ActivityLogListItem.cs`: currently page/filter logs and join actor only. Load old/new and resolve current names for the paged rows in bounded batched queries; distinguish entity ID spaces and preserve ordering and filters.
- `ASI.Basecode.Services/Services/ActivityLogReaderService.cs`, `ServiceModels/ActivityLogItemViewModel.cs`: map known action names and allowlisted audit fields to friendly display; never render raw JSON; safe fallback for unknown actions, malformed values and nulls. Existing writer uses typed snapshots and rejects sensitive keys.
- `ASI.Basecode.Resources/Constants/AuditActions.cs`, `Services/Services/` writers: action and payload shapes. `user.provisioned` is a system seed action outside constants. Role payload uses `RoleCode`; decisions often contain only status and reason.
- `ASI.Basecode.WebApp/Views/ActivityLog/Index.cshtml`, `Views/Shared/_DetailsButton.cshtml`: reuse existing menu/details and list contract.
- `ASI.Basecode.Services/Services/AccountRoleService.cs`: `GetRoleChangeAsync` currently preselects current role; `ChangeRoleAsync` silently returns on same role after locking. Reject after authoritative row lock; retain lock ordering.
- `ASI.Basecode.WebApp/Controllers/AdminAccountsController.cs`, `Views/AdminAccounts/_RoleFields.cshtml`, `_RoleEditor.cshtml`, `ChangeRole.cshtml`: shared modal/full-page role form and validation.
- `tests/ASI.Basecode.Services.Tests/AccountRoleServiceTests.cs`, `tests/ASI.Basecode.IntegrationTests/AdminRoleAssignmentTests.cs`: replace no-op success expectations and cover crafted/stale requests; `ActivityLogViewerTests.cs` covers authorization, paging and unknown rows.

## Tasks & Acceptance

**Execution:**
- [x] `ActivityLogReaderRepository.cs`, `ActivityLogListItem.cs` — fetch audit snapshots and batch-resolve current target labels for users, requests, members, events, types, venues, registrations, attendance and certificates; avoid N+1 and ID collisions.
- [x] `ActivityLogReaderService.cs`, `ActivityLogItemViewModel.cs` — format action-specific allowlisted changes, status/role labels and fallback data; unit-test matrix cases including sensitive/unknown fields and bad JSON.
- [x] `Views/ActivityLog/Index.cshtml`, `PRODUCT.md` — apply agreed slim table and shared Details without changing time, filtering or permissions; update product description.
- [x] `AccountRoleService.cs`, `AdminAccountsController.cs`, `_RoleFields.cshtml` — require an alternative role, recheck locked current role, retain submitted reason/error on both form paths.
- [x] `AccountRoleServiceTests.cs`, `AdminRoleAssignmentTests.cs`, `ActivityLogViewerTests.cs` — cover authorization, no-op rejection/no audit, valid change, representative entity identities and changes, partial/malformed data, HTML encoding, pagination and last-admin guard.
- [x] Changed Razor surfaces — run project-required Impeccable critique, polish and detector; document any inability to render or verify.

**Acceptance Criteria:**
- Given an administrator views the log, when rows are displayed, then they show Occurred, Actor, friendly Action, compact Affected record and Actions; a single note explains names are current, and details show available safe changes.
- Given an officer or anonymous visitor, when requesting the log, then existing access restrictions remain in force.
- Given a valid alternate role and reason, when submitted, then the role changes and exactly one correctly attributed audit is recorded; the last active administrator guard remains effective.
- Given a same-role or stale request, when submitted, then no role or audit change occurs and the user sees an actionable response.

## Implementation Notes

- Current-label resolution is a live lookup performed in bounded per-type batches; it never
  claims to be a historical snapshot. Audit snapshots are read only through a per-action
  allowlist, so unknown properties, contact data (`MobileNumber`) and raw JSON never reach the view.
- `ManilaTimeFormatter` moved to `ASI.Basecode.Services/Formatting` so the services layer and the
  views share one formatter; the namespace import moved in `_ViewImports.cshtml` and the three
  C# consumers.
- The Action filter dropdown uses the same friendly labels as the rows
  (`ActivityLogReaderService.LabelForAction`), so selecting a filter matches the visible vocabulary.
- The same-role guard runs after the authoritative `users` row lock and throws a field-scoped
  `Invalid` domain error; the controller re-populates the display fields, drops the no-longer-offered
  role and retains the reason on both the modal and full-page paths.
- A dedicated `ActivityLogPresentationTests` class (own isolated database) covers identities,
  allowlisted changes, malformed/partial snapshots, HTML encoding, unavailable fallbacks and
  authorization; `ActivityLogReaderServiceTests` covers the pure mapping matrix.

## Plan Change Log

- User requested a slimmer table: move the affected record exclusively to Show details instead of keeping a dedicated column.
- User subsequently chose a compact affected-record column and one shared current-name explanation instead of repeated labels.
- User approved removing technical action/actor-type detail rows, retaining record IDs, supporting friendly-action search, and clearly labeling incomplete history.
- For the approved follow-up, keep search SQL-side before count/paging, case-insensitively match partial friendly labels in addition to existing semantics, retain action-filter intersections and avoid N+1 queries or new filters. Details always show Record ID. For partial historical snapshots say “Some change details are unavailable” and distinguish recorded new values from previous values, preserving one-sided creation records. Add tests covering these cases and missing sides. Do not alter role guards, writer/schema or other workflows.
- Preserve all existing uncommitted changes and user files, including generated `VBCSCompiler/`; do not commit or push. Set TEMP/TMP and build artifacts on F:, and do not run SQL tests that create database files on C:.

## Review Triage Log

## Design Notes

Keep current-label lookups visibly distinct from historical snapshots. Allowlist fields by action/type instead of serializing arbitrary historical payloads; avoid exposing personal contact data unnecessarily. For event date values, use the existing Manila formatter where type and timezone are known.

## Verification

- `dotnet build ASI.Basecode.sln --configuration Release` — succeeds.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` — service suite passes; use documented test-host fallback if runner reports zero tests.
- `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` — SQL suite passes with `ATTENDTA_TEST_SQL` pointing to isolated local master; report if unavailable.
- `npm run test:js --prefix ASI.Basecode.WebApp` and `npm run css:build --prefix ASI.Basecode.WebApp` — pass.
- Inspect both role form paths and activity details at desktop, narrow width and keyboard focus; report if a safe running environment is unavailable.
