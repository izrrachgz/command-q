# command-q - AGENTS

This file captures practical repository knowledge for coding agents working in this solution.

## Solution
- Framework: .NET 10.
- Solution file: `command-q.sln`.
- Restore: `dotnet restore`.
- Build: `dotnet build .\command-q.sln`.
- Tests: `dotnet test` or targeted tests with `--filter FullyQualifiedName~...`.
- Helper scripts live under `scripts/`:
  - `.\scripts\build.ps1`
  - `.\scripts\test.ps1`
  - `.\scripts\run-api.ps1`
  - `.\scripts\run-identity-server.ps1`
  - `.\scripts\add-identity-migration.ps1 -Name MigrationName`
- `command-q.slnLaunch` provides backend-only launch profiles. The Angular SPA is developed in the separate `command-q-front` repository.
- The backend run scripts bypass launch profiles so `-Environment` and `-Urls` are authoritative. Defaults are `https://localhost:7125` for IdentityServer and `https://localhost:7025;http://localhost:7000` for API. Use the exact `Qa` environment name for `appsettings.Qa.json` on case-sensitive hosts.
- Custom executable settings are namespaced by host: `Api` and `IdentityServer`. Framework-owned `Logging`, `Serilog` and `AllowedHosts` sections remain top-level. Hosts pass explicit subsections to reusable libraries; do not make a library depend on a host prefix.
- Backend Postman artifacts live under `postman/`. Keep committed environments free from passwords, API-client keys, client secrets and issued tokens; local environment exports should use the ignored `*.local.postman_environment.json` suffix.
- NuGet package versions are centrally managed in `Directory.Packages.props`; do not add inline `Version` attributes to `PackageReference` items.
- Every successful non-design-time build copies `Common.dll`, `Cqrs.dll`, `Persistence.dll` and `Time.dll` to the sibling `..\command-q-build-output` directory through `Directory.Build.targets`; the last successful build wins. Keep that binary-export boundary limited to stable reusable projects.
- `Email.Client` owns command-q's versioned protobuf client and host-neutral queue contracts. Application, Api and IdentityServer reference it instead of loading `Email*.dll` assemblies.
- Do not add project references from command-q into the sibling `command-emails` repository. Keep the duplicated `.proto` wire contract compatible across repositories.
- The workspace may contain unrelated user changes. Do not revert files unless explicitly asked.

## Architecture
- The backend follows a modular-monolith shape. Keep one deployable API, but group feature-owned code under module folders.
- Module roots:
  - `Api/Modules/{Module}`
  - `Application/Modules/{Module}`
  - `Domain/Modules/{Module}`
- Module namespaces should match module folders, for example `Application.Modules.Identity.Accounts.Commands` and `Domain.Modules.Identity`.
- Current modules include `Identity`, `Configuration`, `Contacts`, `Addresses`, `Files`, `Health` and `Notifications`; domain also has `Events` and `Registers`.
- The application Identity module owns profiles, external identity mapping and queryable business account activity. Record business actions through `IAccountActivityRecorder` or `RecordAccountActivityCommand`; do not create per-account application log files as the source of truth.
- `IdentityServer` owns authentication users, password hashes, lockout, external logins, OAuth clients, scopes, grants and token issuance. Do not move these responsibilities back into `Api` or `Application`.
- Existing module status and removal steps are documented in `docs/modules.md`.
- Keep cross-cutting infrastructure at project root, such as `Api/Middleware`, `Api/Extensions`, `Common/System`, `Persistence/Repository` and general project logging.
- Prefer explicit module registration over convention scanning. Application composition lives in `Application/Modules/ApplicationModuleExtensions.cs`, with each module owning its `*ModuleExtensions.cs`; those extensions explicitly register CQRS requests through `AddApplicationRequest<TRequest, TResponse, THandler>()`, plus validators, mappers and module services. Do not use application-wide exported-type scanning for these services. Keep commands, queries and contracts public for API/test boundaries, while module implementation classes remain internal to the Application assembly. API endpoint composition lives in `Api/Modules/ApiModuleExtensions.cs`, with each module owning its `*ApiModuleExtensions.cs`.
- Host implementation types should be internal by default. Keep API and IdentityServer controllers public for framework discovery, but seal concrete controllers. Keep host configuration models, middleware, persistence helpers and concrete services internal unless another project consumes them through an intentional contract.
- The Angular frontend lives in `https://github.com/izrrachgz/command-q-front` and is not part of this solution. Coordinate browser-facing API and OIDC changes across the two repositories while keeping their build and release lifecycles independent.
- When diagnosing browser, authentication-page or frontend integration failures, inspect the sibling frontend's latest `Logs/front-application-log-YYYYMMDD.txt` file before relying only on a pasted browser error. Development frontend logging is written there by the local log companion process.

