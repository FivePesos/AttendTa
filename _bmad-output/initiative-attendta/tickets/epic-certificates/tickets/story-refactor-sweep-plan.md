---
title: 'Certificate backend sweep'
type: 'refactor'
ticket: '11'
created: '2026-11-08'
status: 'built'
baseline_revision: 'a9749c332a4d2c8ab19d0b942054ee1b8d8f9e35'
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

**Problem:** The development-only certificate tracer still ships in Development — controller, view, service, interface, DI registration and a sample-render path inside the shared renderer — and the certificate module carries dead view-model members plus a doc that still describes the retired demo hand-over. The demo-mock register's certificate rows also name removal commits that do not exist, so 8.11 cannot verify them.

**Approach:** Retire the tracer end to end without touching the retained-PDF pipeline (`RenderSnapshotAsync` → temporary save → promote → `certificates/`), keep every official-template renderer fact in a renamed test class, and close only the small dead-code and record findings this sweep accepts.

## Boundaries & Constraints

**Always:** Keep `RenderSnapshotAsync`, `RenderAndPromoteAsync`, `PrivateFileArea.Temporary`, the store's promotion/containment rules, and every issuance, decision, invalidation and download behavior unchanged. Keep all six official-template renderer facts after the test split. Record every accepted and rejected finding in the Findings Ledger.

