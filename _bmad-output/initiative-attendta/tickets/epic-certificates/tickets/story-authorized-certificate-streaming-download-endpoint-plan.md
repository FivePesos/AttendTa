---
title: 'Authorized certificate streaming download endpoint'
type: 'feature'
ticket: '8'
created: '2026-11-07'
status: 'built'
baseline_revision: '2d31909b6fa986e015b33d0f9fd36b432a9e9d7a'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: [quick]
review_loop_iteration: 0
context:
  - '_bmad-output/initiative-attendta/demo-mock-register.md'
  - '_bmad-output/initiative-attendta/finale-test-cases.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** R6 is demo-only (M-11b): the officer viewer and PDF are served by `DemoCertificateLedgerService` from an in-memory document, while the real `CertificateLedgerService.GetViewerAsync`/`GetPdfAsync` refuse every request. With demo data off, an issued certificate cannot be viewed or downloaded.

**Approach:** Implement the real retrieval behind the existing contract: authorize Management or the registration's owner, require an issued row with a retained `certificates/` file, read it through `IPrivateFileStore.OpenReadAsync` after a containment check, and stream it as `application/pdf`. Admit members on the PDF route only. Delete the M-11b demo provider and its registration line; the seeded row without a file becomes a controlled not-found.

## Boundaries & Constraints

**Always:** Per-request authorization from `ICurrentActor`; owner = `RegistrationReviewIdentity.UserId`; Management = admin/officer; inactive membership is allowed; the AD-8 cookie check refuses inactive Accounts; unknown id and non-owner receive the identical not-found; the route takes an id only; read the retained file, never regenerate it; no-store caching; ledger reads and commands stay Management-only; keep antiforgery on mutations.

**Never:** A caller-supplied path, static-file serving, re-rendering or replacing a retained PDF, SQL/schema changes, member-history rewiring (6.9), deleting `DemoCertificateDocuments`/`DemoStore` (M-12 still reads them), a second controller or screen, or presentation changes.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Owner download | Member owns the issued row with a retained file | 200 `application/pdf`; `download=true` is an attachment; bytes equal the retained file | No error |
| Management download | Officer/admin requests any issued row | Same as owner | No error |
| Non-owner member | Member requests another member's row | Identical not-found response to an unknown id | 404 |
| Unknown id | Any authenticated caller | Not-found | 404 |
| Issued without file | Seeded row with no `file_path` | Not-found | 404 |
| Missing/unsafe file | File deleted or key outside `certificates/` | Not-found, never a 500 | 404 |
| Anonymous | No cookie | Redirect to sign-in | Redirect |
| Inactive account | Cookie rejected by AD-8 | Redirect to sign-in | Redirect |
| Inactive membership | Owner's membership inactive | Download still succeeds | No error |
| Viewer | Management opens `/Certificates/Viewer/{id}` | Metadata page with Open/Download | 404 when unissued/missing |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/Services/CertificateLedgerService.cs:250-260` — replace the `GetViewerAsync`/`GetPdfAsync` stubs; `:487-514` — reuse `RequireManagement`/`NotFound`, add a document-access helper and a retained-file reader.
- `ASI.Basecode.Data/Interfaces/IRegistrationRepository.cs:78,94` — `DiscoverReviewAsync` returns `RegistrationReviewIdentity(UserId, MemberId, EventId)`; this is the owner check.
- `ASI.Basecode.Data/Interfaces/ICertificateRepository.cs:28` — `GetByRegistrationIdAsync` returns the tracked row with `CertificateStatusId`, `FilePath`, snapshots and number.
- `ASI.Basecode.Services/Interfaces/IPrivateFileStore.cs:29` — `OpenReadAsync`; `ASI.Basecode.WebApp/Storage/PrivateFileStore.cs:132-144,269-357` — area/segment/descendant/reparse containment and PDF signature.
- `ASI.Basecode.WebApp/Controllers/CertificateLedgerController.cs:17,58-91` — move `[Authorize(Policy = Policies.Management)]` from the class to `Index`, `Viewer`, `Approve`, `Decline`, `Issue`; leave `[Authorize]` on `Pdf`.
- `ASI.Basecode.WebApp/DemoData/DemoCertificateLedgerService.cs` — delete the M-11b provider.
- `ASI.Basecode.WebApp/DemoData/DemoCertificatesRegistration.cs:7-20` — remove only the `Replace(ICertificateLedgerService, DemoCertificateLedgerService)` line; keep the M-12 `DemoCertificateDocuments` and `DemoMyCertificatesService` registrations.
- `ASI.Basecode.WebApp/DemoData/DemoCertificateDocuments.cs:10`, `DemoStore.cs:14,61-63`, `DemoSeedService.cs:21,112,267` — relabel the remaining seeded-document comments from M-11b to M-12.
- `ASI.Basecode.WebApp/Startup.DI.cs` — correct the stale demo-override comment.
- `tests/ASI.Basecode.IntegrationTests/CertificateLedgerFlowTests.cs:56-61,182-189,233-234` — update member denial, seeded-row and missing-file assertions; add owner/non-owner PDF checks.
- `tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs:219-236` — the switch no longer replaces the real ledger.
- `_bmad-output/initiative-attendta/demo-mock-register.md:97,446,464-470` — mark M-11b removed with the commit and update the detail.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Services/Services/CertificateLedgerService.cs` -- implement `GetViewerAsync` (Management, issued + retained file) and `GetPdfAsync` (authenticated; owner or Management; issued + retained file; `OpenReadAsync`; map I/O and containment failures to `NotFound`) -- the real R6 source.
- [x] `ASI.Basecode.WebApp/Controllers/CertificateLedgerController.cs` -- move Management authorization to the management actions and leave `Pdf` authenticated-only -- one route for officers and owners.
- [x] `ASI.Basecode.WebApp/DemoData/DemoCertificateLedgerService.cs` -- delete -- M-11b removal.
- [x] `ASI.Basecode.WebApp/DemoData/DemoCertificatesRegistration.cs` -- remove the ledger replacement; keep the M-12 member registrations -- no demo ledger in either switch state.
- [x] `ASI.Basecode.WebApp/DemoData/{DemoCertificateDocuments,DemoStore,DemoSeedService}.cs` and `ASI.Basecode.WebApp/Startup.DI.cs` -- relabel the remaining seeded-document comments to M-12 and correct the DI comment -- keep the demo story true.
- [x] `tests/ASI.Basecode.IntegrationTests/CertificateLedgerFlowTests.cs` -- assert owner 200, non-owner 404, seeded-row 404, missing-file 404 for both officer and owner -- the focused smoke.
- [x] `tests/ASI.Basecode.IntegrationTests/CertificateReviewDecisionTests.cs` -- assert the real ledger in both switch states -- no demo override.
- [x] `_bmad-output/initiative-attendta/demo-mock-register.md` -- set M-11b `removed` and update the M-11b/M-12 notes -- register rule 11.

