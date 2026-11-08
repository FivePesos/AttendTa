# Round 1 — Push 1 — Problem Decomposition

Method: Problem Decomposition — Split the whole into parts that can be solved alone.
Form: The whole split into independent parts, each solvable on its own, the order to reassemble, and where the draft's split differs.

## Form — 6.11 sweep split into independent parts

### Part 0 — Preconditions the plan assumes but never establishes

These are not tasks; they are facts the rest of the plan silently depends on. An implementer must confirm them before touching anything, and the plan gives no step for that.

- **P0a. The three register commits resolve and are ancestors of `dev`.** The plan asserts this in "What is settled", and acceptance criterion 5 checks it *after* the work. But the commit hashes are written into the register by task 7 of the same plan. If a hash is wrong, the failure surfaces at acceptance, not at edit time. This is a one-command check (`git merge-base --is-ancestor <sha> dev`) that belongs before task 7, not only after it.
- **P0b. The baseline revision `a9749c3…` is the tree the line numbers were read from.** Every line reference in the Code Map (`CertificateTemplate.cshtml:283-297,299-313,319-322,350-361`, `PlaywrightCertificatePdfRenderer.cs:59-67,74,90-98,112,142-150`, `Startup.DI.cs:59`, `CertificateLedgerViewModels.cs:24,41,52,60-66`) is a point-in-time claim. The plan never says "verify at baseline" or "re-locate by symbol, not by line". A sweep that starts from a moved `dev` will mis-edit.
- **P0c. Chromium and Poppler are present locally.** Verification line 98 needs both, and Design Notes line 92 says only the Poppler prerequisite is documented. An implementer who cannot run the integration class cannot satisfy acceptance 4, and the plan offers no fallback.

### Part 1 — Tracer source deletion (self-contained)

Files: the four tracer sources and the single DI registration. This part has no dependency on any other part and no risk to the retained pipeline. It is the only part whose success is fully mechanical: after it, `CertificateTracer*` cannot resolve.

Ordering note: this part must run **before** Part 2, because `ICertificatePdfRenderer.RenderSampleAsync` is consumed by the tracer controller. Deleting the interface member first leaves the controller uncompilable; deleting the controller first is clean.

### Part 2 — Sample-render path removal inside the renderer and template (depends on Part 1)

Files: `ICertificatePdfRenderer.cs`, `PlaywrightCertificatePdfRenderer.cs`, `CertificateTemplateViewModel.cs`, `CertificateTemplate.cshtml`. This is the one part with a genuine internal ordering constraint, and the plan states it only obliquely ("the `else` branch at `:354-361` is the signatory section and must become unconditional"):

1. Remove `RenderSampleAsync` from the interface and implementation.
2. Remove `IsDevelopmentSample` from the view model.
3. Remove the watermark/banner markup and their inline rules.
4. Make the signatory section unconditional.

Step 4 must not be done before steps 2–3, or the `else` has no condition left to invert. The plan also does not say what to do with the **`if` half** of the branch at `:354` — only that the `else` becomes unconditional. An implementer must infer that the `if` half is the development variant and is deleted; that inference is correct but unstated.

The template edit is the highest-risk edit in the plan for a reason the plan does not name: `CertificateTemplate.cshtml` is the only real-Chromium-rendered artifact under test, so a whitespace or markup slip here is caught only by Part 4's suite, and five of those facts read the PDF through Poppler. The plan's Design Notes (line 92) says the *rename* is the risky part; the more accurate statement is that the *template edit* is the risky part and the *rename* is the part that preserves the ability to detect it.

### Part 3 — Test fakes and unit-test deletion (independent of Parts 1–2 at source level, dependent at compile level)

Files: `CertificateIssuanceTests.cs:569-570,580-581,589-590`, `CertificateReviewDecisionTests.cs:378`, and deletion of `CertificateTracerServiceTests.cs`.

The plan says "remove the four `RenderSampleAsync` overrides". The Code Map lists six line pairs across two files plus one single line — which is four overrides across three files. The task text names two files and says "four", so the count reconciles only if the reader counts overrides rather than files. This is a small ambiguity that will cost an implementer a re-read, not a wrong edit, but it is exactly the kind of slot that a cold executor resolves by guessing.

### Part 4 — Test split and rename (depends on Part 2 for truth, on Part 3 for compile)

