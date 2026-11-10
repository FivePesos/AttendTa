# Session E outcome — detail pages and My Events slots

Date: 2026-11-10
Status: complete for the detail/dossier archetype and the four My Events slots.
Authority: `DESIGN.md` at the repository root (amended this session — see "Decisions").
`docs/design.md` remains the anti-reference.

Read `redesign-visual-world.md` first. This file is the delta from Session E, and the starting
brief for Session F.

## What Session E delivered

### Officer event dossier (`Event/Details`)
- The raw Tailwind block (`rounded-lg border border-slate-200 bg-white p-6 mb-6`) is gone. It was
  the last screen in the app still rendering the superseded slate palette
  (`oklch(0.929 0.013 255.508)`).
- The fact sheet is a `.panel` with a labelled three-up grid: micro labels, 13.5px tabular values,
  capacity at 22px/600, a ruled description, and a check-in row that is no longer pushed to the far
  right by `.row-actions`.
- The roster heading dropped from 22px to 16px through a new `.section-head`; roster rows gained the
  26px initials tile and a 600-weight name cell; the Status cell is a tinted tag (text unchanged);
  Decline/Cancel read as ink links that turn red on hover so only Approve keeps the accent.
- Pager now precedes the count. The fake `aria-label="Breadcrumb"` wrapper became a plain back row.
  The six dialogs use the drawn SVG close icon.

### Member event feed (`MemberEvents/Index`)
- Converted to a stacked list, then **reverted to the cover-card grid at the owner's request** in the
  same session. The shipped state is the original card markup and CSS, on the new tokens.

### Member event detail (`MemberEvents/Details`)
- The 288px scrimmed hero became a 200px bordered cover band with the title in ink at Display 22px.
- The event type moved out of the badge row into a micro label, leaving two badges.
- The registration aside reorders above the description below 768px.

### Profile (`Profile/Index`)
- `.profile-details` is now hairline definition rows instead of a bordered tile per field.
- The static "managed by an officer" notice is neutral; pending ochre is reserved for state.

### Participation history (`MyEvents/Index` + slots)
- Pager before count.
- `_RegistrationsTab`: nested card → hairline row (type label, `h2` title — fixes an h1→h3 skip,
  status tag, one meta line, pass, action).
- `_PassPartial`: accent class dropped, head suppressed inside a registration row so the
  title/date/venue are not stated twice, QR 140px.
- `_CertificateLinkPartial`: View/Download are cobalt text links at 12.5px instead of a primary fill
  per row; the table gained `table-sticky`.
- `_AttendanceHistoryPartial`: reviewed, no markup change needed — it was already on-system.

### Shared primitives
- `app.css` gained `.section-head`, `.detail-back`, `.button-quiet-danger`; `.filter-tab` radius
  3px→4px; `.profile-details` restyled; dead event-card CSS was removed and then restored with the
  card revert.
- `browse-events.css`, `my-events.css`, `passes.css`, `attendance-history.css`,
  `certificate-history.css`, `event-dossier.css` rewritten or re-spaced for the above.

## Files changed in Session E

Views:
- `Views/Event/Details.cshtml`
- `Views/MemberEvents/Index.cshtml`
- `Views/MemberEvents/Details.cshtml`
- `Views/Profile/Index.cshtml`
- `Views/MyEvents/Index.cshtml`
- `Views/MyEvents/_RegistrationsTab.cshtml`
- `Views/MyEvents/_PassPartial.cshtml`
- `Views/MyEvents/_CertificateLinkPartial.cshtml`

Styles:
- `Styles/app.css`
- `Styles/features/browse-events.css`
- `Styles/features/my-events.css`
- `Styles/features/passes.css`
- `Styles/features/attendance-history.css`
- `Styles/features/certificate-history.css`
- `Styles/features/event-dossier.css`
- `wwwroot/css/app.css` (generated)

Authority and artifacts:
- `DESIGN.md` — added the owner exception for the member Events feed card grid
- `.impeccable/critique/2026-11-09T17-21-20Z__asi-basecode-webapp-views-event-details-cshtml.md`
  (new; the snapshot is effectively closed because the target file changed)

