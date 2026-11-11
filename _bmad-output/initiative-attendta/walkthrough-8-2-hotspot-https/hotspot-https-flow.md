# Story 8.2 — hotspot HTTPS flow

This diagram shows only what the branch actually does. The last step — a real phone scanning a
pass — is the human acceptance step and is still `BLOCKED`.

## Mermaid

```mermaid
flowchart TD
    A["Operator runs<br/>--new-hotspot-cert --host &lt;ip&gt;"] --> B["HotspotCertificateFactory<br/>creates root CA + server cert"]
    B --> C["Four files<br/>root .cer / root .pfx<br/>server .cer / server .pfx"]
    C --> D["Install root .cer<br/>on each phone"]
    C --> E["Set Hotspot__*<br/>environment variables"]

    E --> F["App starts<br/>Program.cs"]
    F --> G["HotspotHttpsOptions<br/>validate address, ports,<br/>host, path, password"]
    G -->|invalid| H["Print clear message<br/>exit 1 — no port opened"]
    G -->|valid| I["HotspotHttps.Apply<br/>load PFX and check<br/>key, dates, EKU, SAN"]
    I -->|invalid| H
    I -->|valid| J["Kestrel listens twice"]

    J --> K["HTTP 127.0.0.1:HttpPort<br/>readiness check"]
    J --> L["HTTPS LanAddress:HttpsPort<br/>phones"]

    K --> M["Startup.cs skips<br/>HTTP→HTTPS redirect<br/>readiness gets 200"]

    D --> N["Phone opens<br/>https://LanAddress:HttpsPort"]
    L --> N
    N --> O["TLS handshake<br/>server cert matches address"]
    O --> P["Phone trusts root CA<br/>Chrome shows no warning"]
    P --> Q["Camera scanner page<br/>camera + manual lookup"]
    Q --> R["BLOCKED: real phone,<br/>real hotspot, offline run<br/>not yet recorded"]
```

## ASCII fallback

```text
  OPERATOR (one time)
  ┌──────────────────────────────────────────────────────────────┐
  │ --new-hotspot-cert --host 192.168.137.1 --out <dir>          │
  │        │                                                     │
  │        ▼                                                     │
  │ HotspotCertificateFactory                                    │
  │   root CA (10 years) + server cert (397 days, SAN = host)    │
  │        │                                                     │
  │        ├──► root .cer  ──► install on each phone             │
  │        ├──► root .pfx  ──► keep on laptop only               │
  │        ├──► server .cer ─► optional inspection               │
  │        └──► server .pfx ─► Kestrel loads it                  │
  └──────────────────────────────────────────────────────────────┘

  APP STARTUP (every run)
  ┌──────────────────────────────────────────────────────────────┐
  │ Program.cs                                                   │
  │   HotspotHttpsOptions.FromConfiguration                      │
  │        │                                                     │
  │        ├── invalid ──► clear message, exit 1, no port         │
  │        │                                                     │
  │        ▼ valid                                               │
  │   HotspotHttps.Apply                                         │
  │     load PFX, check key / dates / EKU / SAN                  │
  │        │                                                     │
  │        ├── invalid ──► clear message, exit 1                 │
  │        │                                                     │
  │        ▼ valid                                               │
  │   Kestrel                                                    │
  │     ├── HTTP  127.0.0.1:5080  ──► readiness check (200)      │
  │     └── HTTPS 192.168.137.1:5443 ──► phones                  │
  └──────────────────────────────────────────────────────────────┘

  PHONE (human acceptance, still BLOCKED)
  ┌──────────────────────────────────────────────────────────────┐
  │ open https://192.168.137.1:5443                              │
  │   → TLS handshake, server cert matches the address           │
  │   → phone trusts the installed root CA                       │
  │   → Chrome shows no certificate warning                      │
  │   → camera scanner page loads                                │
  │   → scan a pass, see name + student number                   │
  │   → repeat scan, manual lookup, camera-denied fallback       │
  │   → repeat offline and on a second phone                     │
  └──────────────────────────────────────────────────────────────┘
```

## What the code does not do

- It does not generate a certificate automatically at startup. The command is a deliberate one-time
  operator action, because the root certificate has to be installed on the phones.
- It does not install the root certificate on a phone. That is a manual Android/iOS step.
- It does not prove the phone scan. The evidence file stays `BLOCKED` until a real run is recorded.
