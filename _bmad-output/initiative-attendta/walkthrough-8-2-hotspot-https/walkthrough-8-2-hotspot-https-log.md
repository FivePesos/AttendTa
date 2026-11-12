# Review log: walkthrough-8-2-hotspot-https

Target: `feature/75/Hotspot-network-binding-and-trusted-HTTPS-mobile-camera-scanning`,
`8582f2f9b7969628be3fa4f4778862a301fb7fce..80e497e` (issue #75, PR #209).

## 1 — Review setup — walkthrough opened

Session: unavailable · Timestamp: 2026-11-09T11:51:39+08:00

- Action: Read the story, the plan, the issue body, the full diff and the log template; created the
  narrative and this log.
- Result: Narrative has eight blocks in this order: Intent (story Description and Requirements
  verbatim), Broad strokes, Slice 1 settings and safety checks, Slice 2 certificate trust chain and
  CLI, Slice 3 Kestrel listeners and startup wiring, Slice 4 tests, Slice 5 runbooks and evidence,
  Periphery. No block reviewed yet; Intent is current.
- Evidence: [narrative](walkthrough-8-2-hotspot-https.md); diff `git diff 8582f2f..HEAD` (19 files,
  +2286/−5).
- Open: Real host, certificate and phone evidence remain BLOCKED; PR does not close #75. GitHub
  shows #75 closed 2026-11-08, which the narrative treats as not evidence.

## 2 — Block 1 accepted — Intent

Session: unavailable · Timestamp: 2026-11-09T11:58:33+08:00

- Action: Reviewer confirmed the intent is clear and acknowledged that the offline phone evidence is
  still pending.
- Result: Intent accepted; Broad strokes is current.
- Evidence: narrative block 1.

## 3 — Moves on Broad strokes — Draw and Test

Session: unavailable · Timestamp: 2026-11-09T12:02:00+08:00

- Action: Drew the mechanism as Mermaid plus an ASCII fallback, then ran the focused hotspot tests
  and the `--new-hotspot-cert` command on a disposable output directory.
- Result: Diagram saved; 36/36 hotspot tests passed; the command exited 0, wrote four files, printed
  the root and server thumbprints and the SAN `IP:192.168.137.1`, and did not print the password.
- Evidence: [diagram](hotspot-https-flow.md); test summary 36 passed / 0 failed; CLI output in the
  session. The disposable certificate directory was deleted after the run.
- Unverified: the real host, root installation on a phone, no-warning Chrome, camera scan, manual
  lookup, second phone and offline run remain `BLOCKED`.

## 4 — Block 2 accepted — Broad strokes; Slice 3 started with Program.cs

Session: unavailable · Timestamp: 2026-11-09T12:06:00+08:00

- Action: Reviewer accepted the broad-strokes block and asked to inspect `Program.cs` first.
- Result: Slice 3 is current, starting with `Program.cs`; the rest of Slice 3 (`HotspotHttps.cs`,
  `Startup.cs`) follows when the reviewer is ready. Slices 1 and 2 remain unvisited.
- Evidence: narrative blocks 2 and 5.

## 5 — Program.cs accepted; HotspotHttps.cs current

Session: unavailable · Timestamp: 2026-11-09T12:10:00+08:00

- Action: Reviewer accepted the `Program.cs` startup wiring.
- Result: `HotspotHttps.cs` is current; `Startup.cs` follows in the same slice.
- Evidence: narrative Slice 3.

## 6 — Move on HotspotHttps.cs — Drive

Session: unavailable · Timestamp: 2026-11-12T02:20:00+08:00

- Action: Started a second Release instance with hotspot mode on `192.168.56.1:5444` (HTTP loopback
  `127.0.0.1:5081`), using a disposable root/server certificate generated for that IP. The existing
  app on port 5080 was left untouched.
- Result: App log showed `Now listening on: http://127.0.0.1:5081` and
  `Now listening on: https://192.168.56.1:5444`. HTTP loopback returned 200. HTTPS with the generated
  root returned 200 and `connection hostname (192.168.56.1) matched cert's IP address`. HTTPS without
  the root fails as expected because the chain is not trusted.
- Evidence: app log in `%TEMP%\attendta-8.2-drive.out.log`; disposable certificates in
  `%TEMP%\attendta-8.2-drive-certs` (not committed); process id in `%TEMP%\attendta-8.2-drive.pid`.
- Unverified: real phone, browser trust installation, camera scan, manual lookup, second phone and
  offline run remain `BLOCKED`.

## 7 — HotspotHttps.cs accepted; drive stopped; Startup.cs current

Session: unavailable · Timestamp: 2026-11-12T02:25:00+08:00

- Action: Reviewer accepted `HotspotHttps.cs` and asked to stop the drive process.
- Result: Drive PID 41020 stopped; ports 5081 and 5444 are free; the existing app on port 5080 was
  left untouched. `Startup.cs` is current.
- Evidence: `Stop-Process` output and an empty listener check for 5081/5444.

## 8 — Move on Startup.cs — Second opinion

Session: unavailable · Timestamp: 2026-11-12T02:30:00+08:00

- Action: A fresh subagent without this conversation reviewed `Startup.cs`, `Program.cs`,
  `HotspotHttpsOptions.cs` and `HotspotHttps.cs`.
- Result: Verdict "correct — and in fact necessary — but not fully sufficient". Findings:
  1. The loopback HTTP listener serves the whole app, not only the readiness endpoint; local-only,
     low severity, more surface than the readiness check needs.
  2. The readiness URL validator accepts IPv6 loopback, but only IPv4 loopback is bound; low/medium
     operational edge, mitigated by the runbook's `127.0.0.1` URL.
  3. With hotspot off, `UseHttpsRedirection` is a no-op because no HTTPS port is configured;
     pre-existing, not introduced here.
  4. `Hotspot:HttpPort` and `ATTENDTA_READINESS_LOGIN_URL` are coupled by documentation only;
     changing one without the other breaks the readiness check.
  5. `FromConfiguration` at `Startup.cs:148` is safe and cannot throw.
- Evidence: subagent answer in the session.
- Decision: no code change made during the walkthrough; findings are recorded for the reviewer to
  accept or schedule as follow-up.

## 9 — Fixes applied after second opinion

Session: unavailable · Timestamp: 2026-11-12T02:40:00+08:00

- Action: Applied three fixes:
  1. The loopback HTTP listener now serves only `GET /Account/Login` in hotspot mode; every other
     plain-HTTP request gets 404.
  2. `ListenLocalhost` binds both `127.0.0.1` and `[::1]`, matching the readiness validator.
  3. The runbook now says `Hotspot:HttpPort` must match `ATTENDTA_READINESS_LOGIN_URL`.
- Result: Build clean; 44 hotspot tests pass. Live checks: HTTP `/Account/Login` 200,
  HTTP `/CheckIn` 404, HTTP `POST /Account/Login` 404, HTTP `[::1]` 200, HTTPS `/Account/Login` 200,
  HTTPS `/CheckIn` 302.
- Commit: `8774eac` on `pr-226`. PR #209 was already merged, so this is a follow-up on the current
  branch rather than an update to #209.
- Evidence: build/test output and the live HTTP/HTTPS checks in the session.

## 10 — Slice 3 accepted; Slice 1 current

Session: unavailable · Timestamp: 2026-11-12T02:45:00+08:00

- Action: Reviewer accepted the startup-wiring slice after the fixes.
- Result: Slice 3 accepted. Slice 1 (settings and safety checks) is current; Slice 2, Slice 4,
  Slice 5 and Periphery remain.
- Evidence: narrative Slice 3 and the fix entry above.

## 11 — Slice 1 accepted; Slice 2 current

Session: unavailable · Timestamp: 2026-11-12T02:50:00+08:00

- Action: Reviewer accepted the settings and safety-check slice.
- Result: Slice 1 accepted. Slice 2 (certificate trust chain and CLI) is current; Slice 4, Slice 5
  and Periphery remain.
- Evidence: narrative Slice 1.

## 12 — Move on Slice 2 — Test

Session: unavailable · Timestamp: 2026-11-12T02:55:00+08:00

- Action: Ran the certificate test class and the `--new-hotspot-cert` command with two hosts on a
  disposable output directory.
- Result: 7/7 certificate tests passed. The command exited 0, wrote four files, printed
  `DNS:attendta-demo.local, IP:192.168.137.1`, validity `2026-11-12` to `2027-12-14`, and did not
  print the password. The disposable directory was deleted.
- Evidence: test summary and CLI output in the session.

## 13 — Formal review skipped on Slice 2

Session: unavailable · Timestamp: 2026-11-12T02:58:00+08:00

- Action: Reviewer declined the formal review because it would take too long.
- Result: No reviewers launched. The Test move results stand: 7/7 certificate tests passed and the
  CLI produced the expected SANs without printing the password.
- Decision: Slice 2 remains current until the reviewer says it is done.

## 14 — Slice 2 accepted; Slice 4 current

Session: unavailable · Timestamp: 2026-11-12T03:00:00+08:00

- Action: Reviewer accepted the certificate trust-chain and CLI slice.
- Result: Slice 2 accepted. Slice 4 (tests) is current; Slice 5 and Periphery remain.
- Evidence: narrative Slice 2 and the Test move above.

## 15 — Move on Slice 4 — Test

Session: unavailable · Timestamp: 2026-11-12T03:05:00+08:00

- Action: Ran the full hotspot filter and the Services suite on the current build.
- Result: 44/44 hotspot tests passed; 321/321 Services tests passed. No failures, no skips.
- Evidence: test summaries in the session.
- Unverified: the real phone, browser trust, camera scan, manual lookup, second phone and offline
  run remain `BLOCKED`.

## 16 — Slice 4 accepted; Slice 5 current

Session: unavailable · Timestamp: 2026-11-12T03:10:00+08:00

- Action: Reviewer accepted the tests slice after the 44/44 hotspot and 321/321 Services runs.
- Result: Slice 4 accepted. Slice 5 (runbooks and evidence) is current; Periphery remains.
- Evidence: narrative Slice 4 and the Test move above.

## 17 — Slice 5 accepted; Periphery current

Session: unavailable · Timestamp: 2026-11-12T03:15:00+08:00

- Action: Reviewer accepted the runbooks and evidence slice.
- Result: Slice 5 accepted. Periphery is current; the review is one block from wrap-up.
- Evidence: narrative Slice 5.

## 18 — Wrap-up — review complete

Session: unavailable · Timestamp: 2026-11-12T03:20:00+08:00

- Action: Reviewer chose Wrap-up.
- Result: All eight blocks reviewed. Fix commit `8774eac` on `pr-226`. Verification: 44/44 hotspot
  tests, 321/321 Services tests, and live HTTP/HTTPS checks. Remaining `BLOCKED`: real host, phone
  trust, camera scan, manual lookup, second phone and offline run.
- Follow-through: awaiting the reviewer's choice on committing the walkthrough files, the fix
  branch/PR, and any issue comment.
