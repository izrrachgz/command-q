# CommandQ

CommandQ is a .NET 10 backend starter solution intended to be reusable across
multiple application types. It uses a modular-monolith shape for the application
API and includes a standalone OpenIddict authorization-server host. That host can
be replaced by another standards-compatible issuer, but the API always requires
a configured authority.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [.NET 10 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [.NET 10 Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

## Start A New Project From This Template

1. Choose the backend modules you want to keep.

   `Identity` and `Health` are core. `Files` is optional. `Contacts`, `Addresses`, `Events` and `Registers` are sample-oriented modules. See `docs/modules.md` before removing modules.

2. Configure local backend secrets.

   Prefer .NET user-secrets for local development. See `docs/local-secrets.md`.

   ```powershell
   dotnet user-secrets set --project .\Api\Api.csproj "ConnectionStrings:Engine" "MsSql"
   dotnet user-secrets set --project .\Api\Api.csproj "ConnectionStrings:Db" "Server=localhost;Database=CommandQ;User ID=commandq;Password=change-me;TrustServerCertificate=True;"
   dotnet user-secrets set --project .\Api\Api.csproj "ApiClients:0:Name" "Local Development"
   dotnet user-secrets set --project .\Api\Api.csproj "ApiClients:0:Id" "replace-with-local-client-secret"
   dotnet user-secrets set --project .\Api\Api.csproj "ApiClients:0:Uri" "http://localhost:4200"
   dotnet user-secrets set --project .\Application.Test\Application.Test.csproj "ConnectionStrings:Engine" "MsSql"
   dotnet user-secrets set --project .\Application.Test\Application.Test.csproj "ConnectionStrings:Db" "Server=localhost;Database=CommandQ;User ID=commandq;Password=change-me;TrustServerCertificate=True;"
   dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "ConnectionStrings:Engine" "PgSql"
   dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "ConnectionStrings:IdentityDb" "Host=localhost;Port=5432;Database=command_q_identity;Username=postgres;Password=change-me;"
   ```

3. Prepare the database.

   Create empty application and identity databases. In Development, each host applies only its pending migrations. While version 1 is still being shaped, manually drop/recreate the affected development database when its baseline migration changes.

4. Build and run the database-independent test suites.

   ```powershell
   .\scripts\build.ps1
   dotnet test .\Api.Test\Api.Test.csproj --no-build
   dotnet test .\Email.Test\Email.Test.csproj --no-build
   dotnet test .\Email.IntegrationTest\Email.IntegrationTest.csproj --no-build
   dotnet test .\IdentityServer.Test\IdentityServer.Test.csproj --no-build
   ```

5. Run the identity server, then the API in separate terminals.

   ```powershell
   .\scripts\run-identity-server.ps1
   .\scripts\run-api.ps1
   ```

   In Visual Studio 2022 17.11 or later, select the shared `Backend (IdentityServer + API)` solution launch profile before pressing F5. It starts IdentityServer first and API second with their HTTPS project profiles.

   After the API has applied the application migrations, run the full suite with
   `.\scripts\test.ps1`. Database-backed `Application.Test` cases read their own
   `ConnectionStrings` user-secrets configured in step 2.

6. Optionally run the frontend.

   The backend does not require the frontend. If you keep `FrontApplication`, run:

   ```powershell
   .\scripts\run-front.ps1
   ```

   The frontend is not required by either backend host.

7. Provision an application profile after the first identity login.

   Obtain an authorization-code token containing `command_q_api`, `profile` and `email`, then call authenticated `POST /Account/create` once. IdentityServer owns credentials; the API stores the application profile linked by issuer and subject.

8. Add a new backend feature.

   Add domain entities under `Domain/Modules/{Module}`, use cases under `Application/Modules/{Module}`, and controllers/endpoints under `Api/Modules/{Module}`. Register services in `Application/Modules/ApplicationModuleExtensions.cs` and API mappings in `Api/Modules/ApiModuleExtensions.cs`.

9. Add a migration.

   For application persistence, add `migration-X.sql` under `Persistence/Migrations/{Engine}/{Module}` for each supported engine you intend to keep. Migration IDs must be unique per engine and scripts must move forward only. For IdentityServer, run `.\scripts\add-identity-migration.ps1 -Name MigrationName` to generate the EF migration for all four providers.

10. Add localized validation text.

   Validators should use `ValidationTextProvider`. Add reusable validation templates to `Localization/Resources/ValidationText*.resx`, field display names to `Localization/Resources/FieldNames*.resx`, and operation messages to `Localization/Resources/Messages*.resx`.

## Solution Layout

```text
Api/
  Modules/
    Identity/
    Contacts/
    Addresses/
    Files/
    Health/

Application/
  Modules/
    Identity/
    Contacts/
    Addresses/
    Files/
    Health/

Domain/
  Modules/
    Identity/
    Contacts/
    Addresses/
    Files/
    Health/
    Events/
    Registers/

Common/
Localization/
Persistence/
IdentityServer/
Email/
Application.Test/
Api.Test/
IdentityServer.Test/
Email.Test/
Email.IntegrationTest/
FrontApplication/  (optional; not referenced by CommandQ.sln)
```

Feature-owned code should live under the matching module folder. Cross-cutting infrastructure remains at project root, for example `Api/Middleware`, `Common/System`, `Persistence/Repository`, and root logging for general infrastructure events.

Module namespaces match the folder shape:

```csharp
Api.Modules.Identity
Application.Modules.Identity.Accounts.Commands
Domain.Modules.Identity
```

Existing modules are documented in `docs/modules.md`, including which modules are core, optional or sample-oriented and how to remove a module safely.

Identity includes a queryable account activity trail for business account actions. Use `IAccountActivityRecorder` or `RecordAccountActivityCommand` instead of writing per-account log files. Authentication security events are stored by IdentityServer.

`IdentityServer` owns users, credentials, lockout, external logins, OAuth clients, scopes, grants and tokens. The API Identity module owns application profiles and maps them through `AccountIdentity` using the validated token issuer and subject. See `IdentityServer/README.md`.

## Package Management

NuGet versions are centrally managed in `Directory.Packages.props`. Project files should reference packages without inline versions:

```xml
<PackageReference Include="MediatR" />
```

## Database Support

This solution supports the following databases:

- **Microsoft SQL Server**
- **PostgreSQL**
- **MySQL**
- **SqLite**

The application uses SqlKata. Its base and Development settings select SQL
Server, while the QA and Production templates select PostgreSQL. IdentityServer
uses EF Core and selects PostgreSQL in every committed environment. Both hosts
use the same engine names and `ConnectionStrings:Engine` path, but the
application reads `ConnectionStrings:Db` and IdentityServer reads
`ConnectionStrings:IdentityDb`. IdentityServer keeps a separate EF migration
set for every provider.

## Configuration
Select the engine through configuration and supply the real connection string
through user-secrets in Development or the deployment environment's secret
store. Keep credentials out of committed `appsettings*.json` files. The
following examples show the application `Db` connection format:

### **Microsoft SQL Server**
```json
"ConnectionStrings": {
  "Engine" : "MsSql",
  "Db": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE_NAME;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}
```

### **PostgreSQL**
```json
"ConnectionStrings": {
  "Engine" : "PgSql",
  "Db": "Host=YOUR_HOST;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;"
}
```

### **MySQL**
```json
"ConnectionStrings": {
  "Engine" : "MySql",
  "Db": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE_NAME;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;"
}
```

### **SqLite**
```json
"ConnectionStrings": {
  "Engine" : "SqLite",
  "Db": "Data Source={path_to_file};Version={version_number};"
}
```

## Running the Application

1. Restore dependencies:
   ```sh
   dotnet restore .\CommandQ.sln
   ```
2. Build the project:
   ```sh
   dotnet build .\CommandQ.sln
   ```
3. Run IdentityServer and then the API in separate terminals:
   ```sh
   dotnet run --project .\IdentityServer\IdentityServer.csproj --launch-profile https
   dotnet run --project .\Api\Api.csproj --launch-profile https
   ```

Root helper scripts are also available:

| Script | Defaults and options |
| --- | --- |
| `.\scripts\build.ps1` | Debug build; accepts `-Configuration` and `-NoRestore`. |
| `.\scripts\test.ps1` | Tests `CommandQ.sln`; accepts `-Configuration`, `-Project`, `-Filter` and `-NoBuild`. The default includes database-backed `Application.Test` cases. |
| `.\scripts\run-identity-server.ps1` | Development on `https://localhost:7125`; accepts `-Environment`, `-Urls` and `-NoBuild`. |
| `.\scripts\run-api.ps1` | Development on `https://localhost:7025` and `http://localhost:7000`; accepts `-Environment`, `-Urls` and `-NoBuild`. |
| `.\scripts\add-identity-migration.ps1 -Name MigrationName` | Builds IdentityServer and scaffolds the named migration for all four EF providers. |
| `.\scripts\run-front.ps1` | Optional frontend runner; accepts `-Install`. |

The backend run scripts bypass `launchSettings.json` deliberately so their
`-Environment` and `-Urls` arguments are authoritative. Visual Studio uses the
separate HTTPS launch profiles from `CommandQ.slnLaunch`. Use `Qa` when loading
the committed `appsettings.Qa.json` files on case-sensitive hosts.

## Postman

Import the backend collection and the appropriate environment from [`postman/`](postman/README.md). The Development environment is prepared for IdentityServer, API, authorization code with PKCE and the former `TurboDecoder` / `admin@local.com` account values. Passwords, API-client keys and issued tokens are intentionally not committed.

## Migrations

For migrations, add a `migration-X.sql` script under `Persistence/Migrations/{Engine}/{Module}` for each database engine/module you intend to support. The configured engine is selected from `ConnectionStrings:Engine`, and the matching migration folder is scanned recursively.

Outside Development, startup migration execution is enabled with:

```json
"Migrations": {
  "RunOnStartup": true
}
```

In Development, the app invokes the migration runner regardless of the toggle
so it can move the local database forward automatically. The runner reads the
`MigrationRecord` table first and only applies pending scripts, so an
already-applied `migration-1.sql` is skipped and the next pending script starts
at `migration-2.sql`.

Outside Development, enable `Migrations:RunOnStartup` only in environments where the configured database is reachable and migrations should be applied by the app.

The v1 migration scripts are forward-only bootstrap scripts. They create schema for an empty database and do not drop existing tables. During pre-v1 development, drop/recreate the development database manually when the baseline schema changes.

If the IdentityServer database is recreated, newly registered users receive new
subject identifiers. Reset stale development `AccountIdentity` mappings or the
application database before provisioning those profiles again; never infer an
identity link from email alone.

The migration root can be overridden when the default source/publish path resolution is not enough:

```json
"Persistence": {
  "MigrationsPath": "C:/path/to/Persistence/Migrations"
}
```

Migration execution is fail-fast:

- scripts are ordered numerically
- already-applied migrations are skipped based on the database migration ledger
- migration IDs must be unique per engine, even across module folders
- missing expected scripts stop startup
- failed scripts stop startup
- successful migrations are recorded in the `MigrationRecord` table with migration id, module, name, checksum and applied time

`Persistence/Migrations/{Engine}/Core/migration-1.sql` creates the initial schema and seeds the `MigrationRecord` table.

IdentityServer migrations are EF Core migrations stored under
`IdentityServer/Persistence/Migrations/{Engine}`. Generate the same schema
change for all supported providers with:

```powershell
.\scripts\add-identity-migration.ps1 -Name MigrationName
```

At runtime IdentityServer applies only pending migrations belonging to its
configured provider. Do not create one shared EF migration snapshot across
providers.

After migration handling, IdentityServer reconciles the default `User` role,
configured API scope and configured OpenIddict clients on every startup. When
startup migrations are disabled outside Development, the selected database must
already contain the current schema before IdentityServer starts.

## Data Access

The persistence layer separates read and write entry points:

- `IReadOnlyRepository` is intended for queries and does not expose the write-capable SqlKata `QueryFactory`.
- `IWritableRepository` is intended for commands and migrations.
- `ICommandRunner` executes raw commands and engine-specific bulk insert operations.
- Repositories and command runners can use the configured connection through DI or be created manually with a connection string and database engine when a service needs a separate database target.
- Repository instances dispose their underlying database connections when the DI scope is disposed.

## Security Defaults

- IdentityServer uses ASP.NET Core Identity password hashing and lockout.
- OpenIddict persists clients, scopes, grants and refresh tokens in `command_q_identity`.
- Access tokens are signed `at+jwt` tokens; the API validates issuer discovery keys, audience, lifetime, type and required scope.
- The API does not store passwords, issue JWTs or handle refresh tokens.
- IdentityServer authentication endpoints are rate limited and security events are persisted separately from application activity.
- Production signing, encryption and Data Protection certificates are required through deployment configuration.
- API clients are validated on startup.
- The API pipeline uses authentication before authorization.
- Production exception responses avoid exposing unexpected exception details.
- Security headers are added by `SecurityHeadersMiddleware`.

## Health Checks

Both backend hosts expose:

- `/health/live` for process liveness
- `/health/ready` for dependency readiness
- `/health/checks` for the full registered health-check set

Background health-check publishing is opt-in through `HealthCheck:PublishOnStartup`.
This publishing setting belongs to the application API. IdentityServer does not
run a background publisher; its readiness check validates its EF database.

## Email Delivery

The standalone `Email` project provides reusable MailKit/MimeKit SMTP delivery.
It is disabled by default and includes validated delivery policies, structured
SMTP acceptance results, classified failures, text and HTML overloads,
file-path and in-memory attachments, CID resources, verbatim rendered HTML
templates, and batch delivery over one connection and authentication session.
SMTP authentication can be selected as anonymous, LOGIN, PLAIN or
caller-supplied OAuth 2.0 bearer authentication.

The project is intentionally not referenced by API or IdentityServer yet. Add a
project reference from the host that owns a concrete email workflow, register
`services.AddEmailServices(configuration)` and keep SMTP credentials in that
host's secret store. See `Email/README.md` for configuration and usage.

The opt-in `Email.IntegrationTest` project exercises the real SMTP and HTTP
interfaces of a local smtp4dev instance. With smtp4dev SMTP on port `25` and its
web UI/API on port `5000`, run:

```powershell
.\scripts\test-email-integration.ps1
```

## Module Registration

Application modules are registered through:

```csharp
services.AddApplicationModules();
```

API endpoints are mapped through:

```csharp
app.MapApiModules(configuration);
```

When adding a feature module, add its services to `Application/Modules/ApplicationModuleExtensions.cs` and its API mapping to `Api/Modules/ApiModuleExtensions.cs` when needed.

Each module owns its registration extension:

```text
Application/Modules/Identity/IdentityModuleExtensions.cs
Api/Modules/Identity/IdentityApiModuleExtensions.cs
```

Module-owned constants should stay close to the module that owns the behavior.
For example, API route segments live under `Api/Modules/{Module}`, reusable
HTTP/API constants live under `Api/Constants`, and module-specific limits or
activity names live under `Application/Modules/{Module}`. Use configuration
instead of constants for values that vary by environment or need runtime tuning.

## Tests

Backend tests mirror the module layout:

```text
Application.Test/
  Modules/
    Identity/
    Contacts/
    Addresses/
    Common/

Api.Test/
  Architecture/
  Modules/
    Common/
    Health/
    Identity/

IdentityServer.Test/
  Architecture/
  Modules/
    Accounts/
  Persistence/

Email.Test/
  Architecture/
  Configuration/
  Services/

Email.IntegrationTest/
  Configuration/
  Services/
```

Architecture rules are enforced by `Application.Test/Architecture/ArchitectureRulesTest.cs`. These tests check project dependency direction, CQRS repository usage, module namespace conventions, duplicate logging event IDs, and module logging event-id ranges. Application composition tests also resolve the real application service graph.

API-layer rules and middleware tests live under `Api.Test`. These tests avoid booting the full API host unless startup/pipeline behavior is the explicit target, because full host tests may require configured database settings.

Some `Application.Test` module tests execute against the configured application
database. They use the test project's own user-secrets and require the current
application migrations to have been applied. Architecture and helper tests can
still be run independently with `--filter` when no database is available.

`IdentityServer.Test` verifies authorization-server dependency boundaries,
provider selection, provider migration snapshots, SQLite migration execution,
password hashing, lockout, external-login persistence and security-event
storage.

`Email.Test` verifies project dependency and logging rules, configuration
validation, MIME message construction, policy limits, structured delivery
results, attachment and template handling, and one-connection batch delivery
through an in-memory SMTP transport. `Email.IntegrationTest` is skipped by
default and verifies complete MIME delivery against a locally running smtp4dev
instance when explicitly enabled.

## Logging

Source-generated logging is organized by ownership:

- project root logging for general infrastructure
- module `Logging/` folders for module-owned events

Event IDs use project and module ranges. See `docs/logging-event-ids.md`.

## Shared Helpers

The `Common` project includes reusable helpers:

- `HttpCall` for JSON, form and string HTTP calls.
- `FileUploader` for multipart file uploads.
- `TaskRunner` for bounded-concurrency asynchronous work.

The separate `Email` project contains SMTP-specific abstractions, MIME message
construction and MailKit transport implementation so those dependencies do not
expand the general-purpose `Common` project.

## Cqrs

Command Query Responsibility Segregation (CQRS) is a design pattern that segregates read and write operations
for a data store into separate data models. This approach allows each model to be optimized independently
and can improve the performance, scalability, and security of an application.

---
For more details, refer to [Cqrs on microsoft](https://learn.microsoft.com/es-mx/azure/architecture/patterns/cqrs)


## Deployment

For deployment instructions, ensure you install the **.NET 10 Hosting Bundle** on the target server and configure the environment variables accordingly.

---
For more details, refer to the [official .NET 10 documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview).
