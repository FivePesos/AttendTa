# Verification record: Remove prototype data and prove complete real integrations (8.11)

Target: branch `feature/152-remove-prototype-data-and-prove-real-integrations` against `dev` in
`rainyDayssss/AttendTa`, baseline `2d05a3580645e91aac43770caebe473224ac8f5a`

Ticket: [story-remove-prototype-data-and-prove-complete-real-integrations.md](../tickets/epic-release-acceptance/tickets/story-remove-prototype-data-and-prove-complete-real-integrations.md) · Plan: [story-remove-prototype-data-and-prove-complete-real-integrations-plan.md](../tickets/epic-release-acceptance/tickets/story-remove-prototype-data-and-prove-complete-real-integrations-plan.md)

Register: [demo-mock-register.md](../demo-mock-register.md) · Case: MOCK-01 in [finale-test-cases.md](../finale-test-cases.md)

Status: **Gate passes.** Every register row is `removed` with the commit that deleted its demo
code; the published build starts with the retired switch on, no token key and a non-demo database;
the source and the published output hold no register id, no demo folder and no `DemoData` key; and
every required flow ran end to end against one database through the real services.

---

## 1. Register verdict

The register's 21 rows were checked against git, not against their own text. For each row the
recorded commit is the commit that deleted the row's demo class or its last demo code, confirmed
with `git log --diff-filter=D -- <path>` and `git show --stat <sha>`.

| Row | Removed by | Commit | Deleting evidence |
|---|---|---|---|
| M-01 | 8.13 | `149791b` | deletes `DemoDataOptions.cs`, `DemoDataStartupGuard.cs`, `DemoStore.cs`, `DemoSeedService.cs`, `DemoDataRegistration.cs` |
| M-02 | 3.13 | `6668425` | deletes `DemoMyEventsService.cs`, `EmptyMyEventsService.cs`, `DemoMyEventsRegistration.cs` |
| M-03 | 4.1 | `81f721e` | deletes `DemoPassService.cs`, `UnavailablePassService.cs`, `DemoPassesRegistration.cs` |
| M-04 | 4.2 | `7f6ae22` | deletes `DemoEventStateCommands.cs`, `DemoEventStateOverlay.cs`, `DemoEventsRegistration.cs` and their contracts |
| M-05 | 4.3 | `2a8b9b5` | deletes `DemoCheckInService.cs`, `UnavailableCheckInService.cs` |
| M-06 | 4.5 | `ff06123` | deletes `DemoCheckInLookupService.cs`, `UnavailableCheckInLookupService.cs` |
| M-07 | 4.6 | `6a82840` | deletes `DemoAttendanceInvalidationService.cs`, `UnavailableAttendanceInvalidationService.cs`, `DemoCheckInRegistration.cs` |
| M-08 | 4.7 | `d3edd37` | replaces the empty `MyEventsService.GetAttendanceAsync` body; no demo class remained |
| M-09 | 3.12 | `0abc4f8` | removes the demo `CompleteAsync`; the residual window demo went with M-04 in `7f6ae22` |
| M-10a | 6.4 | `46a2100` | removes the demo review creation from `DemoStore.MarkCompleted` |
| M-10b | 6.6 | `d6de0d5` | removes `DemoStore.DecideCertificate` and the demo review state |
| M-11a | 6.7 | `58cffca` | removes `DemoStore.PublishCertificate`, `GetCertificateNumber` |
| M-11b | 6.8 | `124ef29` | deletes `DemoCertificateLedgerService.cs` |
| M-12 | 6.9 | `7637096` | deletes `DemoMyCertificatesService.cs`, `UnavailableMyCertificatesService.cs`, `DemoCertificateDocuments.cs` |
| M-13a | 7.1 | `c0c0935` | removes the demo summary computation |
| M-13b | 7.2 | `975c08b` | removes the demo no-show and breakdown as the authoritative path; its leftover `CourseBreakdown`/`YearBreakdown` fallback was deleted with the file by `04157b3` |
| M-13c | 7.3 | `04157b3` | deletes `DemoParticipationReportService.cs`, `DemoReportsRegistration.cs`, `DemoParticipationReportOverlayTests.cs` |
| M-14a | 5.4 | `8230045` | removes the demo account `DeactivateAsync` |
| M-14b | 5.5 | `f232933` | deletes `DemoAccountAccessCommands.cs` |
| M-15a | 5.6 | `48e0231` | removes the demo membership `DeactivateAsync` |
| M-15b | 5.7 | `1853ad9` | deletes `DemoMembershipAccessCommands.cs` |

No row is open. Two rows needed correction before this verdict could be recorded, and both are
fixed in the register in this pull request: M-01 named `005f899`, which is the PR #209 merge that
still contained all five seam files, not the commit that deleted them; and nine rows carried
`removed` with no commit at all (M-02, M-03, M-05, M-09, M-13a, M-14a, M-14b, M-15a, M-15b).
The M-13b residue is now described in the register's report section.

## 2. Startup check

The published output was produced with `dotnet publish -c Release` and started from its own
directory with the retired switch on, no token key, and a database that is not `AttendTa_Demo`:

```
DemoData__Enabled=true            (no DemoData__TokenKey set)
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=...Database=AttendTa_IT_811verify...
GET http://127.0.0.1:5199/Account/Login -> 200
```

While the M-01 seam existed, the 1.14 startup guard refused exactly this start. The build now
starts and serves normally, which is the failing check the ticket asks for.

The same fact is covered by an integration test that runs in CI,
`CheckInWindowFlagTests.RetiredSeamSwitchKeyIsInertWhenEnabled`. It previously assembled the
section name as `"Demo" + "Data"` so the 8.11 source search could not see it; this pull request
uses the literal and says in the comment that the test is the one place the retired name may
appear, because it is the check.

