---
title: 'Full visual redesign — new visual world, identical functionality'
type: 'redesign'
created: '2026-11-09'
status: 'ready'
baseline_revision: '17277ff514605c9ffd269f6a1ca5df412e469a79'
baseline_ref: 'upstream/dev'
working_branch: 'feature/51-check-in-refactor-sweep'
ticket: '8.12 / #153'
route: 'impeccable new-work (replacement world)'
risk: 'high'
---

# Handoff — AttendTa visual redesign

## Read this first

> **NEVER commit. NEVER push. NEVER open a pull request. NEVER create, switch, delete,
> tag, stash, reset, or rebase a branch.**
>
> Do not run `git add`, `git commit`, `git push`, `git stash`, `git checkout`, `git switch`,
> `git reset`, `git rebase`, or `gh pr create`. Read-only git (`git status`, `git log`,
> `git diff`, `git show`) is allowed.
>
> All work is **local file edits only**. The user reviews and commits by hand. If you think a
> commit is needed, stop and ask — do not do it.

This file is both the contract and the per-session runbook. Read all of it once, then use the
session block you were given.

## Session prompts

Copy one block per session. Every block repeats the never-commit rule on purpose; do not remove
it. The human gates are **Session A** (direction) and **Session G** (acceptance).

### Session A — Direction (human gate)

```text
Read _bmad-output/initiative-attendta/handoffs/redesign-visual-world.md first.

This is Session A: direction only. Do not build screens yet.

1. Run /impeccable shape for AttendTa. Ask me the discovery questions and wait.
2. Then run the replacement-world flow:
   /impeccable Redesign AttendTa's visual world. Treat docs/design.md as an
   anti-reference and replace it. Keep every route, page, dialog, field,
   validation and behavior identical — visual layer only.
3. If it presents a direction decision page, open it and let me pick. Do not
   start the sweep.
4. Commit the direction contract and write the new DESIGN.md (replacement, not
   an edit of the old look).
5. Update only the visual-authority line in AGENTS.md (~line 154); preserve my
   existing uncommitted edits in that file.

Stop after the direction contract + new DESIGN.md. Never commit, push, or open a
PR. Report: the chosen direction, the new DESIGN.md path, and every file changed.
```

### Session B — Design system + shell + one tracer

```text
Read _bmad-output/initiative-attendta/handoffs/redesign-visual-world.md first.
This is Session B. Use the new DESIGN.md from Session A as the authority.

1. Implement the new tokens in ASI.Basecode.WebApp/Styles/app.css (@theme and
   the base/components layers).
2. Rebuild the shell: Views/Shared/_Layout.cshtml, _Header.cshtml, _NavLinks.cshtml.
3. Rebuild ONE tracer page end to end — Account/Login or MemberEvents/Index,
   whichever proves the most.
4. Run: dotnet build "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"
   then: cd "ASI.Basecode.WebApp"; npm run css:build
5. Look at the tracer at desktop and 390px.

Do not sweep other screens. Never commit, push, or open a PR. Report changed
files, build result, and any visual gaps.
```

### Session C — Lists

```text
Read _bmad-output/initiative-attendta/handoffs/redesign-visual-world.md first.
This is Session C. Authority: the new DESIGN.md. Do not change tokens or the
shell unless a list genuinely requires it — if it does, stop and report.

Reskin the list screens: Member/Index, EventType/Index, Venue/Index,
Event/Index, AdminAccounts/Index, ActivityLog/Index, AccountRequest/Index,
plus Shared/_Pager and Shared/_ResultCount.

Keep filters, search, paging, status toggles, empty/loading/error states and
every data-* hook intact. Run /impeccable critique then /impeccable polish on
each list. Run dotnet build and npm run css:build.

Never commit, push, or open a PR. Report changed files and any gaps.
```

### Session D — Forms and dialogs

```text
Read _bmad-output/initiative-attendta/handoffs/redesign-visual-world.md first.
This is Session D. Authority: the new DESIGN.md.

Reskin the form and dialog surfaces: Event/Create|Edit|_Form,
EventType/Create|Edit|_Editor|_Form, Venue/Create|Edit|_Editor|_Form,
Member/Edit|_Editor|_Form, and Shared/_RecordDialog.

Keep field names, ids, asp-* attributes, validation attributes, field order, and
the dialog's focus containment / Escape / backdrop behavior intact. Run
/impeccable critique then /impeccable polish. Run dotnet build and npm run css:build.

Never commit, push, or open a PR. Report changed files and any gaps.
```

### Session E — Detail pages and My Events slots