## Projects

### Api
- ASP.NET Core HTTP API with module controllers, middleware, resource-server authentication and app configuration.
- Run locally from `Api` with `dotnet run` or Visual Studio launch profiles.
- Runtime application database selection lives under `Api:ConnectionStrings`; the engine key is `Api:ConnectionStrings:Engine`.
- PostgreSQL and SQL Server application databases enforce native account row-level security. API startup verifies all required policies after optional migrations and fails before serving requests when a policy is missing or disabled. MySQL and SQLite are intentionally skipped because they do not offer an equivalent native table-policy feature. See `docs/row-level-security.md`.
- Bearer validation settings are configured through the `Authentication` section in `appsettings*.json`.
- The API validates IdentityServer discovery keys, audience, lifetime, `at+jwt` type and required API scope. It does not issue JWTs or handle refresh tokens.
- Protocol claims are read through `Api/Extensions/ClaimsPrincipalExtensions.cs`; `ICurrentAccountAccessor` resolves `(iss, sub)` to the application profile through `GetAccountByIdentityQuery`.
- `POST /accounts` provisions an application profile for an authenticated identity. Credentials are never accepted or stored by this endpoint.
- API routes use lowercase resource names. Authenticated current-account resources live under `/accounts/me`, and admin resources live under `/admin`.
- API modules own transport-only request DTOs under module `Requests` folders and map them to application commands in controllers.
- API controllers use `ActionResult<T>` for known success payloads and do not expose internal `BasicResponse`, `DetailResponse<T>` or `PagedResponse<T>` wrappers. Public pages use `Api.Models.PageResponse<T>` and command acknowledgements use `OperationResponse`.
- API errors use centralized `ProblemDetails`/`ValidationProblemDetails` with stable `errorCode` and `traceId` extensions. Keep the contract and status mapping aligned with `docs/api-responses.md`.
- The Files API module owns reusable 200 MB upload policies for general files, documents, images, audio, video and ZIP archives. Specialized future endpoints should select the narrowest policy that fits.
- Generic cache mechanics live in `Common.Abstractions.ITypedCache<TEntry>` and `Common.System.TypedMemoryCache<TEntry>`; add specific cache adapters over this when new typed caches are needed.
- API-owned settings live under the `Api` section. Rate limiting, cache, database and API client child paths use constants under `Api/Constants/ConfigKeys.cs`.
- Ordinary API traffic uses the configuration-selected token-bucket default; administrator controllers use the sliding-window policy, account provisioning uses fixed-window limiting and uploads use an instance-global concurrency policy. Authenticated partitions use `(iss, sub)` and anonymous requests fall back to the remote IP. Keep `docs/rate-limiting.md` aligned with policy assignments and defaults.
- Operational API settings use `IOptionsMonitor` where runtime reload is safe: response cache headers, rate-limit policies, feature flags, localization and health publishing. `Api:Localization:DefaultCulture` is the request default; `Api:Localization:SupportedCultures` controls accepted cultures. External clients should be created through `IExternalApiClientFactory` so endpoint and timeout changes apply to new clients.
- Local secrets should use .NET user-secrets or environment variables. Keep real secrets out of `appsettings*.json`, `.env`, scripts and docs. See `docs/local-secrets.md`.
- `SecurityHeadersMiddleware` adds browser security headers. Keep security middleware in backend API infrastructure.
- Unexpected exception details are sanitized outside Development by `ApiExceptionHandler`, registered through ASP.NET Core's exception-handler pipeline.
- The API queues notifications through `Email.Client` and command-emails' authenticated protobuf endpoint. It owns no email outbox persistence, migration lifecycle or SMTP delivery.

