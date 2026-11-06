---
id: 13
type: story
title: "My Events personal registrations list"
parent: epic-events-registration
covers: [R12, R15]
after: [9, 10, 17]
hitl: false
risk: low
tracker_id: "42"
remote: "https://github.com/rainyDayssss/AttendTa/issues/42"
tracker_status: backlog
---

# My Events personal registrations list

## Description

Replace 3.17's fixture-backed registrations source with the real owner-scoped 25-row query, preserving the My Events controller, card contract and partial slots established by the frontend slice. Reuse RegistrationStatusDisplay from merged #133 and ManilaTimeFormatter. Return accurate Pending, Confirmed, Waitlisted, Cancelled and Not confirmed before cutoff states; never expose another member's rows. This integration ticket stays open until the real query is wired; no second UI host or card implementation.

## Requirements

Verify: A member navigates to My Events and views their registrations paged at 25 rows per page, showing accurate statuses (Pending, Confirmed, Waitlisted, Cancelled, Not confirmed before cutoff) with local time display; host slots for _PassPartial and _CertificateLinkPartial are present.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-events-registration/epic-events-registration.md
- _bmad-output/initiative-attendta/delivery-queue.md
- _bmad-output/initiative-attendta/finale-test-cases.md

## Delivery (2026-11-06)

- Frontend already delivered by 3.17 (My Events host, card contract and the `_PassPartial`/`_CertificateLinkPartial` slots). This issue stays open for the real owner-scoped registrations query only; it fills the delivered host and adds no second controller, card or partial.
- Issue #42 needs 3.17 merged first. Reuse `RegistrationStatusDisplay` from merged #133 and `ManilaTimeFormatter`; do not restyle the delivered surfaces.
- Development check: build plus one focused owner-scope/status smoke (a member sees only their own rows, paged at 25); existing CI unchanged. Broad automation goes to 8.5 (#78), the full suite runs once at 8.4 (#77), final manual acceptance is 8.6–8.8.
- Final rules unchanged: Pending, Confirmed, Waitlisted, Cancelled and Not confirmed before cutoff are all shown accurately, and another member's rows are never exposed.
