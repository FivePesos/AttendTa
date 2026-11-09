# Review log: PR #215 — remove the demo seam and unify the shared backend rules

Target: https://github.com/rainyDayssss/AttendTa/pull/215 (head `149791b`, base `dev`)

## 1 — Orientation — target and intent established

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-08T00:00:00+08:00

- Action: Read PR #215 body and file list, ticket 8.13 (`story-final-backend-hardening-and-cross-module-integration-cleanup.md`), and the plan (`...-plan.md`: intent contract, boundaries, I/O matrix, code map, tasks, implementation notes, review triage log, auto-run result). Fetched the PR head as local branch `pr-215` from `upstream`.
- Result: Intent is the plan's frozen Problem/Approach. Scope is backend-only: delete the demo seam (register M-01), unify participation/certificate rules in `ParticipationFacts`, replace the report's five reads with one snapshot, and close the deferred 3.14/5.10 findings. 77 files, +1113 / -1494. Single commit on top of `dev` at `005f899`.
- Evidence: `gh pr view 215`; `git fetch upstream refs/pull/215/head:refs/heads/pr-215`; `git diff --stat 005f899 pr-215`; plan file on `pr-215`.
- Open: Narrative blocks 2+ not yet written. Working tree is on `dev`, so file reads must go through `git show pr-215:<path>` until the branch is checked out.

## 2 — Block 1 (intent) — second opinion

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-08T00:00:00+08:00

- Action: Fresh subagent (no review context, no formal skill) compared the diff against the ticket, the plan's intent contract, boundaries, I/O matrix and task list; independently built and ran both suites in a throwaway worktree.
- Result: Verdict "yes, with caveats". Build 0/0 with `-warnaserror`; services 315/315; integration 457/457. Intent delivered, boundaries hold, no material boundary contradiction. Findings: (a) `GetUsersAsync` has zero production consumers so the async conversion is signature churn presented as work; (b) `AttendanceLogRow.IsValid`, `MyAttendanceRow.IsValid` and `ParticipationReportDetailRow.IsAttendanceInvalidated` still encode the valid-attendance rule outside `ParticipationFacts`; (c) plan records no evidence for the ticket's safe-file, hook-registration, authorization and time-guard Verify items; (d) `deferred-work.md` still lists two items this PR closes; (e) register M-01 names baseline `005f899` not the removal commit, and `demo-mock-register.md:286` still says the token switch "remains until 8.13"; (f) scope creep: `YearBreakdown` always prints `Year N` (copy change pulled from PR #214) and `CheckInWindowFlagTests` splits the `DemoData` literal to dodge the 8.11 grep.
- Evidence: subagent report; `ParticipationFacts.cs:70-78`; `CertificateRepository.cs:26`; `ParticipationReportRepository.cs:33-40`; `ParticipationReportService.cs:189`; `CheckInWindowFlagTests.cs:73`; `CanonicalSchemaMappings.cs:304-310`; `demo-mock-register.md:85,286`.
- Open: Reviewer has not decided on any finding. Hardest three named: report totals now depend on the Member/User inner joins with no `/Reports` render test; `IsCertificateEligible`'s optional `eventItem` plus the new `Completed` requirement on the decision path; the `OccurredAt` UTC `HasConversion` on an indexed, defaulted column.

## 3 — Block 1 (intent) — test

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-08T00:00:00+08:00

