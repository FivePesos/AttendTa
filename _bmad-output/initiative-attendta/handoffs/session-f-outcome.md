# Session F outcome — special surfaces

Date: 2026-11-10
Status: complete for the twelve Session F screens and the shared event selector.
Authority: `DESIGN.md` at the repository root. `docs/design.md` remains the anti-reference.

Read `redesign-visual-world.md` first, then `session-e-outcome.md`. This file is the delta from
Session F, and the starting brief for Session G.

## What Session F delivered

### Home (`Home/Index`)
- The `.home-card` tile grid is gone. Each section is a `.section-head` plus one `.panel.dest-panel`
  of hairline destination rows: 13.5px/600 name, 12.5px muted description, a drawn 16px chevron.
  Rows are 62px and hover to `surface-subtle`; the whole row is the link.
- Nothing was invented: the same destinations, the same role gates, the same copy.

### Check-in (`CheckIn/Index`)
- `.scanner-outcome` lost its 3px `border-left`. It is now a 1px full border with the semantic tint
  and the heading carrying the state. Verified all four variants by computed style.
- The three mode buttons are now the segmented vocabulary (`inline-flex`, 1px container, 2px inset,
  cobalt tint on `aria-selected="true"`), not plain bordered buttons.
- `_Pager` now precedes `_ResultCount`.
- Row actions (Check in, Invalidate) are `.row-actions` text links; Invalidate is
  `button-quiet-danger`. The invalidate dialog's `×` is the drawn SVG close icon.

### Door passes (`Passes/Index`)
- Sections gained `.section-head`; empty states became `.empty-panel`. The pass card itself was
  not touched (Session E owns it).

### Participation report (`Reports/Participation`)
- KPI labels are 11.5px uppercase muted micro labels, not 1rem headings; KPI panels are 14/16
  padding with the 26px tabular figure.
- The event row reuses `.section-head`, so its title is 16px.
- `.report-breakdown-bar` radius 2px → 4px.
- `_Pager` before `_ResultCount`; the no-event state is `.empty-panel`.

### Registration review (`RegistrationReview/Index`) — the big build
- Was completely unstyled. Now: page head, a `.review-toolbar` holding the status filter and the
  promote form, and a `.panel.review-list` of hairline `.review-row`s.
- Each row: name + event link, status tag, student number and submitted time, then the decision
  forms. Approve is a cobalt text link; Decline and Cancel registration are quiet-danger links.
- Every `name`, `id`, `asp-action`, hidden input, `required`, `maxlength` and the inline reason
  inputs are unchanged. Added `_Pager` + `_ResultCount` under the list.
- Empty state is `.empty-panel`.

### Certificate ledger (`CertificateLedger/Index|Viewer`)
- Empty state's non-existent `card` class replaced with `.empty-panel`.
- Approve / Decline / Issue are row text links (Approve and Issue cobalt, Decline quiet-danger);
  `View PDF` is a row link, not a button. The three dialog submits are now `button-primary` /
  `button-danger`, which is what the deleted `.ledger-approve|decline|issue` rules were faking.
- `×` replaced with the drawn close icon.
- Count moved below the pager.
- Viewer toolbar moved from `.page-heading` to `.section-head`; its two actions sit in
  `.certificate-toolbar-actions` so the primary Download PDF is not flattened by `.row-actions`.

### Certificate unavailable (`MyCertificates/Unavailable`)
- The notice is the message block, followed by the back action. No nested panel-in-panel.

### Access denied (`Account/AccessDenied`)
- The h1 moved out of the panel into a page head, matching every other page.

### Event cancel and cover (`Event/Cancel|Cover`)
- Rebuilt as page head + facts panel (`.profile-details`) + form panel.
- Cancel keeps `EventId`, `Reason`, the validation summary and the `Reason-validation` span; the
  actions sit in `.form-actions`.
- Cover's inline `style` on the image is gone; `.cover-preview` carries it. The file input keeps
  `id="cover"`, `name="cover"` and the same `accept` list.

### Shared error (`Shared/Error`)
- Bootstrap-era `text-danger`, `<code>` and the bare heading stack replaced with a page head and two
  panels. Copy is unchanged, including the Development Mode block.
- Note: this app wires no exception-handler pipeline, so the view is not reachable at runtime. It
  was rebuilt for consistency and compiles; it could not be exercised live.

### Shared primitives
- `app.css` gained `.empty-panel` (a complete empty state in the same panel ground as the table it
  replaces) and `.login-card > :last-child { margin-bottom: 0 }`.
- `_EventSelector` is now a bordered filter bar (12/16 padding, 8px radius, 8px radius field), styled
  once in `event-selector.css`, and checked on Check-in, Reports and the ledger.

