# Elicitation — 6.11 Certificate backend sweep

Target: `_bmad-output/initiative-attendta/tickets/epic-certificates/tickets/story-refactor-sweep-plan.md`
Round 1 — 2026-11-08

## Round 1

Three pushes ran cold against the same version of the plan:

- Push 1 — Problem Decomposition (`round-1-push-1-problem-decomposition.md`)
- Push 2 — Hindsight Reflection (`round-1-push-2-hindsight-reflection.md`)
- Push 3 — Consistency Check (`round-1-push-3-consistency-check.md`)

Where the plan is probably weak: it states counts and line ranges instead of naming the facts it must keep, its verify line does not reach the download path, and its register task points at the wrong sentence.

## Proposed revision — full file

The frozen block is shown unchanged. The two edits it needs are listed under **Frozen-block decisions** and are not applied until the user accepts them.

```markdown
---
title: 'Certificate backend sweep'
type: 'refactor'
ticket: '11'
created: '2026-11-08'
status: 'draft'
baseline_revision: 'a9749c332a4d2c8ab19d0b942054ee1b8d8f9e35'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The development-only certificate tracer still ships in Development — controller, view, service, interface, DI registration and a sample-render path inside the shared renderer — and the certificate module carries dead view-model members plus a doc that still describes the retired demo hand-over. The demo-mock register's certificate rows also name removal commits that do not exist, so 8.11 cannot verify them.

**Approach:** Retire the tracer end to end without touching the retained-PDF pipeline (`RenderSnapshotAsync` → temporary save → promote → `certificates/`), keep every official-template renderer fact in a renamed test class, and close only the small dead-code and record findings this sweep accepts.

## Boundaries & Constraints

**Always:** Keep `RenderSnapshotAsync`, `RenderAndPromoteAsync`, `PrivateFileArea.Temporary`, the store's promotion/containment rules, and every issuance, decision, invalidation and download behavior unchanged. Keep all seven official-template renderer facts after the test split. Record every accepted and rejected finding in the Findings Ledger.

**Never:** Schema, endpoint, authorization, audit, transaction or lock-order changes; any user-visible screen change; rewriting the ledger's event or review queries; deleting the demo seam (`DemoStore`, `DemoSeedService`, `DemoDataRegistration` — that is 8.13); editing historical walkthroughs, findings or past plans; new tests beyond the split.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Tracer route | GET or POST `/CertificateTracer`, any environment | No route exists; 404 | ASP.NET not-found |
| Retained PDF | Issued row with a `certificates/` file | Unchanged 200 `application/pdf`, same bytes | Unchanged 404s |
| Sample render | Any `RenderSampleAsync` caller | Does not compile; no sample path remains | n/a |
| Official template | Snapshot with approved signatory | Unchanged PDF, no development markings | Unchanged failures |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.WebApp/Controllers/CertificateTracerController.cs`, `Views/CertificateTracer/Index.cshtml`, `ASI.Basecode.Services/{Interfaces/ICertificateTracerService,Services/CertificateTracerService}.cs` — delete; `Startup.DI.cs:59` is the only registration.
- `ASI.Basecode.Services/Interfaces/ICertificatePdfRenderer.cs:10` `RenderSampleAsync` — tracer-only; `:13` `RenderSnapshotAsync` is the real issuance path and stays.
- `ASI.Basecode.WebApp/Certificates/PlaywrightCertificatePdfRenderer.cs:59-67,74,90-98,112,142-150` — sample method and its `isDevelopmentSample` branches.
- `ASI.Basecode.WebApp/Rendering/CertificateTemplateViewModel.cs:16` `IsDevelopmentSample`; `Views/Certificates/CertificateTemplate.cshtml:283-297,299-313,319-322,350-361` — delete the development watermark/banner markup and its inline rules, delete the `if` branch that tests `IsDevelopmentSample` at `:350-353`, and promote the `else` body at `:354-361` (the signatory section) to unconditional markup. The six kept renderer facts are the check on this edit.
- `tests/ASI.Basecode.IntegrationTests/CertificateTracerFlowTests.cs` — the class holds eight facts. Keep exactly these six official-renderer facts: `RendererCreatesOfficialA4LandscapePageWithApprovedSignatoryAndNoDevelopmentMarkings`, `OfficialRendererRejectsUnapprovedSignatoryConfiguration`, `OfficialRendererFailsWhenPackagedSignatureAssetIsMissing`, `RendererDisplaysMarkupAsTextAndBlocksNetworkRequests`, `RendererKeepsMaximumLengthSnapshotValuesOnOnePage`, `RendererRejectsLocalKindSnapshotTimestamps`. Delete `RendererKeepsDevelopmentTracerSampleWatermarked` (it calls `RenderSampleAsync` and asserts the watermark) and `TracerRequiresConfirmedRegistrationAndManagementInDevelopmentAndDeletesStreamedPdf`. Delete `PostTraceAsync`, `AssertLandscapeA4`, `StorageProbe`, `FailingPdfRenderer`, `TracerLogProbe` and `TrackingPrivateFileStore`; `CertificateNetworkProbe` stays. `CreateFactory` becomes `(string connectionString, string environmentName, ICertificatePdfRenderer? renderer = null, Action<CertificateSignatoryOptions>? configureSignatoryOptions = null)` and loses the `IPrivateFileStore` replacement block.
- `tests/ASI.Basecode.Services.Tests/CertificateTracerServiceTests.cs` — delete.
- Fakes implementing `RenderSampleAsync`: `CertificateIssuanceTests.cs:569-570,580-581,589-590`, `CertificateReviewDecisionTests.cs:378` — four overrides across two files.
- `ASI.Basecode.Services/ServiceModels/CertificateLedgerViewModels.cs:24,41,52,60-66` — dead `CertificateLedgerViewModel.Empty()`, `CertificateLedgerRowViewModel.ViewerUrl`, `CertificateViewerViewModel.PdfUrl`; stale demo doc. `CertificateViewerViewModel.Empty` is used at `CertificateLedgerController.cs:121` — keep.
- `ASI.Basecode.Services/Services/CertificateLedgerService.cs:67-68,76,114` — `ReadAsync`/`BuildPage` have no external caller.
- `ASI.Basecode.Data/Interfaces/ICertificateRepository.cs:28`, `Repositories/CertificateRepository.cs` — tracked read used by read-only paths at `CertificateLedgerService.cs:498` and `MyCertificatesService.cs:54`.
- `_bmad-output/initiative-attendta/demo-mock-register.md:94,95,98` — M-10a/M-10b/M-12 removal commits, and the M-12 row's stale `DemoCertificateDocuments` clause at `:98`. The summary sentence at `:495` is correct and stays.
- `docs/database-setup.md:115-116,170-172`, `docs/demo-quickstart.md:124`, `_bmad-output/initiative-attendta/demo-script.md:134-138`, `handoffs/story-refactor-sweep-follow-up.md:53` — forward-looking tracer text; in the handoff, line 53 only.

