# How the record is built — PR #226

One execution, one journey table, and three projections of it. Nothing in the
document is a second run; every table after the journey is a re-index of the same
evidence.

```mermaid
flowchart TD
    subgraph RUN["One execution — 2026-11-11"]
        ENV["Commit 84b3f54 · Release build<br/>disposable AttendTaMVP_Test<br/>Production app on :5080"]
        DRV["Node HTTP client with a real cookie jar<br/>real MVC form POSTs, real GETs<br/>no browser automation, no fixtures"]
        ENV --> DRV
    end

    DRV --> J["§4 Journey table<br/>68 rows, J1..J68<br/>role · action · observed outcome · result"]

    J --> FR["§5 FR-1..FR-36<br/>36 rows<br/>requirement → journey step"]
    J --> ID["§6 26 functional IDs<br/>26 rows<br/>ID → journey step"]
    J --> BLK["§7 Blocked rows<br/>7 rows<br/>case · missing input · owning file"]

    REU["Reused evidence, cited not repeated<br/>8.4 execution-manifest-fa9ae82.md<br/>8.6 manual-acceptance-checklist.md<br/>8.12 frontend walkthrough"] --> FR
    REU --> ID
    REU --> BLK

    FR --> GATE{"§11 Sign-off<br/>every FR and every ID<br/>carries one result?"}
    ID --> GATE
    BLK --> GATE
    GATE -->|"yes, and a human signs"| DONE["Ticket 8.7 can close"]
    GATE -->|"no"| FIX["Row is wrong — fix the row,<br/>not the result word"]

    subgraph LEGEND["§2 Result vocabulary"]
        L1["PASS — executed here, observed"]
        L2["REUSED — covered by 8.4/8.6/8.12"]
        L3["BLOCKED — missing host, phone or human input"]
        L4["Composite — part here, part reused/blocked"]
    end
    LEGEND -.-> J
```

## What the checks proved

`check-record.ps1` in this folder re-derives the structural claims instead of
trusting them:

| Claim | Check | Result |
|---|---|---|
| 68 journey rows, contiguous | J1..J68 present, no gap, no duplicate | PASS |
| Index tables only cite real rows | 51 distinct J-references, all resolve | PASS |
| FR-1..FR-36 exactly once | 36 rows, no missing, no duplicate | PASS |
| Every FR row has a result | no short row | PASS |
| 26 functional IDs exactly once | all 26 expected, no extra | PASS |
| 7 blocked rows, each naming an owner | 7 rows, each cites a file | PASS |
| Plan and record agree on the blocked count | plan says 6 in one place, 7 in another | **FAIL** |

The one failure is the plan's own prose, not the record: its Implementation Notes
say "6 blocked rows" while its Auto Run Result says "7", and the record has 7.