## Files changed in Session F

Views:
- `Views/Home/Index.cshtml`
- `Views/CheckIn/Index.cshtml`
- `Views/Passes/Index.cshtml`
- `Views/Reports/Participation.cshtml`
- `Views/RegistrationReview/Index.cshtml`
- `Views/CertificateLedger/Index.cshtml`
- `Views/CertificateLedger/Viewer.cshtml`
- `Views/MyCertificates/Unavailable.cshtml`
- `Views/AccountRequest/Request.cshtml` — verified, no change
- `Views/AccountRequest/Submitted.cshtml` — verified, no change
- `Views/Account/AccessDenied.cshtml`
- `Views/Event/Cancel.cshtml`
- `Views/Event/Cover.cshtml`
- `Views/Shared/Error.cshtml`

Styles:
- `Styles/app.css` (appended `.empty-panel`, `.login-card` last-child rule, two new imports)
- `Styles/features/home.css` (rewritten)
- `Styles/features/check-in.css` (rewritten)
- `Styles/features/passes.css`
- `Styles/features/reports.css` (rewritten)
- `Styles/features/certificate-ledger.css`
- `Styles/features/event-selector.css` (rewritten)
- `Styles/features/registration-review.css` (new)
- `Styles/features/event-admin.css` (new)
- `wwwroot/css/app.css` (generated)

Not touched: any controller, service, data, route, form field name, `data-*` hook, JS file, or copy
string.

## Decisions and deviations

- **`RegistrationReview` became a hairline-row queue, not a table.** The inline decline/cancel reason
  inputs make a table cell hostile; the row treatment matches `_RegistrationsTab` from Session E.
- **The review queue gained `_Pager` + `_ResultCount`.** It had a pager but no count. The count is
  the real `PagedResult.TotalCount`, so it is the system's standard table footer, not a new claim.
- **Ledger row actions are links; the dialog keeps the fills.** A ledger page of cobalt buttons is
  the wall the One Accent Rule forbids. The confirm dialog is where the single accent belongs.
- **`event-admin.css` and `registration-review.css` are new surface files.** Neither screen had one;
  app.css stays append-only and shared.
- **`Shared/Error` is rebuilt but unreachable.** No `UseExceptionHandler` is wired. Recorded rather
  than invented.
- **`RegistrationStatusDisplay.BadgeClass` is reused as-is** for the review queue's status tag. It
  maps Waitlisted to the neutral tag; that mapping lives in the service and is out of scope here.

## Verification

- `dotnet build "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"` — clean, 0 warnings, 0 errors.
- `npm run css:build` — regenerated.
- `npm run test:js` — 2/2 pass.
- Integration suite — `423 total, 0 errors, 0 failed, 0 skipped` (75s) against LocalDB `master`.
  This is the first full run since Session D; the Session E view assertions still hold.
- `impeccable detect --json` over the thirteen changed views — `[]` (regex-only for `.cshtml`, so an
  undercount).
- Live at desktop width, by computed style and DOM (no viewport control this session):
  - No console errors, no document overflow, no overflow inside `main` on any of the twelve screens.
  - No radius outside 0/4/8/999px, no border above 1px, no non-dialog shadow on any of them.
  - Home: 12 destination rows for an administrator, 4 for a member; panel padding 0; rows 62px.
  - Check-in: mode keys 30px with cobalt tint on the selected key; outcome callout 1px border on all
    four variants; count after pager; close icon is SVG.
  - Reports: KPI label 11.5px/600 uppercase muted, value 26px/600 tabular, panel 14/16, bar 4px/4px.
  - Registration review: 12 rows, 16/20 padding, 13.5px/600 name, 22px status tag, quiet-danger
    action at 24px, reason input `name="reason" required maxlength="500"` at 32px.
  - Ledger: Approve cobalt, Decline ink-secondary, View PDF a cobalt link; dialog submits cobalt/red.
  - Cancel/Cover: `.profile-details` rows, no inline style, `cover-preview` 4px radius 1px border.
  - Public request/submitted: 576px shell, 544px card, 36px inputs, 16px field rhythm.
- Fixed during verification: `.panel` was overriding `.dest-panel` and `.review-list` padding because
  the imported feature files land earlier in the components layer; both are now `.panel.x`.

## Open items for Session G

### Must be walked (no viewport control in Session F)
- 390 / 768 / 1024 / 200% zoom on: the home destination rows, the review queue's inline reason
  forms, the ledger table's card fallback, the report KPI grid, the check-in mode keys, the cover
  preview. Narrow layouts are CSS-reviewed only.
- `Shared/Error` cannot be reached: the app has no exception-handler pipeline. Decide whether that is
  a defect to record or a deliberate omission.

