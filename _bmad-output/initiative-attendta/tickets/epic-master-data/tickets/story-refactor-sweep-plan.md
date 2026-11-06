---
title: 'Refactor sweep'
type: 'refactor'
ticket: '9'
created: '2026-11-06'
status: 'built'
baseline_revision: 'f6b35428b4f59de181944a270d119ddffab2fea1'
route: 'full'
route_source: 'pinned'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: [quick]
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The Master Data module (Member, Event Type, Venue) passed its feature tickets but still carries dead code, duplicated controller error handling, and Razor markup that drifted from the house pattern. The ticket's verify clause also names `actorUserId` as forbidden, and `EventTypeService` still uses that identifier.

**Approach:** Remove verified-dead code and duplication, align the three view sets to the existing `AccountRequest`/`AdminAccounts` house pattern using classes that already exist in `Styles/app.css`, and rename the remaining `actorUserId` identifiers. No new features, routes, columns, or CSS classes.

## Boundaries & Constraints

**Always:** Behavior stays identical for every existing endpoint and test. Only classes and helpers already present in `ASI.Basecode.WebApp/Styles/app.css` are used. Every mutation stays POST + antiforgery. `ICurrentActor` remains the only service-layer actor source. `Data` repositories and non-Master-Data modules are untouched.

**Never:** Do not add search, status filters, sortable headers, page-number paging, result counts, required markers, form panels, or an `<h1>` promotion — those are new scope or app-wide decisions. Do not remove `InvalidDataException` or `GetActorUserId` outside Master Data. Do not hard-delete records. Do not change the shared `_Pager.cshtml` or `app.css`.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| List renders | Officer opens `/Member`, `/EventType`, `/Venue` | Same rows, order, pager, and actions as before; status is a `.badge`, numeric cells use `.numeric` | No error expected |
| Empty list | 0 rows, or a search with no match | Empty-state message only; no table, no pager | No error expected |
| Toggle on a later page | `POST /EventType/ToggleActive` with `page=4` | Redirect returns to `/EventType?page=4` | No error expected |
| Venue save fails | Capacity below a linked event's limit | Field error redisplayed through the shared `HandleDomainException` | Field error, no redirect |
| Dead code removed | Build plus Master Data tests | Zero warnings, all tests green | n/a |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/Services/EventTypeService.cs` — private `ActorUserId()` at 60 and locals at 109/125/137/157/168/181; the only `actorUserId` left in Master Data.
- `ASI.Basecode.WebApp/Controllers/VenueController.cs` — private `ReportError` at 109 duplicates `ControllerBase.HandleDomainException` (`Mvc/ControllerBase.cs:181`). The `using ASI.Basecode.Services.Services;` at 5 is **required**: it supplies the `OpenSafelyAsync` extension (`Services/Services/PrivateFileOperations.cs:17`).
- `ASI.Basecode.WebApp/Controllers/EventTypeController.cs` — `ToggleActive(int id)` at 99 drops the current page; `VenueController.ToggleActive(int id, int? page)` at 98 is the pattern to copy.
- `ASI.Basecode.WebApp/Startup.AutoMapper.cs` — `CreateMap<EventTypeViewModel, EventType>()` at 28 and `CreateMap<VenueViewModel, Venue>()` at 30 have no `Map<T>` caller; services map by hand.
- `ASI.Basecode.Services/ServiceModels/MemberViewModel.cs` — `UserId` at 9 is written at `MemberService.cs:275` and read nowhere.
- `ASI.Basecode.WebApp/Views/Member/Index.cshtml` — table always renders, then empty-state sits outside `.table-scroll` (54–57).
- `ASI.Basecode.WebApp/Views/EventType/Index.cshtml` — `aria-label="EventType table"` (17), `<td>` without `class="actions"` (34), `<table>` indented one level deeper than siblings (18).
- `ASI.Basecode.WebApp/Views/Venue/Index.cshtml` — `<table>` under-indented (18); `AvailabilityNotes` is free text.
- `ASI.Basecode.WebApp/Views/{Member/Edit,EventType/Create,EventType/Edit,Venue/Create,Venue/Edit}.cshtml` — stray double blank line between `<h2>` and `<form>` (lines 7–8 in each).
- `ASI.Basecode.WebApp/Views/Venue/Edit.cshtml` — line 12 claims a photo is stored but renders none; `Photo` action and `.venue-thumb` already exist.
- House pattern to copy: `Views/AccountRequest/Index.cshtml` (badges 126, `.row-actions` 130, `button-small` 132, `.numeric` 118–125).
- Tests that pin current markup: `MemberPagedListTests` (`data-member-id`, "No members match your search.", "inactive"), `EventTypeManagementFlowTests` (`data-table`), `VenuePagedListTests` (pager labels).

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.Services/Services/EventTypeService.cs` — rename `ActorUserId()` to `RequireActorId()` and every `actorUserId` local to `actorId` — satisfies the verify clause literally.
- [ ] `ASI.Basecode.WebApp/Controllers/VenueController.cs` — replace `ReportError(ex); return View(model);` with `return HandleDomainException(ex, model);` and delete `ReportError`; keep the `Services.Services` using for `OpenSafelyAsync` — removes duplicated error mapping.
- [ ] `ASI.Basecode.WebApp/Controllers/EventTypeController.cs` — accept `int? page` on `ToggleActive` and redirect with it, mirroring `VenueController` — keeps the officer on their page.
- [ ] `ASI.Basecode.WebApp/Startup.AutoMapper.cs` — delete the two unused ViewModel→Entity maps — dead configuration.
- [ ] `ASI.Basecode.Services/ServiceModels/MemberViewModel.cs` and `ASI.Basecode.Services/Services/MemberService.cs` — remove `UserId` and its assignment — dead property.
- [ ] `ASI.Basecode.WebApp/Views/Member/Index.cshtml` — wrap table + pager in the `if (empty) / else` shape used by the other two lists; add `.numeric` to Student Number, Year, Mobile; render Status as `badge badge-active`/`badge badge-inactive` — consistency.
- [ ] `ASI.Basecode.WebApp/Views/EventType/Index.cshtml` — fix `aria-label` to "Event Type table", add `class="actions"`, wrap actions in `.row-actions` with `button-small`, badge the status, normalise table indentation — consistency.
- [ ] `ASI.Basecode.WebApp/Views/Venue/Index.cshtml` — `.numeric` on Capacity, `.row-actions` + `button-small`, badge the status, normalise table indentation — consistency.
- [ ] `ASI.Basecode.WebApp/Views/{Member/Edit,EventType/Create,EventType/Edit,Venue/Create,Venue/Edit}.cshtml` — delete the stray blank line; in `Venue/Edit.cshtml` render the current photo above the form via `Url.Action("Photo", new { id = Model.VenueId })` — removes the artifact and the false claim.
- [ ] Run `/impeccable polish` on the three view folders after the edits, then rebuild CSS only if a class change requires it.

