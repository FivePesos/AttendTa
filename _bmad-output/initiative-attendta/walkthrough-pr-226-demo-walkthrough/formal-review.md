# Formal review — PR #226

Four lenses ran: adversarial, edge-case-hunter, structure, prose (prose on top of structure).
They returned 158 raw observations between them. Triage below keeps what survived verification
against the code and the files, and names what did not — an evidence record is exactly the kind of
document where a plausible-sounding finding is often wrong.

Scope: the 68-row journey table and the claims it carries (Block 3).

## Surviving findings

### Factual errors in the shipped files

| # | Location | Finding | Fix |
|---|---|---|---|
| 1 | `story-...-plan.md:86` | Says "6 blocked rows"; §7 has 7 and the plan's Auto Run Result says 7. | Change to 7. |
| 2 | `story-...-plan.md:98` | Says "Two stale demo-script expectations"; §8 has three notes and the Auto Run Result says three. | Change to three, and note the second is demo *data*, not script text. |
| 3 | `demo-walkthrough-evidence.md:150` (J65) | Says logout "302 to `/`". `AccountController.Logout` returns `RedirectToAction("Login", "Account")` — the redirect is `/Account/Login`. | Correct the target. |
| 4 | `demo-walkthrough-evidence.md:45-49` and `story-...-plan.md:134` | The PASS legend and the plan's manual check both refer to "the evidence field", but §4's columns are `#`, `Beat`, `Role`, `Action`, `Observed outcome`, `Result`. There is no evidence column. | Say "the observed-outcome cell". |
| 5 | `story-...-plan.md:62` | The task list says the header carries a `browser` field; the record's header has no Browser row (its `Driver` row says "No browser automation"). | Drop the field from the task. |
| 6 | `demo-walkthrough-evidence.md:42` | Calls .NET SDK 10.0.403 "the same patch level". 10.0.403 is an SDK feature-band version, not a patch level. | "the same SDK version". |

### Substantive gaps

| # | Location | Finding |
|---|---|---|
| 7 | §5 FR-8, §6 MD-05 | The requirement is "upload **and replace** record images". J11 uploads an event cover that had none; J62 replaces a *venue* photo. Event-cover **replacement** is never exercised and is not marked REUSED. |
| 8 | §7 | The seven blocked rows do not say which FR or functional ID each belongs to, so a reader cannot confirm that nothing is silently unrepresented. OPS-01, OPS-02, M-AUD-01, M-AUTH-01 and CERT-03 appear nowhere in §4–§6. |
| 9 | §11 | "Record complete: Yes" conflates *every row is filled* with *every row is evidenced*. The plan's own follow-up note admits every PASS is the executing agent's testimony. The sign-off should separate the two so the human evaluator knows what they are signing. |
| 10 | §5, §6 | REUSED rows cite test classes by bare name (`VenuePhotoFlowTests`, `RegistrationPromotionTests`, …) with no path and no manifest section, so a reader cannot check that the class still exists. |
| 11 | §4 J59 | A role change requires a non-empty reason (`AccountRoleService.cs:75`), which the row never says. A replay posting only `role_id` fails. |
| 12 | §4 J15 | `PASS (with note)` is not one of the five legend values. |

### Structural

| # | Location | Finding |
|---|---|---|
| 13 | §4 | The `Beat` column mixes three vocabularies: FR numbers (`FR-1`), functional IDs (`A2 / FR-2`) and demo-script beat names (`M3`, `O5`). A reader looking for "where is FR-19 proven?" cannot filter on it. |
| 14 | §4 | 68 consecutive rows with no sub-headings. Six role/lifecycle sub-tables would give the evaluator six checkpoints instead of one 2,500-word wall. |
| 15 | §11 vs opening | The completeness verdict sits at line 301; the evaluator's first question — "is this ready to sign?" — is answered last. |

## Dismissed after verification

| Claim | Why it is wrong |
|---|---|
| J35's quoted message is a string the code cannot emit | It is exactly what `CheckInController.cs:146-147` renders for the `Repeat` outcome. The service's `RepeatMessage` constant is a different surface. |
| J2's "2 pending" contradicts the header's "1 pending" | The header is the state *before* the journey; J1 adds Ava's request. 1 → 2 → 1 → 27 → (25 + 2) reconciles. |
| J58's "34 rows, Page 1 of 2" is arithmetically impossible | 25 per page gives 25 + 9 = 34, two pages. Consistent. |
| Plan `ticket: '7'` contradicts "8.7" | `7` is the BMad ticket id in the ticket file's front matter; 8.7 is the story number. Both correct. |
| The 8.4 manifest citation is unverified | Verified: `git diff --stat fa9ae82 84b3f54` excluding `_bmad-output` and `docs` is empty. The seven commits in that range are documentation and evidence only, so the REUSED rows rest on a suite that ran against the same code. |
| §7 citing `docs/hotspot-https-evidence.md` breaks the legend's "no planning document" rule | Those are evidence files, not planning documents. |

## What the review did not cover

The CSS build claim, the 8.4 manifest's test counts, 8.6's checklist rows, `demo-script.md`'s
account list, and the database state the record left behind.
