---
title: 'Prepare a private-credential demo release'
type: 'feature'
ticket: ''
created: '2026-11-18'
status: 'in-progress'
baseline_revision: '9ce223a05787e7231ea28046c81f604726c91008'
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

**Problem:** Issue #269 requires a reproducible release demonstration, but the existing seed has too few members, incomplete scenarios, and external image and credential setup.

**Approach:** Prepare the full release in stages, keeping the v1.0.0 release experience as the baseline and producing a private credential handover separate from the public release.

## Boundaries & Constraints

**Always:** Target `dev` through `feature/269-demo-release`; preserve the two `main`-only commits' intentional changes through explicit reconciliation. Use fictional demo accounts with realistic presenter labels Gabutan, Taguiam, Talisic, Sinco and Pinca; use fictional Gmail-style addresses and at least ten usable member accounts plus management. The owner amended the prior reserved-domain requirement: these addresses may belong to real people, so use them only in the isolated demo, never send email, and do not use actual production data. Keep secrets and hashes out of Git, public assets, command lines, logs and evidence. Stage venue images with the release and copy them to the private store with valid relative keys. Record the owner's approval to replace the issue's embedded credentials with a separately distributed private file.

**Never:** Treat a distributable executable as secret storage; use real production accounts or data; reset a database, merge to `main`, tag or publish without the relevant explicit approval. Never claim an unperformed demo or unavailable prerequisite passed. Preserve operational guidance, intentional main-only changes and the current Windows, Linux and portable release baselines.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Fresh demo | Dedicated isolated local database and private credentials | Setup produces linked accounts, scenarios and private-store images | Stop on validation or provisioning failure |
| Unsafe target | Shared, production, ambiguous or pre-existing database | No seed or reset | Refuse before changes |
| Credential file missing or invalid | No usable private input | No partial successful setup or password leakage | Explain how to securely provide the file |
| Reset | Explicitly approved disposable demo target | Restore starting data and images without touching other stores | Refuse if target safety cannot be established |

</frozen-after-approval>

## Code Map

- `database/seed-demo-scenario.sql` — existing scenario SQL; currently eight participants, reused member hash, insufficient collision and host safeguards.
- `database/seed-local-admin.sql`, `database/seed-event-types.sql`, `database/attendta-mvp-schema.sql` — canonical schema and existing seed boundaries; do not expose hashes.
- `ASI.Basecode.WebApp/Program.cs` — `--hash-password-env` and demo readiness entry points; reuse securely.
- `docs/demo-quickstart.md`, `docs/demo-host-setup.md`, `_bmad-output/initiative-attendta/demo-script.md` — existing setup and walkthrough; scenario claims require verification.
- `tests/ASI.Basecode.IntegrationTests/DemoReadinessSqlTests.cs` — readiness coverage; add focused isolated seed and guard tests.
- `upstream/main` tree and GitHub v1.0.0 release — main is deliberately pruned; Windows executable, Linux and portable assets are existing baselines. Packaging source was not found in main; investigate before claiming reproducibility.
- `docs/release-cleanup-evidence.md` — retain release audit and necessary setup evidence.

## Tasks & Acceptance

**Execution:**
- [ ] `docs/release-269-reconciliation.md` — compare main-only commits, tracked root structure and release assets; document preserve/add decisions and unresolved packaging provenance.
- [ ] `database/seed-demo-scenario.sql` and associated seed tooling — provide ten distinct linked members, management accounts and scenario data with supplied private per-account credentials; fail safely on collisions.
- [ ] `scripts/` and `database/` — implement isolated local-only setup/reset with strict target checks, explicit reset approval, protected private input and no source-checkout requirement.
- [ ] `database/demo-assets/` and release packaging tooling — include licensed venue and event images and provision private image paths; preserve existing executable experience where reproducible.
- [ ] `docs/demo-quickstart.md`, `docs/demo-host-setup.md` and presenter guide — document folder layout, prerequisites, private handover, all named scenarios, timing, ordering, reset and known blockers.
- [ ] `tests/ASI.Basecode.IntegrationTests/` — cover safe target refusal, credential input, account linkage, seed repeatability, images and scenario prerequisites.
- [ ] `docs/release-269-verification.md` — record exact candidate, build, tests, package smoke, scenario results, #242/#243 status, main reconciliation and remaining approvals; preserve documentation unless removal is justified.

