# CommandQ - AGENTS

This file captures practical repository knowledge for coding agents working in this solution.

## Solution
- Framework: .NET 10.
- Solution file: `CommandQ.sln`.
- Restore: `dotnet restore`.
- Build: `dotnet build .\CommandQ.sln`.
- Tests: `dotnet test` or targeted tests with `--filter FullyQualifiedName~...`.
- The workspace may contain unrelated user changes. Do not revert files unless explicitly asked.

## Projects

### Api
- ASP.NET Core HTTP API with controllers, middleware, auth services and app configuration.
- Run locally from `Api` with `dotnet run` or Visual Studio launch profiles.
- JWT/Auth settings are configured through `Jwt` and `Auth` sections in `appsettings*.json`.
- Current authenticated-account data is resolved from JWT claims through `Api/Extensions/ClaimsPrincipalExtensions.cs`.
- Auth refresh flows use `IAccountSessionCache`/`AccountSessionCache` to avoid repeated account/session database reads.
- Generic cache mechanics live in `Common.Interfaces.ITypeCache<TItem>` and `Common.System.TypeMemoryCache<TItem>`; add specific cache adapters over this when new typed caches are needed.
- Register account/session refresh caching through `services.AddAccountSessionCache()`.
- `Auth:RefreshCacheMinutes` controls cache entry lifetime; `Auth:RefreshCacheCleanupMinutes` controls the hosted cleanup period.
- `AccountSessionCacheCleanupService` periodically clears account/session cache entries. Keep this in API/auth infrastructure, not health checks.
- Refresh tokens are kept in a thread-safe in-memory store in `RefreshTokenService`; use persistent/distributed storage before multi-instance deployments.
- Rate limiting, cache, database and API client settings use constants under `Api/Constants/ConfigKeys.cs`.

### Application
- CQRS use cases, validators, handlers, mappers, services and health checks.
- Depends on `Domain`, `Common` and `Localization`.
- Keep application logic decoupled from direct I/O. Use injected interfaces such as repositories, text providers and services.
- Validators use localized resources through `ValidationTextProvider`.
- Shared validator limits live in `Application/Constants/ValidationLimits.cs`.

### Domain
- Domain entities and interfaces.
- Entity contracts live here:
  - `IEntity<TKey>`
  - `IAuditableEntity`
  - `ISoftDeletableEntity`
- `BaseEntity<TKey>` provides typed identity plus lifecycle timestamps.
- `BaseEntity` remains the long-key default, while `Account` uses `BaseEntity<Guid>`.
- Keep this project free from infrastructure implementation details.

### Common
- Shared utilities, constants, exceptions, CQRS abstractions, response/request models and transport helpers.
- HTTP helpers:
  - `Common.Transfer.Http.HttpCall` for JSON/form/string HTTP calls.
  - `Common.Transfer.Http.FileUploader` for multipart uploads.
  - `Common.Models.Transfer.FileUploadItem` is the reusable upload file model.
- Async helper:
  - `Common.System.TaskRunner` runs async operations with bounded concurrency.
  - Register with `services.AddTaskRunner()` and inject `ITaskRunner`.
  - Default concurrency is `TaskRunnerConstants.DefaultMaxConcurrency`.
- Typed memory cache:
  - `Common.Interfaces.ITypeCache<TItem>` is the reusable cache contract.
  - `Common.System.TypeMemoryCache<TItem>` stores values in `IMemoryCache` using type-scoped keys.
  - Build project-specific cache adapters over `ITypeCache<TItem>` when settings or domain-specific methods are needed.
- Keep this project stable because it is widely referenced.

### Persistence
- SqlKata-backed repositories, database command runners and SQL migrations.
- Persistence-facing abstractions live under `Persistence.Interfaces`, including:
  - `IReadOnlyRepository`
  - `IWritableRepository`
  - `ICommandRunner`
- Supported engines: `MsSql`, `PgSql`, `MySql`, `SqLite`.
- Configuration keys:
  - main connection string name: `Db`
  - selected engine path: `ConnectionStrings:Engine`
- Repositories:
  - `IReadOnlyRepository` is intended for query handlers and does not expose write-capable SqlKata APIs.
  - `IWritableRepository` is intended for commands/migrations and exposes `QueryFactory Db`.
  - `ReadOnlyRepository` and `WritableRepository` can be resolved from DI using configured settings or manually created with a connection string and `DatabaseEngine`.
- Command runners:
  - `ICommandRunner` executes raw commands and engine-specific bulk inserts.
  - `AddCommandRunners` registers all concrete runners and resolves the default `ICommandRunner` from `ConnectionStrings:Engine`.
  - Concrete runners can be constructed manually with a connection string when a service needs a separate runtime database target.
- Migrations:
  - Scripts live under `Persistence/Migrations/{Engine}`.
  - Add scripts as `migration-X.sql`.
  - `MigrationExtensions` runs scripts from the folder matching the configured engine.

### Localization
- Resource project for multi-language text.
- Resource groups:
  - `Messages`: operation messages.
  - `ValidationText`: reusable validation templates.
  - `FieldNames`: localized field display names for validators.
- Providers:
  - `ResxTextProvider`
  - `ValidationTextProvider`
  - `CompositeTextProvider`
  - `JsonTextProvider`
- `AddLocalizationServices` accepts `LocalizationOptions` configuration for provider order, supported cultures, fallback culture and JSON locale path.
- Text providers are scoped so request/current UI culture changes are honored per scope.
- When adding validator text, update every supported `.resx` language file.

### Application.Test
- xUnit tests for application and shared helper behavior.
- Uses `Xunit.DependencyInjection`.
- Useful targeted examples:
  - `dotnet test .\Application.Test\Application.Test.csproj --filter FullyQualifiedName~Application.Test.Common.System.TaskRunnerTest`
  - `dotnet test .\Application.Test\Application.Test.csproj --filter FullyQualifiedName~Application.Test.Common.Transfer.FileUploaderTest`

### Cli
- Code-generation utility project with command/query templates.
- Keep generated code consistent with existing CQRS, validator and localization patterns.

### FrontApplication
- Frontend project placeholder/readme area. Check its own README before changing frontend code.

## Coding Conventions
- Prefer existing project patterns over new abstractions.
- Use `rg`/`rg --files` for searches.
- Use `apply_patch` for manual edits.
- Avoid unrelated refactors and metadata churn.
- Add focused tests when changing shared helpers, DI, repositories, command runners or user-facing behavior.
- Do not expose write-capable persistence APIs to query handlers.
- Keep command handlers using writable dependencies and query handlers using read-only dependencies.
- Use resource providers for user-facing messages and validation text instead of hardcoded strings.

## Verification
- Run `dotnet build .\CommandQ.sln` after code changes.
- Run targeted tests for touched areas.
- Full `dotnet test` can include integration-style tests depending on local database configuration, so prefer targeted tests when validating isolated helpers.
