# Review narrative: Real member certificate history (6.9)

Target: PR #210, head commit `787fd61` — *feat(certificates): [BMAD 6.9] serve the real member certificate history*, on branch `feature/69-real-member-certificate-links` against `dev` in `rainyDayssss/AttendTa`

Ticket: [story-member-certificate-download-links-in-my-events.md](../tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events.md) · Plan: [story-member-certificate-download-links-in-my-events-plan.md](../tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md)

Review status: **In progress — Block 1 (intent) accepted; Block 2 (broad strokes) current.**

---

## Block 1 — Intent

- [x] Accepted · unchanged during review

**Intent (plan):**

> **Problem:** R2 is demo-only (M-12). `DemoMyCertificatesService` serves the member Certificates tab and overlays the seeded in-memory document; `UnavailableMyCertificatesService` blanks the tab when the switch is off. With demo data off, a member with a real issued certificate sees no history and no link.
>
> **Approach:** Implement the real owner-scoped history behind the existing `IMyCertificatesService`, reuse 6.8's authorized retained-file retrieval for the PDF, delete the M-12 providers and seeded-document path, and leave the 6.13 host, row model and partial unchanged.

Source: the `## Intent` section of [story-member-certificate-download-links-in-my-events-plan.md](../tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md#L19), quoted verbatim from its frozen-after-approval block.

The ticket states the same intent from the delivery side: replace 6.13's demo certificate source with the real owner-scoped issued-certificate query and agreed link fields consumed by the existing `_CertificateLinkPartial`, preserving the 3.17/3.13 host contract, so that only an issued certificate owned by the current member receives the authorized 6.8 download URL. It is tracked as issue #69 and covers R2.

---

## Block 2 — Broad strokes

- [ ] Current · not yet accepted

The change swaps the source behind an unchanged contract. The Certificates tab still calls `IMyCertificatesService` through the same controller, row model and partial; what changes is which class answers, and that class now reads the database instead of an in-memory store. Four entry points tell the story in order:

1. [MyCertificatesService.GetPageAsync](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L30) builds the page from the actor's own data: owned event ids, then that event's own confirmed registration, then that registration's certificate row, which decides the state. A row offers a document only when it is Issued and carries a retained `file_path`.
2. [MyCertificatesService.GetPdfAsync](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L100) adds the member guard and hands the id straight to the certificate ledger, which rechecks ownership and the retained file on every request.
3. [Startup.DI.cs](../../../ASI.Basecode.WebApp/Startup.DI.cs#L64) registers the real service next to the ledger, unconditionally, so both demo switch states get the same implementation.
4. The demo side shrinks to nothing that can serve certificates: [DemoDataRegistration.cs](../../../ASI.Basecode.WebApp/DemoData/DemoDataRegistration.cs#L18) adds only the store and seed, and [DemoSeedService.cs](../../../ASI.Basecode.WebApp/DemoData/DemoSeedService.cs#L260) writes the issued review as a SQL row with no document attached.

---

## Block 3 — Slices

- [ ] Not yet written · pending

<!-- Placeholder: one concern per block, split as the review proceeds (e.g. owner-scoped history query, PDF delegation and member guard, demo removal, DI and records). -->

---

## Block 4 — Periphery

- [ ] Not yet written · pending

<!-- Placeholder: docs, DI registration, register rows, PRODUCT.md, test rewrite. References only, one clause each. -->