### IdentityServer
- Standalone ASP.NET Core Identity and OpenIddict authorization-server host. It is independently deployable and connected to `Api` through standard OpenID Connect discovery, not a project reference.
- Run locally with `.\scripts\run-identity-server.ps1`; the Development issuer is `https://localhost:7125/`.
- OpenIddict uses an isolated database named `command-q_identity` through EF Core stores.
- IdentityServer supports `PgSql`, `MsSql`, `MySql` and `SqLite` through `IdentityServer:ConnectionStrings:Engine`.
- Keep IdentityServer on EF Core. Do not introduce SqlKata into this host or share the application's persistence registration.
- Every EF `DateTime` property uses the IdentityServer UTC converter, including nullable OpenIddict timestamps. It rejects non-UTC writes and restores `DateTimeKind.Utc` after provider reads; keep this convention active for all four provider models.
- The shared Identity/OpenIddict model lives in `IdentityServerDbContext`; provider-specific contexts under `IdentityServer/Persistence/Providers` select the EF provider and migration set.
- ASP.NET Core Identity owns local users, password hashing, lockout, roles, claims and external login links.
- OpenIddict owns clients, scopes, authorizations, access/refresh tokens and revocation. Access tokens are signed `at+jwt` tokens; authorization and refresh tokens remain encrypted.
- Account endpoints live under `IdentityServer/Modules/Accounts`; authorization/token handlers and client/scope seeding live under `IdentityServer/Modules/OpenIddict`.
- Google and Microsoft providers are registered only when complete `ExternalProviders` credentials are configured.
- Authentication security events are stored in `IdentitySecurityEvent`; application business activity remains in `AccountActivity`.
- Local accounts require email confirmation. IdentityServer persists hashed, expiring account challenges and transactionally stages Data Protection-encrypted email payloads before idempotently queueing them through Email.Api. It never owns outbox persistence or delivers SMTP directly.
- IdentityServer uses fixed-window credential limits, sliding-window account-challenge and internal-administration limits, and a token-bucket default for ordinary controller traffic. Keep health checks outside controller policy assignment and `docs/rate-limiting.md` aligned with defaults.
- Account email templates use the shared embedded layout under `IdentityServer/Modules/Accounts/Templates/Email`; add both `en-US` and `es-ES` content fragments for each purpose. Links carry a random one-time token in the URL fragment and emails carry a separate six-digit code.
- Three failed confirmation-code checks invalidate only that challenge. Password recovery has separate three-attempt verification and password-submission budgets; exhausting either creates an explicit indefinite administrator-required lock. The browser uses the API's Admin-role-protected unlock route, while the API alone calls the API-key-protected IdentityServer endpoint. Unlocking revokes active challenges.
- EF migrations live under `IdentityServer/Persistence/Migrations/{Engine}`. Use `.\scripts\add-identity-migration.ps1 -Name MigrationName` so every model change is generated for all four providers.
- Do not combine provider migrations into one snapshot. EF migration sets must remain aligned across `PgSql`, `MsSql`, `MySql` and `SqLite`.
- Provider design-time factories live as separate files under `IdentityServer/Persistence/DesignTime` and use non-routable placeholder connections for scaffolding only. Apply database migrations through configured IdentityServer startup, not `dotnet ef database update`.
- Development applies pending EF migrations at startup. Non-development migration execution is opt-in through `IdentityServer:Migrations:RunOnStartup`.
- After migration handling, every IdentityServer startup reconciles the default role, API scope and configured OpenIddict clients. A current schema is therefore required even when startup migrations are disabled.
- Development certificates are allowed only in Development. QA/Production require signing, encryption and Data Protection certificates plus shared persisted Data Protection keys.

