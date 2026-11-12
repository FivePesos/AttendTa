# Walkthrough — Story 8.2, hotspot network binding and trusted HTTPS mobile camera scanning

Target: branch `feature/75/Hotspot-network-binding-and-trusted-HTTPS-mobile-camera-scanning`,
range `8582f2f9b7969628be3fa4f4778862a301fb7fce..80e497e` (three commits).
Tracking issue: [#75](https://github.com/rainyDayssss/AttendTa/issues/75). Pull request:
[#209](https://github.com/rainyDayssss/AttendTa/pull/209).

This walkthrough is a reading guide, not acceptance. The code and documents are here; the real
host, the real hotspot address, the certificate thumbprints and the phone scan are not. Every one
of those rows is still `BLOCKED` in
[hotspot-https-evidence.md](../../../docs/hotspot-https-evidence.md), and this pull request does
not close #75 on its own.

---

## 1. Intent

- [x] **Intent** — reviewed and accepted

Source: the story file
[story-hotspot-network-binding-and-trusted-https-mobile-camera-scan.md](../tickets/epic-release-acceptance/tickets/story-hotspot-network-binding-and-trusted-https-mobile-camera-scan.md),
whose Description and Requirements are also the body of issue #75. Pasted verbatim.

### Description

> Decides the HTTPS mechanism and host address for the hotspot with Technical Manager Gabutan (Q2)
> and records the decision in this epic's Notes; then configures Kestrel to serve HTTPS directly on
> the hotspot LAN interface, with a certificate the officers' phones trust and no IIS or reverse
> proxy (AD-16); then verifies phone Chrome camera QR scanning where the 4.4 scanner page is served
> over the hotspot HTTPS origin here, not only localhost, filing scanner defects as bugs back to
> feature epics.

### Requirements

> Verify: With the host disconnected from the internet, an officer opens the hotspot HTTPS URL in
> mobile Chrome with no certificate warning, grants camera access, scans a valid attendee QR Pass
> and sees the attendee's name and student number, and finds the same attendee through manual lookup
> on that phone.

### What that means in plain English

The demo laptop has to hand out its own Wi-Fi hotspot. Phones join that hotspot and open the
check-in scanner. For the camera to work, the page must be served over HTTPS, and for Chrome to
allow the camera without a scary warning, the phone must already trust the certificate the laptop
presents. The story therefore has three parts: choose and record the mechanism (Q2), build the
laptop side so it can serve that HTTPS, and then prove it on a real phone with the internet off.

The third part is the part this branch cannot do. The repository has no hotspot adapter, no
officer phone and no camera.

**Acceptance check.** The Verify line is a single end-to-end phone scenario. Nothing in this
branch claims it passed: the evidence file marks all twelve verification rows `BLOCKED` with the
missing input named. The Delivery bullets say the same thing — real host acceptance, not a UI
build; localhost-only results do not close the issue; Q2 stays open until the recorded evidence
exists, and trust on a second phone is final acceptance. This block passes only if the reader
agrees the intent is stated honestly.

---

## 2. Broad strokes

- [x] **Broad strokes** — reviewed and accepted

The change adds an opt-in "hotspot mode" to the web app. When it is off — the committed default —
startup is unchanged. When it is on, the app reads a `Hotspot` configuration section, validates it
hard, opens the server certificate, and tells Kestrel to listen twice: plain HTTP on `127.0.0.1`
for the existing readiness check, and HTTPS on the hotspot adapter IP for phones. A small console
command in the same executable creates the two certificates the scheme needs, offline.

Five places to open first:

1. [Program.cs](../../../ASI.Basecode.WebApp/Program.cs) — the `--new-hotspot-cert` switch and the
   branch that turns hotspot mode on before the host is built.
2. [HotspotHttpsOptions.cs](../../../ASI.Basecode.WebApp/Hotspot/HotspotHttpsOptions.cs) — reads
   the `Hotspot` section and refuses unsafe addresses, ports, paths and missing secrets.
3. [HotspotHttps.cs](../../../ASI.Basecode.WebApp/Hotspot/HotspotHttps.cs) — checks the certificate
   is usable and registers the two listeners.
4. [HotspotCertificateCommand.cs](../../../ASI.Basecode.WebApp/Hotspot/HotspotCertificateCommand.cs)
   — the operator-facing command that writes four files and prints only thumbprints and SANs.
5. [hotspot-https.md](../../../docs/hotspot-https.md) — the Q2 decision and the runbook an operator
   follows on the real host.

Read in that order and the mechanism is complete: configure, validate, certify, listen, operate.

**Acceptance check.** Maps to the Delivery bullet "host checklist plus a focused scan/manual-lookup
smoke" and to the first half of the Verify line (the URL is served over HTTPS on the hotspot). It
does not map to the phone half, which is still `BLOCKED`.

---

## 3. Slice 1 — Settings and safety checks

- [x] **Slice 1** — reviewed and accepted

The `Hotspot` section has seven settings: `Enabled`, `LanAddress`, `HttpsPort`, `HttpPort`,
`PublicHostName`, `CertificatePath` and `CertificatePassword`. The committed
[appsettings.json](../../../ASI.Basecode.WebApp/appsettings.json) carries the shape with
`Enabled: false` and empty secrets, so the section documents itself without shipping a secret.

Validation is the interesting part, and it all happens before a port is opened:

- `LanAddress` must parse as an IP and must not be `0.0.0.0`, `::`, `127.0.0.1` or `::1`. A
  wildcard bind would expose the app on whatever network the laptop next joins; a loopback bind
  would be useless to a phone.
- The two ports must be in range and different from each other.
- `PublicHostName` must be a bare DNS name or IP — no scheme, port, path, wildcard or space.
- `CertificatePath` must be absolute and `CertificatePassword` must be non-empty.

Every message names the configuration key. None of them echoes the password. The class also
exposes `HttpEndpoint` and `HttpsEndpoint` so the runbook, the tests and the operator all talk about
the same two addresses.

**Acceptance check.** Covers the matrix rows "Hotspot off", "Missing certificate", "Unsafe bind"
and the port/path cases, and the acceptance criterion that a bad configuration refuses to start
with a clear message and never prints the password. It does not touch the Verify line's phone
steps.

---

## 4. Slice 2 — Certificate trust chain and CLI

- [x] **Slice 2** — reviewed and accepted

The scheme is a local root certificate authority, because that is the only way to give a phone a
chain it trusts with no internet and no public domain.

[HotspotCertificateFactory.cs](../../../ASI.Basecode.WebApp/Hotspot/HotspotCertificateFactory.cs)
builds both certificates in memory:

- A 4096-bit RSA root CA, valid 10 years, marked as a CA, with key-certificate-sign and CRL-sign
  usage.
- A 2048-bit RSA server certificate, valid 397 days — deliberately under Apple's 398-day ceiling —
  with a server-authentication EKU, a subject key identifier, an authority key identifier, and a
  SAN built from every `--host` value, split into DNS names and IP addresses.

The authority key identifier is not decoration: strict OpenSSL clients reject a chain without it
even when the root is trusted, which is recorded in the plan's implementation notes.

[HotspotCertificateCommand.cs](../../../ASI.Basecode.WebApp/Hotspot/HotspotCertificateCommand.cs)
is the operator surface. It parses `--new-hotspot-cert --host <dns-or-ip> [--host ...] [--out
<dir>]`, reads the password from `ATTENDTA_CERT_PASSWORD`, and writes four files: the public root
`.cer`, the root `.pfx`, the public server `.cer`, and the server `.pfx`. The server PFX embeds the
public root so Windows can build the chain without the root private key. The command refuses to
overwrite an existing output directory, so an operator cannot silently destroy the only copy of the
root private key, and its summary prints subjects, thumbprints, SANs, dates and paths — never the
password.

**Acceptance check.** Maps to the acceptance criterion "a generated certificate has the requested
SANs, a server-auth EKU, a private key and a chain to the root CA", and to the Delivery constraint
that private keys and passwords stay out of git, logs and output. It says nothing about whether a
phone trusts the chain; that is the human step.

---

## 5. Slice 3 — Kestrel listeners and startup wiring

- [x] **Slice 3** — reviewed and accepted

`Program.cs`, `HotspotHttps.cs` and `Startup.cs` were reviewed and accepted.

[HotspotHttps.cs](../../../ASI.Basecode.WebApp/Hotspot/HotspotHttps.cs) does the runtime half. It
loads the PFX, then refuses it unless it has a private key, is inside its validity window, carries
the server-authentication EKU, and has a SAN containing `PublicHostName`. On a SAN mismatch the
message lists the SANs the certificate actually has, which is the difference between a five-minute
fix and an afternoon. A wrong password produces one fixed message with no underlying exception
text, so no secret can leak through an error.

If the certificate is good, the class registers two listeners: `ListenLocalhost` on `HttpPort`
(both `127.0.0.1` and `[::1]`) and `IPAddress.Parse(LanAddress)` on `HttpsPort` with
`UseHttps(certificate)`. The loopback listener is readiness-only: `AllowsLoopbackRequest` permits
only `GET /Account/Login` over plain HTTP, so the rest of the app must use HTTPS. The PFX is loaded
with `X509KeyStorageFlags.Exportable`; the plan records that `EphemeralKeySet` produced a
certificate SChannel could not use, closing the handshake with `UNEXPECTED_EOF`.

[Program.cs](../../../ASI.Basecode.WebApp/Program.cs) wires it in before the host is built: the
certificate command returns early, then hotspot options are read and applied only when enabled. A
validation failure prints the message to stderr and exits with code 1 instead of starting a
half-configured host. The same commit removes `UseIISIntegration`, because AD-16 wants direct
Kestrel TLS and no IIS or reverse proxy in the path.

[Startup.cs](../../../ASI.Basecode.WebApp/Startup.cs) makes two related changes: the
HTTP-to-HTTPS redirect is skipped while hotspot mode is on, and the loopback HTTP listener is
restricted to the readiness request. That keeps the readiness check at HTTP 200, which is the
contract story 8.14 depends on, without exposing the whole app over local HTTP. LAN clients still
use HTTPS directly; they are never redirected.

**Acceptance check.** Covers the acceptance criteria "hotspot off behaves as today", "valid PFX
listens on HTTP loopback and HTTPS on the LAN IP, readiness login still 200", and "missing,
wrong-password, expired or SAN-mismatched certificates refuse to listen with a clear message". The
readiness-200 claim was observed on this machine's loopback listener, not on a demo host; the
evidence file still records the host rows as `BLOCKED`.

---

## 6. Slice 4 — Tests

- [x] **Slice 4** — reviewed and accepted

Three new test files cover the matrix without needing a phone:

- [HotspotHttpsOptionsTests.cs](../../../tests/ASI.Basecode.IntegrationTests/HotspotHttpsOptionsTests.cs)
  — configuration binding, defaults, endpoint formatting, and every refusal: empty, wildcard,
  loopback and non-IP addresses, equal ports, missing or relative certificate paths, missing
  password, missing or malformed public host. One test starts a real Kestrel server and proves both
  the loopback HTTP and the hotspot HTTPS addresses answer. The loopback guard tests prove that only
  `GET /Account/Login` is served over plain HTTP, while HTTPS and non-loopback requests are not
  restricted.
- [HotspotCertificateTests.cs](../../../tests/ASI.Basecode.IntegrationTests/HotspotCertificateTests.cs)
  — SANs, EKU, private key, validity windows, authority key identifier, chain building against the
  root as a custom trust anchor, PFX password enforcement, and the command's file output and
  password-free summary.
- [HotspotHttpsEvidenceTests.cs](../../../tests/ASI.Basecode.IntegrationTests/HotspotHttpsEvidenceTests.cs)
  — a guard on the evidence document. It fails if any verification row is not `BLOCKED`, if a
  blocked row stops naming what is missing, or if the honesty sentences disappear. This is the test
  that stops someone turning a wish into a `PASS`.

The evidence document is copied to the test output through
[ASI.Basecode.IntegrationTests.csproj](../../../tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj),
which is how the guard can read it at runtime.

The plan recorded 36 hotspot tests and 310 Services tests passing on 2026-11-08. After the
loopback-guard fix, the focused run on 2026-11-12 is **44 hotspot tests** and **321 Services
tests**, all passing. That is a local run, not CI evidence for this walkthrough.

**Acceptance check.** Covers the development-check Delivery bullet — "host checklist plus a focused
scan/manual-lookup smoke; existing CI unchanged" — and the matrix rows for bad certificates and
unsafe binds. It cannot cover the Verify line, because a unit test cannot hold a phone.

---

## 7. Slice 5 — Runbooks and evidence

- [x] **Slice 5** — reviewed and accepted

The documents are where this story is deliberately honest.

[hotspot-https.md](../../../docs/hotspot-https.md) is the Q2 runbook: the recorded decision (local
root CA, direct Kestrel HTTPS on the hotspot adapter IP, HTTP loopback for readiness, no IIS or
proxy), the address rule, certificate generation with the password read from a secure prompt,
host configuration through `Hotspot__*` environment variables, the firewall rule scoped to the
hotspot subnet, the start-and-check sequence, Android and iOS trust steps, the phone verification
steps, rotation, and a troubleshooting table. It also documents the readiness-only loopback listener
and the `Hotspot:HttpPort` / `ATTENDTA_READINESS_LOGIN_URL` coupling added with the fix.

[hotspot-https-simple-guide.md](../../../docs/hotspot-https-simple-guide.md) is the same procedure
in plain English for whoever runs the demo: the building-and-visitor analogy, the four files, the
two phone platforms, the scan and manual-lookup checks, a printable checklist, and a closing section
explaining why the repository still says `BLOCKED`.

[hotspot-https-evidence.md](../../../docs/hotspot-https-evidence.md) is the dated checklist. Every
host, certificate, phone, scan, duplicate, manual-lookup, camera-denied, second-phone, offline and
firewall row is `BLOCKED` with the missing input named. The sign-off row says Q2 stays open until a
real run exists.

[demo-host-setup.md](../../../docs/demo-host-setup.md) now points at the runbook and adds a hotspot
section, and [demo-host-readiness-evidence.md](../../../docs/demo-host-readiness-evidence.md) row 10
keeps its `BLOCKED` result while noting that the mechanism is recorded. The epic
[epic-release-acceptance.md](../tickets/epic-release-acceptance/epic-release-acceptance.md) gains a
2026-11-08 decision line recording the same mechanism and the same `BLOCKED` state.

**Acceptance check.** Covers the Delivery bullets "records the decision in this epic's Notes" and
"Q2 stays open until the recorded host/HTTPS/phone evidence exists", plus the acceptance criterion
that unverified rows are `BLOCKED` with the missing input named. It also covers "8.14 consumes its
host/HTTPS evidence rather than treating tooling delivery as acceptance": the 8.14 evidence file
still says `BLOCKED`.

GitHub shows #75 closed on 2026-11-08. The evidence file still records the real host, certificate
and phone run as `BLOCKED`; the closed state is not proof. Nothing here claims a phone scan happened.

---

## 8. Periphery

- [x] **Periphery** — reviewed and accepted

- [.gitignore](../../../.gitignore) — ignores the default `hotspot-certificates/` output so a
  generated PFX or private key cannot be committed by accident.
- [The plan file](../tickets/epic-release-acceptance/tickets/story-hotspot-network-binding-and-trusted-https-mobile-camera-scan-plan.md)
  — records the frozen intent, the edge-case matrix, the code map, the implementation notes
  (persisted key set, authority key identifier, root embedded in the server PFX, no-overwrite rule)
  and the verification commands. It is the source for most of the detail above.
- Commit messages — four commits, each prefixed `[BMAD 8.2]`: the feature commit that adds the
  hotspot code and documents, a plain-English annotation pass over the hotspot code, the
  plain-English operator guide, and the follow-up fix for the readiness-only loopback listener and
  IPv6 loopback binding. No commit message claims a phone run.

**Acceptance check.** Supports the Delivery constraint that no PFX, password or private key is
committed, and keeps the audit trail readable. It does not change any acceptance state.

---

## Review outcome

All eight blocks were reviewed. The implementation proves the mechanism: the certificate chain,
the two listeners, the readiness-only loopback guard, the fail-fast validation, the tests and the
runbooks. The follow-up fix `8774eac` on `pr-226` tightened the loopback listener and bound IPv6
loopback. The real host, certificate-on-phone and offline scan evidence remain `BLOCKED`; #75 is
closed on GitHub, but the evidence file is the only thing that can prove the Verify line.

## Still blocked after this walkthrough

- Real demo host, hotspot adapter IP and public host name.
- Generated certificate thumbprints, SANs and validity on that host.
- Root CA installed on phone 1 and phone 2; Chrome showing no warning.
- Camera grant, valid pass scan, duplicate handling, manual lookup, camera-denied fallback.
- The offline run with the host disconnected from the internet.
- Firewall scope check from the hotspot subnet.

Until those exist in
[hotspot-https-evidence.md](../../../docs/hotspot-https-evidence.md), Q2 stays open and #75 is not
satisfied by this branch.
