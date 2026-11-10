---
target: Session E detail pages and My Events slots
total_score: 26
max_score: 40
na_heuristics: 
p0_count: 0
p1_count: 3
target_identity: "file:F:\\AttendTaBMAD\\ASI.Basecode.WebApp\\Views\\Event\\Details.cshtml"
target_fingerprint: "sha256:f084bd39c79da63c24c660315a1e4fe0b0cf5d1d0f35284a26296ce446c5a71f"
target_path: "F:\\AttendTaBMAD\\ASI.Basecode.WebApp\\Views\\Event\\Details.cshtml"
timestamp: 2026-11-09T17-21-20Z
slug: asi-basecode-webapp-views-event-details-cshtml
---
# Critique — Session E surface (detail pages + My Events slots)

Target: `ASI.Basecode.WebApp/Views/Event/Details.cshtml` plus the eight Session E views.
Method: dual-agent (A: general/design review · B: general/detector). Browser evidence was
collected in the orchestrator (sub-agents had no browser channel).

## Design specificity verdict

Partially authored. The participation half is unmistakably AttendTa; the browse half is
category-interchangeable.

- Faithful: `MyEvents` tabs, attendance table, certificate table, `Profile` panel rhythm and the
  officer roster all use the committed vocabulary (1px borders, `--color-border`, 8px panels,
  4px tinted tags with text, 48px rows, tabular figures).
- Drifted by omission: `Event/Details.cshtml:39` is still raw Tailwind —
  `rounded-lg border border-slate-200 bg-white p-6 mb-6`. The live border resolves to
  `oklch(0.929 0.013 255.508)` (slate-200), the one superseded family DESIGN.md forbids. Its three
  sub-headings are 14px (Headline is 16px) and capacity is `text-xl`.
- Drifted by substitution: `MemberEvents/Index` (192px cover cards in a 3-up grid, 0.72 ink scrim,
  white 16.8px title) and the `MemberEvents/Details` hero (288px, 0.68 scrim, 35.2px white title).
  That is the event-marketplace pattern, not this product's pattern. DESIGN.md: content is a
  full-width sheet, never a card grid; Display is 22px.

The tell: the same data is treated two ways — `Event/Index` is a table, `MemberEvents/Index` is a
photo grid; `Event/Details` is a sheet, `MemberEvents/Details` is a hero + split.

## Nielsen heuristics

| # | Heuristic | Score | Key issue |
|---|---|---|---|
| 1 | Visibility of system status | 3 | Roster Status cell prints the bare enum while every other status is a tinted tag; disabled "Complete event" hides its reason 800px below |
| 2 | Match system / real world | 3 | Rail and breadcrumb say "My Events", h1 says "Participation History" |
| 3 | User control and freedom | 3 | Dialogs are correct; breadcrumb never names the record |
| 4 | Consistency and standards | 2 | Count/pager inverted vs the reskinned lists; Published hidden on the feed, shown on the detail; 3px pass rule; 3px filter-tab radius |
| 5 | Error prevention | 3 | Required decline reason, consequence copy, server truth; full-event primary sits under a warning |
| 6 | Recognition over recall | 2 | Feed carries no "you are registered" state; roster name cell is the least weighted cell |
| 7 | Flexibility and efficiency | 2 | No roster search, one promotion at a time, full reload per tab |
| 8 | Aesthetic and minimalist | 2 | Three nested borders per registration; title/date/venue/status stated twice; 288px hero before the first task |
| 9 | Error recovery | 3 | The dossier retry loop (TempData summary + reopen + retained reason) is excellent |
| 10 | Help and documentation | 3 | Guidelines are real; the Manila-time disclosure appears in three placements/wordings |
| **Total** | | **26/40** | **Acceptable** |

## Cognitive load — 5 of 8 fail

Fail: single focus (288px hero, primary action in the aside that stacks last on mobile);
grouping (check-in control wrapped in `.row-actions`, pushed to the far right); visual hierarchy
(two 22px headings on one screen; member status at 14px mid-sentence while capacity is 24px);
working memory (no registration marker on the feed); progressive disclosure (a 168px QR expanded
inside every history row).
Pass: chunking, one-thing-at-a-time, minimal choices.

## Emotional journey

Peak is the pass — holder name, student number, scannable QR, download. That is the moment the
product's promise is cashed and it looks like a credential.
Valley: the sentence that matters ("Your registration is PENDING") is 14px mid-sentence while the
org's aggregate is 24px. The page shouts the org's number and mumbles the member's.
End is weak: the flow terminates in a pagination footnote with no "done" beat.
Dead end: after cancelling, the copy says what you cannot do and offers no next step.

## Strengths

