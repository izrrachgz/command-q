# CommandQ

CommandQ is a .NET 10 backend starter solution intended to be reusable across multiple application types. It uses a modular-monolith shape: one deployable API and shared infrastructure, with feature code grouped by module.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [.NET 10 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [.NET 10 Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

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
Application.Test/
Api.Test/
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

Identity includes a queryable account activity trail for user/account actions. Use `IAccountActivityRecorder` or `RecordAccountActivityCommand` from the Identity module instead of writing per-account log files.

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
3. Run the application:
   ```sh
   dotnet run --project .\Api\Api.csproj
   ```

## Migrations

For migrations, add a `migration-X.sql` script under `Persistence/Migrations/{Engine}/{Module}` for each database engine/module you intend to support. The configured engine is selected from `ConnectionStrings:Engine`, and the matching migration folder is scanned recursively.

Startup migration execution is controlled by:

```json
"Migrations": {
  "RunOnStartup": true
}
```

In Development, the app invokes the migration runner on startup so it can move the local database forward automatically. The runner reads the `Migrations` table first and only applies pending scripts, so an already-applied `migration-1.sql` is skipped and the next pending script starts at `migration-2.sql`.

Outside Development, enable `Migrations:RunOnStartup` only in environments where the configured database is reachable and migrations should be applied by the app.

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
- successful migrations are recorded in the `Migrations` table with migration id, module, name, checksum and applied time

`Persistence/Migrations/{Engine}/Core/migration-1.sql` creates the initial schema and seeds the `Migrations` table.

## Data Access

The persistence layer separates read and write entry points:

- `IReadOnlyRepository` is intended for queries and does not expose the write-capable SqlKata `QueryFactory`.
- `IWritableRepository` is intended for commands and migrations.
- `ICommandRunner` executes raw commands and engine-specific bulk insert operations.
- Repositories and command runners can use the configured connection through DI or be created manually with a connection string and database engine when a service needs a separate database target.
- Repository instances dispose their underlying database connections when the DI scope is disposed.

## Security Defaults

- JWT and auth options are validated on startup.
- API clients are validated on startup.
- The API pipeline uses authentication before authorization.
- Production exception responses avoid exposing unexpected exception details.
- Security headers are added by `SecurityHeadersMiddleware`.
- Refresh-token storage is behind `IRefreshTokenStore`; the default implementation is in-memory and should be replaced with persistent/distributed storage before multi-instance production deployments.

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
```

Architecture rules are enforced by `Application.Test/Architecture/ArchitectureRulesTest.cs`. These tests check project dependency direction, CQRS repository usage, module namespace conventions, duplicate logging event IDs, and module logging event-id ranges. Application composition tests also resolve the real application service graph.

API-layer rules and middleware tests live under `Api.Test`. These tests avoid booting the full API host unless startup/pipeline behavior is the explicit target, because full host tests may require configured database settings.

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
