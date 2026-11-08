# Block 2 — Broad strokes: the mechanism

Three layers, one shape. The lists all produce `PagedResult(T)` and hand it to the same two partials; the editors all render a `_Form` partial and return through the same error handler; the services all resolve the actor, open a transaction, take a row lock, and append an audit entry.

## Read path — lists

```mermaid
flowchart LR
  subgraph READ["Read path"]
    direction TB
    V1["Member/Index"] --> M1["MemberService.GetPageAsync"]
    V2["EventType/Index"] --> M2["EventTypeService.GetPageAsync"]
    V3["Venue/Index"] --> M3["VenueService.GetPageAsync"]
    M1 --> PR["PagedResult(T)"]
    M2 --> PR
    M3 --> PR
    PR --> RC["_ResultCount partial"]
    PR --> PG["_Pager partial"]
    RC -. "reads" .-> IP["IPagedResult: FirstItemNumber, LastItemNumber"]
    PG -. "reads" .-> IP
  end
```

The empty branch keeps the same `<table>` and puts the message in a `colspan` row, so the headers survive.

## Write path — editors, services, toggle

```mermaid
flowchart TB
  subgraph WRITE["Write path"]
    direction TB
    F1["Member/_Form: 3 fieldsets"] --> C1["MemberController"]
    F2["EventType/_Form: 1 fieldset"] --> C2["EventTypeController"]
    F3["Venue/_Form: 3 fieldsets"] --> C3["VenueController"]

    C1 --> A1["RequireManagementActor"]
    C2 --> A2["RequireActorId"]
    C3 --> A3["RequireManagementActor"]

    A1 --> TX["ExecuteInTransactionAsync"]
    A2 --> TX
    A3 --> TX

    TX --> LK["Row lock"]
    TX --> AU["ActivityLogWriter.Append"]
    TX --> TP["TimeProvider.GetUtcNow"]

    C1 --> HDE["HandleDomainException"]
    C3 --> HDE
    HDE --> RE["Field error, redisplay form"]

    C2 --> TG["ToggleActive(id, page)"]
    TG --> RT{"page > 0?"}
    RT -->|yes| R1["RedirectToRoute EventTypeIndex, page"]
    RT -->|no| R2["RedirectToAction Index"]
  end
```

## Why it holds together

- `IPagedResult` is the seam: the pager and the result count both read the same object, so a list cannot show a count that disagrees with its pager.
- `_Form` is the seam for editors: required markers and fieldset grouping are defined once per module and inherited by both Create and Edit.
- `HandleDomainException` is the seam for failures: a field error redisplays the form, a field-less failure redirects with a toast.
- The named `EventTypeIndex` route is what makes the toggle return to `/EventType?page=N` rather than `/EventType/Index?page=N`.