**Acceptance Criteria:**
- Given `dotnet build ASI.Basecode.sln --no-restore -warnaserror`, when it runs, then it reports 0 warnings and 0 errors.
- Given a grep over Master Data controllers, services, view models, and views, when searching for `InvalidDataException`, `GetActorUserId`, `actorUserId`, `DateTime.UtcNow`, and `[Authorize(... Roles`, then there are no matches.
- Given the Services and Integration test projects, when run, then every Master Data test passes.
- Given an Officer opens each of the three lists, when rendered, then status is a badge, numeric cells are monospaced, row actions are small, and the pager markup is identical across the three.
- Given an Event Type is toggled from page 4, when the POST returns, then the redirect lands on `/EventType?page=4`.

## Implementation Notes

- The planned removal of `using ASI.Basecode.Services.Services;` from `VenueController` was wrong: that namespace supplies the `OpenSafelyAsync` extension (`Services/Services/PrivateFileOperations.cs:17`), and removing it fails the build with `CS1061`. The using stays; only `ReportError` was removed.
- `EventType/Index.cshtml` also gained the hidden `page` input on the toggle form. The controller signature alone was not enough to satisfy the page-preserving acceptance criterion.
- `EventTypeController` gained a named `EventTypeIndex` route on `Index`, and the paged toggle branch uses `RedirectToRoute` so the Location is exactly `/EventType?page=N`. `RedirectToAction("Index", new { page })` produced `/EventType/Index?page=N`, which still worked but did not match the frozen matrix.
- Coverage added for the matrix rows that had none: `EventTypeManagementFlowTests` now asserts the paged toggle Location, and `MemberPagedListTests` asserts the `.numeric` and `.badge` markup.
- `VenueToggleAuditTests` pinned the old `<td>Active</td>` markup. Its two assertions now check `class="badge badge-active">Active</span>` and the inactive counterpart, matching the frozen matrix's badge requirement and the `AdminAccountsListTests` style. The status text is still asserted.
- `Member/Index.cshtml` renders `Active`/`Inactive` rather than the raw lowercase lookup code, matching the Event Type and Venue badges. `MemberPagedListTests` asserts the text case-insensitively, so it still passes.
- No CSS classes were added, so `wwwroot/css/app.css` is unchanged.
- Follow-up after the rendered review: all three lists now keep the table and its headers when empty, with the message in a full-width row, per `docs/DESIGN.md:224`. Member distinguishes "no members yet" from "no match"; Event Type and Venue say what to do next. `AuthenticationFlowTests` and `VenuePagedListTests` were updated to the new copy and now also assert `data-table` is retained.