### Application
- CQRS use cases, validators, handlers, mappers, services and health checks, grouped by module under `Application/Modules`.
- Depends on `Domain`, `Common`, `Cqrs`, `Localization`, `Persistence` and the reusable `Email.Client` contracts.
- Keep application logic decoupled from direct I/O. Use injected interfaces such as repositories, text providers and services.
- Every registered CQRS request runs through the account RLS behavior. Account-owned requests implement `IAccountScopedRequest`; trusted cross-account identity lookup and account provisioning implement `ISystemScopedRequest`. Unclassified requests are provisioned with restrictive/no-account context and fail closed on protected tables. Handlers and services must not provision session context manually. The behavior applies context to the read-only repository for queries and the writable repository for commands, including command transactions, then restores restrictive context before pooling.
- Validators use localized resources through `ValidationTextProvider`.
- Shared validator limits live in `Application/Constants/ValidationLimits.cs`.
- Register module-owned services explicitly through `AddApplicationModules(configuration)` and module-specific methods such as `AddIdentityModule()`. Module extensions own request registrations through `AddApplicationRequest<TRequest, TResponse, THandler>()`, as well as validators and mappers; do not move those registrations back into a project-wide scanner. The helper attaches the Application-owned FluentValidation behavior to each closed request pipeline.
- Application accounts are business profiles. Keep passwords, auth lockout and protocol sessions out of Application.
- `Application.Modules.Notifications` owns `QueueEmailCommand` and `IApplicationEmailService`. Queueing is the default; do not add an immediate-send switch to the command.
- Notification overloads converge on `ApplicationEmailRequest`, which carries a prepared `EmailMessage` plus optional idempotency, account and correlation metadata.
- The Notifications module must not reference email persistence or worker implementations. Hosts register `Email.Client`; Email.Api owns the outbox and the worker owns SMTP delivery/retries.
- Do not expose a generic notification endpoint without explicit authorization and abuse controls.

### Domain
- Domain entities and interfaces. Feature-owned entities live under `Domain/Modules/{Module}`.
- Entity contracts live here:
  - `IEntity<TKey>`
  - `IAuditableEntity`
  - `ISoftDeletableEntity`
- `BaseEntity<TKey>` provides typed identity plus lifecycle timestamps.
- `BaseEntity` remains the long-key default, while `Account` uses `BaseEntity<Guid>`.
- `AccountIdentity` links an application `AccountId` to normalized issuer and subject values. Never identify an authenticated account by email or username.
- Keep this project free from infrastructure implementation details.

### Common
- Shared utilities, constants, exceptions, response/request models and transport helpers.
- HTTP helpers:
  - `Common.Transfer.Http.HttpCall` for JSON/form/string HTTP calls.
  - `Common.Transfer.Http.FileUploader` for multipart uploads.
  - `Common.Models.Transfer.FileUploadItem` is the reusable upload file model.
- Async helper:
  - `Common.System.TaskRunner` runs async operations with bounded concurrency.
  - Register with `services.AddTaskRunner()` and inject `ITaskRunner`.
  - Default concurrency is `TaskRunnerConstants.DefaultMaxConcurrency`.
- Typed memory cache:
  - `Common.Abstractions.ITypedCache<TEntry>` is the reusable cache contract.
  - `Common.System.TypedMemoryCache<TEntry>` stores values in `IMemoryCache` using type-scoped keys.
  - Build project-specific cache adapters over `ITypedCache<TEntry>` when settings or domain-specific methods are needed.
- Keep this project stable because it is widely referenced.

### Cqrs
- Framework-light, AOT-compatible in-process request dispatcher. It owns command, query, handler, dispatcher and pipeline contracts formerly supplied by MediatR and `Common.Cqrs`.
- The core depends only on dependency-injection abstractions and must not reference Application, Common, Domain, validation, persistence, transport or host projects.
- Register handlers and behaviors as closed generic types through `AddRequestHandler<TRequest, TResponse, THandler>()` and `AddPipelineBehavior<TRequest, TResponse, TBehavior>()`. Do not add assembly scanning, `MakeGenericType`, reflection invocation, expression compilation or runtime code generation.
- Each request has exactly one handler. Duplicate handlers fail during service registration and missing handlers fail when dispatched.
- The dispatcher is scoped; handlers, invokers and behaviors are transient and resolve from the current scope. Do not inject the dispatcher into a singleton.
- Behaviors run in registration order around the handler and must preserve the supplied request and cancellation token unless intentionally terminating the pipeline.
- FluentValidation integration remains Application-owned while Application is its only consumer. If multiple consumers need it, prefer a separate optional adapter rather than adding the dependency to the core.
- This dispatcher is not durable messaging. Delivery guarantees, retries, leases, deduplication and dead-lettering belong to outbox or broker infrastructure.
- Keep `Cqrs/README.md` and `Cqrs/FUTURE-FEATURES.md` aligned with public behavior and planned extensions.

