---
id: 12
type: story
title: "Final frontend walkthrough targeted refactor and Impeccable polish"
parent: epic-release-acceptance
covers: [R2, R6]
after: [11]
hitl: false
risk: medium
tracker_id: "153"
remote: "https://github.com/rainyDayssss/AttendTa/issues/153"
tracker_status: backlog
---

# Final frontend walkthrough targeted refactor and Impeccable polish

## Description

Implement the replacement visual world recorded in root `DESIGN.md` and walk the result. Every route, dialog, field, validation rule, permission, state transition and copy string stays fixed. The superseded `docs/design.md` identity is an anti-reference, not a constraint. After the build, walk all 20 core screens at desktop, 1024, 768 and 390px plus 200% zoom, and fix concrete presentation, usability, keyboard and responsive findings.

## Requirements

Desktop, 1024, 768 and 390px walkthrough evidence shows no dead-end required routes, hidden primary actions or demo-data outcomes; keyboard/dialog and 200%-zoom checks pass; all critique findings are fixed or recorded as explicit release blockers. Zero functional diff outside the owner-approved Event dialog conversion. `dotnet build` clean, `npm run css:build` regenerated.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-release-acceptance/epic-release-acceptance.md
- `DESIGN.md` at the repository root — visual authority ("The Workspace Standard"). `docs/design.md` is superseded and kept only as an anti-reference.
- _bmad-output/initiative-attendta/delivery-queue.md
- _bmad-output/initiative-attendta/finale-test-cases.md

## Notes

- Frontend only: impeccable is required for implementation, rendered critique and final polish; inspect representative desktop/1024/768/390px surfaces and critical keyboard/zoom states. Use manual evidence rather than introducing broad screenshot automation in the timebox.

### Session C carry-over — list archetype

Session C reskinned the seven list screens (Member, EventType, Venue, Event,
AdminAccounts, ActivityLog, AccountRequest) plus `_Pager` and `_ResultCount`.
These gaps were found there and are carried into this walkthrough. Items 1–2 are
owner decisions; 3–6 are implementation work; 7 is a Session G obligation.

**Owner decisions (not yet signed off)**

1. **Duplicate event action.** Event rows carry five actions — Cover, View
   dossier, Edit, Cancel, Publish/Manage event — and "View dossier" and "Manage
   event" open the same page. Decide whether to collapse them into one dossier
   action. This is a behavior change, not a reskin, so it needs explicit owner
   sign-off before any edit.
2. **Missing loading states.** Event, AdminAccounts, ActivityLog and
   AccountRequest have no loading state (no `data-list-status` /
   `list-feedback.js`). Only Member, EventType and Venue have it. Decide whether
   to add it to the other four.

**Implementation work**

3. **Sticky edge columns — done.** Added a `.data-table--sticky-edges` modifier
   that pins the first and last column at the same `≤1023px` breakpoint Member
   uses, without inheriting `.roster-table`'s `nowrap` and `1180px` floor.
   Applied to Event, AccountRequest and all three Venue tables. Verified in the
   running app: first and last cells compute `position: sticky`, and the Event
   table scrolls 311px and AccountRequest 90px with the edges pinned.
4. **Result-count placement — done.** The rule is now written into root
   `DESIGN.md` under *Tables / Rosters*: pager below the table, count directly
   below the pager, real `TotalCount`, never above the table. AccountRequest now
   uses `_ResultCount` instead of its hand-rolled paragraph. MyEvents, CheckIn
   and Reports still render the count first; those belong to Sessions E and F,
   not this archetype.
5. **AccountRequest double empty state — done.** The hand-rolled `aria-live`
   paragraph was replaced by `_ResultCount`, which renders nothing at zero, so
   the table row is the only empty state. Side effect worth knowing: an empty
   result no longer gets a live-region announcement, which matches Member.
6. **Event empty-state copy — done.** Now reads "No events yet. Use "Create
   draft event" to create the first one.", matching the EventType and Venue
   pattern.

**Session G obligation**

7. **Visual walkthrough — partial.** An authenticated walkthrough now exists,
   but only at the embedded browser's fixed 1296px viewport (`outerWidth` is 0,
   so `resizeTo` cannot reach the breakpoints) and without screenshots, which
   need a visible desktop window. Verified at that width: all seven lists
   render with zero console errors, zero page-level horizontal overflow, and a
   result count on every screen; the AccountRequest empty state renders exactly
   one message. Still unverified: 1024, 768, 390 and 200% zoom, and any visual
   judgement that needs a screenshot. Session G must finish those.

