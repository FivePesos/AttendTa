# Round 1 — Push 3 — Consistency Check

Method: Consistency Check — Restate the draft's claims and find where it contradicts itself or its own numbers.
Form: Pairs of claims that conflict, with the one to keep and why.

## Form — story-refactor-sweep-plan.md

### Fact-count arithmetic

The plan states "seven official-template renderer facts" in four places (Intent-adjacent Code Map, Always, task 3, acceptance 4, verify command). The evidence file has **eight** facts: seven `RenderSnapshotAsync` facts at lines 36–198 plus the tracer fact at 200–410.

- Claim A (Code Map line 49): "lines 36-198 hold seven official-renderer facts to keep; the tracer fact … goes."
- Claim B (Verification line 98): "`-class …CertificatePdfRendererTests` — expected: 7 pass."
- Claim C (Design Notes line 92): "five of its seven facts read the PDF through Poppler."

A and B agree with each other and with the file. C does not: of the seven kept facts, **six** invoke a Poppler tool (`pdftotext`/`pdfinfo`/`pdffonts`). Only `OfficialRendererRejectsUnapprovedSignatoryConfiguration` (line 91) touches no PDF tool. Keep C's arithmetic corrected to six, not A/B.

### What the split actually removes

- Claim A (Code Map line 49): the helpers that go are "`PostTraceAsync`, `AssertLandscapeA4`, `StorageProbe`, `FailingPdfRenderer`, `TracerLogProbe` and `TrackingPrivateFileStore`."
- Claim B (task 3, line 63): "drop the now-unused `StorageProbe` parameter from `CreateFactory`."

B is incomplete, not wrong, but it is the only instruction the implementer gets about `CreateFactory`, and it understates the change. In the evidence, `StorageProbe` is not merely a parameter: it is registered as a singleton (`services.AddSingleton(probe)`, line 425) and consumed by `TrackingPrivateFileStore` (line 426). Deleting both means `CreateFactory` loses its `probe` parameter, its `RemoveAll<IPrivateFileStore>()`/`AddSingleton`/`AddScoped` block, and its `TracerLogProbe` parameter and `AddLogging` block (lines 436–439). What survives is the `renderer` and `configureSignatoryOptions` parameters, which the two kept facts at lines 94–98 need. Keep B's intent but state the surviving signature explicitly, because a literal read of "drop the `StorageProbe` parameter" leaves a `logProbe` parameter with no type.

### The 404 acceptance versus the anonymous request

- Claim A (I/O matrix line 36): "GET or POST `/CertificateTracer`, any environment → No route exists; 404."
- Claim B (Acceptance line 72): "Given a Development host, when `/CertificateTracer` is requested, then the response is 404."

Both are stated unconditionally, but the evidence shows the tracer fact asserted a **redirect** for the anonymous request (`Assert.Equal(HttpStatusCode.Redirect, … GetAsync("/CertificateTracer")`, line 292) and for the member (`/Account/AccessDenied`, line 334). The follow-up handoff records why (line 73): "Missing anonymous URLs can be redirected to sign-in rather than returning 404 due to the existing fallback policy."

So the true post-deletion behaviour is: anonymous → redirect to sign-in, authenticated non-Management → redirect to AccessDenied, authenticated Management → 404. Keep the matrix row (it is the honest "no route exists" statement) but qualify the acceptance criterion to "an authenticated Management request", otherwise the criterion is unverifiable as written and invites an implementer to weaken the fallback policy — which the plan's own Never list forbids.

### The register commits versus the register's own record

- Claim A (task 7, line 67): set M-10a to `46a2100`, M-10b to `d6de0d5`, M-12 to `7637096`.
- Claim B (Findings Ledger line 79): "three wrong register commits."
- Claim C (register, line 94): M-10a currently reads `cd227f694fdf71d10504db4406d13a3694941cbb`; line 95 M-10b is `removed` with no sha; line 98 M-12 reads `d16feed`.

B is accurate for M-10a and M-12 (both carry a sha that the plan replaces). It is not accurate for M-10b: that row has **no** commit to be wrong. The plan also never says what the seven-character shas abbreviate — the register's existing style is inconsistent (line 88 uses a full 40-character sha, lines 90–92 seven characters), and the plan leaves M-10a's full sha being *replaced by* a seven-character one. Keep A, but restate B as "one wrong register commit, one missing, one wrong" and add one clause fixing the abbreviation convention so the register stays internally consistent.

### M-12's row text versus the summary sentence

- Claim A (task 7, line 67): "correct the summary sentence that says `DemoSeedService` holds no certificate state."
- Claim B (register, line 495): "`DemoStore`, `DemoSeedService` and `DemoDataRegistration` no longer carry any certificate state: the store keeps only the pass token, and the seed writes the issued review as a SQL row with no retained file."

