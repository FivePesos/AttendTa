- source_plan: `_bmad-output/initiative-attendta/tickets/epic-platform-access/tickets/story-caller-owned-transaction-unit-of-work-and-append-only-activi-plan.md`
  summary: Remove venue image paths from cleanup-failure diagnostics.
  evidence: `VenueController.TryDeleteVenueImage` predates this story and logs the relative image path and exception, which may contain a filesystem path.
- source_plan: none
  summary: Improve the public account-request form with course choices, password guidance, field hints, and validation feedback.
  evidence: Split from management account-request review so approval and rejection can be delivered independently first.
