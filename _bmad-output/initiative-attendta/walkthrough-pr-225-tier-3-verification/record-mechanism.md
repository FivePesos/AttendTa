# How the record is built — PR #225

One tested commit, two executed rows, and a validity verdict for every source it
does not re-run. Nothing in the document is a second run: the matrix is a
projection of the reuse decisions plus the two observations, and the blockers are
the projection of every decision that came out negative.

```mermaid
flowchart TD
    subgraph RUN["One environment — 2026-11-11, local workstation"]
        ENV["Tested commit 84b3f54<br/>Windows 11 · .NET SDK 10.0.403 · Node v24.12.0<br/>SQL Server LocalDB · not the demo host"]
    end

    ENV --> T2["§5 T3-02 integrated build<br/>dotnet build Release · css:build<br/>test:js · 321 service tests"]
    ENV --> T3["§6 T3-03 migration checks<br/>disposable AttendTa_IT_t3mig…<br/>prefix · idempotence · guard · over-length"]

    subgraph REUSE["§3 Evidence reuse and validity"]
        S1["8.4 execution-manifest-fa9ae82.md<br/>fa9ae82 · 321 + 479, 0 failed"]
        S2["8.6 manual-acceptance-checklist.md<br/>d8046bf · 8 PASS, 5 BLOCKED"]
        S3["8.5 acceptance-coverage-matrix.md<br/>95493ac · case ids only"]
        S4["8.2 hotspot-https-evidence.md<br/>fb783f1 · rows 1-12 BLOCKED"]
        S5["8.14 demo-host-readiness-evidence.md<br/>da3661b · checks 1-10 BLOCKED"]
        S6["8.3 storage-survival-evidence.md<br/>8247832 · every stage BLOCKED"]
        S7["8.13 demo-script.md<br/>149791b · beat definition"]
    end

    S1 --> V{"Validity rule holds<br/>at 84b3f54?"}
    S2 --> V
    S3 --> V
    S4 --> V
    S5 --> V
    S6 --> V
    S7 --> V

    V -->|"yes — docs-only commits since"| M
    V -->|"no, or the evidence is itself blocked"| B

    T2 --> M["§4 Tier 3 matrix<br/>7 rows, T3-01..T3-07<br/>one result each"]
    T3 --> M
    M --> B["§7 Blockers<br/>8 rows<br/>missing input · owning file · owner · unblocking action"]
    B --> S["§8 Sign-off<br/>decision fields deliberately blank"]

    subgraph LEGEND["§2 Result vocabulary"]
        L1["PASS — executed here, observed"]
        L2["REUSED — covered by a cited record whose rule still holds"]
        L3["BLOCKED — not executable here, missing input named"]
        L4["FAIL / NOT RUN — defined, unused"]
    end
    LEGEND -.-> M

    S -.->|"the human decides"| GATE{"Release gate"}
```

## What the diagram exposes

| Observation | Where it shows | Status |
|---|---|---|
| Every row traces to either an executed observation or a named source | T3-02/T3-03 feed the matrix; §3 feeds it too | Sound |
| A source that fails its validity rule becomes BLOCKED, never PASS | the `V` decision node | Sound |
| Two rows use a result word the legend does not define | `PASS (none required)`, `BLANK` | Defect (Tier 2) |
| The matrix has no edge for the 8.7 journey | §3 has no 8.7 source at all | Defect (Tier 1) |
| The sign-off block is the only path to the gate | §8 | Sound — and deliberately empty |

The missing edge is the review's headline. PR #226 merged
[demo-walkthrough-evidence.md](../demo-walkthrough-evidence.md) onto `dev`
after this record was written, and it was executed on the **same tested commit,
`84b3f54`**. Under the rule the `V` node applies, that source is valid, so T3-04
belongs on the `yes` branch. It sits on the `no` branch because the source did
not exist in the tree on the day the record was written.
