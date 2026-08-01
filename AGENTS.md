# CommandQ - AGENTS

This file captures practical repository knowledge for coding agents working in this solution.

## Solution
- Framework: .NET 10.
- Solution file: `CommandQ.sln`.
- Restore: `dotnet restore`.
- Build: `dotnet build .\CommandQ.sln`.
- Tests: `dotnet test` or targeted tests with `--filter FullyQualifiedName~...`.
- NuGet package versions are centrally managed in `Directory.Packages.props`; do not add inline `Version` attributes to `PackageReference` items.
- The workspace may contain unrelated user changes. Do not revert files unless explicitly asked.

## Architecture
- The backend follows a modular-monolith shape. Keep one deployable API, but group feature-owned code under module folders.
- Module roots:
  - `Api/Modules/{Module}`
  - `Application/Modules/{Module}`
  - `Domain/Modules/{Module}`
- Module namespaces should match module folders, for example `Application.Modules.Identity.Accounts.Commands` and `Domain.Modules.Identity`.
- Current modules include `Identity`, `Contacts`, `Addresses`, `Files` and `Health`; domain also has `Events` and `Registers`.
- Existing module status and removal steps are documented in `docs/modules.md`.
- Keep cross-cutting infrastructure at project root, such as `Api/Middleware`, `Api/Extensions`, `Common/System`, `Persistence/Repository` and general project logging.
- Prefer module registration over convention scanning. Application composition lives in `Application/Modules/ApplicationModuleExtensions.cs`, with each module owning its `*ModuleExtensions.cs`. API endpoint composition lives in `Api/Modules/ApiModuleExtensions.cs`, with each module owning its `*ApiModuleExtensions.cs`.
- The frontend is intentionally separate/optional right now. Do not change `FrontApplication` unless the user explicitly asks for frontend work.

## Projects

### Api
- ASP.NET Core HTTP API with module controllers, middleware, auth services and app configuration.
- Run locally from `Api` with `dotnet run` or Visual Studio launch profiles.
- JWT/Auth settings are configured through `Jwt` and `Auth` sections in `appsettings*.json`.
- Current authenticated-account data is resolved from JWT claims through `Api/Extensions/ClaimsPrincipalExtensions.cs`.
- Auth refresh flows use `IAccountSessionCache`/`AccountSessionCache` to avoid repeated account/session database reads.
- Generic cache mechanics live in `Common.Interfaces.ITypedCache<TEntry>` and `Common.System.TypedMemoryCache<TEntry>`; add specific cache adapters over this when new typed caches are needed.
- Register account/session refresh caching through `services.AddAccountSessionCache()`.
- `Auth:RefreshCacheMinutes` controls cache entry lifetime; `Auth:RefreshCacheCleanupMinutes` controls the hosted cleanup period.
- `AccountSessionCacheCleanupService` periodically clears account/session cache entries. Keep this in API/auth infrastructure, not health checks.
- Refresh tokens are accessed through `IRefreshTokenStore`; the default `InMemoryRefreshTokenStore` should be replaced with persistent/distributed storage before multi-instance deployments.
- Rate limiting, cache, database and API client settings use constants under `Api/Constants/ConfigKeys.cs`.
- `SecurityHeadersMiddleware` adds browser security headers. Keep security middleware in backend API infrastructure.
- Unexpected exception details are sanitized outside Development in `ExceptionHandlingMiddleware`.

### Application
- CQRS use cases, validators, handlers, mappers, services and health checks, grouped by module under `Application/Modules`.
- Depends on `Domain`, `Common` and `Localization`.
- Keep application logic decoupled from direct I/O. Use injected interfaces such as repositories, text providers and services.
- Validators use localized resources through `ValidationTextProvider`.
- Shared validator limits live in `Application/Constants/ValidationLimits.cs`.
- Register module-owned services explicitly through `AddApplicationModules()` and module-specific methods such as `AddIdentityModule()`.

### Domain
- Domain entities and interfaces. Feature-owned entities live under `Domain/Modules/{Module}`.
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
  - `Common.Interfaces.ITypedCache<TEntry>` is the reusable cache contract.
  - `Common.System.TypedMemoryCache<TEntry>` stores values in `IMemoryCache` using type-scoped keys.
  - Build project-specific cache adapters over `ITypedCache<TEntry>` when settings or domain-specific methods are needed.
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
  - Scripts live under `Persistence/Migrations/{Engine}/{Module}`.
  - Add scripts as `migration-X.sql`.
  - `MigrationExtensions` scans scripts recursively from the folder matching the configured engine.
  - Scripts are ordered numerically.
  - Migration IDs must be unique per engine, even across module folders.
  - Migration failure and missing expected migration files stop startup.
  - `Core/migration-1.sql` creates and seeds the `Migrations` table; later successful migrations are recorded by the runner.

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
- Tests mirror backend modules under `Application.Test/Modules`.
- Architecture rules live under `Application.Test/Architecture`. Keep these updated when adding modules, changing logging ranges, or adjusting dependency boundaries.
- Uses `Xunit.DependencyInjection`.
- Useful targeted examples:
  - `dotnet test .\Application.Test\Application.Test.csproj --filter FullyQualifiedName~Application.Test.Architecture.ArchitectureRulesTest`
  - `dotnet test .\Application.Test\Application.Test.csproj --filter FullyQualifiedName~Application.Test.Modules.Common.System.TaskRunnerTest`
  - `dotnet test .\Application.Test\Application.Test.csproj --filter FullyQualifiedName~Application.Test.Modules.Common.Transfer.FileUploaderTest`

### Api.Test
- xUnit tests for API-layer architecture, controllers and middleware.
- Tests mirror API modules under `Api.Test/Modules`.
- API architecture rules live under `Api.Test/Architecture`.
- Prefer isolated middleware/controller tests unless the goal is specifically to validate full startup/pipeline behavior.
- Full API host tests may run migrations and require local database configuration.
- Useful targeted examples:
  - `dotnet test .\Api.Test\Api.Test.csproj`
  - `dotnet test .\Api.Test\Api.Test.csproj --filter FullyQualifiedName~Api.Test.Architecture.ApiArchitectureRulesTest`

### Cli
- Code-generation utility project with command/query templates.
- Keep generated code consistent with existing CQRS, validator and localization patterns.

### FrontApplication
- Frontend project placeholder/readme area. It is not part of the current backend modularization work. Check its own README and ask/confirm before changing frontend code unless the user explicitly requests it.

## Logging
- Use source-generated logging with `LoggerMessage`.
- General project logging stays under `{Project}/Logging`.
- Module-owned logging lives under `{Project}/Modules/{Module}/Logging`.
- Event ID ranges are documented in `docs/logging-event-ids.md`.
- Project range constants live in:
  - `Api/Logging/ApiEventIdRanges.cs`
  - `Application/Logging/ApplicationEventIdRanges.cs`
  - `Common/Logging/CommonEventIdRanges.cs`
  - `Persistence/Logging/PersistenceEventIdRanges.cs`
- Add cross-cutting IDs to root `*EventIds.cs` files. Add module-owned IDs to module `Logging/*EventIds.cs` files.

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
