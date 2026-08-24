# CommandQ - AGENTS

This file captures practical repository knowledge for coding agents working in this solution.

## Solution
- Framework: .NET 10.
- Solution file: `CommandQ.sln`.
- Restore: `dotnet restore`.
- Build: `dotnet build .\CommandQ.sln`.
- Tests: `dotnet test` or targeted tests with `--filter FullyQualifiedName~...`.
- Helper scripts live under `scripts/`:
  - `.\scripts\build.ps1`
  - `.\scripts\test.ps1`
  - `.\scripts\run-api.ps1`
  - `.\scripts\run-identity-server.ps1`
  - `.\scripts\run-email-worker.ps1`
  - `.\scripts\add-identity-migration.ps1 -Name MigrationName`
  - `.\scripts\test-email-integration.ps1`
  - `.\scripts\run-front.ps1` is optional and only delegates to `FrontApplication` if present.
- `CommandQ.slnLaunch` provides the `Backend + Email Worker` profile, which starts IdentityServer, API and the standalone email queue worker.
- The backend run scripts bypass launch profiles so `-Environment` and `-Urls` are authoritative. Defaults are `https://localhost:7125` for IdentityServer and `https://localhost:7025;http://localhost:7000` for API. Use the exact `Qa` environment name for `appsettings.Qa.json` on case-sensitive hosts.
- Custom executable settings are namespaced by host: `Api`, `IdentityServer` and `EmailWorker`. Framework-owned `Logging`, `Serilog` and `AllowedHosts` sections remain top-level. Hosts pass explicit subsections to reusable libraries; do not make a library depend on a host prefix.
- Backend Postman artifacts live under `postman/`. Keep committed environments free from passwords, API-client keys, client secrets and issued tokens; local environment exports should use the ignored `*.local.postman_environment.json` suffix.
- NuGet package versions are centrally managed in `Directory.Packages.props`; do not add inline `Version` attributes to `PackageReference` items.
- The workspace may contain unrelated user changes. Do not revert files unless explicitly asked.

## Architecture
- The backend follows a modular-monolith shape. Keep one deployable API, but group feature-owned code under module folders.
- Module roots:
  - `Api/Modules/{Module}`
  - `Application/Modules/{Module}`
  - `Domain/Modules/{Module}`
- Module namespaces should match module folders, for example `Application.Modules.Identity.Accounts.Commands` and `Domain.Modules.Identity`.
- Current modules include `Identity`, `Contacts`, `Addresses`, `Files`, `Health` and `Notifications`; domain also has `Events` and `Registers`.
- The application Identity module owns profiles, external identity mapping and queryable business account activity. Record business actions through `IAccountActivityRecorder` or `RecordAccountActivityCommand`; do not create per-account application log files as the source of truth.
- `IdentityServer` owns authentication users, password hashes, lockout, external logins, OAuth clients, scopes, grants and token issuance. Do not move these responsibilities back into `Api` or `Application`.
- Existing module status and removal steps are documented in `docs/modules.md`.
- Keep cross-cutting infrastructure at project root, such as `Api/Middleware`, `Api/Extensions`, `Common/System`, `Persistence/Repository` and general project logging.
- Prefer explicit module registration over convention scanning. Application composition lives in `Application/Modules/ApplicationModuleExtensions.cs`, with each module owning its `*ModuleExtensions.cs`; those extensions explicitly register MediatR handlers, validators, mappers and module services. Do not use application-wide exported-type scanning for these services. Keep commands, queries and contracts public for API/test boundaries, while module implementation classes remain internal to the Application assembly. API endpoint composition lives in `Api/Modules/ApiModuleExtensions.cs`, with each module owning its `*ApiModuleExtensions.cs`.
- Host implementation types should be internal by default. Keep API and IdentityServer controllers public for framework discovery, but seal concrete controllers. Keep host configuration models, middleware, persistence helpers and concrete services internal unless another project consumes them through an intentional contract.
- The frontend is intentionally separate/optional right now. Do not change `FrontApplication` unless the user explicitly asks for frontend work.

## Projects

