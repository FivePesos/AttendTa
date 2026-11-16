---
title: 'Clarify modal copy'
type: 'refactor'
ticket: ''
created: '2026-11-16'
status: 'built'
baseline_revision: '0d4e6b94c7704fa9cd172f3098e88fcd353ca60f'
route: 'oneshot'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Venue, event, and certificate dialogs repeat section headings and instructions, making forms noisier.

**Approach:** Clarify the four previously identified cases while preserving persistent accessible labels, fieldset legends, existing validation, and the certificate reason length limit. Keep useful identity subtitles and confirmation actions unchanged.

</frozen-after-approval>

## Implementation Notes

Oneshot: narrowly scoped wording changes in three Razor views; shared partials also affect standalone forms. Verify create and edit variants and preserve their conditional image behavior.

Renamed the venue booking section and made upload labels action-oriented while preserving fieldset legends and labels. Removed the duplicate certificate placeholder; the persistent hint retains the reason guidance and 1,000-character limit.

The Release solution build failed on the pre-existing `AccountLoginMatrixTests.cs:104` reference to missing `User.UserId`; the web app project compiled successfully during that build. The unrelated correction is in the separate backend PR.

An invalid or unservable existing venue image can leave no current photo on edit. The optional-photo hint now matches that state instead of referring to a nonexistent current photo. The separate Release web app build passed with no warnings or errors. The design scan found no issues, but authenticated rendering and screen-reader checks were unavailable.

## Plan Change Log

## Review Triage Log

- Medium, deferred: the full solution build fails in the pre-existing login test because `User.UserId` no longer exists. The web app builds; the separate backend PR already contains the correction. Full-solution verification remains blocked until that correction lands.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln --configuration Release` — solution builds.
- Check the affected markup and form labels, conditional branches, and 1,000-character certificate hint.

**Manual checks:**
- Inspect the create/edit venue and event forms and certificate decline dialog if authenticated local access is available; report any unverified states.
