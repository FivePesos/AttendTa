# Session G starting brief — close 8.12

Date: 2026-11-10
Status: ready to start. Sessions B–F are built and desktop-verified; nothing below 1024px has been
walked, and nothing has been committed.
Authority: `DESIGN.md` at the repository root. `docs/design.md` is the anti-reference.

Read in this order:
1. `redesign-visual-world.md` — the contract and hard rules.
2. `session-f-outcome.md` — the delta from the last session, including the event-dialog addendum.
3. This file — the starting state and the acceptance checklist.

---

## The prompt

```text
Read _bmad-output/initiative-attendta/handoffs/redesign-visual-world.md first.
This is Session G: close 8.12.

1. Walk every screen against the new DESIGN.md at desktop, 1024, 768, 390 and
   200% zoom. Fix drift; run /impeccable critique + polish on anything off.
2. Verify zero functional diff: the diff may touch only Views/**, Styles/**,
   fonts/images, and generated wwwroot/css/app.css.
3. Run dotnet build, npm run css:build, and npm run test:js.
4. Update the local 8.12 ticket file to the re-scoped acceptance and record the
   walkthrough evidence there. Do not edit GitHub #153 unless I ask.

Never commit, push, or open a PR. Report the acceptance table, changed files,
and any release blockers.
```

---

## Repository state at handoff

- Branch: `feature/51-check-in-refactor-sweep` at `2cf6ac3` — **stale**, merged into `upstream/dev`
  long ago. The owner was advised to commit on a fresh branch cut from `upstream/dev` before
  starting G. If that has not happened, G still makes no commits.
- Everything from Sessions A–F is **uncommitted**. Nothing was committed, pushed, stashed or
  branched by any session.
- App: `http://localhost:5000`, running with `--no-build`. Stop it before any `dotnet build`.
- Database: `AttendTaMVP_Test` (disposable LocalDB).

### The diff is not purely visual — these are the approved exceptions

Step 2 of the prompt says the diff may touch only Views, Styles, fonts/images and generated CSS.
It does not. These files are outside that set and are **owner-approved**; list them in the report
rather than treating them as accidental:

| File | Why |
|---|---|
| `ASI.Basecode.WebApp/Controllers/EventController.cs` | Session D converted Event create/edit to the record dialog. Adds an `X-Record-Dialog` header branch and a `Json({saved:true})` return. |
| `tests/ASI.Basecode.IntegrationTests/AuthenticationFlowTests.cs` | Session D, same change. |
| `tests/ASI.Basecode.IntegrationTests/VenuePagedListTests.cs` | Session D, same change. |
| `PRODUCT.md` | Session F addendum: Event added to the record-dialog list. |
| `AGENTS.md` | Session A: visual-authority pointer now targets root `DESIGN.md`. |
| `_bmad-output/initiative-attendta/ux-attendta/DESIGN.md` | Session A: superseded pointer. |
| `_bmad-output/initiative-attendta/delivery-queue.md` | Session A: authority repointing. |
| `_bmad-output/initiative-attendta/tickets/.../story-final-frontend-walkthrough-....md` | Ticket re-scope. |
| `.impeccable/**`, `_bmad-output/**/handoffs/**` | Critique artifacts and session records. |

New untracked view/style files that belong to the redesign:
`Views/Event/_Editor.cshtml`, `Views/Shared/_RowMenuTrigger.cshtml`,
`Styles/features/registration-review.css`, `Styles/features/event-admin.css`,
`wwwroot/fonts/*` (Inter, plus four OFL licences).

---

## What Sessions B–F delivered

| Session | Surface | Outcome file |
|---|---|---|
| A | Direction, root `DESIGN.md`, authority repointing | — |
| B | Tokens, shell (`_Layout`, `_Header`, `_NavLinks`), tracer | — |
| C | Seven lists, `_Pager`, `_ResultCount` | `session-c-outcome.md` |
| D | Forms and the record dialog | `session-d-outcome.md` |
| E | Event dossier, member feed/detail, profile, My Events slots | `session-e-outcome.md` |
| F | Home, Check-in, Passes, Reports, Registration review, Certificates, Unavailable, Access denied, Cancel, Cover, Error | `session-f-outcome.md` |

---

## Acceptance checklist for 8.12