- Action: Created a throwaway worktree at `C:\Users\Administrator\AppData\Local\Temp\opencode\pr215-test` on `pr-215` (`149791b`). Ran `dotnet build "ASI.Basecode.sln" -c Release -warnaserror`, both suites with `ATTENDTA_TEST_SQL` pointed at MSSQLLocalDB `master`, the plan's 8.11 grep, the new `RetiredSeamSwitchKeyIsInertWhenEnabled` fact on its own, and a search for any integration test hitting `/Reports`.
- Result: Build 0 warnings / 0 errors. Services 315/315. Integration 457/457. Grep for `DemoData|DemoStore|DemoSeedService|demo-pass-token` outside `_bmad-output` returns no hit (exit 1). Inert-seam fact 1/1 — host starts and serves `/Account/Login` with the retired switch on. No integration test references `/Reports`, confirming the plan's residual risk. All three PR-body numbers reproduce exactly.
- Evidence: worktree `pr215-test`; `dotnet test tests/ASI.Basecode.Services.Tests/...`; `dotnet test tests/ASI.Basecode.IntegrationTests/...`; `rg -n "DemoData|DemoStore|DemoSeedService|demo-pass-token" --glob '!**/bin/**' --glob '!**/obj/**' --glob '!_bmad-output/**' .`; `rg -n "Reports/" tests/ASI.Basecode.IntegrationTests`.
- Open: Verified the second opinion's DTO finding in code — `IAttendanceRepository.cs:61`, `:74` and `IParticipationReportRepository.cs:43` each re-encode the invalidation rule instead of calling `ParticipationFacts`. `CheckInWindowFlagTests.cs:73` confirmed assembling `"Demo" + "Data"`. Neither is covered by a test. Reviewer has not decided on any finding.

## 4 — Block 1 (intent) — formal review and patches

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Ran `bmad-code-review` on chunk 1 (shared rules: `ParticipationFacts`, the report snapshot, the certificate rule, plus their two test files; 9 files, 1327 diff lines). Four lenses: blind-hunter, edge-case-hunter, verification-gap, intent-alignment. 37 raw findings → 23 rejected, 14 grouped into 11 findings (4 medium, 7 low). Two decision-needed resolved by the reviewer as option 1 each (delete the orphaned `NoShows`/`AttendanceRates`; restore the "Unspecified" year branch). All 11 patches applied in the `pr215-test` worktree.
- Result: Build 0 warnings / 0 errors with `-warnaserror`. Services 316/316 (315 − 1 deleted fact + 2 new). Integration 459/459 (457 + 2 new facts). Patches: composed report test against real SQL; twin-vs-expression tests; completion filter test for cancelled/waitlisted seats; blank-reason-on-invalidated-attendance test; deleted `NoShows`/`AttendanceRates` and their assertions; three DTO members and `CheckInLookupService` routed through `ParticipationFacts` (`ConfirmedLookupRow` now carries `InvalidatedAt` instead of a bool); deleted the `IsValidAttendance(bool)` overload; `IsCertificateEligible`'s event made required with a null guard on the decide path; dead cancelled-event fixture removed; `DetailRow` uses its argument; year label restored.
- Evidence: `C:\Users\Administrator\AppData\Local\Temp\opencode\pr215-triage-chunk1.md`; plan `## Code Review` section (11 patch boxes checked); `git diff --stat` in `pr215-test` (15 files, +181/−95).
- Open: Patches are uncommitted in the `pr215-test` worktree; the reviewer's own repo is untouched. Block 1 not yet accepted. Chunks 2–4 not reviewed.

## 5 — Block 2 (broad strokes) — draw

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Drew the mechanism as a four-panel SVG: the deleted seam and its four wiring points; `ParticipationFacts` as the one home for five rules with the SQL expression and in-memory twin side by side plus the caller list; the report's five reads collapsing into `GetSnapshotAsync` and feeding four panels; the certificate decision and issuance converging on `IsCertificateEligible`, with the completion query noted.
- Result: No new claims beyond the code map. Two layout fixes after the first render (an overlapping note in panel 4, an arrow landing outside the predicate box). Presented in the Review pane as an SVG.
- Evidence: [block-2-mechanism.svg](block-2-mechanism.svg)
- Open: Block 2 not yet accepted. Chunks 2–4 of the formal review not run.

