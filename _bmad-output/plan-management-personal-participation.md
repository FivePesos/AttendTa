---
title: 'Management personal participation access'
type: 'feature'
ticket: ''
created: '2026-11-16'
status: 'in-progress'
baseline_revision: '28a6086a59bcb17f4c24a87f05b9b21e99098732'
route: 'full'
route_source: 'auto'
risk: 'high'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Officers and administrators can register as eligible participants but cannot navigate to or open their own passes and participation history.

**Approach:** Reuse the existing personal pages, add their links to the management shell, and align personal-resource authorization with participation ownership rather than the management role.

## Boundaries & Constraints

**Always:** Preserve owner-scoped reads and resource authorization. Keep personal links visible to management on desktop and mobile; explain a missing linked member profile and how to get help. An inactive membership may view its own history and issued certificates, subject to existing record rules; registration remains restricted to active eligible participants. Respect existing cancellation and pass invalidation on membership or account deactivation. Keep management navigation, account menu and sign-out. Preserve the member experience and profile policy.

**Never:** Add a second header, duplicate personal pages, expand the existing Member policy globally, create GitHub issues, change deactivation or check-in rules, grant access to another person's resources through personal routes, or expose QR tokens or private file paths.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Participation | Active signed-in officer or admin with eligible profile and registration | Can navigate to own registrations, attendance, certificates and qualifying pass | Existing record eligibility applies |
| No profile | Signed-in management user without member profile opens personal page | Clear explanation that a member profile is needed and to contact an administrator | No fabricated records or server error |
| Inactive membership | Active account with inactive linked membership | Own retained history and issued certificate remain readable; new registration refused | Existing deactivation cascade governs upcoming registration and pass |
| Resource isolation | Any signed-in user requests another user's QR or certificate through a personal route | No content or token disclosed; management ledger access remains unchanged | Non-owner and unknown IDs have indistinguishable refusals |
| Account disabled | Deactivated account or stale session | No personal access | Existing authentication rejection applies |
| Layout | Management desktop or mobile | Same personal destinations in existing shell with correct active state and breadcrumbs | Sign-out remains accessible |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Views/Shared/_NavLinks.cshtml`, `_Header.cshtml`, `ASI.Basecode.Resources/Constants/NavGroups.cs`, `NavDestinations.cs` — management rail, mobile menu and breadcrumbs; use shared links and labels; leave member header intact.
- `ASI.Basecode.WebApp/Controllers/PassesController.cs`, `MyEventsController.cs`, `MyCertificatesController.cs`, `Startup.Auth.cs` — member-role route gates; introduce a narrowly scoped personal-participation policy rather than widening `Policies.Member`, preserving Profile's restriction.
- `ASI.Basecode.Services/Services/MyCertificatesService.cs`, `CertificateLedgerService.cs` — role guard and PDF owner check; the ledger permits management to download any issued PDF; the personal PDF id is a registration ID. Resolve ownership against the actor before delegation, return the same refusal for unknown and non-owned IDs, and preserve management ledger access. Keep profile checks outside the Certificates action catch so failures are not masked.
- `ASI.Basecode.Services/Services/PassService.cs`, `MyEventsService.cs`, `MemberEventService.cs`, `ParticipationFacts.cs` — owner-scoped reads and existing registration eligibility; reuse instead of duplicating eligibility or changing mutation guards.
- `ASI.Basecode.Data/Interfaces/IMemberRepository.cs`, `Repositories/MemberRepository.cs` — existing user-to-member lookup for a missing-profile state; use a shared approach rather than separate role-specific checks.
- `ASI.Basecode.WebApp/Views/MyEvents/Index.cshtml`, `Views/Passes/Index.cshtml`, `Views/MemberEvents/Details.cshtml` — existing personal pages and registration feedback; add clear missing-profile messaging without obscuring retained inactive-member history.
- `tests/ASI.Basecode.IntegrationTests/AuthenticationFlowTests.cs`, `MemberEventFlowTests.cs`, existing pass and certificate tests — adapt shell expectations and exercise role, membership, ownership and QR/PDF boundaries.
- `DESIGN.md`, `PRODUCT.md` — project UI authority and product context; update live documentation only where the change makes it inaccurate.

## Tasks & Acceptance

**Execution:**
- [ ] `Startup.Auth.cs`, personal controllers and certificate service — implement narrowly scoped authenticated personal access; independently enforce owner-only personal PDF access before delegating to the management-capable ledger.
- [ ] Personal controllers, services and views — show a shared, clear missing-profile state on Passes and all My Events tabs, including direct URLs; distinguish inactive membership from absent profile and empty history. Retain historical access and existing registration refusals.
- [ ] `_NavLinks.cshtml`, `_Header.cshtml`, navigation constants — add My participation destinations for management, including mobile and breadcrumbs, without duplicating the header.
- [ ] Relevant integration and service tests — cover the matrix with HTTP tests for both roles and members, no-profile and inactive states, all tabs, stale sessions and role changes, QR and PDF inline/download ownership and unknown IDs; verify the management ledger still permits authorized management downloads and Profile remains member-only.
- [ ] `DESIGN.md`, `PRODUCT.md` and applicable UI review artifacts — align current guidance and perform the required UI critique and polish where applicable.

**Acceptance Criteria:**
- Given an eligible officer or admin, when they register and use personal navigation, then they can follow their own registration and use the existing pass and history flows.
- Given an active account with an inactive membership, when viewing retained personal records, then history and available issued certificates are accessible while new registration is refused.
- Given a management account without a linked member profile, when opening either personal destination, then the page explains the prerequisite and next step.
- Given any user, when requesting another user's QR or PDF through a personal endpoint, then no private resource is returned and the refusal matches an unknown ID; the authorized management ledger remains available.
- Given a member or management user, when using desktop or mobile navigation, then existing role-specific destinations, sign-out and personal links function consistently.

## Implementation Notes

- Implementation and targeted integration tests were added. Release solution build succeeded with .NET SDK 10.0.404 in a workspace-local installation.
- Service and integration test hosts were blocked by Windows Application Control (0x800711C7); no tests ran. ATTENDTA_TEST_SQL is not configured. Matrix audit and rendered UI critique/polish remain incomplete. The build workflow remains in progress; do not treat this feature as verified.

## Plan Change Log

## Review Triage Log

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — succeeds.
- With `ATTENDTA_TEST_SQL` set, run `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` — all integration tests pass. Run `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` and, if it reports zero tests, the test host directly as documented in AGENTS.md — all service tests pass.
- If styles change, run `npm run css:build` in `ASI.Basecode.WebApp` — succeeds.

**Manual checks:**
- Inspect management navigation and missing-profile, inactive-member and populated personal screens at desktop and mobile widths; verify sign-out remains reachable.




