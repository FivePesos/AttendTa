---
title: 'Refactor sweep'
type: 'chore'
ticket: '10'
created: '2026-11-10'
status: 'in-review'
baseline_revision: 'd675a86b3bb47d21a5c74cfb16d0e014f58e8736'
route: 'full'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: []
review_loop_iteration: 0
followup_review_recommended: false
context: []
warnings: []
deferred: []
---

<intent-contract>

## Intent

**Problem:** The integrated build still ships pre-acceptance release debt: two stale `ASI - Backup.*.csproj` duplicates that are not in the solution (and force full-path `dotnet run`), a leftover `<None Remove="appsettings.json.rej" />`, a machine-specific `Common:SetupRoot` committed into deployment settings that masks the designed fail-fast, a README that is still the BaseCode template and tells the reader to edit a connection string that does not exist in `appsettings.json`, and setup docs that omit the required `Common__SetupRoot`. `DemoData` is already absent from every committed setting (8.11/8.13), so no demo switch remains to remove.

**Approach:** Delete the throwaway duplicates, drop the dead MSBuild item, blank the committed `SetupRoot` so the existing fail-fast fires, and correct README/`AGENTS.md`/`docs/database-setup.md` so a fresh clone and the acceptance host can run this exact commit. Extend `.gitignore` for private storage and secret file types, and record the sweep as evidence.

## Boundaries & Constraints

**Always:** Keep the existing fail-fast messages and their code paths unchanged. Keep `appsettings.json` free of any connection string, demo switch or secret. Keep `docsss/` (epic source of truth), `bruno/`, `tools/`, `database/seed-demo-scenario.sql` and `database/provision-demo-runtime.sql` (8.7 demo walkthrough and 8.14 host evidence still need them). Keep the CI workflow unchanged. Every documented command must be one that works on this commit.

**Never:** Do not touch Razor, CSS, JS or any presentation surface (8.12 owns that). Do not change CI, tests, or the schema. Do not delete planning/evidence docs under `_bmad-output/`, `docs/` or `newdocs/`. Do not introduce a new configuration key, demo switch or default credential.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Missing SetupRoot | `Common__SetupRoot` unset, `Common:SetupRoot` empty in `appsettings.json` | Startup throws `Common:SetupRoot must be configured so Data Protection keys can be stored under its keys folder.` | Clear fail-fast, no partial start |
| SetupRoot inside web root | `Common__SetupRoot` = a path under `wwwroot` or the deployment tree | `PrivateFileStore` throws `Common:SetupRoot must be outside wwwroot and the application deployment directory.` | Unchanged behaviour |
| Fresh clone | Reader follows README only | Build, run and sign-in steps all exist and name the real env vars | No step references a non-existent `appsettings.json` connection string |
| CSS gate | `npm ci` then `css:build` | `wwwroot/css/app.css` unchanged | `git diff --exit-code` passes |

</intent-contract>

## Code Map