### Time
- Framework-light shared UTC and timezone library. It depends only on NodaTime and dependency-injection abstractions, and must not reference feature, host, persistence or transport projects.
- `TimeProvider` remains the clock abstraction. Register it through `services.AddTimeServices()`, which preserves a host or test override registered before it. Do not introduce a parallel `IClock` abstraction.
- `Time.Extensions.TimeProviderExtensions.GetCurrentInstant()` and `GetCurrentUtcDateTime()` are the canonical current-time conversions. Both normalize to the shared persistence precision.
- `UtcTime` owns the canonical UTC timezone and microsecond persistence precision. Its text pattern is fixed to six fractional digits ending in `Z`; use its normalization and parse/format methods at persistence boundaries.
- `UtcDateTime.RestoreFromDatabase()` may relabel provider-returned `Unspecified` values as UTC, but rejects `Local`. Application and API inputs must be explicitly UTC rather than using this database-only restoration path.
- `Time.Extensions.DateTimeUtcExtensions` owns TZDB validation and strict conversions between UTC instants and timezone-local wall-clock values. Ambiguous and skipped local values must continue to be rejected.
- Strict backend JSON UTC handling is registered through `JsonSerializerOptions.AddUtcDateTimeConverters()`.
- Keep framework adapters with their owners: EF Core converters remain in IdentityServer, Dapper/SqlKata handlers remain in persistence projects, and SQL defaults remain in migrations. These adapters should reuse the shared UTC primitives.
- Business timestamps, leases, retry delays, polling intervals and models containing `Instant` or `Duration` remain owned by their feature projects.

### Persistence
- Reusable SqlKata-backed repositories and database command runners. It does not own an application schema or migration lifecycle.
- Persistence-facing abstractions live under `Persistence.Abstractions`, including:
  - `IReadOnlyRepository`
  - `IWritableRepository`
  - `ICommandRunner`
- Supported engines: `MsSql`, `PgSql`, `MySql`, `SqLite`.
- UTC `DateTime` parameters and SQLite materialization use the precision and strict database-boundary helpers from `Time`. `SqliteUtcCompiler` normalizes ordinary SqlKata bindings because Dapper type handlers do not own compiler-generated parameter values. Do not add a second timestamp format or silently convert local values.
- Configuration keys:
  - main connection string name: `Db`
  - selected engine path relative to the supplied section: `ConnectionStrings:Engine`
- Repositories:
  - `IReadOnlyRepository` is intended for query handlers and does not expose write-capable SqlKata APIs.
  - `IWritableRepository` is intended for write-capable consumers and exposes `QueryFactory Db`.
  - `AddRepositories` resolves `IReadOnlyRepository` to a scoped `ReadOnlyRepository` and `IWritableRepository` to a separate scoped `WritableRepository`. Concrete repositories may also be created manually with a connection string and `DatabaseEngine`.
  - `DatabaseSessionValue` is the provider-neutral session-context value. `IDatabaseSessionContextRepository` exposes its infrastructure-only application; handlers must continue to depend on `IReadOnlyRepository` or `IWritableRepository` and must not set session context directly. PostgreSQL and SQL Server apply values natively; MySQL and SQLite are no-ops.
- Command runners:
  - `ICommandRunner` executes raw commands and engine-specific bulk inserts.
  - `AddRepositories(configuration)` and `AddCommandRunners(configuration)` capture an explicitly supplied host or test configuration section; they do not resolve the global root configuration from DI.
  - `AddCommandRunners` registers all concrete runners and resolves the default `ICommandRunner` from the supplied section's `ConnectionStrings:Engine`.
  - Concrete runners can be constructed manually with a connection string when a service needs a separate runtime database target.
