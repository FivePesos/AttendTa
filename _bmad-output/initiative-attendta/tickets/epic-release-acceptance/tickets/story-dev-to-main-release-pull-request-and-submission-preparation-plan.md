---
title: 'Dev to main release pull request and submission preparation'
type: 'chore'
ticket: '9'
created: '2026-11-12'
status: 'built'
baseline_revision: '4eac1b9bb72c26b54a02547e5a95dc428eab5d9a'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
followup_review_recommended: false
context:
  - '{project-root}/_bmad-output/initiative-attendta/execution-manifest-fa9ae82.md'
  - '{project-root}/_bmad-output/initiative-attendta/tier-3-verification-record.md'
  - '{project-root}/_bmad-output/initiative-attendta/manual-acceptance-checklist.md'
  - '{project-root}/_bmad-output/initiative-attendta/demo-walkthrough-evidence.md'
  - '{project-root}/_bmad-output/initiative-attendta/acceptance-coverage-matrix.md'
  - '{project-root}/_bmad-output/initiative-attendta/demo-mock-register.md'
  - '{project-root}/docs/release-cleanup-evidence.md'
  - '{project-root}/.github/workflows/ci.yml'
warnings: []
deferred: []
---

<intent-contract>

## Intent

**Problem:** `dev` is 396 commits ahead of `main` and nothing yet states, for one exact commit, that the Tier 1 local checks pass, what the Tier 2 CI history actually shows, and which acceptance evidence closes which requirement. The team also has no prepared dev-to-main pull request, so the human merge has no reviewable artifact to approve. 8.4/8.6/8.7/8.8 each produced a record, but no release-prep record assembles them or reports the CI and peer-approval exceptions honestly.

**Approach:** Write one release-preparation record for the exact `dev` head. Attest the Tier 1 local checks by re-running them on that commit, report the Tier 2 CI and review history across every PR merged to `dev` including the exceptions, map the acceptance evidence package to R1–R6, list the open release blockers, and carry the prepared dev-to-main pull request body with its exact `gh` command. Leave the human merge and sign-off blank. No second screen, no product change, no CI change.

## Boundaries & Constraints

**Always:** Record the exact `dev` head and `main` head commits, the environment and the date. Re-run the Tier 1 checks rather than citing them. Cite the 8.4/8.5/8.6/8.7/8.8 records with their own commit and validity verdict instead of repeating them; state that `git diff --name-only fa9ae82..<dev head>` touches only `_bmad-output/`, so the 8.4 manifest still holds. Report every CI exception and the absence of GitHub review approvals as observed, with the run id and the failure cause. Leave the human merge decision and every sign-off field blank.

**Never:** Build or restyle a screen, change product behaviour, Razor, CSS, schema, tests or `.github/workflows/ci.yml`. Open the dev-to-main pull request, merge to `main`, or mark the release approved. Re-run the 479-test integration suite while the manifest is valid. Claim green CI for a PR whose check failed or never ran. Put secrets, hashes, tokens, cookies or private paths in the record.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Clean dev head | Tier 1 checks all exit 0 on the recorded commit | Tier 1 section reads PASS with the command, exit code and observed output for each gate | Any non-zero exit → the row reads FAIL and names the failing gate; the record still ships |
| PR merged with a failed check | `gh pr list --base dev --state merged` reports a `FAILURE` conclusion | Tier 2 row names the PR, the run id, the failing job and the observed cause; it is never counted as green | Cause not retrievable → row reads FAILURE with the cause marked unconfirmed |
| PR merged with no check run | PR predates the CI workflow | Tier 2 states the count and the PR numbers, with the workflow's first green run named as the boundary | Never inferred as green |
| Release PR has no CI | `ci.yml` triggers only on `pull_request` to `dev` | The record states that a dev-to-main PR produces no run under the unchanged workflow and names the dev-head attestation as the substitute | Never claim the release PR itself is CI-verified |
| Blocked acceptance row | Host, phone, restore or signatory input missing | The blocker table names the row, the missing input and the owning evidence file | Never inferred from a closed issue or a merged tool |
| Human decision | All sections collected | Sign-off block present with merge decision, approver, date and retest fields empty | Record states the release is not approved by this file |

</intent-contract>

## Code Map