**Acceptance Criteria:**
- Given an issued certificate with a retained file, when the owner or a Management user requests `/Certificates/Pdf/{id}`, then the response is 200 `application/pdf` and the bytes match the retained file.
- Given the same row, when `download=true`, then the response is an attachment named with the certificate number.
- Given a non-owner Member or an unknown id, when either requests the PDF, then both receive the identical 404 response.
- Given the seeded issued row with no `file_path`, when an officer or its owner requests the PDF, then the response is 404.
- Given the retained file is deleted, when the PDF is requested, then the response is 404, not 500.
- Given an inactive membership, when the owner requests the PDF, then the download still succeeds.
- Given an anonymous or inactive-account request, then the request is redirected to sign-in.
- Given `DemoData:Enabled=true`, when the ledger issues a certificate, then the officer viewer/PDF and the owner PDF read the retained file; the seeded row remains a controlled 404.

## Implementation Notes

- `GetPdfAsync` authenticates first, then resolves the registration owner through
  `DiscoverReviewAsync` and requires `identity.UserId == actor.UserId` unless the role is
  `admin`/`officer`. The ownership refusal and every other document refusal throw the same
  `NotFound` ("Certificate not found."), so a non-owner cannot distinguish another member's row
  from an unknown id.
- Both document reads require `certificates.status == issued` and a `file_path` starting with
  `certificates/`; the prefix is checked in the service as well as the store so a corrupt key
  cannot open a venue or event image. `OpenReadAsync` failures (`IOException`,
  `UnauthorizedAccessException`, `InvalidDataException`, `ArgumentException`) map to `NotFound`.
- `GetViewerAsync` stays Management-only; only `Pdf` is authenticated-only. The controller keeps
  `[Authorize(Policy = Policies.Management)]` on `Index`, `Viewer`, `Approve`, `Decline` and
  `Issue`, and `[Authorize]` on `Pdf`.
- The seeded issued row has no retained file, so its officer viewer/PDF is now a controlled 404.
  `DemoStore` and `DemoCertificateDocuments` remain for M-12 member history until 6.9.
- The register row is set to `removed (124ef29)` in the follow-up docs commit, matching the 6.7
  pattern.
- Step-03 matrix audit added two focused assertions to `CertificateLedgerFlowTests`: an inactive
  membership still downloads (200) and an inactive account is redirected to sign-in (302). Both
  passed in the 26-test filtered run.

## Plan Change Log

## Review Triage Log

**Pass 1 (quick)** — high 0, medium 0, low 8, false 5, maybe-false 0.