```text
Read _bmad-output/initiative-attendta/handoffs/redesign-visual-world.md first.
This is Session E. Authority: the new DESIGN.md.

Reskin: Event/Details, MemberEvents/Index|Details, Profile/Index,
MyEvents/Index, and the four slot partials (_RegistrationsTab, _PassPartial,
_AttendanceHistoryPartial, _CertificateLinkPartial).

Keep tabs, slot contracts, and data-* hooks intact. Run /impeccable critique
then /impeccable polish. Run dotnet build and npm run css:build.

Never commit, push, or open a PR. Report changed files and any gaps.
```

### Session F — Special surfaces

```text
Read _bmad-output/initiative-attendta/handoffs/redesign-visual-world.md first.
This is Session F. Authority: the new DESIGN.md.

Reskin: CheckIn/Index, Passes/Index, Reports/Participation,
RegistrationReview/Index, CertificateLedger/Index|Viewer,
MyCertificates/Unavailable, Home/Index, AccountRequest/Request|Submitted,
Account/AccessDenied, Event/Cancel|Cover, and Shared/Error.

Keep scanner camera/manual behavior, QR pass rendering, report data, certificate
viewer behavior, and all JS hooks intact. Run /impeccable critique then
/impeccable polish. Run dotnet build and npm run css:build.

Never commit, push, or open a PR. Report changed files and any gaps.
```

### Session G — 8.12 acceptance (human gate)

```text
Read _bmad-output/initiative-attendta/handoffs/redesign-visual-world.md first.
This is Session G: close 8.12.

1. Walk every screen against the new DESIGN.md at desktop, 1024, 768, 390 and
   200% zoom. Fix drift; run /impeccable critique + polish on anything off.
2. Verify zero functional diff: the diff may touch only Views/**, Styles/**,
   fonts/images, and generated wwwroot/css/app.css.
3. Run dotnet build, npm run css:build, and npm run test:js.
4. Update the local 8.12 ticket file to the re-scoped acceptance and record the
   walkthrough evidence there. Do not edit GitHub #153 unless I ask.

Never commit, push, or open a PR. Report the acceptance table, changed files,
and any release blockers.
```

### Parallel runs

Sessions C–F can run in parallel **only after Session B is done**, and only on
disjoint files. These are shared and need one owner per round:

- `Styles/app.css`
- `Views/Shared/_Layout.cshtml`
- `Views/Shared/_Header.cshtml`
- `Views/Shared/_NavLinks.cshtml`

If two sessions need the same shared file, serialize them.

## Why this handoff exists

The project owner does not like the current frontend. The goal is to **replace the visual
world** — new typography, color, spacing, surfaces, motion, component treatment — while every
route, page, dialog, field, validation rule, permission, and behavior stays exactly the same.
This is a **redesign**, not a polish or a refinement. The old look is evidence of what the
product is, not authority over what it becomes.

The trap: BMAD agents follow written authority. Today that authority is `docs/design.md`
(plus `AGENTS.md:154` and `_bmad-output/initiative-attendta/ux-attendta/DESIGN.md`). If you
ask for "improvements" without replacing those, you will polish the discarded look. Replace
the authority first, then build against it.

## Ticket: 8.12 / #153 is the vehicle

The redesign is delivered under **8.12 — "Final frontend walkthrough targeted refactor and
Impeccable polish"** (`#153`, queue rank 520, finale phase). Do not create a parallel epic or
a second frontend ticket. 8.12 becomes "replace the visual world, then walk it and polish it."

**The current ticket text contradicts the redesign and must be re-scoped before work starts.**
It currently says:

> Follow `docs/DESIGN.md`, preserve the existing Razor/Tailwind identity …
> Refactor only concrete presentation duplication/usability findings; preserve ready backend
> behavior.

That is a refinement instruction. Re-scope it to:

- **Implement the replacement visual world** recorded in the new `DESIGN.md`; the old
  `docs/design.md` identity is an anti-reference, not a constraint.
- **Replace the visual layer across every screen**, then walk the result and fix concrete
  presentation, usability, keyboard, and responsive findings.
- **No functional change:** routes, dialogs, fields, validation, permissions, state
  transitions, and copy stay fixed. A diff touching Controllers, Services, Data, routes, or JS
  contracts is out of scope.
- Keep the ticket's existing acceptance surface: all 20 core screens, distinct role
  navigation, coherent state/copy/actions, centered dialogs, keyboard focus, responsive
  layouts, long content, error/retry, and PDF/QR presentation.

**Where to change it:**

- Local ticket file:
  `_bmad-output/initiative-attendta/tickets/epic-release-acceptance/tickets/story-final-frontend-walkthrough-targeted-refactor-and-impeccable.md`
  — update the Description and Requirements to the re-scope above.
- GitHub issue `#153` — the tracker copy. Update it with an **issue edit only if the owner
  asks**; an issue edit is not a commit or a PR. Never open a PR to change the ticket.

**Acceptance for 8.12 (all must be true before it closes):**

