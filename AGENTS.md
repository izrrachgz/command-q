# command-q - AGENTS

This file captures practical repository knowledge for coding agents working in this solution.

## Solution

- Framework: .NET 10.
- Solution file: `command-q.sln`.
- Restore with `dotnet restore .\command-q.sln`.
- Build and pack with `.\scripts\build.ps1`; use `-Configuration Release` for release artifacts.
- Test with `.\scripts\test.ps1`; target one project with `-Project` and one group with `-Filter`.
- NuGet dependency versions are centrally managed in `Directory.Packages.props`; do not add inline `Version` attributes to `PackageReference` items.
- Package metadata is defined in `Directory.Build.props`. Packages are written to the sibling `..\command-q-build-output` local feed.
- The workspace may contain unrelated user changes. Do not revert files unless explicitly asked.

## Repository Boundary

- This repository owns reusable, product-neutral libraries: `Common`, `Cqrs`, `Localization`, `Persistence` and `Time`.
- Deployable hosts, application use cases, domain models, database schemas and migrations live in consuming repositories such as `command-q-back` and `command-emails`.
- Consumers reference versioned `CommandQ.*` NuGet packages. Never add project references from this solution to an application repository or load locally published DLLs directly.
- Libraries must not reference application, domain or host projects. Keep dependencies flowing from consumers into these packages.
- Preserve public API compatibility deliberately. Treat removals, renames and semantic changes as breaking changes and update package versions accordingly.
- Each library owns a `README.md`, which is embedded as that NuGet package's readme. Keep it aligned with public behavior when changing a package.
- Keep tests in the matching `*.Test` project and keep all ten projects in the solution's `Libraries` and `Tests` folders.

## Projects

### Common

- General utilities, abstractions, exceptions, shared models and transport helpers that are useful across unrelated applications.
- `TaskRunner` provides bounded-concurrency asynchronous execution; register it with `AddTaskRunner()` and inject `ITaskRunner`.
- `ITypedCache<TEntry>` and `TypedMemoryCache<TEntry>` provide type-scoped in-memory caching. Put domain-specific cache adapters in consuming applications.
- `HttpCall` and `FileUploader` provide lightweight HTTP and multipart helpers.
- Do not move CQRS, localization, persistence, time, authentication or business concepts into `Common` merely to share them.

### Cqrs

- Framework-light, AOT-compatible in-process request dispatching for commands, queries, handlers and pipeline behaviors.
- Keep the core dependent only on dependency-injection abstractions. Validation, logging policies and feature behavior belong to consumers unless generally reusable.
- Register requests explicitly through closed generic methods. Do not introduce assembly scanning, reflection-based dispatch, runtime generic construction or code generation.
- Preserve one handler per request. Pipeline behaviors execute in registration order around the handler and must propagate cancellation.
- This is not durable messaging. Broker delivery, retries, leases, deduplication and outboxes belong to application infrastructure.

### Localization

- Owns localization composition mechanisms only. It must not embed application messages, field names, validation catalogs or a product-specific language list.
- Consuming applications register their own supported/default/fallback cultures, RESX resource managers and JSON locale directories through `AddCommandQLocalization`, `AddResx` and `AddJsonFiles`.
- `ITextProvider` resolves registered providers in configured order and returns the key when no source contains a value.
- Providers are scoped so resolution follows the active request's UI culture. Applications remain responsible for request-culture middleware.
- When adding a provider, preserve deterministic provider ordering, fallback behavior and application ownership of content.

### Persistence

- Provider-neutral SqlKata repositories, database command runners and session-context primitives for SQL Server, PostgreSQL, MySQL and SQLite.
- Query flows depend on `IReadOnlyRepository`; write flows depend on `IWritableRepository` or `ICommandRunner`. Do not expose write-capable persistence to read-only consumers.
- Keep SQL parameterized and propagate cancellation tokens through database operations.
- `IDatabaseSessionContextRepository` is an infrastructure capability. Applications define the keys, policies and authorization semantics applied through it.
- Persistence does not own application schemas, migration scripts, migration ledgers, startup migration policy or row-level-security policy names.
- Keep provider behavior aligned where semantics are shared, while retaining explicit provider implementations where SQL or bulk operations differ.

### Time

- UTC validation, precision, serialization and time-zone conversion primitives built on `TimeProvider` and NodaTime.
- Inject `TimeProvider` through `AddTimeServices()` for current-time behavior; do not add direct `DateTime.Now` or `DateTime.UtcNow` calls to reusable code.
- Reject ambiguous or non-UTC inputs at boundaries where UTC is required. Restore UTC kind explicitly after database reads.
- Keep JSON and persistence representations stable and covered across `DateTime`, `DateTimeOffset` and NodaTime `Instant` behavior.

## Testing

- `Common.Test` covers shared helper, extension, caching and task-execution behavior.
- `Cqrs.Test` covers registration, dispatch, pipelines, cancellation and AOT/dependency architecture rules.
- `Localization.Test` owns representative RESX and JSON fixtures and verifies culture, provider order and fallback behavior.
- `Persistence.Test` covers provider-neutral query construction, sorting/searching and UTC filter normalization without requiring an external database.
- `Time.Test` covers UTC contracts, precision, time-zone conversion, JSON converters, dependency injection and architecture rules.
- Add focused tests for changes to public contracts, DI registration, fallback/ordering rules, database provider behavior and serialization formats.
- Tests should remain deterministic and must not require network access, application repositories or developer-specific services.

## Logging

- Use source-generated logging with `LoggerMessage` for reusable library logging.
- General event ID ranges live in `Common/Logging/CommonEventIdRanges.cs` and `Persistence/Logging/PersistenceEventIdRanges.cs`.
- Global repository reservations are documented in `docs/logging-event-ids.md`; update the registry when a solution allocates a new range.
- Logging tests enforce package range membership and uniqueness. Keep them aligned with every new event or logging package.
- Keep IDs stable after release and do not log connection strings, credentials, tokens, localized sensitive values or command parameters that may contain secrets.

## Coding Conventions

- Use CRLF line endings for repository text files, following `.editorconfig`; do not introduce LF-only or mixed-line-ending files.
- Follow `.editorconfig` for formatting, naming, nullable annotations and analyzer preferences.
- Prefer existing project patterns over new abstractions and avoid unrelated refactors or metadata churn.
- Keep declarations on one line when the complete declaration is at most 140 characters; wrap only when needed for readability.
- Use `rg` or `rg --files` for searches and `apply_patch` for manual edits.
- Prefer explicit dependency injection and small public surfaces. Default implementation types to internal when consumers only need an interface or extension method.
- Avoid hardcoded magic strings and numbers when a value is reused, externally observable or likely to change; keep constants with the project that owns the behavior.
- Never commit tokens, credentials, local configuration secrets, `bin`, `obj`, test results, IDE state or generated package artifacts.

## Verification

- Run `.\scripts\build.ps1 -Configuration Release` after library or packaging changes.
- Run `.\scripts\test.ps1 -Configuration Release -NoBuild` after a successful release build.
- For documentation-only changes, still verify the solution builds and the generated packages contain the correct project-specific `README.md`.