### Api
- ASP.NET Core HTTP API with module controllers, middleware, resource-server authentication and app configuration.
- Run locally from `Api` with `dotnet run` or Visual Studio launch profiles.
- Runtime application database selection lives under `Api:ConnectionStrings`; the engine key is `Api:ConnectionStrings:Engine`.
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
- Operational API settings use `IOptionsMonitor` where runtime reload is safe: response cache headers, rate-limit policies, feature flags, localization and health publishing. `Api:Localization:DefaultCulture` is the request default; `Api:Localization:SupportedCultures` controls accepted cultures. External clients should be created through `IExternalApiClientFactory` so endpoint and timeout changes apply to new clients.
- Local secrets should use .NET user-secrets or environment variables. Keep real secrets out of `appsettings*.json`, `.env`, scripts and docs. See `docs/local-secrets.md`.
- `SecurityHeadersMiddleware` adds browser security headers. Keep security middleware in backend API infrastructure.
- Unexpected exception details are sanitized outside Development in `ExceptionHandlingMiddleware`.
- The API is an email outbox producer: it registers `AddEmailOutboxProducer()` and `Email.Outbox.Persistence`, but does not register `IEmailOutboxProcessor`, run the email worker or own SMTP delivery.
- Development outbox persistence targets the same local MsSql `command_q` database as the application. Base and non-development templates leave the connection empty so deployment configuration is required. `Email.Worker` is the designated outbox migrator; API persistence options contain no migration lifecycle flag.

### IdentityServer
- Standalone ASP.NET Core Identity and OpenIddict authorization-server host. It is independently deployable and connected to `Api` through standard OpenID Connect discovery, not a project reference.
- Run locally with `.\scripts\run-identity-server.ps1`; the Development issuer is `https://localhost:7125/`.
- OpenIddict uses an isolated database named `command_q_identity` through EF Core stores.
- IdentityServer supports `PgSql`, `MsSql`, `MySql` and `SqLite` through `IdentityServer:ConnectionStrings:Engine`.
- Keep IdentityServer on EF Core. Do not introduce SqlKata into this host or share the application's persistence registration.
- Every EF `DateTime` property uses the IdentityServer UTC converter, including nullable OpenIddict timestamps. It rejects non-UTC writes and restores `DateTimeKind.Utc` after provider reads; keep this convention active for all four provider models.
- The shared Identity/OpenIddict model lives in `IdentityServerDbContext`; provider-specific contexts under `IdentityServer/Persistence/Providers` select the EF provider and migration set.
- ASP.NET Core Identity owns local users, password hashing, lockout, roles, claims and external login links.
- OpenIddict owns clients, scopes, authorizations, access/refresh tokens and revocation. Access tokens are signed `at+jwt` tokens; authorization and refresh tokens remain encrypted.
- Account endpoints live under `IdentityServer/Modules/Accounts`; authorization/token handlers and client/scope seeding live under `IdentityServer/Modules/OpenIddict`.
- Google and Microsoft providers are registered only when complete `ExternalProviders` credentials are configured.
- Authentication security events are stored in `IdentitySecurityEvent`; application business activity remains in `AccountActivity`.
- EF migrations live under `IdentityServer/Persistence/Migrations/{Engine}`. Use `.\scripts\add-identity-migration.ps1 -Name MigrationName` so every model change is generated for all four providers.
- Do not combine provider migrations into one snapshot. EF migration sets must remain aligned across `PgSql`, `MsSql`, `MySql` and `SqLite`.
- Provider design-time factories live as separate files under `IdentityServer/Persistence/DesignTime` and use non-routable placeholder connections for scaffolding only. Apply database migrations through configured IdentityServer startup, not `dotnet ef database update`.
- Development applies pending EF migrations at startup. Non-development migration execution is opt-in through `IdentityServer:Migrations:RunOnStartup`.
- After migration handling, every IdentityServer startup reconciles the default role, API scope and configured OpenIddict clients. A current schema is therefore required even when startup migrations are disabled.
- Development certificates are allowed only in Development. QA/Production require signing, encryption and Data Protection certificates plus shared persisted Data Protection keys.

