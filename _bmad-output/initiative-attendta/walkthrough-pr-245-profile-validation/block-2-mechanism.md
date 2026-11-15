# Block 2 — how the one rule set reaches both surfaces

Mermaid source. Renders in the VS Code markdown preview, on GitHub, and in any Mermaid viewer.
Everything drawn here is taken from the files linked in the walkthrough narrative.

```mermaid
flowchart TD
    subgraph source["One rule set — ProfileOptions"]
        PO["Courses · CoursePattern<br/>YearLevels 1–4 · MinYearLevel · MaxYearLevel<br/>Sections A–F · SectionPattern<br/>StudentNumberPattern<br/>MobileNumberPattern<br/>PasswordPattern<br/>NormalizeCourse · NormalizeSection"]
    end

    subgraph surface1["Surface 1 — the form"]
        V1["AccountRequestViewModel<br/>annotations"]
        V2["MemberViewModel<br/>annotations"]
        RV["Request.cshtml<br/>selects · inputmode"]
        MF["_Form.cshtml<br/>selects · inputmode"]
        MS{"ModelState valid?"}
        ERR["Redisplay the form<br/>error on its own control<br/>input retained · password cleared"]
        RV --> MS
        MF --> MS
        V1 --> MS
        V2 --> MS
        MS -->|no| ERR
    end

    subgraph surface2["Surface 2 — the service"]
        AS["AccountRequestService.SubmitAsync<br/>Normalize → Validate"]
        AM["MemberService.UpdateMemberAsync<br/>Normalize → lock user → Validate"]
        AP["AccountRequestService.ApproveAsync<br/>stored request → MemberViewModel"]
        CM["MemberService.CreateMemberFromApprovedAccountRequestAsync<br/>Normalize → Validate"]
        AP --> CM
    end

    PO --> V1
    PO --> V2
    PO --> RV
    PO --> MF
    V1 -.->|same annotations| AS
    V2 -.->|same annotations| AM
    MS -->|yes| AS
    MS -->|yes| AM
    AS -->|"unique? · hash password"| DB1[("account_requests")]
    AM -->|"unique? · audit"| DB2[("members")]
    CM --> DB2
    AP -.->|"legacy row outside the rules"| X["DomainException Invalid<br/>transaction rolls back<br/>request stays pending"]
```

## Reading it

- **Left of the form:** both view models point at the same constants, so a rule cannot be changed
  in one form and not the other.
- **The diamond:** MVC applies the annotations to the raw post. A failure redisplays with the error
  on its own control; the input is kept and the password is not echoed back.
- **The dotted lines:** the services do not own a second rule set. They run
  `Validator.TryValidateObject` over the same annotated view models, which is why a direct service
  call is refused by the same rules as a crafted POST.
- **The bottom right:** approval builds a `MemberViewModel` from the *stored* request, so a pending
  request written before the rules existed fails validation and rolls back. That is the one path
  with no in-app correction; `database/inventory-legacy-profile-values.sql` lists those rows.