## Tasks & Acceptance

**Execution:**
- [ ] `demo-mock-register.md:94,95,98` — set M-10a to `46a2100`, M-10b to `d6de0d5`, M-12 to `7637096`, seven-character form for all three; correct the M-12 row's stale `DemoCertificateDocuments` clause at `:98`; leave the summary sentence at `:495` unchanged. Before editing, confirm each SHA with `git merge-base --is-ancestor <sha> dev`.
- [ ] Delete the four tracer source files and the `Startup.DI.cs:59` registration — the tracer must not exist in any environment.
- [ ] `ICertificatePdfRenderer.cs`, `PlaywrightCertificatePdfRenderer.cs`, `CertificateTemplateViewModel.cs`, `CertificateTemplate.cshtml` — remove `RenderSampleAsync`, `isDevelopmentSample`, the watermark/banner markup and their inline rules, delete the `if` branch that tests `IsDevelopmentSample`, and make the signatory section unconditional; leave `RenderSnapshotAsync` untouched — retire the sample path without replacing the retained-PDF pipeline.
- [ ] `CertificateIssuanceTests.cs`, `CertificateReviewDecisionTests.cs` — remove the four `RenderSampleAsync` overrides — keep the fakes compiling.
- [ ] `tests/.../CertificateTracerFlowTests.cs` → `CertificatePdfRendererTests.cs` — keep the six named official-renderer facts, delete `RendererKeepsDevelopmentTracerSampleWatermarked` and `TracerRequiresConfirmedRegistrationAndManagementInDevelopmentAndDeletesStreamedPdf` with their helpers, and reduce `CreateFactory` to `(connectionString, environmentName, renderer, configureSignatoryOptions)` — preserve official-template coverage.
- [ ] `CertificateLedgerViewModels.cs`, `CertificateLedgerService.cs` — delete `CertificateLedgerViewModel.Empty()`, `CertificateLedgerRowViewModel.ViewerUrl`, `CertificateViewerViewModel.PdfUrl` (keep `CertificateViewerViewModel.Empty`, used at `CertificateLedgerController.cs:121`); make `ReadAsync`/`BuildPage` private; rewrite the `CertificateLedgerRead` doc without the demo hand-over — remove dead surface.
- [ ] `ICertificateRepository.cs`, `CertificateRepository.cs`, `CertificateLedgerService.cs`, `MyCertificatesService.cs` — add a no-tracking read and use it on the two document/history paths; keep the tracked read for the decision and issuance transactions — stop tracking rows a GET never mutates.
- [ ] `docs/database-setup.md`, `docs/demo-quickstart.md`, `demo-script.md` — remove the tracer instructions and rename the retained coverage file. `handoffs/story-refactor-sweep-follow-up.md:53` — update the one coverage-file bullet only; do not edit the rest of the past plan.