### Application
- CQRS use cases, validators, handlers, mappers, services and health checks, grouped by module under `Application/Modules`.
- Depends on `Domain`, `Common`, `Localization`, `Persistence` and the reusable `Email`/`Email.Outbox` contracts.
- Keep application logic decoupled from direct I/O. Use injected interfaces such as repositories, text providers and services.
- Validators use localized resources through `ValidationTextProvider`.
- Shared validator limits live in `Application/Constants/ValidationLimits.cs`.
- Register module-owned services explicitly through `AddApplicationModules(configuration)` and module-specific methods such as `AddIdentityModule()`. Module extensions own the registrations for their handlers, validators and mappers; do not move those registrations back into a project-wide scanner.
- Application accounts are business profiles. Keep passwords, auth lockout and protocol sessions out of Application.
- `Application.Modules.Notifications` owns `QueueEmailCommand` and `IApplicationEmailService`. Queueing is the default; do not add an immediate-send switch to the command.
- Notification overloads converge on `ApplicationEmailRequest`, which carries a prepared `EmailMessage` plus optional idempotency, account and correlation metadata.
- The Notifications module must not reference `Email.Outbox.Persistence` or `Email.Worker`. Producer hosts select the outbox store; the worker owns SMTP delivery and retries.
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
  - `Common.Abstractions.ITypedCache<TEntry>` is the reusable cache contract.
  - `Common.System.TypedMemoryCache<TEntry>` stores values in `IMemoryCache` using type-scoped keys.
  - Build project-specific cache adapters over `ITypedCache<TEntry>` when settings or domain-specific methods are needed.
- Keep this project stable because it is widely referenced.

### Persistence
- Reusable SqlKata-backed repositories and database command runners. It does not own an application schema or migration lifecycle.
- Persistence-facing abstractions live under `Persistence.Abstractions`, including:
  - `IReadOnlyRepository`
  - `IWritableRepository`
  - `ICommandRunner`
- Supported engines: `MsSql`, `PgSql`, `MySql`, `SqLite`.
- Configuration keys:
  - main connection string name: `Db`
  - selected engine path relative to the supplied section: `ConnectionStrings:Engine`
- Repositories:
  - `IReadOnlyRepository` is intended for query handlers and does not expose write-capable SqlKata APIs.
  - `IWritableRepository` is intended for write-capable consumers and exposes `QueryFactory Db`.
  - `ReadOnlyRepository` and `WritableRepository` can be resolved from DI using configured settings or manually created with a connection string and `DatabaseEngine`.
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

### Api.Test
- xUnit tests for API-layer architecture, controllers and middleware.
- Tests mirror API modules under `Api.Test/Modules`.
- API architecture rules live under `Api.Test/Architecture`.
- Prefer isolated middleware/controller tests unless the goal is specifically to validate full startup/pipeline behavior.
- Full API host tests may run migrations and require local database configuration.
- Useful targeted examples:
  - `dotnet test .\Api.Test\Api.Test.csproj`
  - `dotnet test .\Api.Test\Api.Test.csproj --filter FullyQualifiedName~Api.Test.Architecture.ApiArchitectureRulesTest`

### Application.Migrations.Test
- Isolated xUnit tests for migration dependency boundaries, logging range ownership, embedded script discovery and runner behavior.
- Uses an in-memory SQLite repository to verify that the baseline runs once and an already-applied migration is skipped on the next invocation.
- Useful targeted command: `dotnet test .\Application.Migrations.Test\Application.Migrations.Test.csproj`.

### IdentityServer.Test
- xUnit tests for IdentityServer architecture and isolated ASP.NET Identity persistence behavior.
- Uses EF Core InMemory for focused identity-store tests and relational SQLite for migration execution tests. Production persistence uses the provider selected through configuration.
- Useful targeted command: `dotnet test .\IdentityServer.Test\IdentityServer.Test.csproj`.

