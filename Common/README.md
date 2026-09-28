# CommandQ.Common

`CommandQ.Common` contains small, product-neutral utilities, contracts, exceptions and transfer models shared by CommandQ applications. It targets .NET 10 and intentionally excludes business, CQRS, localization, persistence and time concerns owned by other packages.

## Install

```powershell
dotnet add package CommandQ.Common
```

Package versions are produced by the repository build and published to the configured CommandQ package feed.

## Main areas

- `Common.Abstractions`: reusable contracts such as `ITaskRunner` and `ITypedCache<TEntry>`.
- `Common.System`: bounded-concurrency task execution and typed in-memory caching.
- `Common.Transfer.Http`: lightweight JSON, form, string and multipart HTTP helpers.
- `Common.Models`: shared request, response, command, system and file-transfer shapes.
- `Common.Extensions`: focused string, enum and enumerable helpers.
- `Common.Constants` and `Common.Enumerations`: stable cross-application values.
- `Common.Exceptions`: HTTP-oriented exceptions for integrations that need them.

## Task execution

Register the runner once in the composition root:

```csharp
services.AddTaskRunner();
```

Inject `ITaskRunner` to execute asynchronous operations with bounded concurrency. The default limit is `TaskRunnerConstants.DefaultMaxConcurrency`. Individual operation failures are logged while the runner continues waiting for the scheduled batch to finish.

## Typed memory cache

`ITypedCache<TEntry>` scopes cache keys by entry type. `TypedMemoryCache<TEntry>.Clear()` removes only keys tracked for that type. Applications can add expressive, domain-specific cache adapters over the generic contract without moving domain concepts into this package.

## HTTP helpers

`HttpCall` supports reusable request/content headers, JSON serialization settings, query strings and common HTTP operations. `FileUploader` builds multipart uploads from files, streams, byte arrays and `FileUploadItem` values.

## Ownership rules

`DateRangeCriteria` represents a reusable inclusive UTC range. `SearchCriteria`
inherits that range for the primary creation-date filter while adding paging,
sorting and free-text search fields. Consumers that filter a second timestamp
should expose a separately named `DateRangeCriteria` instead of duplicating date
properties or creating another `SearchCriteria`.

- Add a helper only when it is useful across unrelated applications and independent of business rules.
- Keep CQRS contracts in `CommandQ.Cqrs`, localization in `CommandQ.Localization`, database access in `CommandQ.Persistence` and UTC behavior in `CommandQ.Time`.
- Keep the public surface stable because this package is referenced broadly.
- Build application-specific adapters in the consuming application.

## Tests

Focused unit tests live in [`Common.Test`](../Common.Test/README.md).