- `_bmad-output/initiative-attendta/release-preparation-record.md` (new) — the deliverable.
- `_bmad-output/initiative-attendta/execution-manifest-fa9ae82.md` — 8.4 evidence: commit `fa9ae82`, 321 service / 479 integration, 0 failed; states its own invalidation rule (source or test change invalidates, docs-only does not). Read-only.
- `_bmad-output/initiative-attendta/tier-3-verification-record.md` — 8.8 decision record on `84b3f54`: T3-01 REUSED, T3-02/T3-03 PASS, T3-04 REUSED, T3-05 BLOCKED (7 rows), T3-06 PASS (none required), T3-07 BLANK. Source of the release blockers. Read-only.
- `_bmad-output/initiative-attendta/manual-acceptance-checklist.md` — 8.6 on `d8046bf`: 8 PASS, 5 BLOCKED, 2 BLOCKED sub-fields, evaluator decision blank. Read-only.
- `_bmad-output/initiative-attendta/demo-walkthrough-evidence.md` — 8.7 on `84b3f54`, merged at `71e7373`: 36 FR rows, 26 functional-ID rows, 7 blocked parts. Read-only.
- `_bmad-output/initiative-attendta/acceptance-coverage-matrix.md` — 8.5 coverage map on `95493ac`; case ids only, not execution evidence. Read-only.
- `_bmad-output/initiative-attendta/demo-mock-register.md` — 8.11 gate: every register row `removed`. Read-only.
- `_bmad-output/initiative-attendta/tickets/epic-release-acceptance/epic-release-acceptance.md` — R1–R6, the requirement ids the evidence package maps to. Read-only.
- `docs/release-cleanup-evidence.md` — 8.10 sweep: scans, build, CSS and startup-smoke results. Read-only.
- `docs/hotspot-https-evidence.md`, `docs/demo-host-readiness-evidence.md`, `docs/storage-survival-evidence.md` — every real-host row `BLOCKED`; the owning files for the open blockers. Read-only.
- `.github/workflows/ci.yml` — `on: pull_request: branches: [dev]` only; the fact that a dev-to-main PR triggers no run. Read-only.
- `gh pr list --repo rainyDayssss/AttendTa --base dev --state merged --limit 200 --json number,title,mergedAt,reviews,reviewDecision,statusCheckRollup` — the Tier 2 population: 117 merged PRs.

## Tasks & Acceptance

