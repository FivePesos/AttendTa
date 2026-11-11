---
title: 'Recorded manual SPEC §20 acceptance checklist verification'
type: 'chore'
ticket: '6'
created: '2026-11-11'
status: 'built'
baseline_revision: 'd8046bf6fe41643f06d2cac64560e115161cb821'
route: 'full'
route_source: 'auto'
risk: 'high'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context:
  - '_bmad-output/initiative-attendta/finale-test-cases.md'
  - '_bmad-output/initiative-attendta/acceptance-coverage-matrix.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** SPEC §20 requires every acceptance item to pass with recorded evidence, but 8.6's manual matrix exists only as a coverage map and a test plan. No executed, per-scenario record exists, and the manual remainder (UI-01, UI-02, ATT-03, CERT-03, OPS-01, OPS-02, M-AUTH-01, M-MD-01, M-ATT-02, M-ADM-01, M-CERT-02a/b, M-AUD-01) has never been run.

**Approach:** Write one recorded checklist that turns every SPEC §20 bullet and every matrix case into preconditions, numbered steps, expected outcome and an evidence field. Author all rows first, then execute every case the local build can carry against the seeded disposable database; mark host/phone/storage rows BLOCKED with the missing input named; cite 8.4's manifest and 8.12's walkthrough instead of repeating them; leave the evaluator sign-off for the human.

## Decisions

- 2026-11-11 — the agent executes every locally runnable case; the two live-session races are recorded as manual demonstrations, not proofs, and the automated same-kind races own the proof.
- 2026-11-11 — the record lives at `_bmad-output/initiative-attendta/manual-acceptance-checklist.md`, beside the matrix and the 8.4 manifest.

## Boundaries & Constraints

**Always:** Evidence only — no product behaviour, Razor, CSS, schema or test change. Record the tested commit, environment and date. Cite 8.4 and 8.12; never re-run the suite or the geometry harness. Every SPEC §20 bullet and every matrix case appears exactly once with a result. BLOCKED rows name the missing input and the owning evidence file. A race row says "demonstration, not proof". Defects are filed as issues and referenced, not fixed here. No secrets, hashes, tokens or private paths in the record.

**Never:** Build a second screen or change presentation. Close the ticket without the human sign-off. Mark a row PASS from a planning document or a tool-only run. Reset a shared or production database, or run destructive scripts against `AttendTa_Demo` or `AsiBasecodeDB`.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Local case runs | Seeded `AttendTaMVP_Test`, local app, Chromium/Poppler | Row records PASS/FAIL with tested commit, environment and observed evidence | FAIL row names the observed result and a filed defect id |
| Host/phone/storage case | No real host, phone or restore target | Row is BLOCKED and names the missing input plus the owning evidence file | Never inferred PASS from tooling or a closed issue |
| Race case | Two live sessions, sub-second interleaving | Row records the observed interleaving as a demonstration | Not reproducible → BLOCKED, with the automated race cited |
| Missing tool | Chromium or Poppler absent | Row is BLOCKED as an environment gap | Recorded as blocked, not a product failure |

</frozen-after-approval>

## Code Map

- `_bmad-output/initiative-attendta/manual-acceptance-checklist.md` (new) — the deliverable record.
- `_bmad-output/initiative-attendta/acceptance-coverage-matrix.md` — §1 cases AUTH-01…MOCK-01, §4 manual cases, §6 residuals; read-only source.
- `_bmad-output/initiative-attendta/finale-test-cases.md` — case inventory and preconditions; read-only source.
- `_bmad-output/initiative-attendta/execution-manifest-fa9ae82.md` — 8.4 manifest to cite (321 service, 479 integration, commit `fa9ae82`).
- `_bmad-output/initiative-attendta/demo-mock-register.md` — MOCK-01 evidence (all rows `removed`, last `149791b`).
- `.../story-final-frontend-walkthrough-targeted-refactor-and-impeccable.md` §"Walkthrough evidence — Session G" — 8.12 route/width/dead-end evidence to cite.
- `docs/AttendTa-System-Specification.md` §20 (lines 377–393) — nine authoritative bullets, including the pending signatory.
- `docs/hotspot-https-evidence.md`, `docs/demo-host-readiness-evidence.md`, `docs/storage-survival-evidence.md` — all BLOCKED; source of ATT-03, OPS-01, OPS-02, M-AUD-01 rows.
- `database/reset-local-mvp-test-db.sql`, `attendta-mvp-schema.sql`, `seed-local-admin.sql`, `seed-event-types.sql`, `seed-demo-scenario.sql` — disposable fixture on `AttendTaMVP_Test`.
- `_bmad-output/initiative-attendta/demo-script.md` §2–§6 — provisioning, accounts, starting state, role beats.
- `ASI.Basecode.Data/PrivateFileStore` (signature validation) + `ASI.Basecode.WebApp/Controllers/VenueController.cs` + `Views/Venue/_Form.cshtml` — M-MD-01 premise re-check; read-only.
- `ASI.Basecode.WebApp/appsettings.json` — certificate signatory values for CERT-03; read-only.

