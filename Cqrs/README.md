# Cqrs

`Cqrs` is CommandQ's small, framework-light in-process request dispatcher. It
owns the command, query, handler and pipeline contracts that were previously
provided through MediatR and `Common.Cqrs`.

The project exists separately from `Common` so its public surface and future
features can evolve without expanding a widely referenced utility assembly.
It has no dependency on Application, Domain, persistence, transport, logging
or validation packages.

## What It Provides

- `IRequest<TResponse>` and `IRequestHandler<TRequest, TResponse>` as the base
  request/response contracts.
- `ICommand`, `ICommand<TResponse>` and their handler aliases for state-changing
  use cases.
- `IQuery<TResponse>` and `IQueryHandler<TQuery, TResponse>` for read-only use
  cases.
- `IRequestDispatcher.Send(...)` for in-process dispatch.
- Explicit per-request pipeline behaviors through
  `IPipelineBehavior<TRequest, TResponse>`.
- `Unit` for commands that intentionally return no value.
- Fast failures for duplicate handler registrations and dispatches without a
  registered handler.

This is in-process composition, not durable messaging. Broker delivery,
retries, leases, deduplication and outbox behavior belong in their owning
infrastructure projects.

## Registration

The library deliberately uses explicit, strongly typed registrations:

```csharp
services.AddRequestHandler<CreateWidgetCommand, Widget, CreateWidgetCommandHandler>();
services.AddPipelineBehavior<CreateWidgetCommand, Widget, AuditBehavior<CreateWidgetCommand, Widget>>();
```

`AddRequestHandler` also registers the dispatcher, so calling `AddCqrs()`
separately is optional unless a composition root wants to declare the
dispatcher before registering handlers.

Application code should normally use its local helper:

```csharp
services.AddApplicationRequest<CreateWidgetCommand, Widget, CreateWidgetCommandHandler>();
```

That helper registers the handler and the Application-owned FluentValidation
behavior. FluentValidation intentionally remains outside this project.

Resolve and use the dispatcher from the current dependency-injection scope:

```csharp
Widget widget = await dispatcher.Send(command, cancellationToken);
```

Each request type has exactly one handler. Registering a second handler for the
same request throws `DuplicateRequestHandlerException` during composition.
Sending an unregistered request throws `MissingRequestHandlerException`.

## Pipeline Semantics

Behaviors execute in registration order around the handler. Given behaviors A
then B, execution is:

```text
A before -> B before -> handler -> B after -> A after
```

The cancellation token supplied to `Send` is forwarded to every behavior and
the handler. Behaviors must forward it when invoking `next` unless they are
intentionally terminating the pipeline.

The dispatcher is scoped. Handlers, request invokers and behaviors are
transient and are resolved from the dispatcher's current scope. Do not capture
the dispatcher in a singleton.

## AOT Compatibility

`Cqrs.csproj` opts into .NET AOT compatibility and reference-AOT verification.
Dispatch uses closed generic registrations and keyed dependency-injection
lookups. It does not scan assemblies, call `MakeGenericType`, invoke handlers
through `MethodInfo`, compile expression trees or generate code at runtime.

These choices make the library friendly to trimming and Native AOT and avoid
the startup cost of discovering handlers by reflection. The consuming host and
all its other dependencies still determine whether the complete application
can be published with Native AOT. In particular, this library does not make an
ASP.NET Core host AOT-compatible by itself.

## Ownership Rules

- Keep the core contracts and general dispatch mechanics in `Cqrs`.
- Keep feature handlers and feature-specific behaviors in their feature
  projects.
- Keep FluentValidation integration in Application while it is the only
  consumer.
- Prefer explicit registration over assembly scanning or runtime generic type
  construction.
- Preserve one handler per request. Use a future notification abstraction when
  one message intentionally has multiple consumers.
- Add optional integrations as separate projects or packages when they would
  otherwise add dependencies to the core.

Potential additions and the problems they would solve are tracked in
[`FUTURE-FEATURES.md`](FUTURE-FEATURES.md).
