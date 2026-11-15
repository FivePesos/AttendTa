---
title: 'Registration bulk approval: keep pageSize across a batch and stop the auto-fit GETting a POST-only endpoint'
type: 'bugfix'
ticket: ''
created: '2026-11-16'
status: 'built'
baseline_revision: '114c7f6bc83400ba22fef2e1d76b542a6d03e434'
route: 'oneshot'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
followup_review_recommended: false
context: []
warnings: []
deferred:
  - summary: >-
      The row-level Approve, Decline and Cancel path on the registration review queue still drops
      pageSize and search, so a single decision taken from a sized or searched queue returns the
      reader to the default page size and an unfiltered list.
    evidence: |-
      Views/RegistrationReview/Index.cshtml:147 posts only id, status, eventId and page from the
      per-row dialogs, and RegistrationReviewController.cs:120 redirects to Index with
      new { status, eventId, page }. The GET then renders at the default size and without the
      search term. Pre-existing, outside issue #253's bulk-approval scope, and not caused by this
      change; the bulk path is fixed here.
    location: >-
      ASI.Basecode.WebApp/Views/RegistrationReview/Index.cshtml:147
    severity: low
---

<intent-contract>

## Intent

**Problem:** Two defects on the registration bulk-approval surfaces (issue #253, follow-up to PR #251). The review queue's bulk dialog builds its route values from `status`, `eventId`, `search` and `page` but not `pageSize`, so a batch taken from a page sized above the default re-renders at the default size and the reader loses their place. Separately, both bulk POSTs re-render in place, so after a batch the browser URL is a POST-only action; `list-controls.js` then runs its paging fit and issues a GET to that URL, which the POST-only action answers with 405.

**Approach:** Add `pageSize` to the queue's dialog key list, matching the dossier. Stop the auto-fit on any document that a non-GET request produced, by having `_Pager` mark the footer and `list-controls.js` skip the fit when that mark is present — the re-render already carries the reader's size, so there is nothing to fit, and the marker holds on the resize path too.

</intent-contract>

## Implementation Notes

Oneshot: three production edits of a few lines each plus two test files, all inside the existing list-paging contract. No new service, no controller signature change, no change to the re-render pattern the other two bulk surfaces share.

Decisions taken during planning, with the evidence behind them:

- **`pageSize` is read from the request query, not an action parameter.** `ListReadOptionsFactory.From` (`ASI.Basecode.WebApp/Models/ListReadOptionsFactory.cs:39`) reads `pageSize` off `Request.Query`, and `Startup.DI.cs:28` scopes it per request. So carrying `pageSize` in the dialog's route values is enough for the POST re-render to read the right size; no controller parameter is needed.
- **The auto-fit must be stopped, not redirected.** The issue offers redirect-after-POST first, but all three bulk surfaces (`AccountRequest`, `CertificateLedger`, `RegistrationReview`/`Event`) re-render in place, and the result panel is a `ViewBag` object that the cookie TempData provider would have to serialise. Option 2 of the issue ("have the fit skip when the current path is a POST-only action") is the smallest complete fix.
- **The marker is gated on the request method, not on the two views.** `_Pager` knows `Context.Request.Method`, so one change covers the review queue, the dossier and the two sibling bulk surfaces. It must not be `ViewData["AutoFit"] = false`: that also drops `pageSize` and `fit` from pager links (`_Pager.cshtml:13`), which would re-create the very page-size loss this ticket fixes.
- **`fit=2` on the POST route values was rejected.** It fixes the load path only; `fitRows` bypasses the `fit=2` guard once `resized` is true (`list-controls.js:83,100-108`), so a window resize after a batch would still GET the POST action.
- **The JS submit handler already copies `pageSize` onto the dialog form when the current URL has it** (`list-controls.js:34-50`). That makes the queue's loss intermittent rather than absolute, but the server-side route values are the contract the dossier already follows, and the handler is not reachable from a non-JS or partially hydrated page.

Files touched:
- `ASI.Basecode.WebApp/Views/RegistrationReview/Index.cshtml` — `pageSize` added to the bulk dialog's route keys.
- `ASI.Basecode.WebApp/Views/Shared/_Pager.cshtml` — `data-paging-fit="off"` on the pager of any document a non-GET request produced.
- `ASI.Basecode.WebApp/wwwroot/js/features/list-controls.js` — the fit returns before wiring itself up when that mark is present.
- `ASI.Basecode.WebApp/tests/list-controls.test.js` — `fit` option on the harness plus the POST-render case.
- `tests/ASI.Basecode.IntegrationTests/RegistrationReviewTests.cs` — `BulkApprovalKeepsThePageSizeAndMarksThePostRerenderAsUnfittable`.

Surprises:
- The first draft asserted the marker on the dossier inside `BulkApprovalOnTheDossierRefusesARegistrationOfAnotherEvent`. That test's event has exactly one registration, which the batch approves, so the roster count drops to zero and `_Pager` renders nothing. The assertion moved into the new test, which seeds a 26-row queue and can post a dossier batch with rows left over.
- The fit does not merely skip a navigation on a marked pager: it never registers its resize listener, because the guard sits before the listener wiring. That is the intended behaviour and the JS test asserts it, since a resize after a batch would otherwise GET the POST action.

## Verification

**Commands:**
- `npm --prefix ASI.Basecode.WebApp run test:js` -- expected: every `list-controls` test passes, including the new POST-render case.
- `dotnet build ASI.Basecode.sln --configuration Release` -- expected: no warnings or errors.
- `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` with `ATTENDTA_TEST_SQL` set -- expected: the registration review surface tests pass, including the new queue `pageSize` and pager-marker assertions.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` -- expected: no regression in the service suite.

**Manual checks:**
- Load `/RegistrationReview/Index?status=pending&pageSize=12`, inspect the bulk dialog `<form action>`: it must carry `pageSize=12`, and the pager must carry `data-paging-fit="off"` only on a POST re-render, never on a GET.

## Review Triage Log

### 2026-11-16 — Review pass
- verdicts: 4 findings — high 0, medium 0, low 2, false 2, maybe-false 0
- findings:
  - `[false]` `[reject]` The pager marker also suppresses the fit on the membership queue and the certificate ledger, whose bulk dialogs do not carry `pageSize`, so those surfaces come back at the default size. — Refuted: a size above the default can only come from `pageSize` in the GET query, and `list-controls.js:34-50` copies every current query parameter the form action lacks onto the bulk form, so the POST query always carries the size the GET had. `_Pager` keeps `pageSize` in its links because `autoFit` stays true. The marker therefore suppresses a fit that had nothing to restore; before this change that same fit could only issue a GET to the POST-only action. Carrying the key server-side on those two views would be tidier but changes no outcome.
  - `[low]` `[defer]` The row-level Approve/Decline/Cancel path on the same queue posts only `id`, `status`, `eventId` and `page` and redirects with the same three, so a single decision still drops `pageSize` and `search`. — Real, and confirmed at `Views/RegistrationReview/Index.cshtml:147` and `RegistrationReviewController.cs:120`, but pre-existing and outside issue #253's bulk-approval scope. Recorded in `deferred`.
  - `[low]` `[patch]` The pager still advertises "Auto · N per page" on a document where the fit has been disabled. — Real: the label was gated on `autoFit`, which stays true on a POST re-render. Patched by gating the label on `fitAllowed`, so a POST re-render reads "12 per page" while every GET view keeps its existing wording.
  - `[false]` `[reject]` `AGENTS.md` "UI review" requires `/impeccable critique` and `/impeccable polish` for this change because it touches Razor. — Refuted on the rule's own scope: it applies to a UI-facing ticket — a new screen, form, table, dialog, navigation change, or a refactor sweep — and this change adds a hidden data attribute plus a hidden query key, with the one visible string becoming more accurate. The surface's critique snapshot from PR #251 (`.impeccable/critique/2026-11-15T18-03-35Z__app-views-registrationreview-index-cshtml-5ae7faab.md`) still stands; no new surface was introduced. The issue carries the `frontend` label, so the owner can ask for a fresh rendered critique if they read the rule more broadly.

## Auto Run Result

**Summary.** Issue #253's two defects are fixed. The review queue's bulk dialog now carries `pageSize` in its route values, matching the dossier, so a batch taken from a page the fit sized above the default re-renders at the same size. The paging fit no longer runs on a document a non-GET request produced: `_Pager` marks such a pager with `data-paging-fit="off"` and `list-controls.js` returns before wiring the fit, so neither the load path nor the resize path can issue a GET to a POST-only bulk action.

**Files changed.**
- `ASI.Basecode.WebApp/Views/RegistrationReview/Index.cshtml` — `pageSize` added to the bulk dialog's route keys, with the reason recorded beside it.
- `ASI.Basecode.WebApp/Views/Shared/_Pager.cshtml` — `fitAllowed` derived from the request method, `data-paging-fit="off"` on a non-GET render, and the "Auto" label gated on `fitAllowed`.
- `ASI.Basecode.WebApp/wwwroot/js/features/list-controls.js` — the fit returns early when the pager carries the mark.
- `ASI.Basecode.WebApp/tests/list-controls.test.js` — a `fit` option on the harness and a POST-render case proving no navigation on load and no resize listener.
- `tests/ASI.Basecode.IntegrationTests/RegistrationReviewTests.cs` — `BulkApprovalKeepsThePageSizeAndMarksThePostRerenderAsUnfittable`, covering the queue GET, the queue POST re-render and the dossier POST re-render.
- `_bmad-output/plan-gh-253-bulk-approval-pagesize-and-autofit.md` — this plan.

**Review findings.** 4 reported: 1 patched (low — the "Auto" label on a non-fittable pager), 1 deferred (low — the pre-existing row-level path dropping `pageSize` and `search`), 2 rejected as false (the sibling-surface page-size claim; the impeccable process claim). Patched counts by verdict: high 0, medium 0, low 1.

**Follow-up review recommendation.** `false` — no `high` entry was patched and fewer than two `medium` entries were patched.

**Verification.** `npm --prefix ASI.Basecode.WebApp run test:js` — 36 passed, 0 failed. `dotnet build ASI.Basecode.sln --configuration Release` — succeeded, 0 warnings, 0 errors. Integration suite (`-parallelMode none`, 537 tests) — 0 failed. Service suite — 382 passed, 0 failed. All re-run after the review patch.

**Residual risks.** The 405 was never reproduced in a browser; the fix is verified by the rendered markup and by the JS harness, not by a live resize against a real POST-rendered page. The marker depends on the request method, so an action that answers both GET and POST would lose its fit on the POST render — no such action exists in this app today.
