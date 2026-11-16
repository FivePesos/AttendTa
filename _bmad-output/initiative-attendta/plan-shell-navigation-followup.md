---
title: 'Shell navigation review follow-up'
type: 'refactor'
ticket: ''
created: '2026-11-16'
status: 'built'
baseline_revision: 'd74e24c0e452a3450e3f4cee1d14f07fe07cd813'
route: 'full'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The management dashboard has a one-link section, destination labels are duplicated, and existing navigation tests and live demo instructions do not fully cover the changed shell.

**Approach:** Place Browse events beside the welcome heading, group the remaining links under descriptive Manage records and Run operations headings, share destination labels, and address the agreed review follow-ups.

## Boundaries & Constraints

**Always:** Preserve each destination, its route and role restrictions; Browse events remains a functional link for eligible management users. Keep the single Home crumb. Preserve historical walkthrough evidence as a dated record.

**Never:** Add a member breadcrumb, detail-record breadcrumb, or attempted-destination access-denied breadcrumb. Do not change registration eligibility, authentication or access policies. Do not push or rewrite the existing commit without authorization.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Dashboard | Admin or officer opens Home | Browse events link and two descriptive sections, retaining role-appropriate links | No error expected |
| Access denial | Management opens access denied | Home link followed by current Access denied | No fabricated Account section |
| Navigation | Home and management destinations | Single Home crumb on Home; ordered, linked root and correct current crumb elsewhere | Unmapped route does not render an empty current crumb |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Views/Home/Index.cshtml` — dashboard sections and role-conditional destinations; preserve member branch.
- `ASI.Basecode.WebApp/Views/Shared/_Header.cshtml` and `_NavLinks.cshtml` — breadcrumb and rail mappings; retain group and role behavior.
- `ASI.Basecode.Resources/Constants/NavGroups.cs` — existing group labels; add shared destination labels in the same constants area.
- `tests/ASI.Basecode.IntegrationTests/AuthenticationFlowTests.cs` — existing admin/officer shell and breadcrumb tests; use their fixture and extract helper.
- `DESIGN.md`, `_bmad-output/initiative-attendta/demo-script.md`, and `.memlog.md` — current design, rehearsal instructions and decision history. `demo-walkthrough-evidence.md` is historical, not a live script.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.WebApp/Views/Home/Index.cshtml` — implement option A without changing routes or role visibility.
- [ ] `ASI.Basecode.Resources/Constants/` and `ASI.Basecode.WebApp/Views/Shared/` — share destination labels; guard Home index and empty fallback; remove confirmed non-layout controller arms.
- [ ] `tests/ASI.Basecode.IntegrationTests/AuthenticationFlowTests.cs` — assert successful responses, ordered breadcrumb links, rail labels, access-denied trail and dashboard destinations for both management roles.
- [ ] `DESIGN.md`, `_bmad-output/initiative-attendta/demo-script.md`, `.memlog.md` — align live instructions and current design, wrap prose and update memory timestamp. Preserve dated walkthrough evidence.

**Acceptance Criteria:**
- Given an admin or officer, when opening Home, then Browse events remains available above the two descriptive sections and all existing authorized destinations remain present.
- Given either management role, when opening Home, Browse events, Members or Access denied, then the breadcrumb has the expected ordered items and Home link behavior.
- Given the member role, when opening Home, then its existing participation links and shell remain unchanged.
- Given the current demo instructions, when following the Home step, then they name the visible headings and Browse events link.

## Implementation Notes

- User approved the dashboard visually. Desktop and narrow-width inspection were not independently verified.
- Release build succeeded and AuthenticationFlowTests passed (43 cases). Quick review found two issues; both were patched and verification rerun.

## Plan Change Log

## Review Triage Log

- Quick review: 0 high, 2 medium, 0 low, 0 false, 0 maybe-false.
- Medium, patch: the Browse events link lacks the 44px touch target required by DESIGN.md; apply an existing appropriate control style.
- Medium, patch: destination tests compare dashboard and rail hrefs without checking their intended routes; assert the expected routes as well.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — succeeds.
- Set `ATTENDTA_TEST_SQL` to LocalDB master and run `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none -class ASI.Basecode.IntegrationTests.AuthenticationFlowTests` — passes.
- Inspect the management dashboard at desktop and narrow widths, and verify the member view remains unchanged.