- `ASI.Basecode.WebApp/ASI - Backup.Basecode.WebApp.csproj` -- stale duplicate, absent from `ASI.Basecode.sln`; delete.
- `ASI.Basecode.Services/ASI - Backup.Basecode.Services.csproj` -- same; delete.
- `ASI.Basecode.WebApp/ASI.Basecode.WebApp.csproj:10` -- `<None Remove="appsettings.json.rej" />` refers to a file that does not exist; delete the line only.
- `ASI.Basecode.WebApp/appsettings.json:3` -- `"SetupRoot": "C:\\setup\\projects\\ASI.Basecode\\root"` is a developer-machine path; blank it. No `ConnectionStrings` and no `DemoData` key exist here.
- `ASI.Basecode.WebApp/Startup.cs:170-175` -- throws on a whitespace `SetupRoot`; do not change.
- `ASI.Basecode.WebApp/Storage/PrivateFileStore.cs:28-40` -- second `SetupRoot` guard; do not change.
- `ASI.Basecode.WebApp/Configuration/ConfigurationExtensions.cs:16-20` -- reads `Common:SetupRoot`; do not change.
- `README.md` -- still "C# BaseCode / ASI Bridge/JumpStart", tells the reader to edit `appsettings.json` (no such key) and to use Visual Studio 2022 (cannot target `net10.0`).
- `docs/database-setup.md:81-95` -- §6 run block omits `Common__SetupRoot`; §7 gives no URL.
- `AGENTS.md:11-28` (build/run), `:15` (Backup-csproj workaround), `:68-69` (policies), `:125-142` (test commands and doc list).
- `.github/workflows/ci.yml` -- the real gate: `npm --prefix ASI.Basecode.WebApp ci`, `npm run css:build`, `git diff --exit-code -- ASI.Basecode.WebApp/wwwroot/css/app.css`, `dotnet build ASI.Basecode.sln --configuration Release`.
- `.gitignore:151-205` -- already covers `*.pfx`, `hotspot-certificates/`, `launchSettings.json`, `docs-local/`, `[Tt]est[Rr]esult*/`; add the private-storage and secret file types it misses.
- `docs/release-cleanup-evidence.md` -- new evidence record for this sweep.
- `tests/ASI.Basecode.IntegrationTests/AttendTaWebApplicationFactory.cs:39` -- sets `Common:SetupRoot` itself, so blanking the default cannot break the suite.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.WebApp/ASI - Backup.Basecode.WebApp.csproj`, `ASI.Basecode.Services/ASI - Backup.Basecode.Services.csproj` -- delete both -- throwaway duplicates outside the solution.
- [x] `ASI.Basecode.WebApp/ASI.Basecode.WebApp.csproj` -- remove the `appsettings.json.rej` line -- dead item from a failed patch.
- [x] `ASI.Basecode.WebApp/appsettings.json` -- set `Common:SetupRoot` to `""` -- no machine-specific path in deployment settings; the existing fail-fast takes over.
- [x] `README.md` -- replace the BaseCode template with AttendTa prerequisites, build, run (both env vars), test and sign-in steps linking `docs/database-setup.md` and `docs/demo-quickstart.md` -- setup instructions must match the integrated build.
- [x] `docs/database-setup.md` -- add `$env:Common__SetupRoot` to §6 with both fail-fast messages, and the `http://localhost:5000` URL in §7 -- same reason.
- [x] `AGENTS.md` -- add `Common__SetupRoot` to the run block, drop the Backup-csproj workaround note, add `Policies.Member`, align the two test commands with `ci.yml`, add the missing doc paths -- keep agent instructions true.
- [x] `.gitignore` -- add `uploads/`, `keys/`, `certificates/`, `*.cer`, `*.crt`, `*.key`, `*.pem`, `*.p12`, `*.bak`, `*.sqlite`, `*.db`, `.env`, `appsettings.*.local.json` -- private storage and secret hygiene.
- [x] `docs/release-cleanup-evidence.md` -- new record: date, commit, what was removed, the config/secret scan results, the build and CSS results, and what remains intentionally -- retain useful evidence.

**Acceptance Criteria:**
- Given the repository at this commit, when `dotnet build ASI.Basecode.sln` runs, then it succeeds with no warnings introduced by this change.
- Given `npm --prefix ASI.Basecode.WebApp ci` and `npm run css:build` have run, when `git diff --exit-code -- ASI.Basecode.WebApp/wwwroot/css/app.css` runs, then it reports no difference.
- Given the built app with `ConnectionStrings__DefaultConnection` set and `Common__SetupRoot` unset, when it starts, then it exits with `Common:SetupRoot must be configured so Data Protection keys can be stored under its keys folder.`
- Given the same app with `Common__SetupRoot` set to a private directory outside the deployment tree, when it starts, then it passes configuration and fails only on database connectivity, not on configuration.
- Given the repository, when searching committed settings for `DemoData`, `Mock`, `Prototype` or `Fixture` keys, then `ASI.Basecode.WebApp/appsettings*.json` contains none.
- Given the repository, when searching for `ASI - Backup`, then no project file matches and `dotnet run --project "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"` needs no full-path workaround.
- Given a fresh clone, when a reader follows `README.md` and `docs/database-setup.md` verbatim, then every command and environment variable they name exists and is required by the app.
- Given the repository, when `git ls-files` is searched for private keys, certificates, `.env`, database files or plaintext credentials, then no match is returned.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` -- expected: build succeeded, 0 errors.
- `npm --prefix ASI.Basecode.WebApp ci` then `npm --prefix ASI.Basecode.WebApp run css:build` then `git diff --exit-code -- ASI.Basecode.WebApp/wwwroot/css/app.css` -- expected: no diff.
- `git grep -n "DemoData" -- "ASI.Basecode.WebApp/appsettings*.json"` -- expected: no match.
- `git ls-files | Select-String "ASI - Backup"` -- expected: no match.
- `git ls-files | Select-String "\.(pfx|pem|key|crt|cer|p12|snk|mdf|ldf|sqlite|db)$|^\.env"` -- expected: no match.
- Start-up smoke: `$env:ConnectionStrings__DefaultConnection="Server=(localdb)\MSSQLLocalDB;Database=AttendTaMVP_Test;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"; dotnet run --project "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj" --no-build` with `Common__SetupRoot` unset, then set to a temp directory -- expected: the two outcomes in the Acceptance Criteria.
- `dotnet "tests\ASI.Basecode.IntegrationTests\bin\Release\net10.0\ASI.Basecode.IntegrationTests.dll" -class "ASI.Basecode.IntegrationTests.PrivateFileStoreTests" -parallelMode none` with `ATTENDTA_TEST_SQL` set -- expected: all pass, including the two SetupRoot guard cases.
- `dotnet "tests\ASI.Basecode.IntegrationTests\bin\Release\net10.0\ASI.Basecode.IntegrationTests.dll" -class "ASI.Basecode.IntegrationTests.StartupConfigurationGuardTests" -parallelMode none` with `ATTENDTA_TEST_SQL` set -- expected: 1 passed.

## Implementation Notes

- Both `ASI - Backup.*.csproj` files were removed with `git rm`, so the deletion is staged.
- `appsettings.json` keeps `Common:SetupRoot` as an empty string rather than dropping the key, per
  the design note; `Startup.ConfigureDataProtection` and `PrivateFileStore` were not touched.
- `.gitignore` uses root-anchored `/uploads/`, `/keys/` and `/certificates/` instead of the bare
  directory names in the task list. Unanchored `certificates/` matches the tracked source folder
  `ASI.Basecode.WebApp/Certificates/` on case-insensitive filesystems (Windows), which would
  ignore new renderer files. Root-anchoring still covers a stray private store created at the
  repository root, which is the only place the bare pattern could realistically fire.
- `README.md` documents `dotnet run` (default `http://localhost:5000`) rather than Visual Studio,
  because VS 2022 cannot target `net10.0`.