- New `DESIGN.md` is the visual authority and `AGENTS.md` points at it.
- All ~35 screens / 20 core screens and the 10 shared partials render on the new system.
- Zero functional diff.
- Desktop, 1024, 768, and 390 px walkthroughs show no dead ends, hidden primary actions, or
  demo-data outcomes; keyboard/dialog and 200% zoom checks pass.
- Critique findings are fixed or recorded as explicit release blockers.
- `dotnet build` clean and `npm run css:build` regenerated.
- **Nothing committed, pushed, or opened as a PR by this session.**

**Sequencing:** 8.12 runs before 8.10 (#83), 8.5 (#78), 8.4 (#77), 8.6 (#79), 8.7 (#80),
8.8 (#81), and 8.9 (#82). 8.9 remains last.

## Hard rules

**Never commit, push, or PR.** See the block above.

**Visual layer only.** A redesign diff may touch:

- `ASI.Basecode.WebApp/Views/**/*.cshtml` — markup, class names, wrapper elements
- `ASI.Basecode.WebApp/Styles/**` — `app.css` `@theme` tokens, `features/*.css`
- `ASI.Basecode.WebApp/wwwroot/fonts/**`, `wwwroot/img/**`, `wwwroot/assets/**` — new assets
- `wwwroot/css/app.css` — **generated only**, via `npm run css:build`

A redesign diff must **not** touch:

- `ASI.Basecode.WebApp/Controllers/**`
- `ASI.Basecode.Services/**`, `ASI.Basecode.Data/**`, `ASI.Basecode.Resources/**`
- `ASI.Basecode.WebApp/Startup.*.cs`, `Program.cs`, `appsettings*.json`
- `ASI.Basecode.WebApp/wwwroot/js/**` behavior/contracts (styling-only JS is fine)
- Route names, action names, form `name`/`id`/`asp-*` attributes, `data-*` hooks,
  antiforgery tokens, validation attributes, or field order
- Copy, labels, and error text (unless the user explicitly asks)

If a visual change seems to require a controller, service, model, or route change, **stop and
report it** instead of making it.

**Do not disturb existing uncommitted work.** The working tree already contains:

- `M AGENTS.md`
- `?? _bmad/custom/bmad-build-auto.toml`
- `?? _bmad/custom/bmad-build.toml`
- `?? _bmad/custom/bmad-code-review.toml`

Leave these alone. Do not revert, delete, or commit them.

## Current repository state (2026-11-09)

- Repo: `F:\AttendTaBMAD`
- Remotes: `origin` = `Wynnwrath/AttendTav2` (fork), `upstream` = `rainyDayssss/AttendTa` (team)
- `dev` is level with `upstream/dev` at `17277ff` (PR #217, 8.11 review record)
- Checked-out branch: `feature/51-check-in-refactor-sweep` at `2cf6ac3`
  (ticket 4.9, already merged into `upstream/dev`)
- No open pull requests
- Open tickets: #153 (8.12), #83 (8.10), #78 (8.5), #77 (8.4), #79 (8.6), #80 (8.7),
  #81 (8.8), #82 (8.9) — all finale phase, all `backlog`

## Read before editing

1. `PRODUCT.md` — product truth, users, constraints (impeccable schema, already present)
2. `docs/design.md` — the **old** visual authority. Read it as an anti-reference.
3. `AGENTS.md` line ~154 — the pointer that makes agents obey the old design
4. `_bmad-output/initiative-attendta/ux-attendta/DESIGN.md` — BMAD's UX copy of the old world
5. `ASI.Basecode.WebApp/Styles/app.css` — the real design system (`@theme` tokens + base/components)
6. `ASI.Basecode.WebApp/Styles/features/*.css` — 14 per-surface style files
7. `ASI.Basecode.WebApp/Views/Shared/_Layout.cshtml`, `_Header.cshtml`, `_NavLinks.cshtml` — the shell
8. `ASI.Basecode.WebApp/Views/Shared/_RecordDialog.cshtml`, `_Pager.cshtml`,
   `_EventSelector.cshtml`, `_CheckInControl.cshtml` — shared primitives
9. `.opencode/skills/impeccable/SKILL.md` and `reference/new-work.md` — the redesign contract

## Surface inventory

63 `.cshtml` files, but ~35 screens. Design by archetype, not by file.

| Archetype | Screens |
|---|---|
| Auth | `Account/Login`, `Account/AccessDenied` |
| Lists (7) | `Member/Index`, `EventType/Index`, `Venue/Index`, `Event/Index`, `AdminAccounts/Index`, `ActivityLog/Index`, `AccountRequest/Index` |
| Forms (4 archetypes) | `Event/Create|Edit`, `EventType/Create|Edit`, `Venue/Create|Edit`, `Member/Edit` |
| Detail / dossier (4) | `Event/Details`, `MemberEvents/Details`, `CertificateLedger/Viewer`, `Profile/Index` |
| My Events | `MyEvents/Index` + `_RegistrationsTab`, `_PassPartial`, `_AttendanceHistoryPartial`, `_CertificateLinkPartial` |
| Special (5) | `Home/Index`, `CheckIn/Index`, `Reports/Participation`, `Passes/Index`, `RegistrationReview/Index` |
| Certificates (3) | `CertificateLedger/Index`, `Certificates/CertificateTemplate` (PDF), `MyCertificates/Unavailable` |
| Misc | `AccountRequest/Request`, `AccountRequest/Submitted`, `Event/Cancel`, `Event/Cover`, `Shared/Error` |

Shared partials: `_Layout`, `_Header`, `_NavLinks`, `_Pager`, `_RecordDialog`, `_EventSelector`,
`_CheckInControl`, `_DeactivationPreview`, `_ResultCount`, `_ValidationScriptsPartial`.

## The workflow

Use the installed `/impeccable` skill. It explicitly supports replacement worlds:

> Refinement preserves; redesign replaces. … choose a replacement world in `new-work` and
> replace DESIGN.md.

### 1. Shape (discovery, no code)

```text
/impeccable shape
```

Answer the discovery interview: who uses this, in what scene, what must stay untouched,
what would make a polished result feel wrong. Do not choose colors or CSS values yet.

### 2. Replace the visual world

```text
/impeccable Redesign AttendTa's visual world. Treat docs/design.md as an
anti-reference and replace it. Keep every route, page, dialog, field,
validation and behavior identical — visual layer only.
```

- Say **"redesign"** and **"replace DESIGN.md"**; without that the skill treats the current
  design as incumbent truth and polishes it.
- If the owner pins a direction, state it and skip the direction roll — a pinned direction
  beats the roll.
- Otherwise the `new-work` flow runs `concept-seed`, presents a decision page, and the owner
  picks the new world.
- Do not write artifact code before the direction is committed.

### 3. Record and extract

```text
/impeccable document     # regenerate DESIGN.md from the built world
/impeccable extract      # pull tokens/components into the design system
```

Write the new tokens into `Styles/app.css` `@theme` and the per-surface files in
`Styles/features/`.

### 4. Rebuild the shell and one tracer

Do the shell (`_Layout`, `_Header`, `_NavLinks`) plus **one** high-traffic page (login or the
member events feed) end to end. Look at it in the browser. Do not sweep until this looks right.

### 5. Sweep by archetype

Per archetype, run:

```text
/impeccable critique <page>
/impeccable polish <page>
```

Use `bolder`, `quieter`, `typeset`, `layout`, `colorize`, `animate` for targeted work.
Use `live` or `generate` to iterate variants in the browser.

Suggested order: shell + auth → lists → forms/dialogs → detail/dossier → My Events slots →
check-in → certificates → reports → admin → misc.

### 6. Update the authority pointers

After the new `DESIGN.md` exists, update:

- `AGENTS.md` line ~154: point at the new visual authority
- `_bmad-output/initiative-attendta/ux-attendta/DESIGN.md` (or mark superseded)
- `_bmad-output/initiative-attendta/delivery-queue.md` where it names `docs/DESIGN.md`
- Mark `_bmad-output/initiative-attendta/ux-attendta/imports/prototype-assets/` and the
  `F:/AttendTa` prototype as **superseded visual evidence**

Do not rewrite the old `docs/design.md` history; create the replacement and repoint.

## Verification (no commit)

```powershell
dotnet build "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"
cd "ASI.Basecode.WebApp"; npm run css:build
```

- `wwwroot/css/app.css` is regenerated, never hand-edited.
- Run `npm run test:js` if JS-adjacent classes changed.
- Look at each changed page at desktop and phone widths.
- Existing CI must stay green, but do not claim a CI result for unpushed work.

## Definition of done

8.12's acceptance list above is the definition of done. In file terms:

- New visual world recorded in `DESIGN.md` (replacement, not an edit of the old look)
- All ~35 screens and 10 shared partials reskinned
- Zero functional diff: no controller/service/data/route/field/JS-contract changes
- `dotnet build` clean, `npm run css:build` regenerated
- `AGENTS.md` and the delivery queue point at the new authority
- Handoff back to the owner with the list of changed files and any visual gaps
- **Nothing committed, pushed, or opened as a PR**

## Open questions for the owner

1. Do you want the direction roll, or is there a pinned direction (era, material, palette,
   type voice, reference products)?
2. Who is the demo audience and in what scene (laptop at the door, phone between classes,
   projector demo)?
3. What must not change beyond functionality (logo, University of Cebu identity, certificate
   template, brand color)?
4. Is a light-only theme still required, or is dark mode in scope now?
5. Do you approve re-scoping 8.12 / #153 from "preserve the existing identity" to "implement
   the replacement world, then walk and polish it"? (The handoff assumes yes.)
