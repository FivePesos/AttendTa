# Walkthrough: PR #222 — whole IntegrationTests suite execution manifest

Target: https://github.com/rainyDayssss/AttendTa/pull/222

## Block 1 — Intent

- [x] Block 1 — Intent — accepted

Source: ticket 8.4, [story-whole-integrationtests-suite-execution-on-sql-server.md](../tickets/epic-release-acceptance/tickets/story-whole-integrationtests-suite-execution-on-sql-server.md), tracked as issue #77 under the epic-release-acceptance epic. Pasted verbatim from the ticket:

> Run the existing service and SQL integration suites plus the important cases authored/reused by 8.5 against the integrated real-data build. Use ATTENDTA_TEST_SQL pointing at local SQL master; each class owns AttendTa_IT_<guid>, never the demo/shared database. Include the existing JavaScript test:js and CSS build checks. Confirm Chromium and Poppler are installed on the local test host (CI already provisions both); environment failures are blockers, not passes. Record one execution manifest with commit, environment, commands and results; reuse it for 8.6–8.8 instead of repeating full runs without code changes. Existing PR CI remains enabled throughout development.

> Verify: On the tested commit all existing and important new suites pass, with zero overbooking or duplicate approval/attendance/certificate and last-admin races safe. Environment failures and skipped critical cases are explicit blockers, not green results; commands and evidence are attached to #77.

This pull request delivers only the last sentence of that ticket: the single manifest, [execution-manifest-fa9ae82.md](../execution-manifest-fa9ae82.md). It adds no tests and changes no product code. The suites it records were already run; what lands here is the evidence, pinned to one commit so that 8.6, 8.7 and 8.8 can cite it instead of re-running the seven-minute SQL suite.

## Block 2 — Broad strokes

- [x] Block 2 — Broad strokes — accepted

The change is one new markdown file, [execution-manifest-fa9ae82.md](../execution-manifest-fa9ae82.md): 46 lines added, nothing deleted, no code touched. It is a record rather than a mechanism — four sections that together let a later reader decide whether a green result can be trusted and reused.

Open these in order:

1. [The status line](../execution-manifest-fa9ae82.md) — states the invalidation rule: a code change after this commit makes the manifest void and requires a new run.
2. [The field table](../execution-manifest-fa9ae82.md) — the pin: ticket, branch, tested commit, date, OS, .NET and Node versions, database, PDF tooling, browser.
3. [The commands-and-results table](../execution-manifest-fa9ae82.md) — five invocations, each with exit code, a human-readable result and a duration.
4. [Scope covered](../execution-manifest-fa9ae82.md) — the four race classes the ticket names, asserted as zero duplicates and zero overbooking.
5. [Notes and non-defects](../execution-manifest-fa9ae82.md) — the explanation for the EF `fail:` lines and SQL error 334 entries that a future reader will meet in the integration log.

Read in that order the file answers four questions: what was run, on what, with what result, and why the log looks alarming.

## Block 3 — The pin

- [x] Block 3 — The pin — accepted

The field table exists so the evidence cannot drift. It fixes one commit and one environment, and states in the status line that a code change after that commit voids the manifest. Everything 8.6, 8.7 and 8.8 later cite hangs off this pin.

The pin is checkable and it holds. The tested commit, `fa9ae82a1ea97e83e924c5b2ef2217dcd3bff1ec`, was the tip of `dev` when the manifest was written, and it is the merge of pull request #220 from `feature/78-acceptance-cases`. That matters more than it looks: the ticket requires the run to include the important cases authored by 8.5, and 8.5 is exactly what that merge brought in. The precondition is satisfied by construction, not by assertion.

Two small imprecisions in an otherwise precise pin:

- The commit is described as the "origin/dev tip". In this repository `origin` is a personal fork and `upstream` is the canonical repo, so the label is ambiguous about which remote was read.
- The browser row lists two Chromium builds, `chromium-1228` and `chromium-1248`, without saying which one the PDF checks actually used. A pin whose whole purpose is precision should name one.

## Block 4 — The results

- [x] Block 4 — The results — accepted

Five rows, each an invocation with an exit code, a human-readable result and a duration. Together they cover every check the ticket names: build, service suite, SQL integration suite, CSS build and JavaScript tests. 805 individual checks in total, dominated by the 479-test integration suite at 413.6 seconds.

The strongest sign that this is a real record rather than a plausible one is that the two .NET invocations match, word for word, the commands the repository documents for itself at that commit. The service row uses `dotnet test --project ... --no-build --no-restore --configuration Release`; the integration row runs the built DLL directly with `-parallelMode none`. Neither shape was invented for the manifest.

