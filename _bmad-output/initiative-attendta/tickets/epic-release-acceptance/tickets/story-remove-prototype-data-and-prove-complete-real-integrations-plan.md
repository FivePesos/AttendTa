---
title: 'Remove prototype data and prove complete real integrations'
type: 'feature'
ticket: '11'
created: '2026-11-09'
status: 'built'
baseline_revision: '2d05a3580645e91aac43770caebe473224ac8f5a'
route: 'oneshot'
route_source: 'auto'
risk: 'high'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
followup_review_recommended: false
context: []
warnings: []
deferred: []
---

## Intent

**Problem:** 8.13 deleted the demo seam and every register row now reads `removed`, but nothing has
proven it. The 8.11 gate requires evidence that fails while any part remains: no register id, no
`DemoData` key and no demo folder in the source or the published output; the published build starts
with the retired switch on, no token key and a non-demo database; and every real flow works end to
end against one coherent database. The gate's own search already fails: register ids M-03, M-05 and
M-06 survive in five source comments, `CheckInWindowFlagTests` assembles the retired section name
from two words so the search cannot see it, and the register's M-01 row names `005f899` (the
baseline merge) instead of the commit that deleted the seam.

**Approach:** Run every gate check and record the evidence in a walkthrough record. Clear only the
residue that makes the gate's own search fail — five stale register-id comments and the split
literal — and fill in the register's removal commits so each row carries the commit that actually
deleted its demo code. No behaviour changes; findings that need a product fix are recorded, not
fixed here.

## Implementation Notes

Oneshot: the only code edit is comment text plus one test literal (well under 100 lines); the bulk
of the work is running the gate and writing the evidence record, which is not delegable without
re-narrating the whole investigation.

Scope, in order:

1. **Search residue.** Drop the register-id parenthetical from
   `ASI.Basecode.Services/Services/CheckInService.cs:14` (M-05),
   `ASI.Basecode.Services/Services/CheckInLookupService.cs:14` (M-06),
   `ASI.Basecode.WebApp/wwwroot/js/features/scanner.js:1` (M-05),
   `ASI.Basecode.WebApp/wwwroot/js/features/scanner-camera.js:1` (M-05) and
   `ASI.Basecode.WebApp/Styles/features/passes.css:1` (M-03). The surrounding sentences stay; only
   the id goes. Rebuild `app.css` because `passes.css` is a Tailwind source.
2. **Search dodge.** `tests/ASI.Basecode.IntegrationTests/CheckInWindowFlagTests.cs:73` builds
   `"Demo" + "Data"` so the 8.11 grep cannot see it. Use the literal and say in the comment that
   this test is the only place the retired key may appear, because it is the check.