### Email
- Standalone .NET 10 SMTP delivery library backed by MailKit and MimeKit. It has no dependency on API, Application or IdentityServer.
- Public abstractions and message models live under `Email/Abstractions` and `Email/Models`; MailKit transport details stay internal under `Email/Services`.
- `Email/appsettings.json` is copied to `Email/appsettings.json` in the output as library defaults. `services.AddEmailServices(configuration)` layers consuming-host configuration over that file and validates the merged `Email` section on startup.
- Email delivery is disabled by default. Disabled configuration may remain incomplete; when `Email:Enabled` is `true`, SMTP, sender, authentication and delivery-policy settings are validated.
- Supported SMTP authentication types are `Anonymous`, `Login`, `Plain` and `OAuth`. Only the selected authentication section is consumed. OAuth expects an already-issued access token and does not acquire, refresh or persist tokens.
- Authenticated modes require `StartTls` or `SslOnConnect`; do not weaken this validation or bypass certificate validation.
- `IEmailSender` returns `EmailSendResult`/`EmailBatchResult` with message IDs and SMTP responses. `EmailDeliveryException` exposes failure classification and messages accepted before a partial batch failure.
- Email timestamps use NodaTime `Instant`, current-time reads use the DI-provided `TimeProvider`, and the canonical email timezone is UTC. Register test or host providers before email services so `TryAddSingleton(TimeProvider.System)` preserves the override.
- `IEmailSender` monitors `EmailOptions`; SMTP endpoint, sender, authentication, delivery limits and supplied OAuth tokens apply to the next batch. Each batch captures one valid options snapshot and creates its own transport. Reload logs contain safe metadata only.
- `IEmailSender.SendAsync(IEnumerable<EmailMessage>)` validates the full batch and configured delivery limits, opens one connection, authenticates once, sends sequentially, disconnects once and disposes the SMTP client.
- File, byte and copied-stream attachments are supported. HTML messages may include `EmailInlineResource` items referenced through `cid:` URLs. Caller-owned streams are copied and are never retained or disposed by the library.
- HTML template overloads read already-rendered files verbatim. Rendering, substitutions and localization belong to the caller.
- Do not add automatic SMTP retries to the sender. `Email.Outbox` and `Email.Worker` own durable retries, deduplication and dead-letter handling because a failed SMTP batch can be partially delivered.
- Keep SMTP credentials in the consuming host's user-secrets or deployment secret store. Do not add them to the Email project or committed settings.
- The Email project is consumed by `Email.Worker` but remains independent of API and IdentityServer. Add other host references only for concrete immediate-delivery workflows.
- Detailed configuration, API examples and batch semantics are documented in `Email/README.md`.

### Email.Outbox
- Transport-independent durable queue contracts and delivery workflow. References `Email`, but not Persistence, API, Application or IdentityServer.
- `IEmailOutbox` snapshots fully prepared messages and returns an enqueue result with the durable record ID and duplicate status.
- Idempotency keys are optional but strongly recommended for business-triggered messages.
- File attachments and inline resources are copied into the payload at enqueue time; queued delivery does not depend on temporary files.
- `IEmailOutboxProcessor` sends a claimed batch through one `IEmailSender` batch call, handles partial acceptance, schedules exponential retries and dead-letters permanent or exhausted failures.
- Delivery is at least once. A crash between SMTP acceptance and the database update can produce a duplicate; message workflows must tolerate this.
- Complete bodies and attachments are stored in the outbox database. Treat that database according to the sensitivity of its content.
- Outbox timestamps and scheduling boundaries use NodaTime `Instant`; lease and retry intervals use `Duration`. Do not reintroduce `DateTime.UtcNow`, local timestamps or offset-dependent contracts.
- Configuration uses the `EmailOutbox` section. See `Email.Outbox/README.md`.
- Application workflows normally enqueue through the Notifications module. A producer host must register both the outbox core and an `IEmailOutboxStore` implementation before executing those commands.
- Producer-only hosts use `AddEmailOutboxProducer(configuration)`. Consumer hosts use `AddEmailOutbox(configuration)` or explicitly add `AddEmailOutboxProcessor()` and must also register `IEmailSender`.

