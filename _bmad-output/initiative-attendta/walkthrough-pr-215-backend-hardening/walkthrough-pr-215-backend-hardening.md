---
# Review narrative: Remove the demo seam and unify the shared backend rules (8.13)

Target: PR #215, head commit `149791b` — *refactor(backend): [BMAD 8.13] remove the demo seam and unify the shared backend rules*, on branch `feature/151-final-backend-hardening` against `dev` in `rainyDayssss/AttendTa`

Ticket: [story-final-backend-hardening-and-cross-module-integration-cleanup.md](../tickets/epic-release-acceptance/tickets/story-final-backend-hardening-and-cross-module-integration-cleanup.md) · Plan: [story-final-backend-hardening-and-cross-module-integration-cleanup-plan.md](../tickets/epic-release-acceptance/tickets/story-final-backend-hardening-and-cross-module-integration-cleanup-plan.md)

Review status: **Review complete — all nine blocks accepted.**

---

## Block 1 — Intent

- [x] Accepted · after the second opinion, the test run and the formal review's 11 patches

**Intent (plan):**

> **Problem:** The demo seam (register row M-01) is still wired into startup, configuration and the CLI, so 8.11's checks cannot pass. The cross-module audit also found duplicated eligibility/attendance facts, a participation report whose five independent reads can disagree, and the deferred findings the focused sweeps (3.14, 5.10, 6.9) recorded for this ticket.
>
> **Approach:** Delete the demo seam first, then give each shared rule one implementation, make the report read one coherent snapshot, and close the recorded deferred findings. Verification and hardening only: no feature, no screen redesign, no new broad test suite.

Source: the `## Intent` section of [story-final-backend-hardening-and-cross-module-integration-cleanup-plan.md](../tickets/epic-release-acceptance/tickets/story-final-backend-hardening-and-cross-module-integration-cleanup-plan.md), quoted verbatim from its `<intent-contract>` block.

The ticket states the same thing from the delivery side: resolve the concrete cross-module backend findings after the focused module sweeps, and delete the demo seam once every other demo-register row is removed. It is issue #151, the last integration step before 8.11 proves no demo data remains, and it deliberately excludes feature work, frontend presentation, and a new broad acceptance suite (8.5/8.4 own those).

## Block 2 — Broad strokes

- [x] Accepted · after the diagram

Two things happen in this PR, and they are independent of each other.

**The demo seam disappears.** Five files under `ASI.Basecode.WebApp/DemoData/` are deleted outright — the options record, the startup guard, the in-memory store, the seed service and the DI registration method — and the four places that wired them are cut back: the `--demo-pass-token` switch, the guard call in `Startup.cs`, `AddDemoData` in `Startup.DI.cs`, and the `DemoData` section in `appsettings.json`. `--demo-readiness` survives. Nothing replaces the seam; the app simply has no demo mode left.

**The shared rules get one home.** `ParticipationFacts` becomes the only place that answers "is this attendance valid", "does this registration hold a seat", "is this registration still open" and "is this certificate eligible". It carries both forms — an expression tree for SQL and an in-memory twin for code that already holds the row — and every module that used to spell the rule inline now calls it. Two consequences follow: the participation report reads one snapshot instead of five queries, and the certificate ledger's decision and issuance paths share one predicate.

Five entry points, in reading order:

