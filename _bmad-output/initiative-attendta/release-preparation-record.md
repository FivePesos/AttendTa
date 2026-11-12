# Release preparation record — `dev` to `main`

Ticket 9 / issue #82. Authored 2026-11-12 against the exact `dev` head recorded in §1.

This is the **release preparation record**, not the release approval. It does three things and
nothing else: it re-runs the Tier 1 local checks on one exact commit and attests them, it reports
the Tier 2 CI and review history across every pull request merged to `dev` **as observed** including
the exceptions, and it assembles the acceptance evidence package that 8.4/8.5/8.6/8.7/8.8/8.10/8.11
already produced into a map against R1–R6. It also carries the prepared release pull request and the
filtered release tree that `main` is meant to receive.

It does not build a screen, change product behaviour, touch Razor, CSS, schema, tests or
`.github/workflows/ci.yml`, open the pull request, merge to `main`, or mark the release approved.
The merge decision and every sign-off field in §9 are deliberately blank.

## 1. Record header

| Field | Value |
|---|---|
| Ticket | 9 / issue #82 — Dev to main release pull request and submission preparation |
| Repository | `rainyDayssss/AttendTa` |
| **`dev` head (the integration commit)** | `4eac1b9bb72c26b54a02547e5a95dc428eab5d9a` — *Merge pull request #228 from wigglebop25/docs/225-review-record*, 2026-11-12 22:23:33 +0800 |
| **`main` head (the merge target)** | `174d1fd7e20c6a8b9095d9a351e1d058da550511` — *Initial commit* |
| **Release branch** | `release/82-main-submission` at `ec95549` — the filtered tree proposed for `main` (§8) |
| **Release tree** | **11 top-level entries, 503 files**: `.gitignore`, `ASI.Basecode.Data`, `ASI.Basecode.Resources`, `ASI.Basecode.Services`, `ASI.Basecode.WebApp`, `ASI.Basecode.sln`, `README.md`, `database/`, `docs/`, `global.json`, `tests/` |
| Divergence | `dev` is **396 commits ahead** of `main` and **0 behind** (`git rev-list --count upstream/main..4eac1b9` = 396, `4eac1b9..upstream/main` = 0). Of the 396, **162 are merge commits** — 117 of them first-parent, one per merged pull request — and **234 are non-merge commits**. The branch is fast-forwardable: `main` has no commit that `dev` lacks. |
| Merge base | `174d1fd7e20c6a8b9095d9a351e1d058da550511` — `main` is a strict ancestor of `dev` |
| Date | 2026-11-12 |
| Operator | AI agent (subagent), executing under this ticket plan; human review, merge and sign-off pending |
| OS | Windows 11 Pro, build 10.0.26200 |
| Runtime | .NET SDK 10.0.403, Node v24.12.0, npm 11.19.1 |
| `gh` CLI | 2.103.3, authenticated as `wigglebop25` against `github.com` |
| Environment | Local developer workstation. **This is not the demo host**; it supplies no host, phone, restore or signatory evidence. |
| CI workflow | `.github/workflows/ci.yml` — `on: pull_request: branches: [dev]` only (unchanged) |
| Integration suite | **Not re-run.** The 8.4 manifest is still valid under its own rule (§5.1). |

The divergence count is 396 against the **remote** `main` (`upstream/main` = `174d1fd`), which is the
release target. It is 389 against the **local** `main` branch (`e81a404`), which is 7 commits ahead of
the remote and is not what a pull request merges into; the earlier draft of this record read that
stale local ref and reported 389. Both figures are reproducible — `git rev-list --count
upstream/main..4eac1b9` = 396 and `git rev-list --count main..4eac1b9` = 389 — and the remote figure
is the one this record uses. The splits are 162 merge commits (117 first-parent, one per merged pull
request) and 234 non-merge commits.

## 2. Result legend

| Result | Meaning |
|---|---|
| **PASS** | Re-run on the recorded `dev` head in the environment above; the row names the command, the exit code and the observed output. |
| **FAIL** | Re-run; the observed result contradicts the expected outcome. The row names the failing gate. |
| **VALID** | A cited record whose own validity rule still holds at the recorded head. Not re-run for this ticket. |
| **BLOCKED** | Not executable here. The row names the exact missing input and the evidence file that owns it. A closed issue, a merged tool or a planning document is never substituted. |
| **BLANK** | Reserved for a decision this record cannot make. The human merge decision is the only such row. |

Tier 1 and Tier 2 are **process** evidence: they say the build is green and the branch discipline
held. The acceptance package in §5 is **product** evidence: it says the SPEC §20 scenarios were
executed. A green build never stands in for an unproven acceptance row, and a PASS in §5 is always
the cited record's verdict, not this file's.

## 3. Tier 1 — local checks re-run on `4eac1b9`

The four Tier 1 gates were re-run on the recorded head on 2026-11-12, with the build gate run twice
(incremental and clean). Nothing below is cited from an earlier record.

| Gate | Command | Exit | Observed output | Result |
|---|---|---|---|---|
| Build | `dotnet build ASI.Basecode.sln --configuration Release` | **0** | `Build succeeded. 0 Warning(s) 0 Error(s)`, elapsed 2.24 s (incremental) | **PASS** |
| Build (clean) | `dotnet build ASI.Basecode.sln --configuration Release --no-incremental` | **0** | `Build succeeded. 0 Warning(s) 0 Error(s)`, elapsed 11.76 s; six projects emitted (Resources, Data, Services, Services.Tests, WebApp, IntegrationTests) | **PASS** |
| CSS build | `npm --prefix ASI.Basecode.WebApp run css:build` | **0** | `tailwindcss v4.3.4`; `Done in 166ms` | **PASS** |
| CSS gate | `git diff --exit-code -- ASI.Basecode.WebApp/wwwroot/css/app.css` | **0** | No difference; the committed stylesheet is byte-identical to the build output | **PASS** |
| JS tests | `npm --prefix ASI.Basecode.WebApp run test:js` | **0** | `tests 5`, `pass 5`, `fail 0`, `cancelled 0`, `skipped 0`, `todo 0`, `duration_ms 89.28` — the five dialog/antiforgery contract tests | **PASS** |
| Service tests | `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` | **0** | `Test run summary: Passed!` — `total: 321`, `failed: 0`, `succeeded: 321`, `skipped: 0`, duration 5s 110ms | **PASS** |

Notes:

