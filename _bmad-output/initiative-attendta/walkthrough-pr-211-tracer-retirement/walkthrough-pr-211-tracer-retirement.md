# Review narrative: Retire the development certificate tracer (6.11)

Target: PR #211, head commit `7eb3054` — *refactor(certificates): [BMAD 6.11] retire the development certificate tracer*, on branch `feature/70-certificate-backend-sweep` against `dev` in `rainyDayssss/AttendTa`

Ticket: [story-refactor-sweep.md](../tickets/epic-certificates/tickets/story-refactor-sweep.md) · Plan: [story-refactor-sweep-plan.md](../tickets/epic-certificates/tickets/story-refactor-sweep-plan.md)

Review status: **Review complete — all eight blocks accepted.**

---

## Block 1 — Intent

- [x] Accepted · unchanged during review

**Intent (plan):**

> **Problem:** The development-only certificate tracer still ships in Development — controller, view, service, interface, DI registration and a sample-render path inside the shared renderer — and the certificate module carries dead view-model members plus a doc that still describes the retired demo hand-over. The demo-mock register's certificate rows also name removal commits that do not exist, so 8.11 cannot verify them.
>
> **Approach:** Retire the tracer end to end without touching the retained-PDF pipeline (`RenderSnapshotAsync` → temporary save → promote → `certificates/`), keep every official-template renderer fact in a renamed test class, and close only the small dead-code and record findings this sweep accepts.

Source: the `## Intent` section of [story-refactor-sweep-plan.md](../tickets/epic-certificates/tickets/story-refactor-sweep-plan.md), quoted verbatim from its frozen-after-approval block.

The ticket states the same intent from the delivery side: a finale backend-only cleanup of concrete certificate service, transaction, storage and endpoint findings, including retiring the development tracer without replacing the retained-PDF pipeline. It is tracked as issue #70 and is the last ticket in `epic-certificates`. The frozen boundaries are explicit — no schema, endpoint, authorization, audit, transaction or lock-order change, no user-visible screen change, and no new tests beyond the test-class split.

---

## Block 2 — Broad strokes

- [x] Accepted · after the formal review and the applied patch

Almost all of this PR is subtraction: 1,334 deletions against 993 additions, and most of the additions are the new test file and the plan/elicitation records. The mechanism is that one development-only surface disappears and the official rendering path becomes unconditional. Five entry points tell the story in order:

