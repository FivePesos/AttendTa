---
title: 'Protect member edits from stale forms'
type: 'bugfix'
ticket: ''
created: '2026-11-17'
status: 'ready-for-dev'
baseline_revision: 'ac8118749672a43e6550ab7d77a190d1503ac76c'
route: 'full'
route_source: 'auto'
risk: 'high'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** An operator can submit an older member form after another operator saves newer information, unintentionally overwriting those changes.

**Approach:** Detect stale member edits and safely refuse them with a clear reload path on both the dialog and standalone editor. Preserve entered values until the operator explicitly reloads; do not change current validation, audit, or membership rules.

## Boundaries & Constraints

**Always:** Cover editable member fields and linked user name; compare current values while holding the existing linked-user serialization lock before any no-op decision, write or audit. Preserve authorization, antiforgery, input, reason, and validation re-renders. Reject missing, malformed, tampered or wrong-record version evidence without writing. Document cooperating-writer limitations.

**Never:** Silently overwrite on conflict, automatically replace a stale token while preserving stale input, migrate existing member data, use timestamp as a unique version, or weaken business validation. Legacy invalid-data handling and normalized uniqueness are separate follow-up work.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Current form | Version matches current editable values | Existing changed or no-op path operates normally | Existing validation remains |
| Concurrent edit | Another operator changed an editable member field or linked name after form load | Reject stale submit, preserve entered input, offer explicit reload; no mutation or audit | Explain why and how to recover |
| Token failure | Missing, malformed, tampered, wrong-member or no longer decryptable evidence | Reject safely; no mutation or audit | Ask operator to reload |
| Validation retry | Validation fails, then current or stale form is resubmitted | Keep original version; validate and detect conflict | Never silently renew token on failed validation |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/Services/MemberService.cs` — edit locks linked user and refreshes member before normalized no-op and audit; make conflict check atomic with this path and retain no-op behavior.
- `ASI.Basecode.Data/Repositories/UserRepository.cs`, `MemberRepository.cs`, `UnitOfWork.cs` — user row lock serializes cooperating writers; follow lock order. Member and user editable fields span two rows. Avoid relying on nullable datetime timestamps.
- `ASI.Basecode.Services/ServiceModels/MemberViewModel.cs`, `Interfaces/IMemberService.cs` — carry record-bound original-state evidence from GET to POST; avoid exposing sensitive data.
- `ASI.Basecode.WebApp/Controllers/MemberController.cs`, `Views/Member/_Form.cshtml`, `_Editor.cshtml`, `Edit.cshtml` — GET, POST, model validation and both editors. Use purpose-separated Data Protection or equivalent tamper-resistant snapshot tied to member and linked-user IDs; keep token intact on errors and provide explicit reload.
- `ASI.Basecode.WebApp/wwwroot/js/features/record-dialog.js` — preserve existing HTML error, focus and discard handling when a stale dialog is re-rendered.
- `tests/ASI.Basecode.IntegrationTests/MemberManagementTests.cs` — existing no-op, locking and audit tests. Add independent forms, identity binding, rollback, validation and UI checks.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.Services/Services/MemberService.cs` and relevant interfaces/models — compare original editable state against fresh member and linked user under the lock; reject stale or invalid state without writes and preserve no-op semantics. Specify canonical normalization consistently with existing edit behavior.
- [ ] `ASI.Basecode.WebApp/Controllers/MemberController.cs`, `Views/Member/` and token service/registration — create and verify protected record-bound version evidence, carry it through both editors and validation failures, and give a clear reload path that does not silently discard input.
- [ ] `tests/ASI.Basecode.IntegrationTests/MemberManagementTests.cs` and applicable tests — verify each matrix case, concurrent cooperating writers, per-row changes, tampering, no-op, audit/metadata absence and both UI paths.
- [ ] Run required scoped UI critique and polish; document limitations and any deferred unrelated findings.

**Acceptance Criteria:**
- Given two forms loaded for one member, when the first saves a valid edit and the second submits, then the second sees a conflict and the first edit remains intact without a second audit entry.
- Given a form whose member or linked-user editable value changes before submission, when submitted through either editor, then conflict recovery preserves entered data until explicit reload.
- Given invalid, missing or wrong-record evidence, when submitted, then the request cannot alter data or produce an audit entry and the operator is offered reload.
- Given valid unchanged data and current evidence, when submitted, then existing no-change feedback and no-write behavior remain.
- Given validation failure, when the form is corrected or becomes stale before resubmission, then the original evidence remains and stale changes cannot be silently applied.

## Implementation Notes

Not started. An implementation attempt was aborted before producing usable code, and its partial edits were discarded. Nothing on this plan is built or verified; start from the task list above.

The existing user lock prevents races among writers that follow that protocol; it does not protect against arbitrary direct SQL or other writers that bypass it. A snapshot detects different current values, not a change subsequently reverted to the same values. Confirm GET reads a coherent member/user snapshot and do not claim broader database-enforced guarantees.

## Plan Change Log

## Review Triage Log

## Verification

- `dotnet build ASI.Basecode.sln --configuration Release`
- Run focused member integration and service tests after the build, including independent competing form and lock tests; use disposable local SQL only.
- Test dialog and standalone conflict, preserved input, reload, keyboard and mobile behavior. Run JavaScript/CSS verification as applicable.
