---
target: membership-request bulk selection (AccountRequest queue)
total_score: 22
max_score: 40
na_heuristics: 
p0_count: 0
p1_count: 3
target_identity: "file:C:\\Users\\Administrator\\source\\repos\\wigglebop25\\AttendTa-bmad\\ASI.Basecode.WebApp\\Views\\AccountRequest\\Index.cshtml"
target_fingerprint: "sha256:eb4ccb2a020afdc14a96feeb54ed83cf2be1a5ee0d16eac2af0a802fb706cf8a"
target_path: "C:\\Users\\Administrator\\source\\repos\\wigglebop25\\AttendTa-bmad\\ASI.Basecode.WebApp\\Views\\AccountRequest\\Index.cshtml"
timestamp: 2026-11-15T16-21-39Z
slug: webapp-views-accountrequest-index-cshtml-3c4f4163
---
# Critique — Membership request bulk selection (AccountRequest queue)

Method: dual-agent (A: ses_e2011c714ffeLkG9B1ub6O22rv · B: ses_e2011c712ffenbNs2pJ2VcRZPF)

Target: the bulk-selection surface on `ASI.Basecode.WebApp/Views/AccountRequest/Index.cshtml` — `_BulkSelectionBar.cshtml`, `_BulkConfirmDialog.cshtml`, `_BulkResultPanel.cshtml`, `Styles/features/bulk-actions.css`, `wwwroot/js/features/bulk-selection.js`. Mode: Operate.

Browser visualization was not available in this session, so Assessment B ran the bundled detector against the four markup files only: **0 findings, exit 0 on every file**, after a probe confirmed the engine parses Razor (`@model`, `asp-*`, inline styles). The engine's only repo-level finding is `overused-font` on Inter in `app.css`, which is the documented direction, not a defect.

## Design Health Score

| # | Heuristic | Score | Key Issue |
|---|---|---:|---|
| 1 | Visibility of System Status | 3 | Count, disabled states and the indeterminate header are exact, but the selected row itself had no visual state — the only confirmation was a 16px tick. No busy state beyond the button label. |
| 2 | Match System / Real World | 3 | "Select all on this page" is literally true and the dialog names real people. "0 selected" drops the noun the dialog uses; "Deselect all" sits beside the toolbar's "Reset". |
| 3 | User Control and Freedom | 2 | Cancel, Escape and per-row untick exist, but the dialog is read-only: a wrong name means Cancel → re-find → untick → reopen. Paging silently destroys the selection. |
| 4 | Consistency and Standards | 2 | Reuses `.button`/`.badge`/`.task-dialog`/`.notice` well, but broke the One Accent Rule (bulk primary plus one cobalt Approve per pending row) and stacked a third panel weight above the table. |
| 5 | Error Prevention | 3 | Named-list confirmation, server-side bound, per-record guards, disabled submit at zero. The bound is stated only inside the dialog. |
| 6 | Recognition Rather Than Recall | 1 | Weakest axis. Selection lived only in checkboxes; the officer had to rescan a 25-row table to remember what was ticked. |
| 7 | Flexibility and Efficiency | 2 | Select-all-on-page is a real accelerator, but no range select, no keyboard select-all, no cross-page selection, no "select all matching". |
| 8 | Aesthetic and Minimalist Design | 2 | The bar rendered on every tab, including tabs where nothing is selectable, showing a zero count and three dead buttons. |
| 9 | Error Recognition and Recovery | 2 | Per-record failure lines are good, but the message carried no next step and the bulk error notice had no action while the neighbouring decision notice offers "Refresh the queue". |
| 10 | Help and Documentation | 2 | The consequence and bound are explained inline — genuinely good. Nothing explained that selection is page-scoped and ephemeral. |
| **Total** | | **22/40** | **Acceptable** |

## Design Specificity Verdict

**Product-specific in language and semantics; category-interchangeable in form.**

The copy is authored for AttendTa: the consequence names the real mechanism ("creates an active member account and lets the applicant sign in with the password they chose"), the per-record isolation is stated, the nouns are `request`/`applicant`, and the partials are parameterised rather than hardcoded to one screen. Nothing here could drop into a generic admin template without rewriting the words.

The visual layer was not. A bordered strip holding a count and three buttons is what every bulk surface ships. The one move `DESIGN.md` already reserved for exactly this state — Cobalt Tint on the selected row — was absent, so the surface read as "a bulk bar was added to the table" rather than "this table has a selection state".

## Overall Impression

