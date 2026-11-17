---
title: 'Fix venue integration test stub'
type: 'bugfix'
ticket: ''
created: '2026-11-17'
status: 'built'
baseline_revision: 'dec766fadb5a5e74941ab06fb9acbaf97085d504'
route: 'oneshot'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
followup_review_recommended: false
context: []
warnings: []
deferred: []
---

<intent-contract>

## Intent

**Problem:** The venue integration test stub lacks two interface members, preventing the Release solution build.

**Approach:** Implement the missing members as unsupported operations, preserving the existing failing list behavior and assertions. Build and run venue and full SQL integration tests when local SQL Server is available; report results and coordinate with issue #242.

</intent-contract>

## Implementation Notes

Oneshot: two localized test-double methods, without production changes.

Added the missing `IVenueService` methods to `VenuePagedListTests.FailingVenueService`; both throw `NotSupportedException` to expose unexpected use.

## Plan Change Log

## Review Triage Log

### 2026-11-17 — Review pass
- verdicts: 1 finding — high 0, medium 1, low 0, false 0, maybe-false 0
- findings:
  - `medium` `patch` Issue #242 coordination was incomplete — posted the verification results and requested a single-commit rerun at https://github.com/rainyDayssss/AttendTa/issues/242#issuecomment-6924933751.

## Verification

- `dotnet build ASI.Basecode.sln --configuration Release` succeeds.
- Run `VenuePagedListTests` and the full integration test assembly against an available local SQL Server master connection; report unavailable dependencies or failures.

## Auto Run Result

Status: blocked

Blocking condition: implementation verification failed. The Release solution build succeeded with zero warnings and errors; `VenuePagedListTests` passed 3/3. The full LocalDB integration run completed 609 tests with 608 passed and one failed: `CompactListFlowTests.RealListsFilterBeforePagingAndRenderCompactDetailsWithoutExposingHashes` timed out after 30 seconds waiting for navigation to `/Member?pageSize=5&page=1&fit=3m` at `tests/ASI.Basecode.IntegrationTests/CompactListFlowTests.cs:247`. This failure is outside the changed venue stub; its cause is unconfirmed. The earlier full-suite run was interrupted before reporting a result. No commit, push, PR, or issue #242 coordination was performed.

Continuation: the failed test passed in isolation (1/1), and a complete LocalDB integration rerun passed 609/609. The earlier timeout's cause remains unconfirmed.

Review continuation blocked: the mandatory quick-review subagent could not run: `Vibe budget exhausted`. No review result was returned. No commit, push, PR, or issue #242 coordination was performed.

Final continuation: quick review completed using OpenAI. The one finding, outstanding #242 coordination, was addressed by the linked comment. No code patches or deferred findings; no follow-up review recommended. Release build passed with zero errors or warnings; venue integration tests passed 3/3 and full SQL integration rerun passed 609/609. The earlier isolated Playwright timeout has an unconfirmed cause. Changed files: this plan and `tests/ASI.Basecode.IntegrationTests/VenuePagedListTests.cs` (two fail-fast stub methods). Follow-up #242 verification must run on the post-merge commit.
