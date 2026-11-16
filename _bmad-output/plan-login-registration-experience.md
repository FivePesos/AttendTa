---
title: 'Login and membership request experience'
type: 'feature'
ticket: ''
created: '2026-11-16'
status: 'in-progress'
baseline_revision: 'd65868de43985adc1fe07e6dafd12d9eba8ec874'
route: 'full'
route_source: 'auto'
risk: 'high'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
context: ['DESIGN.md', 'PRODUCT.md']
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The sign-in and membership-request pages lack polish and password visibility; applicants who have not yet been approved see an unhelpful generic sign-in error.

**Approach:** Revamp both public forms within the established AttendTa visual system and report a pending request only after its password is verified. Password visibility uses an eye icon inside the right side of each password field. This is an explicit user-approved exception to the existing generic-failures requirement, not a new applicant session or a status dashboard.

## Boundaries & Constraints

**Always:** Preserve existing account authentication, authorization, CSRF, request validations, privacy, email normalization, no-secret logging, responsive behavior, and existing application design tokens. A verified pending request must never issue an authentication cookie or redirect to a protected return URL. Keep fields, error messages, keyboard use and autocomplete accessible; password fields default to masked and remain usable without JavaScript.

**Never:** Reveal pending status based on email alone; change approved or inactive account semantics; expose hashes; introduce password reset, email verification, throttling, a status dashboard, or unrelated feature changes. Do not alter the approved visual identity.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Pending | Pending email and correct password | State request awaits approval; no login cookie | Remain on sign-in page |
| Invalid | Unknown email, wrong password, inactive account, rejected request or unusable hash | Existing generic failure | No cookie and no disclosure |
| Account | Active approved account and correct password | Existing signed-in flow | Preserve local return-URL restrictions |
| Concurrent review | Request changes during verification | Avoid reporting a stale pending state where practical; do not authenticate as a member | Generic failure if no longer pending |
| Visibility | Password and confirmation controls | Each control independently toggles visibility without submitting or changing the value | Default masked; no-JS form works |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/Services/AccountService.cs`, `Interfaces/IAccountService.cs` — current user authentication returns a user or null; preserve account precedence and use `IPasswordHasher.Verify` for pending credentials.
- `ASI.Basecode.Data/Interfaces/IAccountRequestRepository.cs`, `Repositories/AccountRequestRepository.cs` — existing pending status query and no-tracking patterns; retrieve only needed pending credentials and recheck status after verification.
- `ASI.Basecode.WebApp/Controllers/AccountController.cs` — generic failure and sole cookie issuance path; add a distinct pending outcome without cookie issuance.
- `ASI.Basecode.WebApp/Views/Account/Login.cshtml`, `Views/AccountRequest/Request.cshtml` — form markup, validation hooks and antiforgery; preserve existing bindings and request form's `public-wide` shell.
- `ASI.Basecode.WebApp/Styles/app.css`, `wwwroot/css/app.css`, `wwwroot/js/features/`, `Views/Shared/_Layout.cshtml` — existing compiled styles, tokens, and feature-script conventions. Maintain `DESIGN.md` branding and 44px touch controls.
- `tests/ASI.Basecode.IntegrationTests/AuthenticationFlowTests.cs`, `AccountServiceTests.cs`, `AccountRequestDecisionTests.cs`, `AccountRequestSubmissionTests.cs` — relevant credential, request, and construction coverage. `ASI.Basecode.WebApp/tests/` hosts Node JS tests.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.Data/Interfaces/IAccountRequestRepository.cs`, `ASI.Basecode.Data/Repositories/AccountRequestRepository.cs` — add narrow pending credential lookup and status recheck.
- [ ] `ASI.Basecode.Services/Interfaces/IAccountService.cs`, `ASI.Basecode.Services/Services/AccountService.cs`, related service models — implement an explicit login outcome that retains existing account precedence and verifies pending hashes.
- [ ] `ASI.Basecode.WebApp/Controllers/AccountController.cs` — render verified pending feedback without sign-in; keep all other failures generic.
- [ ] `ASI.Basecode.WebApp/Views/Account/Login.cshtml`, `Views/AccountRequest/Request.cshtml`, `Styles/app.css`, `wwwroot/js/features/`, `wwwroot/css/app.css` — redesign the public forms, clarify approval and password requirements, add accessible independent show/hide controls, and rebuild compiled CSS.
- [ ] `tests/ASI.Basecode.IntegrationTests/AuthenticationFlowTests.cs`, `AccountServiceTests.cs`, `AccountRequestDecisionTests.cs`, `AccountRequestSubmissionTests.cs` and `ASI.Basecode.WebApp/tests/` — update existing tests as necessary and cover the matrix and password toggles.
- [ ] `PRODUCT.md` — record the approved pending-credential experience if the implementation ships.

**Acceptance Criteria:**
- Given a valid active account, when the user signs in, then existing cookie and redirect behavior remains unchanged.
- Given a pending request, when its correct password is submitted, then pending feedback appears without a member session; wrong credentials and decided requests remain generic.
- Given either form on desktop or mobile, when it is completed by keyboard, then fields, help, errors, and visibility controls are understandable and operable without clipping.
- Given JavaScript is disabled, when either form is used, then passwords remain masked and submission and server-side validation still work.

## Implementation Notes

- Implemented the revised public forms, in-field eye controls, and password-verified pending feedback without creating a session.
- Independent review identified timing differences, case-insensitive SQL hash comparison and ambiguous toggle semantics. Added dummy verification, binary hash rechecks, stable toggle names, and regression coverage. Timing is not guaranteed constant across database paths or differing Argon2 parameters; dummy hashing increases anonymous CPU/memory cost, and rate limiting is outside the approved scope.
- Release build passed; 403 service, 100 targeted SQL/MVC integration, and 46 JavaScript tests passed. CSS build and the Impeccable source detector passed. A full integration run exceeded 240 seconds and another is running with a longer allowance.
- Final desktop/mobile screenshot inspection and the required rendered Impeccable critique and polish remain incomplete: the browser reported its desktop tab was not visible. Do not mark this plan built until these checks and the full integration run are resolved.

## Plan Change Log

## Review Triage Log

## Design Notes

Use the existing neutral, cobalt, bordered AttendTa system; improve content hierarchy and form organization rather than introducing a new brand. Pending feedback must be based on the password-verified request, not a general email lookup. Approval may race verification; recheck pending state without logging credentials or hashes.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — solution builds.
- `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` — integration tests pass with local SQL Server configured; report explicitly if unavailable.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` — service suite passes; use documented direct host fallback if zero tests run.
- `npm run css:build` and `npm run test:js` in `ASI.Basecode.WebApp` — generated styles and JavaScript tests pass.
- Run the Impeccable detector and required critique/polish on changed surfaces; inspect rendered sign-in, request and feedback states at desktop and mobile widths when the app can run.
