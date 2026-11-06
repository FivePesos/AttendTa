# AttendTa frontend-first shared delivery queue

Decision: 2026-11-06 — approved by the project owner. Keep the eight capability epics,
R1–R36 and the final MVP acceptance goals. Change delivery order, not product scope.
Five interchangeable builders self-claim the next unblocked ticket; no fixed person/module lanes.

**Live GitHub queue:** [AttendTa frontend-first shared delivery](https://github.com/rainyDayssss/AttendTa/milestone/1).
Its ordered description, phase/area labels and native blocker links carry the current tracker view.
The eight capability epics stay local; do not create duplicate epic issues.

For the sequential five-builder batches and the shared tickets that unlock several consumers,
use [delivery-waves.md](delivery-waves.md). It is a derived guide, not a change to issue blockers.

**First handoff:** [#135 — frontend walkthrough](https://github.com/rainyDayssss/AttendTa/issues/135),
then [#136 — shared shell/routes/contracts](https://github.com/rainyDayssss/AttendTa/issues/136).
After #136, [#137](https://github.com/rainyDayssss/AttendTa/issues/137),
[#138](https://github.com/rainyDayssss/AttendTa/issues/138),
[#139](https://github.com/rainyDayssss/AttendTa/issues/139),
[#140](https://github.com/rainyDayssss/AttendTa/issues/140) and
[#141](https://github.com/rainyDayssss/AttendTa/issues/141) are the first five independent
surface choices. Writing/publishing this plan did not start any of them.

## Baseline and authority

- Work from integrated `dev`, including PR #134 (Master Data filters/profile) and PR #133
  (start cutoff), merge `997f290`. Do not rebuild these features or reopen completed history.
- GitHub issue state and merged evidence must be reconciled before dispatch. Local plan-file
  `built`/`in-review` text is historical build evidence, not a fresh claim or GitHub closure.
- The `tickets.toml` entries and pulled leaf files are the requirements. `after` contains real
  prerequisites; `phase`, `area` and numeric `queue` order the remaining shared work.
- `docs/DESIGN.md` remains the visual authority. The separate `F:/AttendTa` prototype is a
  read-only visual source, not a second app or a source of production business rules.
- The architecture's one writer per table/module still applies to code. It does **not** assign
  a permanent person to that module. Historical WBS owners remain attribution/context only.
- Old dependency waves, `bmad-progress.*` and older ownership tables are historical snapshots,
  not today's dispatch queue. Always check the live issue before starting.

## Twelve-hour budget

| Window | Checkpoint |
|---|---|
| 0–45 minutes | Reconcile state, early rendered walkthrough, shared routes/contracts and source boundaries. |
| 45 minutes–4.5 hours | Complete required frontend surfaces; use ready backend and isolated fixtures for gaps. |
| 4.5–8.5 hours | Finish backend commands/queries and replace prototype sources. |
| 8.5–10 hours | Focused backend hardening, mock-free integration, final frontend walkthrough/refactor/polish. |
| 10–12 hours | Important automated cases, combined regression, manual/demo/device/storage evidence and release decision. |

This is a planning budget, not a completion guarantee. Environment preparation may run early.
Cheap ready-backend integrations may accompany frontend work. If time slips, record actual
remaining work; do not label fixture-backed workflows complete or remove core safety rules.

## Claim and status protocol

1. Check GitHub for an open, unclaimed issue with satisfied native blockers. Select the lowest
   numeric queue rank in the earliest phase with available work; prefer frontend first.
2. Comment `Claim: <person/session>`, assign yourself where possible, and change `backlog` to
   `in-progress` **before editing**. Re-read the issue: an earlier claim wins; a second builder
   backs off. Shared-account sessions must use distinct session names in their claim comments.
3. Keep one active implementation per builder. A blocked ticket says exactly what it awaits;
   mark it `blocked`, keep its blocker links current and take another unblocked ticket only
   after releasing/parking the claim explicitly. Clear the block on resolution.
4. Move to `review` with PR, checks and handoff evidence; another available builder can review.
   Close only after the ticket's own acceptance is true. A prototype slice can finish while
   its linked functional issue stays open. Never close an epic merely because children close.
5. Update the GitHub issue and BMAD mirror together using the configured tracker workflow.
   Read the current remote body before editing; do not overwrite another builder's new scope.

No permanent assignments are created by this replan. Historical assignees on unstarted backlog
items are cleared for the shared pool; active claims/reviews are not silently reassigned.

## Frontend queue

All rows below are **frontend/UI-facing** and require the installed `impeccable` skill for
implementation and rendered critique, with polish before review. Keep the incumbent visual
identity; do not substitute a redesign. For an inspection-only spike, critique is the deliverable.

| Rank | Ref | Outcome / handoff | Prerequisite |
|---:|---|---|---|
| 10 | 1.12 | Early frontend walkthrough; concrete route, usability and refactor findings. | Existing shell 1.9 |
| 20 | 1.13 | Shared role navigation, routes, typed screen/partial contracts, isolated fixture mechanism and feature asset seams. | 1.12 |
| 30 | 3.17 | One My Events/history host, tabs/pager and agreed pass/attendance/certificate extension slots. | 1.13 |
| 40 | 4.11 | Scanner/manual/invalidation screen and distinct camera/manual adapters. | 1.13 |
| 50 | 6.12 | Certificate ledger, decision dialogs and sample document viewer using the existing renderer/template. | 1.13, shipped 6.2/6.3 |
| 60 | 7.4 | Per-event report frontend and coherent scenario fixtures. | 1.13 |
| 70 | 3.16 | Officer event dossier, contextual roster and lifecycle dialogs; reuse ready real commands. | 1.13 |
| 80 | 3.18 | Member browse/details visual alignment; preserve real signup/cancellation/cutoff. | 1.13, shipped 3.10 |
| 90 | 2.10 / #127 | Residual loading/error/retry only; filters/counts already shipped. | 1.13, shipped 2.9 |
| 100 | 2.12 | Reusable centered dialog and real Event Type create/edit tracer. | 1.13, shipped 2.9 |
| 110 | 4.10 | Pass cards and QR availability/download presentation. | 3.17 |
| 120 | 4.12 | Attendance-history partial with independent paging. | 3.17 |
| 130 | 6.13 | Member certificate-history partial and download-link states. | 3.17 |
| 140 | 2.13 | Member centered editor using the shared dialog. | 2.12 |
| 150 | 2.14 | Venue centered editor/photo/validation using the shared dialog. | 2.12 |
| 160 | 5.11 | Account/membership access dialogs; reuse real impact preview. | 2.13, 1.13, shipped 5.3 |
| 170 | 4.13 / #129 | Remaining profile-to-passes action; retain profile work from #132/#134. | 4.10 |
| 180 | 2.11 / #128 | Accept the full original three-editor visual scope, not only the tracer. | 2.13, 2.14 |

After 1.13, five independent surface tickets (3.17, 4.11, 6.12, 7.4, 3.16) are unblocked
together. These are queue options, **not assignments to five named builders**.

### Source boundaries that make parallel work safe

- 1.13 owns shared shell/navigation, route and DI/provider seams, global JS and primitive asset
  setup. Its handoff must name actual files and data contracts before consumers start.
- 3.17 owns `MyEventsController`, the host/card model and extension points. 4.10, 4.12 and 6.13
  implement only their own partial/query contracts; no competing host or last-minute model edits.
- 4.11 prepares separate camera/manual adapters. 4.4 and 4.5 can wire these independently after
  the validation engine; manual fallback does not wait for the camera to work.
- 2.12 owns the dialog primitive and Event Type tracer; 2.13/2.14 consume it in separate editors.
  5.11 consumes the Member status-action seam after 2.13 rather than racing the same markup.
- Backend tickets extend the existing owner service/hook, not a duplicate rule implementation.
  Access mutations serialize their shared administration changes; certificate handlers, queries
  and issuance use separate agreed files where they run in parallel.
- Declare a new shared-file dependency before changing another ticket's source boundary.
  `Startup.DI.cs` and global source CSS/JS changes require a single integration claim. Generated
  `wwwroot/css/app.css` is regenerated from current sources after merges, never hand-merged.

## Backend / real integration queue

Backend-only entries do **not** require Impeccable. The two browser adapter integrations 4.4/4.5
are frontend-facing and do require it. Existing full business guards and final acceptance remain.

| Rank | Ref / existing issue | Real delivery |
|---:|---|---|
| 190 | 8.14 / #150 | Actual demo-host evidence/fixes following tooling-only #74; prepare HTTPS inputs early. |
| 200 | 3.13 / #42 | Real owner-scoped registrations query in the existing 3.17 host. |
| 210 | 3.12 / #41 | Completion transaction, close check-in, completion hook. |
| 220 | 4.1 / #44 | Real secure QR PNG and authorized download in the 4.10 presentation. |
| 230 | 4.2 / #45 | Real audited check-in window command. |
| 240 | 4.3 / #46 | Shared validated, transactional, idempotent attendance engine. |
| 250–280 | 5.4–5.7 / #55–#58 | Real account/member deactivate/reactivate, cascades and session behavior. |
| 290 | 6.4 / #64 | Create reviews through the completion hook and canonical attendance facts. |
| 300–310 | 4.4–4.5 / #47–#48 | Real camera and manual adapters; local assets and no search side effects. |
| 320 | 4.6 / #49 | Audited attendance invalidation and hook declaration. |
| 330 | 4.7 / #50 | Real owner-scoped attendance source in the existing history partial. |
| 340–380 | 6.5–6.9 / #65–#69 | Invalidation/issued guard, real review decisions, retained issuance, downloads and member links. |
| 390–410 | 7.1–7.3 / #71–#73 | Real coherent report summaries, snapshot distributions and paged details. |

Ranks are tie-breaks among **unblocked** work, not a demand to wait for every earlier number.
Mapped attendance/facts let review creation and report query implementation start without waiting
for the whole scanner UI. Final real-data proof still waits for all required integrations.

## Final project phase

| Rank | Ref / issue | Acceptance responsibility |
|---:|---|---|
| 450–480 | 3.14 / #43, 4.9 / #51, 5.10 / #60, 6.11 / #70 | Focused backend module cleanup from concrete findings; no repeated whole-app UI review. |
| 500 | 8.13 / #151 | Cross-module backend hardening; preserve transactions, locks, facts, auth, hooks and storage. |
| 510 | 8.11 / #152 | Replace/remove required fixtures and simulated outcomes; prove real integrations. |
| 520 | 8.12 / #153 | Final frontend walkthrough, targeted refactor, Impeccable implementation/critique/polish. |
| 530 | 8.10 / #83 | Release cleanup **before** acceptance tests and release. |
| 540 | 8.5 / #78 | Add/reuse only important automated cases; map every other SPEC scenario to manual verification. |
| 550 | 8.4 / #77 | One combined existing/new automated run with environment and commit evidence. |
| 560–570 | 8.2 / #75, 8.3 / #76 | Actual trusted-HTTPS phone scanning and backup/restore/redeploy proof. |
| 580 | 8.6 / #79 | Complete manual SPEC checklist; reuse automated/UI/device evidence. |
| 590 | 8.7 / #80 | Full 26-function plus FR-35/FR-36 demo continuity and evidence. |
| 600 | 8.8 / #81 | Human Tier 3 acceptance; retest only missing/affected cases. |
| 610 | 8.9 / #82 | Release PR and human merge **last**, with peer review and green CI. |

The test cases and release blockers are in [finale-test-cases.md](finale-test-cases.md).
No implementation, review, execution evidence or human sign-off is claimed by writing this plan.

## Prototype data register and removal rule

1.13 establishes the register as part of its implementation handoff; each surface records its
source, fixture scenarios, real-provider contract, replacement ticket and removal evidence.

| Surface | Real integration accountable |
|---|---|
| My Events registrations | 3.13 / #42 |
| Pass/QR download | 4.1 / #44 |
| Scanner/toggle/manual/invalidation | 4.2–4.6 / #45–#49 |
| Member attendance history | 4.7 / #50 |
| Completion and review creation | 3.12 / #41; 6.4 / #64 |
| Account/member access changes | 5.4–5.7 / #55–#58 |
| Certificate review/issue/download/history | 6.5–6.9 / #65–#69 |
| Reports | 7.1–7.3 / #71–#73 |

Fixtures require explicit Development-only enablement, a conspicuous prototype notice and no
real mutation. They must not silently mask a real API failure or mix fake attendance/certificates
with real counts. They cannot become passwords, real passes, official certificates or production
credentials. 8.11 removes prototype runtime wiring; isolated test fixtures may remain in tests.

## Development verification policy

- Frontend: build/CSS when affected, short rendered primary/error/keyboard/phone smoke,
  Impeccable implementation + critique + polish; no exhaustive mock-data or screenshot suite.
- Backend: build, one primary/guard check and a focused regression when changing a live
  high-risk mutation. Keep existing protections/tests; do not introduce broad per-ticket quotas.
- Finale: 8.5 fills important coverage gaps, 8.4 executes the complete existing suite, and
  8.6–8.8 provide full scenario/manual/demo acceptance. Existing required PR CI is not disabled.
- Do not invent lint/typecheck scripts: the web package has `css:build` and `test:js`, not a
  separate lint/typecheck command. Use the repository's actual .NET/CI commands.

## Decisions and unresolved inputs

- Q1 signatory setup shipped in #63/PR #102; reuse it. Official final PDF acceptance still runs.
- Q2 needs a real host address/certificate decision and phone verification; preparation can
  start early, but #75 cannot pass on a localhost screenshot alone.
- #74 closed the provisioning tooling; 8.14 owns the still-unproven host/SQL/log/offline checks.
- Report wording follows the existing SPEC-over-PRD precedence and `docs/DESIGN.md`; do not
  invent a new PM override or extra cross-event metrics while implementing the report.
- All final MVP goals remain required. Cosmetic extras can be reduced only as recorded scope
  decisions; remaining mocks, unsafe mutations or unverified major tests block final completion.