## 6 — Block 3 (demo seam) — test

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Started the app from the `pr215-test` worktree with `DemoData__Enabled=true`, no token key, `ASPNETCORE_ENVIRONMENT=Development`, against LocalDB `AttendTaMVP_Test`; requested `/Account/Login`. Then ran `--demo-readiness` separately, grepped source plus `bin`/`obj`, checked the `DemoData/` folder is gone, listed the surviving CLI switches, and compared the old `DemoSeedService` (from `005f899`) against the new `CertificateLedgerFlowTests.SeedAttendanceAndReviewsAsync`.
- Result: Host started and served `/Account/Login` 200 with the retired key set — the acceptance criterion holds. `--demo-readiness` still runs and reports its checks (exit 1 in this scratch worktree: no setup root, no running app — expected). `DemoData/` absent; grep over source and build output clean. `Program.cs` keeps `--demo-readiness`, `--hash-password`, `--hash-password-env` and the hotspot command; no token switch. Seed rewrite is faithful: same three states (invalidated → NotEligible with the same reason, one declined with the same reason, one Issued with `AT-YYYY-NNNNNN` and no file path). The old seed was idempotent and derived from actual rows; the test helper does not need either.
- Evidence: app log `sh_1bc01297d0012DJIoHS6sIhheI.out`; `Invoke-WebRequest http://localhost:5199/Account/Login` → 200; `dotnet run -- --demo-readiness`; `rg "DemoData|DemoStore|DemoSeedService|demo-pass-token"`; `git show 005f899:ASI.Basecode.WebApp/DemoData/DemoSeedService.cs`.
- Open: Found `_bmad-output/initiative-attendta/deferred-work.md:13,47` still describing `DemoSeedService` and `DemoStore.GetPassToken` as live, and `:23` still in future tense about the store's removal. Not patched; reviewer has not decided.