## Tasks & Acceptance

**Execution:**
- [x] `manual-acceptance-checklist.md` — author the record: header (ticket, tested commit, environment, operator, date), result legend (PASS/FAIL/BLOCKED/REUSED), one section per SPEC §20 bullet with a one-line automated citation, every manual row as PREPARED with id, requirement, preconditions, numbered steps, expected outcome, evidence and result, plus sign-off and defects tables.
- [x] Same file — add the automated coverage index: every matrix §1 case id mapped to its test classes and the `fa9ae82` manifest line; MOCK-01 mapped to `demo-mock-register.md`.
- [x] Same file — execute UI-01 (role, route, 200 and no dead link per demo-script §4–§6 beats; not the 8.12 geometry harness) and UI-02 (keyboard, dialog focus, 200% zoom at 1440/1024/768/390).
- [x] Same file — execute CERT-03 (retained landscape A4 PDF via Poppler, fonts, branding, recipient/event/date/number, no sample artefact as official output) with a signatory sub-field: name/title/signature source and whether the human approved it; unapproved signatory → BLOCKED for sign-off.
- [x] Same file — execute M-MD-01 (re-check the matrix's "PNG not accepted" premise, then PNG through save → thumbnail → replace → delete; a stale premise is a matrix note, not a defect), M-CERT-02b (edit event after issuance), M-CERT-02a (second app on a spare port with a fresh `Common__SetupRoot` whose `certificates` folder is ACL-denied; assert no issued row, no retained file, no audit row, clean retry; if not achievable → BLOCKED).
- [x] Same file — execute M-AUTH-01 (non-persistent cookie flags, then re-check after 8 h; an incomplete wait is BLOCKED with the flags recorded as partial evidence).
- [x] Same file — execute M-ATT-02 (two sessions, invalidation dialog held open, 5 attempts, recorded as demonstration) and M-ADM-01 (attended-data survival sequential PASS/FAIL; demote-vs-deactivate race BLOCKED as not reproducible manually, citing the automated same-kind races).
- [x] Same file — record ATT-03, OPS-01, OPS-02 and M-AUD-01 as BLOCKED with the exact missing input, citing the three evidence files.
- [x] Same file — fill the defects table and the sign-off block; leave the evaluator decision blank.

**Acceptance Criteria:**
- Given the finished record, when an evaluator opens it, then all nine SPEC §20 bullets and every case id in matrix §1, §4 and §6 appear exactly once, each with a result.
- Given an authored manual row, when it has not been run, then its result reads PREPARED or BLOCKED, never PASS.
- Given an executed row, when it reads PASS, then its evidence field names the tested commit, environment and the observation made.
- Given a host, phone, storage or race case, when it cannot run, then its row is BLOCKED and names the missing input and the owning evidence file.
- Given the 8.4 manifest and 8.12 walkthrough, when the checklist cites them, then no automated suite or geometry walkthrough is repeated.
- Given a defect, when found, then it is filed as a GitHub issue and referenced from the row, with no product code change in this branch.
- Given the completed record, when the human evaluator signs, then the sign-off block is filled; until then the ticket is not done.

## Implementation Notes

- 2026-11-11 — one new file, `manual-acceptance-checklist.md`; no product, Razor, CSS, schema or test change. `dotnet build ASI.Basecode.sln --configuration Release` → 0 warnings, 0 errors on `d8046bf`.
- Executed against a reset and seeded `AttendTaMVP_Test` (schema, admin, event types, demo scenario) with a throw-away `Common__SetupRoot`; app on `localhost:5080` plus a second instance on `5091` for M-CERT-02a. Chromium and Poppler were both present, so the matrix's "missing tool" row had no trigger.
- The implementer reported a defect (D-1, venue edit keeps the old photo). Re-checked against `VenueController.Edit` (`PhotoRequired = false`), `VenueService.UpdateVenueAsync` (`if (newPath != null)`) and `_Form.cshtml` ("…to replace the current one"): keeping the current photo is the designed edit behaviour, so D-1 was removed and the row records PASS. No defect was filed.
- The matrix's "PNG not accepted end-to-end" premise is stale: `PrivateFileStore.NormalizeExtension` admits `.png`, `ValidateSignature` checks the PNG magic number, and a real PNG created, replaced and re-served. Recorded as a coverage-map note, not a defect; the matrix is 8.5's record and was not edited.
- Five rows are BLOCKED on inputs no local run supplies: ATT-03 (host, certificate, phone), OPS-01 and M-AUD-01 (demo host and runtime SQL login), OPS-02 (approved backup and restore targets), M-AUTH-01 (8-hour idle observation; cookie flags recorded as partial evidence). CERT-03's signatory sub-field is BLOCKED on human approval.
- The local `AttendTaMVP_Test` database now holds the acceptance fixtures; reset it before the next rehearsal.

## Plan Change Log

## Review Triage Log

- 2026-11-11 — quick lens, 10 findings: 0 high, 1 medium, 3 low, 6 false. Routes: 4 patch, 6 rejected.

| # | Finding | Verdict | Route | Evidence / action |
|---|---|---|---|---|
| 1 | M-CERT-02a: `AT-2026-000023` cannot belong to registration 24 | false | rejected | Verified against `CertificateLedgerService.cs:240` (`AT-{year}-{registrationId:D6}`) and the database: registration 23 issued `AT-2026-000023`, registration 24 had no row. The wording was ambiguous, so the row now names each registration and records a re-run same-review retry of registration 24, which issued `AT-2026-000024`. |
| 2 | UI-02: 200% zoom not checked on officer routes | low | patch | Real coverage gap in the claim. The row now states 200% zoom on the Administrator and Member routes only, and that the officer routes are covered at 390px, which is narrower than the 616px zoom viewport. |
| 3 | UI-02: status tabs, reason dialogs and cancel/reopen not exercised | medium | patch | Real. Executed with real key presses on `/CheckIn?eventId=3`: three tabs, visible focus ring, ArrowRight and Enter switch tabs; the invalidate-attendance reason dialog traps focus, refuses an empty reason, and returns focus on Escape and Cancel. Recorded in the row. |
| 4 | §7: "9 SPEC bullets" under REUSED contradicts §3 | low | patch | Corrected to "7 of 9 bullets wholly REUSED; B5 and B9 BLOCKED; B10 signatory BLOCKED". |
| 5 | M-ATT-02 should be BLOCKED, not PASS as a demonstration | false | rejected | The plan's Decisions and I/O matrix explicitly allow an observed interleaving to be recorded as a demonstration, and the row says "demonstration, not proof" and cites the automated race. |
| 6 | M-ADM-01 counted as PASS though the race is BLOCKED | false | rejected | The row's Result and §7's "BLOCKED sub-fields" row both state the mixed race is BLOCKED; the PASS covers the sequential attended-data survival only. |
| 7 | Plan execution checkboxes left unticked | low | patch | Housekeeping; all nine tasks are now ticked. |
| 8 | §6 re-lists case ids, breaking "exactly once" | false | rejected | §6 is titled a cross-reference and points at the §5 rows; each case has exactly one result row and one execution. |
| 9 | CERT-03 headline PASS despite the signatory being BLOCKED | false | rejected | The Result field, §3 B7 and §7's sub-field row all state the signatory is BLOCKED; the evaluator decision is blank, which is the "BLOCKED for sign-off" the plan asks for. |
| 10 | Header SDK 10.0.401 vs manifest 10.0.403 | low | patch | Real; the header now records that the cited manifest ran on 10.0.403 while this run used 10.0.401 on the same OS build. |

## Design Notes

One record file, not an edit of `finale-test-cases.md`: that file declares itself a plan with every case "not executed", and 8.7/8.8 need a single place that shows what already passed. BLOCKED is the honest result for rows whose evidence files are still BLOCKED; a closed issue or a merged tool is not host evidence. The two races are demonstrations because a human clicking two windows cannot produce a sub-second interleaving; the automated suite already proves the same-kind races.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln` — expected: 0 warnings, 0 errors on the tested commit.
- Coverage check: every case id in matrix §1/§4/§6 appears in the checklist — expected: no missing id, no duplicate.
- `git diff --stat` — expected: only `_bmad-output/initiative-attendta/manual-acceptance-checklist.md` and this plan file.
- `git diff --check` — expected: clean.

**Manual checks (if no CLI):**
- Open the checklist: every row has preconditions, steps, expected outcome, evidence and result; every BLOCKED row names its missing input.
