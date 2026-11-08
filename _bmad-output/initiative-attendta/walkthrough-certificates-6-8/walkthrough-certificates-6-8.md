# Review narrative: Authorized certificate streaming download endpoint (6.8)

Target: commits `124ef29`, `f5ba1b9`, `cda64b2`, `c228863` — *feat(certificates): [BMAD 6.8] stream retained certificates to owners and management*, *docs(bmad): [BMAD 6.8] record the M-11b removal commit*, *test(certificates): [BMAD 6.8] cover the retained-key refusal and align the review notes*, *docs(bmad): [BMAD 6.8] state the demo-only gap in the story intent*

Ticket: [story-authorized-certificate-streaming-download-endpoint.md](../tickets/epic-certificates/tickets/story-authorized-certificate-streaming-download-endpoint.md) · Plan: [story-authorized-certificate-streaming-download-endpoint-plan.md](../tickets/epic-certificates/tickets/story-authorized-certificate-streaming-download-endpoint-plan.md)

Review status: **Review complete — all six blocks accepted.** Six commits pushed to `feature/68-authorized-certificate-download-endpoint`; PR #207 now carries the review's four commits (`cda64b2`, `c228863`, `e206178`, `46a0de1`) and CI is running on the new head.

---

## Block 1 — Intent

- [x] Accepted · intent reworded during review to state the demo-only gap

**Intent (ticket):** "Delivers R6 through a GET action that takes a certificate id and never a path, admits the owner (matching ActorUserId) or any caller with the Management policy, lets inactive memberships through, relies on the AD-8 session check that refuses inactive Accounts, and streams the file_path read through IPrivateFileStore.OpenReadAsync after a containment check as application/pdf."

