---
title: 'Attendance invalidation with confirmation and reason'
type: 'feature'
ticket: '6'
created: '2026-11-07'
status: 'in-review'
baseline_revision: '26815ee27223dd18d7f020c40bdb8abf77bff3fe'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 1
context:
  - '_bmad-output/initiative-attendta/demo-mock-register.md'
  - '_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Invalidation still runs in the M-07 demo provider: the reason never reaches SQL, the row is never stamped, no `attendance.invalidated` audit entry is written, the certificate-review hook does not exist, and the participation report still reads the demo store, so a correction is invisible to reports and the Activity Log.

**Approach:** Implement the real `IAttendanceInvalidationService` as one transaction that locks the owner's `users` row, then the event, then the attendance row, validates the reason, calls every registered `IAttendanceInvalidationHandler` before the write, stamps `invalidated_at`/`invalidated_by`/`invalidation_reason` on the retained row and appends one `attendance.invalidated` entry with the reason; delete the M-07 providers and point the M-13c report overlay at real attendance.

## Boundaries & Constraints

**Always:** Management only, actor from `ICurrentActor`. One `ExecuteInTransactionAsync` with lock order owner's `users` → event → attendance (AD-4). The reason is required, trimmed and validated inside the transaction. Handlers run inside the transaction before the write, so a `Forbidden` throw rolls the whole invalidation back. The attendance row is retained; the registration status never changes. Exactly one `attendance.invalidated` entry carries the reason; no token or QR value appears in any payload. UTC time from `TimeProvider`. Controller, view and the 4.11 dialog stay unchanged.

**Never:** No second dialog or UI change. No certificate handler implementation (6.5 owns it) and no issuance serialization (6.7). No registration-status change, no schema change, no EF migration. No per-ticket Impeccable critique or polish (8.12).

