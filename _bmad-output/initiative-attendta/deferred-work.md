- source_plan: `_bmad-output/initiative-attendta/tickets/epic-platform-access/tickets/story-caller-owned-transaction-unit-of-work-and-append-only-activi-plan.md`
  summary: Remove venue image paths from cleanup-failure diagnostics.
  evidence: `VenueController.TryDeleteVenueImage` predates this story and logs the relative image path and exception, which may contain a filesystem path.
- source_plan: none
  summary: Improve the public account-request form with course choices, password guidance, field hints, and validation feedback.
  evidence: Split from management account-request review so approval and rejection can be delivered independently first.
- source_plan: `_bmad-output/initiative-attendta/plan-gh-34-signup-deadline.md`
  summary: Show precise Manila signup deadlines when stored values include seconds or milliseconds.
  evidence: Existing ManilaTimeFormatter truncates to minutes while EventLifecycleService accepts more precise timestamps, potentially displaying a deadline earlier than the actual cutoff.
- source_plan: `_bmad-output/initiative-attendta/handoffs/story-refactor-sweep-follow-up.md`
  summary: Add reviewer, reviewed-at, and reason metadata to decided account-request rows and keep the table shell on empty tabs.
  evidence: Impeccable critique on 2026-11-06 scored the queue 17/40; decided rows show only "Reviewed" and empty tabs drop the toolbar and column context.
- source_plan: `_bmad-output/initiative-attendta/tickets/epic-check-in/tickets/story-frontend-member-attendance-history-partial-plan.md`
  summary: Give one demo member enough attendance rows for the Attendance tab's pager to be demonstrable.
  status: resolved (149791b)
  evidence: `DemoSeedService` seeded at most eight confirmed registrations of one event (`MaxSeededCheckIns = 8`) and a member has one registration per event, so a member's history held one row and `_Pager` never rendered on `/MyEvents/Attendance`. 8.13 deleted the seed service with the rest of the demo seam, so the demo no longer seeds attendance at all and the concern has no mechanism left; the tab still pages correctly for 26+ rows (verified by a temporary integration check) once real check-ins supply the volume.
- source_plan: `_bmad-output/initiative-attendta/tickets/epic-participation-reports/tickets/story-event-selector-and-participation-summary-kpis-plan.md`
  summary: Until 7.3 supplies real detail rows, the switch-off report shows its summary KPIs beside the unchanged view's "No registrations"/"No attendee data" empty-state copy.
  status: resolved (04157b3)
  evidence: `ParticipationReportService` returned empty `Rows` by design while 7.2/7.3 were open. 7.3 closed it: the service now reads the counted page and fills the detail table whenever the event has registrations.
- source_plan: `_bmad-output/initiative-attendta/tickets/epic-check-in/tickets/story-member-attendance-view-on-my-events-plan.md`
  summary: Seeded demo-store check-ins no longer appear in the member Attendance tab after 4.7; only real SQL attendance does.
  status: resolved (04157b3)
  evidence: 4.7's approved intent replaced the demo attendance source with the owner-scoped SQL query, so `DemoSeedService`'s in-memory check-ins remained visible on the report overlay (M-13c seam) but not in `/MyEvents/Attendance`. 7.3 removed that overlay, so the report also reads real SQL attendance only and the seeded check-ins feed the certificate ledger alone. A check-in made through the real 4.3 engine does appear. The demo store was removed by 8.13 (149791b).
- source_plan: `_bmad-output/initiative-attendta/tickets/epic-check-in/tickets/story-member-attendance-view-on-my-events-plan.md`
  summary: `MyEventsController.Attendance` (and `Registrations`) lacks the `ResponseCache(NoStore)` attribute that the sibling `Certificates` action carries.
  evidence: `docs/CODING_STANDARDS.md` §5 asks sensitive responses to use the agreed no-store policy, and `Certificates` sets `NoStore = true` while `Attendance` and `Registrations` do not. The gap predates 4.7 and the approved intent freezes the controller action, so it is recorded rather than patched; no cache leak was demonstrated.

## Deferred from: code review of story-transactional-certificate-issuance-pipeline-and-audit-plan (2026-11-07)

