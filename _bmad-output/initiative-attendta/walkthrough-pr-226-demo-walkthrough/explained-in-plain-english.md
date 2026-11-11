# PR #226 explained in plain English

For explaining this pull request to an instructor. No prior context assumed.

---

## The one-sentence version

This PR adds **no code**. It adds a written record proving that a student ran the finished
system once, start to finish, and checked that every feature the requirements ask for actually
works — plus a note of the seven things they could not check and why.

---

## Why a "record" is a deliverable at all

Software projects usually prove their work with automated tests: programs that press the buttons
for you and check the result. This project has those — 321 service tests and 479 integration tests,
all passing.

But a school project also has to be *demonstrated*. Someone has to sit at the app and walk an
evaluator through it. That creates a gap: the automated tests prove the code works, and the demo
proves a human can use it, but nothing connects the two. Nobody had written down "here is the one
continuous story from a stranger asking to join, all the way to a printed certificate."

That written story is this PR. It is called an **evidence record**.

---

## The two files

| File | What it is |
|---|---|
| `demo-walkthrough-evidence.md` | The record itself. 306 lines. This is the deliverable. |
| `story-...-plan.md` | The plan the person followed while doing it, plus the mistakes a reviewer caught before the PR opened. |

Think of it as: the first file is the lab report, the second is the lab notebook.

---

## `demo-walkthrough-evidence.md`, section by section

### Header (§1) — "what exactly did you run?"

A table pinning the run to one specific version of the code (`84b3f54`), one build, one database,
one machine, one date. This matters because "it worked on my machine" is worthless without saying
*which* machine and *which* version.

It also names three other documents it *reuses* instead of repeating — the automated test run, the
manual checklist, and the interface walkthrough. Like citing sources in an essay.

### Legend (§2) — "what do your labels mean?"

Five words, defined so nobody can argue about them later:

- **PASS** — I did this here and saw it work.
- **FAIL** — I did this and it broke.
- **BLOCKED** — I could not do this here, and here is exactly what I was missing.
- **REUSED** — I did not re-run this; the automated tests already cover it.
- **Composite** — part of this I did, part I did not, and I say which is which.

The `BLOCKED` definition is the important one: it forbids the trick of substituting "well, we fixed
that in another ticket" for actual evidence.

### Provisioning (§3) — "how do I get to the starting line?"

Six steps to rebuild the exact starting conditions: wipe the throwaway database, load the schema,
create the admin account with a securely hashed password, seed the sample data, copy the images,
start the app.

This exists so a second person could repeat the whole thing. Without it the record is unrepeatable,
which makes it an anecdote rather than evidence.

### The journey (§4) — the actual evidence

**68 rows. This is the heart of the document.**

Each row is one action: who did it, what they clicked, what happened, and whether it passed.
They are numbered J1 to J68 and they are *continuous* — the registration approved at J28 is the
same pass shown at J30, scanned at J34, invalidated at J38, and the same event completed at J42.
State carries forward from row to row.

That continuity is the whole point. The existing checklist tested each feature as an isolated case.
This tests them as one story, which is what a demo actually is.

### FR-1 to FR-36 (§5) — "did you cover every requirement?"

The requirements document lists 36 functional requirements, numbered FR-1 to FR-36. This table has
one row per requirement, pointing at the journey row that proves it — or at the automated test that
covers it instead.

It is the same evidence re-sorted. Nothing here is a second test run; it is an index, so an
evaluator can look up "where is FR-19 proven?" without reading all 68 rows.

### The 26 functional IDs (§6) — the same index, different grouping

The acceptance matrix groups those requirements into 26 shorthand IDs (`AUTH-01`, `EVT-03`,
`CERT-02` and so on). This table is the same mapping against those IDs. Again: an index, not new
evidence.

### Blocked rows (§7) — "what couldn't you do?"

Seven things. Each names the missing input and the file that owns it:

- Scanning a QR code with a **real phone** on a hotspot network (needs a phone and a certificate
  installed on it).
- Demo-host readiness (needs a real server and a service account).
- Backup and restore (needs a server, a backup destination and an approver).
- Database column permissions (needs a real database login).
- A specific race between two administrators clicking at the same instant (not reproducible by
  hand).
- An eight-hour idle timeout (needs eight idle hours).
- Human approval of the certificate signatory's name and signature (needs a human to approve it).

This is the section that makes the record trustworthy. A dishonest record would have quietly
claimed these.

### Notes (§8) — "what surprised you?"

Six observations where reality differed from the demo script or the sample data. Three are cases
where the *script* is out of date, not the app. They are recorded as notes rather than quietly
fixed, because the script belongs to a different ticket.

### No-workaround statement (§9) — the promise

Every single change of state in the record came from a real form submission against the running
app. No fake data, no editing the database by hand, no pretending a certificate was issued.

It then admits the one exception: the QR pass page shows a picture of the code but never the code
itself, so the tester read the code out of the database to feed it to the scanner. Reading, not
writing. That disclosure is what separates this from a claim.

### Defects (§10) and sign-off (§11)

Zero product defects found. The sign-off block is deliberately **blank** — the person who ran the
test cannot also be the person who approves it. That signature is what closes the ticket.

---

## The plan file, briefly

A working document with the intent, the decisions taken, the tasks, and — most usefully — a
**triage log**: nine problems a reviewer found before the PR was opened, each with the file it was
checked against and what was changed. For example, one requirement had been marked as passing when
the demo never actually exercised it; it was downgraded to "covered by automated tests instead."

Its most honest line admits the record's central weakness: every PASS is the tester's word about
something the reader cannot go back and watch.

---

## What the review of this PR found

Four independent review passes looked at it. Six small factual errors were found and fixed —
a wrong count in two places, a wrong redirect address, a reference to a table column that does not
exist, a header field that was never there, and one wrong technical term.

Two bigger questions were left for the human to decide, because they change what the signature
means:

1. **One requirement is only half-demonstrated.** FR-8 asks for uploading *and replacing* images.
   The demo replaced a venue photo but never replaced an event cover, and still marked it as
   passing.
2. **"Record complete" is ambiguous.** It means every row has an answer. It does not mean every
   row was personally verified — two requirements and parts of three others rest on the automated
   tests instead.

---

## If the instructor asks...

**"Is this code?"** No. Two Markdown documents. Zero lines of application code changed.

**"Then what did you actually do?"** Ran the finished system once, end to end, as one continuous
story, and wrote down what happened at each step with the exact messages and page addresses.

**"How do you know it works?"** Three ways: the automated tests (800 of them), this manual run,
and a live re-run during review where seven of the record's claims were reproduced in a browser.

**"What doesn't work?"** Seven things that need hardware, a server, eight idle hours, or a human
signature — all listed with what was missing.

**"Why should I trust it?"** Because it says what it could *not* do, names the evidence it borrowed
instead of repeating, and leaves the approval signature blank for someone else to fill in.