- The first build was incremental, so a `--no-incremental` build was run as well and is recorded
  separately. Both are clean at 0 warnings and 0 errors, so the Tier 1 build attestation does not
  rest on an incremental no-op.
- `css:build` left no tracked change: `git status --short` after the CSS gate shows no modified
  tracked file (only untracked ticket files), and the CSS gate itself exited 0.
- The 321-test service suite was run with `--no-build --no-restore` against the Release build from
  the row above, which is the same invocation `ci.yml` uses.
- **The 479-test integration suite was not re-run.** Its 8.4 manifest is still valid under its own
  rule (§5.1), and re-running it while valid is outside this ticket's boundaries. It needs
  `ATTENDTA_TEST_SQL`, Playwright Chromium and Poppler; none of those is a release-preparation
  input.
- The Tier 1 gates say the build, the stylesheet, the browser-free JS contracts and the service
  layer are green **on this commit**. They do not say the integration suite is green **on this
  commit**; §5.1 is the argument for why it does not need to be.

## 4. Tier 2 — CI and review history across every PR merged to `dev`

Population: `gh pr list --repo rainyDayssss/AttendTa --base dev --state merged --limit 200
--json number,title,mergedAt,reviews,reviewDecision,statusCheckRollup` → **117 merged pull
requests**, which is exactly the 117 first-parent merge commits counted in §1.

### 4.1 Summary

| Outcome | Count | PRs |
|---|---|---|
| Check run concluded `SUCCESS` | **107** | Every merged PR other than the ten named in §4.2 and §4.3 — first green run 42009022564 on PR #86, latest on PR #229 |
| Check run concluded `FAILURE` | **5** | #155, #157, #199, #200, #201 |
| No check run at all (pre-CI) | **5** | #5, #6, #8, #84, #85 |
| Merged while its run was still in progress | **1** | #229 |
| Carries a GitHub review **approval** | **0** | — |
| Carries any review object at all | **7** | #5, #88, #92, #94, #104, #105, #226 — all state `COMMENTED`, none `APPROVED` |
| `reviewDecision` populated | **0** | — |

The ticket plan quotes 112 green. That figure counts the five pre-CI pull requests as green by
subtraction (117 − 5 failed). They are not green: they never ran a check. The observed split is
**107 green / 5 failed / 5 never ran**, and this record uses that split.

### 4.2 The five merged PRs with a failed check

Each row names the PR, the workflow run, the failing job, the failing step and the cause read from
the run log.

| PR | Title | Merged | Run | Job | Failing step | Observed cause |
|---|---|---|---|---|---|---|
| #155 | `docs(bmad): replan the initiative for a frontend-first demo` | 2026-11-06 14:15:24Z | [42231593613](https://github.com/rainyDayssss/AttendTa/actions/runs/42231593613) | 127938563995 | Build solution | **Compile break on the branch.** Two `CS0535` errors at `tests/ASI.Basecode.Services.Tests/RegistrationReviewTests.cs(350,63)`: `RegistrationReviewTests.FakeMembers` does not implement `IMemberRepository.CountAsync(string, string, CancellationToken)` or `IMemberRepository.GetPageAsync(string, string, int, int, CancellationToken)`. `Build FAILED`, 2 Error(s). |
| #157 | `docs(bmad): record tracker id for the demo data seam ticket` | 2026-11-06 14:59:35Z | [42234394292](https://github.com/rainyDayssss/AttendTa/actions/runs/42234394292) | 127948161480 | Build solution | **Same compile break**, same file and line, same two `CS0535` errors. The branch did not carry the interface implementation the test double needed. |
| #199 | `feat(check-in): [BMAD 4.7] read member attendance from the real owner-scoped query` | 2026-11-07 11:18:08Z | [42367086922](https://github.com/rainyDayssss/AttendTa/actions/runs/42367086922) | 128400317482 | Run SQL integration tests | **One integration test failed.** `RegistrationReviewTests.BrowserQueuePaginatesAndFiltersAndEnforcesPolicyAndAntiforgery` — `System.Net.Http.HttpRequestException: Response status code does not indicate success: 500 (Internal Server Error)` at `MemberEventTestSupport.SignInAsync` (`MemberEventFlowTests.cs:1109`), i.e. the sign-in POST returned 500. Suite totals: `Total: 402, Errors: 0, Failed: 1, Skipped: 0, Not Run: 0`. |
| #200 | `feat(certificates): [BMAD 6.5] disqualify certificate reviews on attendance invalidation` | 2026-11-07 11:56:46Z | [42370097970](https://github.com/rainyDayssss/AttendTa/actions/runs/42370097970) | 128396253277 | Run SQL integration tests | **Same single test, same sign-in 500.** `Total: 409, Errors: 0, Failed: 1, Skipped: 0, Not Run: 0`. |
| #201 | `feat(reports): [BMAD 7.3] read paged participant detail rows from SQL` | 2026-11-07 12:04:30Z | [42370801320](https://github.com/rainyDayssss/AttendTa/actions/runs/42370801320) | 128398325185 | Run SQL integration tests | **Same single test, same sign-in 500.** `Total: 407, Errors: 0, Failed: 1, Skipped: 0, Not Run: 0`. |

Two corrections to the ticket plan's expected causes, recorded here rather than repeated from it:

- The plan expected #200 and #201 to be a *SQL Server container login timeout*. They are not. In
  every run — including the 107 green ones — the "Start SQL Server" step logs one transient
  `Sqlcmd: Error: Login timeout expired` / `TCP Provider: Error code 0x2749` while the container
  boots, then succeeds on a later attempt of its 30-attempt loop. That step is **not** in the failed
  step list of any run. The failing step in #199/#200/#201 is "Run SQL integration tests", and the
  failure is one test, not the database.
- The plan expected #199's cause to be "a single integration test returning 500 at sign-in". That is
  correct, and #200 and #201 are the identical failure. All three merged one after another inside
  ~50 minutes on 2026-11-07, so the same failing test reached `dev` three times before it stopped
  failing.

Whether that sign-in 500 was a product defect that reached `dev` or a test-harness flake is a
judgement for the reviewer, not for this record. What the record can state is that the same test
passes in the 479-test 8.4 run on `fa9ae82` (2026-11-11, §5.1) and in every green run after
2026-11-07, so no current evidence shows a live sign-in defect at the recorded head.

### 4.3 The five merged PRs with no check run