Not touched: `_AttendanceHistoryPartial.cshtml` (no change needed), any controller, service, data,
route, JS contract, field name, or copy string.

## Decisions and deviations

- **Card grid restored.** The owner preferred the cover-card feed. `DESIGN.md` now carries a dated
  exception: the member Events feed is the only card grid in the system. Do not "fix" it back to a
  list.
- **Event type is a micro label, not a badge**, on the member detail and on registration rows. Type
  is a category, not a state; the status palette stays reserved for state.
- **The pass head is suppressed inside a registration row** so the title, date and venue are not
  repeated. `Passes/Index` still renders the full pass head.
- **Certificate row actions are text links**, per the One Accent Rule. A page of issued certificates
  must not become a wall of cobalt fills.
- **Roster status keeps its exact wording** (`Pending`, `Confirmed`, `Not confirmed before cutoff`);
  only the tag treatment was added. No copy changed anywhere in this session.
- **`PassViewModel.AccentClass` is now unused.** The CSS accent rules were deleted; the service
  property remains because removing it is a service change outside this session's scope.

## Verification

- `dotnet build "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"` — clean, 0 warnings, 0 errors.
- `npm run css:build` — regenerated.
- `npm run test:js` — 2/2 pass.
- `impeccable detect --json` over the nine views — `[]` (regex-only for `.cshtml`, so an undercount).
- Live at 1233px: no console errors, no document overflow, no overflow inside `main`, no radius
  outside 0/4/8/999px, no border above 1px, no non-dialog shadow on the six member pages. The one
  flagged shadow is the 1px sticky hairline on the certificate row header.
- Fixed during verification: a 17px white gap inside the roster's bordered table (a margin on the
  table itself) and a 0px gap between the fact sheet and the roster.
- Data-* hooks verified present: `data-registration-id`, `data-event-id`, `data-certificate-id`,
  `data-dialog-open`, `data-dialog-value-id`, `data-dialog-value-summary`, `data-dialog-close`,
  `data-dialog-field`, `data-reopen*`, `data-submitting-label`.

## Open items for Session F and G

### Must be walked (no viewport control in Session E)
- 390 / 768 / 200% zoom on: the dossier fact grid, the registration rows, the certificate table, the
  feed cards. Narrow layouts are CSS-reviewed only.
- Three states have no demo data and were verified by injecting the partial's markup and reading
  computed styles, not from real records: an active door pass, an issued certificate row with
  View/Download, and a pending/waitlisted roster with Approve/Decline.

### Carried from Session D
- Event dialog **successful save** was never exercised live.
- Escape/focus containment could not be re-verified through the automation; the JS is untouched.
- The global `.notice` restyle affects list and layout flash messages.
- Sticky columns on EventType / Accounts / ActivityLog at 390px.

### Suite
- The integration suite has not been re-run since Session D. The two view-HTML assertions that touch
  this surface (`<dl class="profile-details">` and the `badge-closed` registration card) still hold
  by grep, but run the suite before the demo.

## Session F starting point (reconnaissance done in Session E)

Session F is **not started**. Nothing below has been changed.

### Routes that are not what the file names suggest
- The certificate ledger is **`/Certificates`**, not `/CertificateLedger`. Viewer is
  `/Certificates/Viewer/{id}`.
- Cancel and cover are query-string routes: **`/Event/Cancel?id=4`**, **`/Event/Cover?id=4`**.
- **`/Passes` is member-only** — an administrator gets Access denied, so walk it as a member.
- `/Event/Details` is `/Event/Details?id=4`; `/Event/{id}` also resolves.

### State of each Session F surface (measured live at 1233px)

