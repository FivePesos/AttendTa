---
title: 'Rename user email property'
type: 'refactor'
ticket: ''
created: '2026-11-16'
status: 'built'
baseline_revision: 'e17de42d5dd9ee77a1c7c2b62ec9b579e61eeb98'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

Rename the string `User.UserId` property to `User.Email` and update its references, so the model names the value it actually contains.

## Boundaries & Constraints

**Always:** Preserve mapping to `users.email`, email lookup and sign-in behavior, and member lookup behavior.

**Never:** Rename integer identifiers such as `User.Id`, `Member.UserId`, actor identifiers, authenticated-user identifiers, or `userId` parameters; change database schema or authentication semantics.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Sign-in | Active user and valid email/password | Authenticated result has original email and integer ID | Existing invalid/unknown/inactive behavior remains unchanged |
| Member lookup | Member linked to a user by integer ID | Joined record contains the user's email and unchanged integer IDs | Missing member remains absent |
| Mapping | EF model for `User.Email` | Column remains `users.email` with existing uniqueness and length | No schema change |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Models/User.cs`, `ASI.Basecode.Data/CanonicalSchemaMappings.cs` — string property and EF column/index mapping; retain `Id` and database column.
- `ASI.Basecode.Data/Repositories/UserRepository.cs`, `MemberRepository.cs`, `AccountAdministrationRepository.cs` — email lookups and projections; member joins use integer IDs.
- `ASI.Basecode.Services/Services/AccountService.cs`, `AccountRequestService.cs` — sign-in mapping and approved account creation/duplicate checks.
- `tests/ASI.Basecode.IntegrationTests/MemberRepositoryTests.cs`, `AccountServiceTests.cs`, `AuthenticationFlowTests.cs` — member join, authentication results, and cookie sign-in regression coverage.
- Other `tests/ASI.Basecode.{IntegrationTests,Services.Tests}/*.cs` — user fixtures, email comparisons and sign-in callers. Distinguish `User` from `Member`, actor and DTO `UserId` references by type.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Data/Models/User.cs`, `ASI.Basecode.Data/CanonicalSchemaMappings.cs` — rename the property and EF selectors while retaining mapping and index behavior.
- [x] `ASI.Basecode.Data/Repositories/`, `ASI.Basecode.Services/Services/` — update only references bound to the renamed string property.
- [x] `tests/ASI.Basecode.Services.Tests/`, `tests/ASI.Basecode.IntegrationTests/` — update user fixtures and references; strengthen member lookup and mapping assertions if needed; retain integer identifier assertions.
- [x] Run the build and targeted/full available tests; report any unavailable SQL-dependent coverage rather than claiming it passed.

**Acceptance Criteria:**
- Given an active account, when it signs in with its email and password, then the original email and integer identity are returned and existing failure cases remain unchanged.
- Given a linked member, when its record is retrieved by integer user ID, then the original email and member/user IDs are returned; a missing ID yields no record.
- Given the EF model, when `User.Email` is mapped, then it continues to target the unique `users.email` column without a schema migration.

## Implementation Notes

- Renamed the entity property and its typed references. Added a SQL-independent EF model test and strengthened the member lookup assertion.
- Release build passed; 383 service tests and 567 SQL Server integration tests passed, with no skipped tests. Subagents were unavailable (budget exhausted).

## Plan Change Log

## Review Triage Log

- Quick independent review: no findings. The first review attempts were unavailable; the review completed after changing the project subagent model.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — solution builds.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release` — service tests pass; use the documented direct host fallback if zero tests run.
- `dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -parallelMode none` with a disposable local SQL Server master connection — integration suite passes, including sign-in and member lookup; if unavailable, explicitly report the limitation.
