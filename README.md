# CommandQ

CommandQ is a .NET 10 backend starter solution intended to be reusable across multiple application types. It uses a modular-monolith shape for the application API and includes an optional, standalone OpenIddict authorization-server host.

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
   dotnet user-secrets set --project .\Api\Api.csproj "ApiClients:0:Uri" "https://localhost:4200"
   dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "ConnectionStrings:Engine" "PgSql"
   dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "ConnectionStrings:IdentityDb" "Host=localhost;Port=5432;Database=command_q_identity;Username=postgres;Password=change-me;"
   ```

3. Prepare the database.

   Create empty application and identity databases. In Development, each host applies only its pending migrations. While version 1 is still being shaped, manually drop/recreate both development databases when either baseline migration changes.

4. Build and test.

   ```powershell
   .\scripts\build.ps1
   .\scripts\test.ps1
   ```

5. Run the identity server, then the API.

   ```powershell
   .\scripts\run-identity-server.ps1
   .\scripts\run-api.ps1
   ```

   In Visual Studio 2022 17.11 or later, select the shared `Backend (IdentityServer + API)` solution launch profile before pressing F5. It starts IdentityServer first and API second with their HTTPS project profiles.

6. Optionally run the frontend placeholder.

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

   Add `migration-X.sql` under `Persistence/Migrations/{Engine}/{Module}` for each supported engine you intend to keep. Migration IDs must be unique per engine and scripts must move forward only.

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
Application.Test/
Api.Test/
IdentityServer.Test/
Cli/
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

The application uses SqlKata and defaults to SQL Server. IdentityServer uses EF
Core and defaults to PostgreSQL. Both hosts use the same engine names and
configuration pattern, but each process owns its connection string and database.
IdentityServer keeps a separate EF migration set for every provider.

## Configuration
Update the `appsettings.json` file with the appropriate connection string for your database. Example:

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
3. Run IdentityServer and then the API:
   ```sh
   dotnet run --project .\IdentityServer\IdentityServer.csproj
   dotnet run --project .\Api\Api.csproj
   ```

Root helper scripts are also available:

```powershell
.\scripts\build.ps1
.\scripts\test.ps1
.\scripts\run-api.ps1
.\scripts\run-identity-server.ps1
.\scripts\add-identity-migration.ps1 -Name MigrationName
.\scripts\run-front.ps1
```

## Postman

Import the backend collection and the appropriate environment from [`postman/`](postman/README.md). The Development environment is prepared for IdentityServer, API, authorization code with PKCE and the former `TurboDecoder` / `admin@local.com` account values. Passwords, API-client keys and issued tokens are intentionally not committed.

## Migrations

For migrations, add a `migration-X.sql` script under `Persistence/Migrations/{Engine}/{Module}` for each database engine/module you intend to support. The configured engine is selected from `ConnectionStrings:Engine`, and the matching migration folder is scanned recursively.

Startup migration execution is controlled by:

```json
"Migrations": {
  "RunOnStartup": true
}
```

In Development, the app invokes the migration runner on startup so it can move the local database forward automatically. The runner reads the `MigrationRecord` table first and only applies pending scripts, so an already-applied `migration-1.sql` is skipped and the next pending script starts at `migration-2.sql`.

Outside Development, enable `Migrations:RunOnStartup` only in environments where the configured database is reachable and migrations should be applied by the app.

The v1 migration scripts are forward-only bootstrap scripts. They create schema for an empty database and do not drop existing tables. During pre-v1 development, drop/recreate the development database manually when the baseline schema changes.

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

The API exposes:

- `/health/live` for process liveness
- `/health/ready` for dependency readiness
- `/health/checks` for the full registered health-check set

Background health-check publishing is opt-in through `HealthCheck:PublishOnStartup`.

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

IdentityServer.Test/
  Architecture/
  Modules/
    Accounts/
```

Architecture rules are enforced by `Application.Test/Architecture/ArchitectureRulesTest.cs`. These tests check project dependency direction, CQRS repository usage, module namespace conventions, duplicate logging event IDs, and module logging event-id ranges. Application composition tests also resolve the real application service graph.

API-layer rules and middleware tests live under `Api.Test`. These tests avoid booting the full API host unless startup/pipeline behavior is the explicit target, because full host tests may require configured database settings.

`IdentityServer.Test` verifies authorization-server dependency boundaries,
provider selection, provider migration snapshots, SQLite migration execution,
password hashing, lockout, external-login persistence and security-event
storage.

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