## 7 — Block 3 (demo seam) — records fixed and patches pushed

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Marked the two deferrals whose subject this PR deletes as `resolved (149791b)` in `_bmad-output/initiative-attendta/deferred-work.md` (the `DemoSeedService` pager-volume entry and `DemoStore.GetPassToken`), and changed the resolved attendance-source entry from "is removed by 8.13" to "was removed by 8.13 (149791b)". Then committed the 11 review patches and the records fix on `feature/151-final-backend-hardening` and pushed to `origin`.
- Result: Two commits — `7df7a30` (15 files, +181/−95, the review patches and the plan's `## Code Review` section) and `768cc70` (deferred-work, +5/−4). Pushed `149791b..768cc70`; PR #215 head is now `768cc70` with three commits. `build-and-test` is pending.
- Evidence: `git log --oneline -3`; `gh pr view 215 --json headRefOid,commits`; `gh pr checks 215`.
- Open: CI result not yet observed. Block 3 not yet accepted. Walkthrough artifacts remain untracked in the main repo. Chunks 2–4 of the formal review not run.

## 8 — Block 3 (demo seam) — PR body reconciled

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Rewrote the PR #215 body because it was stale after the two pushed commits. Updated the verification numbers (services 315 → 316, integration 457 → 459 with the arithmetic), added the `deferred-work.md` change to the seam section, replaced the "one quick review pass" paragraph with an account of the chunked walkthrough review (37 raw, 23 rejected, 11 patched in `7df7a30`), and qualified the `/Reports` residual risk now that a composed service-plus-repository test exists.
- Result: Body updated via `gh pr edit --body-file`. `build-and-test` on run 42500886550 was still in progress at the time; a `gh run watch` was started to report the result.
- Evidence: `gh pr edit 215`; `gh pr checks 215`; `gh run view 42500886550`.
- Open: CI conclusion not yet observed. Block 3 not yet accepted. Walkthrough artifacts still untracked in the main repo.

## 9 — Block 3 (demo seam) — CI green

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Watched run 42500886550 on `768cc70` to completion.
- Result: `build-and-test` passed in 9m11s. Every step green, including `Run service tests` and `Run SQL integration tests`, so the pushed patches hold on the CI database as well as LocalDB. The only annotations are two runner deprecation notices (Node 20 actions, the `ubuntu-latest` migration), both pre-existing and unrelated.
- Evidence: `gh run watch 42500886550 --exit-status`; job 128793161803.
- Open: Block 3 not yet accepted. Walkthrough artifacts still untracked in the main repo. Chunks 2–4 of the formal review not run.

## 10 — Block 4 (one rule) — test

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Ran `ParticipationFactsTests` as a baseline (7/7), then mutated the twins twice to check the pinning test has teeth: (1) `HoldsSeat` also accepted `Waitlisted`; (2) `IsValidAttendance(DateTime?)` returned `true` unconditionally. Rebuilt and re-ran the class after each, then reverted with `git checkout --` and re-ran.
- Result: Baseline 7/7. Mutation 1 → 2 failures (`InMemoryTwinsAgreeWithTheirSqlExpressions`, `CertificateEligibilityRequiresSeatAttendanceReviewStateAndCompletedEvent`). Mutation 2 → the same 2 failures. After revert, 7/7 and a clean tree. The pinning test catches a one-sided change in either the seat rule or the attendance rule, and the eligibility matrix catches it independently.
- Evidence: `dotnet run --project tests/ASI.Basecode.Services.Tests/... -- -class ASI.Basecode.Services.Tests.ParticipationFactsTests -parallelMode none`; `git status --short` empty after revert.
- Open: A consistent change to both twins would pass, by design — the test pins the two forms to each other, not to the schema. `IsCertificateEligible` is covered by a hand-written case list, not a property. Block 4 not yet accepted.

## 11 — Block 4 (one rule) — second opinion

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Fresh subagent (no review context, no formal skill) audited completeness, call-site quality, design and behaviour of the `ParticipationFacts` unification, searching the whole solution for surviving inline encodings.
- Result: Verdict "mostly sound, with narrow real problems". Verified by the orchestrator: (a) `RegistrationRepository.cs:286` and `:303` still filter `!= Cancelled`, a third encoding of open registration that contradicts the interface doc at `IRegistrationRepository.cs:81-84`; (b) the twin-agreement test added in `7df7a30` compares `IsOpenRegistration` to a hard-coded literal and never compiles `OpenRegistration`, so the one rule with three encodings has no agreement test; (c) `MyAttendanceRow.IsValid` has no reader and `ParticipationReportDetailRow.IsAttendanceInvalidated` is read only by tests; (d) `AttendanceRecordingService.cs:70` keeps a redundant `current is null` before a predicate that already handles null; (e) `CheckInLookupService.cs:65-66` negates a rule named for its opposite, a readability regression introduced by the review patch; (f) `IsCertificateEligible` lives in `Data.Queries` with no expression twin and certificate-ledger vocabulary, and both call sites are in the ledger. Confirmed behaviour change: the completion query previously selected any registration with valid attendance and now selects Confirmed only, so a member cancelled after checking in no longer receives a review.
- Evidence: subagent report; `rg "Cancelled" RegistrationRepository.cs`; `git show 005f899:ASI.Basecode.Data/Repositories/CertificateRepository.cs`; `rg "IsAttendanceInvalidated|MyAttendanceRow"`.
- Open: None of the six is patched. The reviewer has not decided. Block 4 not yet accepted.

## 12 — Block 4 (one rule) — second-opinion patches

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Patched four of the six second-opinion findings and added the release note for the fifth. (1) `GetOpenForEventAsync` and `DiscoverOpenEventIdsForUserAsync` now use `ParticipationFacts.OpenRegistration` instead of `!= Cancelled`. (2) The twin test compiles `OpenRegistration` and compares per enum value. (3) Added `ParticipationFacts.IsInvalidated` as the named negation and used it in `CheckInLookupService`, `AttendanceInvalidationService` and `AttendanceRecordingService`. (4) Deleted `MyAttendanceRow.IsValid` and `ParticipationReportDetailRow.IsAttendanceInvalidated`, with the report tests asserting `InvalidatedAt` directly. Declined the sixth (the redundant `current is null` guard): `current` is read at `AttendanceRecordingService.cs:83`, so the guard is load-bearing for null-state analysis. Added a "Behaviour changes worth knowing" section to the PR body covering the cancelled-after-check-in review change, the per-request materialization cost and the year label.
- Result: Build 0 warnings / 0 errors with `-warnaserror`; services 316/316; integration 459/459. Mutation check: dropping `Waitlisted` from `OpenRegistration` alone now fails `InMemoryTwinsAgreeWithTheirSqlExpressions`, closing the hole that the earlier mutation test missed. Committed `019e4be` (9 files, +24/−26) and pushed; PR head is now `019e4be`.
- Evidence: `git commit 019e4be`; `gh pr edit 215`; `gh pr checks 215` (run 42504316406 pending); mutation run on `ParticipationFactsTests`.
- Open: CI result not yet observed. `IsCertificateEligible`'s placement (finding 3) left as a judgement call for the reviewer. Block 4 not yet accepted.

## 13 — Block 4 (one rule) — CI green

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Checked run 42504316406 on `019e4be` after the watch ended.
- Result: `build-and-test` passed in 7m27s. The second-opinion patches hold on the CI database. Only the same two runner deprecation annotations as before.
- Evidence: `gh run view 42504316406 --json conclusion` → `success`; `gh pr checks 215`.
- Open: Block 4 not yet accepted. `IsCertificateEligible`'s placement left to the reviewer. Blocks 5–9 not written.

## 14 — Block 5 (report snapshot) — test

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Baseline run of `ParticipationReportServiceTests` (19/19) and `ParticipationReportRepositoryTests` (2/2). Then two mutations: (A) dropped `ParticipationFacts.HoldsSeat` from `ParticipationReportService.IsValidAttendee`; (B) made `NoShowCount` `confirmedCount - attendees.Count + 1`. Rebuilt and re-ran both classes after each, then reverted.
- Result: Mutation A passed all 21 tests — the invariant "an attendee must hold a confirmed seat" is untested, because the fixture's pending, waitlisted and cancelled registrations carry no attendance row and so cannot become attendees even without the seat check. Mutation B failed 7 tests (6 service, 1 integration), so the derivation itself is well covered. Tree clean and build green after revert.
- Evidence: `dotnet run --project tests/ASI.Basecode.Services.Tests/... -- -class ...ParticipationReportServiceTests`; `dotnet run --project tests/ASI.Basecode.IntegrationTests/... -- -class ...ParticipationReportRepositoryTests`; `git status --short` empty.
- Open: Proposed adding one fixture row — a cancelled registration carrying a valid attendance row — asserting it is excluded from `ValidAttendeeCount` and that `NoShowCount` stays non-negative. Not applied; reviewer has not decided. Block 5 not yet accepted.

## 15 — Block 5 (report snapshot) — gap closed

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Gave the fixture's cancelled registration a real attendance row and asserted it is excluded from the attendee count. First attempt also changed `ToRow.Attended` to use `IsValidAttendee`, which failed the existing `DetailRowsMapStatusAndAttendanceLabelsForACompletedEvent` — that test deliberately asserts a checked-in non-confirmed row still shows `Attended = true`, so the display flag and the count answer different questions. Reverted the service change and left the display behaviour alone; the count is what the new fixture pins.
- Result: Build 0 warnings / 0 errors; services 316/316; integration 459/459. Mutation re-checked: removing `ParticipationFacts.HoldsSeat` from `IsValidAttendee` now fails `SnapshotReturnsEveryRegistrationWithAttendanceStateIncludingInactiveHistory`, where before it passed all 21 report tests. Committed `93a1875` (1 file, +12/−2) and pushed; PR head is `93a1875`.
- Evidence: `git commit 93a1875`; `ParticipationReportRepositoryTests` run before and after the mutation; `ParticipationReportServiceTests.DetailRowsMapStatusAndAttendanceLabelsForACompletedEvent` at `:309-313`.
- Open: CI result not yet observed. Block 5 not yet accepted.

## 16 — Block 5 (report snapshot) — CI green

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Checked run 42507294413 on `93a1875` after the watch ended.
- Result: `build-and-test` passed in 8m0s. The new report fixture holds on the CI database. Same two runner deprecation annotations.
- Evidence: `gh pr checks 215` → `pass 8m0s`.
- Open: Block 5 not yet accepted. Blocks 6–9 not written.

## 17 — Block 6 (certificate rule) — test

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Baseline run of the four certificate classes (32/32). Then three mutations, each reverted after: (A) dropped `eventItem.EventStatusId == Completed` from `IsCertificateEligible`; (B) made the decide path require `Eligible` instead of `PendingApproval`; (C) dropped `SeatHolders` from the completion query. Also grepped the tests for the three refusal message strings.
- Result: (A) 1 failure — the eligibility matrix. (B) 6 failures across the decision suite and the ledger flow. (C) 1 failure — exactly `CompletionCreatesNoReviewForACancelledOrWaitlistedRegistrationWithAttendance`. All three rules are pinned. Message coverage is partial: only "already been decided" is asserted (in `CertificateLedgerFlowTests:150` and `AccountRequestDecisionTests:368`); "attendance is no longer valid" and "details changed" are never asserted, so the message-selection patch from the review is untested.
- Evidence: `dotnet run --project tests/ASI.Basecode.IntegrationTests/... -- -class ...`; `rg "already been decided|attendance is no longer valid|details changed" tests`.
- Open: Proposed two assertions on the refusal message — one for an invalidated attendance, one for a lost seat. Not applied; reviewer has not decided. Block 6 not yet accepted.

## 18 — Block 6 (certificate rule) — message branches pinned

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Added the two missing message assertions. `InvalidatedAttendanceWithBlankDeclineReasonIsStaleNotInvalid` now asserts the attendance message; a new `LostSeatIsStaleWithTheChangedMessage` cancels the registration after the review exists (new `cancelRegistration` parameter on `SeedAsync`) and asserts the changed message. Mutation-checked both: replacing the changed branch with the attendance message fails the new fact, and replacing the attendance branch with the changed message fails the invalidated fact.
- Result: Build 0 warnings / 0 errors; services 316/316; integration 460/460 (one new fact). Committed `40c6f97` (1 file, +34/−1) and pushed; PR head is `40c6f97`.
- Evidence: `git commit 40c6f97`; `CertificateReviewDecisionTests` 14/14; two mutation runs.
- Open: CI result not yet observed. Block 6 not yet accepted.

## 19 — Block 6 (certificate rule) — CI green

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Checked run 42509387895 on `40c6f97` after the watch ended.
- Result: `build-and-test` passed in 8m59s. The two message assertions hold on the CI database.
- Evidence: `gh pr checks 215` → `pass 8m59s`.
- Open: Block 6 not yet accepted. Blocks 7–9 not written.

## 20 — Block 7 (3.14 refactors) — test

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Baseline run of the affected classes (`MyEventsRegistrationQueryTests`, `MyEventsAttendanceQueryTests`, `EventCancellationFlowTests`, `EventPublicationFlowTests`, plus `MemberEventServiceTests`, `EventLifecycleServiceTests`, `RegistrationReviewTests` — 155 service tests). Then three mutations, each reverted after: (A) `ReadPageAsync` offset `+ 1`; (B) inverted the `CutoffPassed` mapping item; (C) removed the `OpenRegistration` filter from `GetOpenForEventAsync`.
- Result: (A) 2 failures — exactly the two MyEvents paging tests. (B) 1 failure — `RegistrationsAreOwnerScopedPagedAndLabeledAcrossStatuses`. (C) 0 failures across all 460 integration tests: the caller re-checks `IsOpenRegistration` before writing, so the SQL filter is an optimisation rather than a correctness requirement. That matches the plan's "Reviewed and left alone" note, and means the method's open-registration contract is enforced one layer up.
- Evidence: `dotnet test tests/ASI.Basecode.IntegrationTests/...` after each mutation; `git status --short` empty after revert; build 0/0.
- Open: Block 7 not yet accepted.

## 21 — Block 8 (5.10 refactors) — test

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Baseline run of `AccessDialogServiceTests` and `AccountRoleServiceTests` (32/32). Then two mutations, each reverted after: (A) made the `OccurredAt` value converter identity on read; (B) removed `StringComparison.OrdinalIgnoreCase` from `MembershipAccessViewModel.For`.
- Result: (A) 1 failure — `TransactionVerificationTests.NestedWorkIsInvisibleUntilOwnerCommitsAndRollsBackWithOwner`, which asserts the read-back kind. (B) 0 failures: `MembershipStatusIsCaseInsensitive` still passes because it exercises `AccessDialogService.cs:74`, not the factory. Verified by grep that `MembershipAccessViewModel.For` has no caller anywhere — the 5.10 `OrdinalIgnoreCase` fix landed in dead code, while the live comparison at `AccessDialogService.cs:74` already used `OrdinalIgnoreCase` before this PR and was only made async.
- Evidence: `git diff 005f899 149791b -- AccessDialogService.cs AccessDialogViewModels.cs`; `rg "MembershipAccessViewModel"`; `TransactionVerificationTests` run after mutation A.
- Open: Proposed deleting the uncalled `MembershipAccessViewModel.For`, so the block's claim about the comparison is either removed or the factory is wired up. Not applied; reviewer has not decided. Block 8 not yet accepted.

## 22 — Block 8 (5.10 refactors) — the factory is live, and now tested

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Tried to delete `MembershipAccessViewModel.For` as dead code; the build failed at `Views/Member/Index.cshtml:132`, which calls it. The earlier grep covered `*.cs` only. Reverted the deletion and added a five-case theory instead, covering `active`, `ACTIVE`, `Active`, `inactive` and `Inactive` through the factory.
- Result: Correction to entry 21 — `For` is live, the 5.10 fix is live, and the mutation went unnoticed only because no test exercised the factory. The new theory catches the revert in the two mixed-case cases. Services 321/321 (16 in `AccessDialogServiceTests`), integration 460/460. Committed `14f699b` (1 file, +22) and pushed; PR head is `14f699b`.
- Evidence: build error `CS0117` at `Views/Member/Index.cshtml:132`; `MembershipAccessRowModelTreatsTheStatusCaseInsensitively` failing on `ACTIVE` and `Active` after the revert.
- Open: The same status comparison is still written twice — `AccessDialogService.cs:74` for the deactivation preview and `AccessDialogViewModels.cs:77` for the list's action model. Left alone as outside this block's scope. CI result not yet observed. Block 8 not yet accepted.

## 23 — Block 8 (5.10 refactors) — CI green

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Checked run 42542493647 on `14f699b`.
- Result: `build-and-test` passed in 7m41s. The five-case factory theory holds on the CI database.
- Evidence: `gh pr checks 215` → `pass 7m41s`.
- Open: Block 8 not yet accepted. Block 9 (periphery) not written.

## 24 — Block 9 (periphery) — stale register sentences fixed

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Fixed the two sentences in `demo-mock-register.md` that described the seam as live outside the section the PR marked historical: `:286` now says the token switch and `DemoStore.GetPassToken` were removed by 8.13 (149791b), and `:418` now says the seed service used to seed eight check-ins and that the scenario seed writes registrations only, with the presenter recording check-ins through the real scanner. Checked the rest of the file's seam mentions: they sit in sections that narrate a removal or list deleted files, so they read correctly as provenance and were left alone.
- Result: Docs only, no build or test impact. Committed `d5f85f9` (1 file, +7/−6) and pushed; PR head is `d5f85f9`.
- Evidence: `rg "remain until|still seeds|DemoStore\.|DemoSeedService" demo-mock-register.md`; the remaining hits are historical.
- Open: CI result not yet observed. Block 9 not yet accepted.

## 25 — Block 9 (periphery) — CI green

Session: ses_e4465698effew7cc0TwbRjURxU · Timestamp: 2026-11-09T00:00:00+08:00

- Action: Checked run 42543676550 on `d5f85f9`.
- Result: `build-and-test` passed in 7m2s.
- Evidence: `gh pr checks 215` → `pass 7m2s`.
- Open: Block 9 not yet accepted. Wrap-up not started. Walkthrough artifacts still untracked in the main repo.