| # | Criterion | Status at handoff |
|---|---|---|
| 1 | Root `DESIGN.md` is the visual authority and `AGENTS.md` points at it | **Done** (Session A) |
| 2 | All ~35 screens / 20 core screens and 10 shared partials on the new system | **Built.** Desktop-verified per session; not walked as one set. |
| 3 | Zero functional diff | **Has approved exceptions** (table above). G must verify nothing *else* drifted. |
| 4 | Desktop / 1024 / 768 / 390 walkthroughs: no dead ends, hidden primary actions, demo-data outcomes | **Desktop only.** 1024, 768, 390 and 200% are unverified. |
| 5 | Keyboard, dialog and 200% zoom checks pass | **Partly.** Record-dialog Escape and focus return verified in F; the rest is carried from D. |
| 6 | Critique findings fixed or recorded as release blockers | **Per session.** No single consolidated list exists. |
| 7 | `dotnet build` clean, `npm run css:build` regenerated | **Green as of Session F** |
| 8 | Nothing committed, pushed or opened as a PR | **True** |

---

## The walk matrix

Every row needs desktop, 1024, 768, 390 and 200% zoom. "F" means Session F verified it at desktop
only; the rest were verified at desktop by their own session.

| Screen | Route | Session |
|---|---|---|
| Sign in | `/Account/Login` | B |
| Access denied | `/Account/AccessDenied` (reach by opening `/Passes` as an admin) | F |
| Members list | `/Member` | C |
| Event types | `/EventType` | C |
| Venues | `/Venue` | C |
| Events | `/Event` | C |
| Accounts | `/AdminAccounts` | C |
| Activity log | `/ActivityLog` | C |
| Account requests | `/AccountRequest` | C |
| Event create / edit | `/Event/Create`, `/Event/Edit?id=4` | D |
| Event type create / edit | `/EventType/Create`, `/EventType/Edit?id=` | D |
| Venue create / edit | `/Venue/Create`, `/Venue/Edit?id=` | D |
| Member edit | `/Member/Edit?id=` | D |
| Event dossier | `/Event/Details?id=4` | E |
| Member event feed | `/MemberEvents` | E |
| Member event detail | `/MemberEvents/Details?id=` | E |
| Profile | `/Profile` | E |
| My Events | `/MyEvents` (registrations, passes, attendance, certificates tabs) | E |
| Home | `/Home/Index` (walk as admin **and** as member) | F |
| Check-in | `/CheckIn` | F |
| Door passes | `/Passes` (member only) | F |
| Participation report | `/Reports/Participation?eventId=1` | F |
| Registration review | `/RegistrationReview/Index` | F |
| Certificate ledger | `/Certificates` (both tabs) | F |
| Certificate viewer | `/Certificates/Viewer/{id}` | F |
| Certificate unavailable | `/MyEvents/Certificates/999999/Pdf` | F |
| Request membership | `/AccountRequest/Request` | F |
| Request submitted | `/AccountRequest/Submitted` | F |
| Cancel event | `/Event/Cancel?id=4` | F |
| Event cover | `/Event/Cover?id=1` | F |
| Shared error | **Unreachable** — no exception-handler pipeline is wired | F |

Route traps that cost time last session:

- `/RegistrationReview` alone is a **404**. Use `/RegistrationReview/Index`.
- `/Passes` is member-only. An administrator gets `/Account/AccessDenied`.
- `/MyCertificates/Unavailable` is not a route; the view comes from the PDF action.
- `/Event/Cancel` and `/Event/Cover` take `?id=`, not a path segment.
- `/Event/Details?id=4` and `/Event/4` both resolve.

---

## Open items to close or record

### Must be walked — the core of this session

1. **All four breakpoints plus 200% zoom.** No session has had viewport control. This is the single
   largest gap in the redesign and the reason G exists.
2. **Sticky columns at 390px** on EventType, Accounts and ActivityLog. Carried from Session D.

### States with no demo data

3. An **active door pass** on `/Passes` — no member currently holds one.
4. A **pending registration** in the review queue, so the Approve + Decline pair is verified by
   markup only. `?status=confirmed` exercises Cancel + reason.
5. **Check-in success and repeat** outcomes — only the closed outcome came from a real scan.
6. A **paginated list** — no demo data spans two pages, so "pager before count" is verified by
   markup order, not by a rendered second page.

### Keyboard and dialog

7. **Event dialog successful save** was never exercised live. Carried from D.
8. **Escape and focus containment** on the dossier decision dialogs. The record dialog was verified
   in F; the six dossier dialogs were not re-checked.