- Health checks:
  - `/health/live` is the liveness endpoint.
  - `/health/ready` is the dependency readiness endpoint.
  - `/health/checks` returns the full registered check set.
  - API readiness is host-local and selected by the `ready` tag. Startup diagnostics use the `startup` tag and background publishing uses the `publish` tag.
  - Startup health execution is opt-in through the host-relative `HealthCheck:RunOnStartup` key (`Api:HealthCheck:RunOnStartup` in the API host).
  - Background health publishing is opt-in through the host-relative `HealthCheck:PublishReports` key (`Api:HealthCheck:PublishReports` in the API host).

### Application.Migrations
- Standalone application-schema migration library. It references `Persistence` and `Common`, but not `Application`, `Domain` or `Api`.
- Owns the application SQL scripts, `MigrationRecord` ledger model, embedded script discovery, validation, runner and migration-specific logging.
- Scripts live under `Application.Migrations/Migrations/{Engine}/{Module}` and are embedded in the assembly.
- Add scripts as `migration-X.sql` for every supported application engine.
- `AddApplicationMigrations()` registers migration services; the API composition root calls it and invokes `RunApplicationMigrations()` according to its startup policy.
- Development startup always invokes the migration runner, regardless of `Api:Migrations:RunOnStartup`.
- Non-development startup migration execution is opt-in through `Api:Migrations:RunOnStartup`.
- The runner reads the database migration ledger first and only applies pending migrations.
- Scripts are ordered numerically and migration IDs must be unique per engine, even across module folders.
- Migration scripts are forward-only and should not drop existing tables.
- Migration failure, missing expected scripts, a ledger ahead of the deployed assembly and duplicate or non-contiguous IDs stop startup.
- `Core/migration-1.sql` creates and seeds `MigrationRecord`; later successful migrations are recorded with migration id, module, name, checksum and applied time.
- Until version 1 is finalized, reset changed baseline schemas by manually dropping/recreating the development database.
- The PostgreSQL and SQL Server baselines own account RLS policies for `Account`, its directly account-owned records, `Register`, `RegisterEvent` and `RegisterPlace`. `Event` and `Place` remain catalogs. Every migration that adds, renames or removes an RLS-protected table, or changes its predicate function or ownership column, must update `ApplicationRowLevelSecurityVerifier.ProtectedTables` in the same change. Native-provider integration tests are deferred; keep the planned scenarios in `docs/row-level-security.md` aligned with policy changes.
- There is no runtime migration path setting; embedded resources make source and publish directory layouts irrelevant.
- Detailed ownership and authoring guidance lives in `Application.Migrations/README.md`.

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
- Database-backed module tests read `ConnectionStrings` from the test project's own user-secrets store and require the current application schema. They do not reuse `Api` user-secrets automatically.
- Application composition tests should use the real `ConfigureApplicationServices(configuration)` path.
- Architecture rules live under `Application.Test/Architecture`. Keep these updated when adding modules, changing logging ranges, or adjusting dependency boundaries.
- Uses `Xunit.DependencyInjection`.
- Useful targeted examples:
  - `dotnet test .\Application.Test\Application.Test.csproj --filter FullyQualifiedName~Application.Test.Architecture.ArchitectureRulesTest`
  - `dotnet test .\Application.Test\Application.Test.csproj --filter FullyQualifiedName~Application.Test.Modules.Common.System.TaskRunnerTest`
  - `dotnet test .\Application.Test\Application.Test.csproj --filter FullyQualifiedName~Application.Test.Modules.Common.Transfer.FileUploaderTest`

### Cqrs.Test
- Isolated xUnit tests for request dispatch, command aliases, pipeline ordering, service scopes, cancellation and registration failures.
- Architecture coverage keeps `Cqrs` independent from feature, validation, persistence, transport and host projects.
- Useful targeted command: `dotnet test .\Cqrs.Test\Cqrs.Test.csproj`.

### Api.Test
- xUnit tests for API-layer architecture, controllers and middleware.
- Tests mirror API modules under `Api.Test/Modules`.
- API architecture rules live under `Api.Test/Architecture`.
- Prefer isolated middleware/controller tests unless the goal is specifically to validate full startup/pipeline behavior.
- `ApiFactory` full-host tests use a unique temporary SQLite database and apply the baseline so startup verification remains realistic without local database configuration. Other full-host fixtures may still require explicit database settings.
- Useful targeted examples:
  - `dotnet test .\Api.Test\Api.Test.csproj`
  - `dotnet test .\Api.Test\Api.Test.csproj --filter FullyQualifiedName~Api.Test.Architecture.ApiArchitectureRulesTest`

