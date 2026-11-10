---
version: 1
slug: "asi-basecode-webapp"
primary_target: "ASI.Basecode.WebApp"
related_targets: []
---

# Surface: AttendTa web application

## Scope and visitor mode

Operate. The whole application's visual layer: ~35 screens, 20 core screens, 10 shared
partials. Routes, page structure, dialogs, fields, validation, permissions, state
transitions, copy, and `data-*` hooks stay exactly as they are. Only markup wrappers,
class names, `Styles/**`, fonts and images change.

## Audience and job

Officers and administrators run the lifecycle from a management shell: review requests,
maintain members, event types and venues, publish events, review registrations, run
check-in, read reports. Members browse events, hold passes, and read their own history.

Primary scene: a projector or panel walkthrough of the full officer lifecycle, so hierarchy
and state clarity at distance outrank novelty. The door (camera check-in on a small laptop)
and the phone (member browsing) must still hold.

## Chosen direction

**The Workspace Standard** — the category standard, taken as the standing exit and executed
at full fidelity. No world, no metaphor, no irony. The craft is in alignment, density,
complete states, and restraint.

Break list against the superseded identity, all enforced:

| Axis | Superseded | This system |
|---|---|---|
| Ground | `#F8FAFC` pale slate | `#F5F5F6` neutral |
| Ink | `#090D16` / `#0B1120` navy | `#0E0F12` graphite |
| Accent | amber `#D97706` | cobalt `#1D4ED8`, actions only |
| Type | Outfit + JetBrains Mono | Inter only, tabular figures |
| Radius | 6 / 10 / 16 | 4 / 8 |
| Shell | top nav row + dark identity strip | left rail + top bar |
| Status | filled rounded badge | 4px tinted tag, uppercase, no dot |
| Depth | subtle panel shadows | 1px borders, no shadow except dialogs |

## Constraints

- `wwwroot/img/attendta-logo.jpg` and the "University of Cebu" line are fixed.
- Light theme only. No dark mode, no theme toggle, no new preferences.
- WCAG 2.1 AA: 4.5:1 normal text, visible focus, keyboard-operable dialogs, status never
  carried by colour alone.
- Usable at 1440, 1024, 768, 390 px and at 200% zoom.

## Data honesty

The canon look leans on summary tiles and a create button. This product does not have them
on every screen, so they are dropped rather than faked:

- **Members list:** no create action (members are created from approved account requests),
  no filter panel (search plus status keys only), and no per-status counts
  (`PagedResult` exposes `TotalCount` only). The page shows the total count and nothing else.
- **KPI cards** appear only where real aggregates exist: Reports/Participation, Event
  details capacity, Check-in counts. Never a figure the service cannot produce.

## Unresolved

- Token values land in `ASI.Basecode.WebApp/Styles/app.css` `@theme` in Session B; DESIGN.md
  is re-extracted from the built world at finish.
- The certificate PDF template is not owner-pinned; it may be reskinned later, but its
  signatory and structure stay.

## Direction contract

THESIS: A campus operations console executed at the category's best — dense tables, one
cobalt action, hairline structure — refusing both the superseded slate-and-amber identity
and any decorative world.

OWN-WORLD: Neutral ground `#F5F5F6`, white panels, graphite ink `#0E0F12`, one cobalt
`#1D4ED8` action accent, semantic status tags at 4px; Inter only with tabular figures and no
mono family; 4px controls and 8px panels; 1px borders, no shadow except dialogs.

STORY: The officer recognises every pattern instantly and spends no attention on the
interface; the craft shows in alignment, density, and complete states. The member reads the
same calm system without relearning it.

FIRST VIEWPORT: A left rail with grouped destinations, a 64px top bar with breadcrumb, the
page title with no invented primary action, search and status keys, the result count, then a
48px-row table with tinted status tags.

FORM: The category standard (canon), taken as the standing exit — not a position on the
grounded list; seed:01e7be653fe4eea9.

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review,
the verdict, DESIGN.md, and every shipping raster carrying its provenance.