1. The dossier failure-recovery loop reconstructs a human summary, reopens the exact dialog and
   restores the typed reason (`Event/Details.cshtml:9-17, 102, 116`).
2. Consequence copy in all six dialogs is written in domain terms and explains the waitlist rule
   better than the spec does.
3. Attendance and certificate rows always pair the tint with a text label and surface the reason —
   the status rule is honoured, not just declared.

## Priority issues

### P1 — `Event/Details` is one section short of reskinned, and it is the superseded palette
`Event/Details.cshtml:39-50`. Slate-200 border, 14px sub-headings, `text-xl` capacity, raw
`md:grid-cols-3`/`mt-6`/`mb-6` utilities. Fix: `.panel` + a labelled fact grid (micro label,
13.5px tabular value), capacity as the sanctioned Event-details KPI, three h2s at the system size,
utilities deleted.

### P1 — Nested frames and restated facts
`.panel.my-events-panel` → `.registration-card` → `.pass-card` → `.badge` is four frames; the same
title/date/venue/status appear twice per row. `.table-scroll` sits inside `.panel` on two tabs.
`dl.profile-details` draws a bordered tile per field inside a panel. Fix: one frame per level; the
registration becomes a row with a hairline separator; profile becomes definition rows.

### P1 — Certificate history puts a cobalt fill on every issued row
`_CertificateLinkPartial.cshtml:62` — `button-primary` per row. Six issued certificates = six
cobalt fills, against the One Accent Rule. Also `min-width: 52rem` with no sticky identity column.
Fix: row text links; keep at most one cobalt affordance on the page.

### P2 — Member event detail: the hero outranks the member
288px hero, 35.2px title, four equal-weight tags of which three are green, registration state at
14px mid-sentence, aside stacking after two panels on mobile. Fix: bounded cover band, ink title at
Display 22px, two tags (member state + one org fact), registration panel first in DOM order.

### P3 — Ordering and radius debt
Count before pager here vs pager before count in the reskinned lists; dossier count fused into the
timezone sentence; `.filter-tab` 3px radius; `.pass-card` 3px top rule over seven collapsed accent
classes; `notice-amber` used for a static fact.

## Persona red flags

**Alex (officer at the door):** "Registration roster" renders at 22px, same as the event title;
roster names have no initials tile or weight; every tab click and promotion is a full reload, one
participant at a time.

**Sam (accessibility):** certificate table forces 52rem with no sticky identity column; the feed
title's cobalt focus ring sits on a near-black scrim (~1.2:1); heading order jumps h1 → h3 on
My Registrations; `disabled` on the only primary button hides its explanation.

**Casey (390px):** register action is the fourth thing reachable; ~112px of a 358px content width
is consumed by nested padding; the certificate table is 720px inside a 358px column; the back link
is 40px against the 44px touch target.

## Minor observations

- `<p class="event-badges">` as a flex row of tags; wrong element.
- Event *type* rendered with `.badge`, so categories read as states.
- Capacity panel repeats Schedule and Venue already in the hero meta.
- Feed empty state explains the state but not the next step.
- `<nav aria-label="Breadcrumb">` whose only child is a back button, duplicating the shell.
- Approve and Decline both flatten to identical cobalt text links.
- Six dialogs emitted unconditionally on the dossier.
- Three placements/wordings for the Manila-time disclosure.
- Two empty-state treatments in one tab group.
- `.certificate-history::selection` is a dead rule.
- `.dossier-roster` writes a sentence into a column sized for a tag.

## Detector

`impeccable detect --json` over the nine views: `[]`, exit 0. The tool reports regex-only matching
for `.cshtml`, so this is an undercount, not a clean bill of health. No rule fired.

## Live measurements (orchestrator, 1233px)

- `Event/Details`: section border `oklch(0.929 0.013 255.508)`; h2 14px; capacity 20px.
- `MemberEvents/Details`: hero 288px, scrim `rgba(14,15,18,0.68)`, title 35.2px white.
- `MemberEvents/Index`: cover 192px, scrim `rgba(14,15,18,0.72)`, title 16.8px white, 2-up grid.
- `MyEvents/Registrations`: `.registration-card` padding 0, radius 8, nested in a 24px panel.
- No horizontal document overflow and no console errors on any of the six member pages.

## Questions to consider

1. If the cover is worth 288px and a 68% scrim, what is it doing for the member deciding whether to
   attend?
2. Why does the page know the org's number in 24px and the member's status in 14px?
3. Is the pass a document the member presents, or a property of the registration?
4. What is the 3px top border on `.pass-card` for, now that all seven accents collapse to one value?
5. `Published`, `Check-in open` and `Signup open` are all green — which "yes" matters?