Correct, honest, and unusually well-guarded for a batch command; the confirmation dialog is the best moment in it. The single biggest opportunity is that the surface is capped at one page of 25, which is not the unit the officer's backlog actually has.

## What's Working

1. **The confirmation dialog does the hard job properly** — names every selected person with their student number, states the real consequence, sets the per-record expectation.
2. **Honest scoping with no overclaiming** — "on this page" is literally true, selection is never persisted, a bfcache restore clears it, and the 25 bound is one shared constant so copy and server guard cannot drift.
3. **Disabled-state discipline and per-record results** — each control is disabled exactly when it would change nothing, the header shows the partial state, and every record gets its own line with a text label beside the tint.

## Priority Issues

**[P1] The selected row has no visual state.** `bulk-actions.css` had no rule for a ticked row; `DESIGN.md` commits Cobalt Tint to "selected row" explicitly. In a 25-row table the entire feedback for a selection decision was a 16px tick and a number above the fold. *Fix applied:* `tr:has(input[data-bulk-row]:checked) td` carries the tint, with a `:hover` variant that outranks the shared row-hover ground.

**[P1] The bar was always on, including where selection is impossible.** On the Approved and Rejected tabs every row rendered an empty select cell, so the bar still showed "0 selected" with three disabled buttons — the opposite of the product's "no dead ends". *Fix applied:* the bar, dialog and select column render only where at least one pending row exists.

**[P1] Two cobalt primaries for the same command.** The bar's action and every pending row's Approve were both `button-primary`; `DESIGN.md` is unambiguous that a screen with two cobalt fills is wrong. *Fix applied:* the batch action is secondary, leaving the dialog's submit as the single primary at the moment of action.

**[P2] The confirmation dialog is read-only.** A wrong pick costs a full restart at the one checkpoint where correcting a mistake should be cheap. *Not fixed:* an editable list is a new interaction the intent did not ask for.

**[P2] The result panel's live region cannot announce, and failures had no next step.** `role="status"` on a server-rendered section is inert. *Partly fixed:* the panel keeps the role for hosts that update it in place, the heading is now `aria-labelledby`, and the count line states that unapproved records were left unchanged. A true announcement needs an in-place update.

**[P3] Paging silently discards the selection.** One stray "Next" loses a 25-row selection with no warning. *Not fixed:* the "on this page" label scopes it; a caption line would be chrome.

## Persona Red Flags

**Alex (power user, clearing a 200-request backlog):** cannot select across pages — a 200-request queue is eight rounds of select-all → approve → full reload. No shift-click range, no keyboard select-all, no "approve all N matching this filter", which is the only accelerator that matches his job. Paging silently wipes the selection. No keyboard path from the last ticked row to the batch action.

**Sam (keyboard / screen reader):** the row checkbox announces its state but nothing else in the row changes. Entering the dialog announced the title only — the summary, consequence and name list had no `aria-describedby` link, and `showModal()` puts focus on the close button first. The name list is a scroll container that is not focusable, so a keyboard user cannot scroll past roughly eight names. The result live region was inert, and focus reset to the top after the POST.

## Minor Observations

- Row checkbox label was `Select @request.FullName` while the dialog used name + student number, so same-name students were indistinguishable at the checkbox. *Fixed:* the label now carries the student number.
- Decided rows rendered an empty select cell with no text, reading as malformed when traversed cell by cell. *Fixed:* `aria-hidden="true"`.
- Count voice is inconsistent: "0 selected" in the bar, "N requests selected" in the dialog.
- `role="status"` plus `aria-live="polite"` on the same element is redundant.
- `table-sticky` and `request-table` have no CSS anywhere in `Styles/`; `DESIGN.md`'s sticky-column policy, including the account-request exception, is unimplemented — and the select column now occupies the "first column is applicant identity" slot that exception describes.
- `MaxBatchSize` (25) exactly equals the auto-fit ceiling, so select-all can never exceed it, but nothing enforces that relationship.

## Questions to Consider

1. If the officer can select at most one page and at most 25, and the queue's real job is clearing a backlog — is per-page selection the right unit at all, or should the primary action be "approve all N matching this filter" with the count in the button?
2. Why does the bar exist before anything is selected? What would the screen lose if it materialised on the first tick?
3. Should row-level Approve survive on this queue once bulk exists? Two entry points to the same irreversible command is a decision the design has not yet made.
4. If the confirmation dialog is the trust moment, why is it read-only? A list you cannot edit is a receipt shown before the purchase.

Questions skipped: unattended build-auto run.