3. **Register commits.** `demo-mock-register.md`: M-01 `005f899` → `149791b`; fill M-02
   `6668425`, M-03 `81f721e`, M-05 `2a8b9b5`, M-09 `0abc4f8`, M-13a `c0c0935`, M-14a `8230045`,
   M-14b `f232933`, M-15a `48e0231`, M-15b `1853ad9`. Note M-13b's residue was deleted by
   `04157b3` (M-13c's commit) in the row's detail, not by rewriting its recorded commit.
4. **Evidence.** `_bmad-output/initiative-attendta/walkthrough-8-11-remove-prototype-data/`
   `walkthrough-8-11-remove-prototype-data.md` plus `-log.md`, following the existing walkthrough
   convention. Record: the 20-row register verdict, the startup check, the search transcript, and
   the end-to-end run with its environment and commit.

Do not touch `ASI.Basecode.WebApp/Readiness/DemoReadiness.cs`: `--demo-readiness` is a host
readiness probe that legitimately names `AttendTa_Demo`, and it is not the seam.

### Implementation record

- Files touched: `CheckInService.cs`, `CheckInLookupService.cs`, `scanner.js`,
  `scanner-camera.js`, `passes.css` (comment text only), `CheckInWindowFlagTests.cs` (split
  literal replaced by the literal), `demo-mock-register.md` (commit column and the M-13b note),
  this plan, and the new walkthrough record and log.
- Decision: the register's "removed with its commit" verdict is 8.11's own output, so the nine
  rows that recorded no commit and the M-01 row that recorded the baseline merge were corrected
  here instead of being sent back to tickets that merged months ago. Every corrected commit was
  proved by `git show --stat`, not inferred from the ticket id.
- Decision: the five register-id comments were deleted rather than reworded, because the register
  defines those ids as deletion markers and the sentences read correctly without them.
- Decision: the inertness test now names the retired section literally. Leaving the split literal
  would keep the gate's search blind to the one place the name must appear, which is the check
  itself.
- Surprise: `dotnet ASI.Basecode.WebApp.dll` from the repository root fails with
  `Common:SetupRoot must be configured`; the published build must be started from the publish
  directory so its `appsettings.json` is the content root. The first startup attempt failed for
  that reason only.
- Surprise: the scenario seed inserts registrations directly, so seeded passes have no `qr_token`
  and cannot scan or download. The pass and scanner checks therefore used the token minted by the
  real approval command. Recorded as a seed limitation, not a defect.
- Surprise: sign-in rotates the antiforgery cookie, so a page token read before login is rejected
  with 400 afterwards; the token must be re-read after signing in.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln -warnaserror` -- expected: 0 warnings, 0 errors.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` -- expected: all pass.
- `$env:ATTENDTA_TEST_SQL="Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` -- expected: all pass, including `RetiredSeamSwitchKeyIsInertWhenEnabled`.
- `rg -n "DemoData|DemoStore|DemoSeedService|DemoDataRegistration|demo-pass-token" --glob '!**/bin/**' --glob '!**/obj/**' --glob '!_bmad-output/**' --glob '!**/node_modules/**' .` -- expected: one hit, the inertness test.
- `rg -n "M-0[1-9]|M-1[0-5][ab]?" ASI.Basecode.Data ASI.Basecode.Services ASI.Basecode.Resources ASI.Basecode.WebApp --glob '!**/bin/**' --glob '!**/obj/**' --glob '!**/node_modules/**'` -- expected: no hits.
- `dotnet publish ASI.Basecode.WebApp/ASI.Basecode.WebApp.csproj -c Release -o <temp>` then the same two searches over `<temp>` -- expected: no hits.
- Start the published build with `DemoData__Enabled=true`, no `DemoData__TokenKey` and a database other than `AttendTa_Demo`, then request `/Account/Login` -- expected: 200, no startup refusal.
- `npm --prefix ASI.Basecode.WebApp run css:build` then `git diff --exit-code -- ASI.Basecode.WebApp/wwwroot/css/app.css` -- expected: no diff after the rebuild is committed.

**Manual checks:**
- One coherent database, one session, one role per flow: approved account request → member,
  registration → pass, open window → scan → manual lookup → invalidation, completion → review
  approve/decline → issue → view/download, account and membership deactivate/reactivate, and the
  participation report. Evidence in the walkthrough record, not in this plan.

## Review Triage Log

### 2026-11-09 — Review pass

- verdicts: 2 findings — high 0, medium 0, low 2, false 0, maybe-false 0
- note: the quick lens was launched as instructed but did not return across three collection
  attempts, so the inspection below was performed in this session against the same diff, plan and
  `AGENTS.md`. Recorded as a deviation, not as a lens result.
- findings:
  - `[low]` `[patch]` `demo-mock-register.md:7` — the register's introduction still said "the code
    keeps the same ids in a comment on each demo class", which this change makes false because it
    deleted the last five such comments. Patched: the sentence is now past tense and states that
    the register is the only place the ids appear.
  - `[low]` `[patch]` `walkthrough-8-11-remove-prototype-data-log.md` §6 — the pass tally read "42
    checks … 39 passed", which does not match the two runs it summarises (38 + 13 = 51 checks, 42
    passed, 9 failed). Patched: the tally is corrected and the nine failures are the ones listed.

### 2026-11-09 — Review pass (quick lens, reported after finalize)