- source_plan: `_bmad-output/initiative-attendta/tickets/epic-certificates/tickets/story-transactional-certificate-issuance-pipeline-and-audit-plan.md`
  summary: With demo data off, an issued certificate cannot be viewed or downloaded until 6.8.
  status: deferred
  evidence: `CertificateLedgerService.GetViewerAsync` and `GetPdfAsync` still refuse as not-found, so 6.7 can issue a certificate that no non-demo user can open. The demo provider serves the retained file, so the demo is unaffected. Accepted by the project owner because 6.8 is the next integration ticket and implements the real owner-or-Management viewer/PDF.

## Deferred from: code review of story-refactor-sweep-plan (2026-11-07)

- source_plan: `_bmad-output/initiative-attendta/tickets/epic-events-registration/tickets/story-refactor-sweep-plan.md`
  summary: `ParticipationFacts.PublishableEventIds` builds a correlated projection (`venueIds` selects `eventItem.EventId` from a `Venue`-typed lambda) that is correct only because the keys are unique FKs, and it carries no comment.
  evidence: Code review of PR #204 (2026-11-07). The body is unchanged context in that diff — only the pass-through wrapper was removed — so the readability hazard predates the sweep and was deferred rather than patched.
- source_plan: `_bmad-output/initiative-attendta/tickets/epic-events-registration/tickets/story-refactor-sweep-plan.md`
  summary: No findings ledger backs the "concrete deferred findings" that the 3.14 refactor sweep closed.
  evidence: Code review of PR #204 (2026-11-07). None of the eleven findings appears in this file, the epic memlog, or any sibling plan's deferrals; the sweep's own plan is their only enumeration, so closure cannot be checked against a record that predates the change. Recording future findings here is what makes the next sweep verifiable.

## Deferred from: code review of story-member-certificate-download-links-in-my-events-plan (2026-11-08)

- source_plan: `_bmad-output/initiative-attendta/tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md`
  summary: `DemoStore.GetPassToken` has no consumer.
  status: resolved (149791b)
  evidence: The instance method was already unreferenced at the plan baseline (e757812); only the static `DerivePassToken` was used, by `Program.cs`. 8.13 deleted `DemoStore` and the `--demo-pass-token` switch with the rest of the demo seam, so the accessor no longer exists.

## Deferred from: code review of story-member-certificate-download-links-in-my-events-plan (2026-11-08, decisions)

- source_plan: `_bmad-output/initiative-attendta/tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md`
  summary: An issued certificate with no retained file renders as "Not issued" with its number hidden in the member history.
  status: deferred
  evidence: The service demotes an Issued row without `file_path` to `Unavailable` and the unchanged partial prints "Not issued" and "No PDF available" while the SQL row holds a real certificate number. Deferred because the frozen matrix defines this row as Unavailable with no links and the copy lives in a partial the ticket forbids touching; revisit in 8.5/8.12.
- source_plan: `_bmad-output/initiative-attendta/tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md`
  summary: No test exercises the rendered member certificate surface.
  status: deferred
  evidence: The only behavioural assertion is that the certificate number string appears in the page; the Issued links, unissued rows, issued-without-file row, member-route identical 404 and switch-off tab are unasserted. The deferral to 8.5 (#78) was approved on 2026-11-08 and the rows were driven manually in the 6.9 walkthrough.

## Deferred from: code review of story-refactor-sweep-plan (2026-11-08)

- source_plan: `_bmad-output/initiative-attendta/tickets/epic-administration/tickets/story-refactor-sweep-plan.md`
  summary: The two membership `IsActive` derivations disagree on letter case: `AccessDialogService` compares `Status` with `OrdinalIgnoreCase` while `MembershipAccessViewModel.For` uses ordinal `== "active"`.
  status: deferred
  evidence: `AccessDialogService.GetMembershipAccessAsync` and `AccessDialogViewModels.For(MemberViewModel)` derive the same flag differently. `MemberRepository` emits lowercase `active`/`inactive` today, so both agree in practice, and the new `AccessDialogServiceTests.MembershipStatusIsCaseInsensitive` pins only the service's contract. The `For` factory predates this sweep, so it is recorded rather than patched; a one-line `string.Equals(..., OrdinalIgnoreCase)` in `For` would close it.