The sentence the plan wants corrected is not wrong about the seed. It says the seed writes the *review* as a SQL row — which is exactly what the register's own M-01 detail says (line 145, 456) and what the store paragraph repeats (line 136). What is actually stale is the M-12 row's own text at line 98, which still lists `DemoCertificateDocuments` as "its only remaining consumer after M-11b" — a class line 491 says 6.9 deleted. Keep A's target (the summary sentence) but name the defect precisely: it is the M-12 row's `DemoCertificateDocuments` clause, not the seed sentence, that contradicts the register. As written, task 7 sends the implementer to edit a correct sentence.

### The view-model deletions versus the retained `Empty()`

- Claim A (Code Map line 52): "`CertificateLedgerViewModels.cs:24,41,52,60-66` — dead `Empty()`, `ViewerUrl`, `PdfUrl`; stale demo doc. `CertificateViewerViewModel.Empty` is used at `CertificateLedgerController.cs:121` — keep."
- Claim B (task 5, line 65): "delete `CertificateLedgerViewModel.Empty()`, `CertificateLedgerRowViewModel.ViewerUrl`, `CertificateViewerViewModel.PdfUrl`."

A and B agree, and the evidence confirms the line numbers exactly: `Empty()` at 24, `ViewerUrl` at 41, `PdfUrl` at 52, the demo doc at 60–63. This pair is consistent and needs no change — recorded here because it is the one place the plan's line references are all exact, which raises the bar for the references that are not (below).

### The doc list versus the follow-up handoff's own status

- Claim A (task 8, line 68): edit `handoffs/story-refactor-sweep-follow-up.md` to "remove the tracer instructions and rename the retained coverage file."
- Claim B (Never, line 30): "editing historical walkthroughs, findings or past plans."

The handoff is a `status: 'built'` plan with a frozen block, an Implementation Notes section and a Review Triage Log — it is a past plan by the Never list's own definition. The only tracer text in it is line 53, a Code Map bullet naming the file as existing coverage. Keep A but narrow it to line 53 only and say so, or move the edit out of task 8; as written, task 8 authorizes an edit the Never list forbids, and the two cannot both be satisfied.

## Findings

1. **Design Notes, line 92 — "five of its seven facts read the PDF through Poppler."** Wrong count. Six of the seven kept facts invoke a Poppler tool; only `OfficialRendererRejectsUnapprovedSignatoryConfiguration` (evidence line 91) does not. Change to "six of its seven facts", and keep the following sentence about `ci.yml` and `docs/database-setup.md` — it is correct and important.

2. **I/O matrix line 36 and Acceptance line 72 — unconditional 404.** Contradicted by the evidence (anonymous GET redirected at line 292, member redirected to AccessDenied at line 334) and by the follow-up handoff line 73, which names the fallback policy as the cause. Change the acceptance criterion to "an authenticated Management request" and add one line to the matrix row: "anonymous and non-Management requests keep the existing sign-in and AccessDenied redirects."

3. **Task 3, line 63 — "drop the now-unused `StorageProbe` parameter from `CreateFactory`."** Understates the change. `StorageProbe` is registered as a singleton (evidence line 425) and consumed by `TrackingPrivateFileStore` (line 426); `TracerLogProbe` is a second parameter feeding `AddLogging` (lines 436–439). State the surviving signature explicitly: `CreateFactory(connectionString, environmentName, renderer = null, configureSignatoryOptions = null)`, with the `IPrivateFileStore` replacement block removed.

4. **Findings Ledger, line 79 — "three wrong register commits."** Only two are wrong (M-10a line 94, M-12 line 98); M-10b (line 95) has no commit to be wrong, and M-12's row text at line 98 still credits `DemoCertificateDocuments`, which the register says 6.9 deleted. Restate as "one wrong, one missing, one wrong" and add task 7's missing clause: pick one sha length for the three new values so the register stops mixing 40-character (line 88) and 7-character (lines 90–92) forms.

5. **Task 7, line 67 — "correct the summary sentence that says `DemoSeedService` holds no certificate state."** Misidentified target. That sentence (register line 495) is consistent with the register's own M-01 detail (lines 145, 456) and with line 136. The actual contradiction is the M-12 row's `DemoCertificateDocuments` clause at line 98. Name line 98 as the sentence to correct, or the implementer will edit a correct sentence and leave the wrong one.

6. **Task 8, line 68 versus Never, line 30 — editing `story-refactor-sweep-follow-up.md`.** The handoff is a `status: 'built'` past plan with a frozen block and triage log, which the Never list forbids editing. Its only tracer text is line 53. Narrow task 8 to "line 53 of the handoff only" and say why (it names the coverage file by its old name), or drop it from task 8 and leave the stale reference to be fixed by whoever next touches the handoff.