### Email.Outbox.Persistence
- SqlKata implementation of `IEmailOutboxStore` using the reusable `Persistence` project through an adapter boundary.
- Owns its schema, embedded provider scripts, `EmailOutboxMigrationRecord` ledger and migration runner. It does not use `Application.Migrations` or the application database ledger.
- Version 1 keeps one consolidated `migration-1.sql` baseline per provider. Fold schema changes into that baseline and recreate development outbox databases until version 1 is finalized.
- Persistence maps NodaTime instants to native UTC provider values; SQLite uses the shared seven-digit UTC text pattern ending in `Z`.
- Supported engines are `SqLite`, `MsSql`, `PgSql` and `MySql`; the worker's Development settings target local MsSql `command_q`.
- Reusable persistence options contain only `Engine` and `ConnectionString`. The CommandQ worker supplies them from `EmailWorker:EmailOutbox:Persistence`.
- SQLite relative paths resolve against the consuming host content root, and the database directory is created when needed.
- Queue claims use conditional updates, unique lock IDs and expiring leases so multiple workers can share one store.
- Configure the lease longer than the maximum expected SMTP batch duration.
- In multi-instance deployments, apply migrations through one deployment step or one designated instance.
- See `Email.Outbox.Persistence/README.md`.

### Email.Worker
- Standalone .NET 10 worker host composing Email transport, outbox workflow and outbox persistence.
- Development uses smtp4dev at `localhost:25`, enables polling and uses the independently migrated outbox schema in local MsSql `command_q`.
- Base, QA and Production settings disable delivery and polling and leave the outbox connection empty. QA/Production also disable startup migrations until explicitly enabled.
- Worker settings live under `EmailWorker:Worker`: `Enabled`, `BatchSize`, `PollIntervalSeconds`, `LeaseSeconds` and `ErrorDelaySeconds`. Startup migration policy lives separately under `EmailWorker:Migrations:RunOnStartup`.
- Worker settings are monitored at runtime. Changes apply to new polling cycles; disabling pauses the worker and re-enabling resumes it. Active batches are not reconfigured mid-flight.
- The worker shares the registered `TimeProvider` with Email, Email.Outbox and Email.Outbox.Persistence; all current values are converted to NodaTime instants immediately.
- Run independently with `.\scripts\run-email-worker.ps1`. The shared Visual Studio backend launch profile also starts it alongside API and IdentityServer.
- The `Email.Worker` Visual Studio launch profile and run script set `DOTNET_ENVIRONMENT=Development`, allowing the generic host to load the worker project's user-secrets. Do not load user-secrets explicitly outside Development.
- OAuth token acquisition and refresh remain external to the worker.
- See `Email.Worker/README.md`.

### Email.Test
- Isolated xUnit tests for dependency boundaries, logging ranges, options and policy validation, MIME construction, structured failures and SMTP connection lifecycle.
- Uses an in-memory fake transport and never contacts a real SMTP endpoint.
- Useful targeted command: `dotnet test .\Email.Test\Email.Test.csproj`.

### Email.IntegrationTest
- Opt-in xUnit tests against a developer-owned smtp4dev instance. The committed setting disables the test so normal solution and CI runs do not require an SMTP service.
- Local defaults are SMTP `localhost:25` and HTTP API `http://localhost:5000`; override them through the `Smtp4Dev` configuration section or environment variables.
- The test verifies SMTP acceptance, plain and HTML bodies, an attachment and a CID resource through smtp4dev's API, then deletes the test message.
- Useful targeted command: `.\scripts\test-email-integration.ps1`.

### Email.Outbox.Test
- Isolated xUnit coverage for dependency boundaries, event ID ranges, message snapshots, batch workflow and dedicated SQLite persistence.
- The SQLite test applies migrations, verifies idempotent enqueueing, exclusive leases, sent completion and a second no-op migration pass.
- Useful targeted command: `dotnet test .\Email.Outbox.Test\Email.Outbox.Test.csproj`.

### FrontApplication
- Optional Angular frontend project. It is not referenced by `CommandQ.sln` and is not part of the current backend modularization work. Check its own README and ask/confirm before changing frontend code unless the user explicitly requests it.

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
  - `Email/Logging/EmailEventIdRanges.cs`
  - `Application.Migrations/Logging/ApplicationMigrationsEventIdRanges.cs`
  - `Email.Outbox/Logging/EmailOutboxEventIdRanges.cs`
  - `Email.Outbox.Persistence/Logging/EmailOutboxPersistenceEventIdRanges.cs`
  - `Email.Worker/Logging/EmailWorkerEventIdRanges.cs`
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
- Run `dotnet build .\CommandQ.sln` after code changes.
- Run targeted tests for touched areas.
- Full `dotnet test` can include integration-style tests depending on local database configuration, so prefer targeted tests when validating isolated helpers.