### Owner decisions — do not fix without sign-off

9. **Duplicate event action.** Event rows carry Cover, View dossier, Edit, Cancel, Publish/Manage.
   "View dossier" and "Manage event" open the same page. Collapsing them is a behavior change.
   (Carried from Session C.)
10. **Missing loading states.** Event, AdminAccounts, ActivityLog and AccountRequest have no
    `data-list-status` / `list-feedback.js`. Only Member, EventType and Venue do. Adding it to the
    other four is a behavior change. (Carried from Session C.)
11. **`Shared/Error` is unreachable.** Decide: record it as a deliberate omission, or wire
    `UseExceptionHandler` (a functional change, outside the redesign brief).

### Known defects

12. **Raw timestamps in the published-event locked summary.** `_Form` prints
    `yyyy-MM-dd HH:mm:ss.fff` instead of `ManilaTimeFormatter.Format`. Pre-existing, more visible in
    the dialog. Small, safe fix.
13. **`PassViewModel.AccentClass` is unused** since Session E deleted the CSS accent rules. The
    property remains because removing it is a service change. Record, do not remove.

---

## Ticket edits G must make

The local ticket's **Description** and **Requirements** still carry the pre-redesign scope. The
References and Notes were updated in Session A; these two were not. Replace them with:

**Description**

> Implement the replacement visual world recorded in root `DESIGN.md` and walk the result. Every
> route, dialog, field, validation rule, permission, state transition and copy string stays fixed.
> The superseded `docs/design.md` identity is an anti-reference, not a constraint. After the build,
> walk all 20 core screens at desktop, 1024, 768 and 390px plus 200% zoom, and fix concrete
> presentation, usability, keyboard and responsive findings.

**Requirements**

> Desktop, 1024, 768 and 390px walkthrough evidence shows no dead-end required routes, hidden
> primary actions or demo-data outcomes; keyboard/dialog and 200%-zoom checks pass; all critique
> findings are fixed or recorded as explicit release blockers. Zero functional diff outside the
> owner-approved Event dialog conversion. `dotnet build` clean, `npm run css:build` regenerated.

Then record the walkthrough evidence in the same file, under a dated heading, as a table:
screen × breakpoint × result, with the route used.

---

## Environment

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=(localdb)\MSSQLLocalDB;Database=AttendTaMVP_Test;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
$env:Common__SetupRoot = "C:\setup\projects\ASI.Basecode\root"
$env:ASPNETCORE_URLS = "http://localhost:5000"
dotnet run --project "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj" --no-build
```

Stop the app before `dotnet build` — the running exe locks the build output.

Accounts — one password for all: `AttendTa-Demo-2026!`

| Role | Email |
|---|---|
| Administrator | `temp.admin@attendta.local` |
| Administrator (original demo) | `demo.admin@attendta.local` |
| Officer | `temp.officer@attendta.local` |
| Member | `temp.student@attendta.local` |

`temp.officer` also holds a member record (`UC-2026-9001`), so one person can walk both sides.

Demo data: event 1 Completed, 2 Cancelled, 3 Published, 4 Draft. Use 3 for the published-event
paths and 4 for the editable ones.

### Commands

```powershell
dotnet build "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"
cd "ASI.Basecode.WebApp"; npm run css:build
cd "ASI.Basecode.WebApp"; npm run test:js
```

Integration suite — run the exe, not `dotnet test` (which reports "Zero tests ran"):

```powershell
$env:ATTENDTA_TEST_SQL = "Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"
& "tests\ASI.Basecode.IntegrationTests\bin\Debug\net10.0\ASI.Basecode.IntegrationTests.exe"
```

Last run: **423 total, 0 errors, 0 failed, 0 skipped** (75s), end of Session F. No test references
the record dialog or the event form, so the Session F addendum does not affect it.

---

## Definition of done for G

- Every row of the walk matrix exercised at all five sizes, with the result recorded in the ticket.
- Any drift fixed, or written down as a release blocker with a reason.
- The diff re-checked against the approved-exception table; nothing new outside it.
- `dotnet build` clean, `npm run css:build` regenerated, `npm run test:js` 2/2, integration suite
  green.
- Ticket Description and Requirements re-scoped; walkthrough evidence recorded.
- **Nothing committed, pushed, or opened as a PR.**
- Report: acceptance table, changed files, release blockers.