## Plan Change Log

## Review Triage Log

- medium / patch — `MemberPagedListTests`' new `class="numeric"` assertion also matched `_Pager.cshtml:31`'s `<span class="numeric">`, so it would pass with the member cells reverted; tightened to `<td class="numeric">` and `<td><span class="badge badge-`. Fixed.
- medium / patch — `EventTypeManagementFlowTests` posted `page=2` itself and never pinned the view's hidden page input, so removing it would go unnoticed; added a `name="page" value="2"` assertion on the page-2 response. Fixed.
- low / patch — The plan scheduled only polish and no critique artifact existed, though `AGENTS.md` asks for critique and polish; the critique was run (dual-agent, detector `[]`) and its snapshot is now persisted at `.impeccable/critique/2026-11-06T07-54-11Z__asi-basecode-webapp-views.md`. Fixed.
- low / patch — `Venue/Edit.cshtml`'s current-photo block sat inside the `<form>` rather than above it; moved above the form. Fixed.
- low / patch — `EventType/Index.cshtml`'s Actions `<th>` lacked `class="actions"` while `AccountRequest/Index.cshtml:111` has it; added. Fixed.

## Design Notes

The three lists currently disagree on empty-state shape, action-cell class, and status treatment. `AccountRequest/Index.cshtml` is the newest and most complete Operate screen, so it is the reference, not a new design.

Deliberately out of scope and worth a follow-up ticket: `.data-table th:nth-child(3)` in `Styles/app.css:81` pins the third column, which is Course on Member, Status on Event Type, and Location on Venue — the identity column scrolls away. Fixing it changes shared CSS and another module's table, so it is not part of this sweep.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --no-restore -warnaserror` — expected: 0 warnings, 0 errors.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — expected: all pass.
- With `ATTENDTA_TEST_SQL` set to a local SQL Server `master` connection: `dotnet test --project tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` — expected: all pass, including `MemberPagedListTests`, `EventTypeManagementFlowTests`, `VenuePagedListTests`, and `MasterDataTransactionTests`.
- `npm --prefix ASI.Basecode.WebApp run css:build` then `git diff --exit-code -- ASI.Basecode.WebApp/wwwroot/css/app.css` — expected: no diff, since no new classes are introduced.

**Manual checks:**
- Sign in as an Officer and open `/Member`, `/EventType`, `/Venue`; confirm badges, numeric alignment, small row actions, and that the Event Type toggle keeps its page.
