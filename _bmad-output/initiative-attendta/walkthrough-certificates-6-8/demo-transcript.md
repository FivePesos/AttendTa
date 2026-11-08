# Demo transcript - story 6.8 authorized certificate download

Captured 2026-11-08 01:12:48 +08:00 against http://localhost:5080 (AttendTaMVP_Test, DemoData on).

## Owner read - joshua.reyes@uc.edu.ph

Sign-in: HTTP 302

`GET /Certificates/Pdf/2`

- Status: 200
- Content-Type: application/pdf
- Cache-Control: no-store,no-cache
- Bytes: 446093
- First bytes: %PDF-

## Non-owner and unknown id - liza.fernandez@uc.edu.ph

Sign-in: HTTP 302

| Request | Status | Body bytes | Message |
|---|---|---|---|
| `/Certificates/Pdf/2` (joshua's certificate) | 404 | 3280 | True |
| `/Certificates/Pdf/999999` (unknown) | 404 | 3285 | True |

Both are 404 with the same "Certificate not found." alert. The bodies differ only by the id in the page's retry link.
