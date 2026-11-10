# Redesign — session prompts

Copy one block per session. Every session starts by reading the handoff:
`_bmad-output/initiative-attendta/handoffs/redesign-visual-world.md`

The **never commit / never PR** rule is repeated in every prompt on purpose. Do not remove it.

---

## Common preamble (already inside each block)

> Read `_bmad-output/initiative-attendta/handoffs/redesign-visual-world.md` first.
> Hard rule: **never commit, push, open a PR, or create/switch/stash/reset a branch.** Local
> file edits only; read-only git is fine. Visual layer only: `.cshtml`, `Styles/**`,
> fonts/images, generated `wwwroot/css/app.css`. No controllers, services, data, routes, form
> attributes, JS contracts, or copy. Leave the existing uncommitted `AGENTS.md` and
> `_bmad/custom/*.toml` alone.

---

## Session A — Direction (human gate)

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

---

## Session B — Design system + shell + one tracer

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

---

## Session C — Lists

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

---

## Session D — Forms and dialogs

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

---

## Session E — Detail pages and My Events slots

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

---

## Session F — Special surfaces

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

---

## Session G — 8.12 acceptance

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

---

## Parallel runs

Sessions C–F can run in parallel **only after Session B is done**, and only on
disjoint files. These are shared and need one owner per round:

- `Styles/app.css`
- `Views/Shared/_Layout.cshtml`
- `Views/Shared/_Header.cshtml`
- `Views/Shared/_NavLinks.cshtml`

If two sessions need the same shared file, serialize them.
