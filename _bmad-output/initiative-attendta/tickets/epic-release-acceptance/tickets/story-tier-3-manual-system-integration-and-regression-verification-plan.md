---
title: 'Tier 3 manual system integration and regression verification'
type: 'chore'
ticket: '8'
created: '2026-11-11'
status: 'built'
baseline_revision: '84b3f54e58d127f993d55c45a9287dc9b267dda5'
route: 'full'
route_source: 'auto'
risk: 'high'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 1
context:
  - '{project-root}/_bmad-output/initiative-attendta/manual-acceptance-checklist.md'
  - '{project-root}/_bmad-output/initiative-attendta/demo-walkthrough-evidence.md'
  - '{project-root}/_bmad-output/initiative-attendta/execution-manifest-fa9ae82.md'
  - '{project-root}/_bmad-output/initiative-attendta/acceptance-coverage-matrix.md'
  - '{project-root}/docs/hotspot-https-evidence.md'
  - '{project-root}/docs/storage-survival-evidence.md'
  - '{project-root}/docs/demo-host-readiness-evidence.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Tier 3 is the release gate: the whole team signs off cross-feature compatibility, the integrated build and database migrations on the shared `dev` build before `dev` goes to `main`. No Tier 3 record exists. 8.4 (#77) produced the automated manifest, 8.6 (#79) the manual checklist, 8.7 (#80) the demo walkthrough and 8.2/8.3 (#75/#76) the environment evidence — but nothing states, for one exact commit, which rows are proven, which are still blocked, and that the human decision is outstanding.

**Approach:** Write one Tier 3 verification record that cites the existing evidence with its validity rule, executes only the rows nothing else covers (integrated build and database migration checks), marks every unmet row BLOCKED with its missing input named, and leaves the team-lead/tester decision blank for the human. No second checklist, no second screen, no product change.

## Boundaries & Constraints

**Always:** Record the exact tested commit, environment and date. Cite 8.4/8.6/8.7/8.2/8.3 rather than repeating them; a docs-and-evidence-only commit does not invalidate the 8.4 manifest, a source or test change does. Every row carries a result; BLOCKED rows name the missing input and the owning evidence file. A closed issue or a merged tool is never host evidence. Database migration checks run only on disposable `AttendTaMVP_Test` or `AttendTa_IT_<guid>` databases. Defects are filed as issues and referenced, not fixed here.

**Never:** Build or restyle a screen, change product behaviour, Razor, CSS, schema, tests or CI. Re-run the 479-test integration suite while the manifest is still valid. Author a third full checklist. Mark the human decision for the team, or mark a blocked row PASS. Touch `AttendTa_Demo`, a shared/production database or `AsiBasecodeDB`. Put secrets, hashes, tokens, cookies or private paths in the record.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Covered row | Evidence file at a still-valid commit | Row reads REUSED with the source record and commit named | Stale source → row becomes BLOCKED, not PASS |
| Missing environment | No demo host, phone or restore target | Row reads BLOCKED with the exact missing input and owning file | Never inferred from a closed issue or a merged tool |
| Migration check | Disposable `AttendTa_IT_<guid>` with legacy `venues/<name>` rows | Legacy keys prefixed to `uploads/venues/<name>`; rerun changes nothing; guard refuses other database names; over-length path aborts with no rows changed | Nonzero exit and the record names the failing assertion |
| Human decision | All rows collected | Sign-off block present with the decision blank | Record states sign-off is blocked, never approved |

</frozen-after-approval>

## Code Map

- `_bmad-output/initiative-attendta/tier-3-verification-record.md` (new) — the deliverable.
- `_bmad-output/initiative-attendta/execution-manifest-fa9ae82.md` — 8.4 evidence: commit `fa9ae82`, 321 service / 479 integration, 0 failed; states its own invalidation rule. Read-only.
- `_bmad-output/initiative-attendta/manual-acceptance-checklist.md` — 8.6 execution record on `d8046bf`: 8 PASS, 5 BLOCKED, 2 BLOCKED sub-fields, evaluator decision blank. Read-only.
- `_bmad-output/initiative-attendta/demo-walkthrough-evidence.md` — 8.7 / #80 execution record on `84b3f54`, merged at `71e7373`: 68 journey rows, 36 FR rows, 26 functional-ID rows, 7 blocked host/phone rows; the T3-04 evidence. Read-only.
- `_bmad-output/initiative-attendta/acceptance-coverage-matrix.md` — 8.5 coverage map and residuals, not execution evidence. Read-only.
- `_bmad-output/initiative-attendta/finale-test-cases.md` §"One finale execution programme" — the 8.8 step definition. Read-only.
- `_bmad-output/initiative-attendta/demo-script.md` — 8.15 presenter beats; the journey definition, not execution evidence. Read-only.
- `docs/hotspot-https-evidence.md`, `docs/demo-host-readiness-evidence.md`, `docs/storage-survival-evidence.md` — every real-host row `BLOCKED`; source of ATT-03, OPS-01, OPS-02, M-AUD-01. Read-only.
- `database/migrate-venue-image-paths.sql` — allow-list guard (lines 9–15), `venues/%` update (47–49); the migration under test. Read-only.
- `database/attendta-mvp-schema.sql`, `reset-local-mvp-test-db.sql`, `seed-local-admin.sql`, `seed-event-types.sql` — disposable fixture. Read-only.
- `docs/git_qa_guidelines.pdf` Tier 3 row — scope is cross-feature compatibility, integrated build, database migrations.
- `docs/database-setup.md` — the setup sequence the migration check mirrors. Read-only.