| PR | Head branch | Merged | Why no run |
|---|---|---|---|
| #5 | `feature/update-alliance-erd` | 2026-11-03 04:41:29Z | Predates the CI workflow |
| #6 | `docs/context` | 2026-11-03 15:38:46Z | Predates the CI workflow |
| #8 | `feature/bmad-pr` | 2026-11-04 16:22:04Z | Predates the CI workflow |
| #84 | `development` | 2026-11-04 17:43:06Z | Predates the CI workflow |
| #85 | `feature/gh-9-net10-test-baseline` | 2026-11-05 07:40:02Z | Predates the CI workflow |

The boundary is the workflow's first run: **run 42009022564, 2026-11-05T10:14:18Z**, on PR #86
(`feature/10`, *feat(db): canonical schema and EF Core mapping for all tables*), which concluded
`SUCCESS`. Every PR merged after that timestamp has a check run; every PR merged before it has none.
These five are **not** counted as green anywhere in this record.

### 4.4 #229 merged while its run was still in progress

| Field | Value |
|---|---|
| PR | #229 — `docs(acceptance): [BMAD 8.8] record the Tier 3 matrix with the build and migration checks passing and sign-off blocked` |
| Merged | 2026-11-12T14:22:53Z |
| Run started | 2026-11-12T14:20:19Z |
| Run concluded | 2026-11-12T14:28:14Z |
| Conclusion (final) | `SUCCESS` |

The merge landed **5 minutes 21 seconds before** the check run concluded. At merge time the run was
in progress, so the merge was not gated on a green result. The run has since concluded `SUCCESS`, so
the commit it tested is green; the *process* exception is that the merge did not wait for it. That
is a release decision for the human (§7.2), not a defect.

### 4.5 Review approvals

**No merged pull request carries a GitHub review approval.** `reviewDecision` is empty on all 117.
Seven PRs carry a review object and every one is `COMMENTED`, never `APPROVED`: #5 (2 comments),
#88, #92, #94, #104, #105, #226 (1 each).

The epic's constraint is "PRs to `dev` with green CI **and a peer approval**". The green-CI half is
met for 107 of 117 with five named exceptions. The peer-approval half is **not** met through GitHub
review state at all. What exists in its place is committed peer review as repository artifacts:

| Artifact | PR reviewed | Content |
|---|---|---|
| `_bmad-output/initiative-attendta/walkthrough-pr-225-tier-3-verification/formal-review.md` | #225 (the 8.8 Tier 3 record) | Formal review with Tier 1 / Tier 2 / Tier 3 findings and a "Not findings" section |
| `_bmad-output/initiative-attendta/walkthrough-pr-226-demo-walkthrough/formal-review.md` | #226 (the 8.7 walkthrough record) | Formal review of four lenses (adversarial, edge-case, structure, prose), 158 raw observations triaged to surviving findings, dismissed findings and a "what the review did not cover" section |

Both are committed on `dev` and both were authored against the PR they review. They are peer review
of the **evidence records**, not an approval of the code in those PRs, and they do not substitute for
a GitHub approval on the other 115 pull requests. This is a release decision the human owns (§7.3).

### 4.6 The release pull request itself cannot be CI-verified

`.github/workflows/ci.yml` triggers on `pull_request` with `branches: [dev]` only. A pull request
into `main` — whether from `dev` or from `release/82-main-submission` — matches no trigger, so **the
release PR produces no check run** under the unchanged workflow. This record does not claim the
release PR is CI-verified and does not propose a workflow change. The substitute is the dev-head
attestation in §3 plus the CI history in §4: the application content that would be merged is the
content whose build, CSS, JS and 321 service tests were re-run here, and whose integration coverage is
the still-valid 8.4 manifest on an ancestor commit. The release tree itself was also built clean
(§6.4).

If the team wants the release PR itself gated by CI, that is a workflow change and belongs to a
separate ticket; this ticket is explicitly barred from touching `ci.yml`.

## 5. Acceptance evidence package

### 5.1 Validity check — only `_bmad-output/` changed since `fa9ae82`

The 8.4 manifest's own rule is that any change under `ASI.Basecode.*/` or `tests/` after `fa9ae82`
invalidates it, and that documentation and evidence-only commits do not.

```
git diff --name-only fa9ae82..4eac1b9
```

→ **22 paths, every one under `_bmad-output/`**, and **zero** paths under `ASI.Basecode.*/`,
`tests/`, `database/` or `.github/`:

```
_bmad-output/initiative-attendta/demo-walkthrough-evidence.md
_bmad-output/initiative-attendta/execution-manifest-fa9ae82.md
_bmad-output/initiative-attendta/manual-acceptance-checklist.md
_bmad-output/initiative-attendta/tickets/epic-release-acceptance/tickets/story-full-functional-demo-script-walkthrough-across-all-26-featur-plan.md
_bmad-output/initiative-attendta/tickets/epic-release-acceptance/tickets/story-recorded-manual-spec-20-acceptance-checklist-verification-plan.md
_bmad-output/initiative-attendta/tickets/epic-release-acceptance/tickets/story-tier-3-manual-system-integration-and-regression-verification-plan.md
_bmad-output/initiative-attendta/tier-3-verification-record.md
_bmad-output/initiative-attendta/walkthrough-8-2-hotspot-https/hotspot-https-flow.md
_bmad-output/initiative-attendta/walkthrough-8-2-hotspot-https/walkthrough-8-2-hotspot-https-log.md
_bmad-output/initiative-attendta/walkthrough-8-2-hotspot-https/walkthrough-8-2-hotspot-https.md
_bmad-output/initiative-attendta/walkthrough-pr-222-integration-suite-manifest/walkthrough-pr-222-integration-suite-manifest-log.md
_bmad-output/initiative-attendta/walkthrough-pr-222-integration-suite-manifest/walkthrough-pr-222-integration-suite-manifest.md
_bmad-output/initiative-attendta/walkthrough-pr-225-tier-3-verification/formal-review.md
_bmad-output/initiative-attendta/walkthrough-pr-225-tier-3-verification/record-mechanism.md
_bmad-output/initiative-attendta/walkthrough-pr-225-tier-3-verification/walkthrough-pr-225-tier-3-verification-log.md
_bmad-output/initiative-attendta/walkthrough-pr-225-tier-3-verification/walkthrough-pr-225-tier-3-verification.md
_bmad-output/initiative-attendta/walkthrough-pr-226-demo-walkthrough/check-record.ps1
_bmad-output/initiative-attendta/walkthrough-pr-226-demo-walkthrough/explained-in-plain-english.md
_bmad-output/initiative-attendta/walkthrough-pr-226-demo-walkthrough/formal-review.md
_bmad-output/initiative-attendta/walkthrough-pr-226-demo-walkthrough/record-mechanism.md
_bmad-output/initiative-attendta/walkthrough-pr-226-demo-walkthrough/walkthrough-pr-226-demo-walkthrough-log.md
_bmad-output/initiative-attendta/walkthrough-pr-226-demo-walkthrough/walkthrough-pr-226-demo-walkthrough.md
```

