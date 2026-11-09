---
id: 12
type: story
title: "Final frontend walkthrough targeted refactor and Impeccable polish"
parent: epic-release-acceptance
covers: [R2, R6]
after: [11]
hitl: false
risk: medium
tracker_id: "153"
remote: "https://github.com/rainyDayssss/AttendTa/issues/153"
tracker_status: backlog
---

# Final frontend walkthrough targeted refactor and Impeccable polish

## Description

Walk the complete real integrated frontend against docs/DESIGN.md and the 8.15 demo rehearsal findings: all 20 core screens, distinct role navigation, coherent state/copy/actions, centered dialogs, keyboard focus, responsive layouts, long content, error/retry and PDF/QR presentation. Refactor only concrete presentation duplication/usability findings; preserve ready backend behavior. Use the impeccable skill for implementing fixes, rendered critique and polish before review, and run its detector on changed UI targets.

## Requirements

Verify: Desktop and phone walkthrough evidence shows no dead-end required routes, hidden primary actions or demo data outcomes; important keyboard/dialog and 200%-zoom checks pass, and all critique findings are fixed or recorded as explicit release blockers. Final backend and test-only tickets do not inherit this design-tool requirement.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-release-acceptance/epic-release-acceptance.md
- `DESIGN.md` at the repository root — visual authority ("The Workspace Standard"). `docs/design.md` is superseded and kept only as an anti-reference.
- _bmad-output/initiative-attendta/delivery-queue.md
- _bmad-output/initiative-attendta/finale-test-cases.md

## Notes

- Frontend only: impeccable is required for implementation, rendered critique and final polish; inspect representative desktop/1024/768/390px surfaces and critical keyboard/zoom states. Use manual evidence rather than introducing broad screenshot automation in the timebox.

### Session C carry-over — list archetype

Session C reskinned the seven list screens (Member, EventType, Venue, Event,
AdminAccounts, ActivityLog, AccountRequest) plus `_Pager` and `_ResultCount`.
These gaps were found there and are carried into this walkthrough. Items 1–2 are
owner decisions; 3–6 are implementation work; 7 is a Session G obligation.

**Owner decisions (not yet signed off)**

1. **Duplicate event action.** Event rows carry five actions — Cover, View
   dossier, Edit, Cancel, Publish/Manage event — and "View dossier" and "Manage
   event" open the same page. Decide whether to collapse them into one dossier
   action. This is a behavior change, not a reskin, so it needs explicit owner
   sign-off before any edit.
2. **Missing loading states.** Event, AdminAccounts, ActivityLog and
   AccountRequest have no loading state (no `data-list-status` /
   `list-feedback.js`). Only Member, EventType and Venue have it. Decide whether
   to add it to the other four.

**Implementation work**

3. **Sticky edge columns — done.** Added a `.data-table--sticky-edges` modifier
   that pins the first and last column at the same `≤1023px` breakpoint Member
   uses, without inheriting `.roster-table`'s `nowrap` and `1180px` floor.
   Applied to Event, AccountRequest and all three Venue tables. Verified in the
   running app: first and last cells compute `position: sticky`, and the Event
   table scrolls 311px and AccountRequest 90px with the edges pinned.
4. **Result-count placement — done.** The rule is now written into root
   `DESIGN.md` under *Tables / Rosters*: pager below the table, count directly
   below the pager, real `TotalCount`, never above the table. AccountRequest now
   uses `_ResultCount` instead of its hand-rolled paragraph. MyEvents, CheckIn
   and Reports still render the count first; those belong to Sessions E and F,
   not this archetype.
5. **AccountRequest double empty state — done.** The hand-rolled `aria-live`
   paragraph was replaced by `_ResultCount`, which renders nothing at zero, so
   the table row is the only empty state. Side effect worth knowing: an empty
   result no longer gets a live-region announcement, which matches Member.
6. **Event empty-state copy — done.** Now reads "No events yet. Use "Create
   draft event" to create the first one.", matching the EventType and Venue
   pattern.

**Session G obligation**

7. **Visual walkthrough — partial.** An authenticated walkthrough now exists,
   but only at the embedded browser's fixed 1296px viewport (`outerWidth` is 0,
   so `resizeTo` cannot reach the breakpoints) and without screenshots, which
   need a visible desktop window. Verified at that width: all seven lists
   render with zero console errors, zero page-level horizontal overflow, and a
   result count on every screen; the AccountRequest empty state renders exactly
   one message. Still unverified: 1024, 768, 390 and 200% zoom, and any visual
   judgement that needs a screenshot. Session G must finish those.

Re-checked against the working tree and the running app after Session C. Items 1
and 2 remain open owner decisions; 3, 4, 5 and 6 are applied; 7 is partly
closed. `dotnet build`, `npm run css:build` and a signed-in pass over all seven
lists pass. Evidence anchors: `Views/Event/Index.cshtml` (actions, empty copy),
`Views/{Member,EventType,Venue}/Index.cshtml` (the only three with
`data-list-status`), `Styles/app.css` `.roster-table` and
`.data-table--sticky-edges`, `Views/AccountRequest/Index.cshtml` (pager,
`_ResultCount`, empty row).
