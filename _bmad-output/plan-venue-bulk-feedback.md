---
title: 'Improve venue spacing and bulk approval feedback'
type: 'bugfix'
ticket: ''
created: '2026-11-16'
status: 'built'
baseline_revision: 'db4b58a46e053509abd1a03b65d27c63a0cba76a'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Venue capacity values appear crowded against status, and bulk approval results can occupy excessive screen space when every outcome is shown at once.

**Approach:** Improve the venue table's spacing and make bulk approval results compact without losing access to individual outcomes or hiding failures in the summary. Deliver both improvements together on top of the two pending PRs.

## Boundaries & Constraints

**Always:** Keep capacity values and headings aligned as numeric columns, status readable, mobile rows usable, and visual treatment consistent with the existing design system. Preserve every result's identity, outcome, message and server ordering, including confirmed, waitlisted, refusals, duplicates and stale results. The compact summary must clearly indicate failures; an accessible keyboard-operable disclosure must expose all details, including without JavaScript. Keep existing per-item business semantics and the 25-record bound unchanged.

**Never:** Suppress failure information, auto-approve records, create issues, change bulk approval rules, or modify unrelated surfaces. Keep the two pending PRs intact; this branch is intentionally stacked and must not be presented as a clean PR against dev until parent changes merge or its commit list is checked.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Venues | Desktop, zoomed, mobile, empty/error views | Capacity and status have clear separation while headings and numeric values remain aligned; stacked mobile remains usable | Keep existing list error and empty states |
| Bulk success | One to 25 successful outcomes, including confirmed or waitlisted | Show compact counts; disclosure opens every individual identity, status and message | No results silently lost |
| Mixed/failure | Up to 25 outcomes with failures or stale entries | Visible summary names failed count; disclosure shows every result and reason | Preserve outcome text and details even if all fail |
| Accessibility | Keyboard, screen reader, JavaScript disabled | Native disclosure works by keyboard and without script; labels identify counts and meaningful state | Do not announce a long hidden list as one notification |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Views/Venue/Index.cshtml` — three venue table variants; header Capacity and Status at lines 39–40, 55–56 and 85–86, rows at 94–99. Give all states a consistent venue-specific hook if needed.
- `ASI.Basecode.WebApp/Styles/app.css` — fixed table widths, numeric alignment, 14px cell padding, and mobile stacked-row rules. Scope spacing changes to venues and avoid clipping inactive labels or changing numeric direction.
- `ASI.Basecode.WebApp/Views/Shared/_BulkResultPanel.cshtml` — shared heading, counts, list of all outcomes. Used by account requests, registration review, event dossier and certificate ledger. Native `details` and `summary` preserve all records without a script dependency.
- `ASI.Basecode.WebApp/Styles/features/bulk-actions.css` — existing result styles; retain design tokens and add keyboard-visible disclosure focus styling as needed.
- `ASI.Basecode.WebApp/Models/RegistrationBulkResult.cs` and `BulkResultViewModel` — success and failure mapping includes both confirmed and waitlisted; do not change mapping or the original per-item sequence.
- `tests/ASI.Basecode.IntegrationTests/VenuePagedListTests.cs`, `AccountRequestDecisionTests.cs`, `RegistrationReviewTests.cs`, `CertificateReviewDecisionTests.cs` — existing list and bulk-flow coverage. Add focused assertions for shared panel behavior and the four host surfaces.
- `ASI.Basecode.WebApp/package.json` — `npm run css:build` generates committed `wwwroot/css/app.css`; `npm run test:js` runs the JavaScript suite.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.WebApp/Views/Venue/Index.cshtml` and `Styles/app.css` — add venue-scoped capacity/status spacing across populated, empty and error tables, preserving mobile and numeric alignment.
- [ ] `ASI.Basecode.WebApp/Views/Shared/_BulkResultPanel.cshtml` and `Styles/features/bulk-actions.css` — show compact success/failure counts and a native disclosure containing all result rows in original order; handle zero or one item sensibly without hiding failure counts.
- [ ] `ASI.Basecode.WebApp/wwwroot/css/app.css` — regenerate from source with the repository CSS build.
- [ ] Relevant integration and UI tests — cover list states, all-success, mixed and all-failure bulk results, 25 records, confirmed versus waitlisted, four hosts, disclosure semantics, and no-JavaScript availability.
- [ ] `PRODUCT.md` — update the documented result presentation if needed. Run required UI critique and polish unless the repository's demo/integration exception applies.

**Acceptance Criteria:**
- Given a venue list with values of different sizes, when viewed at desktop or 200% zoom, then the capacity header and values remain right-aligned and visually separated from a readable status; mobile rows remain labeled and usable.
- Given a bulk result on any of the four surfaces, when it renders, then success and failure counts are visible and the full outcome list is collapsed rather than occupying the page.
- Given any mixed or all-failure batch, when the result renders, then the failed count is visible without expanding and every identity, status and reason remains available through the disclosure in original order.
- Given a keyboard user or JavaScript-disabled browser, when the result disclosure is activated, then all rows can be reached and read without relying on color.

## Implementation Notes

Venue-specific numeric padding adds separation above the mobile breakpoint; bulk counts remain visible and all original result rows are in a collapsed native disclosure. Generated CSS and product context are updated. The two predecessor PRs are included in this branch's baseline merge.

The Release solution build passed with no warnings or errors, the CSS build passed, 51 JavaScript tests passed, and 98 targeted LocalDB integration tests passed. These include rendered viewport spacing checks and keyboard/no-JavaScript 25-result coverage. Interactive screen-reader inspection remains outstanding.

## Plan Change Log

## Review Triage Log

- Verification gap, patched: added rendered venue spacing/alignment checks at desktop, zoom-equivalent, and mobile widths.
- Verification gap, patched: added a rendered mixed Confirmed/Waitlisted batch assertion with both outcomes counted as successes.
- Test isolation, patched: the venue browser test now removes only its own records from its disposable test database; the full selected test set passes.
- UI critique: independent source assessment and detector found no confirmed defects; the desktop browser was disconnected, so subjective visual and screen-reader checks remain unverified.
- Quick final review: no additional findings.

## Verification

**Commands:**
- `npm run css:build` and `npm run test:js` in `ASI.Basecode.WebApp`.
- `dotnet build ASI.Basecode.sln --configuration Release`.
- With `ATTENDTA_TEST_SQL` set to LocalDB master, run the integration test classes for venue paging and all four bulk result hosts against their disposable fixture databases.
- Inspect desktop and 200% zoom, mobile, keyboard, no-JavaScript, and 25-result states without modifying existing demo records; explicitly report any inaccessible verification.