Re-checked against the working tree and the running app after Session C. Items 1
and 2 remain open owner decisions; 3, 4, 5 and 6 are applied; 7 is partly
closed. `dotnet build`, `npm run css:build` and a signed-in pass over all seven
lists pass. Evidence anchors: `Views/Event/Index.cshtml` (actions, empty copy),
`Views/{Member,EventType,Venue}/Index.cshtml` (the only three with
`data-list-status`), `Styles/app.css` `.roster-table` and
`.data-table--sticky-edges`, `Views/AccountRequest/Index.cshtml` (pager,
`_ResultCount`, empty row).

> Correction (Session G, 2026-11-10): the sticky modifier named in item 3 above
> is `.table-sticky` in the shipped CSS, not `.data-table--sticky-edges`. The
> Session C note recorded a working name that Sessions D–F replaced.

---

## Walkthrough evidence — Session G, 2026-11-10

### Method

- Build under test: `cc8e474` plus the Session G working-tree fixes listed
  below. The app was rebuilt (`dotnet build`, 0 warnings, 0 errors) and
  restarted with `--no-build` against `AttendTaMVP_Test`.
- Widths: **desktop = 1233px** (the embedded browser window's fixed width;
  `resizeTo` cannot reach a wider viewport), **1024**, **768**, **390**, and
  **200% zoom**. 200% zoom is measured as a **616px CSS viewport** (1233 / 2),
  which is what a 200% zoom of the desktop window produces.
- 1024/768/390/616 were exercised by loading each route in a same-origin iframe
  sized to the target width, so media queries resolve against the real target
  viewport. Desktop was exercised in the tab itself.
- Per route and width the harness recorded: document `scrollWidth - clientWidth`
  (page-level horizontal overflow), every element whose right edge passed the
  viewport outside a scroll container, the visible `.button-primary` set and its
  right edge, the page heading, and whether the rail was rendered.
- **Screenshots were unavailable**: `browser.screenshot` requires a visible
  desktop window and the window was not visible in this session. Visual
  judgement therefore rests on computed geometry, computed styles and the
  design-system conformance scan, not on pixels. A human eyeball pass is still
  worth doing before the demo.
- **Real key events could not be injected** (same cause). The record dialog's
  Tab containment and close-to-trigger focus return were verified
  programmatically; Escape itself was not exercised this session.

### Result matrix

Every row was checked at 1233, 1024, 768, 390 and 616. "Pass" means: 200 OK,
zero page-level horizontal overflow, no element escaping the viewport outside a
scroll container, the page heading present, and every primary action inside the
viewport.

| Screen | Route used | Role | Result |
|---|---|---|---|
| Sign in | `/Account/Login` | public | Pass |
| Request membership | `/AccountRequest/Request` | public | Pass |
| Request submitted | `/AccountRequest/Submitted` | public | Pass |
| Home | `/Home/Index` | admin | Pass |
| Home | `/Home/Index` | officer | Pass |
| Home | `/Home/Index` | member | Pass |
| Access denied | `/Passes` as admin, `/CheckIn` as member | admin / member | Pass |
| Members list | `/Member` | admin | Pass |
| Event types | `/EventType` | admin | Pass — sticky edges added |
| Venues | `/Venue` | admin | Pass |
| Events | `/Event` | admin | Pass |
| Accounts | `/admin/accounts` | admin | Pass — sticky edges added |
| Activity log | `/admin/auditlogs` | admin | Pass — sticky first column added |
| Account requests | `/AccountRequest` | admin | Pass |
| Event create / edit | `/Event/Create`, `/Event/Edit?id=4` | admin | Pass |
| Event type create / edit | `/EventType/Create`, `/EventType/Edit?id=1` | admin | Pass |
| Venue create / edit | `/Venue/Create`, `/Venue/Edit?id=1` | admin | Pass |
| Member edit | `/Member/Edit/4` | admin | Pass |
| Event dossier | `/Event/Details?id=3`, `/Event/Details?id=4` | admin | Pass |
| Participation report | `/Reports/Participation?eventId=3` | admin / officer | Pass — sticky first column added |
| Registration review | `/RegistrationReview/Index`, `?status=confirmed` | admin | Pass |
| Certificate ledger | `/Certificates` | admin / officer | Pass |
| Certificate viewer | `/Certificates/Viewer/1` | admin | Pass |
| Cancel event | `/Event/Cancel?id=4` | admin | Pass |
| Event cover | `/Event/Cover?id=1` | admin | Pass |
| Check-in | `/CheckIn` | officer | Pass — sticky edges added |
| Member event feed | `/MemberEvents` | member | Pass |
| Member event detail | `/MemberEvents/Details?id=3` | member | Pass |
| Profile | `/Profile` | member | Pass |
| My Events — registrations | `/MyEvents/Registrations` | member | Pass |
| My Events — attendance | `/MyEvents/Attendance` | member | Pass |
| My Events — certificates | `/MyEvents/Certificates` | member | Pass |
| Door passes | `/Passes` | member | Pass — empty state ("No active passes") |
| Certificate unavailable | `/MyEvents/Certificates/999999/Pdf` | member | Pass |
| Shared error | — | — | **Unreachable** — no exception-handler pipeline is wired |

