- source_plan: `_bmad-output/initiative-attendta/tickets/epic-platform-access/tickets/story-canonical-database-schema-update-and-ef-core-mapping-for-all-plan.md`
  summary: Update `AGENTS.md` Test Conventions to document `ATTENDTA_TEST_SQL` and the per-class `AttendTa_IT_<guid>` fixture, replacing the retired `ConnectionStrings__DefaultConnection` / `AttendTaMVP_Test` smoke-test instructions.
  evidence: `AGENTS.md` still says the integration tests read `ConnectionStrings__DefaultConnection` and that the SQL smoke test creates `AttendTaMVP_Test`. `SqlServerSmokeTests.cs` is deleted, and `SqlServerDatabaseFixture.InitializeAsync` now throws when `ATTENDTA_TEST_SQL` is absent and creates only `AttendTa_IT_<guid>`, so following the documented command fails. Deferred because the fix edits an agent-context file.
- source_plan: `_bmad-output/plan-modal-copy-clarity.md`
  summary: Restore the full solution build after the user ID rename.
  evidence: Release build fails in `AccountLoginMatrixTests.cs:104` because `User.UserId` no longer exists; the separate backend PR already includes a correction.
- source_plan: none
  summary: Address legacy member values that fail current validation and student numbers that differ only by whitespace.
  evidence: Split from stale-form protection at the user's request; legacy data policy and database uniqueness require separate decisions.
- source_plan: none
  summary: Add upcoming/past event filtering to Registration Review while preserving access to history.
  evidence: Split from cancellation-action availability at the user's request; implement after the first fix.