- `AGENTS.md` now lists the CI invocations for both test projects and keeps the local test-host
  fallback for the `xunit.v3.mtp-v2` `Zero tests ran` trap.
- `docs/release-cleanup-evidence.md` records the scans, build, CSS and startup-smoke results.
- Matrix rows 1 and 2 had no covering test, so two were added: `PrivateFileStoreTests` gained
  `RejectsAMissingSetupRoot` and a two-case `RejectsASetupRootInsideTheWebOrContentRoot` theory,
  and `StartupConfigurationGuardTests` proves the Data Protection fail-fast through the real host.
  Both classes ran and passed. Rows 3 and 4 were exercised directly by the documented commands in
  this run (build, run with both env vars returning HTTP 200, and the CSS gate reporting no diff).
- README and `AGENTS.md` were reconciled to one integration-test invocation (the CI DLL form) after
  the first draft gave two different commands.
- The evidence doc's second startup-smoke row was corrected to the run actually observed: the app
  started, listened on `127.0.0.1:5199`, answered `GET /` with 200, and wrote `<SetupRoot>/keys`.

## Plan Change Log

- 2026-11-10: `.gitignore` directory patterns implemented as root-anchored (`/uploads/`, `/keys/`,
  `/certificates/`) to avoid ignoring the tracked `ASI.Basecode.WebApp/Certificates/` source
  folder on Windows. Intent (ignore private storage) unchanged.
- 2026-11-10: the matrix test audit required covering tests for the two startup guards, which the
  plan's "do not change tests" boundary did not anticipate. Two additive tests were added; no
  existing test or expectation was altered.

## Review Triage Log

### 2026-11-10 — Review pass
- verdicts: 4 findings — high 0, medium 0, low 2, false 2, maybe-false 0
- findings:
  - `[false]` `[reject]` README/AGENTS/database-setup claim a *startup* fail-fast for an unsafe `Common:SetupRoot`, while the location guard is lazy in the `PrivateFileStore` constructor — refuted: none of the three passages says "at startup" or "on start"; each says the app "fails fast with a clear message", which the scoped store's constructor does on first resolution. That the location guard is lazy rather than eager is pre-existing behaviour this sweep did not introduce or change.
  - `[low]` `[reject]` The plan's "Never: do not change tests" boundary is broken by the two added guard tests — the boundary was self-imposed by the plan, not by the ticket, which asks for a focused guard smoke and only says to retain existing CI; the fix would be an edit to this build's plan, which review rules exclude, and the deviation is already recorded in `## Plan Change Log`.
  - `[low]` `[patch]` `README.md` documents the service test-host fallback at `bin\Debug\net10.0` directly under a Release-only build instruction — corrected the path to `bin\Release\net10.0`, which the documented build produces. The matching line in `AGENTS.md` is not affected: its build instruction is a plain `dotnet build`, whose default configuration is Debug.
  - `[low]` `[patch]` `docs/release-cleanup-evidence.md` records the baseline revision but not the revision of the sweep itself — recorded the sweep commit id in the evidence header after committing, and amended the commit so the record and the tree match.

## Design Notes

`Common:SetupRoot` is blanked rather than deleted so the key stays discoverable in `appsettings.json` while the value comes from the environment, matching how `ConnectionStrings:DefaultConnection` already works (absent from the file, documented as an env var, guarded by a fail-fast throw). The integration test factory sets `Common:SetupRoot` through `builder.UseSetting`, so the suite is unaffected.
