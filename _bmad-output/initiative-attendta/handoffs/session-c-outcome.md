# Session C outcome — lists

Date: 2026-11-09
Status: complete for the list archetype; Session D is next.
Authority: `DESIGN.md` at the repository root. `docs/design.md` remains the anti-reference.

Read `redesign-visual-world.md` first. This file is the delta from Session C.

## What Session C delivered

### Lists
- Removed the subtitle from all seven list pages. Page heads are now title + optional primary action only.
- Added the row overflow menu: one visible primary action plus a native Popover `⋯` menu.
  - Members: Edit visible; Deactivate in menu.
  - Event types: Edit visible; Activate/Deactivate in menu.
  - Venues: Edit visible; Activate/Deactivate in menu.
  - Events: Publish (draft) / Manage event (published) / View dossier (closed) visible; Cover, View dossier, Edit, Cancel, Open/Close check-in in menu.
  - Accounts: Change role visible; Preview deactivation, Deactivate in menu.
  - Account requests: Approve + Decline stay visible; no menu.
  - Activity log: no actions.
- Removed the Photo column from the Venues table. It is now 7 columns. The photo is still required on the record and in the form.
- Added `.table-sticky` to Member, Venue, Event and AccountRequest. First and last columns stay pinned while the middle scrolls.
- Column policy: name/title floors, fixed numeric widths, date columns nowrap, actions column fixed.
- Event check-in cell is now just the status badge; the toggle moved into the menu and the audit line is a menu caption plus a `title` tooltip. The 139px Event row is gone.
- Added `_ResultCount` to Event, AdminAccounts and ActivityLog.

### Venue form/dialog
- Capacity and Location share a row.
- Current photo and the file input share a row.
- Current photo is 260×146.
- Record dialog is now 760px wide with tighter density.
- Live upload preview was removed at the owner's request. `record-dialog.js` still supports previews for any future form, but the Venue form no longer has a `[data-record-preview]` host.
- Edit Venue dialog: 760px wide, 840px tall, no scroll.
- New Venue dialog: 760px wide, 770px tall, no scroll.

## Files changed in Session C

Views:
- `Views/Member/Index.cshtml`
- `Views/EventType/Index.cshtml`
- `Views/Venue/Index.cshtml`
- `Views/Event/Index.cshtml`
- `Views/AdminAccounts/Index.cshtml`
- `Views/ActivityLog/Index.cshtml`
- `Views/AccountRequest/Index.cshtml`
- `Views/Shared/_Pager.cshtml`
- `Views/Shared/_RowMenuTrigger.cshtml` (new)
- `Views/Venue/_Form.cshtml`
- `Views/Venue/_Editor.cshtml`
- `Views/Venue/Edit.cshtml`

Styles:
- `Styles/app.css`
- `Styles/features/record-dialog.css`
- `Styles/features/my-events.css`
- `wwwroot/css/app.css` (generated)

Reviewed and unchanged: `Views/Shared/_ResultCount.cshtml`.

## Decisions and deviations

- Subtitles were removed from all lists, not kept selectively. The owner preferred density over a description on every page.
- The Venue photo is a schema requirement: `venues.image_path` is `NOT NULL` with `chk_venues_required_photo` in `database/attendta-mvp-schema.sql`. It cannot be removed from create/edit without a schema/PRD change. Only the list column was removed.
- The row menu uses the native Popover API (`popover` + `popovertarget`) with CSS anchor positioning. No new JS.
- Sticky columns were consolidated on `.table-sticky`. A concurrent edit had added `.data-table--sticky-edges`; that class and its CSS were removed.
- The record dialog width is now 760px for every record dialog (Event, EventType, Member, Venue).

## Verification

- `dotnet build "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"` — clean.
- `npm run css:build` — regenerated.
- `npm run test:js` — pass.
- `impeccable detect --json ASI.Basecode.WebApp/Views` — `[]`.
- Live checks at the current desktop width: no console errors, no page-level horizontal overflow, menus open on-screen when the trigger is visible, sticky columns pin after scroll, and the Venue edit/create dialogs do not scroll.

## Open items for Session D / G

- Draft-event Publish path was not exercised live (no draft event in the demo data); the Razor branch is build-valid.
- EventType/Venue toggle POST was not clicked, to avoid mutating demo data. The markup and `data-submitting-label` hooks are intact.
- Account requests has a 57px row (name/email wrap). Venues has a 76px row (notes wrap). Everything else is 48px.
- EventType, Accounts and Activity log do not carry `table-sticky`. They fit at the tested widths; at 390px they may scroll without pinned edges. Session G responsive check.
- The standalone `Venue/Edit` fallback page uses the same `_Form` layout. The preview is removed, so it has none either.
- A concurrent session was editing `Venue/Index`, `AccountRequest/Index` and `app.css` during Session C. If it is still running, serialize it before Session D touches shared files.

## Environment

- App: `http://localhost:5000`
- Sample admin: `demo.admin@attendta.local` / `AttendTa-Demo-2026!`
- Database: `AttendTaMVP_Test` (disposable local test DB)
- Start command:
  ```powershell
  $env:ConnectionStrings__DefaultConnection = "Server=(localdb)\MSSQLLocalDB;Database=AttendTaMVP_Test;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  $env:Common__SetupRoot = "C:\setup\projects\ASI.Basecode\root"
  $env:ASPNETCORE_URLS = "http://localhost:5000"
  cd "ASI.Basecode.WebApp\bin\Debug\net10.0"
  .\ASI.Basecode.WebApp.exe
  ```

## Git

- Branch: `feature/51-check-in-refactor-sweep` at `2cf6ac3`.
- All Session B + C changes are uncommitted. Nothing was committed, pushed, stashed, or branched.
- Before committing, branch from `upstream/dev` (per `AGENTS.md`), not from this stale branch.