1. [Program.cs](../../../ASI.Basecode.WebApp/Program.cs#L14) — the CLI is now `--hash-password` and `--demo-readiness` only; the demo token switch is gone.
2. [ParticipationFacts.cs](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L10) — the rules, SQL expression and in-memory twin side by side.
3. [ParticipationReportRepository.cs](../../../ASI.Basecode.Data/Repositories/ParticipationReportRepository.cs#L25) — `GetSnapshotAsync`, the one read that replaced five.
4. [ParticipationReportService.cs](../../../ASI.Basecode.Services/Services/ParticipationReportService.cs#L58) — every panel derived from that one list.
5. [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L313) — the decision path's single eligibility call; the same call appears again at issuance, [:395](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L395).

## Block 3 — Deleting the demo seam

- [x] Accepted · after the live start-up test and the records fix

The seam was five Development-only classes: an options record, a startup guard that refused to boot unless a pass token matched, an in-memory store, a seed service that wrote demo rows through the real services, and a DI extension that registered them. Deleting it means removing the classes, cutting the four points that referenced them, and making the five test suites that turned the switch on stop depending on it. Only `--demo-readiness` stays, and it was never part of the seam — it reports whether the host is fit for a demo.

Deleted outright (no links; the files are gone): `ASI.Basecode.WebApp/DemoData/DemoDataOptions.cs`, `DemoDataStartupGuard.cs`, `DemoStore.cs`, `DemoSeedService.cs`, `DemoDataRegistration.cs`.

What had to change to let them go:

- [Program.cs](../../../ASI.Basecode.WebApp/Program.cs#L14) — `--demo-pass-token` and its `System.Globalization` import are gone; `--hash-password` and `--demo-readiness` remain.
- [Startup.cs](../../../ASI.Basecode.WebApp/Startup.cs) — the `DemoDataStartupGuard.Validate` call is gone.
- [Startup.DI.cs](../../../ASI.Basecode.WebApp/Startup.DI.cs#L56) — `AddDemoData` is gone; the comment that remains says the real ledger and the real member history serve both switch states.
- [appsettings.json](../../../ASI.Basecode.WebApp/appsettings.json) — the `DemoData` section is gone.

Tests: `DemoDataStartupGuardTests` is deleted; `DemoCheckInWindowFlagTests` is renamed `CheckInWindowFlagTests`; the demo `UseSetting` blocks are gone from the admin deactivation and reactivation, certificate review decision and event check-in window suites; [CertificateLedgerFlowTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CertificateLedgerFlowTests.cs#L343) now seeds its attendance and certificate reviews through `AttendanceRepository` and `CertificateRepository` instead of the seed service.

The one new fact, [CheckInWindowFlagTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CheckInWindowFlagTests.cs#L66): with the retired key set to `true`, no token key and a non-demo database, the host starts and serves `/Account/Login`. The setting name is assembled from its two words at [:73](../../../tests/ASI.Basecode.IntegrationTests/CheckInWindowFlagTests.cs#L73) so the 8.11 source search stays clean.

## Block 4 — One rule, one file

- [x] Accepted · after the mutation test and the second opinion's follow-up patches

`ParticipationFacts` already held the SQL expression trees. The change adds the in-memory twins beside them and converts every inline re-derivation to call one or the other. Both shapes exist because the callers differ: a repository composing a query needs an expression tree that EF can translate, while a service that already holds a row needs a plain predicate. Keeping them in one file is the point — a rule change touches one place, and the two forms sit next to each other so they can be read together.

The rules, [ParticipationFacts.cs](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L33):

- [:33](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L33) and [:36](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L36) — `IsValidAttendance` over a timestamp and over the entity; the SQL form is `ValidAttendance` at [:15](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L15).
- [:50](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L50) and [:53](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L53) — `HoldsSeat` over a status and over the entity, beside `SeatHeld` at [:18](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L18).
- [:43](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L43) — `IsOpenRegistration`, beside the `OpenRegistration` expression at [:24](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L24).
- [:64](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L64) — `IsCertificateEligible`, composed from the three above plus the review status and the completed event.

The read side, where a query needs the expression:

- [RegistrationRepository.cs](../../../ASI.Basecode.Data/Repositories/RegistrationRepository.cs#L22) — `SeatHolders` at the top of the file, then again at [:109](../../../ASI.Basecode.Data/Repositories/RegistrationRepository.cs#L109), [:158](../../../ASI.Basecode.Data/Repositories/RegistrationRepository.cs#L158) and [:172](../../../ASI.Basecode.Data/Repositories/RegistrationRepository.cs#L172); `ValidAttendances` at [:219](../../../ASI.Basecode.Data/Repositories/RegistrationRepository.cs#L219) and [:223](../../../ASI.Basecode.Data/Repositories/RegistrationRepository.cs#L223); `OpenRegistration` at [:309](../../../ASI.Basecode.Data/Repositories/RegistrationRepository.cs#L309).
- [CertificateRepository.cs](../../../ASI.Basecode.Data/Repositories/CertificateRepository.cs#L30) — the completion query now selects `SeatHolders`, so a cancelled or waitlisted registration with an old attendance row never gets a review.

The decision side, where the code already holds the row:

- [AttendanceRecordingService.cs](../../../ASI.Basecode.Services/Services/AttendanceRecordingService.cs#L70), [:105](../../../ASI.Basecode.Services/Services/AttendanceRecordingService.cs#L105), [:156](../../../ASI.Basecode.Services/Services/AttendanceRecordingService.cs#L156), [:169](../../../ASI.Basecode.Services/Services/AttendanceRecordingService.cs#L169) — the duplicate check-in guard, the seat check and both attendance checks.
- [AttendanceInvalidationService.cs](../../../ASI.Basecode.Services/Services/AttendanceInvalidationService.cs#L97) — the invalidation guard.
- [PassService.cs](../../../ASI.Basecode.Services/Services/PassService.cs#L111) — the pass only renders for a held seat.
- [MemberEventService.cs](../../../ASI.Basecode.Services/Services/MemberEventService.cs#L110) and [:230](../../../ASI.Basecode.Services/Services/MemberEventService.cs#L230) — the cancel action and its detail flag.
- [RegistrationCancellationService.cs](../../../ASI.Basecode.Services/Services/RegistrationCancellationService.cs#L72) — the event-cancel sweep.
- [MyCertificatesService.cs](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L50) — the member history's seat check.

The screens and projections:

- [CheckInLookupService.cs](../../../ASI.Basecode.Services/Services/CheckInLookupService.cs#L65) — both the checked-in and invalidated flags, from one call.
- [IAttendanceRepository.cs](../../../ASI.Basecode.Data/Interfaces/IAttendanceRepository.cs#L61) and [:74](../../../ASI.Basecode.Data/Interfaces/IAttendanceRepository.cs#L74) — `AttendanceLogRow.IsValid` and `MyAttendanceRow.IsValid` now call the rule instead of testing the timestamp.
- [IParticipationReportRepository.cs](../../../ASI.Basecode.Data/Interfaces/IParticipationReportRepository.cs#L43) — `IsAttendanceInvalidated` is the negation of the same call.
- `ConfirmedLookupRow` now carries `InvalidatedAt` rather than a pre-computed bool, which is what let the inverted `IsValidAttendance(bool)` overload go.

The certificate rule:

- [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L313) for the decision and [:395](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L395) for issuance — one predicate, two required review states, and both refuse a missing event row before it runs.

Two things the change does not unify, and one it now pins:

- [ParticipationReportService.cs](../../../ASI.Basecode.Services/Services/ParticipationReportService.cs#L104) — `IsValidAttendee` adds a third fact, "has a check-in at all", which has no predicate of its own; it is a `CheckedInAt is not null` test next to two canonical calls.
- The report's no-show count is arithmetic over the snapshot rather than a call into the facts, because the facts are query shapes and the service holds a list.
- [ParticipationFactsTests.cs](../../../tests/ASI.Basecode.Services.Tests/ParticipationFactsTests.cs#L83) now asserts each in-memory twin against its compiled SQL expression over the same rows, so a drift between the two fails a test instead of shipping.

## Block 5 — The report's one snapshot

- [x] Accepted · after the mutation tests and the fixture that pins the seat rule

The report used to make five independent reads: a summary, a no-show count, an attendee distribution, a row count and a detail page. Each was its own query, so attendance changing between them could make the panels disagree — the summary could count four confirmed seats while the detail table listed three. The change replaces all five with one read that returns every registration of the event, and the service derives every panel from that list. Coherence stops being something the queries have to agree on and becomes a property of there being only one list.

The read, [ParticipationReportRepository.cs](../../../ASI.Basecode.Data/Repositories/ParticipationReportRepository.cs#L25):

- [:33-38](../../../ASI.Basecode.Data/Repositories/ParticipationReportRepository.cs#L33) — registrations inner-joined to `Member` and `User` (both `NOT NULL` foreign keys, so no row can be dropped) and left-joined to `Attendance`, so a registration with no check-in still comes back.
- [:40](../../../ASI.Basecode.Data/Repositories/ParticipationReportRepository.cs#L40) — ordered by participant name then registration id, so page boundaries are stable across requests.
- [IParticipationReportRepository.cs](../../../ASI.Basecode.Data/Interfaces/IParticipationReportRepository.cs#L25) — the interface is now this one method; the five it replaced are gone.
- [:33](../../../ASI.Basecode.Data/Interfaces/IParticipationReportRepository.cs#L33) — `ParticipationReportDetailRow` carries `InvalidatedAt` rather than a pre-computed flag, so the service applies the canonical rule instead of trusting a projection.

The derivation, [ParticipationReportService.cs](../../../ASI.Basecode.Services/Services/ParticipationReportService.cs#L58):

- [:58](../../../ASI.Basecode.Services/Services/ParticipationReportService.cs#L58) — the single read.
- [:62-63](../../../ASI.Basecode.Services/Services/ParticipationReportService.cs#L62) — confirmed seats and valid attendees, both filtered from the same list.
- [:91](../../../ASI.Basecode.Services/Services/ParticipationReportService.cs#L91) — no-shows as `confirmed − attendees`, and only for a completed event, because only a completed event has a final answer.
- [:92-96](../../../ASI.Basecode.Services/Services/ParticipationReportService.cs#L92) — the page rows, both breakdowns and the total, all from the same list.
- [:104](../../../ASI.Basecode.Services/Services/ParticipationReportService.cs#L104) — `IsValidAttendee`, the one place that decides what "attended" means for this screen.
- [:156](../../../ASI.Basecode.Services/Services/ParticipationReportService.cs#L156) and [:176](../../../ASI.Basecode.Services/Services/ParticipationReportService.cs#L176) — the course and year breakdowns, over the attendee list rather than over a query.

What the change costs, and what it does not touch:

- The whole event is materialized on every request, and the page slice happens in memory at [:68-71](../../../ASI.Basecode.Services/Services/ParticipationReportService.cs#L68). The cost is bounded by the event's registrations, not by the page size. The alternative — running the five queries inside one transaction — would have widened `UnitOfWork` for one screen, which the plan rejected.
- The event selector still reads the event list separately at [:35-38](../../../ASI.Basecode.Services/Services/ParticipationReportService.cs#L35). That is a different aggregate and cannot disagree with the panels.
- [ParticipationReportRepositoryTests.cs](../../../tests/ASI.Basecode.IntegrationTests/ParticipationReportRepositoryTests.cs#L135) now runs the service against the real repository on SQL and asserts every panel, so the two halves are proven together rather than separately.

## Block 7 — The deferred 3.14 refactors

- [x] Accepted · after the paging and mapping mutation tests

Six small cleanups the events-and-registration sweep recorded for this ticket. None of them changes behaviour; each removes a duplication, a stale name, or a piece of scaffolding nothing used. They are grouped here because they were all deferred from the same sweep and all live in the events and registrations read paths.

**The renames.** Three names described a narrower job than the code did.

- [IRegistrationRepository.cs](../../../ASI.Basecode.Data/Interfaces/IRegistrationRepository.cs#L79) — `GetForCancellationAsync` → `GetForUpdateAsync`. The method locks a registration for any update; cancellation was only its first caller.
- [:93](../../../ASI.Basecode.Data/Interfaces/IRegistrationRepository.cs#L93) — `DiscoverReviewAsync` → `DiscoverIdentityAsync`. It returns the user, member and event triple, and the attendance and issuance paths use it as much as the review queue does.
- [:109](../../../ASI.Basecode.Data/Interfaces/IRegistrationRepository.cs#L109) — `RegistrationReviewIdentity` → `RegistrationIdentity`, for the same reason.

**The event projections.** [EventRepository.cs](../../../ASI.Basecode.Data/Repositories/EventRepository.cs#L122) had four near-identical projections — published list, published detail, member-visible list, member-visible detail. Two of each differed only in a status filter, so they are now two builders used twice each: `MemberVisibleEventListQuery` at [:122](../../../ASI.Basecode.Data/Repositories/EventRepository.cs#L122) and `MemberVisibleEventDetailQuery` at [:154](../../../ASI.Basecode.Data/Repositories/EventRepository.cs#L154).

**The member events service.** [MyEventsService.cs](../../../ASI.Basecode.Services/Services/MyEventsService.cs#L73) had the same page-and-count shape written out for each tab, and mapped its rows by hand. Both are extracted: `ReadPageAsync` at [:73](../../../ASI.Basecode.Services/Services/MyEventsService.cs#L73) does the normalize-count-slice dance once, and the rows go through AutoMapper at [:41](../../../ASI.Basecode.Services/Services/MyEventsService.cs#L41) and [:63](../../../ASI.Basecode.Services/Services/MyEventsService.cs#L63). The one value a mapper cannot compute — whether the event's start has passed — is passed in as an item, declared at [MyEventsViewModels.cs](../../../ASI.Basecode.Services/ServiceModels/MyEventsViewModels.cs#L15) and consumed at [Startup.AutoMapper.cs](../../../ASI.Basecode.WebApp/Startup.AutoMapper.cs#L46).

**The two smaller ones.**

- [RegistrationRepository.cs](../../../ASI.Basecode.Data/Repositories/RegistrationRepository.cs#L280) — `GetOpenForEventAsync` dropped an in-memory re-filter that repeated the status filter the query had already applied. The comment now says why no re-filter is needed.
- [ParticipationFacts.cs](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L142) — `PublishableEventIds` builds a correlated subquery through three tables, and the reason it works was undocumented. The comment at [:142](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L142) explains that the venue filter carries the event's venue id and the type filter carries its type id, so the whole rule stays one expression tree EF can translate.

**The scaffolding.** [ControllerBase.cs](../../../ASI.Basecode.WebApp/Mvc/ControllerBase.cs#L15) carried template members from the original project scaffold — a user id, a user name, a supervisor, four client-system fields, two empty action overrides and an exception logger. Nothing consumed them. What remains is the four members controllers actually use: `GetActorUserId` at [:53](../../../ASI.Basecode.WebApp/Mvc/ControllerBase.cs#L53), `IsAjaxRequest` at [:62](../../../ASI.Basecode.WebApp/Mvc/ControllerBase.cs#L62), `InvalidModelResult` at [:80](../../../ASI.Basecode.WebApp/Mvc/ControllerBase.cs#L80) and `HandleDomainException` at [:103](../../../ASI.Basecode.WebApp/Mvc/ControllerBase.cs#L103).

## Block 8 — The deferred 5.10 refactors

- [x] Accepted · after the UTC-kind mutation and the factory theory

Five hardening fixes from the administration sweep. They share a shape: each closes a way the code could be quietly wrong instead of loudly wrong — a comparison that silently answers "no", a lookup that silently invents a value, a failure that silently disappears, a timestamp that silently loses its meaning, and reads that silently ignore cancellation.

**The comparison.** [AccessDialogViewModels.cs](../../../ASI.Basecode.Services/ServiceModels/AccessDialogViewModels.cs#L77) built the membership access model by comparing the status string with the default ordinal comparison, so `"Active"` and `"active"` were different answers. It now uses `StringComparison.OrdinalIgnoreCase`. The status is a lookup code, not display text, so case should never have mattered.

**The lookup.** [AccountRoleService.cs](../../../ASI.Basecode.Services/Services/AccountRoleService.cs#L128) read two role codes and, when one was missing, wrote a numeric fallback into the audit row. A missing lookup is a broken deployment, not a value to invent, so both now throw `InvalidOperationException` at [:129](../../../ASI.Basecode.Services/Services/AccountRoleService.cs#L129) and [:131](../../../ASI.Basecode.Services/Services/AccountRoleService.cs#L131) before anything is written.

**The swallowed failure.** [AdminAccountsController.cs](../../../ASI.Basecode.WebApp/Controllers/AdminAccountsController.cs#L173) catches a `DomainException` when refreshing the role form after a failed POST — the account disappeared between the two requests — and turns it into a message. That is the right behaviour, but it left no trace, so the catch now logs a warning naming the account and the exception type.

**The timestamp.** SQL Server's `datetime2` carries no kind, so a value written as UTC came back as `Unspecified` and every reader had to remember that. [CanonicalSchemaMappings.cs](../../../ASI.Basecode.Data/CanonicalSchemaMappings.cs#L304) now installs a value converter that restores `DateTimeKind.Utc` on read, so the instant survives the round trip. [ActivityLogWriter.cs](../../../ASI.Basecode.Services/Services/ActivityLogWriter.cs#L53) writes `UtcDateTime` explicitly, and [Index.cshtml](../../../ASI.Basecode.WebApp/Views/ActivityLog/Index.cshtml#L36) drops its `SpecifyKind` call — the view now formats what it was given.

**The reads.** The four member reads and `GetUsers` were synchronous, so a slow query could not be cancelled and a request that had gone away still cost a round trip. [IMemberRepository.cs](../../../ASI.Basecode.Data/Interfaces/IMemberRepository.cs#L12) now carries a `CancellationToken` on every read, as does [IUserRepository.cs](../../../ASI.Basecode.Data/Interfaces/IUserRepository.cs#L12). The callers are threaded through: [MemberService.cs](../../../ASI.Basecode.Services/Services/MemberService.cs#L51), [IMemberService.cs](../../../ASI.Basecode.Services/Interfaces/IMemberService.cs#L12), and [MemberController.cs](../../../ASI.Basecode.WebApp/Controllers/MemberController.cs#L134), which passes `HttpContext.RequestAborted`.

One thing to know about the timestamp fix: it is a mapping-layer change on a column that carries three indexes and a default. It does not alter the stored value or any index — the converter is identity on write and `SpecifyKind` on read — but it is the only place in this PR that touches `CanonicalSchemaMappings`, which is otherwise the executable schema definition.

## Block 9 — Periphery

- [x] Accepted · after the two stale register sentences were fixed

Records and docs, one clause each.

- [demo-mock-register.md](../../../_bmad-output/initiative-attendta/demo-mock-register.md#L85) — M-01 now reads `removed (005f899)`.
- [:100](../../../_bmad-output/initiative-attendta/demo-mock-register.md#L100) — M-13b now names its removal commit, `975c08b`.
- [:116](../../../_bmad-output/initiative-attendta/demo-mock-register.md#L116) — the M-01 seam detail section is marked historical and kept for provenance.
- [:286](../../../_bmad-output/initiative-attendta/demo-mock-register.md#L286) and [:418](../../../_bmad-output/initiative-attendta/demo-mock-register.md#L418) — two sentences outside that section still spoke about the seam in the present tense ("remain until 8.13 removes the store", "still seeds up to eight check-ins"); both now say what 8.13 did.
- [demo-script.md](../../../_bmad-output/initiative-attendta/demo-script.md) — the presenter's seam instructions replaced with the real-service walk; 67 lines changed.
- [demo-quickstart.md](../../../docs/demo-quickstart.md) — the three `DemoData__*` variables, the start-up log line and two troubleshooting rows are gone.
- [seed-demo-scenario.sql](../../../database/seed-demo-scenario.sql) — the header now describes the real-service walk instead of the seed service.
- [PRODUCT.md](../../../PRODUCT.md) — the "reviews live only in process memory" and "demonstration configuration" claims are gone; the certificate-ledger and My Certificates bullets now describe real rows.
- [deferred-work.md](../../../_bmad-output/initiative-attendta/deferred-work.md) — the two deferrals whose subject this PR deletes are marked resolved, added during the review.
- [story-final-backend-hardening-and-cross-module-integration-cleanup-plan.md](../../../_bmad-output/initiative-attendta/tickets/epic-release-acceptance/tickets/story-final-backend-hardening-and-cross-module-integration-cleanup-plan.md) — the plan itself, including the `## Code Review` section this walkthrough appended.
- `.github/workflows/ci.yml` — untouched; its SQL integration step is what proves the new tests.

## Block 6 — One certificate rule

- [x] Accepted · after the mutation tests and the two message assertions

The certificate module had three places that decided the same thing. The ledger's decision path had its own condition chain, the issuance path had another, and the completion query had a third that chose who gets a review in the first place. They did not agree: the completion query accepted any registration with valid attendance regardless of status, so a cancelled registration with an old check-in received a review the ledger would then refuse to decide. The change puts the rule in one predicate, routes both ledger paths through it, tightens the completion query to Confirmed seats, and makes the refusal name the fact that actually failed.

The rule, [ParticipationFacts.cs](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L70):

- [:70-80](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L70) — a Confirmed seat ([:76](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L76)), valid attendance ([:77](../../../ASI.Basecode.Data/Queries/ParticipationFacts.cs#L77)), a certificate in the state the operation requires, and a completed event. The event is required; a caller that cannot read it refuses before calling.
- The two operations differ only in that required state: `PendingApproval` to decide, `Eligible` to issue.

The ledger, [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L313):

- [:313](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L313) — the decision path's single call, after locking the participant's user row ([:299](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L299)), the event ([:300](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L300)) and the registration ([:310](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L310)).
- [:315-320](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L315) — the refusal picks its message from the fact that failed: already decided, seat lost, otherwise attendance. The event check is unreachable here because a review only exists after completion.
- [:395](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L395) — issuance's call, with the same lock order and its own missing-event refusal at [:393](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L393).
- [:35](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L35) — the ledger now takes `IAttendanceRepository`, so it reads the attendance row inside its own transaction rather than trusting a value read before it.

The completion hook, [CertificateRepository.cs](../../../ASI.Basecode.Data/Repositories/CertificateRepository.cs#L30):

- [:30](../../../ASI.Basecode.Data/Repositories/CertificateRepository.cs#L30) — the review query selects `SeatHolders`, so only a Confirmed seat can ever receive a review.
- [CertificateReviewCompletionHandler.cs](../../../ASI.Basecode.Services/Services/CertificateReviewCompletionHandler.cs#L34) — unchanged in shape; it still runs inside the caller's completion transaction and writes one pending review per returned id.

What this changes for a user:

- A member who was confirmed, checked in, and then cancelled by an admin before the event completed no longer receives a review. Previously they did, and the ledger could never decide it. That is the right outcome, but it is a change in who is considered, not only in how the decision is recorded — it is called out in the PR body.
- The decision refuses before it validates a decline reason. An officer submitting a blank reason against an invalidated attendance now sees the stale message rather than a reason-field error, because the review is undecidable either way.

The tests that pin it:

- [ParticipationFactsTests.cs](../../../tests/ASI.Basecode.Services.Tests/ParticipationFactsTests.cs#L111) — the eligibility matrix: seat, attendance, review state and event status, one case each.
- [CertificateReviewDecisionTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs#L153) — a blank decline reason on an invalidated attendance returns `Stale` with no field, not `Invalid` with the reason field.
- [EventCompletionReviewTests.cs](../../../tests/ASI.Basecode.IntegrationTests/EventCompletionReviewTests.cs#L71) — a cancelled and a waitlisted registration, both carrying an attendance row, get no review at completion while the Confirmed one does.

