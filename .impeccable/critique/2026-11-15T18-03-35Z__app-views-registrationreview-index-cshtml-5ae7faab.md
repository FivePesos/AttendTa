---
target: registration review and dossier bulk approval surface
total_score: 33
max_score: 40
na_heuristics: 
p0_count: 0
p1_count: 1
target_identity: "file:C:\\Users\\Administrator\\source\\repos\\wigglebop25\\AttendTa-bmad\\ASI.Basecode.WebApp\\Views\\RegistrationReview\\Index.cshtml"
target_fingerprint: "sha256:97e9ac63221a7f4fa7bbae1b9428d1d473d092440eac03ba7930de093c0d335d"
target_path: "C:\\Users\\Administrator\\source\\repos\\wigglebop25\\AttendTa-bmad\\ASI.Basecode.WebApp\\Views\\RegistrationReview\\Index.cshtml"
timestamp: 2026-11-15T18-03-35Z
slug: app-views-registrationreview-index-cshtml-5ae7faab
closed: true
---
# Critique: registration review and dossier bulk approval

Method: degraded, single-context (no sub-agent tool exposed in this session); Assessment A ran inline, Assessment B ran the bundled detector. Browser visualization skipped: no browser automation tool is exposed.

## Design Health Score

| # | Heuristic | Score | Key Issue |
|---|-----------|-------|-----------|
| 1 | Visibility of System Status | 3 | Count, disabled action, per-record badges and a live result panel; a POST re-render returns focus to the top of the page. |
| 2 | Match System / Real World | 3 | Confirmed and Waitlisted are named as different outcomes; the dialog calls them "records" where the surface noun is "registrations". |
| 3 | User Control and Freedom | 4 | Deselect all, Cancel, individual Approve/Decline/Cancel retained, selection clears on every navigation. |
| 4 | Consistency and Standards | 3 | The result panel sits below the toolbar here but above it on Membership Account Requests; the dialog list is labelled "Selected requests". |
| 5 | Error Prevention | 4 | Only pending rows are selectable, the dialog names targets and consequences, the bound is enforced, and the server re-checks every record. |
| 6 | Recognition Rather Than Recall | 3 | The dialog repeats names and the consequence; the cross-event queue shows the event per row and in the dialog list. |
| 7 | Flexibility and Efficiency | 4 | Batch plus individual paths, keyboard-reachable bar and dialog, select-all-on-page. |
| 8 | Aesthetic and Minimalist Design | 3 | The bar is quiet and secondary; the consequence sentence is long for a dialog. |
| 9 | Error Recovery | 4 | Per-record refusals keep the existing wording, nothing rolls back, refused rows stay pending. |
| 10 | Help and Documentation | 2 | Nothing explains what Waitlisted means beyond the dialog consequence, and nothing explains a mixed result after the fact. |
| **Total** | | **33/40** | **Good** |

## Design Specificity Verdict

**LLM assessment:** The surface is authored for this product: the roster keeps its density, the status tags keep their tinted-label vocabulary, and the batch controls deliberately stay secondary so the single cobalt action per screen is not multiplied. The batch pattern itself is category-standard, which is correct for an Operate surface. The one generic leak is copy: the dialog's selected-record list is labelled "Selected requests", which is the membership surface's noun, not this one.

**Deterministic scan:** `impeccable detect --json` over the two views and the three shared bulk partials returned `[]`, exit 0. No findings, no false positives to discount.

**Visual overlays:** none. No browser automation is exposed, so no overlay was injected and no live page was inspected; this run's evidence is source plus the detector.

## Overall Impression

The batch reads as an extension of the roster rather than a second workflow, which is the hard part. The remaining gaps are all small and all about the shared components speaking the wrong surface's language or sitting in a slightly different place than their sibling.

## What's Working

- The bar's action is secondary and starts disabled, so the screen still has one primary action and the count carries the state.
- The result panel distinguishes Confirmed from Waitlisted with separate words and tints, which keeps the honest capacity story intact.
- The dossier hides the surface entirely on decided tabs and started events, so the control never appears where it cannot act.

## Priority Issues

- **[P1] The confirmation dialog's selected-record list is labelled "Selected requests".** Why it matters: a screen reader user hears the membership surface's noun at the moment they are confirming who to approve, which is exactly when the target identity must be unambiguous. Fix: give `BulkSelectionViewModel` an optional selected-list label and set "Selected registrations" on both registration surfaces. Suggested command: `/impeccable polish`.
- **[P2] The result panel sits below the toolbar on the review queue and dossier, above it on Membership Account Requests.** Why it matters: the same batch outcome appears in two different places across sibling surfaces, so readers learn the pattern twice. Fix: one rule, the result is the first element of the list block. Suggested command: `/impeccable polish`.
- **[P2] The review queue's consequence sentence says "the event's live capacity" on a cross-event queue.** Why it matters: the queue spans events, so "the event" has no referent and the sentence reads as if the batch is scoped to one event. Fix: "its event's live capacity" on the queue, "this event's" on the dossier. Suggested command: `/impeccable clarify`.
- **[P3] The dialog copy says "Up to 25 records at a time".** Why it matters: "records" is generic on a surface whose noun is registrations. Fix: keep the shared default and pass the noun through the same label the surface already supplies. Suggested command: `/impeccable clarify`.
- **[P3] The dossier's bulk wrapper indentation drifts from the surrounding view.** Why it matters: source only, but it makes the block look appended rather than part of the section. Fix: align the wrapper with its siblings. Suggested command: `/impeccable polish`.

## Persona Red Flags

**Alex (Power User):** No keyboard accelerator for select-all or approve; both are reachable by Tab but a 25-row page costs a bar visit. Acceptable for a console, worth noting.

**Sam (Accessibility-Dependent):** The dialog's list group is named "Selected requests" on a registration surface. The heading takes focus and the summary and consequence are described, which is otherwise correct.

**Riley (Stress Tester):** A batch taken on a started event from the review queue is accepted and then refused row by row. That is the specified behaviour, but the queue gives no advance warning while the dossier does.

## Minor Observations

- "3 of 3 succeeded" counts waitlisted rows as successes. Accurate, since the approval succeeded, but the word can be misread next to a Waitlisted badge.
- No CSS change is needed; `table-sticky` is not defined in the stylesheet, so the dossier's select column keeps the shared 44px target.
- Empty-state colspans track the selection column on both surfaces.

## Questions to Consider

- Should the queue warn before a batch on an event that has already started, the way the dossier hides the surface?
- Is "succeeded" the right word for a batch where some rows were waitlisted rather than confirmed?