**Decisions (2026-11-07):** the M-13c report overlay hand-over is in this ticket's scope. Invalidating an already-invalidated row is refused and writes nothing, so the first reason and its audit entry survive. The reason is capped at 500 characters server-side, matching the dialog.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Valid invalidation | Officer, valid attendance, reason | Row retained with `invalidated_at`, `invalidated_by` and the reason; one `attendance.invalidated` entry with that reason | No error expected |
| Re-check-in after invalidation | Same registration scanned or manually checked in | Refused by 4.3's existing rule; no new row | Refusal message |
| Handler refuses | Registered handler throws `Forbidden` | Row stays valid; no audit entry; whole transaction rolls back | `DomainException(Forbidden)` |
| No handler registered | Empty handler list | Invalidation commits | No error expected |
| Blank reason | Empty or whitespace | Nothing written | `DomainException(Invalid)` on `reason` |
| Reason too long | More than 500 characters after trimming | Nothing written | `DomainException(Invalid)` on `reason` |
| Repeat invalidation | Row already carries `invalidated_at` | Nothing written; first reason and audit entry unchanged | `DomainException(Invalid)` |
| Unknown or wrong event | No attendance row, or a registration of another event | Nothing written | `DomainException(NotFound)` |
| Non-management | Member role | Forbidden before any write | `DomainException(Forbidden)` |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Resources/Constants/AuditActions.cs` — add `AttendanceInvalidated = "attendance.invalidated"` after `AttendanceRecorded`.
- `ASI.Basecode.Resources/Messages/Audit.resx` — add the `AttendanceInvalidated` template ("Attendance invalidated.").
- `ASI.Basecode.Resources/Messages/AuditDescriptions.cs` — map the new action; an unmapped action throws.
- `ASI.Basecode.Services/Services/ActivityLogWriter.cs` — line 34 guards `AttendanceRecorded` to entity type `attendance`; add the new action to that same guard.
- `ASI.Basecode.Services/Interfaces/IAttendanceInvalidationHandler.cs` — new hook, mirroring `IEventCompletionHandler` (empty list is valid; a throw rolls back).
- `ASI.Basecode.Services/ServiceModels/AttendanceInvalidationContext.cs` — new record `(EventId, RegistrationId, AttendanceId, Reason, InvalidatedBy, InvalidatedAt)`, mirroring `EventCompletionContext`.
- `ASI.Basecode.Services/Interfaces/IAttendanceInvalidationService.cs` — keep `InvalidateAsync(eventId, registrationId, reason)`; replace the stale M-07 doc comment. `CheckInController.Invalidate` already calls it.
- `ASI.Basecode.Services/Services/AttendanceInvalidationService.cs` — new. Copy `AttendanceRecordingService.RequireManagement`; discover with `IRegistrationRepository.DiscoverReviewAsync`; then the transaction.
- `ASI.Basecode.Data/Interfaces/IAttendanceRepository.cs` + `Repositories/AttendanceRepository.cs` — add `GetForEventAsync(eventId)` returning `EventAttendanceRow(RegistrationId, RecordedAt, InvalidatedAt, InvalidationReason)` for the report overlay. Reuse `GetByRegistrationIdAsync` + `LockAttendanceAsync` for the write path.
- `ASI.Basecode.WebApp/Startup.DI.cs` — register `IAttendanceInvalidationService → AttendanceInvalidationService` beside `ICheckInService` (line 56). `AddDemoData` runs later, so its lines must go.
- `ASI.Basecode.WebApp/DemoData/DemoDataRegistration.cs` — delete only the switch-off `UnavailableAttendanceInvalidationService` line and the `DemoCheckInRegistration` call; keep the `if (!enabled) return;` guard and the certificate/report registrations, or the store and seed stop registering with the switch off. Fix the stale comments there, in `DemoSeedService.cs:16-17` and in `DemoStore.cs:140-144`.
- Delete `ASI.Basecode.WebApp/DemoData/DemoAttendanceInvalidationService.cs`, `UnavailableAttendanceInvalidationService.cs`, `DemoCheckInRegistration.cs`.
- `ASI.Basecode.WebApp/DemoData/DemoParticipationReportService.cs` — M-13c hand-over: build the attendance map from `DemoStore.TryGetAttendance` as today, then overlay `IAttendanceRepository.GetForEventAsync` so a real row wins over a store row for the same registration. Keep the existing count/rate/no-show/breakdown derivation from that merged map, so the seeded demo event stays populated while a real invalidation shows. Derive the rate and no-show figures from the same `confirmed` list the map is built from, not from `report.ConfirmedCount`. Keep `DemoStore` for `IsCompleted`.
- `ASI.Basecode.WebApp/DemoData/DemoCertificateLedgerService.cs` — same hand-over for the still-open M-10b/M-11a rows: inject `IAttendanceRepository` and let a real attendance row win over a store row for the validity gate in `RequireCurrentParticipationAsync` (`:148`) and for `AttendedAt` in `GetPageAsync` (`:38`). Without it a real invalidation leaves the store row valid and an Officer can approve or issue against invalid attendance.
- `ASI.Basecode.WebApp/DemoData/DemoSeedService.cs` — drop M-07 from the startup "Demo data active" log line; the register marks it removed.
- `ASI.Basecode.WebApp/DemoData/DemoStore.cs` — delete the now-unreachable `GetAttendanceForEvent`; the report overlay reads `IAttendanceRepository.GetForEventAsync` and nothing else calls it.
- `_bmad-output/initiative-attendta/demo-mock-register.md` — set M-07 to `removed` with the commit; update the M-05/M-06/M-07 removal paragraph and the M-13c hand-over and seeded-state paragraphs (lines ~99-100, 385-387, 396-404, 421-425) to say real attendance now overrides the store.
- `tests/ASI.Basecode.IntegrationTests/AttendanceInvalidationFlowTests.cs` — new focused SQL smoke; reuse `CheckInFlowTests` seeding and service wiring.
- `tests/ASI.Basecode.IntegrationTests/DemoParticipationReportOverlayTests.cs` — add a case where a real invalidated row overrides a seeded store row; the existing count assertions stay valid.
- `tests/ASI.Basecode.IntegrationTests/DemoCheckInWindowFlagTests.cs` — assert `IAttendanceInvalidationService` resolves to `AttendanceInvalidationService` in both switch-state tests.

## Tasks & Acceptance

**Execution:**
- [x] Audit action, resx, description and writer guard — land first so `Append` resolves.
- [x] `IAttendanceInvalidationHandler` + `AttendanceInvalidationContext` — declare the hook with no implementation.
- [x] `AttendanceInvalidationService` — management check, discovery, transaction, locks, reason, handlers, stamp, audit.
- [x] `IAttendanceRepository.GetForEventAsync` — the uncapped per-event read the overlay needs.
- [x] DI registration; delete the three M-07 demo files and their lines.
- [x] `DemoParticipationReportService` hand-over — real rows override store rows; real counts.
- [x] `AttendanceInvalidationFlowTests` — the four required proofs plus blank reason, over-long reason, repeat invalidation, unknown/wrong event and Forbidden.
- [x] `DemoParticipationReportOverlayTests` — a real invalidated row overrides a seeded store row.
- [x] `DemoCheckInWindowFlagTests` — invalidation resolves to the real service with the demo switch on and off.
- [x] Demo register M-07 row and the M-13c paragraphs.
- [x] `DemoCertificateLedgerService` hand-over - real attendance wins for the validity gate and `AttendedAt`.
- [x] `DemoSeedService` log line and the unreachable `DemoStore.GetAttendanceForEvent`.
- [x] `AttendanceInvalidationFlowTests` - assert the real `GetForEventAsync` read returns the invalidated row.
- [x] `CertificateLedgerTests` - constructor takes the real attendance read; an invalidated real row is refused.

**Acceptance Criteria:**
- Given a valid attendance, when an Officer invalidates it with a reason, then the row is retained with `invalidated_at`, `invalidated_by` and the reason, and exactly one `attendance.invalidated` entry carries that reason.
- Given that invalidation, when the same registration is scanned or manually checked in, then the check-in is refused and no attendance or audit row is added.
- Given a registered handler that throws `Forbidden`, when invalidation runs, then the row stays valid and no audit entry is written.
- Given no registered handler, when invalidation runs, then it commits.
- Given a blank reason, an unknown registration, a wrong event or a Member role, when invalidation is posted, then nothing is written and the matching outcome is returned.
- Given `DemoData:Enabled` on or off, when the app starts, then `IAttendanceInvalidationService` resolves to `AttendanceInvalidationService`.
- Given a completed event with real attendance, when the report overlay runs, then its rows and counts reflect real invalidation.

## Open Questions

_None — all three settled on 2026-11-07 and recorded as Decisions in the frozen block._

## Implementation Notes

- `AttendanceInvalidationService` discovers the registration outside the transaction, then locks the participant's `users` row, the event and the attendance row in AD-4 order. The reason is trimmed and validated inside the transaction (1–500 characters, field `reason`), then an already-invalidated row is refused before any handler runs, so the first reason and its audit entry survive.
- Handlers run after the attendance lock and before `SaveChanges`, so a `Forbidden` throw rolls back the stamp and the audit together; an empty handler list commits. The audit payload is `EventId`, `RegistrationId`, `InvalidatedBy`, `InvalidatedAt` and `InvalidationReason`, and the entry's `Reason` carries the trimmed reason; no token or QR value is present.
- The attendance row is retained and the registration status is untouched, so 4.3's existing invalidated rule refuses a later scan or manual check-in.
- The M-13c overlay merges `IAttendanceRepository.GetForEventAsync` over the store map, so a real row wins for the same registration while seeded rows still fill the demo event. The merged map drives the valid count, rate, no-shows and breakdowns when it holds any row; otherwise the real SQL facts pass through.
- `DemoStore.InvalidateAttendance` stays as the seed helper; `GetAttendanceForEvent` was deleted because the report overlay now reads `IAttendanceRepository.GetForEventAsync` and nothing else called it.

## Plan Change Log

- 2026-11-07 review loopback 1 (bad_plan): the certificate ledger still answered attendance validity from the demo store, so a real invalidation left the store row valid and an Officer could approve or issue against invalid attendance — a regression this ticket introduced when invalidation stopped writing the store. Amended the Code Map and tasks to add the same real-over-store hand-over to `DemoCertificateLedgerService` that the report overlay already got. KEEP: the transaction/lock/hook design, the audit vocabulary, the DI replacement, the M-07 deletion, the report overlay merge and the `AttendanceInvalidationFlowTests` coverage all worked and must survive re-derivation. Also folded in three low findings: the `DemoSeedService` log still naming M-07, the unreachable `DemoStore.GetAttendanceForEvent`, and the overlay's mixed confirmed-count source.

## Review Triage Log

Review pass 1 (quick): 1 medium, 4 low, 1 false, 1 low folded into the medium.

| # | Finding | Verdict | Route | Evidence |
|---|---------|---------|-------|----------|
| 1 | `DemoCertificateLedgerService` reads attendance validity from the demo store, so a real invalidation is invisible and a stale review can be approved or issued | medium | bad_plan | `DemoCertificateLedgerService.cs:148` gates `RequireCurrentParticipationAsync` on `store.TryGetAttendance(...).IsValid` and `:38` reads the same row for `AttendedAt`. Before this change `DemoAttendanceInvalidationService` invalidated the store row, so the gate refused; now invalidation writes SQL only, so the store row stays valid and issuance is no longer blocked. Fixed by merging the real read over the store read, real wins. |
| 2 | `DemoSeedService` startup log still names M-07 as active | low | patch | `DemoSeedService.cs:93` logs "M-01, M-07" while `demo-mock-register.md:91` marks M-07 removed. |
| 3 | `DemoStore.GetAttendanceForEvent` is unreachable | low | patch | No caller in the solution; the overlay uses `IAttendanceRepository.GetForEventAsync`. Deleted. |
| 4 | Overlay derives the rate and no-show figures from `report.ConfirmedCount` while building the map from `confirmed` | low | patch | `DemoParticipationReportService.cs:83` vs `:110,:114`. Both mean Confirmed today; use `confirmed.Count` so the derivation is self-consistent. |
| 5 | The overlay's `hasAttendance` switch suppresses real facts and drops real breakdown membership | false | - | `validAttendees` is derived from the merged map over `confirmed`, and both breakdowns are computed from those same `validAttendees` (`:126-127`), so a real attendee with a real row is included. Only registrations with no attendance at all are excluded, which is the correct no-show rule. |
| 6 | No real-SQL test for `GetForEventAsync` or the seeded-path merge | low | patch | The overlay case uses `FakeAttendance`; added a real-SQL assertion in `AttendanceInvalidationFlowTests` that the read returns the invalidated row. |
| 7 | `DemoDataRegistration` comment understates the remaining demo surface | low | patch | Folded into finding 1: once the ledger reads real attendance the comment is accurate; updated it to name the merged read. |

## Design Notes

The transaction order and the hook placement are the whole design. Discovery runs outside the transaction (as 4.3 does) so a bad id fails cheaply; every fact is re-read after the locks. The handler runs after the attendance row is locked and before `SaveChanges`, so 6.5 can set the review row in the same `DbContext` and a `Forbidden` throw leaves both the stamp and the audit unwritten. The audit is appended after the stamp is saved, exactly like `attendance.recorded`, so its `EntityId` is the real attendance id.

No event-status check is added: SPEC §11.1 allows a correction after completion, and the demo provider never checked it. The issued-certificate refusal belongs to 6.5's handler, which is why this ticket's own verify says invalidation commits with no handler registered.

The overlay hand-over is required by register rule 12 and was explicitly left to 4.6 by the 4.3 plan ("the M-13c report overlay still reads the store until 4.6"). It is a merge, not a replacement: `DemoSeedService` writes its seeded check-ins only to the store, so a pure switch would empty the demo report that the register's seeded-state rule requires to stay populated. A real SQL row therefore wins over a store row for the same registration, and the store only fills registrations that have no real row. The certificate ledger applies the same merge to its validity gate and `AttendedAt`, so a real invalidation now blocks approval and issuance; the store still supplies the seeded reviews and issues until 6.5/6.7 replace them.

## Verification

**Commands:**
- `dotnet build` — expected: full solution builds.
- `$env:ATTENDTA_TEST_SQL="Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --filter "FullyQualifiedName~AttendanceInvalidationFlowTests|FullyQualifiedName~DemoParticipationReportOverlayTests|FullyQualifiedName~DemoCheckInWindowFlagTests|FullyQualifiedName~CheckInFlowTests|FullyQualifiedName~CheckInLookupFlowTests"` — expected: all pass.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — expected: all pass.

**Manual checks (if no CLI):**
- With a seeded open event, check a member in, invalidate with a reason, and confirm the row stays with the reason, the Activity Log shows `attendance.invalidated`, and a second check-in is refused.
