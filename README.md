# command-q

command-q is a .NET 10 backend starter solution intended to be reusable across
multiple application types. It uses a modular-monolith shape for the application
API and includes a standalone OpenIddict authorization-server host. That host can
be replaced by another standards-compatible issuer, but the API always requires
a configured authority.

The runtime spans two command-q hosts and the independently deployed email service:

```text
IdentityServer  ->  encrypted identity dispatch  ->  command-emails/Email.Api (gRPC)  ->  outbox
API             -------------------------------->  command-emails/Email.Api (gRPC)  ->  outbox
command-emails/Email.Worker  ------------------------------------------------------->  SMTP
```

IdentityServer issues tokens, the API owns application workflows and queues
notifications, and the sibling
[`command-emails`](https://github.com/izrrachgz/command-emails) repository owns
durable delivery. The API does not send SMTP mail directly.

The Angular SPA is maintained and released independently in
[`command-q-front`](https://github.com/izrrachgz/command-q-front). The browser
client consumes this backend through HTTP and OpenID Connect; it is not a build
dependency of the backend solution.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- A supported database engine for the hosts or tests you run: SQL Server,
  PostgreSQL, MySQL or SQLite.
- A running `command-emails` Email.Api and Email.Worker when durable email is needed.

## Start A New Project From This Template

1. Choose the backend modules you want to keep.

   `Identity`, `Configuration` and `Health` are core. `Notifications` is
   optional infrastructure for durable email enqueueing. `Files` is optional.
   `Contacts`, `Addresses`, `Events` and `Registers` are sample-oriented
   modules. See `docs/modules.md` before removing modules.

2. Configure local backend secrets.

   Prefer .NET user-secrets for local development. See `docs/local-secrets.md`.

   ```powershell
   dotnet user-secrets set --project .\Api\Api.csproj "Api:ConnectionStrings:Engine" "MsSql"
   dotnet user-secrets set --project .\Api\Api.csproj "Api:ConnectionStrings:Db" "Server=localhost;Database=command-q;User ID=command-q;Password=change-me;TrustServerCertificate=True;"
   dotnet user-secrets set --project .\Api\Api.csproj "Api:EmailApiClient:Password" "replace-with-email-api-password"
   dotnet user-secrets set --project .\Api\Api.csproj "Api:ApiClients:0:Name" "Local Development"
   dotnet user-secrets set --project .\Api\Api.csproj "Api:ApiClients:0:Id" "replace-with-local-client-secret"
   dotnet user-secrets set --project .\Api\Api.csproj "Api:ApiClients:0:Uri" "http://localhost:4200"
   dotnet user-secrets set --project .\Application.Test\Application.Test.csproj "ConnectionStrings:Engine" "MsSql"
   dotnet user-secrets set --project .\Application.Test\Application.Test.csproj "ConnectionStrings:Db" "Server=localhost;Database=command-q;User ID=command-q;Password=change-me;TrustServerCertificate=True;"
   dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "IdentityServer:ConnectionStrings:Engine" "PgSql"
   dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "IdentityServer:ConnectionStrings:IdentityDb" "Host=localhost;Port=5432;Database=command-q_identity;Username=postgres;Password=change-me;"
   dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "IdentityServer:EmailApiClient:Password" "replace-with-email-api-password"
   ```

3. Prepare the database.

   Create empty application and identity databases. In Development, each host applies only its pending migrations. While version 1 is still being shaped, manually drop/recreate the affected development database when its baseline migration changes.

4. Build and run the isolated test suites.

   ```powershell
   .\scripts\build.ps1
   dotnet test .\Cqrs.Test\Cqrs.Test.csproj --no-build
   dotnet test .\Application.Migrations.Test\Application.Migrations.Test.csproj --no-build
   dotnet test .\Api.Test\Api.Test.csproj --no-build
   dotnet test .\Application.Test\Application.Test.csproj --filter FullyQualifiedName~Application.Test.Architecture --no-build
   dotnet test .\IdentityServer.Test\IdentityServer.Test.csproj --no-build
   dotnet test .\Time.Test\Time.Test.csproj --no-build
   ```

   Email-specific tests live in `command-emails`. Database-backed
   `Application.Test` cases require the test project's own
   user-secrets and an up-to-date application database.

5. Run the identity server and API in separate terminals.

   ```powershell
   .\scripts\run-identity-server.ps1
   .\scripts\run-api.ps1
   ```

   In Visual Studio 2022 17.11 or later, select the `Backend (IdentityServer +
   API)` solution launch profile before pressing F5. Run the email worker from a
   sibling `command-emails` checkout with its `Email API + Worker` launch profile
   when durable delivery is needed.

   After the API has applied the application migrations, run the full suite with
   `.\scripts\test.ps1`. Database-backed `Application.Test` cases read their own
   `ConnectionStrings` user-secrets configured in step 2.

6. Run the frontend from a sibling checkout when you need the browser client.

   ```powershell
   git clone https://github.com/izrrachgz/command-q-front.git ..\command-q-front
   ..\command-q-front\scripts\run.ps1 -Environment Development -Install
   ```

   See the frontend repository README for its environment, test and release
   commands. The backend and frontend repositories can be checked out side by
   side, but neither build depends on the other's filesystem layout.

7. Provision an application profile after the first identity login.

   Obtain an authorization-code token containing `command-q_api`, `profile` and `email`, then call authenticated `POST /accounts` once. IdentityServer owns credentials; the API stores the application profile linked by issuer and subject.

8. Add a new backend feature.

   Add domain entities under `Domain/Modules/{Module}`, use cases under `Application/Modules/{Module}`, and controllers/endpoints under `Api/Modules/{Module}`. Register services in `Application/Modules/ApplicationModuleExtensions.cs` and API mappings in `Api/Modules/ApiModuleExtensions.cs`.

9. Add a migration.

   For application persistence, add `migration-X.sql` under `Application.Migrations/Migrations/{Engine}/{Module}` for each supported engine you intend to keep. Migration IDs must be unique per engine and scripts must move forward only. The project embeds these scripts in its assembly, so no runtime migration-files path is required. For IdentityServer, run `.\scripts\add-identity-migration.ps1 -Name MigrationName` to generate the EF migration for all four providers.

10. Add user-facing text and reusable constants.

   Backend validators should use `ValidationTextProvider`. Add reusable validation templates to `Localization/Resources/ValidationText*.resx`, field display names to `Localization/Resources/FieldNames*.resx`, and operation messages to `Localization/Resources/Messages*.resx`. IdentityServer-owned UI/configuration text lives in `IdentityServer/Resources/IdentityServerMessages.resx`. Frontend localization and TypeScript constants are owned by the separate `command-q-front` repository.

## Solution Layout

```text
Api/
  Modules/
    Identity/
    Configuration/
    Contacts/
    Addresses/
    Files/
    Health/

Application/
  Modules/
    Identity/
    Configuration/
    Contacts/
    Addresses/
    Files/
    Health/
    Notifications/

Domain/
  Modules/
    Identity/
    Configuration/
    Contacts/
    Addresses/
    Files/
    Health/
    Events/
    Registers/

Common/
Cqrs/
Time/
Localization/
Persistence/
Application.Migrations/
IdentityServer/
Application.Test/
Application.Migrations.Test/
Api.Test/
IdentityServer.Test/
Cqrs.Test/
Time.Test/
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

[API success and error shapes](docs/api-responses.md) are documented centrally. Controllers expose typed resources and pages, while all client and server errors use the shared `ProblemDetails` contract.

Identity includes a queryable account activity trail for business account actions. Use `IAccountActivityRecorder` or `RecordAccountActivityCommand` instead of writing per-account log files. Authentication security events are stored by IdentityServer.

Account preferences are available to authenticated clients through `GET` and
`PUT /accounts/me/preferences`. The backend stores culture, theme, navigation and
accessibility flags, dense-table display, and an IANA timezone identifier. A
successful preference update also writes the serialized preference snapshot to
account activity metadata.

The API validates `Api:Application:TimeZone` once at startup with NodaTime and
records the effective timezone, assembly version, language, environment and
server instance name in
`ApplicationConfiguration`. Admin users can query the server-recorded startup
history through `GET /admin/configurations`; clients cannot append snapshots.

Paged backend searches accept an optional `StartDate`/`EndDate` range as
ISO-8601 UTC instants. Supply both values together; both timestamps must have
zero offset (normally serialized with a trailing `Z`) and `EndDate` cannot
precede `StartDate`. Database filters remain in UTC. Account-local conversion
uses the timezone stored in account preferences rather than a per-search value.

`IdentityServer` owns users, credentials, lockout, external logins, OAuth clients,
scopes, grants and tokens. The API Identity module owns application profiles and
maps them through `AccountIdentity` using the validated token issuer and subject.
Admin role management is exposed to the SPA through protected API endpoints under
`/admin/*`; the API calls IdentityServer through the server-side
`Api:IdentityServerAdmin` integration and never exposes the internal admin key
to the browser. The same boundary exposes administrator-only account unlock at
`POST /admin/accounts/{accountId}/unlock` for a future administration page.
See `IdentityServer/README.md`.

## Package Management

NuGet versions are centrally managed in `Directory.Packages.props`. Project files should reference packages without inline versions:

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
```

### Shared assemblies

Every successful non-design-time build publishes `Common.dll`, `Cqrs.dll`,
`Persistence.dll` and `Time.dll` to the sibling `..\command-q-build-output`
directory. The export is implemented in `Directory.Build.targets` and provides
a deliberate binary boundary for separate solutions that reuse command-q's
framework-light libraries. The last successfully built configuration wins.

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

Custom settings are grouped by executable host: `Api` and `IdentityServer`.
Framework-owned sections such as `Logging`, `Serilog` and `AllowedHosts` remain
at the document root. Reusable libraries receive an explicit subsection from
their composition root, so they do not depend on a host name. The external
email worker owns its `EmailWorker` configuration in `command-emails`.

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
   dotnet restore .\command-q.sln
   ```
2. Build the project:
   ```sh
   dotnet build .\command-q.sln
   ```
3. Run IdentityServer and the API in separate terminals:
   ```sh
   dotnet run --project .\IdentityServer\IdentityServer.csproj --launch-profile https
   dotnet run --project .\Api\Api.csproj --launch-profile https
   ```

Root helper scripts are also available:

| Script | Defaults and options |
| --- | --- |
| `.\scripts\build.ps1` | Debug build; accepts `-Configuration` and `-NoRestore`. |
| `.\scripts\test.ps1` | Tests `command-q.sln`; accepts `-Configuration`, `-Project`, `-Filter` and `-NoBuild`. The default includes database-backed `Application.Test` cases. |
| `.\scripts\run-identity-server.ps1` | Development on `https://localhost:7125`; accepts `-Environment`, `-Urls` and `-NoBuild`. |
| `.\scripts\run-api.ps1` | Development on `https://localhost:7025` and `http://localhost:7000`; accepts `-Environment`, `-Urls` and `-NoBuild`. |
| `.\scripts\add-identity-migration.ps1 -Name MigrationName` | Builds IdentityServer and scaffolds the named migration for all four EF providers. |
| `.\scripts\request-account-email.ps1 -Email address@example.test -Purpose Confirmation` | Requests a confirmation or password-recovery message from local IdentityServer for inspection in smtp4dev. |
| `.\scripts\test-account-email-templates.ps1` | Sends confirmation and recovery template previews directly to local smtp4dev without requiring account or database setup. |

The backend run scripts bypass `launchSettings.json` deliberately so their
`-Environment` and `-Urls` arguments are authoritative. Visual Studio uses the
solution launch profiles from `command-q.slnLaunch`; the backend profile starts
IdentityServer and the API. Use `Qa` when loading the committed
`appsettings.Qa.json` files on case-sensitive hosts. Run the worker from
`command-emails`.

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
- OpenIddict persists clients, scopes, grants and refresh tokens in `command-q_identity`.
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

API and IdentityServer expose:

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
its EF database. Email worker readiness is owned by `command-emails`.

## OpenTelemetry

OpenTelemetry is disabled by default in the API. Enable it with
`Api:OpenTelemetry:Enabled`, then provide the host service name and an OTLP
endpoint under the same section. Email worker telemetry is configured in
`command-emails`.

The API subscribes to HTTP server and email-outbox instrumentation. Its telemetry
uses only low-cardinality engine and outcome tags. Recipients, subjects, bodies,
tokens, correlation IDs and outbox IDs are never emitted as tags.

The outbox persists W3C trace-parent and trace-state values when an enqueue
activity has a W3C context; the worker uses those contexts as links on the
profile processing activity without exposing them as metric tags.

Backend projects use the shared `Time` project for injectable `TimeProvider`
registration, NodaTime conversion, TZDB validation, strict UTC JSON handling and
the canonical microsecond UTC persistence precision. Current values are
normalized when they enter persistence, so equality does not depend on a
provider silently rounding or truncating sub-microsecond ticks. Feature projects
continue to own their timestamp-bearing models and scheduling policy. EF Core
and Dapper/SqlKata adapters remain with their persistence owners and reuse the
shared UTC primitives. SQL Server uses `datetime2(6)`, PostgreSQL uses
`timestamptz(6)`, MySQL uses UTC-session `datetime(6)`, and SQLite stores the
shared fixed six-digit UTC pattern ending in `Z`. During
version-1 development, the complete outbox schema remains folded into one
`migration-1.sql` baseline per provider.

## Email Delivery

SMTP delivery, outbox processing, persistence migrations, the authenticated
enqueue API and worker live in
[`command-emails`](https://github.com/izrrachgz/command-emails). Email
implementation DLLs are no longer copied into or loaded by command-q.

The API and IdentityServer enqueue through the reusable `Email.Client` project
and do not compose SMTP delivery or persistence. The client authenticates with
Email.Api and uses the protobuf endpoint; Email.Api also exposes an equivalent
JSON REST endpoint for interop and diagnostics.
IdentityServer uses the outbox for confirmation and password-recovery messages;
its authentication data remains EF Core-backed in the separate identity
database.

The Application `Notifications` module references host-neutral Email.Client
contracts and provides `QueueEmailCommand` plus
`IApplicationEmailService`. The service offers
prepared-message, text, HTML, attachment and verbatim-template overloads; all
paths converge on one `EmailQueueRequest` carrying optional
idempotency, account and correlation metadata. Delivery and retries remain the
external worker's responsibility.

The module does not select storage. command-emails owns API authentication,
schema migration and delivery. SMTP configuration and email-specific tests are
documented in that repository.

## Module Registration

Application modules are registered through:

```csharp
services.AddApplicationModules(configuration);
```

API endpoints are mapped through:

```csharp
app.MapApiModules(configuration);
```

When adding a feature module, add its services to `Application/Modules/ApplicationModuleExtensions.cs` and its API mapping to `Api/Modules/ApiModuleExtensions.cs` when needed. The module's own `*ModuleExtensions.cs` file explicitly registers each CQRS request through `AddApplicationRequest<TRequest, TResponse, THandler>()`, plus its validators, mappers and internal services. Do not introduce application-wide assembly scanning. Keep commands, queries and contracts public for API/test boundaries, while implementation classes remain internal to the Application assembly.

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
`Api/Constants`. Frontend-owned strings and configuration stay in the separate
`command-q-front` repository. Values that change by environment belong in
configuration rather than constants.

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

Cqrs.Test/
  Architecture/
  Dispatching/

Time.Test/
  Architecture/
  Extensions/
  Serialization/
```

Architecture rules are enforced by `Application.Test/Architecture/ArchitectureRulesTest.cs`. These tests check project dependency direction, CQRS repository usage, module namespace conventions, duplicate logging event IDs, and module logging event-id ranges. Application composition tests also resolve the real application service graph.

API-layer rules and middleware tests live under `Api.Test`. These tests avoid booting the full API host unless startup/pipeline behavior is the explicit target, because full host tests may require configured database settings.

Some `Application.Test` module tests execute against the configured application
database. They use the test project's own user-secrets and require the current
application migrations to have been applied. Architecture and helper tests can
still be run independently with `--filter` when no database is available.

`IdentityServer.Test` verifies authorization-server dependency boundaries,
provider selection, provider migration snapshots, UTC conversion across all
provider models, SQLite migration and UTC round-trip behavior, password hashing,
lockout, external-login persistence and security-event storage.

`Cqrs.Test` verifies handler dispatch, command aliases, pipeline order, scoped
dependency resolution, cancellation forwarding, duplicate registration and
missing-handler failures. Its architecture rule keeps the dispatcher core
independent from application, feature, persistence and transport projects.

`Time.Test` verifies strict UTC serialization, TZDB wall-clock conversion,
canonical persistence formatting, `TimeProvider` conversion and DI override
behavior. Its architecture rule keeps the shared project independent from
feature, host and persistence projects.

## Logging

Source-generated logging is organized by ownership:

- project root logging for general infrastructure
- module `Logging/` folders for module-owned events

Event IDs use project and module ranges. See `docs/logging-event-ids.md`.

## Shared Helpers

The separate `Cqrs` project owns the AOT-compatible in-process dispatcher,
command/query contracts and strongly typed pipeline behaviors. Handlers are
registered explicitly, with no assembly scanning or runtime generic type
construction. Application keeps its FluentValidation adapter and registers
each request through `AddApplicationRequest`. See
[`Cqrs/README.md`](Cqrs/README.md) and the
[`future-feature notes`](Cqrs/FUTURE-FEATURES.md).

The `Common` project includes reusable helpers:

- `HttpCall` for JSON, form and string HTTP calls.
- `FileUploader` for multipart file uploads.
- `TaskRunner` for bounded-concurrency asynchronous work.
- `ITypedCache<TEntry>` and its memory-cache implementation for typed caches.

The separate `Time` project owns shared UTC and timezone mechanics: the default
`TimeProvider` registration, conversion to NodaTime `Instant`, TZDB wall-clock
conversion and validation, strict UTC JSON converters, and the canonical UTC
persistence pattern. Provider-specific database adapters remain in their
owning persistence projects.

The sibling `command-emails` solution owns SMTP-specific abstractions, MIME
construction, MailKit transport, durable outbox processing, its authenticated
API and worker host. This solution consumes only its protobuf client contract.

## Deployment

For deployment, provide host-scoped environment variables or secret-store
values for database connections, IdentityServer issuer/certificates and API
authentication. SMTP credentials belong to `command-emails`. Do not copy local
user-secrets into committed settings. See
[`docs/local-secrets.md`](docs/local-secrets.md),
[`IdentityServer/README.md`](IdentityServer/README.md) and the
[`command-emails` repository](https://github.com/izrrachgz/command-emails).
