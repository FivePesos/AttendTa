# Handoff — member edit follow-up work

Status as of 2026-11-17. Branch: `fix/member-edit-unchanged`.
Base: `upstream/dev` at `989b62aa9ed529fcb0ea4d2fed01ae6c411429de`.

## Done and shipped in this branch

Commit `ac8118749672a43e6550ab7d77a190d1503ac76c` — "fix(members): avoid updates when
details are unchanged".

- Saving an unchanged member no longer updates user/member modification metadata and no
  longer appends a false `MemberEdited` audit row.
- The service compares normalized submitted values against freshly loaded persisted data
  under the existing linked-user lock, and returns `false` for a no-op.
- Both the dialog and the standalone editor show a neutral "No changes were made to the
  member." notice instead of a success message. The notice stays visible under the sticky
  header on mobile and is associated with the restored Edit control.
- HTTP submissions normalize before MVC validation, so padded/aliased values are accepted
  without weakening validation, and binding errors (for example an invalid year level) are
  still reported. Normalized course, section and membership type survive a validation
  re-render.
- A stored student number that is semantically unchanged is preserved byte-for-byte during
  an unrelated edit.
- Member reads for this edit path refresh only through `GetMemberForEditAsync`; that path
  refuses to discard unsaved tracked changes rather than silently overwriting them.
- Regression coverage: `MemberManagementTests` (24 tests), service suite 405, JavaScript
  suite 51, Release build clean, CSS build run.

Not covered by that commit: screen-reader announcement of the notice was not verified with
assistive technology, and the full SQL integration suite was not rerun on the branch.

## Remaining work

### 1. Stale member-form protection (next)

Approved plan, not implemented: `_bmad-output/plan-member-stale-form-protection.md`
(status `ready-for-dev`).

An operator can still submit an older member form after another operator saved newer
information, silently overwriting it. `UpdateMemberAsync` compares against current database
values for the no-op decision but never checks what the form originally showed.

An implementation attempt was aborted by a tooling limit; its partial edits were reverted.
Nothing from that attempt is in the branch.

Key constraints already investigated (details in the plan's Code Map):

- Editable data spans two rows: `users.name` and member student number, course, year level,
  section, mobile number, membership type.
- `UserRepository.LockUserAsync` (`UPDLOCK, HOLDLOCK`) is the existing serialization point;
  keep that lock order.
- There is no rowversion/concurrency token. Nullable `datetime2(3)` timestamps are not
  version identifiers.
- `OutcomeCode.Stale` already exists.
- The snapshot detects different current values, not a change later reverted to the same
  values. It also cannot stop writers that bypass the lock protocol (for example direct SQL).

Recommended approach from the investigation: a purpose-separated, tamper-resistant snapshot
of editable member and linked-user values, carried from GET through both editor forms, and
verified inside the existing edit transaction before the no-op check, mutation or audit.
Missing, malformed, tampered or wrong-record evidence must fail closed without writes, and
validation re-renders must keep the original version rather than silently renewing it.

### 2. Legacy member data (deferred)

Logged in `_bmad-output/deferred-work.md`.

- Unchanged legacy values that fail current validation can block saving entirely. Examples
  seen in fixtures: mobile `555-0100`, `Original course`.
- A stored student number with surrounding whitespace is preserved as-is on unrelated edits.
  Because uniqueness checks use the stored bytes, a value that differs only by whitespace can
  be treated as distinct.
- Fixing this needs a product/data decision (correct the rows through the editor, reject and
  resubmit, or widen acceptance) plus a database-level uniqueness decision. Do not change it
  as a side effect of the concurrency work.

## Verification commands used

```powershell
dotnet build ASI.Basecode.sln --configuration Release
dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --no-build --no-restore --configuration Release
node --test ASI.Basecode.WebApp/tests/*.test.js
npm --prefix ASI.Basecode.WebApp run css:build
$env:ATTENDTA_TEST_SQL='Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True'
dotnet tests/ASI.Basecode.IntegrationTests/bin/Release/net10.0/ASI.Basecode.IntegrationTests.dll -class ASI.Basecode.IntegrationTests.MemberManagementTests -parallelMode none
```