**Never:** Schema, endpoint, authorization, audit, transaction or lock-order changes; any user-visible screen change; rewriting the ledger's event or review queries; deleting the demo seam (`DemoStore`, `DemoSeedService`, `DemoDataRegistration` — that is 8.13); editing historical walkthroughs, findings or past plans; new tests beyond the split.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Tracer route | GET or POST `/CertificateTracer`, any environment | No route exists. An anonymous caller is challenged by the app-wide fallback policy and redirected to `/Account/Login`; an authenticated member or Management caller gets 404 | Fallback-policy challenge, then ASP.NET not-found |
| Retained PDF | Issued row with a `certificates/` file | Unchanged 200 `application/pdf`, same bytes | Unchanged 404s |
| Official template | Snapshot with approved signatory | Unchanged PDF, no development markings | Unchanged failures |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Controllers/CertificateTracerController.cs`, `Views/CertificateTracer/Index.cshtml`, `ASI.Basecode.Services/{Interfaces/ICertificateTracerService,Services/CertificateTracerService}.cs` — delete; `Startup.DI.cs:59` is the only registration.
- `ASI.Basecode.Services/Interfaces/ICertificatePdfRenderer.cs:10` `RenderSampleAsync` — tracer-only; `:13` `RenderSnapshotAsync` is the real issuance path and stays.
- `ASI.Basecode.WebApp/Certificates/PlaywrightCertificatePdfRenderer.cs:59-67,74,90-98,112,142-150` — sample method and its `isDevelopmentSample` branches.
- `ASI.Basecode.WebApp/Rendering/CertificateTemplateViewModel.cs:16` `IsDevelopmentSample`; `Views/Certificates/CertificateTemplate.cshtml:283-297,299-313,319-322,350-361` — delete the development watermark/banner markup and its inline rules, delete the `if` branch that tests `IsDevelopmentSample` at `:350-353`, and promote the `else` body at `:354-361` (the signatory section) to unconditional markup. The six kept renderer facts are the check on this edit.
- `tests/ASI.Basecode.IntegrationTests/CertificateTracerFlowTests.cs` — the class holds eight facts. Keep exactly these six official-renderer facts: `RendererCreatesOfficialA4LandscapePageWithApprovedSignatoryAndNoDevelopmentMarkings`, `OfficialRendererRejectsUnapprovedSignatoryConfiguration`, `OfficialRendererFailsWhenPackagedSignatureAssetIsMissing`, `RendererDisplaysMarkupAsTextAndBlocksNetworkRequests`, `RendererKeepsMaximumLengthSnapshotValuesOnOnePage`, `RendererRejectsLocalKindSnapshotTimestamps`. Delete `RendererKeepsDevelopmentTracerSampleWatermarked` (it calls `RenderSampleAsync` and asserts the watermark) and `TracerRequiresConfirmedRegistrationAndManagementInDevelopmentAndDeletesStreamedPdf`. Delete `PostTraceAsync`, `AssertLandscapeA4`, `StorageProbe`, `FailingPdfRenderer`, `TracerLogProbe` and `TrackingPrivateFileStore`; `CertificateNetworkProbe` stays. `CreateFactory` becomes `(string connectionString, string environmentName, ICertificatePdfRenderer? renderer = null, Action<CertificateSignatoryOptions>? configureSignatoryOptions = null)` and loses the `IPrivateFileStore` replacement block. Add `RetiredTracerRouteHasNoEndpointForAnonymousMemberAndManagementCallers` so the matrix's tracer row stays covered: anonymous GET and POST are challenged to `/Account/Login`, authenticated member and Management get 404.
- `tests/ASI.Basecode.Services.Tests/CertificateTracerServiceTests.cs` — delete.
- Fakes implementing `RenderSampleAsync`: `CertificateIssuanceTests.cs:569-570,580-581,589-590`, `CertificateReviewDecisionTests.cs:378` — four overrides across two files.
- `ASI.Basecode.Services/ServiceModels/CertificateLedgerViewModels.cs:24,41,52,60-66` — dead `CertificateLedgerViewModel.Empty()`, `CertificateLedgerRowViewModel.ViewerUrl`, `CertificateViewerViewModel.PdfUrl`; stale demo doc. `CertificateViewerViewModel.Empty` is used at `CertificateLedgerController.cs:121` — keep.
- `ASI.Basecode.Services/Services/CertificateLedgerService.cs:67-68,76,114` — `ReadAsync`/`BuildPage` have no external caller.
- `ASI.Basecode.Data/Interfaces/ICertificateRepository.cs:28`, `Repositories/CertificateRepository.cs` — tracked read used by read-only paths at `CertificateLedgerService.cs:498` and `MyCertificatesService.cs:54`.
- `_bmad-output/initiative-attendta/demo-mock-register.md:94,95,98` — M-10a/M-10b/M-12 removal commits only. The M-12 row's `DemoCertificateDocuments` clause and the summary sentence at `:495` are accurate history and stay.
- `docs/database-setup.md:115-116,170-172`, `docs/demo-quickstart.md:124`, `_bmad-output/initiative-attendta/demo-script.md:134-138`, `handoffs/story-refactor-sweep-follow-up.md:53` — forward-looking tracer text.

## Tasks & Acceptance

**Execution:**
- [ ] `demo-mock-register.md:94,95,98` — set M-10a to `46a2100`, M-10b to `d6de0d5`, M-12 to `7637096`, seven-character form for all three; change nothing else on those rows. Before editing, confirm each SHA with `git merge-base --is-ancestor <sha> dev`.
- [ ] Delete the four tracer source files and the `Startup.DI.cs:59` registration — the tracer must not exist in any environment.
- [ ] `ICertificatePdfRenderer.cs`, `PlaywrightCertificatePdfRenderer.cs`, `CertificateTemplateViewModel.cs`, `CertificateTemplate.cshtml` — remove `RenderSampleAsync`, `isDevelopmentSample`, the watermark/banner markup and their inline rules, delete the `if` branch that tests `IsDevelopmentSample`, and make the signatory section unconditional; leave `RenderSnapshotAsync` untouched — retire the sample path without replacing the retained-PDF pipeline.
- [ ] `CertificateIssuanceTests.cs`, `CertificateReviewDecisionTests.cs` — remove the four `RenderSampleAsync` overrides — keep the fakes compiling.
- [ ] `tests/.../CertificateTracerFlowTests.cs` → `CertificatePdfRendererTests.cs` — keep the six named official-renderer facts, delete `RendererKeepsDevelopmentTracerSampleWatermarked` and `TracerRequiresConfirmedRegistrationAndManagementInDevelopmentAndDeletesStreamedPdf` with their helpers, and reduce `CreateFactory` to `(connectionString, environmentName, renderer, configureSignatoryOptions)` — preserve official-template coverage.
- [ ] `CertificateLedgerViewModels.cs`, `CertificateLedgerService.cs` — delete `CertificateLedgerViewModel.Empty()`, `CertificateLedgerRowViewModel.ViewerUrl`, `CertificateViewerViewModel.PdfUrl` (keep `CertificateViewerViewModel.Empty`, used at `CertificateLedgerController.cs:121`); make `ReadAsync`/`BuildPage` private; rewrite the `CertificateLedgerRead` doc without the demo hand-over — remove dead surface.
- [ ] `ICertificateRepository.cs`, `CertificateRepository.cs`, `CertificateLedgerService.cs`, `MyCertificatesService.cs` — add `GetByRegistrationIdNoTrackingAsync` and use it on the two document/history paths; keep the tracked read for the decision and issuance transactions — stop tracking rows a GET never mutates.
- [ ] `docs/database-setup.md`, `docs/demo-quickstart.md`, `demo-script.md` — remove the tracer instructions and rename the retained coverage file. `handoffs/story-refactor-sweep-follow-up.md:53` — update that one coverage bullet only.

**Acceptance Criteria:**
- Given the solution, when built, then no `CertificateTracer*`, `RenderSampleAsync` or `IsDevelopmentSample` symbol remains.
- Given any host, when a caller requests `/CertificateTracer`, then no route matches: an anonymous caller is redirected to `/Account/Login` by the fallback policy, and an authenticated member or Management caller gets 404.
- Given an issued certificate, when the owner or Management downloads it, then the bytes equal the retained file and the non-owner/unknown refusals are unchanged.
- Given the integration suite, when the renamed renderer class runs, then exactly the six named official-template facts pass and neither the tracer fact nor the development-watermark fact exists.
- Given `dev`, when each certificate register row's removal commit is checked, then it resolves and is an ancestor of `dev`.

## Findings Ledger

Accepted: tracer still shipping; the watermark fact asserting a removed path; dead `Empty()`/`ViewerUrl`/`PdfUrl`; public-only-internal `ReadAsync`/`BuildPage`; tracked GET reads; three bad register commits.

Rejected:
- Issuance skips the confirmed-registration recheck — unreachable: cancellation is refused with valid attendance (`RegistrationReviewService.cs:115`), as is event cancellation (`EventLifecycleService.cs:278-286`), and only draft/published events cancel (`:251`).
- Invalidation leaves `ReviewedBy` on the approver — intended; 6.5's patch asserts the reviewer survives.
- Two `SaveChangesAsync` — the module's house idiom.
- `CreateTemporaryPath` — used by `PrivateFileStore.SaveAsync` and its tests.
- Ledger selector, `FindReviewAsync` queue read, member N+1 — rewrites needing new repository methods and five `IEventRepository` fakes; 6.9 rejected the analogous batching.
- Duplicated actor guard — extraction adds a type for two small methods.
- Missing recipient/issuer message — unreachable behind the FK.

## Design Notes

The risky edit is the template, not the deletion: `CertificateTemplate.cshtml` is the only real-Chromium artifact, so a markup slip is caught only by the renderer facts. Three of the six read the PDF through Poppler, so `ci.yml`'s Poppler step and `docs/database-setup.md`'s prerequisite stay.

## Verification

**Preconditions:** Chromium for Playwright is installed and Poppler's `pdftotext`, `pdfinfo` and `pdffonts` resolve on `PATH`; without them the renderer facts cannot run.

**Commands:**
- `dotnet build` — expected: clean. The solution includes the integration test project.
- `$env:ATTENDTA_TEST_SQL = "Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet run --project tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --no-build -- -class ASI.Basecode.IntegrationTests.CertificatePdfRendererTests -parallelMode none` — expected: 7 pass — the six official-template facts plus `RetiredTracerRouteHasNoEndpointForAnonymousMemberAndManagementCallers`.
- Same command with `-class ASI.Basecode.IntegrationTests.CertificateIssuanceTests`, `CertificateReviewDecisionTests` and `CertificateLedgerFlowTests` — expected: all pass. `CertificateLedgerFlowTests.OfficerCanReviewIssueAndDownloadTheRetainedCertificateWithAnAuditRow` discharges the download-bytes and refusal acceptance criterion.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — expected: green.
- `git grep -n "CertificateTracerController\|ICertificateTracerService\|CertificateTracerService\|RenderSampleAsync\|IsDevelopmentSample" -- "ASI.Basecode.*" tests` — expected: no hits. The retired route path survives only as a URL literal in `CertificatePdfRendererTests`, which is what proves no endpoint answers it.
- `git grep -n "CertificateLedgerViewModel.Empty\|\.ViewerUrl\|\.PdfUrl" -- "ASI.Basecode.*" tests` — expected: no hits.
- `git grep -n "GetByRegistrationIdNoTrackingAsync" -- "ASI.Basecode.*" tests` — expected: the interface, the repository implementation and exactly the two intended call sites.