**Acceptance Criteria:**
- Given the solution, when built, then no `CertificateTracer*`, `RenderSampleAsync` or `IsDevelopmentSample` symbol remains.
- Given a Development host, when an authenticated Management request asks for `/CertificateTracer`, then the response is 404; anonymous requests keep the sign-in redirect and non-Management requests keep the `/Account/AccessDenied` redirect.
- Given an issued certificate, when the owner or Management downloads it, then the bytes equal the retained file and the non-owner/unknown refusals are unchanged.
- Given the integration suite, when the renamed renderer class runs, then exactly the six named official-template facts pass and neither the tracer fact nor the development-watermark fact exists.
- Given `dev`, when each certificate register row's removal commit is checked, then it resolves and is an ancestor of `dev`.

## Findings Ledger

Accepted: tracer still shipping; the development-watermark fact still asserting a removed path; dead `Empty()`/`ViewerUrl`/`PdfUrl`; public-only-internal `ReadAsync`/`BuildPage`; tracked reads on GET paths; one wrong register commit, one missing, one wrong.

Rejected, with evidence:
- Issuance does not recheck confirmed registration under the locks — unreachable: review cancellation is refused with valid attendance (`RegistrationReviewService.cs:115`), event cancellation is refused with valid attendance or an issued certificate (`EventLifecycleService.cs:278-286`), and only draft/published events can be cancelled (`:251`).
- Invalidation leaves `ReviewedBy`/`ReviewedAt` on the approver — intended; 6.5's approved patch asserts the original reviewer survives the flip.
- Two `SaveChangesAsync` per transaction — the module's house idiom (mutate, save, append audit, save).
- `CreateTemporaryPath` on `IPrivateFileStore` — used by `PrivateFileStore.SaveAsync` and `PrivateFileStoreTests`.
- Ledger selector reads all events; `FindReviewAsync` reads the whole queue; `MyCertificatesService` N+1 — performance rewrites needing new repository methods and five `IEventRepository` fakes; 6.9's review already rejected the analogous batching as "a rewrite, not a correction".
- Duplicated actor guard — extraction adds a shared type for two small methods; 6.7 rejected the same.
- Missing recipient/issuer maps to "review not found" — unreachable behind the user FK; changing the outcome alters the endpoint contract.

## Design Notes

The risky part is the template edit, not the deletion: `CertificateTemplate.cshtml` is the only real-Chromium-rendered artifact, and a markup slip is caught only by the renderer facts. The rename must keep the six official facts intact, three of which read the PDF through Poppler. `ci.yml`'s Poppler step and `docs/database-setup.md`'s Poppler prerequisite stay.

## Verification

**Preconditions:** Chromium for Playwright is installed and Poppler's `pdftotext`, `pdfinfo` and `pdffonts` resolve on `PATH`; without them the renderer facts cannot run.