The weakness is in what the table measures. It records counts, and the ticket's actual acceptance question — zero overbooking, zero duplicate approval, attendance and certificate, last-admin races safe — is answered in prose under "Scope covered", not by a number in this table. A skeptical reader cannot point at a row and say "there is the zero".

One reproducibility hole, now confirmed against CI. The two npm rows give no working directory and no install step. CI runs `npm --prefix ASI.Basecode.WebApp ci` and then `npm --prefix ASI.Basecode.WebApp run css:build` ([ci.yml](../../../.github/workflows/ci.yml)); there is no root `package.json`, so the manifest's bare `npm run css:build` fails from the repository root and skips the restore that makes the result reproducible.

Two independent checks support the record rather than the claim. The three .NET rows match CI verbatim, and the 321 service-test count matches what the two earlier walkthrough logs recorded for the same suite, in [the 8.11 log](../walkthrough-8-11-remove-prototype-data/walkthrough-8-11-remove-prototype-data-log.md) and [the PR #215 log](../walkthrough-pr-215-backend-hardening/walkthrough-pr-215-backend-hardening-log.md). The author's numbers have been consistent across runs.

## Block 5 — The non-defects

- [x] Block 5 — The non-defects — accepted

This is the section that earns the manifest its keep. The integration log from a green run is not clean: it contains 13 EF `fail:` lines and 9 SQL error 334 entries. Without an explanation, the next person to open that log concludes the suite is broken. The manifest names the cause — test-only triggers created on purpose by negative-path tests — and names three of them: `TR_TestFailVenueUpdate`, `TR_TestFailEventPublicationAudit`, `TR_TestFailEventCoverUpdate`, plus an unnamed readiness trigger.

It also disposes of two lesser alarms: six EF `Model.Validation[20601]` warnings about database-generated defaults with no sentinel value, and a modified [app.css](../../../ASI.Basecode.WebApp/wwwroot/css/app.css) that is a CRLF stat-cache artefact with byte-identical content and is not committed.

The honest limitation is that this section cannot be checked from the pull request. The raw log is not attached anywhere, so the reader is asked to trust the classification of those lines. That is normal for execution evidence, but it means the section is testimony, not proof.

One attribution deserves a second look. The three named triggers raise their own errors — 51044, 51052 and 51053 — and the readiness trigger raises 51991. Error 334 is none of those; it is the doomed-transaction error SQL Server reports after a statement fails inside a transaction. The manifest's note is probably right about the cause and imprecise about the mechanism: the 334 entries are the aftermath of those triggers, not the triggers' own exceptions. Without the log attached, a reader cannot tell which reading is meant.

## Block 6 — What the manifest does not carry

- [x] Block 6 — What the manifest does not carry — accepted

The ticket's verification clause ends with "commands and evidence are attached to #77". They are not. Issue #77 has no comments at all. The evidence lives in the repository and the pull request body says "Refs #77", which links the two, but nothing is attached to the issue itself. If the issue is the audit trail, this pull request does not close that loop.

Three other things are absent:

- No raw log, no per-test-class breakdown, and no link to a CI run. The 479 figure is not independently decomposable.
- "No environment blockers: LocalDB, Chromium and Poppler were all present" is asserted. The field table gives versions, but no command output shows the checks that confirmed presence.
- This is the first execution manifest in the repository — no other `execution-manifest-*` file exists. There is therefore no precedent for the format that 8.6–8.8 were promised; this file becomes the template by default rather than by design.
- No cross-reference to [acceptance-coverage-matrix.md](../acceptance-coverage-matrix.md). 8.5 added seven important cases and the matrix names them; the manifest reports only a total, so a successor cannot tell from this file whether those seven ran.
- The invalidation rule is undefined. "A code change after this commit invalidates it" does not say whether that means product code, tests, or the documentation commits 8.6–8.8 will themselves make. Read strictly, the reuse promise collapses the first time anyone commits.
- The exact `ATTENDTA_TEST_SQL` value is not recorded, only that master was reached through it. A LocalDB master string is not a secret and would make the environment reproducible.

## Block 7 — Periphery

- [x] Block 7 — Periphery — accepted

- [The pull request body](https://github.com/rainyDayssss/AttendTa/pull/222) — restates the five results as a table, adds the non-defect note and the `Refs #77` link.
- Branch `feature/8.4-integration-suite-execution`, base `dev`, one commit, `21c8b1f`.
- No CI, schema, source or test change — consistent with the ticket's "test-only, no Impeccable requirement" delivery note.
- The manifest sits under `_bmad-output/initiative-attendta/`, which is tracked in git, so the evidence is versioned alongside the code it describes rather than living in a chat thread.