Route corrections found while walking (documentation, not app defects):
`/AdminAccounts` and `/ActivityLog` are 404; the real routes are
`/admin/accounts` and `/admin/auditlogs`. `/Member/Edit` takes a path segment
(`/Member/Edit/4`), not `?id=`.

### Dead-end crawl

90 distinct GET links and form actions were harvested from 12 authenticated
screens and requested. Every non-2xx response was a **405 on a POST-only form
action** (`/Account/Logout`, `/Event/Publish`, `/Venue/ToggleActive`,
`/CheckIn/Scan`, `/RegistrationReview/Approve`, and so on). **No dead ends.**

### Design-system conformance scan

Computed styles were scanned on 15 routes at 1233 and 390 for the superseded
palette (`#F8FAFC`, `#0B1120`, `#090D16`, `#D97706`, `#059669`, `#DC2626`), for
`Outfit`/`JetBrains Mono`/`Cinzel`/`monospace`, for 10px and 16px corner radii,
and for gradients. **Zero hits on every route.**

Measured against `DESIGN.md`: rail 256px, top bar 64px, table head 38px, row
48px, panel 8px radius with a 1px border and no shadow, button 4px radius at
36px, body Inter on `#F5F5F6`, table heads 10px uppercase, cells 13px. All
match. The only `box-shadow` values outside dialogs are the 1px
`1px 0 0 var(--color-border)` sticky-column edges, which draw a border rather
than a lift.

### Fixes applied in this session

1. **Dialogs were not centered.** `.task-dialog`, `.app-dialog` and
   `.confirm-dialog` relied on the UA `margin: auto` for modal dialogs, which
   Tailwind preflight's `* { margin: 0 }` overrode — every dossier, certificate
   and access dialog rendered at the viewport's top-left corner. Added
   `margin: auto` to all three in `Styles/app.css`. Verified: the dossier
   dialogs now center at desktop (x=377 in 1233) and at 390 (x=17 in a 375px
   content viewport), stay inside the viewport, and keep focus inside.
   The record dialog was already centered because `record-dialog.css` sets
   `margin: auto` explicitly.
2. **Wide tables lost their identity column at 390px.** `EventType`,
   `CheckIn` (both tables), `Event/Details` and `AdminAccounts` overflow their
   `.table-scroll` container by 157–376px at 390 and had no pinned column.
   Added `table-sticky` (first + last column), matching Member, Venue, Event and
   AccountRequest. Verified at 390: first and last cells keep their
   `getBoundingClientRect` across a 300px scroll.
3. **Two tables whose last column is not an action** (`Reports` — last column
   "Check-in time"; `ActivityLog` — last column "Reason") would have pinned a
   long text column over the table. Added a `.table-sticky-first` modifier that
   pins only the identifying column and applied it to those two. Verified at
   390: the first cell stays pinned, the last does not.
4. **`AdminAccounts` pinned away its own content.** With first + last pinned at
   390 the table left ~25px of reachable middle, so email and role were
   effectively unreachable. Moved it to `.table-sticky-first`, which keeps the
   name pinned and returns ~176px of scrolling content.
5. **Task-dialog close button was a 32px borderless glyph.** `DESIGN.md`
   requires a 1px border on every interactive control and 44px targets on touch
   surfaces. `.dialog-close` is now 36px with a 1px border and 4px radius,
   matching `.record-dialog-close`, and both are added to the `pointer: coarse`
   44px rule.