| Finding | Verdict | Route | Evidence / action |
|---|---|---|---|
| `demo-mock-register.md:97` — M-11b is `removed` without the removing commit, against register rule 11 | low | patch | Verified: every other removed row carries a sha. Set to `removed (124ef29)` in the follow-up docs commit, as 6.7 did for M-11a. |
| `demo-mock-register.md:97` — the M-11b Surface column still describes the deleted in-memory fallback | low | patch | Verified: the row text names the seeded in-memory document that 6.8 deleted. Reword to the real authorized retained-file retrieval. |
| `CertificateReviewDecisionTests.cs:216-235` — the test never resolves the ledger from a switch-on host | false | — | The bare `ServiceCollection` proves the demo registration adds no `ICertificateLedgerService` descriptor, and the host proves the real registration; with no switch-on branch in `DemoCertificatesRegistration`, a second host would resolve the same descriptor. |
| `CertificateLedgerFlowTests.cs:56,264` — the anonymous and inactive-account assertions check only 302, not the sign-in target | low | patch | Verified: `AllowAutoRedirect = false` and `LocationPath` is already used for the AccessDenied check. Assert the sign-in path for both. |
| `CertificateLedgerFlowTests.cs:62` — the early non-owner comment claims an ownership refusal, but the row is unissued at that point | low | patch | Verified: completion happens later, so the early 404 is the unissued path. Correct the comment; the later assertion covers the ownership path. |
| `CertificateLedgerFlowTests.cs:253` — the missing-file test cannot distinguish a controlled not-found from another 404 | false | — | The test's claim is the status after `File.Delete`; the service maps the store's `IOException` family to the same `NotFound`, and the acceptance criterion is "404, not 500". |
| `CertificateLedgerFlowTests.cs:178-180` — the officer `download=true` path does not assert the attachment name | low | patch | Verified: the controller shares the filename expression, but the matrix says Management is "same as owner". Add the filename assertion. |
| `DemoSeedService.cs:21` — the M-12 summary says the document is seeded "only when the real review is approved" | low | patch | Verified: the seed writes the document when the row it just marked `Issued` is not already in the store. Reword to the actual guard. |
| `Startup.DI.cs:60-61` — the comment says the demo registration adds only M-12 member history, omitting the shared document reader | low | patch | Verified: `DemoCertificatesRegistration` also registers `DemoCertificateDocuments`. Include it in the comment. |
| `CertificateLedgerService.cs:271-284` — the ownership comparison is not the earliest gate | false | — | The identity is resolved first and the ownership comparison happens before any bytes are read; the certificate-row read is server-side and every refusal is the identical `NotFound`. No caller-visible difference. |
| `CertificateLedgerService.cs:519-522` — the endpoint buffers the PDF instead of true streaming | false | — | The existing `CertificateDocument` contract returns bytes; the file is read through `OpenReadAsync` and the controller streams it as `application/pdf`. True chunked streaming would change the contract outside this ticket. |
| `CertificateLedgerController.cs:120-121` — the Viewer retry link re-requests the same PDF | false | — | Pre-existing presentation behavior; the plan forbids presentation changes and the 404 is the accepted controlled not-found. |
| `CertificateLedgerController` / `Index.cshtml` — the ledger still renders View PDF for the seeded row that now 404s, and the register's Known limits does not say so | low | patch | Verified: the plan accepts the seeded-row 404 and forbids presentation changes. Record the officer-facing dead end in the register's Known limits. |

## Design Notes

- The route id is the registration id used by the existing ledger contract (`CertificateViewerViewModel.CertificateId`), not a path. The store derives the area from the stored key; the service additionally requires the `certificates/` prefix so a corrupt `file_path` cannot open a venue/event image.
- `GetViewerAsync` stays Management-only: the viewer page is the officer affordance and its "Back to certificates" link targets the Management ledger. The shared owner-or-Management route is `Pdf`, which the 6.13 partial already targets in 6.9.
- The seeded issued row keeps its in-memory document for M-12 (member history) until 6.9; only the officer M-11b adapter is removed, so the ledger's seeded row becomes a controlled 404.

## Verification

**Commands:**
- `dotnet build` -- expected: clean.
- `$env:ATTENDTA_TEST_SQL = "Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --filter "FullyQualifiedName~CertificateLedger|FullyQualifiedName~CertificateReviewDecision|FullyQualifiedName~CertificateIssuance"` -- expected: all pass.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` -- expected: green.

**Manual checks:**
- With `DemoData__Enabled=true`, complete the seeded event, issue a certificate, open the officer viewer and download; sign in as the owner and download `/Certificates/Pdf/{id}`; confirm a non-owner and the seeded row return 404 and the retained file is unchanged.