Source: the Description of [story-authorized-certificate-streaming-download-endpoint.md](../tickets/epic-certificates/tickets/story-authorized-certificate-streaming-download-endpoint.md#L17), which carries the story's `covers: [R6]` and tracker issue #68.

The plan states the same intent from the other side: R6 is demo-only today (M-11b), because the real `CertificateLedgerService` refuses every viewer and PDF request while `DemoCertificateLedgerService` serves the officer from an in-memory document. With demo data off, an issued certificate cannot be viewed or downloaded at all. So this is not a new screen or a new contract — it is the real implementation behind the contract the demo already exposes, plus the deletion of the demo provider.

**Settled during review.** The ticket's Description now opens with the demo-only gap — the real service refused every request while the demo provider served an in-memory document — and its Delivery note records the seeded row's controlled 404 as the one visible change, replacing the earlier "should not change visibly" claim. The plan, the register and the ticket now say the same thing.

---

## Block 2 — Broad strokes

- [x] Accepted · unchanged during review

One service now decides who may read a certificate, and one route serves both audiences. Five entry points, in reading order:

1. [CertificateLedgerService.GetPdfAsync](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L271) — the shared route's brain: authenticate, resolve the row, allow Management or the registration's owner, then read the retained bytes.
2. [RequireIssuedDocumentAsync](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L487) — the single gate both the viewer and the PDF pass through: the registration must exist, the certificate must be issued, and the stored key must sit under `certificates/`.
3. [ReadRetainedAsync](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L513) — opens the file through `IPrivateFileStore.OpenReadAsync` and turns every I/O or containment failure into the same not-found.
4. [CertificateLedgerController.Pdf](../../../ASI.Basecode.WebApp/Controllers/CertificateLedgerController.cs#L67) — the class-level Management policy moved down onto the five management actions, leaving `Pdf` open to any authenticated caller.
5. [DemoCertificatesRegistration.AddCertificates](../../../ASI.Basecode.WebApp/DemoData/DemoCertificatesRegistration.cs#L12) — the demo seam now registers only member history; `DemoCertificateLedgerService` is gone.

---

## Block 3 — Service authorization and streaming

- [x] Accepted · unchanged during review

The service grew a second authorization level. `RequireManagement()` was split into `RequireAuthenticated()` (cookie present, positive user id) and `IsManagement()` (role is admin or officer), so `GetPdfAsync` can admit any signed-in user and then apply ownership itself. Ownership is not inferred from the certificate row: it comes from `DiscoverReviewAsync`, whose `RegistrationReviewIdentity.UserId` is the registration's owner.

The read path is shared and deliberately blunt. `RequireIssuedDocumentAsync` returns the certificate plus its identity, and refuses with the same `NotFound` for a missing registration, a missing row, a row that is not issued, a blank `file_path`, or a key outside `certificates/`. Because the store derives its area from the key, the prefix check in the service is a second lock on the same door. `ReadRetainedAsync` then copies the stream into the existing `CertificateDocument` contract and maps `IOException`, `UnauthorizedAccessException`, `InvalidDataException` and `ArgumentException` to that same not-found — a deleted file is a 404, never a 500.

The owner check sits after the shared gate but before any bytes are read, and a non-owner gets the identical `NotFound` as an unknown id, so the response cannot be used to probe for other members' certificates.

- [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L271) — `GetPdfAsync`: `RequireAuthenticated`, shared gate, `identity.UserId != actorUserId` refusal, then `ReadRetainedAsync`
- [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L256) — `GetViewerAsync`: still `RequireManagement`, now builds the metadata view from the same gate
- [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L487) — `RequireIssuedDocumentAsync`, the shared gate
- [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L513) — `ReadRetainedAsync`, the contained read
- [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L570) — `RequireAuthenticated` / `IsManagement` split

---

## Block 4 — Controller and demo removal

- [x] Accepted · unchanged during review

The authorization boundary moved from the class to the actions. Dropping `[Authorize(Policy = Policies.Management)]` from `CertificateLedgerController` and re-applying it to `Index`, `Viewer`, `Approve`, `Decline` and `Issue` leaves `Pdf` with plain `[Authorize]`. Everything else on the controller is untouched: the same `DocumentFailure` mapping, the same `File(...)` response, the same `download` filename expression, the same no-store cache attribute, and antiforgery still on every POST.

The demo provider is deleted outright, and its `Replace(ICertificateLedgerService, DemoCertificateLedgerService)` line is removed from the registration. What remains under the switch is M-12: `DemoCertificateDocuments` and `DemoMyCertificatesService`. The comments that still described M-11b as live were relabelled to M-12, because the seeded document did not go away — only the officer adapter that read it did.

- [CertificateLedgerController.cs](../../../ASI.Basecode.WebApp/Controllers/CertificateLedgerController.cs#L17) — class-level policy removed
- [CertificateLedgerController.cs](../../../ASI.Basecode.WebApp/Controllers/CertificateLedgerController.cs#L67) — `Pdf` with `[Authorize]` only
- [CertificateLedgerController.cs](../../../ASI.Basecode.WebApp/Controllers/CertificateLedgerController.cs#L83) — Management policy on the three mutations
- [DemoCertificatesRegistration.cs](../../../ASI.Basecode.WebApp/DemoData/DemoCertificatesRegistration.cs#L12) — the `Replace` line gone, M-12 registrations kept
- `ASI.Basecode.WebApp/DemoData/DemoCertificateLedgerService.cs` — deleted (121 lines)

---

## Block 5 — Tests

- [x] Accepted · three gaps closed during review

The existing end-to-end certificate flow test was tightened rather than replaced, and the switch test now asserts the absence of an override instead of its presence.

New behaviour pinned in [CertificateLedgerFlowTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CertificateLedgerFlowTests.cs):

- Anonymous requests to both the ledger and the PDF are redirects whose `Location` actually starts with `/Account/Login`, not merely 302s.
- A member's PDF request against the seeded row is a 404 from the missing document, and the comment says so rather than claiming an ownership refusal.
- The officer `download=true` response asserts the attachment filename `AT-2026-<id>.pdf`.
- The seeded issued row's viewer and PDF are both 404 now that the in-memory fallback is gone.
- The owner's PDF returns 200 `application/pdf` with no-store, bytes identical to the officer's read, and the owner's `download=true` carries the same filename.
- An inactive membership still downloads (200).
- A non-owner and an unknown id both get 404.
- An issued row whose `file_path` is tampered to `uploads/venues/not-a-certificate.png` is 404 for the officer, the owner and the viewer. The test writes a real PNG to the setup root first, so the assertion fails if the `certificates/` prefix gate is removed — mutation-verified: clause removed returns 200 OK.
- A deleted retained file is 404 for both the officer and the owner.
- An inactive account is refused by the cookie check with a sign-in redirect.

In [CertificateReviewDecisionTests.cs](../../../tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs#L216) the renamed test builds a bare `ServiceCollection` for both switch states. Switch on: no `ICertificateLedgerService` descriptor is added, and `IMyCertificatesService` resolves to `DemoMyCertificatesService`. Switch off: still no ledger descriptor, and the member service resolves to `UnavailableMyCertificatesService`. The comment explains why a second host is avoided: file-watcher budget on the CI runner.

---

## Block 6 — Periphery

- [x] Accepted · unchanged during review

- [story-authorized-certificate-streaming-download-endpoint-plan.md](../tickets/epic-certificates/tickets/story-authorized-certificate-streaming-download-endpoint-plan.md) — the approved plan, added in this range, now carrying a `## Code Review` section with the eight patches, thirteen rejections, and a refreshed Code Map.
- [story-authorized-certificate-streaming-download-endpoint.md](../tickets/epic-certificates/tickets/story-authorized-certificate-streaming-download-endpoint.md) — the ticket, reworded during review: the Description opens with the demo-only gap, and the Delivery note names the seeded row's 404 as the one visible change.
- [demo-mock-register.md](../demo-mock-register.md#L97) — M-11b set to `removed (124ef29)`, its Surface column reworded, and the M-11b removal recorded in the detail section.
- [demo-mock-register.md](../demo-mock-register.md#L98) — the M-12 row now says it owns the last in-memory document path.
- [demo-mock-register.md](../demo-mock-register.md#L546) — Known limits records the seeded row's officer 404 and the ledger's dead View PDF link.
- [Startup.DI.cs](../../../ASI.Basecode.WebApp/Startup.DI.cs#L60) — the stale "demo replaces the interface" comment corrected.
- [DemoStore.cs](../../../ASI.Basecode.WebApp/DemoData/DemoStore.cs#L12), [DemoSeedService.cs](../../../ASI.Basecode.WebApp/DemoData/DemoSeedService.cs#L21), [DemoCertificateDocuments.cs](../../../ASI.Basecode.WebApp/DemoData/DemoCertificateDocuments.cs#L10) — M-11b comments relabelled M-12; the seed summary now describes this run's guard.

**Deliberately untouched:** `DemoStore`, `DemoCertificateDocuments` and `DemoMyCertificatesService` (M-12, removed by 6.9), the ledger views and CSS, the schema, and the member-history route.