The 8.4 manifest is therefore **VALID at `4eac1b9`** and the 479-test integration suite is **not**
re-run. `check-record.ps1` is a PowerShell script, but it lives under `_bmad-output/`, is not part
of any solution or test project, and is not compiled or executed by the build; it does not affect
the manifest's rule.

### 5.2 The package, one row per finale artifact

Every row names the ticket, the commit the record was produced at, its verdict, and the requirement
it closes. All cited commits are confirmed ancestors of `4eac1b9`.

| # | Artifact | Ticket | Record commit | Verdict | Closes |
|---|---|---|---|---|---|
| 1 | `execution-manifest-fa9ae82.md` | 8.4 / #77 | tested `fa9ae82`, record `970781b` | **VALID.** 321 service + 479 integration, 0 failed, 0 skipped, 0 not run; clean Release build 0/0; CSS and 5 JS tests green; SQL Server LocalDB with `ATTENDTA_TEST_SQL`; real Playwright renderer and Poppler. | **R3** (SM-4 concurrency in one SQL Server run), and the automated half of **R1** |
| 2 | `acceptance-coverage-matrix.md` | 8.5 / #78 | `95493ac` | **VALID as the case-to-test coverage map only.** It is not execution evidence and is never cited as such; §5 of it maps each case to its test class and the seven cases 8.5 added. Its "JPEG only end-to-end" premise is stale (8.6 M-MD-01 disproved it) — a documentation correction owned by 8.5, not a Tier 3 defect. | **R1**, **R3** (mapping) |
| 3 | `manual-acceptance-checklist.md` | 8.6 / #79 | tested `d8046bf`, record `211a1f2` | **VALID.** 8 PASS (UI-01, UI-02, CERT-03 content, M-ADM-01 sequential, M-ATT-02 demonstration, M-CERT-02a, M-CERT-02b, M-MD-01), 5 BLOCKED (ATT-03, OPS-01, OPS-02, M-AUD-01, M-AUTH-01), 2 BLOCKED sub-fields (CERT-03 signatory, M-ADM-01 mixed race), 0 FAIL, 0 defects. Evaluator decision blank. | **R1** (SPEC §20 manual remainder), **R4**, **R5** |
| 4 | `demo-walkthrough-evidence.md` | 8.7 / #80 | executed `84b3f54`, corrected `71e7373`, merged in #226 | **VALID.** One continuous request → approval → registration → check-in → certificate → report journey on the integrated Production build, with **36 FR rows** (FR-1…FR-36) and **26 functional-ID rows**, every one carrying a result; **7 blocked parts**, the same seven T3-05 carries. | **R2** (SM-2), **R1** |
| 5 | `tier-3-verification-record.md` | 8.8 / #81 | `84b3f54`, merged in #225 | **VALID as the gate decision record.** T3-01 REUSED, T3-02 PASS, T3-03 PASS, T3-04 REUSED, **T3-05 BLOCKED (7 rows)**, T3-06 PASS (none required), **T3-07 BLANK**. Sign-off blocked, not approved. | **R6** (Tier 3 half), **R1** |
| 6 | `demo-mock-register.md` | 8.11 / #152 + 8.13 / #151 | gate verified at `89b362b`, last removal `149791b` | **VALID.** All **21 rows `removed`** (M-01…M-15b); no row open; no product source file outside the register carries a register id. | **R1**, **R2** (the "no demo data, mock or fixture fallback" done-when) |
| 7 | `docs/release-cleanup-evidence.md` | 8.10 / #83 | sweep commit `9bfc27a`, baseline `d675a86` | **VALID.** Build 0 warnings / 0 errors; `npm ci` clean; CSS build Tailwind v4.3.4 and the CSS gate exit 0; startup smoke proved both the missing-`SetupRoot` fail-fast and a clean environment-only start serving `200`; secret and configuration scans all clean. | **R1** (build/configuration half) |
| 8 | `docs/hotspot-https-evidence.md` | 8.2 / #75 | `fb783f1` | **VALID as a BLOCKED record.** Rows 1–12 all `BLOCKED`: no host name, no hotspot adapter address, no certificate thumbprint, no phone. A closed #75 is not host evidence. | **R4** — **not closed** |
| 9 | `docs/demo-host-readiness-evidence.md` | 8.14 / #150 | blocked record, evidence dated 2026-11-06 | **VALID as a BLOCKED record.** Checks 1–10 all `BLOCKED`: no demo host, no `AttendTa_Demo`, no runtime SQL login, no staged browser, no disconnection method. Its own header states that closed tracking issues, including #74, are not proof. | **R4**, **R5** — **not closed** |
| 10 | `docs/storage-survival-evidence.md` | 8.3 / #76 | `8247832` | **VALID as a BLOCKED record.** Every host stage `BLOCKED`; its own local section says the helper proves fixtures only and is not host acceptance. | **R5** — **not closed** |

### 5.3 Requirement map