**Acceptance Criteria:**
- Given the clean packaged release and an authorized private file, when an isolated demo is set up, then ten distinct member accounts and management accounts sign in, member links exist, and seeded venue images display.
- Given any listed presenter scenario, when its documented steps are attempted on a fresh setup, then the result is recorded as pass or explicitly blocked, with dependencies and reset ordering.
- Given the proposed release tree, when compared with current `main`, then intentional main changes and every added or removed top-level item are documented.
- Given the exact release candidate, when build, focused tests and package smoke run, then results and blockers are recorded without claiming unperformed checks.
- Given approval gates remain outstanding, when the feature PR is prepared, then no release merge, tag, publication or database reset occurs without separate authorization.

## Implementation Notes

## Plan Change Log

## Review Triage Log

## Design Notes

Private credential material is an external input, not a release asset; it can be delivered separately and remains sensitive even for disposable demo accounts. A database name alone is not proof of an isolated server.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — build succeeds.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` — service tests pass, or use the documented direct test host if the runner reports zero tests.
- `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` — integration tests pass using an isolated local master connection.

**Manual checks:**
- With explicit approval and a fresh disposable target, smoke-test the packaged release, each seeded login, images and all presenter scenarios; record blockers and avoid destructive operations otherwise.

## Code Review

### 2026-11-19 — seed and safety group

Partial group review: 5 decision-needed, 3 patch, 0 deferred, 3 rejected. Packaging, tests/documentation, and the event-validation fix are separate outstanding review groups.

- [ ] [Review][Decision] Fixed event dates can become past and conflict with setup-relative registration timestamps [ASI.Basecode.WebApp/Demo/DemoAccountSeeder.cs:313]. Options: retain the requested 2026 dates; or calculate upcoming dates relative to setup. Recommended: retain requested dates for the immediate demo, then change to relative dates for future setups.
- [x] [Review][Decision] Protect private storage ACLs [ASI.Basecode.WebApp/Demo/DemoSetup.cs] — restrictive Windows ACL implemented; focused tests passed.
- [x] [Review][Decision] Failed setup hides its stage [ASI.Basecode.WebApp/Demo/DemoSetup.cs] — sanitized stage-specific diagnostics implemented without automatic cleanup.
- [x] [Review][Decision] Simple credentials are accepted [ASI.Basecode.WebApp/Demo/DemoCredentials.cs] — owner accepted these for the isolated localhost demo.
- [x] [Review][Decision] Fictional Gmail addresses may belong to real people [ASI.Basecode.WebApp/Demo/DemoAccountSeeder.cs] — owner accepted Gmail-style demo addresses; do not send email.
- [x] [Review][Patch] Validate complete PNG content rather than only the signature [ASI.Basecode.WebApp/Demo/DemoSetup.cs]
- [x] [Review][Patch] Add automated successful fresh-setup coverage [ASI.Basecode.WebApp/Demo/DemoSetup.cs]
- [x] [Review][Patch] Add automated ownership mismatch coverage [ASI.Basecode.WebApp/Demo/DemoOwnership.cs]

Rejected: two mojibake reports were artifacts of the review diff encoding; the source contains the correct Unicode names. The alleged deployment-ancestor bypass is rejected because an ancestor of the existing deployment already exists and is refused by the new-root check.

### 2026-11-19 — packaging, tests, and event fix groups

- [x] [Review] Prevent inherited Kestrel endpoint settings from overriding the launcher's intended loopback binding [tools/DemoLauncher/Program.cs].
- [x] [Review] Prevent a competing process from being mistaken for the launched application after ephemeral port release [tools/DemoLauncher/Program.cs].
- [x] [Review] Make first-time setup and private credential input instructions available to recipients of the standalone executable [tools/DemoBundle/Program.cs].
- [x] [Review] Explain how to use the same custom LocalDB instance for setup and application startup [docs/demo-fresh-setup-269.md].
- [x] [Review] Make fresh-setup test opt-out visible rather than silently passing [tests/ASI.Basecode.IntegrationTests/DemoFreshSetupTests.cs].
- [x] [Review] Remove or make non-operational obsolete quickstart commands that contradict the safety warning [docs/demo-quickstart.md].
- [x] [Review] Fix event draft integration tests that use the real clock with fixed December 2026 dates [tests/ASI.Basecode.IntegrationTests/EventDraftFlowTests.cs].

Combined working-tree verification after fixes: isolated Release build passed; 415 service tests, 687 integration tests with fresh-setup opt-in, and the event date-min JavaScript test passed. The external-artifacts integration harness required a temporary solution-file copy and WebApp directory junction to discover the source root. Windows, Linux, portable and standalone packages built; standalone extraction and package-presence checks passed. Live standalone launch on the exact committed candidate and full browser rehearsal remain outstanding.

Rejected: the proposed persistent demo-mode banner confuses the requested illustrative image banners with an application-wide warning banner, which was not requested.