Files: `CertificateTracerFlowTests.cs` → `CertificatePdfRendererTests.cs`.

This part is the one the plan correctly identifies as the acceptance hinge, and it is the only part whose output is checked by a named class in the verification commands (line 98). Its internal split is stated as "keep the seven renderer facts, drop the tracer fact and its helpers, drop the now-unused `StorageProbe` parameter from `CreateFactory`". The seven facts live at "lines 36-198" — a range, not a list. The plan does not enumerate them, so an implementer must decide which of the facts in that range are the seven. Since acceptance 4 says "all seven official-template facts pass", a miscount is a silent acceptance failure: drop one and the class still passes with six.

This is the single most important gap in the plan's split, and it is specific to this draft: the Draft chose to describe the retained facts by line range and by count, not by name.

### Part 5 — Dead view-model and service surface (fully independent of Parts 1–4)

Files: `CertificateLedgerViewModels.cs`, `CertificateLedgerService.cs`.

Three deletions plus one visibility change plus one doc rewrite. This part touches nothing the tracer parts touch and can be done first, last, or in parallel. The plan correctly carves out `CertificateViewerViewModel.Empty` as used at `CertificateLedgerController.cs:121` — that carve-out is the part's only trap and it is stated.

The `ReadAsync`/`BuildPage` → private change is the only edit in the entire plan that can produce a compile error in a file the plan does not list, if either method has a caller the Code Map missed. The plan asserts "no external caller" without a grep command in Verification. Verification line 101 greps only for tracer symbols.

### Part 6 — No-tracking read (independent of Parts 1–5, but the only part that changes runtime behavior)

Files: `ICertificateRepository.cs`, `CertificateRepository.cs`, `CertificateLedgerService.cs`, `MyCertificatesService.cs`.

This is the only part of the sweep that alters what code does rather than what code exists. It adds a repository method and switches two GET paths onto it. It is independent of every other part, but it is the part most likely to be *wrong in a way the tests do not catch*, because the plan's own Boundaries say "no transaction or lock-order change" and this part sits adjacent to `CertificateLedgerService.cs:498`, which the Code Map says is a read-only path but which lives in the same service as the decision and issuance transactions the task explicitly says to keep tracked.

The plan does state the guard ("keep the tracked read for the decision and issuance transactions"), which is correct and specific. What it does not state is how the implementer distinguishes the two call sites at `:498` and `MyCertificatesService.cs:54` from any tracked read inside a transaction — the instruction is a rule, not a check.

Ordering note: this part should run **after** Part 5, because Part 5 makes `ReadAsync`/`BuildPage` private and Part 6 edits the same service file. Doing Part 6 first means re-touching the same file, and the plan's task list already puts them in that order (task 5 then task 6), so the draft's order is correct here.

### Part 7 — Register commit hashes (independent, but gated on P0a)

File: `demo-mock-register.md:94,95,98`. Three hash substitutions plus one summary-sentence correction. This part is independent of all code parts and could be done first — and should be, because it is the only part whose acceptance (criterion 5) is a `git` check rather than a build check, and it is the only part that can be verified without Chromium, Poppler, or SQL.

The plan bundles the "correct the summary sentence that says `DemoSeedService` holds no certificate state" into this task. That correction depends on the same fact as the hashes (what `DemoSeedService` actually does at those commits), so the bundling is right; the plan does not say whether the sentence correction is verified by the same ancestor check or by reading the commits.

### Part 8 — Documentation (independent, but depends on Part 4 for the new filename)

Files: `docs/database-setup.md:115-116,170-172`, `docs/demo-quickstart.md:124`, `demo-script.md:134-138`, `handoffs/story-refactor-sweep-follow-up.md:53`.

This part is independent of all code, but it contains one hidden dependency: "rename the retained coverage file" means the doc must name `CertificatePdfRendererTests`, which only exists after Part 4. This is the only cross-part ordering constraint in the plan that the draft does not state.

The plan also says Design Notes line 92 that "`docs/database-setup.md`'s Poppler prerequisite stay" — so this task must remove tracer text from `:115-116,170-172` while leaving the Poppler prerequisite. The two line ranges are not labelled as tracer-text vs. Poppler-text, so an implementer must read both ranges and decide which is which.

### Reassembly order