6. **`ActivityLog` timestamps were invisible to screen readers.** The `<time>`
   carried `aria-label="Occurred at in Asia/Manila"`, which replaces the
   element's accessible name — a reader announced the label instead of the
   time. Removed; the visible Manila timestamp is now the accessible name.
7. **Task dialogs used `100vh` where the record dialog uses `100dvh`.**
   Aligned to `100dvh` so the dialog height accounts for a collapsing mobile
   URL bar.
8. **Raw millisecond timestamps in the published-event locked summary.**
   `Event/_Form.cshtml` printed `yyyy-MM-dd HH:mm:ss.fff` under a label that
   says "(Asia/Manila)". Now `yyyy-MM-dd HH:mm`, matching the app's display
   format. The value is already Manila wall-clock here, so
   `ManilaTimeFormatter.Format` would have double-converted it.

### Critique

`/impeccable critique` ran on the six changed table surfaces and the dialog
surfaces with two isolated assessments: a design review and the bundled
detector. The detector scanned all six views and returned **zero findings**
(exit 0, verified against a control file that does trigger rules, so the clean
result is not a file-type blind spot). Browser overlay was skipped because the
desktop window was not visible.

Accepted findings, fixed above: the `AdminAccounts` pinning (P1), the
`.dialog-close` target and border (P3), the `ActivityLog` accessible name, and
the `dvh` mismatch.

Findings rejected after checking the source, recorded here so they are not
re-litigated:

- **"`Event/Details` row `Approve` renders white-on-white."** False.
  `.row-actions .button` sets `color: var(--color-cobalt)`, so the row action
  renders as a cobalt text link with a transparent ground. Measured on the
  sibling `/Event` row `Publish`, which uses the same classes: `background:
  rgba(0,0,0,0)`, `color: rgb(29,78,216)`.
- **"Sticky cells break row hover."** False. `.data-table tbody tr:hover td`
  scores (0,2,3); `.table-sticky td:first-child` scores (0,2,1). The hover rule
  wins on specificity regardless of source order.
- **"`ActivityLog` needs a filter."** That is a service-side filter — a
  functional change, out of scope for this ticket. Recorded as an owner
  decision.
- **"`Reports` should reorder its columns."** Reordering changes the reading
  order of a report, which is an information-architecture change rather than a
  reskin. `.table-sticky-first` stays; flagged below for the owner.
- **"Hidden scanner panels may still render their primary."** Verified: the
  inactive panels carry `hidden` and compute `display: none`.


### Verification

- `dotnet build "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"` — succeeded,
  0 warnings, 0 errors.
- `npm run css:build` — regenerated `wwwroot/css/app.css`.
- `npm run test:js` — 2/2 pass.

### Release blockers and recorded limitations

1. **No screenshot evidence.** The desktop window was not visible, so
   `browser.screenshot` failed for every attempt. The walkthrough is
   measurement-based. A human visual pass of the demo surface is still
   recommended before 8.15.
2. **Escape was not exercised with a real keypress** (no key injection in this
   environment). The record dialog's `cancel` handler is unchanged from
   Session F, where Escape was verified; containment and focus return were
   re-verified here.
3. **PDF and QR presentation unverified.** No certificate is issued and no
   member holds an active door pass in the demo data, so neither the certificate
   PDF nor the pass QR renders. The PDF is produced by
   `MyCertificatesController.Pdf` from service bytes, not from Razor markup, so
   the visual layer cannot change it.
4. **No list spans two pages**, so the pager is verified by markup order and by
   the `_Pager`/`_ResultCount` rendering, not by a rendered second page.
5. **Check-in success and repeat outcomes** were not exercised; only the
   closed-outcome path has real data.
6. **Owner decisions still open:** the duplicate event action ("View dossier"
   vs "Manage event"), and missing loading states on Event, AdminAccounts,
   ActivityLog and AccountRequest.
7. **`Reports/Participation` column order.** The decision-relevant columns
   (Attendance, Check-in time) sit right of a 447px overflow at 390px, and only
   the member column is pinned. Reordering them left of the fold would be a
   better report, but it changes the reading order of a report — an
   information-architecture change the owner should sign off.
8. **`ActivityLog` has no filter.** The audit trail is the screen most likely to
   need one, but filtering is a service change, so it is out of scope here.
9. **`Shared/Error` is unreachable** — no exception-handler pipeline is wired.
   Deliberate omission unless the owner wants `UseExceptionHandler`.
10. **`PassViewModel.AccentClass` is unused** since Session E deleted the accent
   rules. Removing it is a service change, so it stays.