| Id | Requirement (epic) | Closed by | Status at `4eac1b9` |
|---|---|---|---|
| **R1** | prd SM-1; spec §20 — every SPEC §20 acceptance scenario passes on the integrated dev build, with the evidence recorded | Rows 1–7: 8.4 automated run, 8.5 mapping, 8.6 manual checklist, 8.7 journey, 8.8 gate record, 8.10 cleanup, 8.11 mock gate | **PASS with 5 BLOCKED rows and 2 BLOCKED sub-fields.** Of the ten SPEC §20 rows in the 8.6 record, B1–B4 and B6–B8 are PASS, B7 is PASS with the CERT-03 signatory sub-field blocked, B5 is BLOCKED on ATT-03, B9 is BLOCKED on OPS-01/OPS-02, and B10 is BLOCKED on the signatory. No FAIL and no product defect anywhere in the package. |
| **R2** | prd SM-2 — all 26 functional IDs, FR-35 and FR-36 demonstrated end to end on the integrated dev build | Row 4 (8.7) | **PASS.** 36/36 FR rows and 26/26 functional-ID rows carry a result; camera scanning (ATT-03) is the one blocked part and is R4's, not R2's. |
| **R3** | prd SM-4 — the seat, attendance and certificate concurrency tests pass together on SQL Server with zero overbooking or duplicates | Row 1 (8.4), mapped by row 2 §7 | **PASS.** Zero overbooked events, zero duplicate attendance, zero duplicate certificates, zero duplicate registrations, all inside one 479-test SQL Server run with 0 failed. |
| **R4** | prd NFR-1, NFR-6, Q2 — officers' phones scan a QR Pass in Chrome over the hotspot through trusted HTTPS on the demo host, with manual lookup still available | Rows 8, 9, 3 (ATT-03) | **BLOCKED.** The pass-code scan and the manual-lookup fallback are proven (8.7 J33–J36); the camera path on the real HTTPS origin is not. Missing: a real demo host with the hotspot HTTPS binding, its root CA on a real officer phone, and a phone on the hotspot subnet. |
| **R5** | prd NFR-4 — SQL Server and stored images and certificates survive app restart and redeploy and are backed up and restored together into a disposable location | Rows 9, 10, 3 (OPS-01, OPS-02) | **BLOCKED.** Missing: a real host, a dedicated service identity and SQL login, an approved backup destination, an approved empty restore target, a maintenance window and an approver. |
| **R6** | prd SM-3; addendum D — Tier 3 manual E2E passes and `dev` is released to `main` | Row 5 (8.8) + this record | **HALF OPEN.** The Tier 3 matrix is complete and T3-01/T3-02/T3-03/T3-04/T3-06 are settled, but **T3-05 is BLOCKED on seven rows** and **T3-07 (the human Tier 3 decision) is BLANK**. The release pull request is opened as #231 and not merged. |

**R2 and R3 are closed outright. R1 passes with 5 BLOCKED rows and 2 BLOCKED sub-fields — the same
host, phone and human inputs that block R4 and R5 — so it is not cleanly closed. R4 and R5 are
blocked. R6 is half open on T3-05 and the human decision.** This record does not close R1, R4, R5 or
R6 and does not state that the release is approved.

## 6. Release scope

### 6.1 What `main` receives — a filtered tree, not a plain `dev` merge

`main` is the submission branch and holds the application only. A plain merge of `dev` into `main`
would change 1 664 files, of which **1 137** are planning, evidence, agent and tooling files that do
not belong in a submission. The release therefore ships a **filtered tree**: the `dev` head content of
ten top-level entries, committed on `release/82-main-submission` on top of `main`.

| Top-level entry | Why it ships |
|---|---|
| `ASI.Basecode.Data`, `ASI.Basecode.Resources`, `ASI.Basecode.Services`, `ASI.Basecode.WebApp` | The four application projects |
| `ASI.Basecode.sln` | The solution that builds them |
| `.gitignore` | Keeps build output, private storage and key material out of the tree |
| `README.md` | Prerequisites, build, database, run, test and sign-in |
| `database/` | Canonical schema, reset, admin seed, event-type seed, venue migration, demo provisioning |
| `global.json` | Selects the `Microsoft.Testing.Platform` runner, without which the documented `dotnet test --project` command fails (§6.4) |
| `tests/` | The 321-test service suite and the 479-test integration suite CI runs |
| `docs/` | Setup guides, architecture, ERD, sequence diagrams and the evidence records they cite |

Release tree: **11 top-level entries, 503 tracked files**. `main` before the release: 7 entries, 133
files.

### 6.2 What is deliberately not released

1 137 of the 1 664 files that `dev` changes stay out of `main`:

| Excluded top-level entry | Files changed `main..dev` | Reason |
|---|---|---|
| `.agents/` | 558 | Installed agent skill definitions |
| `_bmad-output/` | 398 | Planning, ticket and acceptance evidence |
| `.opencode/` | 88 | Agent harness configuration |
| `_bmad/` | 32 | BMad method scripts and render output |
| `.impeccable/` | 17 | Design-skill configuration |
| `docsss/` | 13 | Course and specification source documents |
| `bruno/` | 12 | Manual API collection |
| `newdocs/` | 8 | Superseded design notes |
| `tools/` | 2 | Host tooling |
| `AGENTS.md`, `DESIGN.md`, `PRODUCT.md`, `AttendTa-User-Stories.md`, `opencode.jsonc`, `skills-lock.json`, `.gitmodules`, `spec/`, `.github/` | 9 | Agent instructions, design authority, submodule wiring and the CI workflow, none of which the submission needs |

`spec/` is a private submodule and `.github/workflows/ci.yml` triggers only on pull requests to `dev`;
shipping either would add a broken submodule or a workflow that never runs on `main`.

### 6.3 The release diff

`git diff --shortstat upstream/main release/82-main-submission` → **529 files changed, 71 376
insertions(+), 65 300 deletions(-)**.

| Area | Files changed |
|---|---|
| `ASI.Basecode.WebApp` | 220 |
| `ASI.Basecode.Services` | 109 |
| `tests/` | 86 |
| `ASI.Basecode.Data` | 59 |
| `docs/` | 37 |
| `ASI.Basecode.Resources` | 7 |
| `database/` | 7 |
| `.gitignore`, `ASI.Basecode.sln`, `README.md`, `global.json` | 4 |
| **Total** | **529** |

The large deletion count is the BaseCode template being replaced by the AttendTa implementation:
`main` is the template, `dev` is the finished MVP.

### 6.4 Three release-branch decisions

The filtered tree needed three decisions that are not on `dev`. All are confined to the release branch
and all are disclosed here rather than folded in silently.