### Application.Migrations.Test
- Isolated xUnit tests for migration dependency boundaries, logging range ownership, embedded script discovery and runner behavior.
- Uses an in-memory SQLite repository to verify that the baseline runs once and an already-applied migration is skipped on the next invocation.
- Useful targeted command: `dotnet test .\Application.Migrations.Test\Application.Migrations.Test.csproj`.

### Time.Test
- Isolated xUnit tests for UTC JSON serialization, TZDB conversion, persistence formatting, `TimeProvider` conversion and DI override behavior.
- Architecture coverage keeps `Time` independent from all feature, host and persistence projects.
- Useful targeted command: `dotnet test .\Time.Test\Time.Test.csproj`.

### IdentityServer.Test
- xUnit tests for IdentityServer architecture and isolated ASP.NET Identity persistence behavior.
- Uses EF Core InMemory for focused identity-store tests and relational SQLite for migration execution tests. Production persistence uses the provider selected through configuration.
- Useful targeted command: `dotnet test .\IdentityServer.Test\IdentityServer.Test.csproj`.

### External email service
- The sibling `command-emails` repository owns Email.Api, SMTP delivery, outbox processing/persistence, the email worker and email-specific tests.
- This solution retains `Application.Modules.Notifications` and the `Email.Client` protobuf consumer. Do not copy email source projects back into command-q.
- Keep both copies of `email_queue.proto` wire-compatible. command-q uses protobuf; command-emails also exposes JSON REST for interop.
- Outbox migrations, polling, SMTP delivery, retries and dead-letter processing belong to `command-emails`.

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
  - `IdentityServer/Logging/IdentityServerEventIdRanges.cs`
  - `Application.Migrations/Logging/ApplicationMigrationsEventIdRanges.cs`
- Add cross-cutting IDs to root `*EventIds.cs` files. Add module-owned IDs to module `Logging/*EventIds.cs` files.
- `Logging` and Serilog level/threshold settings use the host configuration reload pipeline. Do not log configuration secrets while observing reloads.

## Coding Conventions
- Use CRLF line endings for repository text files, following `.editorconfig`; do not introduce LF-only or mixed-line-ending files.
- Prefer existing project patterns over new abstractions.
- Follow `.editorconfig` for formatting, naming and analyzer preferences.
- Keep method, constructor and primary-constructor declarations on one line when the complete declaration is at most 140 characters. Wrap parameters only when the declaration exceeds that limit or readability genuinely requires it.
- Use `rg`/`rg --files` for searches.
- Use `apply_patch` for manual edits.
- Avoid unrelated refactors and metadata churn.
- Avoid hardcoded magic strings and numbers when a value is reused, user-facing, externally observable, or likely to change.
- Keep constants near the owner of the behavior: API-wide constants under `Api/Constants`, module routes under `Api/Modules/{Module}`, module behavior limits under `Application/Modules/{Module}`, and persistence settings under `Persistence/Constants`.
- Keep configuration boundaries consistent: configuration path constants belong in `Constants/ConfigKeys.cs`, bindable configuration models belong in a `Configuration` folder and use the `*Options` suffix, and `Models` should not contain configuration options.
- Prefer configuration over constants for environment-specific or runtime-tunable values.
- Do not over-abstract one-off SQL schema definitions, source-generated log message templates, or local test data unless reuse or consistency requires it.
- Add focused tests when changing shared helpers, DI, repositories, command runners or user-facing behavior.
- Do not expose write-capable persistence APIs to query handlers.
- Keep command handlers using writable dependencies and query handlers using read-only dependencies.
- Use resource providers for user-facing messages and validation text instead of hardcoded strings.

## Verification
- Run `dotnet build .\command-q.sln` after code changes.
- Run targeted tests for touched areas.
- Full `dotnet test` can include integration-style tests depending on local database configuration, so prefer targeted tests when validating isolated helpers.