| Screen | State | Concrete defects to fix |
|---|---|---|
| `Home/Index` | Grid of `.home-card` tiles, 2-up at 768, 3-up at 1280 | Same-size heading+text cards as the page structure — the exact pattern the world refuses. Convert to destination rows per section. |
| `CheckIn/Index` | Mostly on-system (panels, data-table, badges) | `.scanner-outcome` uses `border-left-width: 3px` (banned); `.scanner-mode` tabs are plain bordered buttons, not the segmented vocabulary; `_ResultCount` before `_Pager`; `×` glyph in the invalidate dialog. |
| `Passes/Index` | Uses the already-reskinned `_PassPartial` | Sections have bare `h2` with no `.section-head`; empty states are bare paragraphs. Do not rebuild the pass card. |
| `Reports/Participation` | Four KPI panels, status strip, two breakdown panels | KPI `<h2>` labels at 1rem instead of the 11.5px label; `_ResultCount` before `_Pager`; `.report-breakdown-bar` radius 2px; `.report-event h2` at 1.25rem. |
| `RegistrationReview/Index` | **Completely unstyled** — bare `h1`, raw `select` + `button`, `<article>` rows with `<h2>` and inline forms | The biggest build in the session. Keep the inline reason inputs (decline/cancel require them) and every form field name. |
| `CertificateLedger/Index` | On-system table with tabs | Empty state uses a `card` class that does not exist; count before pager; `×` glyph; Approve/Decline/Issue all render as identical secondary buttons; `View PDF` is a button rather than a row link. |
| `CertificateLedger/Viewer` | Fine | `.certificate-toolbar` reuses `.page-heading` for a toolbar with no heading; use `.section-head`. |
| `MyCertificates/Unavailable` | Panel + error notice | Minor spacing only. |
| `AccountRequest/Request` | Public form in `.login-card`, on-system | Single column is correct at the 576px public width. Verify the field rhythm only. |
| `AccountRequest/Submitted` | On-system | Verify only. |
| `Account/AccessDenied` | Panel containing an h1 | Heading should sit outside the panel like every other page. |
| `Event/Cancel` | **Unstyled** — page title is an `h2`, bare `h3`/`p`, form with no panel | Rebuild as page head + facts panel + form panel. Keep every field name and validation span. |
| `Event/Cover` | **Unstyled** — same, plus an inline `style` on the image | Rebuild; move the inline style into CSS. Keep the file input contract. |
| `Shared/Error` | Bootstrap-era markup (`text-danger`, `<code>`, dev-mode block) | Restructure visually; keep the copy. |

### Shared files and ownership
- `Styles/app.css` is still single-owner. Sessions C–E are done, so Session F is the only remaining
  writer — append, do not restructure.
- `Styles/features/passes.css` and `Views/MyEvents/_PassPartial.cshtml` were reskinned in Session E.
  F should verify `Passes/Index`, not rebuild the pass card.
- `Styles/features/certificate-history.css` belongs to the My Events certificate tab (Session E), not
  to the ledger. The ledger has its own file.
- `_EventSelector` is shared by Check-in, Reports and the Ledger: restyle it once, in
  `event-selector.css`, and check all three.

### Critique
The Session E critique covered only the Session E surface. Session F must run its own
`/impeccable critique` then `/impeccable polish` over these nine screens. Two sub-agent assessments
were started in Session E and aborted before returning anything, so there is no inherited finding set
for F.

## Environment

- App: `http://localhost:5000` (running, `--no-build`, log at
  `C:\Users\cruiz\AppData\Local\Temp\opencode\attendta-run.log`).
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

  The seeded `uc.edu.ph` people keep their original unknown passwords. `temp.officer` also has a
  member record (`UC-2026-9001`), so one person can act on both sides.
- Stop the app before `dotnet build` — the running exe locks the build output.
- Integration tests: run
  `tests\ASI.Basecode.IntegrationTests\bin\Debug\net10.0\ASI.Basecode.IntegrationTests.exe` with
  `ATTENDTA_TEST_SQL` set to LocalDB `master`. `dotnet test` on that project reports "Zero tests ran".

## Git

- Branch: `feature/51-check-in-refactor-sweep` at `2cf6ac3`.
- All Session B + C + D + E changes are uncommitted. Nothing was committed, pushed, stashed, or
  branched by any of these sessions.
- Before committing, branch from `upstream/dev` per `AGENTS.md`, not from this stale branch.
- The diff includes `EventController.cs` and two integration test files from Session D — an
  owner-approved functional change (the Event dialog conversion). Call it out in the PR description
  so the reviewer does not treat it as accidental.