**Execution:**
- [x] `_bmad-output/initiative-attendta/release-preparation-record.md` — author the record: header (ticket, dev head, main head, divergence, date, operator, environment), result legend, Tier 1 section, Tier 2 section, acceptance evidence package, release scope, blockers, prepared pull request, sign-off block, defects.
- [x] Same file — Tier 1 section: attest `dotnet build ASI.Basecode.sln --configuration Release`, `npm --prefix ASI.Basecode.WebApp run css:build` plus the `git diff --exit-code` CSS gate, `npm --prefix ASI.Basecode.WebApp run test:js`, and the 321-test service suite, each with command, exit code and observed output, re-run on the recorded dev head.
- [x] Same file — Tier 2 section: report the 117 merged PRs as 107 green, 5 with a failed check (#155, #157, #199, #200, #201) each with its run id and observed cause, 5 with no check run (#5, #6, #8, #84, #85, pre-CI), and #229 merged while its run was still in progress; report that no merged PR carries a GitHub review approval and that peer review exists as the committed `formal-review.md` records for #225 and #226; state that `ci.yml` gives a dev-to-main PR no run.
- [x] Same file — acceptance evidence package: one row per finale artifact with ticket, record commit, verdict and the requirement it closes (R1–R6), including the docs-only-changes-since-`fa9ae82` validity check that keeps the 8.4 manifest valid.
- [x] Same file — release scope and blockers: the `main..dev` diff summary, the seven T3-05 host/device/human rows with their owning files, and the CI and peer-approval exceptions as release decisions the human owns.
- [x] Same file — prepared pull request: the dev-to-main title, the full reviewable body, and the exact `gh pr create --base main --head dev` command, marked as a human step that this ticket does not execute.

**Acceptance Criteria:**
- Given the finished record, when a reader looks for the release commit, then one exact `dev` head and the `main` head it would merge into are stated with the divergence count.
- Given the Tier 1 section, when a reader re-runs a listed command on the recorded commit, then it exits 0 and produces the output the record names.
- Given the Tier 2 section, when a reader checks any PR the record calls green, then that PR's check run concluded `SUCCESS`; when a reader checks a PR the record calls a failed or missing run, then the record names it rather than counting it green.
- Given the acceptance evidence package, when a reader follows a row to its record, then that record exists at the commit named and its verdict matches what the row claims.
- Given the 8.4 manifest, when the record cites it, then the record shows that only `_bmad-output/` files changed since `fa9ae82`, so the suite is not re-run.
- Given the prepared pull request, when a reviewer reads it, then the body is complete enough to open the PR without further authoring, and the record states that opening and merging it are human steps.
- Given the sign-off block, when the team reads it, then the merge decision, approver, date and retest fields are blank and the record never states the release is approved.

## Implementation Notes

- 2026-11-12 — one new file, `_bmad-output/initiative-attendta/release-preparation-record.md`; no
  product, Razor, CSS, schema, test or CI change. Recorded `dev` head `4eac1b9` and `main` head
  `e81a404`; divergence measured as 389 commits ahead / 0 behind — 162 merge commits (117 of them
  first-parent, one per merged pull request) and 227 non-merge commits. `main` is a strict ancestor
  of `dev`, so the merge is fast-forwardable.
- Tier 1 was re-run, not cited: `dotnet build` (incremental, exit 0, 0/0) plus a
  `--no-incremental` build (exit 0, 0/0, 11.76 s) so the attestation does not rest on a no-op;
  `css:build` (exit 0, Tailwind v4.3.4, 166 ms); the CSS gate `git diff --exit-code` (exit 0);
  `test:js` (exit 0, 5 tests, 5 pass, 0 fail); the 321-test service suite (exit 0, 321 succeeded,
  0 failed, 0 skipped).
- Tier 2 was read from `gh pr list --base dev --state merged --limit 200` (117 PRs) and each
  exception's run log: 107 `SUCCESS`, 5 `FAILURE`, 5 with no check run. The failures were read from
  the run logs rather than assumed — #155/#157 are two `CS0535` errors in
  `RegistrationReviewTests.FakeMembers` at `RegistrationReviewTests.cs(350,63)` on the build step,
  and #199/#200/#201 are the same single test,
  `RegistrationReviewTests.BrowserQueuePaginatesAndFiltersAndEnforcesPolicyAndAntiforgery`, failing
  with a 500 at `MemberEventTestSupport.SignInAsync`. #229's merge (14:22:53Z) preceded its run's
  conclusion (14:28:14Z) by 5m21s. No merged PR has a `reviewDecision`; 7 carry a `COMMENTED`
  review and 0 carry `APPROVED`.
- The 8.4 manifest was kept valid by measurement, not assertion: `git diff --name-only
  fa9ae82..4eac1b9` is 22 paths, all under `_bmad-output/`, with zero paths under
  `ASI.Basecode.*/`, `tests/`, `database/` or `.github/`. The 479-test integration suite was not
  re-run.
- The prepared pull request is carried in full (title, `gh pr create` command, standalone body) and
  marked as a human step. It was not opened and nothing was merged. The sign-off block is blank and
  the record states that the release is not approved by it.
- 2026-11-12 — **the release target is a filtered tree, not a plain `dev` merge.** The project owner
  specified that `main` must keep the application shape it already has (7 top-level entries) and
  approved adding `database/`, `tests/` and `docs/`. A `dev` → `main` merge would have changed 1 664
  files, 1 138 of them planning, evidence, agent and tooling files. Built `release/82-main-submission`
  at `ce699b0` from `upstream/main` (`174d1fd`) with the `dev` head content of ten top-level entries:
  502 tracked files, 528 files changed, +71 371 / −65 300.
- 2026-11-12 — **two release-branch-only adjustments.** The integration test project copied its
  venue-upload fixture from `_bmad-output/.../prototype-assets/event-cyber.jpg`, so the solution failed
  to build once `_bmad-output/` was excluded (`error MSB3030`). Copied the blob (`1f03f88`, 175 060
  bytes, `git hash-object` matches) to `tests/ASI.Basecode.IntegrationTests/TestAssets/venue-upload.jpg`
  and repointed the `Content Include`. Also dropped the two README references that point outside the
  release tree (`_bmad-output/.../demo-script.md`, `AGENTS.md`); every remaining README link resolves.
  Nothing in `ASI.Basecode.*/` differs from `dev`. The release tree builds clean (0 warnings,
  0 errors).
- 2026-11-12 — **the record's first draft read the stale local `main`.** It reported `main` at
  `e81a404` and a divergence of 389. `upstream/main` is `174d1fd` and the divergence is 396; the local
  branch is 7 commits ahead of the remote and is not what a pull request merges into. Corrected in the
  record's §1, §6, §8, §9 and §10. The ticket plan's original 396 was right.

- 2026-11-12 — **the release tree was verified, not assumed.** Built it (0 warnings, 0 errors) and ran
  the full suites against it: **479/479 integration tests passed** (714.6 s, 0 failed, 0 skipped, 0 not
  run) and 321/321 service tests passed, plus 5/5 JS tests. The release branch is therefore not resting
  on the 8.4 manifest's validity argument.
- 2026-11-12 — **found and fixed a defect in the release tree while verifying it (D-2).** The first
  draft excluded `global.json`, which is not an SDK pin: its only content is
  `{"test": {"runner": "Microsoft.Testing.Platform"}}`, and without it the README's documented
  `dotnet test --project` command fails with `MSB1001: Unknown switch. ... Switch: --project`. Added
  it to the release tree; the tree is now 11 entries / 503 files and the diff against `main` is 529
  files, +71 376 / −65 300.

## Plan Change Log

- 2026-11-12 — **supersedes the divergence correction below.** The 389 figure was measured against the
  local `main` branch (`e81a404`), not the remote `main` (`upstream/main` = `174d1fd`). The plan's 396
  was correct and is restored. `git rev-list --count upstream/main..4eac1b9` = 396; `main..4eac1b9` =
  389 is the local-ref figure. The record now states both and uses the remote one.
- 2026-11-12 — **scope change from the project owner:** the release is a filtered tree for `main`, not
  a `dev` → `main` merge. The plan's Task 5 ("prepared pull request: the dev-to-main title...") is
  superseded: the prepared PR is `release/82-main-submission` → `main`, and §6 now records the release
  tree, the excluded trees and the two release-branch adjustments. The plan's boundary "Never: change
  ... tests" is honoured on `dev`; the test-project fixture path was changed on the release branch only
  and is disclosed in the record's §6.4 and §10 (D-1).
- 2026-11-12 — three plan figures were corrected against observation rather than repeated, because
  the plan is the source of truth for *what to record* and the record must report what the
  repository and GitHub actually show:
  - the divergence is **389**, not 396. *(Superseded by the entry above: 396 is correct against the
    remote `main`; 389 was the stale local ref.)* The plan's 396 does not reproduce at any ref here
    (`main..dev` and `main..origin/dev` are both 389; the stale local `dev` at `84b3f54` is 376), and a
    `dev` that only gains commits cannot have dropped from 396, so the plan's figure is recorded as
    unconfirmed rather than explained;
  - the green count is **107**, not 112 — the plan's 112 is 117 − 5, which silently counts the five
    pre-CI PRs as green; they never ran a check;
  - the expected causes for #200 and #201 (SQL Server container login timeout) are wrong: all three
    of #199/#200/#201 failed the same single integration test with a sign-in 500. The container
    readiness step logs one transient `Login timeout expired` and then succeeds in every run,
    including the 107 green ones, and is not a failed step in any run.
  Each correction is recorded in the deliverable's §10 so a reader can see the plan's figure and the
  observed figure side by side. No product, Razor, CSS, schema, test or CI change.

## Review Triage Log

## Design Notes

The release gate has two independent halves and the record keeps them apart. Tier 1 and Tier 2 are *process* evidence: they say the build is green and the branch discipline held. The acceptance package is *product* evidence: it says the SPEC §20 scenarios were executed. A release record that blends them lets a green build stand in for an unproven acceptance row, which is the failure mode 8.8 already guarded against by refusing to mark a blocked row PASS.

The CI history is reported as found rather than as hoped. Five PRs merged with a failed check and one merged while its run was still going; none of the 117 carries a GitHub review approval. Those are facts a reviewer needs before approving a 396-commit release, and hiding them in a summary sentence would make the record useless at exactly the moment it matters. The record names the cause of each failure so the reviewer can judge whether it was a defect that reached `dev` or a flake that did not: #155/#157 were a compile break on the branch, #199 a single integration test returning 500 at sign-in, #200/#201 a SQL Server container login timeout.

`ci.yml` triggering only on pull requests to `dev` is the structural reason the release PR itself cannot be CI-verified under the unchanged workflow. The record states that plainly and names the dev-head attestation as the substitute, rather than implying a green release PR that will never run.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — expected: 0 warnings, 0 errors.
- `npm --prefix ASI.Basecode.WebApp run css:build` then `git diff --exit-code -- ASI.Basecode.WebApp/wwwroot/css/app.css` — expected: build succeeds and the CSS gate exits 0.
- `npm --prefix ASI.Basecode.WebApp run test:js` — expected: 5 tests, 5 pass, 0 fail.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` — expected: 321 total, 321 succeeded, 0 failed.
- `git diff --name-only fa9ae82..<dev head>` — expected: only `_bmad-output/` paths, so the 8.4 manifest stays valid.
- `git diff --stat` — expected: only the record and this plan file.
- `git diff --check` — expected: clean.

**Manual checks (if no CLI):**
- Open the record: Tier 1 has a command, exit code and output per gate; Tier 2 names every exception; the evidence package maps each row to a requirement id; the sign-off block is blank.
