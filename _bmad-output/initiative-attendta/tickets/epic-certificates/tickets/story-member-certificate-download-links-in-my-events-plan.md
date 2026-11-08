---
title: 'Member certificate download links in My Events'
type: 'feature'
ticket: '9'
created: '2026-11-08'
status: 'built'
baseline_revision: 'e7578123db5fc604ace31166f59fd5da6c32f6a7'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** R2 is demo-only (M-12). `DemoMyCertificatesService` serves the member Certificates tab and overlays the seeded in-memory document; `UnavailableMyCertificatesService` blanks the tab when the switch is off. With demo data off, a member with a real issued certificate sees no history and no link.

**Approach:** Implement the real owner-scoped history behind the existing `IMyCertificatesService`, reuse 6.8's authorized retained-file retrieval for the PDF, delete the M-12 providers and seeded-document path, and leave the 6.13 host, row model and partial unchanged.

## Boundaries & Constraints

**Always:** Actor identity only from `ICurrentActor`; owner scope from `DiscoverOpenEventIdsForUserAsync` plus `GetMemberRegistrationAsync` with the actor's own id; links only for an issued row with a retained `file_path`; every PDF request rechecks ownership through 6.8; the seeded issued row has no file and must be a controlled not-found; keep the 6.13 row fields, 25-row paging, ordering and partial unchanged; update only M-12's register row/detail and `PRODUCT.md`.

**Never:** SQL/schema changes, a second My Events controller, card model or presentation rewrite, partial/CSS edits, regenerating or replacing a retained PDF, new automated tests, per-ticket critique or polish, or removing `DemoStore`'s pass-token surface.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output | Error Handling |
|---|---|---|---|
| Issued history | Confirmed registration, issued row with retained file | Issued row, View/Download links | No error |
| Unissued review | Pending, eligible or declined row | State plus reason, no links | No error |
| Issued without file | Seeded issued row, no `file_path` | Unavailable, no links | No error |
| No history | No confirmed registrations | Empty page, host empty state | No error |
| Non-member | Officer calls the member route | Refused | 403 from service |
| Other owner | Member requests another member's id | Identical not-found | 404 |
| Unknown id | Any member | Not-found | 404 |
| Switch off | `DemoData:Enabled=false` | Same real history and PDF | No error |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/Services/MyCertificatesService.cs` (new) — real service: owner-scoped page, PDF delegation.
- `ASI.Basecode.Services/Interfaces/IMyCertificatesService.cs` — unchanged contract; `certificateId` is the registration id.
- `ASI.Basecode.Services/Services/CertificateLedgerService.cs:272-285` — reuse 6.8's owner-or-Management `GetPdfAsync`.
- `ASI.Basecode.Data/Interfaces/{IRegistrationRepository,ICertificateRepository,IEventRepository}.cs` — `DiscoverOpenEventIdsForUserAsync`, `GetMemberRegistrationAsync`, `GetByRegistrationIdAsync`, `GetEventAsync`.
- Unchanged: `MyEventsViewModels.cs:230-245`, `MyEventsController.cs:59-72`, `MyCertificatesController.cs:26-58`, `_CertificateLinkPartial.cshtml:57-69`.
- `Startup.DI.cs:60-65` — register the real service; correct the stale comment.
- `DemoData/{DemoMyCertificatesService,UnavailableMyCertificatesService,DemoCertificateDocuments,DemoCertificatesRegistration}.cs` — delete.
- `DemoData/DemoDataRegistration.cs:19-23` — drop `AddCertificates`; keep the store and seed (M-01, 8.13).
- `DemoData/DemoSeedService.cs:21-23,32,36-46,112,267-284` — drop the seeded document and the `DemoStore` dependency.
- `DemoData/DemoStore.cs:13-16,22,61-112` — drop the certificate block; keep the pass token.
- `tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs:215-242` — rewrite; it names the deleted types.
- `demo-mock-register.md:98,136-139,488-527,540-548`; `PRODUCT.md:42` — records.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Services/Services/MyCertificatesService.cs` -- `GetPageAsync` (member guard, owned non-cancelled events, confirmed registration, certificate row, state map, issued-only `HasDocument`, start-descending, 25-row page) and `GetPdfAsync` (member guard, then delegate to `ICertificateLedgerService.GetPdfAsync`) -- the real R2 source.
- [x] `ASI.Basecode.WebApp/Startup.DI.cs` -- register `IMyCertificatesService` to `MyCertificatesService` -- real in both switch states.
- [x] `ASI.Basecode.WebApp/DemoData/{DemoMyCertificatesService,UnavailableMyCertificatesService,DemoCertificateDocuments,DemoCertificatesRegistration}.cs` -- delete -- M-12 removal.
- [x] `ASI.Basecode.WebApp/DemoData/DemoDataRegistration.cs` -- remove the `AddCertificates` call and switch-off sentence -- no certificate demo registration remains.
- [x] `ASI.Basecode.WebApp/DemoData/DemoSeedService.cs` -- delete the M-12 block and the `DemoStore` field/parameter; relabel the summary and log line -- no document is seeded.
- [x] `ASI.Basecode.WebApp/DemoData/DemoStore.cs` -- delete the issue dictionary, four methods and `DemoCertificateIssue` -- only the pass token remains.
- [x] `tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs` -- assert the real ledger and member service resolve -- keeps CI compiling, no new coverage.
- [x] `_bmad-output/initiative-attendta/demo-mock-register.md` -- M-12 `removed (d16feed)`, removal paragraph, store summary and known limits; the follow-up docs commit filled the short sha -- register rule 11.
- [x] `PRODUCT.md` -- replace the in-memory/demo clause with the real owner-scoped retained-file behavior -- product truth.

