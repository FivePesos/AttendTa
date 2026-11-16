---
title: 'Fix authenticated pagination normalization regression'
type: 'bugfix'
ticket: ''
created: '2026-11-16'
status: 'built'
baseline_revision: 'f56e9ba1dfc95755b14ee48effbb2a4cddc6bd96'
route: 'oneshot'
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

**Problem:** The integration suite fails because an authenticated event roster POST uses a stale two-row page size instead of the normalized five-row minimum. The login change made request-scoped list options resolve during cookie validation before pagination middleware rewrites the request.

**Approach:** Eliminate premature list-option construction or otherwise ensure authenticated requests use normalized list options, without weakening authorization or changing pending-account login behavior. Preserve existing GET/POST pagination and filters; prove behavior in end-to-end tests instead of changing the expected range to match a defect.

</frozen-after-approval>

## Implementation Notes

Oneshot: a focused cross-layer dependency/order correction and regression tests, expected under 100 changed lines. `Startup.cs` authenticates and authorizes before `AutoFitPagination.NormalizeAsync`; `Startup.Auth.cs` resolves `IAccountService` during cookie validation. `AccountService` currently constructs `IAccountRequestRepository`, which eagerly takes scoped `ListReadOptions`, before normalization. Preserve authorization and redirect semantics. See `ListReadOptionsFactory.cs`, `AccountRequestRepository.cs`, `Startup.DI.cs`, `RegistrationReviewService.cs`, `AutoFitPaginationTests.cs`, and `CompactListFlowTests.cs`. Reproduce or assert an authenticated dossier POST with eight pending rows, pageSize=2 and page=4: normalized page size 5, displayed rows 6–8; also cover a non-dossier POST with legacy size and form-page precedence. Do not weaken user/account lookup or authentication security. Keep unrelated user work in the original worktree untouched.

- Replaced the eager account-request repository dependency with a request-scoped factory used only when pending-request login verification is needed; cookie validation no longer constructs list options early. Middleware ordering and policy behavior are unchanged.
- Added an authenticated registration-review POST regression for legacy size and form-page precedence; the existing dossier integration assertion now passes. Release build passed with no warnings; 403 service tests, 120 focused integration tests, and all 579 full integration tests passed.

## Plan Change Log

## Review Triage Log

- Quick independent review: no findings.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — succeeds.
- With `ATTENDTA_TEST_SQL` set to LocalDB master, run the integration host for `ASI.Basecode.IntegrationTests.CompactListFlowTests`, `ASI.Basecode.IntegrationTests.AutoFitPaginationTests`, `ASI.Basecode.IntegrationTests.AuthenticationFlowTests`, and `ASI.Basecode.IntegrationTests.RegistrationReviewTests` — all pass.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` — all pass.
- Run the complete SQL integration suite if feasible and report any failures.