**Commands:**
- `dotnet build` — expected: clean. The solution includes the integration test project.
- `$env:ATTENDTA_TEST_SQL = "Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet run --project tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --no-build -- -class ASI.Basecode.IntegrationTests.CertificatePdfRendererTests -parallelMode none` — expected: 6 pass (needs Chromium and Poppler).
- Same command with `-class ASI.Basecode.IntegrationTests.CertificateIssuanceTests`, `CertificateReviewDecisionTests` and `CertificateLedgerFlowTests` — expected: all pass. `CertificateLedgerFlowTests.OfficerCanReviewIssueAndDownloadTheRetainedCertificateWithAnAuditRow` discharges the download-bytes and refusal acceptance criterion.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — expected: green.
- `git grep -n "CertificateTracer\|RenderSampleAsync\|IsDevelopmentSample"` — expected: no hits.
- `git grep -n "CertificateLedgerViewModel.Empty\|\.ViewerUrl\|\.PdfUrl"` — expected: no hits.
- `git grep -n "<new no-tracking method name>"` — expected: the repository definition and exactly the two intended call sites.
```

## Frozen-block decisions

These two edits touch `<frozen-after-approval>` and are not applied without the user's renegotiation.

1. **"Keep all seven official-template renderer facts after the test split."** The class holds eight facts: six official-renderer facts, one development-watermark fact (`RendererKeepsDevelopmentTracerSampleWatermarked`) and one tracer-flow fact. The watermark fact calls `RenderSampleAsync`, so it cannot survive the sample-path removal. Recommend "six" and the deletion of the watermark fact. Without this, the frozen intent and the task list cannot both be satisfied.
2. **I/O matrix row "GET or POST `/CertificateTracer`, any environment → 404".** Anonymous requests redirect to sign-in and non-Management requests redirect to `/Account/AccessDenied`; only an authenticated Management request gets 404. Recommend leaving the row as the authenticated-case statement and keeping the qualified acceptance criterion above, or renegotiating the row to name the redirects.

## Set aside

- **Push 2, finding 1 — `-class` is not a supported xUnit v3 switch.** Set aside: this project uses `xunit.v3.mtp-v2` 4.0.2 and its In-Process Runner documents `-class "name"` as a simple filter (`--help`), so the plan's command shape is valid.
- **Push 2, finding 2 — `--no-build` after a rename.** Set aside: `dotnet build` at the repository root resolves `ASI.Basecode.sln`, which includes `ASI.Basecode.IntegrationTests`, so the test assembly is rebuilt before the `--no-build` run.
- **Push 3, finding 1 and Push 1's "seven facts" — the count correction.** The underlying issue is kept, but both figures are wrong: the class has eight facts, six are official, and three of the six read the PDF through Poppler. The revision uses the verified inventory.
- **Push 1, finding 5 — restate the `Empty()` carve-out.** Kept as a one-line clarification inside task 6 rather than a separate change.
- **Push 2, finding 5 and Push 3, finding 5 — the register sentence.** Merged into the register task change; the target is the M-12 row at `:98`, not the summary sentence at `:495`.

## Decision log

2026-11-08 — user chose **apply all**. Applied to `story-refactor-sweep-plan.md`:

1. Applied — test inventory: six named official facts; development-watermark and tracer-flow facts deleted; "7 pass" → "6 pass".
2. Applied — template edit: `if` branch at `:350-353` deleted, `else` body promoted, six facts named as the check.
3. Applied — `CreateFactory` surviving signature stated; `IPrivateFileStore` replacement block removed.
4. Applied — register task: one wrong, one missing, one wrong; M-12 row clause at `:98`; seven-character shas; summary at `:495` left alone.
5. Applied — register task moved first with `git merge-base --is-ancestor` pre-check.
6. Applied — acceptance qualified to authenticated Management; anonymous and non-Management redirects stated.
7. Applied — `CertificateLedgerFlowTests.OfficerCanReviewIssueAndDownloadTheRetainedCertificateWithAnAuditRow` named for the download criterion.
8. Applied — Chromium and Poppler preconditions added.
9. Applied — negative greps added for the dead members and the no-tracking call sites.
10. Applied — handoff edit narrowed to line 53.
11. Applied — Design Notes corrected to "three of the six" Poppler facts.
12. Applied — `CertificateViewerViewModel.Empty` carve-out restated in task 6.

Frozen-block decisions:

1. Applied — "Keep all seven official-template renderer facts" → "six", with the development-watermark fact deleted.
2. Applied as recommended — the I/O matrix row is unchanged; the qualified acceptance criterion carries the redirect behavior.

Rejected: none.