### States with no demo data
- Registration review `pending` is empty, so the Approve + Decline pair was verified by markup, not
  live. `confirmed` (Cancel + reason) and the empty state were exercised.
- No member currently holds a door pass, so `Passes/Index` was verified in its empty state only.
- Check-in success and repeat outcomes were verified by injecting the classes and reading computed
  styles; only the closed outcome came from a real scan.
- No list in Session F paginates with the demo data, so "pager before count" is verified by markup
  order, not by a rendered second page.

### Carried from Session D/E
- Event dialog **successful save** still not exercised live.
- Escape/focus containment not re-verified; the JS is untouched.
- Sticky columns on EventType / Accounts / ActivityLog at 390px.

### Suite
- Green as of this session (423/0/0). Re-run before the demo after any further view edit.

## Environment

- App: `http://localhost:5000` (restarted at the end of this session, `--no-build`).
- Database: `AttendTaMVP_Test` (disposable local test DB), LocalDB.
- Start command:
  ```powershell
  $env:ConnectionStrings__DefaultConnection = "Server=(localdb)\MSSQLLocalDB;Database=AttendTaMVP_Test;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  $env:Common__SetupRoot = "C:\setup\projects\ASI.Basecode\root"
  $env:ASPNETCORE_URLS = "http://localhost:5000"
  dotnet run --project "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj" --no-build
  ```
- Accounts — one password for all: `AttendTa-Demo-2026!`

  | Role | Email |
  |---|---|
  | Administrator | `temp.admin@attendta.local` |
  | Administrator (original demo) | `demo.admin@attendta.local` |
  | Officer | `temp.officer@attendta.local` |
  | Member | `temp.student@attendta.local` |

- Stop the app before `dotnet build` — the running exe locks the build output.
- Integration tests: run
  `tests\ASI.Basecode.IntegrationTests\bin\Debug\net10.0\ASI.Basecode.IntegrationTests.exe` with
  `ATTENDTA_TEST_SQL` set to LocalDB `master`. `dotnet test` on that project reports "Zero tests ran".

## Route notes for Session G

- `/RegistrationReview` alone is a 404; the route is `/RegistrationReview/Index`.
- `/Certificates` is the ledger; `/Certificates/Viewer/{id}` the viewer.
- `/MyCertificates/Unavailable` is not a route. The view is reached from
  `/MyEvents/Certificates/{id}/Pdf` when the document is unavailable (404/403/503).
- `/Passes` is member-only; an administrator gets `/Account/AccessDenied`.
- `/Event/Cancel?id=4` and `/Event/Cover?id=4` are query-string routes.

## Addendum — event edit dialog (owner-approved, same session)

The owner reviewed the Event create/edit dialog and confirmed it should stay a dialog. Two gaps were
closed after the main sweep:

- `Event/Details` now opens the same record dialog as `Event/Index`. The Edit link carries
  `data-record-open="edit-event-{id}"` with the event title as subtitle; the view sets
  `ViewData["Feature"] = "record-dialog"` and renders `_RecordDialog`. Both entry points now behave
  identically, and the dossier keeps its roster tab and scroll position after a save. The full page
  remains the no-JS and "Open full page" fallback.
- `Event/_Form` gained the required markers the shared dialog promises. Title, Event type, Venue,
  Starts, Signup deadline and Participant limit now carry `<span class="required" aria-hidden="true">*</span>`,
  matching Member, Venue and Event Type. On a published event only Title is marked, because the other
  five fields are locked and submitted as hidden inputs.
- `PRODUCT.md` now lists Event alongside Member, Event Type and Venue as a record-dialog screen.

Verified live: the dialog opens from the dossier (draft and published), Escape closes it and returns
focus to the Edit link, the URL stays on `/Event/Details`, and `/Event/Edit?id=4` still renders the
standalone page. No console errors. `dotnet build` clean, `npm run css:build` regenerated,
`npm run test:js` 2/2. No integration test references the record dialog or the event form, so the
423-test suite is unaffected.

Still open: the published-event locked summary prints raw timestamps
(`yyyy-MM-dd HH:mm:ss.fff`) instead of the Manila-formatted time used elsewhere. Pre-existing in
`_Form`, now more visible inside the dialog.

## Git

- Branch: `feature/51-check-in-refactor-sweep` at `2cf6ac3`.- All Session B + C + D + E + F changes are uncommitted. Nothing was committed, pushed, stashed, or
  branched by any of these sessions.
- Before committing, branch from `upstream/dev` per `AGENTS.md`, not from this stale branch.
- The diff still includes `EventController.cs` and two integration test files from Session D — an
  owner-approved functional change (the Event dialog conversion). Call it out in the PR description
  so the reviewer does not treat it as accidental.
