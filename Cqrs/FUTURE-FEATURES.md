# Possible Future Features

The current dispatcher intentionally covers only the request/response behavior
CommandQ uses today. The following features are candidates, not commitments.
They should be added only when a concrete use case justifies their API and
maintenance cost, and every addition should preserve trimming and AOT safety.

| Candidate | What it would serve | Guidance and tradeoffs |
| --- | --- | --- |
| Observability behavior | Create consistent traces, metrics and request-duration measurements around every dispatch. | Prefer an optional OpenTelemetry integration project so the core does not acquire telemetry dependencies. Never record request bodies or sensitive values by default. |
| Timeout or deadline behavior | Bound in-process work and distinguish an operation deadline from caller cancellation. | Cancellation in .NET is cooperative; this can signal a timeout but cannot safely terminate code that ignores its token. A host or feature should choose the deadline. |
| Authorization behavior | Apply shared policy checks before selected handlers execute. | Authorization data and policy decisions remain feature-owned. Avoid hiding endpoint-level authorization or making domain code depend on HTTP concepts. |
| Transaction behavior | Wrap a command handler and its database work in one atomic transaction. | Implement as a persistence-specific adapter because transactions and unit-of-work semantics vary by provider. Do not apply transactions to queries or external side effects automatically. |
| In-process notifications | Publish one event to zero or more handlers for decoupled work inside one process. | Define ordering, failure and cancellation semantics before adding it. This would not be durable and must not replace an outbox or broker for work that must survive a crash. |
| Streaming requests | Return `IAsyncEnumerable<T>` for large or progressive result sets without buffering the entire response. | Requires separate streaming handler and behavior contracts with carefully defined cancellation and disposal semantics. Add only when a real streaming endpoint needs it. |
| Source-generated registration | Generate closed handler and pipeline registrations at compile time when the number of requests becomes large. | Explicit registrations are simpler today. A generator could reduce repetitive composition code while retaining AOT safety, but adds analyzer packaging, diagnostics and build complexity. |
| Registration manifest validation | Report all missing, duplicate or inconsistent request registrations during startup instead of finding missing handlers on first dispatch. | Duplicate handlers already fail during registration. Full missing-handler validation needs an explicit request manifest or generated catalog because safe AOT code should not discover requests by assembly scanning. |
| Resilience behavior | Apply retry or circuit-breaking policies to narrowly selected idempotent in-process requests. | Never retry every command. Retrying state changes can duplicate effects, and durable external work belongs in an outbox or queue. Prefer feature-specific policies and explicit idempotency. |
| FluentValidation adapter | Reuse standardized validation pipeline registration across multiple applications. | Create a separate `Cqrs.FluentValidation` project only if more than one consumer needs it. Application owns the current adapter, keeping the core dependency-free. |
| Performance benchmark project | Track dispatch overhead, allocations, pipeline scaling and changes between .NET versions. | Add BenchmarkDotNet outside the core and establish realistic baselines before optimizing. Functional tests remain the correctness gate. |
| Diagnostic registration report | Expose a startup-safe list of registered request, response and handler types for support tooling. | Keep it metadata-only and avoid service construction. Decide whether exposing internal type names is acceptable before surfacing it through operational endpoints. |

## Features That Should Stay Elsewhere

Some capabilities may look like dispatcher features but have stronger owners:

- Durable delivery, retries, leasing, dead-lettering and deduplication belong to
  outbox or messaging infrastructure.
- Database repositories and transaction implementations belong to Persistence.
- Feature validation rules, authorization rules and business logging belong to
  Application modules.
- HTTP status mapping and `ProblemDetails` belong to Api.
- Timeouts for SMTP, HTTP or database clients belong to those client adapters,
  even if a higher-level request deadline also exists.

Keeping those boundaries prevents the dispatcher from becoming a service
locator or a second application framework.
