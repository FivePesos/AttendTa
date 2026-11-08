# Review narrative: Master Data refactor sweep (2.9)

Target: commits `5a0ee85`, `baafcce`, `debfa1d` — *refactor(master-data): [BMAD 2.9] remove dead code and align views to the house pattern*, *fix(master-data): [BMAD 2.9] retain table context in empty list states*, *fix(master-data): [BMAD 2.9] align forms, lists, profile, and login with the design spec*

Ticket: [story-refactor-sweep.md](../tickets/epic-master-data/tickets/story-refactor-sweep.md) · Plan: [story-refactor-sweep-plan.md](../tickets/epic-master-data/tickets/story-refactor-sweep-plan.md)

Review status: **Review complete — all six blocks accepted.** The target expanded from one commit to four after the rendered critique and code review.

---

## Block 1 — Intent

- [x] Accepted · unchanged during review

**Intent (ticket):** "Cleans up deferred review findings, legacy Bootstrap markup artifacts, and dead code across Master Data controllers, services, and views without adding new scope."

Source: the Description of [story-refactor-sweep.md](../tickets/epic-master-data/tickets/story-refactor-sweep.md#L18).

**Placeholder — to be visited:** confirm the frozen Intent, Boundaries, and I/O matrix in the [approved plan](../tickets/epic-master-data/tickets/story-refactor-sweep-plan.md#L20) against the commit's actual reach, and note anything the plan explicitly excludes.

---

## Block 2 — Broad strokes

- [x] Accepted · unchanged during review

Three commits on `feature/28-refactor-sweep` form one sweep: remove what is dead, restore table context in empty states, then close the design gaps the rendered critique found.

Five entry points, in reading order:

1. [Member/Index.cshtml](../../../ASI.Basecode.WebApp/Views/Member/Index.cshtml#L1) — the list carrying most of the visible change: canonical title, Email column, result count, retained empty state.
2. [Member/_Form.cshtml](../../../ASI.Basecode.WebApp/Views/Member/_Form.cshtml#L3) — the fieldset + required-marker pattern the Event Type and Venue editors copy.
3. [EventTypeController.cs](../../../ASI.Basecode.WebApp/Controllers/EventTypeController.cs#L101) — the page-preserving toggle and the named `EventTypeIndex` route.
4. [EventTypeService.cs](../../../ASI.Basecode.Services/Services/EventTypeService.cs#L60) — the actor rename, and the transaction-plus-audit shape the other two services share.
5. [Profile/Index.cshtml](../../../ASI.Basecode.WebApp/Views/Profile/Index.cshtml#L13) — the member-facing screen rebuilt into three grouped panels with a status badge.

Shared enablers: [_ResultCount.cshtml](../../../ASI.Basecode.WebApp/Views/Shared/_ResultCount.cshtml), the `FirstItemNumber`/`LastItemNumber` pair on [PagedResult.cs](../../../ASI.Basecode.Services/ServiceModels/PagedResult.cs#L25), and the new rules in [app.css](../../../ASI.Basecode.WebApp/Styles/app.css#L54).

---

## Block 3 — Service and controller cleanup

- [x] Accepted · unchanged during review

This slice changes code rather than markup: one rename satisfying the ticket's verify clause, one dead-code removal, one duplication removal, and a small behavior fix that fell out of the duplication.

**Actor rename.** `EventTypeService` was the last Master Data file containing `actorUserId`. The helper and its six call sites are now `RequireActorId()` / `actorId`, matching the other two services. Behavior is unchanged: `Forbidden` when the actor is missing or non-positive, and `ICurrentActor` remains the only source.

**Dead code.** `MemberViewModel.UserId` was written by `ToViewModel` and read nowhere. Removed with its assignment. Two AutoMapper maps (`EventTypeViewModel → EventType`, `VenueViewModel → Venue`) were unused because both services map by hand; removed.

**Duplication.** `VenueController.ReportError` did exactly what `ControllerBase.HandleDomainException` does. Both call sites use the shared handler and `ReportError` is gone. The `using ASI.Basecode.Services.Services;` stays — it supplies `OpenSafelyAsync`.

**Behavior fix.** `EventTypeController.ToggleActive` takes `int? page` and redirects through the named `EventTypeIndex` route, so a toggle on page 2 returns to `/EventType?page=2` rather than page 1.

- [EventTypeService.cs](../../../ASI.Basecode.Services/Services/EventTypeService.cs#L60) — `RequireActorId()`
- [MemberService.cs](../../../ASI.Basecode.Services/Services/MemberService.cs#L270) — `ToViewModel` without `UserId`
- [Startup.AutoMapper.cs](../../../ASI.Basecode.WebApp/Startup.AutoMapper.cs#L24) — the trimmed profile
- [VenueController.cs](../../../ASI.Basecode.WebApp/Controllers/VenueController.cs#L73) — `HandleDomainException`
- [EventTypeController.cs](../../../ASI.Basecode.WebApp/Controllers/EventTypeController.cs#L21) — the named route constant
- [EventTypeController.cs](../../../ASI.Basecode.WebApp/Controllers/EventTypeController.cs#L101) — `ToggleActive(int id, int? page)`

---

## Block 4 — View alignment

- [x] Accepted · unchanged during review

One idea repeated: every Master Data screen should look like the same product.

1. **The lists share one shape.** Each keeps its table and headers when empty with the message in a full-width row; status is a badge; numeric cells are monospaced; row actions are small and right-aligned; Member gained an Email column; all three render the same result count above the pager.
   - [Member/Index.cshtml](../../../ASI.Basecode.WebApp/Views/Member/Index.cshtml) — Email column, `colspan="10"` empty row, result count
   - [EventType/Index.cshtml](../../../ASI.Basecode.WebApp/Views/EventType/Index.cshtml) — badge, `.row-actions`, `colspan="4"` empty row
   - [Venue/Index.cshtml](../../../ASI.Basecode.WebApp/Views/Venue/Index.cshtml) — badge, `.numeric` capacity, `colspan="8"` empty row
   - [_ResultCount.cshtml](../../../ASI.Basecode.WebApp/Views/Shared/_ResultCount.cshtml) — shared count, silent at zero

2. **The editors share one shape.** Each `_Form` partial is grouped into fieldsets with legends, marks required fields with `required` + `aria-required` + a visible asterisk, and the Create/Edit pages constrain the form to 600px with a footer action row.
   - [Member/_Form.cshtml](../../../ASI.Basecode.WebApp/Views/Member/_Form.cshtml) — Identity / Academic / Membership
   - [EventType/_Form.cshtml](../../../ASI.Basecode.WebApp/Views/EventType/_Form.cshtml) — Classification details
   - [Venue/_Form.cshtml](../../../ASI.Basecode.WebApp/Views/Venue/_Form.cshtml) — Facility / Availability / Photo, photo required only on Create

3. **The profile is three panels.** Account, Academic details, and Membership, with the status as a badge rather than raw lowercase text.
   - [Profile/Index.cshtml](../../../ASI.Basecode.WebApp/Views/Profile/Index.cshtml)

4. **The chrome matches the spec.** Canonical screen names in the heading and `<title>`, one `<h1>` per screen, and the login card back on Outfit.
   - [app.css](../../../ASI.Basecode.WebApp/Styles/app.css) — `.required`, `.form-panel`, `.form-actions`, `.panel + .panel`, input border `#64748b`

---

## Block 5 — Test coverage

- [x] Accepted · unchanged during review

The tests moved from pinning incidental markup to pinning the behaviour the design requires, and the code review closed the gaps where a regression could still ship green.

**Behaviour now pinned:**

- [MemberPagedListTests.cs](../../../tests/ASI.Basecode.IntegrationTests/MemberPagedListTests.cs) — `<td class="numeric">` and `<td><span class="badge badge-` at cell level.
- [EventTypeManagementFlowTests.cs](../../../tests/ASI.Basecode.IntegrationTests/EventTypeManagementFlowTests.cs) — empty list retains `data-table`; active and inactive badges; hidden `page` input; paged toggle redirect.
- [VenueToggleAuditTests.cs](../../../tests/ASI.Basecode.IntegrationTests/VenueToggleAuditTests.cs) — badge markup replacing the old `<td>Active</td>`.
- [VenuePagedListTests.cs](../../../tests/ASI.Basecode.IntegrationTests/VenuePagedListTests.cs) — new empty-state copy plus `data-table`.
- [AuthenticationFlowTests.cs](../../../tests/ASI.Basecode.IntegrationTests/AuthenticationFlowTests.cs) — Member empty copy, and `AssertReadOnlyProfile` iterating every panel.

**Gaps the code review closed:**

- [PagingTests.cs](../../../tests/ASI.Basecode.Services.Tests/PagingTests.cs) — `FirstItemNumber`/`LastItemNumber` on first, last and empty pages.
- [VenuePhotoFlowTests.cs](../../../tests/ASI.Basecode.IntegrationTests/VenuePhotoFlowTests.cs) — the edit page renders the photo.
- [StaticAssetTests.cs](../../../tests/ASI.Basecode.IntegrationTests/StaticAssetTests.cs) — the retired Plus Jakarta asset is no longer expected.

**Not covered, deliberately:** the browser-side `required` path (the suite posts directly) and visual regression. Both are recorded rather than pretended.

---

## Block 6 — Periphery

- [x] Accepted · unchanged during review

- [story-refactor-sweep-plan.md](../tickets/epic-master-data/tickets/story-refactor-sweep-plan.md) — the approved plan, now with a Plan Change Log entry, a `## Code Review` section, and corrected Implementation Notes.
- [2026-11-06T07-54-11Z critique](../../../.impeccable/critique/2026-11-06T07-54-11Z__asi-basecode-webapp-views.md) — the first critique, marked `superseded_by` the later run.
- [2026-11-06T08-25-22Z critique](../../../.impeccable/critique/2026-11-06T08-25-22Z__asi-basecode-webapp-views.md) — the dual-agent critique that drove `debfa1d`.
- [_ResultCount.cshtml](../../../ASI.Basecode.WebApp/Views/Shared/_ResultCount.cshtml) — the shared count partial, silent at zero.
- [IPagedResult.cs](../../../ASI.Basecode.Services/ServiceModels/IPagedResult.cs) and [PagedResult.cs](../../../ASI.Basecode.Services/ServiceModels/PagedResult.cs) — the two paging properties the partial and pager share.
- [app.css](../../../ASI.Basecode.WebApp/Styles/app.css) and the rebuilt `wwwroot/css/app.css` — the only CSS changes, all required by approved findings.
- `wwwroot/fonts/plus-jakarta-sans-latin.woff2` — deleted, with [StaticAssetTests.cs](../../../tests/ASI.Basecode.IntegrationTests/StaticAssetTests.cs) updated in the same commit.

**Deliberately untouched:** `docs/bmad-source/` and `docsss/`, the ticket file, `Views/Shared/_Pager.cshtml`, and every `Data` repository.

**Outside the repo:** issues #127–#130 record the epic-2 gaps this sweep did not close.