**Acceptance Criteria:**
- Given a member's confirmed registration whose certificate is issued with a retained file, when they open My Events → Certificates, then the row is Issued with View/Download links to `MyCertificates/Pdf`, and the PDF is 200 `application/pdf`.
- Given a pending, eligible or declined review, when the member opens the tab, then the state and reason show and no link appears.
- Given an issued row with no `file_path`, when the member opens the tab, then the row is Unavailable with no link.
- Given another member's registration id or an unknown id, when the member requests the PDF, then both return the identical 404.
- Given either switch state, when the tab loads, then the same real service serves it and no certificate demo provider is registered.

## Implementation Notes

- `MyCertificatesService` lives in `ASI.Basecode.Services/Services` and takes `IRegistrationRepository`,
  `IEventRepository`, `ICertificateRepository`, `ICertificateLedgerService` and `ICurrentActor`. The
  member guard mirrors `MemberService.GetSelfProfileAsync` (authenticated, `UserId > 0`, role
  `member`) and is applied to both reads; the PDF read then delegates to the ledger, which re-resolves
  the owner and requires an issued row with a retained `certificates/` key, so another member's id and
  an unknown id are the identical not-found.
- The page keeps the 6.13 contract, ordering (start descending, then registration id descending), the
  25-row page and the row fields. It reads each owned event through `GetEventAsync` and skips a
  cancelled event, and it reads the actor's own registration with `GetMemberRegistrationAsync`, so a
  non-confirmed registration contributes no row.
- `HasDocument` is `Issued && !string.IsNullOrWhiteSpace(FilePath)`. An issued row without a file is
  reported as `Unavailable` with no link, which is the seeded row's state now that the seed writes no
  document.
- `Startup.DI.cs` registers `IMyCertificatesService` → `MyCertificatesService` next to the ledger, and
  the stale comment that described a demo replacement is corrected.
- `DemoMyCertificatesService`, `UnavailableMyCertificatesService`, `DemoCertificateDocuments` and
  `DemoCertificatesRegistration` are deleted; `DemoDataRegistration` no longer calls a certificate
  registration file; `DemoSeedService` drops the store field, parameter and seed block; `DemoStore`
  keeps only the pass token.
- `CertificateReviewDecisionTests` now asserts the real ledger and the real member history resolve from
  the host and that no type in the demo namespace implements either certificate contract. No new
  coverage was added, per the plan.