## Review Triage Log

Quick lens, 2026-11-08. Every reported item is listed, including the ones the reviewer recorded as clean.

| # | Finding | Verdict | Route | Evidence |
|---|---|---|---|---|
| 1 | `demo-script.md:136` says both requests "must return 404 from every caller" | low | patch | Measured: anonymous GET and POST are challenged to `/Account/Login`; only authenticated callers get 404. The step signs in as Administrator first, so the operator's check passes, but the sentence is false as written. |
| 2 | `docs/demo-quickstart.md` deletes the tracer troubleshooting row instead of rewriting it | false | — | The plan's task says "remove the tracer instructions"; the row was tracer guidance for a route that no longer exists. Deleting an obsolete row is the task, and no operator can hit the retired route. |
| 3 | Register hashes are ancestors of local `dev` but unverifiable on `upstream/dev` | false | — | `upstream/dev` resolves to `a9749c3`, and `git merge-base --is-ancestor` returns 0 for `46a2100`, `d6de0d5` and `7637096` against `upstream/dev` itself, not just the local branch. |
| 4 | The route fact exercises only `Development` while the matrix says "any environment" | low | rejected | No current defect: no environment has the route. The fix adds an environment parameter and a second host launch for a regression that would be caught by the build. Unlikely to be met, and more than a direct correction. |
| 5 | `CertificateLedgerRead` stays public with no producer or consumer outside the service | low | patch | `git grep CertificateLedgerRead` returns only its declaration and three references inside `CertificateLedgerService`. The plan's accepted finding was public-only-internal `ReadAsync`/`BuildPage`; the record they carry has the same problem. |
| 6 | The new renderer comment claims IOptions validation covers a missing signatory configuration | low | patch | `Startup.DI.cs:97-107` validates only `Name`, `Title` and `PackagedSignatureAsset` against the approved constants. A missing or corrupt file fails in `ReadPngAssetDataUriAsync`, which is why `OfficialRendererFailsWhenPackagedSignatureAssetIsMissing` exists. |
| 7 | `database-setup.md:170-172` drops the temporary area from the promotion description | false | — | The claim that the old text named the tracer's temporary file is wrong: it said the sample was written to private `certificates/` storage. `docs/database-setup.md` never mentions `temporary` anywhere, before or after. |
| 8 | The handoff says the work is done while the plan is `in-review` with unchecked tasks | false | — | The handoff says "implemented and verified but not committed", which is accurate for this stage. The unchecked boxes and the ledger's "three bad register commits" wording are plan bookkeeping; the fix edits this build's plan, which triage rejects. |
| 9 | The verification grep cannot pass as written | false | — | The reviewer ran it and got exit 1 with no hits, matching the plan. |
| 10 | The push-2 elicitation file states "seven template facts" and "five Poppler facts" | false | — | That file is a record of what a cold review believed before the inventory was corrected; the decision log in the same folder records the correction, and the intent forbids editing findings. It is not a claim about the shipped code. |
| 11 | The no-tracking read serves the viewer and download paths | false | — | Both paths are GETs that never mutate, which is the plan's rule. The decision and issuance reads at `:300` and `:371` stay tracked, exactly as the Code Map requires. |
| 12 | The anonymous assertions hard-code the encoded `ReturnUrl` query form | low | patch | The assertions pass, but they couple to framework encoding where the repo's other tests assert through `MemberEventTestSupport.LocationPath`. A direct correction to the existing helper. |
| 13 | The promoted signatory section's indentation and closing markup | false | — | The reviewer checked the resulting file and found the section renders at the surrounding indentation with the page closing correctly; the six official facts cover it. |
| 14 | The three issuance fakes compile with only `RenderSnapshotAsync` | false | — | The reviewer confirmed the sibling-class call to `PdfBytes()` resolves and `UnusedPdfRenderer` keeps compiling. |
| 15 | The follow-up handoff's line-53 rename | false | — | Only that bullet changed, which is the one authorized exception to the no-past-plan-edits rule. |
