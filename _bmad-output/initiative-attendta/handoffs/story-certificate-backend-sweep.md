---
title: 'Certificate backend sweep (6.11) — handoff'
type: 'handoff'
created: '2026-11-08'
plan: '_bmad-output/initiative-attendta/tickets/epic-certificates/tickets/story-refactor-sweep-plan.md'
branch: 'feature/70-certificate-backend-sweep'
baseline: 'a9749c332a4d2c8ab19d0b942054ee1b8d8f9e35'
---

# Handoff — 6.11 Certificate backend sweep

This note carries the state around the plan. **The plan is the source of truth; do not re-plan from this note.**

## Start here

```
/bmad-build 6.11
```

The plan is `in-progress`, so the session resumes at implementation. Do not run step-02 again, and do not ask for approval — it was given on 2026-11-08 and the `<frozen-after-approval>` block is locked except for the 2026-11-08 renegotiation of the tracer-route row (decision 5).

## State right now

- Branch `feature/70-certificate-backend-sweep`, cut from `upstream/dev` at `a9749c3` (the 6.9 merge). The sweep is implemented and verified but **not committed**; the tree holds the change plus untracked artifacts.
- Untracked: the plan, this handoff and `_bmad-output/initiative-attendta/elicitation-6-11-refactor-sweep/`. Commit all three with the work, or they will not survive a worktree or branch move.
- Plan: `risk: medium`, `route: full` — implementation is dispatched to a fresh subagent with the plan as its only input.

## Decisions already made — do not reopen

1. **Retire the tracer, keep the retained-PDF pipeline.** `RenderSnapshotAsync` → temporary save → promote → `certificates/` is untouched. Only the sample path goes.
2. **Six official renderer facts survive, not seven.** The test class holds eight facts. `RendererKeepsDevelopmentTracerSampleWatermarked` calls `RenderSampleAsync`, so it dies with the sample path. The plan names the six by method name — use that list as a checklist, not a count.
3. **Three of the six facts need Poppler.** `pdftotext`, `pdfinfo`, `pdffonts`. `ci.yml`'s Poppler step and `docs/database-setup.md`'s prerequisite stay.
4. **The register gets three hash fixes only.** M-10a `46a2100`, M-10b `d6de0d5`, M-12 `7637096`. The M-12 row's `DemoCertificateDocuments` clause and the summary sentence at `:495` are accurate history — leave them.
5. **`/CertificateTracer` has no route after deletion; the 404 is not universal.** `Startup.Auth.cs:46-48` sets a `FallbackPolicy` of `RequireAuthenticatedUser`, and the authorization middleware runs before a missing endpoint becomes 404. So an anonymous GET or POST is redirected to `/Account/Login` (with `ReturnUrl`), while an authenticated member or Management caller gets 404. The plan's matrix row and acceptance criterion 2 were amended on 2026-11-08 to say this; the earlier claim that the redirects came from the controller's `[Authorize]` was wrong.
6. **Rejected findings stay rejected.** The Findings Ledger in the plan records why (issuance recheck, `ReviewedBy`, double save, `CreateTemporaryPath`, query rewrites, actor guard, not-found message). Do not "fix" them.

## Must not break

- Issuance, decision, invalidation and download behavior; audit rows; transaction and lock order.
- The `certificates/` prefix check and owner-or-Management authorization on the PDF route.
- The demo seam: `DemoStore`, `DemoSeedService`, `DemoDataRegistration` belong to 8.13, not here.
- Historical walkthroughs, findings and past plans — evidence, not instructions.

## Environment

```powershell
$env:ATTENDTA_TEST_SQL = "Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"
```

Integration tests use Microsoft.Testing.Platform syntax — `-class <FQN>`, not `--filter`. The plan's Verification block has the exact commands.

**Poppler decision still open:** if it is not on `PATH`, run what is runnable and record the three Poppler-dependent facts as a CI gap. Do not report acceptance 4 green without running them.

## Traps

- Delete the controller **before** `RenderSampleAsync`, or the controller stops compiling mid-change.
- `CertificateTemplate.cshtml:350-361` is an `if`/`else`. Delete the `if` branch and promote the `else` body (the signatory section) to unconditional markup. Do not drop the signatory.
- `CertificateViewerViewModel.Empty` is used at `CertificateLedgerController.cs:121` — keep it. Only `CertificateLedgerViewModel.Empty` is dead.
- The verification greps are scoped to `ASI.Basecode.*` and `tests` on purpose; an unscoped `git grep` hits the plan and the historical docs.
- `CreateFactory` loses `StorageProbe` and `TracerLogProbe` and the `IPrivateFileStore` replacement block.

## Done for this ticket

Build clean; the renamed renderer class runs seven facts — the six official-template ones plus the retired-route fact; `CertificateIssuanceTests`, `CertificateReviewDecisionTests` and `CertificateLedgerFlowTests` pass; services suite green; the three scoped greps return what the plan says; the three register commits resolve as ancestors of `dev`.

## After this

`6.11 → 8.13` (delete the demo seam and cross-module hardening) `→ 8.11` (verify no register id, demo folder or `DemoData` key in source or published output). Open PRs #208 (8.3 backup/restore) and #209 (8.2 hotspot HTTPS) are independent of this branch.