## 3. Search check

Both searches are the ticket's own: every register id, the demo data folder, and the `DemoData`
configuration keys, over the source and over the published output.

| Scope | Command | Result |
|---|---|---|
| Product source | `rg -n "M-0[1-9]\|M-1[0-5][ab]?" ASI.Basecode.Data ASI.Basecode.Services ASI.Basecode.Resources ASI.Basecode.WebApp` | no hits |
| Product source | `rg -n "DemoData\|DemoStore\|DemoSeedService\|DemoDataRegistration\|demo-pass-token" .` (excluding `_bmad-output`, `bin`, `obj`, `node_modules`) | one hit: `CheckInWindowFlagTests.cs:73`, the inertness test itself |
| Published output | both commands over the publish directory | no hits |
| Folder | `ASI.Basecode.WebApp/DemoData/` | absent |

The source search failed before this pull request. Five files still carried register ids in their
comments — `CheckInService.cs` and `CheckInLookupService.cs` (M-05, M-06), `scanner.js` and
`scanner-camera.js` (M-05), and `passes.css` (M-03). The ids were the register's own deletion
markers, left behind by the tickets that removed the code; the comments read correctly without
them, so only the id was dropped. Nothing else in those files changed.

`ASI.Basecode.WebApp/Readiness/DemoReadiness.cs` still names `AttendTa_Demo`. That is the
`--demo-readiness` host probe, which reports whether a machine is fit to host a demo; it is not
the seam and it holds no demo data, so it is out of scope for the search.

## 4. Real integrations, one database

One disposable LocalDB database (`AttendTa_IT_811verify`) was built from
`database/attendta-mvp-schema.sql`, seeded with `seed-local-admin.sql`, `seed-event-types.sql` and
`seed-demo-scenario.sql`, and then driven over HTTP against the published build. Every flow used
one role's real session and the real service behind it; no SQL state was written by hand to make a
step succeed. Credentials were ephemeral and are not recorded.

| Flow | Observed |
|---|---|
| Approved account request → member | the pending request for the seeded applicant approved through `/AccountRequest/Approve`; a `users` row and a linked `members` row appeared |
| Member registration | the approved applicant registered for the upcoming event through `/MemberEvents/Register`; a pending registration row appeared |
| Officer registration decision | the pending registration approved through `/RegistrationReview/Approve`; status became Confirmed and the real command minted a 64-character `qr_token` |
| Pass | the owner's `/Passes` page rendered the card; `/Passes/Qr/{id}` streamed `image/png`; `?download=true` streamed the same bytes as `attachment; filename=door-pass-15.png`; another member got the identical `403` |
| Check-in window | the officer opened the window on the started event and on the upcoming event; `events.is_check_in_open` became 1 |
| Scanner | a real token scanned as `success` and wrote exactly one `attendance` row; the second scan returned `repeat` with the original timestamp; a token used against an event whose window is closed returned `closed` |
| Manual lookup and check-in | the lookup by name returned the confirmed participant; the explicit check-in wrote one attendance row |
| Invalidation | invalidation with a reason stamped `invalidated_at` and left the registration Confirmed |
| Completion | the started published event completed (`event_status_id=4`); the cancelled event's completion was refused and its status stayed `3` |
| Certificate review | one `pending_approval` row existed per valid attendance; approving wrote `eligible` |
| Certificate issue | issuing wrote `AT-2026-000001` with a retained `certificates/...` path; the officer PDF download returned `application/pdf` (447 KB) |
| Member certificate | the owner's history showed the certificate number and the member PDF download returned `application/pdf` |
| Audit | the same run left 18 `activity_logs` rows covering the whole chain: `account_request.approved`, `member.created`, `user.deactivated`, `user.reactivated`, `event.check_in_opened` (both events), `attendance.recorded` (three), `attendance.invalidated`, `event.completed`, `certificate.approved`, `certificate.issued`, `registration.approved`, `member.membership_deactivated` and `member.membership_reactivated`, each with the acting user and the affected entity id |
| Account access | deactivation set `users.is_active=0`, reactivation restored `1` |
| Membership access | deactivation set `membership_status_id=2`, reactivation restored `1` |
| Reports | `/Reports/Participation?eventId=1` rendered the completed event's report from the real reads |
| Cancelled event | the cancelled event produced no attendance and refused completion |

Two observations are recorded rather than fixed, because neither is a defect in this gate:

- **The scenario seed mints no pass tokens.** `seed-demo-scenario.sql` inserts registrations
  directly, so all eight seeded registrations on the started event have `qr_token = NULL` and
  their passes show the empty state. A token only exists after a registration is confirmed through
  the real approval command, which is what the scanner and pass checks above used. The seed is an
  8.15 rehearsal artefact, not product code, and the demo script already approves a registration
  live; this is a limitation of the seed, not of the integration.
- **Invalidating before completion creates no review.** The completion hook creates a review only
  for valid attendance, so an attendance invalidated before the event completes has no certificate
  row at all rather than a `not_eligible` one. That matches the 6.4 hook's stated rule; the
  "pending or approved review becomes not eligible" case needs a review that already exists.

## 5. What this record does not claim

This is a functional integration gate, not the acceptance run. It does not claim the final test
suite passed, the SPEC §20 checklist passed, a phone scanned a pass over trusted HTTPS, or the
frontend was reviewed. Those are 8.4, 8.6, 8.2/8.14 and 8.12. It also does not fix any behaviour
it observed: the only source edits are the five stale register-id comments and the test literal,
and the only record edits are the register's commit column and the M-13b note.
