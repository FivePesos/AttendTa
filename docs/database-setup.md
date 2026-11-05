# Database setup (MVP LocalDB)

AttendTa runs on one SQL Server database built from the canonical script
`database/attendta-mvp-schema.sql`. This guide sets up the disposable local database
`AttendTaMVP_Test` and provisions the first Administrator.

Do not point these scripts at a shared or production database. The schema, reset and seed
scripts refuse any database other than `AttendTaMVP_Test`, `AttendTa_IT_<guid>` or
`AttendTa_Demo`.

## Prerequisites

- .NET 10 SDK
- SQL Server LocalDB (`sqllocaldb info` should list `MSSQLLocalDB`)
- `sqlcmd` (ships with SQL Server and the ODBC client tools)

The examples below use LocalDB and PowerShell.

## 1. Reset the disposable database

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -i "database\reset-local-mvp-test-db.sql"
```

This drops and recreates only `AttendTaMVP_Test`.

## 2. Apply the canonical schema

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -d "AttendTaMVP_Test" -i "database\attendta-mvp-schema.sql"
```

The script creates all ten business tables, the eight lookup tables, the filtered unique
indexes, the append-only `activity_logs` trigger and `READ_COMMITTED_SNAPSHOT ON`.

## 3. Generate the Administrator password hash

Never write a plaintext password or a precomputed hash into a tracked file. Build once, then run
the hasher without rebuilding so stdout contains only the PHC string (the timing note goes to
stderr).

```powershell
dotnet build "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"
$hash = dotnet run --project "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj" --no-build -- --hash-password "<choose-a-strong-password>"
```

`$hash` is exactly one `$argon2id$v=19$m=19456,t=2,p=1$...` line. Keep it in the current shell
session only.

## 4. Provision the first Administrator

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -d "AttendTaMVP_Test" `
  -i "database\seed-local-admin.sql" `
  -v AdminName="AttendTa Local Admin" `
     AdminEmail="admin@example.edu" `
     AdminPasswordHash="$hash"
```

What the script does:

- refuses a missing name, a malformed email, or a hash that is not a `$argon2id$` PHC string;
- inserts one active Administrator (`role_id = 1`, `is_active = 1`) with `created_by = NULL`;
- appends exactly one system audit row to `activity_logs`
  (`actor_type_id = 2`, `action = 'user.provisioned'`, `entity_type = 'users'`);
- never stores or prints the plaintext password, and never writes the hash into the audit row;
- is idempotent: re-running it with the same email leaves the existing active Administrator and
  its audit row untouched.

There are no default credentials. Public requests never grant a privileged role.

## 5. Seed the starter event types

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -d "AttendTaMVP_Test" -i "database\seed-event-types.sql"
```

The script selects the active Administrator by role (not by a hard-coded email) for the required
`created_by` field, and is idempotent.

## 6. Run the app

The connection string is not committed. Set it for the session:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=(localdb)\MSSQLLocalDB;Database=AttendTaMVP_Test;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
dotnet run --project "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"
```

The app fails fast with a clear message when `ConnectionStrings:DefaultConnection` is missing.

## 7. Sign in

Open the app and sign in with the `AdminEmail` and password you provisioned. Sign-in is by
email, not username.

- The cookie is named `AttendTa.Auth`, is non-persistent, lasts 8 hours and does not slide.
- `OnValidatePrincipal` reloads the account on every request: deactivating the account or
  changing its role in the database rejects the session on its next request.
- Every failure (unknown email, wrong password, inactive account) shows one generic message.
- Logout is POST-only and requires an antiforgery token; a GET to `/Account/Logout` returns 405.

Data Protection keys are stored under `<Common:SetupRoot>/keys` so cookies survive restarts.
Deleting that folder signs every user out.

## Integration tests

The integration test project creates and drops its own `AttendTa_IT_<guid>` database for each
test class, from the same canonical script. Point it at a local SQL Server **master** database:

```powershell
$env:ATTENDTA_TEST_SQL = "Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"
dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj
```

The principal in `ATTENDTA_TEST_SQL` must be allowed to create and drop databases on a local
instance.
