# CommandQ

CommandQ is a .NET 10 backend starter solution intended to be reusable across
multiple application types. It uses a modular-monolith shape for the application
API and includes a standalone OpenIddict authorization-server host. That host can
be replaced by another standards-compatible issuer, but the API always requires
a configured authority.

The backend runtime is split into three local hosts:

```text
IdentityServer  ->  API  ->  Email.Outbox  ->  Email.Worker  ->  SMTP
```

IdentityServer issues tokens, the API owns application workflows and queues
notifications, and Email.Worker performs durable delivery. The API does not
send SMTP mail directly.

The Angular SPA lives in `FrontApplication` and is included in the solution for
developer convenience. Backend hosts stay independently runnable and do not
require the SPA process.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- A supported database engine for the hosts or tests you run: SQL Server,
  PostgreSQL, MySQL or SQLite.
- [smtp4dev](https://github.com/rnwood/smtp4dev) for local SMTP delivery and the
  opt-in email integration test.

## Start A New Project From This Template

1. Choose the backend modules you want to keep.

   `Identity` and `Health` are core. `Notifications` is optional infrastructure
   for durable email enqueueing. `Files` is optional. `Contacts`, `Addresses`,
   `Events` and `Registers` are sample-oriented modules. See `docs/modules.md`
   before removing modules.

2. Configure local backend secrets.

   Prefer .NET user-secrets for local development. See `docs/local-secrets.md`.

   ```powershell
   dotnet user-secrets set --project .\Api\Api.csproj "Api:ConnectionStrings:Engine" "MsSql"
   dotnet user-secrets set --project .\Api\Api.csproj "Api:ConnectionStrings:Db" "Server=localhost;Database=CommandQ;User ID=commandq;Password=change-me;TrustServerCertificate=True;"
   dotnet user-secrets set --project .\Api\Api.csproj "Api:EmailOutbox:Persistence:Engine" "MsSql"
   dotnet user-secrets set --project .\Api\Api.csproj "Api:EmailOutbox:Persistence:ConnectionString" "Server=localhost;Database=CommandQ;User ID=commandq;Password=change-me;TrustServerCertificate=True;"
   dotnet user-secrets set --project .\Api\Api.csproj "Api:ApiClients:0:Name" "Local Development"
   dotnet user-secrets set --project .\Api\Api.csproj "Api:ApiClients:0:Id" "replace-with-local-client-secret"
   dotnet user-secrets set --project .\Api\Api.csproj "Api:ApiClients:0:Uri" "http://localhost:4200"
   dotnet user-secrets set --project .\Email.Worker\Email.Worker.csproj "EmailWorker:EmailOutbox:Persistence:Engine" "MsSql"
   dotnet user-secrets set --project .\Email.Worker\Email.Worker.csproj "EmailWorker:EmailOutbox:Persistence:ConnectionString" "Server=localhost;Database=CommandQ;User ID=commandq;Password=change-me;TrustServerCertificate=True;"
   dotnet user-secrets set --project .\Application.Test\Application.Test.csproj "ConnectionStrings:Engine" "MsSql"
   dotnet user-secrets set --project .\Application.Test\Application.Test.csproj "ConnectionStrings:Db" "Server=localhost;Database=CommandQ;User ID=commandq;Password=change-me;TrustServerCertificate=True;"
   dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "IdentityServer:ConnectionStrings:Engine" "PgSql"
   dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "IdentityServer:ConnectionStrings:IdentityDb" "Host=localhost;Port=5432;Database=command_q_identity;Username=postgres;Password=change-me;"
   ```

3. Prepare the database.

   Create empty application and identity databases. In Development, each host applies only its pending migrations. While version 1 is still being shaped, manually drop/recreate the affected development database when its baseline migration changes.

4. Build and run the isolated test suites.

   ```powershell
   .\scripts\build.ps1
   dotnet test .\Application.Migrations.Test\Application.Migrations.Test.csproj --no-build
   dotnet test .\Api.Test\Api.Test.csproj --no-build
   dotnet test .\Application.Test\Application.Test.csproj --filter FullyQualifiedName~Application.Test.Architecture --no-build
   dotnet test .\Email.Test\Email.Test.csproj --no-build
   dotnet test .\Email.Outbox.Test\Email.Outbox.Test.csproj --no-build
   dotnet test .\IdentityServer.Test\IdentityServer.Test.csproj --no-build
   ```

   `Email.IntegrationTest` is opt-in and requires a running smtp4dev instance.
   Database-backed `Application.Test` cases require the test project's own
   user-secrets and an up-to-date application database.

5. Run the identity server, the API, and the email worker in separate terminals.

   ```powershell
   .\scripts\run-identity-server.ps1
   .\scripts\run-api.ps1
   .\scripts\run-email-worker.ps1
   ```

   In Visual Studio 2022 17.11 or later, select the shared `Backend + Email Worker` solution launch profile before pressing F5. It starts IdentityServer first, API second and the email worker third.

   After the API has applied the application migrations, run the full suite with
   `.\scripts\test.ps1`. Database-backed `Application.Test` cases read their own
   `ConnectionStrings` user-secrets configured in step 2.

6. Run the frontend when you need the browser client.

   ```powershell
   .\scripts\run-front.ps1 -Environment Development
   ```

   `npm start` inside `FrontApplication` does the same Development launch and
   starts the local-only frontend log writer.

7. Provision an application profile after the first identity login.

   Obtain an authorization-code token containing `command_q_api`, `profile` and `email`, then call authenticated `POST /Account/create` once. IdentityServer owns credentials; the API stores the application profile linked by issuer and subject.

8. Add a new backend feature.

   Add domain entities under `Domain/Modules/{Module}`, use cases under `Application/Modules/{Module}`, and controllers/endpoints under `Api/Modules/{Module}`. Register services in `Application/Modules/ApplicationModuleExtensions.cs` and API mappings in `Api/Modules/ApiModuleExtensions.cs`.

9. Add a migration.

   For application persistence, add `migration-X.sql` under `Application.Migrations/Migrations/{Engine}/{Module}` for each supported engine you intend to keep. Migration IDs must be unique per engine and scripts must move forward only. The project embeds these scripts in its assembly, so no runtime migration-files path is required. For IdentityServer, run `.\scripts\add-identity-migration.ps1 -Name MigrationName` to generate the EF migration for all four providers.

10. Add user-facing text and reusable constants.

   Backend validators should use `ValidationTextProvider`. Add reusable validation templates to `Localization/Resources/ValidationText*.resx`, field display names to `Localization/Resources/FieldNames*.resx`, and operation messages to `Localization/Resources/Messages*.resx`. IdentityServer-owned UI/configuration text lives in `IdentityServer/Resources/IdentityServerMessages.resx`. The SPA keeps UI text in `FrontApplication/src/app/core/i18n/translations.ts`, TypeScript keys in `TextKeys`, and route/API/storage/protocol/report strings in owned constants.

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
    Notifications/

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
Application.Migrations/
IdentityServer/
Email/
Email.Outbox/
Email.Outbox.Persistence/
Email.Worker/
Application.Test/
Application.Migrations.Test/
Api.Test/
IdentityServer.Test/
Email.Test/
Email.IntegrationTest/
Email.Outbox.Test/
FrontApplication/  (Angular SPA included for local development)
```

Feature-owned code should live under the matching module folder. Cross-cutting infrastructure remains at project root, for example `Api/Middleware`, `Common/System`, `Persistence/Repository`, and root logging for general infrastructure events.

The Visual Studio solution view groups projects into `Hosts`, `Core`,
`Infrastructure` and `Tests`. These are solution folders only; the physical
project directories, namespaces, project references and build behavior remain
unchanged.

Module namespaces match the folder shape:

```csharp
Api.Modules.Identity
Application.Modules.Identity.Accounts.Commands
Domain.Modules.Identity
```

Existing modules are documented in `docs/modules.md`, including which modules are core, optional or sample-oriented and how to remove a module safely.

Identity includes a queryable account activity trail for business account actions. Use `IAccountActivityRecorder` or `RecordAccountActivityCommand` instead of writing per-account log files. Authentication security events are stored by IdentityServer.

Account preferences are available to authenticated clients through `GET` and
`PUT /Account/preferences`. The backend stores culture, theme, navigation and
accessibility flags, dense-table display, and an IANA timezone identifier. A
successful preference update also writes the serialized preference snapshot to
account activity metadata.

The API validates `Api:Application:TimeZone` once at startup with NodaTime and
records the effective timezone, assembly version, language and environment in
`ApplicationConfiguration`. Admin users can query or append configuration
snapshots through `/Admin/configurations`.

`IdentityServer` owns users, credentials, lockout, external logins, OAuth clients,
scopes, grants and tokens. The API Identity module owns application profiles and
maps them through `AccountIdentity` using the validated token issuer and subject.
Admin role management is exposed to the SPA through protected API endpoints under
`/Admin/*`; the API calls IdentityServer through the server-side
`Api:IdentityServerAdmin` integration and never exposes the internal admin key
to the browser.
See `IdentityServer/README.md`.

## Package Management

NuGet versions are centrally managed in `Directory.Packages.props`. Project files should reference packages without inline versions:

```xml
<PackageReference Include="MediatR" />
```

## Runtime Configuration Reload

The API reloads JSON and environment configuration for operational settings when
the configuration provider supports reload-on-change. Cache response headers,
rate-limit policies, feature flags, health-check publishing and localization use
the current values for new work. `Api:HealthCheck:RunOnStartup` is startup-bound
because startup checks run before the server begins accepting traffic. External
API clients created through `IExternalApiClientFactory` also use the current
endpoint and timeout.

The framework `Logging` section and Serilog `MinimumLevel`, overrides and sink
thresholds are also reloadable through the standard host configuration pipeline.
Changes affect new log events; they do not require rebuilding the host.

Database providers, migrations, dependency-injection registrations, identity
certificates and other infrastructure topology remain startup-bound. Outbox
retry and migration policies are startup-bound. Email profile changes are
monitored by the worker and apply to new batches; active SMTP connections keep
their captured profile snapshot.

## Database Support

This solution supports the following databases:

- **Microsoft SQL Server**
- **PostgreSQL**
- **MySQL**
- **SqLite**

The application uses SqlKata and IdentityServer uses EF Core. Each host selects
its engine independently through `Api:ConnectionStrings:Engine` or
`IdentityServer:ConnectionStrings:Engine`. The API reads
`Api:ConnectionStrings:Db`; IdentityServer reads
`IdentityServer:ConnectionStrings:IdentityDb` and keeps a separate EF migration
set for every provider.

## Configuration

Select the engine through configuration and supply the real connection string
through user-secrets in Development or the deployment environment's secret
store. Keep credentials out of committed `appsettings*.json` files. The
following examples show the application `Db` connection format:

Custom settings are grouped by executable host: `Api`, `IdentityServer` and
`EmailWorker`. Framework-owned sections such as `Logging`, `Serilog` and
`AllowedHosts` remain at the document root. Reusable libraries receive an
explicit subsection from their composition root, so they do not depend on a
host name. This also keeps environment variables unambiguous, for example
`Api__Migrations__RunOnStartup` and
`EmailWorker__Migrations__RunOnStartup` control different processes.

### **Microsoft SQL Server**
```json
"Api": {
  "ConnectionStrings": {
    "Engine" : "MsSql",
    "Db": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE_NAME;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

### **PostgreSQL**
```json
"Api": {
  "ConnectionStrings": {
    "Engine" : "PgSql",
    "Db": "Host=YOUR_HOST;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;"
  }
}
```

### **MySQL**
```json
"Api": {
  "ConnectionStrings": {
    "Engine" : "MySql",
    "Db": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE_NAME;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;"
  }
}
```

### **SqLite**
```json
"Api": {
  "ConnectionStrings": {
    "Engine" : "SqLite",
    "Db": "Data Source={path_to_file};Version={version_number};"
  }
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
3. Run IdentityServer, the API and Email.Worker in separate terminals:
   ```sh
   dotnet run --project .\IdentityServer\IdentityServer.csproj --launch-profile https
   dotnet run --project .\Api\Api.csproj --launch-profile https
   dotnet run --project .\Email.Worker\Email.Worker.csproj --launch-profile Email.Worker
   ```

Root helper scripts are also available:

| Script | Defaults and options |
| --- | --- |
| `.\scripts\build.ps1` | Debug build; accepts `-Configuration` and `-NoRestore`. |
| `.\scripts\test.ps1` | Tests `CommandQ.sln`; accepts `-Configuration`, `-Project`, `-Filter` and `-NoBuild`. The default includes database-backed `Application.Test` cases. |
| `.\scripts\run-identity-server.ps1` | Development on `https://localhost:7125`; accepts `-Environment`, `-Urls` and `-NoBuild`. |
| `.\scripts\run-api.ps1` | Development on `https://localhost:7025` and `http://localhost:7000`; accepts `-Environment`, `-Urls` and `-NoBuild`. |
| `.\scripts\run-email-worker.ps1` | Development email outbox worker using the independently migrated outbox schema in local MsSql `command_q`; accepts `-Environment` and `-NoBuild`. |
| `.\scripts\add-identity-migration.ps1 -Name MigrationName` | Builds IdentityServer and scaffolds the named migration for all four EF providers. |
| `.\scripts\run-front.ps1` | Frontend runner for `FrontApplication`; accepts `-Environment` and `-Install`. |

The backend run scripts bypass `launchSettings.json` deliberately so their
`-Environment` and `-Urls` arguments are authoritative. Visual Studio uses the
solution launch profiles from `CommandQ.slnLaunch`; the recommended `Backend +
Email Worker` profile starts all three hosts. Use `Qa` when loading the committed
`appsettings.Qa.json` files on case-sensitive hosts.

## Postman

Import the backend collection and the appropriate environment from
[`postman/`](postman/README.md). The collection exercises IdentityServer, the
API and authorization code with PKCE. Passwords, API-client keys and issued
tokens are intentionally not committed; follow the Postman README for local
account setup.

## Migrations

Application schema ownership lives in `Application.Migrations`, separate from the
generic database access in `Persistence`. Add a `migration-X.sql` script under
`Application.Migrations/Migrations/{Engine}/{Module}` for each database
engine/module you intend to support. The scripts are embedded in the migration
assembly at build time. At runtime, the configured `Api:ConnectionStrings:Engine`
selects the matching embedded script set.

Outside Development, startup migration execution is enabled with:

```json
"Api": {
  "Migrations": {
    "RunOnStartup": true
  }
}
```

In Development, the app invokes the migration runner regardless of the toggle
so it can move the local database forward automatically. The runner reads the
`MigrationRecord` table first and only applies pending scripts, so an
already-applied `migration-1.sql` is skipped and the next pending script starts
at the next contiguous migration ID. The current pre-v1 schema remains entirely
in `migration-1.sql`.

Outside Development, enable `Api:Migrations:RunOnStartup` only in environments where the configured database is reachable and migrations should be applied by the app.

The v1 migration scripts are forward-only bootstrap scripts. They create schema for an empty database and do not drop existing tables. During pre-v1 development, drop/recreate the development database manually when the baseline schema changes.

If the IdentityServer database is recreated, newly registered users receive new
subject identifiers. Reset stale development `AccountIdentity` mappings or the
application database before provisioning those profiles again; never infer an
identity link from email alone.

Migration execution is fail-fast:

- scripts are ordered numerically
- already-applied migrations are skipped based on the database migration ledger
- migration IDs must be unique per engine, even across module folders
- missing expected scripts stop startup
- a database ledger ahead of the deployed script assembly stops startup
- failed scripts stop startup
- successful migrations are recorded in the `MigrationRecord` table with migration id, module, name, checksum and applied time

`Application.Migrations/Migrations/{Engine}/Core/migration-1.sql` creates the
initial schema and seeds the `MigrationRecord` table. The migration ledger model
and runner also live in `Application.Migrations`; neither belongs to the domain
model or reusable persistence library.

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
the `Admin` role, configured API scope and configured OpenIddict clients on every
startup. In Development only, if no admin user exists yet, the first existing
identity user is granted `Admin` so a freshly migrated local account can manage
roles. When startup migrations are disabled outside Development, the selected
database must already contain the current schema before IdentityServer starts.

## Data Access

The persistence layer separates read and write entry points:

- `IReadOnlyRepository` is intended for queries and does not expose the write-capable SqlKata `QueryFactory`.
- `IWritableRepository` is intended for write-capable consumers, including application commands and the separate application migration runner.
- `ICommandRunner` executes raw commands and engine-specific bulk insert operations.
- Repositories and command runners can use the configured connection through DI or be created manually with a connection string and database engine when a service needs a separate database target.
- Repository instances dispose their underlying database connections when the DI scope is disposed.

## Security Defaults

- IdentityServer uses ASP.NET Core Identity password hashing and lockout.
- OpenIddict persists clients, scopes, grants and refresh tokens in `command_q_identity`.
- Access tokens are signed `at+jwt` tokens; the API validates issuer discovery keys, audience, lifetime, type and required scope.
- The API does not store passwords, issue JWTs or handle refresh tokens.
- IdentityServer authentication endpoints are rate limited and security events are persisted separately from application activity.
- IdentityServer role administration is an internal server-to-server surface. Keep `IdentityServer:AdminApi:ApiKey` and `Api:IdentityServerAdmin:ApiKey` secret and matching in environments where role management is enabled.
- Production signing, encryption and Data Protection certificates are required through deployment configuration.
- API clients are validated on startup.
- The API pipeline uses authentication before authorization.
- Production exception responses avoid exposing unexpected exception details.
- Security headers are added by `SecurityHeadersMiddleware`.

## Health Checks

API, IdentityServer and Email.Worker expose:

- `/health/live` for process liveness
- `/health/ready` for dependency readiness
- `/health/checks` for the full registered health-check set

API readiness is intentionally cheap and host-local. It uses the `ready` tag and
does not call the email worker or run mutating diagnostics. Deeper API checks
use the `startup` and `publish` tags.

Startup health checks are opt-in through `Api:HealthCheck:RunOnStartup`.
`Api:HealthCheck:FailStartupOnUnhealthy` controls whether an unhealthy startup
report stops the API host. Background health-check publishing is opt-in through
`Api:HealthCheck:PublishReports`. The legacy `PublishOnStartup` setting is still
accepted as a publishing alias for existing local configuration.

IdentityServer does not run a background publisher; its readiness check validates
its EF database. Email.Worker readiness validates email outbox persistence.

## OpenTelemetry

OpenTelemetry is disabled by default and is configured independently by the API
and email worker hosts. Enable it with `Api:OpenTelemetry:Enabled` or
`EmailWorker:OpenTelemetry:Enabled`, then provide the host service name and an
OTLP endpoint under the same section. The hosts own SDK registration and
exporters; reusable libraries only define their local `ActivitySource` and
`Meter` instrumentation.

The API subscribes to HTTP server and email-outbox instrumentation. The worker
subscribes to HTTP server, SMTP, email-outbox and polling instrumentation. The
telemetry uses only low-cardinality profile, engine, failure-category and
outcome tags. Recipients, subjects, bodies, tokens, correlation IDs and outbox
IDs are never emitted as tags.

The current instrumentation measures email SMTP connection, authentication,
MIME construction, send and disconnect operations; outbox enqueue, duplicate
enqueue, serialize, claim, process, retry, dead-letter, batch size and queue
age; and worker poll and batch activity. Named SMTP profiles are low-cardinality
telemetry tags and outbox records persist only the logical profile name. The
outbox also persists W3C trace-parent and trace-state values when an enqueue
activity has a W3C context; the worker uses those contexts as links on the
profile processing activity without exposing them as metric tags.

## Email Delivery

The standalone `Email` project provides reusable MailKit/MimeKit SMTP delivery.
It is disabled by default and includes validated delivery policies, structured
SMTP acceptance results, classified failures, text and HTML overloads,
file-path and in-memory attachments, CID resources, verbatim rendered HTML
templates, and batch delivery over one connection and authentication session.
SMTP authentication can be selected as anonymous, LOGIN, PLAIN or
caller-supplied OAuth 2.0 bearer authentication.

Email supports named SMTP profiles. Each profile owns its endpoint,
authentication, sender validation and delivery limits, including raw content
limits, attachment MIME-type and extension policies, unknown-content handling
and exact serialized MIME-size limits. The outbox stores only the selected
logical profile, groups delivery by profile and opens one SMTP connection per
group. The durable `default` marker resolves to the worker's configured
`Email:DefaultProfile`; a profile removed from configuration is dead-lettered as
a permanent configuration failure. Application notifications allow the default
marker by default; sensitive named profiles require an explicit application-
provided `IEmailProfileSelectionPolicy`.

The API remains an outbox producer and does not compose SMTP delivery.
Immediate-delivery consumers can register
`services.AddEmailServices(configuration)`. Durable delivery uses
`Email.Outbox`, its dedicated persistence adapter and the standalone
`Email.Worker` host. Development targets local MsSql `command_q`, while PgSql,
MySql and SqLite remain configurable and non-development hosts must supply a
connection. See `Email/README.md`, `Email.Outbox/README.md` and
`Email.Worker/README.md`.

Development is prepared for smtp4dev on `localhost:25` and stores the independently
migrated outbox schema in the local MsSql `command_q` database. Start the worker separately:

```powershell
.\scripts\run-email-worker.ps1
```

The Application `Notifications` module references the reusable email and
outbox contracts and provides `QueueEmailCommand` plus
`IApplicationEmailService`. The service offers
prepared-message, text, HTML, attachment and verbatim-template overloads; all
paths converge on one durable `EmailOutboxRequest` carrying optional
idempotency, account and correlation metadata. Delivery and retries remain the
worker's responsibility.

The module does not select storage. The API composition root registers the
outbox core and persistence adapter against the configured email outbox store,
while the worker owns schema migration and delivery. No generic email API
endpoint is exposed by default.

The opt-in `Email.IntegrationTest` project exercises the real SMTP and HTTP
interfaces of a local smtp4dev instance. With smtp4dev SMTP on port `25` and its
web UI/API on port `5000`, run:

```powershell
.\scripts\test-email-integration.ps1
```

## Module Registration

Application modules are registered through:

```csharp
services.AddApplicationModules(configuration);
```

API endpoints are mapped through:

```csharp
app.MapApiModules(configuration);
```

When adding a feature module, add its services to `Application/Modules/ApplicationModuleExtensions.cs` and its API mapping to `Api/Modules/ApiModuleExtensions.cs` when needed. The module's own `*ModuleExtensions.cs` file explicitly registers its MediatR handlers, validators, mappers and internal services; do not reintroduce application-wide assembly scanning. Keep commands, queries and contracts public for API/test boundaries, while implementation classes remain internal to the Application assembly.

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

## Text Resources And Constants

Use resource providers for backend text that can reach users, operators or
clients. Application validation and operation messages live in the shared
`Localization` project. IdentityServer keeps its host-owned sign-in page,
configuration and protocol response messages in
`IdentityServer/Resources/IdentityServerMessages.resx` so future localization
can extend the same keys.

Keep externally observable or repeated strings behind constants near their
owner. Host configuration paths belong in `Constants/ConfigKeys.cs`; API route
segments stay with the owning API module; reusable HTTP names stay under
`Api/Constants`; frontend route, storage, OIDC protocol, API query/header,
reporting and diagnostic strings stay under the matching `FrontApplication`
`core` folder. Values that change by environment belong in configuration rather
than constants.

## Tests

Backend tests mirror the module layout:

```text
Application.Test/
  Modules/
    Identity/
    Contacts/
    Addresses/
    Notifications/
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

Email.Outbox.Test/
  Architecture/
  Persistence/
  Serialization/
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

`Email.Outbox.Test` verifies durable message snapshots, batching, retry/release
behavior, architecture boundaries and the dedicated SQLite migration and store
lifecycle.

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
- `ITypedCache<TEntry>` and its memory-cache implementation for typed caches.

The separate `Email` project contains SMTP-specific abstractions, MIME message
construction and MailKit transport implementation so those dependencies do not
expand the general-purpose `Common` project.

## Deployment

For deployment, provide host-scoped environment variables or secret-store
values for database connections, IdentityServer issuer/certificates, API
authentication and SMTP credentials. Do not copy local user-secrets into
committed settings. See [`docs/local-secrets.md`](docs/local-secrets.md),
[`IdentityServer/README.md`](IdentityServer/README.md) and
[`Email.Worker/README.md`](Email.Worker/README.md).