1. [Startup.DI.cs](../../../ASI.Basecode.WebApp/Startup.DI.cs#L56) — the tracer service registration is gone, and with it the controller, view, service and interface that only it used.
2. [PlaywrightCertificatePdfRenderer.cs](../../../ASI.Basecode.WebApp/Certificates/PlaywrightCertificatePdfRenderer.cs#L59) — `RenderSampleAsync` is gone, and `RenderSnapshotAsync` no longer takes an `isDevelopmentSample` flag, so the signatory and signature checks always run.
3. [CertificateTemplate.cshtml](../../../ASI.Basecode.WebApp/Views/Certificates/CertificateTemplate.cshtml#L313) — the watermark, the red development banner and their inline CSS are gone; the signatory section is now unconditional markup instead of the `else` of a sample check.
4. [CertificatePdfRendererTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CertificatePdfRendererTests.cs#L25) — the renamed test class keeps the six official-template facts and adds one that pins what the retired route does now.
5. [CertificateRepository.cs](../../../ASI.Basecode.Data/Repositories/CertificateRepository.cs#L41) — a no-tracking read for the two GET paths, so a read-only request no longer attaches a certificate row it cannot mutate.

---

## Block 3 — Retiring the tracer end to end

- [x] Accepted · unchanged during review

The tracer was a Development-only Management route that rendered a fixed sample PDF through the shared renderer, pushed it through the real temporary → promote path, streamed it back, and deleted the promoted file when the response stream was disposed. Deleting it means removing the route, the service that drove it, and the one method on the renderer interface that only it called — so `RenderSnapshotAsync` becomes the only way to produce a PDF.

Deleted outright (no link — the files no longer exist):

- `ASI.Basecode.WebApp/Controllers/CertificateTracerController.cs` — carried `[Authorize(Policy = Management)]`, `[Route("CertificateTracer")]` and an `IsDevelopment()` gate in both actions.
- `ASI.Basecode.Services/Services/CertificateTracerService.cs` — 163 lines: render the sample, save to `PrivateFileArea.Temporary`, promote into `Certificates`, wrap the stream in a delete-on-dispose.
- `ASI.Basecode.Services/Interfaces/ICertificateTracerService.cs` and `ASI.Basecode.WebApp/Views/CertificateTracer/Index.cshtml`.

What had to change to let those go:

- [Startup.DI.cs](../../../ASI.Basecode.WebApp/Startup.DI.cs#L56) — the registration is gone, and it was the only one.
- [ICertificatePdfRenderer.cs](../../../ASI.Basecode.Services/Interfaces/ICertificatePdfRenderer.cs#L7) — `RenderSampleAsync` removed, and the interface summary narrowed from "snapshots and the development tracer sample" to just snapshots.
- [PlaywrightCertificatePdfRenderer.cs](../../../ASI.Basecode.WebApp/Certificates/PlaywrightCertificatePdfRenderer.cs#L59) — the sample method and every `isDevelopmentSample` branch removed.
- [CertificateTemplateViewModel.cs](../../../ASI.Basecode.WebApp/Rendering/CertificateTemplateViewModel.cs#L16) — the flag dropped from the record.
- [CertificateIssuanceTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CertificateIssuanceTests.cs#L566) and [CertificateReviewDecisionTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs#L375) — four `RenderSampleAsync` overrides removed so the fakes match the one-method interface.

The tracer's delete-on-dispose is gone, but issuance keeps its own cleanup at [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L249) and [:439](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L439), so no orphaned-file path is left uncovered.

Driven in Development on `http://localhost:5180`: an anonymous `GET /CertificateTracer` returns 302 to `/Account/Login?ReturnUrl=%2FCertificateTracer`; a signed-in request returns 404. The app has no custom 404 page, so the browser shows its own "page not found" screen.

---

## Block 4 — Making the renderer and template unconditional

- [x] Accepted · after the test run and the rendered demo

The renderer used to branch on a flag: the sample path skipped the signatory entirely, and the official path read it. Removing the flag means the signatory read and the signature-image check now run on every render, with no branch to skip them. The template mirrors that: the watermark and red banner were conditional, and the signatory section was the `else` of the same condition. Both conditions are gone, so the signatory section is now plain markup.

The renderer, [PlaywrightCertificatePdfRenderer.cs](../../../ASI.Basecode.WebApp/Certificates/PlaywrightCertificatePdfRenderer.cs#L59):

- [:59](../../../ASI.Basecode.WebApp/Certificates/PlaywrightCertificatePdfRenderer.cs#L59) — `RenderSnapshotAsync` calls `RenderPdfAsync(snapshot, cancellationToken)`; the `isDevelopmentSample` argument is gone from both.
- [:80-83](../../../ASI.Basecode.WebApp/Certificates/PlaywrightCertificatePdfRenderer.cs#L80) — the signatory options are read and the signature PNG is loaded unconditionally. The comment at [:77-79](../../../ASI.Basecode.WebApp/Certificates/PlaywrightCertificatePdfRenderer.cs#L77) is new and worth reading: it says options validation covers name, title and asset path, and that a missing or corrupt file fails in `ReadPngAssetDataUriAsync` instead.
- [:126-130](../../../ASI.Basecode.WebApp/Certificates/PlaywrightCertificatePdfRenderer.cs#L126) — the signature-image page check is no longer inside an `if`.

The template, [CertificateTemplate.cshtml](../../../ASI.Basecode.WebApp/Views/Certificates/CertificateTemplate.cshtml#L313):

- [:313-318](../../../ASI.Basecode.WebApp/Views/Certificates/CertificateTemplate.cshtml#L313) — the signatory section, promoted from the `else` branch to unconditional markup.
- The `.watermark` and `.development-banner` rules and both conditional blocks are deleted; roughly 54 lines out, 5 in.

And [CertificateTemplateViewModel.cs](../../../ASI.Basecode.WebApp/Rendering/CertificateTemplateViewModel.cs#L16) — `IsDevelopmentSample` dropped from the record, so the template can no longer be told it is a sample.

This is the riskiest edit in the PR, and the only real defence is the renderer facts — which is block 5.

---

## Block 5 — Splitting the test class

- [x] Accepted · after the second opinion and the dead-parameter fix

The old `CertificateTracerFlowTests` held eight facts: six that render an official certificate and read the PDF, one that rendered the watermarked sample, one that exercised the tracer route. The rename keeps the six, deletes the two that test deleted code, and adds one fact for the retired route. `CreateFactory` also loses the private-file-store replacement it only needed for the tracer.

The kept class, [CertificatePdfRendererTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CertificatePdfRendererTests.cs#L25):

- [:25](../../../tests/ASI.Basecode.IntegrationTests/CertificatePdfRendererTests.cs#L25) — the class, renamed and now holding exactly seven `[Fact]`s.
- [:28](../../../tests/ASI.Basecode.IntegrationTests/CertificatePdfRendererTests.cs#L28), [:65](../../../tests/ASI.Basecode.IntegrationTests/CertificatePdfRendererTests.cs#L65), [:86](../../../tests/ASI.Basecode.IntegrationTests/CertificatePdfRendererTests.cs#L86), [:105](../../../tests/ASI.Basecode.IntegrationTests/CertificatePdfRendererTests.cs#L105), [:131](../../../tests/ASI.Basecode.IntegrationTests/CertificatePdfRendererTests.cs#L131), [:157](../../../tests/ASI.Basecode.IntegrationTests/CertificatePdfRendererTests.cs#L157) — the six official-template facts, unchanged in substance.
- [:217](../../../tests/ASI.Basecode.IntegrationTests/CertificatePdfRendererTests.cs#L217) — `CreateFactory(string connectionString, string environmentName, Action<CertificateSignatoryOptions>? configureSignatoryOptions = null)`. The `IPrivateFileStore` replacement block is gone, and so is the unused `renderer` parameter it used to take.

The new fact, [:173](../../../tests/ASI.Basecode.IntegrationTests/CertificatePdfRendererTests.cs#L173):

- Seeds a member and an officer through `MemberEventTestSupport`, signs both in, and asserts the split: anonymous `GET` and `POST` get 302 to `/Account/Login`; authenticated member and officer get 404 on both verbs.
- It asserts the redirect through `MemberEventTestSupport.LocationPath` rather than a hand-encoded `ReturnUrl` string — one of the review triage patches.
- The comment at [:191](../../../tests/ASI.Basecode.IntegrationTests/CertificatePdfRendererTests.cs#L191) explains the ordering: the fallback policy runs before a missing endpoint becomes 404.

Deleted: `CertificateTracerFlowTests.cs` (710 lines, including the watermark and tracer facts), `tests/ASI.Basecode.Services.Tests/CertificateTracerServiceTests.cs` (238 lines), and the helpers those facts used — `PostTraceAsync`, `AssertLandscapeA4`, `StorageProbe`, `FailingPdfRenderer`, `TracerLogProbe`, `TrackingPrivateFileStore`. `CertificateNetworkProbe` survives; the markup-as-text fact uses it.

A second opinion enumerated all eight old facts and found no live renderer behaviour orphaned. It did find one dead parameter left behind by the split — `CreateFactory`'s `renderer` argument, whose only caller was the deleted `FailingPdfRenderer`. That is now removed.

---

## Block 6 — Dead ledger surface and the no-tracking read

- [x] Accepted · after the read-path test run

Two independent cleanups on the certificate read side. The ledger's view models carried three members nothing read, and two service methods were public with no caller outside the service; both are now private or internal, so the compiler enforces the boundary instead of a convention. Separately, a no-tracking repository read is added and used by the two GET paths, so a read-only request no longer attaches a certificate row it cannot mutate — while every path that does mutate keeps the tracked read.

The dead surface, [CertificateLedgerViewModels.cs](../../../ASI.Basecode.Services/ServiceModels/CertificateLedgerViewModels.cs#L23):

- [:23](../../../ASI.Basecode.Services/ServiceModels/CertificateLedgerViewModels.cs#L23) — `CertificateLedgerViewModel.Empty()` deleted.
- [:39](../../../ASI.Basecode.Services/ServiceModels/CertificateLedgerViewModels.cs#L39) — `CertificateLedgerRowViewModel.ViewerUrl` deleted.
- [:49](../../../ASI.Basecode.Services/ServiceModels/CertificateLedgerViewModels.cs#L49) — `CertificateViewerViewModel.PdfUrl` deleted. `CertificateViewerViewModel.Empty` stays; it is still used at [CertificateLedgerController.cs](../../../ASI.Basecode.WebApp/Controllers/CertificateLedgerController.cs#L121).
- [:54](../../../ASI.Basecode.Services/ServiceModels/CertificateLedgerViewModels.cs#L54) — `CertificateLedgerRead` is now `internal`, and its summary no longer talks about the demo hand-over.

And [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L76):

- [:76](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L76) — `ReadAsync` private.
- [:114](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L114) — `BuildPage` private.

The new read:

- [ICertificateRepository.cs](../../../ASI.Basecode.Data/Interfaces/ICertificateRepository.cs#L35) and [CertificateRepository.cs](../../../ASI.Basecode.Data/Repositories/CertificateRepository.cs#L41) — `GetByRegistrationIdNoTrackingAsync`, a `SingleOrDefaultAsync` with `AsNoTracking()`.
- [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L498) — used by the shared viewer/PDF read.
- [MyCertificatesService.cs](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L54) — used by the member history loop.

Still tracked, because they mutate: [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L300) (decision), [:371](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L371) (issuance), [CertificateReviewInvalidationHandler.cs](../../../ASI.Basecode.Services/Services/CertificateReviewInvalidationHandler.cs#L34), and [DemoSeedService.cs](../../../ASI.Basecode.WebApp/DemoData/DemoSeedService.cs#L188).

---

## Block 7 — Records, docs and the plan artifacts

- [x] Accepted · after the SHA and doc checks

The register's certificate rows named removal commits that do not exist, so 8.11 could not verify them. Three SHAs are corrected to real ancestors of `dev`. The docs that described the tracer are rewritten or trimmed. And the ticket's own artifacts — plan, handoff, elicitation folder — arrive with the change.

The register, [demo-mock-register.md](../../../_bmad-output/initiative-attendta/demo-mock-register.md#L94):

- [:94](../../../_bmad-output/initiative-attendta/demo-mock-register.md#L94) — M-10a now names `46a2100` instead of a 40-character hash that resolves to nothing.
- [:95](../../../_bmad-output/initiative-attendta/demo-mock-register.md#L95) — M-10b now names `d6de0d5`; it previously had no commit at all.
- [:98](../../../_bmad-output/initiative-attendta/demo-mock-register.md#L98) — M-12 now names `7637096` instead of `d16feed`.

The docs:

- [database-setup.md](../../../docs/database-setup.md#L115) — "Every rendered certificate is an official certificate; there is no sample or watermarked rendering."
- [database-setup.md](../../../docs/database-setup.md#L171) — the promotion description rewritten to describe issuance → promote → `certificates/` → streamed download.
- [demo-quickstart.md](../../../docs/demo-quickstart.md#L124) — the `/CertificateTracer` troubleshooting row deleted.
- [demo-script.md](../../../_bmad-output/initiative-attendta/demo-script.md#L16) — the Production note no longer mentions the tracer.
- [demo-script.md](../../../_bmad-output/initiative-attendta/demo-script.md#L134) — the pre-flight check rewritten, and corrected again in `08cc539` for the "before routing" error.
- [story-refactor-sweep-follow-up.md](../../../_bmad-output/initiative-attendta/handoffs/story-refactor-sweep-follow-up.md#L53) — the coverage bullet now names `CertificatePdfRendererTests`.

Added artifacts: the four files under `_bmad-output/initiative-attendta/elicitation-6-11-refactor-sweep/`, `handoffs/story-certificate-backend-sweep.md`, and `tickets/epic-certificates/tickets/story-refactor-sweep-plan.md` — the last of which also carries the `## Code Review` section from `08cc539`.

Checked: all three corrected SHAs resolve as commits and are ancestors of `upstream/dev`, with the matching ticket subjects (6.4, 6.6, 6.9). The old M-10a hash does not exist at all. The old M-12 hash `d16feed` does exist, but it is a duplicate 6.9 commit that is **not** an ancestor of `upstream/dev` — so 8.11 would have failed on it, and the correction is the right one. No doc still tells an operator to use the tracer.

---

## Block 8 — Periphery

- [x] Accepted · unchanged during review

- [story-refactor-sweep-plan.md](../../../_bmad-output/initiative-attendta/tickets/epic-certificates/tickets/story-refactor-sweep-plan.md#L1) — the plan itself, including the `## Code Review` section appended in `08cc539`.
- [story-certificate-backend-sweep.md](../../../_bmad-output/initiative-attendta/handoffs/story-certificate-backend-sweep.md#L1) — the build handoff, recording the decisions and the verification.
- `_bmad-output/initiative-attendta/elicitation-6-11-refactor-sweep/` — four files: the decision log and three rounds of elicitation, kept as the record of how the scope was narrowed.
- `.github/workflows/ci.yml` — untouched; the Poppler and Chromium steps still gate the renderer facts, which is why they must stay.
- [BMAD_TEAM_TUTORIAL.md](../../../docs/BMAD_TEAM_TUTORIAL.md#L850) — untouched; its 6.1 title still says "issuance tracer", which is accurate history.
- `_bmad-output/initiative-attendta/walkthrough-pr-211-tracer-retirement/` — this review's own artifacts, still untracked.