- Matrix coverage decision (2026-11-08, human-approved): the Issued-history row and the shared
  other-owner/unknown-id refusals are covered by `CertificateLedgerFlowTests`; the unissued-review,
  issued-without-file, empty-history, non-member and switch-off rows stay on the plan's manual checks
  and are authored in 8.5 (#78), because the frozen block forbids new automated tests.

## Plan Change Log

## Review Triage Log

Quick lens, run directly on the working-tree diff of the nine changed paths and the four deleted
`DemoData` files (2026-11-08). Build clean; services 310/310; integration 424/424 with
`ATTENDTA_TEST_SQL` set.

| # | Location | Finding | Verdict | Evidence |
|---|---|---|---|---|
| 1 | `tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs:230-235` | The two `DoesNotContain` assertions scan the Services assembly (`typeof(ICertificateLedgerService).Assembly`, `typeof(IMyCertificatesService).Assembly`) but compare each type's namespace to `typeof(DemoDataRegistration).Namespace`, which is `ASI.Basecode.WebApp.DemoData` in the WebApp assembly. No Services type can carry that namespace, so both predicates are always false and the assertions can never fail; the method name also says `InBothSwitchStates` while only one host is built. | `low` — developer-only false confidence; the smallest fix is `typeof(DemoDataRegistration).Assembly`. Recorded, not fixed, per the review instruction. | `DemoDataRegistration` is declared in `ASI.Basecode.WebApp/DemoData/DemoDataRegistration.cs:5`; `ICertificateLedgerService` and `IMyCertificatesService` live in `ASI.Basecode.Services`. The `Assert.IsType` pair above them does assert the real implementations resolve from the host, so the DI hand-over is still covered. |

Behavior note (requested): `ASI.Basecode.Services/Services/MyCertificatesService.cs:43` skips an
event whose `EventStatusId` is `Cancelled`, which the 6.13 provider did not do. **Intended, kept.**
The plan's Code Map and Implementation Notes both record "owned non-cancelled events" / "skips a
cancelled event". It cannot hide an issued certificate: `EventLifecycleService.cs:283-286` refuses
cancellation once the event has an issued certificate, and `CancelAllForEventAsync` cancels every
registration, so the skip is a redundant guard behind the confirmed-registration check at line 49
rather than a behavior change users can reach.

No other findings: the owner scope, state map, issued-only `HasDocument`, ordering, 25-row paging,
member guard and ledger delegation match the frozen matrix; the demo removal leaves `DemoStore`'s
pass-token surface intact and no certificate state behind.

## Design Notes

- The page reads each owned event through `GetEventAsync` instead of loading the whole event table, so it stays bounded by the member's registrations.
- `HasDocument` is `Issued && !string.IsNullOrWhiteSpace(FilePath)`; the PDF route stays the authority and returns the same not-found for a corrupt key.
- Delegating the PDF to 6.8 avoids repeating the prefix, containment and `OpenReadAsync` handling; the member guard is added here.
- Removing the seeded document is intentional: the seed still writes the issued SQL row without a retained file, so officer and member access both become a controlled not-found.

## Verification

**Commands:**
- `dotnet build` -- expected: clean.
- `$env:ATTENDTA_TEST_SQL = "Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` -- expected: green, including `CertificateLedgerFlowTests`.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` -- expected: green.

**Manual checks:**
- With `DemoData__Enabled=true`, issue a certificate, open My Events → Certificates as the owner, confirm the Issued row and a 200 PDF; confirm the seeded row shows no link and a non-owner id returns 404.

## Code Review

### 2026-11-08

**Code review complete.** 2 decision-needed, 3 patch, 1 defer, 46 rejected.

- [x] [Review][Defer] An issued certificate with no retained file renders as "Not issued" with its number hidden [ASI.Basecode.Services/Services/MyCertificatesService.cs:63-67] — deferred: the frozen matrix defines this row as Unavailable with no links, and the copy lives in a partial the ticket forbids touching; revisit in 8.5/8.12.
- [x] [Review][Defer] No test exercises the rendered member certificate surface [tests/] — deferred: the deferral to 8.5 (#78) was approved on 2026-11-08 and the rows were driven manually in the 6.9 walkthrough.
- [x] [Review][Patch] `CertificateId` carries the registration id with nothing at the assignment saying so [ASI.Basecode.Services/Services/MyCertificatesService.cs:71]
- [x] [Review][Patch] The rewritten DI test cannot fail and covers only the switch-off host [tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs:230-235]
- [x] [Review][Patch] The plan's task line still says `removed (pending commit)` while the register records `d16feed` [_bmad-output/initiative-attendta/tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md:71]
- [x] [Review][Defer] `DemoStore.GetPassToken` has no consumer [ASI.Basecode.WebApp/DemoData/DemoStore.cs:53] — deferred: pre-existing, already unreferenced at the plan baseline, and the plan deliberately keeps the pass-token surface for 8.13.

**Rejected**

- R1 — low: N+1 reads are bounded by the member's own history and the plan records the trade; a batched query is a rewrite, not a correction.
- R2 — low: full materialisation before paging shares R1's root cause and reasoning.
- R4 — false: the row model has no field for the discarded columns and the ticket scopes the change to the existing contract.
- R5 — false: the partial gates on non-blank notes; showing an officer's note on a pending row is informative and matches the demo provider.
- R7 — false: the member route is `[Authorize(Policy = Policies.Member)]`, so the ledger's management branch is unreachable from it.
- R8 — low: the guard duplicates a check with no behavioural difference; the divergence is plan wording.
- R9 — low: style only; extracting a helper changes no outcome.
- R10 — low: the catch converts a missing claim into the generic refusal the auth design mandates.
- R11 — false: cancellation is refused when an issued certificate exists and cancels every registration, so the skipped state is unreachable.
- R12 — false: same unreachable state as R11.
- R13 — low: reordering the reads is a micro-optimisation with no result change.
- R14 — false: the fallback is deliberate and matches the ledger's PDF fallback.
- R15 — false: no event deletion path exists.
- R16 — low: cosmetic; the class summary carries the mechanism.
- R17 — false: the lens found no dead usings.
- R18 — low: cosmetic wording.
- R19 — false: the lock guarded the removed dictionary; the remaining surface is a pure static HMAC.
- R20 — false: both values feed the staged issued row and the class summary explains the missing document.
- R21 — false: the sentence is about the document read, which is a not-found on both routes.
- R25 — low: the register records the seeded row's state; adding a clause is cosmetic.
- R26 — false: `status` describes the build, not the review.
- R27 — false: the plan is 149 lines and line 112 is the `## Review Triage Log` heading.
- R29 — false: removed rows name the deleted types by convention (M-11a/M-11b do the same).
- R30 — false: the sentence is accurate where it sits.
- R31 — false: the claim is about another member's record and an unknown id, which is exactly what the ledger returns.
- R32 — false: the controller sets `CertificateHistoryError` and the partial renders an error notice with a retry link.
- R33 — low: at most 25 tracked rows per request; a no-tracking overload is more than a direct correction.
- R34 — false: both states are unreachable or already covered by the default arm.
- R36 — false: same unreachable state as R11.
- R37 — false: same as R15.
- R38 — false: issuance is the only writer and always prefixes `certificates/`.
- R39 — false: duplicate of R1.
- R40 — false: every repository call receives the token, so cancellation surfaces on the next await.
- R41 — false: the intended contract.
- R42 — false: `OnValidatePrincipal` reloads the account on every request and rejects an inactive session.
- R46 — false: duplicate of R7.
- R47 — false: the document is deleted and the row is deliberately kept.
- R48 — false: the lock guarded the removed dictionary only.
- R49 — false: same unreachable state as R11.
- R50 — false: the PR body names the new service, the DI registration and the deletions.
- R52 — false: the issued row needs both values.
- R56 — false: the import is used by the assertion that must be fixed.
- R57 — false: historical record of a past review.
- R58 — false: descriptive observation, no defect claimed.
- R61 — false: duplicate of R11.
- R62 — false: observation about emphasis, not a defect.
