Project: Common
Purpose: Shared utilities, constants, common exceptions, helper types and reusable transport models used across the solution.
Framework: .NET 10 (class library).

Main areas:
- Common.Cqrs: MediatR command/query marker interfaces, handlers and validation pipeline behavior.
- Common.Transfer.Http: Lightweight HTTP helpers for JSON/form/string requests and multipart file uploads.
- Common.System: TaskRunner for bounded-concurrency asynchronous work and TypedMemoryCache for reusable typed memory caching.
- Common.Abstractions: Reusable contracts such as ITaskRunner and ITypedCache.
- Common.Models: Shared response, request, command, system and transfer models.
- Common.Constants: Stable shared constants for logging, process messages, database messages, languages and helper defaults.
- Common.Extensions: Small extension methods used by other projects.

HTTP helpers:
- HttpCall supports reusable request headers, content headers, JSON serialization settings, query-string building and common GET/POST helpers.
- FileUploader supports multipart uploads from files, streams, byte arrays and FileUploadItem models.
- FileUploadItem lives under Common.Models.Transfer because it is a reusable transfer model, not an HTTP implementation detail.

Task runner:
- Register with services.AddTaskRunner().
- Resolve ITaskRunner when a service needs bounded-concurrency async execution.
- The default max concurrency is TaskRunnerConstants.DefaultMaxConcurrency.
- TaskRunner logs individual operation failures and continues waiting for the scheduled batch to complete.

Typed memory cache:
- `ITypedCache<TEntry>` is the reusable cache contract.
- `TypedMemoryCache<TEntry>` stores values in `IMemoryCache` using type-scoped keys.
- `Clear()` removes only keys tracked for that cached value type.
- Projects can build domain-specific cache adapters over `ITypedCache<TEntry>` when they need expressive methods or settings.

Notes:
- Keep this project stable and minimize breaking changes because it is referenced across application layers.
- Prefer adding small reusable helpers here only when they are independent from infrastructure and business-specific rules.
