---
title: 'Protect member edits from stale forms'
type: 'bugfix'
ticket: ''
created: '2026-11-17'
status: 'built'
baseline_revision: 'ac8118749672a43e6550ab7d77a190d1503ac76c'
route: 'full'
route_source: 'auto'
risk: 'high'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
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
- [x] `ASI.Basecode.Services/Services/MemberService.cs` and relevant interfaces/models — compare original editable state against fresh member and linked user under the lock; reject stale or invalid state without writes and preserve no-op semantics. Specify canonical normalization consistently with existing edit behavior.
- [x] `ASI.Basecode.WebApp/Controllers/MemberController.cs`, `Views/Member/` and token service/registration — create and verify protected record-bound version evidence, carry it through both editors and validation failures, and give a clear reload path that does not silently discard input.
- [x] `tests/ASI.Basecode.IntegrationTests/MemberManagementTests.cs` and applicable tests — verify each matrix case, concurrent cooperating writers, per-row changes, tampering, no-op, audit/metadata absence and both UI paths.
- [x] Run required scoped UI critique and polish; document limitations and any deferred unrelated findings.

**Acceptance Criteria:**
- Given two forms loaded for one member, when the first saves a valid edit and the second submits, then the second sees a conflict and the first edit remains intact without a second audit entry.
- Given a form whose member or linked-user editable value changes before submission, when submitted through either editor, then conflict recovery preserves entered data until explicit reload.
- Given invalid, missing or wrong-record evidence, when submitted, then the request cannot alter data or produce an audit entry and the operator is offered reload.
- Given valid unchanged data and current evidence, when submitted, then existing no-change feedback and no-write behavior remain.
- Given validation failure, when the form is corrected or becomes stale before resubmission, then the original evidence remains and stale changes cannot be silently applied.

## Implementation Notes

Not started. An implementation attempt was aborted before producing usable code, and its partial edits were discarded. Nothing on this plan is built or verified; start from the task list above.

2026-11-17 follow-up: added `MemberEditSnapshot`, a purpose-separated
`MemberEditSnapshotProtector`, and two focused normalization/identity tests. Neither component
is connected to the form or update transaction; stale edits remain possible. The protector has
not yet been tested. Do not treat this as a completed safeguard. The focused tests passed via
`tests/ASI.Basecode.Services.Tests/bin/Release/net10.0/ASI.Basecode.Services.Tests.exe -class ASI.Basecode.Services.Tests.MemberEditSnapshotTests`.

2026-11-17 implementation in progress: wired protected snapshots into both member editors and
the locked update path. The Release solution builds and the two snapshot normalization tests
pass. Existing member integration tests still need to supply original snapshots; new end-to-end,
tamper, concurrency, audit, and browser recovery tests remain outstanding. The UI detector found
no source findings, but scoped visual critique and polish were not completed. No SQL test
connection was configured. Do not treat this work as verified or complete.

2026-11-17 test continuation: updated the existing service and HTTP member tests for version
evidence and added independent-form coverage for every editable field, invalid and wrong-record
tokens, retry behavior, user identity, metadata/audit absence, and a writer blocked on the
linked-user lock. The Release build passed; the focused LocalDB member integration class passed
42/42, including Chromium dialog conflict checks at 390px and 1440px; snapshot tests passed 2/2.
The full integration suite and scoped design critique/polish were not run in this continuation.
The browser test checks the reload link and reopening, but does not follow the new-tab link.

Reload-link follow-up: Chromium now follows the actual new-tab link from both dialog and
standalone conflict forms at 390px and 1440px. It verifies the fresh values and version, retained
input and original version in the old form, and a successful fresh save. Focused new cases passed
4/4 and the member integration class passed 46/46 on disposable LocalDB. The test observes the
save's 302 response and database state; its in-memory browser proxy cannot follow that redirect.

Scoped critique follow-up: recovery instructions appear only for stale or unverifiable evidence,
including when other fields are invalid. They explain transferring edits and saving the fresh
form; the dialog announces conflict recovery instead of field-error guidance. Focused LocalDB
member integration tests passed 47/47 (including 390px and 1440px Chromium interactions), snapshot
tests 2/2, and dialog guard tests 10/10. The Member markup detector returned no findings. No
manual visual, screen-reader, or cross-browser polish inspection was performed in this pass.

Verification-gap follow-up: added dialog and standalone tests where the form is already stale
before an invalid submission, retains its version and input, then rejects the corrected retry.
The waiting-writer test now signals the intercepted lock command before the winner changes and
commits the user row, rather than using a delay as evidence of the attempt. Release build passed
and the disposable LocalDB member integration class passed 49/49.

The existing user lock prevents races among writers that follow that protocol; it does not protect against arbitrary direct SQL or other writers that bypass it. A snapshot detects different current values, not a change subsequently reverted to the same values. Confirm GET reads a coherent member/user snapshot and do not claim broader database-enforced guarantees.

## Plan Change Log

## Review Triage Log

- **medium — patch:** A valid token for a different member reaches the validation-only
  re-render without conflict recovery when other fields are invalid; compare its member ID
  with the submitted ID before that return.
- **medium — verification gap:** The scoped UI critique and polish passes ran, but manual
  visual and assistive-technology inspection was unavailable; retain this limitation in the
  handoff rather than claiming those checks passed.

## Verification

- `dotnet build ASI.Basecode.sln --configuration Release`
- Run focused member integration and service tests after the build, including independent competing form and lock tests; use disposable local SQL only.
- Test dialog and standalone conflict, preserved input, reload, keyboard and mobile behavior. Run JavaScript/CSS verification as applicable.

Follow-up verification: standalone conflicts now focus the summary; dialog and standalone
summaries describe their recovery instructions, and the dialog has visible focus and spacing.
Chromium exercised both editors at 390px and 1440px, including keyboard recovery and saving in
the fresh editor. The unchanged-save viewport test now waits for reload and delayed scrolling;
the timing hypothesis is supported by a delayed-load regression but not proven for the original
failure. Service tests passed 407/407, JavaScript tests 51/51, and the full LocalDB integration
suite passed 645/645 after a prior run failed the intermittent viewport assertion. The user
visually confirmed the conflict behavior. Actual screen-reader announcement and other browser
engines remain unverified.