The draft's task list order is: delete sources → renderer/template → test split → fakes → view models → repository → register → docs. That order is **wrong in one place and right everywhere else**.

- **Correct:** Parts 1 → 2 → 4 (source before renderer before the test that renders it), and Parts 5 → 6 (view models before repository, same service file).
- **Wrong:** Part 3 (fakes) is listed *after* Part 4 (test split), but the fakes are in `CertificateIssuanceTests` and `CertificateReviewDecisionTests`, which are separate files from the split class. Their order relative to Part 4 does not matter for the split itself — but both must land before any build, so listing them after the split is harmless. The real problem is that the split (task 3) is listed before the fakes (task 4) even though deleting `RenderSampleAsync` in Part 2 already breaks the fakes' compilation. An implementer who builds after task 3 gets a build failure from task 4's files and may misattribute it to the split.
- **Missing:** Part 7 (register) should be first, because it is the cheapest acceptance check and gates P0a. Part 8 (docs) must be last, because it names the file Part 4 creates.

**Recommended reassembly:** P0a check → Part 7 (register) → Part 1 → Part 2 → Part 3 (fakes) → Part 4 (split) → Part 5 → Part 6 → Part 8 (docs) → full verification.

The draft's split is otherwise sound: the parts are genuinely independent (five of eight touch disjoint file sets), and the one real coupling — Part 4's class name into Part 8's docs — is the only reassembly constraint the draft leaves unstated.

## Findings

1. **`Tasks & Acceptance` line 63 — the seven retained facts are named by line range, not enumerated.** The task says "keep the seven renderer facts" and the Code Map says "lines 36-198 hold seven official-renderer facts to keep", but neither lists them. Acceptance line 74 ("all seven official-template facts pass") is therefore uncheckable against the plan alone: dropping one fact still leaves a green class of six. **Change:** enumerate the seven facts by test method name in the Code Map entry for `CertificateTracerFlowTests.cs`, so the split is a checklist rather than a count.

2. **`Verification` line 98 — the integration command is the only check on the highest-risk edit, and it is not runnable without Chromium and Poppler, which the plan never confirms are installed.** Design Notes line 92 says the Poppler prerequisite "stays" in `docs/database-setup.md`, but the plan gives no precondition step and no fallback. An implementer without Poppler cannot distinguish "template edit broke a fact" from "environment missing". **Change:** add a precondition line to Verification stating both are required, and add a first command that proves Poppler and Chromium resolve before the class is run.

3. **`Tasks & Acceptance` line 67 — the register hash task is ordered after four code tasks, but it is the only acceptance (line 75) that can be checked without building anything.** The hashes are asserted as "real ancestors of `dev`" in the settled facts, yet the plan writes them in and only verifies ancestry at acceptance. **Change:** move the register task to the front of Execution and add the `git merge-base --is-ancestor <sha> dev` check inline, so a bad hash fails in seconds rather than after the renderer and test work.

4. **`Code Map` line 48 — the template instruction says the `else` at `:354-361` "must become unconditional" but never says what happens to the `if` half of the branch.** The line range `350-361` is listed as "development watermark/banner markup", which conflates the condition with the signatory section. An implementer must infer that the `if` half is deleted and the `else` body is promoted. **Change:** state explicitly that the `if` branch is deleted and its condition removed, leaving the `else` body as the unconditional markup.

5. **`Tasks & Acceptance` line 65 — "delete `CertificateLedgerViewModel.Empty()`" and the Code Map's carve-out for `CertificateViewerViewModel.Empty` sit one line apart and are easy to conflate.** The two types differ by one word; the carve-out ("`CertificateViewerViewModel.Empty` is used at `CertificateLedgerController.cs:121` — keep") is correct but visually easy to miss. **Change:** restate the carve-out inside the task text itself, not only in the Code Map, and name the two types side by side.

6. **`Verification` line 101 — the grep checks only tracer symbols, so the two other deletions in the plan have no negative check.** `ReadAsync`/`BuildPage` made private (line 65) and the no-tracking read (line 66) are both verified only by "`dotnet build` clean", which passes if a missed caller happens to be in an excluded file or if the new repository method is added but never wired. **Change:** add `git grep -n "CertificateLedgerViewModel.Empty\|\.ViewerUrl\|\.PdfUrl"` and a grep confirming the new no-tracking method is referenced at exactly the two intended call sites.