| File | Change | Why |
|---|---|---|
| `tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` | The `Content Include` for the venue-upload fixture now points at `TestAssets\venue-upload.jpg` inside the test project; the file was copied there byte-identically from `_bmad-output/initiative-attendta/ux-attendta/imports/prototype-assets/event-cyber.jpg` (blob `1f03f88`, 175 060 bytes, `git hash-object` matches) | The test project referenced a planning folder. Without the copy the solution does not build once `_bmad-output/` is excluded: `error MSB3030: Could not copy the file ... because it was not found`. This is a real coupling defect on `dev`, recorded in §10. |
| `global.json` | Added to the release tree (it was in the first draft's excluded set) | It is not an SDK pin. Its only content is `{"test": {"runner": "Microsoft.Testing.Platform"}}`, which is what makes `dotnet test --project` work. Without it the documented command fails with `MSB1001: Unknown switch. ... Switch: --project`, and the README tells readers to run exactly that command. |
| `README.md` | Removed the `_bmad-output/initiative-attendta/demo-script.md` / `demo-rehearsal-findings.md` sentence and the `AGENTS.md` bullet | Both point at paths that are not in the release tree. Every remaining README link resolves inside the released tree. |

The release tree is therefore `dev`'s application content plus these three decisions, not a byte-exact
subset of `dev`. Nothing in `ASI.Basecode.*/` differs from `dev`.

What a reviewer is approving, in product terms: the full AttendTa MVP across platform access, master
data, events and registration, check-in, administration, certificates and participation reports, plus
the canonical schema, the private file store, the certificate renderer and the test suites. The
epic-level `Done when` and the per-epic requirements are the source of truth for the feature set; this
record does not restate them.

## 7. Release blockers

None of these is inferable from a closed issue or a merged tool. Each names the row, the missing
input and the evidence file that owns it.

### 7.1 The seven T3-05 host, device and human rows

| Blocker | Rows affected | Missing input | Owning evidence file | Owner | Unblocking action |
|---|---|---|---|---|---|
| **ATT-03** — phone camera scan on the real HTTPS origin | T3-05, R4, SPEC B5 | A real demo host with the hotspot HTTPS binding, its generated root CA installed on a real officer phone, a valid printed pass, and the app reachable from the hotspot subnet | `docs/hotspot-https-evidence.md` rows 1–12 (all BLOCKED); `manual-acceptance-checklist.md` §5 ATT-03 | Demo host owner (#75 is closed; reopen or file a new ticket) | Provision the host, generate the root CA and server certificate, install the root on the phone, and record rows 1–12 with redacted output |
| **OPS-01** — demo-host readiness under the runtime identity | T3-05, R4, R5, SPEC B9 | A provisioned demo host, a dedicated Windows service identity, a dedicated SQL login, `AttendTa_Demo`, a staged `PLAYWRIGHT_BROWSERS_PATH`, and an approved disconnection method | `docs/demo-host-readiness-evidence.md` checks 1–10 (all BLOCKED) | Demo host owner (8.14 follow-up) | Provision the host and identity, run `--demo-readiness` as the service identity, and record checks 1–10 |
| **OPS-02** — backup, restart/redeploy and restore to a separate target | T3-05, R5, SPEC B9 | A real host, an approved backup destination, an approved empty restore target, a maintenance window and an approver | `docs/storage-survival-evidence.md` (every stage BLOCKED) | Backup/restore owner (#76 is closed) | Execute `storage-backup-restore.md` end to end and record every stage with redacted evidence |
| **M-AUD-01** — `activity_logs` column permissions under the runtime SQL login | T3-05, R5 | A provisioned runtime SQL login on a demo host | `docs/demo-host-readiness-evidence.md` check 3; `manual-acceptance-checklist.md` §5 M-AUD-01 | Demo host owner (8.14 follow-up) | Connect as the runtime login and record effective table- and column-level `UPDATE`/`DELETE` permissions on `activity_logs`; unblocks when OPS-01 provisions the login |
| **M-AUTH-01** — 8-hour non-sliding cookie expiry | T3-05, R1 | An 8-hour idle observation of a real browser session | `manual-acceptance-checklist.md` §5 M-AUTH-01 (cookie flags recorded `HttpOnly`, `SameSite=Lax`, session-scoped; expiry not observed) | Human tester or release manager | Sign in, leave the session idle past 8 hours, re-request a management route and record the outcome |
| **CERT-03 signatory** — human approval of the certificate signatory | T3-05, R1, SPEC B10 | The human confirming the authorized signatory name, title and signature as official | `manual-acceptance-checklist.md` §5 CERT-03 signatory sub-field (configured: `Christian Kyle Ducay` / `Organization President` / packaged signature asset; approval **NOT RECORDED**) | Project owner / human signatory | Confirm or replace the values and record the approval; SPEC §20 says the signatory "remains pending and must be supplied before official certificate issuance" |
| **M-ADM-01 mixed race** — demotion racing a deactivation of the last two administrators | T3-05, R1 | A demotion and a deactivation of the last two administrators interleaving inside the same transaction window, which two live sessions cannot produce by hand | `manual-acceptance-checklist.md` §5 M-ADM-01 (mixed race BLOCKED; the two same-kind races are automated in `AdminAccountDeactivationTests` / `AdminRoleAssignmentTests`) | Human tester or release manager | Either record a human decision that the two same-kind automated races are sufficient, or file a follow-up to script the mixed pair in `tests/ASI.Basecode.IntegrationTests`; this ticket does not change tests |

### 7.2 CI exceptions the human owns

| Exception | Observed | Decision the human owns |
|---|---|---|
| 5 merged PRs with a failed check (#155, #157, #199, #200, #201) | §4.2 — two compile breaks on 2026-11-06, then the same single sign-in 500 test on three consecutive merges on 2026-11-07 | Whether the branch discipline failure is acceptable, and whether the sign-in 500 was a defect that reached `dev` or a harness flake. No current evidence shows a live sign-in defect at `4eac1b9`. |
| 5 merged PRs with no check run (#5, #6, #8, #84, #85) | §4.3 — all predate the workflow's first run (42009022564, 2026-11-05T10:14:18Z, PR #86) | Whether pre-CI history is acceptable in a release that also contains 112 post-CI merges |
| #229 merged while its run was in progress | §4.4 — merged 2026-11-12T14:22:53Z, run concluded `SUCCESS` at 14:28:14Z | Whether a merge that did not wait for its own green result is acceptable on the head-adjacent commit |
| The release PR itself gets no CI run | §4.6 — `ci.yml` triggers on `pull_request` to `dev` only | Whether to accept the dev-head attestation in §3 as the substitute, or to file a separate ticket to widen the workflow trigger |

### 7.3 The peer-approval exception

No merged PR carries a GitHub review approval (§4.5). The epic constraint says every PR to `dev`
needs green CI **and one peer approval**. Only two PRs (#225, #226) have a committed formal review,
and those review the evidence records rather than approving the code. The human owns the decision
whether committed review artifacts plus the 107 green runs are sufficient, or whether the release
should be held for retroactive approvals.

This record does not resolve any of §7.1–§7.3 and does not recommend a verdict.

## 8. Release pull request — opened, not merged

**Opened as [#231](https://github.com/rainyDayssss/AttendTa/pull/231) on 2026-11-12, after this record
was written. Not merged. Do not treat this section as an approval.**

The pull request is `release/82-main-submission` → `main`, not `dev` → `main`. A `dev` → `main` pull
request would merge the planning, evidence, agent and tooling trees into the submission branch; §6
explains what is excluded and why. The release branch carries `dev`'s application content only.

### 8.1 Title

```
release: AttendTa MVP submission tree for main
```

### 8.2 Command — already run

This command was run on 2026-11-12 and produced [#231](https://github.com/rainyDayssss/AttendTa/pull/231).
It is recorded here so the pull request can be reproduced or reopened if it is closed without merging:

```powershell
gh pr create --repo rainyDayssss/AttendTa --base main --head release/82-main-submission `
  --title "release: AttendTa MVP submission tree for main" `
  --body-file "$env:TEMP\release-pr-body.md"
```

If the reviewer would rather carry the whole record as the PR body, point `--body-file` at
`_bmad-output/initiative-attendta/release-preparation-record.md` instead. Opening the pull request is
not the decision: the merge is, and that remains a human step.

### 8.3 Reviewable body

```markdown
## Summary

Releases the AttendTa MVP to `main` (`174d1fd`) as a **submission tree**: the application, schema,
tests and setup docs only. `dev` (`4eac1b9`) is 396 commits ahead of `main` and 0 behind.

This is a release, not a feature. It carries the finished MVP — platform access, master data, events
and registration, check-in, administration, certificates and participation reports — and leaves the
planning, evidence, agent and tooling trees on `dev`.

## What `main` receives

11 top-level entries, 503 files:

```
.gitignore  ASI.Basecode.Data  ASI.Basecode.Resources  ASI.Basecode.Services
ASI.Basecode.WebApp  ASI.Basecode.sln  README.md  database/  docs/  global.json  tests/
```

1 137 of the 1 664 files `dev` changes are deliberately excluded: `.agents/` (558), `_bmad-output/`
(398), `.opencode/` (88), `_bmad/` (32), `.impeccable/` (17), `docsss/` (13), `bruno/` (12),
`newdocs/` (8), `tools/` (2) and nine root files including `AGENTS.md`, `DESIGN.md`, `PRODUCT.md`,
`.gitmodules`, `spec/` and `.github/`. `spec/` is a private submodule and `ci.yml` triggers only on
pull requests to `dev`, so neither belongs on `main`.

Diff against `main`: **529 files changed, 71 376 insertions(+), 65 300 deletions(-)**.

## Three decisions not on `dev`

| File | Change |
|---|---|
| `tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` | The venue-upload fixture now points at `TestAssets\venue-upload.jpg` inside the test project, copied byte-identically from the planning folder it used to reference. Without it the solution fails to build once `_bmad-output/` is excluded (`MSB3030`). |
| `global.json` | Shipped. It is not an SDK pin — its only content selects the `Microsoft.Testing.Platform` runner, without which the documented `dotnet test --project` command fails with `MSB1001`. |
| `README.md` | Dropped the two references to paths outside the release tree, so every link resolves. |

Nothing in `ASI.Basecode.*/` differs from `dev`.

## Release preparation record

`_bmad-output/initiative-attendta/release-preparation-record.md` on `dev` — the exact commits, the
re-run Tier 1 gates, the Tier 2 CI and review history, the R1–R6 evidence map, the open blockers and
this pull request body.

## Evidence package

| Requirement | Closed by | Status |
|---|---|---|
| R1 — SPEC §20 acceptance | 8.4 manifest, 8.5 matrix, 8.6 checklist, 8.7 journey, 8.8 gate, 8.10 cleanup, 8.11 mock gate | PASS with 5 BLOCKED rows and 2 BLOCKED sub-fields |
| R2 — 26 functional IDs + FR-35/FR-36 | `demo-walkthrough-evidence.md` (8.7) | PASS — 36/36 FR rows, 26/26 functional-ID rows |
| R3 — SM-4 concurrency on SQL Server | `execution-manifest-fa9ae82.md` (8.4) | PASS — zero overbooking, zero duplicate attendance/certificates/registrations |
| R4 — phone scan over hotspot HTTPS | `docs/hotspot-https-evidence.md` (8.2), `docs/demo-host-readiness-evidence.md` (8.14) | BLOCKED — no host, no root CA on a phone |
| R5 — storage survival, backup and restore | `docs/storage-survival-evidence.md` (8.3) | BLOCKED — no host, no restore target |
| R6 — Tier 3 passes and dev released to main | `tier-3-verification-record.md` (8.8) + this PR | HALF OPEN — T3-05 blocked on 7 rows, T3-07 blank |

## Verification — Tier 1 re-run on `4eac1b9` (2026-11-12)

| Gate | Result |
|---|---|
| `dotnet build ASI.Basecode.sln --configuration Release` | exit 0 — 0 warnings, 0 errors |
| `dotnet build ASI.Basecode.sln --configuration Release --no-incremental` | exit 0 — 0 warnings, 0 errors |
| `npm --prefix ASI.Basecode.WebApp run css:build` | exit 0 — Tailwind v4.3.4, 166 ms |
| `git diff --exit-code -- ASI.Basecode.WebApp/wwwroot/css/app.css` | exit 0 — no difference |
| `npm --prefix ASI.Basecode.WebApp run test:js` | exit 0 — 5 tests, 5 pass, 0 fail |
| `dotnet test --project tests/ASI.Basecode.Services.Tests/... --configuration Release` | exit 0 — 321 total, 321 succeeded, 0 failed, 0 skipped |
| `dotnet build ASI.Basecode.sln --configuration Release` on the release tree | exit 0 — 0 warnings, 0 errors |
| `dotnet test --project tests/ASI.Basecode.Services.Tests/... --configuration Release` on the release tree | exit 0 — 321 total, 321 succeeded, 0 failed, 0 skipped |
| `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` on the release tree | exit 0 — **479 total, 479 passed, 0 failed, 0 skipped, 0 not run**, 714.6 s |
| `npm --prefix ASI.Basecode.WebApp run test:js` on the release tree | exit 0 — 5 tests, 5 pass, 0 fail |

The 479-test integration suite was **run on the release tree**, not just on `dev`, so the release
branch is not resting on the 8.4 manifest's validity argument. On `dev` it was not re-run:
`git diff --name-only fa9ae82..4eac1b9` touches only `_bmad-output/`, so the 8.4 manifest is still
valid under its own rule. The full CI job also ran green on PR #230 (`build-and-test`, 7m 28s, run
43242205714) for the same application content.

## CI status — reported as observed

- **117** PRs merged to `dev`: **107** with a `SUCCESS` check run, **5** with `FAILURE`
  (#155, #157, #199, #200, #201), **5** with no check run at all (#5, #6, #8, #84, #85 — all
  predate the workflow's first run, 42009022564 on PR #86).
- #229 merged 5m21s **before** its run concluded; the run has since concluded `SUCCESS`.
- **No** merged PR carries a GitHub review approval. 7 carry a `COMMENTED` review; 0 carry
  `APPROVED`; `reviewDecision` is empty on all 117. Peer review exists as committed
  `formal-review.md` records for #225 and #226 only.
- **This pull request will produce no CI run.** `ci.yml` triggers on `pull_request` to `dev` only.
  The substitute is the dev-head attestation above.

## Open blockers — the human owns these

1. **ATT-03** — camera scan on the real hotspot HTTPS origin (host, root CA, phone).
2. **OPS-01** — demo-host readiness under the runtime identity (host, service identity, SQL login).
3. **OPS-02** — backup, restart/redeploy and restore to a separate target (host, destinations, approver).
4. **M-AUD-01** — `activity_logs` column permissions under the runtime SQL login.
5. **M-AUTH-01** — 8-hour non-sliding cookie expiry (needs an 8-hour idle observation).
6. **CERT-03 signatory** — human approval of the signatory name, title and signature.
7. **M-ADM-01 mixed race** — demotion racing deactivation of the last two administrators.

## Test plan

- [ ] Re-run the six Tier 1 gates on the release commit and confirm the same results.
- [ ] Spot-check one row of the R1–R6 map against the record it cites.
- [ ] Confirm `git diff --name-only fa9ae82..4eac1b9` still touches only `_bmad-output/`.
- [ ] Confirm `git ls-tree --name-only release/82-main-submission` lists exactly the ten entries above.
- [ ] Walk one role route per role (Administrator, Officer, Member) on a local Production build.

## Reviewer checklist

- [ ] The release commit and the merge target in the record match this PR's base and head.
- [ ] Every PR the record calls green has a `SUCCESS` check run; every exception is named.
- [ ] The peer-approval exception (§7.3) has an explicit decision.
- [ ] The seven blockers (§7.1) have an explicit decision: accept, or hold the release.
- [ ] The sign-off block in the record is filled by the human, not by an agent.

## Not in this PR

No product change, no Razor, CSS, schema or `.github/workflows/ci.yml` change. Nothing in
`ASI.Basecode.*/` differs from `dev`. The only non-`dev` items are the three decisions above: the
test-project fixture path, the shipped `global.json`, and two README lines. The planning, evidence,
agent and tooling trees stay on `dev`.
```

## 9. Sign-off block

| Field | Value |
|---|---|
| Record complete | Yes — Tier 1 re-run per gate, Tier 2 exceptions named, evidence package mapped to R1–R6, blockers listed, pull request prepared |
| Release commit | `ce699b09ef62304a5330cf44321f59e7ac0d0a53` (`release/82-main-submission`) |
| Integration commit | `4eac1b9bb72c26b54a02547e5a95dc428eab5d9a` (`dev` head) |
| Merge target | `174d1fd7e20c6a8b9095d9a351e1d058da550511` (`main`) |
| Operator | AI agent (subagent), 2026-11-12 |
| Reviewer | — (blank, to be filled by the human) |
| Peer approval reference | — (blank) |
| **Merge decision** | — (blank: merge / hold / retest) |
| Approver | — (blank) |
| Date | — |
| Affected retests | — (blank until the decision is recorded) |

**The release is not approved by this file.** The merge decision, the approver, the date and the
retest fields are empty on purpose. R1 passes with blocked rows, R4 and R5 are blocked, R6 is half open
on T3-05 and T3-07, the peer-approval exception in §7.3 is undecided, and the release pull request in
§8 is open (#231) but unmerged. Only the human sign-off in this section can close the gate.

## 10. Defects found

No product defect was found in this ticket's scope. The build, the stylesheet, the JS contracts and
the 321-test service suite are green on the recorded head, and the evidence package reports 0 FAIL
and 0 product defects across 8.4–8.11.

| Id | Area | Observation | Severity | Filed |
|---|---|---|---|---|
| D-1 | `tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` | The integration test project copies its venue-upload fixture from `_bmad-output/initiative-attendta/ux-attendta/imports/prototype-assets/event-cyber.jpg`. The test project therefore cannot build unless a planning folder is present, which blocks any release tree that excludes `_bmad-output/`. Observed as `error MSB3030: Could not copy the file ... because it was not found`. Worked around on the release branch (§6.4); the coupling itself remains on `dev`. | Low — build portability, no runtime effect | Not filed; follow-up needed on `dev` |
| D-2 | `global.json` | The file is not an SDK pin. Its only content is `{"test": {"runner": "Microsoft.Testing.Platform"}}`, which is what makes `dotnet test --project` work. Any tree that omits it — a release tree, a submission archive, a fresh clone without it — fails the README's documented test command with `MSB1001: Unknown switch. ... Switch: --project`. Found while verifying the release tree, not on `dev`. | Low — tooling, no runtime effect | Not filed; the release tree now ships it (§6.4) |

Three record-level observations, none of them a product defect and none of them fixed here:

1. **The record's first draft read the stale local `main` branch** — it reported `main` at `e81a404`
   and a divergence of 389. The remote `main` is `174d1fd` and the divergence is 396. The ticket
   plan's 396 was correct; the draft's 389 came from the local ref. Corrected in §1.
2. **The ticket plan's expected CI causes for #200/#201 (SQL Server container login timeout) are
   wrong** — the observed cause is the same single sign-in 500 test that failed in #199. The
   container readiness step logged one transient refusal and then succeeded in every run, including
   the 107 green ones. Corrected in §4.2.
3. **The ticket plan's green count (112) double-counts the five pre-CI PRs** — the observed split is
   107 green / 5 failed / 5 never ran. Corrected in §4.1.
