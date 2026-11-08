# Review narrative: Real member certificate history (6.9)

Target: PR #210, head commit `787fd61` — *feat(certificates): [BMAD 6.9] serve the real member certificate history*, on branch `feature/69-real-member-certificate-links` against `dev` in `rainyDayssss/AttendTa`

Ticket: [story-member-certificate-download-links-in-my-events.md](../tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events.md) · Plan: [story-member-certificate-download-links-in-my-events-plan.md](../tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md)

Review status: **In progress — Blocks 1–4 accepted; Block 5 (periphery) current.**

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

- [x] Accepted · unchanged during review

The change swaps the source behind an unchanged contract. The Certificates tab still calls `IMyCertificatesService` through the same controller, row model and partial; what changes is which class answers, and that class now reads the database instead of an in-memory store. Four entry points tell the story in order:

1. [MyCertificatesService.GetPageAsync](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L30) builds the page from the actor's own data: owned event ids, then that event's own confirmed registration, then that registration's certificate row, which decides the state. A row offers a document only when it is Issued and carries a retained `file_path`.
2. [MyCertificatesService.GetPdfAsync](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L100) adds the member guard and hands the id straight to the certificate ledger, which rechecks ownership and the retained file on every request.
3. [Startup.DI.cs](../../../ASI.Basecode.WebApp/Startup.DI.cs#L64) registers the real service next to the ledger, unconditionally, so both demo switch states get the same implementation.
4. The demo side shrinks to nothing that can serve certificates: [DemoDataRegistration.cs](../../../ASI.Basecode.WebApp/DemoData/DemoDataRegistration.cs#L18) adds only the store and seed, and [DemoSeedService.cs](../../../ASI.Basecode.WebApp/DemoData/DemoSeedService.cs#L260) writes the issued review as a SQL row with no document attached.

---

## Block 3 — The page query

- [x] Accepted · unchanged during review

`GetPageAsync` walks the actor's own data one event at a time rather than querying certificates directly. Ownership is fixed at the first query: the repository returns the distinct event ids where the member's registration is not cancelled, joining registrations to members on the actor's user id. For each owned event the service reads the event, skips a cancelled one, then reads the actor's own registration for that event and requires it to be confirmed. Only then does it read the certificate row, map its status to a display state, and decide whether a document exists. An Issued row with no retained file is demoted to Unavailable, which is why the seeded row has no link. The rows are ordered by event start descending then registration id descending, and paged at 25.

- [MyCertificatesService.cs](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L36) — the owned event ids
- [RegistrationRepository.cs](../../../ASI.Basecode.Data/Repositories/RegistrationRepository.cs#L32) — the SQL ownership scope
- [MyCertificatesService.cs](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L43) — cancelled event skip
- [MyCertificatesService.cs](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L48) — own confirmed registration
- [RegistrationRepository.cs](../../../ASI.Basecode.Data/Repositories/RegistrationRepository.cs#L297) — registration joined to the actor
- [MyCertificatesService.cs](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L55) — status to state map
- [MyCertificatesService.cs](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L63) — `HasDocument` and the Unavailable demotion
- [MyCertificatesService.cs](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L83) — ordering and paging

---

## Block 4 — The PDF read

- [x] Accepted · unchanged during review

The download path is deliberately thin. `GetPdfAsync` runs the member guard and then hands the id to the certificate ledger, which does the real authorization on every request. The guard refuses an unauthenticated caller, a missing user id and any role other than member. The ledger re-resolves the registration's owner, requires an issued row whose retained key starts with `certificates/`, and refuses a non-owner member. Because the ledger resolves the owner from the id itself, another member's registration id and an unknown id take the same branch and produce the same not-found. The controller maps that not-found to 404 and renders one message for both not-found and forbidden, which is why the two URLs you tried were indistinguishable. The bytes come from the retained file, never from a regeneration.

- [MyCertificatesService.cs](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L102) — the delegation
- [MyCertificatesService.cs](../../../ASI.Basecode.Services/Services/MyCertificatesService.cs#L108) — the member guard
- [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L272) — owner-or-management check
- [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L488) — issued row with a retained `certificates/` key
- [CertificateLedgerService.cs](../../../ASI.Basecode.Services/Services/CertificateLedgerService.cs#L514) — retained-file read, missing file is a controlled not-found
- [MyCertificatesController.cs](../../../ASI.Basecode.WebApp/Controllers/MyCertificatesController.cs#L17) — member-only policy
- [MyCertificatesController.cs](../../../ASI.Basecode.WebApp/Controllers/MyCertificatesController.cs#L38) — outcome-to-status mapping and the shared message

---

## Block 5 — Periphery

- [ ] Current · not yet accepted

- [PRODUCT.md](../../../PRODUCT.md#L42) — the member certificate line now states the real owner-scoped history and the issued-plus-retained-file rule in both switch states.
- [demo-mock-register.md](../demo-mock-register.md#L98) — M-12's row is marked removed at `d16feed`.
- [demo-mock-register.md](../demo-mock-register.md#L488) — the M-12 removal paragraph, store summary and known limits, including that the seeded issued row has no document.
- [story-member-certificate-download-links-in-my-events-plan.md](../tickets/epic-certificates/tickets/story-member-certificate-download-links-in-my-events-plan.md) — the plan itself, with its Review Triage Log and the Code Review section this session added.
- [walkthrough-member-certificates-6-9.md](walkthrough-member-certificates-6-9.md) — this narrative, and its [log](walkthrough-member-certificates-6-9-log.md) beside it.