## Tasks & Acceptance

**Execution:**
- [x] `_bmad-output/initiative-attendta/tier-3-verification-record.md` — author the record: header (ticket, tested commit, environment, operator, date), result legend (PASS/FAIL/BLOCKED/REUSED/NOT RUN/PASS (none required)/BLANK), evidence-reuse table with each source's commit and validity verdict, the Tier 3 matrix, the blockers table, and the sign-off block with the decision blank.
- [x] Same file — Tier 3 matrix rows: T3-01 cross-feature compatibility (REUSED from 8.6 UI-01/UI-02 and the 8.4 suite), T3-02 integrated build (executed: `dotnet build` Release, `css:build`, `test:js`, service tests), T3-03 database migration checks (executed), T3-04 request-to-certificate/report integration journey (8.7, REUSED after PR #226 merged the 8.7 record), T3-05 environment rows ATT-03/OPS-01/OPS-02/M-AUD-01/M-AUTH-01/CERT-03-signatory/M-ADM-01 (BLOCKED), T3-06 affected retests after fixes (none required — no source change since `fa9ae82`), T3-07 human Tier 3 decision (blank).
- [x] Same file — execute T3-03 against a disposable `AttendTa_IT_<guid>` database: schema load, legacy `venues/<name>` fixture rows, run the migration, assert the `uploads/venues/` prefix, rerun for idempotence, assert the guard refuses a non-allow-listed name, and assert an over-length path aborts with no rows changed. Record the commands and observed output.
- [x] Same file — fill the blockers table: ATT-03 phone scan, OPS-01 demo host, OPS-02 backup/restore, M-AUD-01 runtime SQL login, M-AUTH-01 8-hour expiry, CERT-03 signatory approval; each with owner and unblocking action.

**Acceptance Criteria:**
- Given the finished record, when a reader looks for the tested commit, then one exact commit and its environment are stated, and every reused source names its own commit.
- Given the 8.4 manifest, when the record cites it, then the record shows that only documentation and evidence commits landed since `fa9ae82`, so the suite is not re-run.
- Given a row whose evidence file is still BLOCKED, when the record reports it, then the row reads BLOCKED with the missing input and owning file named.
- Given the migration check, when it runs, then the prefix, idempotence, guard and over-length assertions all pass on a disposable database and no other database is touched.
- Given the completed record, when the team lead and testers read it, then the sign-off block is present and the decision is blank; the record never states the release is approved.

## Implementation Notes

- 2026-11-11 - one new file, `_bmad-output/initiative-attendta/tier-3-verification-record.md`; no product, Razor, CSS, schema, test or CI change. `dotnet build ASI.Basecode.sln --configuration Release` gave 0 warnings, 0 errors on `84b3f54`.
- T3-02 re-ran the integrated build, `css:build`, `test:js` and the 321 service tests; all exit 0. The 479-test integration suite was not re-run because `git diff --stat fa9ae82..84b3f54` touches five files, all under `_bmad-output/`, so the 8.4 manifest's own invalidation rule still holds.
- T3-03 ran on a disposable `AttendTa_IT_t3mig523f203e` created from the canonical schema: legacy `venues/<name>` keys gained exactly one `uploads/` prefix, the rerun was a no-op, the allow-list guard refused a non-allow-listed database with exit 1 before any DML, and an over-length path aborted with exit 1 while the row checksum stayed unchanged. The prefix, idempotence and over-length assertions are also automated by `VenueImagePathMigrationTests` inside the 479 suite; only the guard is new evidence. A second probe database proved the guard; both were dropped afterwards.
- The record originally left T3-04 (8.7 demo journey) and T3-05 (seven host/device/signatory/race rows) BLOCKED with the missing input and owning file named, and left T3-07 and section 8 blank. Sign-off is stated as blocked, never approved.
- 2026-11-12 — after PR #226 merged `demo-walkthrough-evidence.md` (8.7 / #80) into `dev` at `71e7373`, T3-04 was reclassified **REUSED** against that record, which was executed on the same `84b3f54` commit. The 8.7 blocker row was removed; §3 gained the 8.7 source; §8 now names only T3-05 as open. No product, Razor, CSS, schema, test or CI change.
- A pre-existing untracked `story-full-functional-demo-script-walkthrough-across-all-26-featur-plan.md` (ticket 7 / #80) was in the working tree before this run. It is not part of this change and was left untracked and unmodified.

## Plan Change Log

- 2026-11-12 — human renegotiation: T3-04 reclassified from BLOCKED to REUSED after PR #226 merged the 8.7 / #80 execution record. The frozen Approach's "marks every unmet row BLOCKED" is superseded for this row. Added the 8.7 source and the `demo-script.md` definition to §3 and the plan's Context/Code Map, corrected the `demo-script.md` owner to 8.15 / #135, removed the 8.7 blocker from §7, and updated §8 to name only T3-05 as open. Documentation-only; no source, test, schema or CI change.
- 2026-11-12 — formal walkthrough review (adversarial, edge-case, structure, prose) on `1f5e8d9`: 12 substantive findings, 5 gate-affecting, all patched in this change.

## Review Triage Log

- 2026-11-11 — quick lens, 5 findings: 0 high, 4 medium, 1 low, 0 false, 0 maybe-false. Routes: 5 patch, 0 rejected.
- 2026-11-12 — formal walkthrough review (adversarial, edge-case, structure, prose) on `1f5e8d9`: 12 substantive findings, 5 gate-affecting, all patched in this change.

| # | Finding | Verdict | Route | Evidence / action |
|---|---|---|---|---|
| 1 | Plan Code Map says the 8.6 record is "31 pass, 5 BLOCKED" | low | patch | Verified against `manual-acceptance-checklist.md` §7 Summary: `PASS 8`, `BLOCKED 5`, `BLOCKED sub-fields 2`. The 31 was a row total, not a pass count. Bullet corrected; the record's own §3 already read 8 PASS. |
| 2 | Record attributes `docs/demo-host-readiness-evidence.md` to `8.2 / #74` | medium | patch | Verified: `git log` shows the file created by `da3661b` under `[BMAD 8.14]` (issue #150), and its own header states closed issues including #74 are not proof. Naming a closed tooling ticket as owner points the unblocking action at a party that does not own the evidence. Attribution corrected to `8.14 / #150` in §3 and both §7 owner cells. |
| 3 | Record §6 commands are not reproducible: `$probe` never assigned, the probe `CREATE DATABASE` and the `users`/over-length inserts missing, and a non-GUID name claimed as the `AttendTa_IT_<guid>` form | medium | patch | Verified by reading §6: the guard and over-length assertions could not be re-run from the record. The missing statements were recorded verbatim and the parenthetical reworded to the `AttendTa_IT_` prefix and length the guard actually checks (`database/migrate-venue-image-paths.sql:9-15`). |
| 4 | 8.6's second BLOCKED sub-field (M-ADM-01 mixed race) is absent from the release gate, and T3-01 omits last-admin concurrency | medium | patch | Verified against `manual-acceptance-checklist.md:276` (`BLOCKED sub-fields 2`) and `execution-manifest-fa9ae82.md:31-32` (last-admin concurrency inside the 479). A blocked sub-field missing from the only release-gate record is a real gate gap. Added as a seventh §7 blocker row and to T3-05; last-admin concurrency added to T3-01. |
| 5 | T3-03 and the Design Note claim the migration checks were run by no earlier ticket, but `VenueImagePathMigrationTests` already asserts the prefix, idempotence and over-length abort inside the 479 | medium | patch | Verified: `tests/ASI.Basecode.IntegrationTests/VenueImagePathMigrationTests.cs` loads the canonical schema, inserts a legacy key, runs the script, asserts `uploads/`, reruns for idempotence and asserts the over-length abort. T3-03 now cites the test and states only the allow-list guard is new; the Design Note claim was narrowed to the guard. |

## Design Notes

8.6 already executed the manual remainder, 8.4 already ran the combined suite, and 8.7 executed the continuous journey, so 8.8 is a decision record, not a third checklist. Only two rows are genuinely unexecuted and locally runnable: the integrated build on this exact commit, and the venue image path migration's allow-list guard, which Tier 3 names and no earlier ticket ran — the prefix, idempotence and over-length assertions are already automated in `VenueImagePathMigrationTests` inside the 479 suite, so only the guard is new evidence. T3-04 now reuses the 8.7 record, which was executed on the same `84b3f54` commit and merged at `71e7373`. Everything else is either reused with a stated validity rule or blocked on a human or host input.

The migration check is recorded as commands and observed output rather than a new script: the ticket is evidence-only, and a one-off run on a disposable database matches the 8.6 precedent without adding tooling that 8.9 would then have to maintain.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — expected: 0 warnings, 0 errors.
- `npm --prefix ASI.Basecode.WebApp run css:build` — expected: Tailwind build succeeds.
- `npm --prefix ASI.Basecode.WebApp run test:js` — expected: all JS tests pass.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` — expected: all service tests pass.
- Migration check via `sqlcmd` against a disposable `AttendTa_IT_<guid>` — expected: prefix, idempotence, guard and over-length assertions pass; database dropped afterwards.
- `git diff --stat` — expected: only the record and this plan file.
- `git diff --check` — expected: clean.

**Manual checks (if no CLI):**
- Open the record: every Tier 3 row has a result, every BLOCKED row names its missing input and owning file, and the sign-off decision is blank.