- verdicts: 20 findings — high 0, medium 1, low 6, false 13, maybe-false 0
- findings:
  - `[false]` `[reject]` `CheckInService.cs:14` broken sentence — the committed comment reads "The real door scanner. The screen composes the published-event read and the real attendance log; …", two complete sentences. The lens read the pre-change text.
  - `[false]` `[reject]` `CheckInLookupService.cs:14` same splice — the committed comment reads "The real manual lookup and manual check-in. The search is a registration-scoped, server-paged read …". Same cause.
  - `[false]` `[reject]` comment-only JS edits widen risk — no statement differs and the files are served as-is, not bundled; the edit is the gate's own search requirement.
  - `[low]` `[patch]` M-13b row names only `975c08b` — the row now reads `removed (975c08b; residue 04157b3)`, so the commit that deleted the leftover fallback is on the row itself, not only in the detail section.
  - `[false]` `[reject]` M-13b description inverted — the row carries a commit, not a description; the detail section already states that `975c08b` left the fallback and `04157b3` deleted it.
  - `[low]` `[patch]` log commands not reproducible — added a note that the driver and the sqlcmd wrappers were temporary files outside the repository, and that every check is enumerated with its request and observed value.
  - `[low]` `[patch]` unexplained 404 on the pass download — the log now says `PassesController.Qr` has no 404 path, so the 404 came from the client or routing before the action, and records the curl evidence.
  - `[low]` `[patch]` cross-member 403 unevidenced — the log now records that a different member's session requesting the same QR returned the same 403 with the same 25-byte body.
  - `[false]` `[reject]` upcoming-event window unevidenced — the record makes no pass-window claim; its window row is about check-in and cites `events.is_check_in_open`.
  - `[medium]` `[patch]` audit half of the requirement unevidenced — the ticket asks for "matching report and audit". Re-ran both passes from a worktree at the committed head against a fresh database and read `activity_logs`: 18 rows covering the whole chain. Recorded in the record and the log.
  - `[false]` `[reject]` check counts inconsistent — corrected to 51/42/nine before this triage; the current text is internally consistent.
  - `[false]` `[reject]` seed limitation misdescribed — the record says the passes "show the empty state", which is the page's own empty-state message; `GetMyPassesAsync` skipping tokenless rows is why that state renders.
  - `[false]` `[reject]` `AT-2026-000001` vs `registration_id=15` — different rows: the certificate belongs to the event-1 registration, 15 is the event-3 registration whose token was scanned. The record never equates them.
  - `[low]` `[patch]` unterminated frontmatter — both new records opened with `---` and never closed it. Removed the opening delimiter so they start with the heading, matching the walkthroughs that carry no frontmatter.
  - `[false]` `[reject]` plan expects 20 rows — the plan states no row count; the register has 21 rows.
  - `[false]` `[reject]` `followup_review_recommended: false` unsupported — the rule is a patched `high` or two patched `medium`; this pass patched one `medium`.
  - `[false]` `[reject]` `--no-build` configuration unstated — the log names `--no-build` on both test commands and the `dotnet build` that precedes them.
  - `[false]` `[reject]` published-output scope ambiguous — the log names `-c Release`, the `-o` directory and that both searches ran over it.
  - `[low]` `[patch]` M-04 full SHA — shortened to `7f6ae22` so every row uses the same form.
  - `[false]` `[reject]` `DemoData` literal not disclosed — the record's search table names `CheckInWindowFlagTests.cs:73` as the single hit and explains it.

## Auto Run Result

- Summary: ran the 8.11 gate and recorded its evidence. Every register row resolves to the commit
  that deleted its demo code; the published build starts with the retired switch on, no token key
  and a non-demo database; the source and published output hold no register id, demo folder or
  `DemoData` key; and every required flow ran end to end against one disposable database through
  the real services.
- Files changed:
  - `ASI.Basecode.Services/Services/CheckInService.cs` — dropped the M-05 register id from the doc comment.
  - `ASI.Basecode.Services/Services/CheckInLookupService.cs` — dropped the M-06 register id from the doc comment.
  - `ASI.Basecode.WebApp/wwwroot/js/features/scanner.js` — dropped the M-05 register id from the header comment.
  - `ASI.Basecode.WebApp/wwwroot/js/features/scanner-camera.js` — dropped the M-05 register id from the header comment.
  - `ASI.Basecode.WebApp/Styles/features/passes.css` — dropped the M-03 register id from the header comment.
  - `tests/ASI.Basecode.IntegrationTests/CheckInWindowFlagTests.cs` — names the retired section literally instead of splitting it.
  - `_bmad-output/initiative-attendta/demo-mock-register.md` — corrected M-01's commit, filled nine missing commits, noted the M-13b residue, corrected the introduction.
  - `_bmad-output/initiative-attendta/tickets/epic-release-acceptance/tickets/story-remove-prototype-data-and-prove-complete-real-integrations-plan.md` — this plan.
  - `_bmad-output/initiative-attendta/walkthrough-8-11-remove-prototype-data/walkthrough-8-11-remove-prototype-data.md` and `-log.md` — the verification record and its log.
- Review findings: 22 findings across two passes — 1 `medium`, 8 `low`, 13 `false`. All 9 real
  findings were patched; none deferred; the 13 `false` findings are rejected with their refutations
  in the triage log. The quick lens returned only after the run had finalized, so it is recorded as
  a second pass.
- Follow-up review recommendation: false (no patched `high`, and only one patched `medium`).
- Verification performed: `dotnet build ASI.Basecode.sln -warnaserror` 0/0; services 321/321;
  integration 460/460; `css:build` with no `app.css` diff; published build started from its own
  directory with `DemoData__Enabled=true`, no token key and `AttendTa_IT_811verify`, serving
  `/Account/Login` 200; both searches clean over source and published output; a 51-check
  end-to-end run over one disposable database, repeated from a worktree at the committed head to
  read the 18-row `activity_logs` trail; the database was dropped afterwards.
- Residual risks: the end-to-end run used HTTP sessions rather than a browser, so it proves the
  services and controllers, not the rendered interaction; 8.12 owns the frontend pass. One
  `Invoke-WebRequest` run returned 404 for the pass download and never reproduced (curl and later
  runs returned 200 with the attachment header); the action has no 404 path, so the cause is
  client-side or routing. The scenario seed mints no pass tokens, so the pass and scanner checks
  used a token produced by the real approval command.
