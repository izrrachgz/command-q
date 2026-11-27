# CommandQ code style

Use [.editorconfig](../../.editorconfig), the boundaries in [AGENTS.md](../../AGENTS.md) and the README of the library being changed. Add abstractions where consumers need a common contract. Keep product-specific code with its consumer.

- Keep registration explicit. Preserve closed-generic CQRS registration, registration order and scope semantics. Preserve the AOT and dependency guarantees documented by [Cqrs](../../Cqrs/README.md) and [Messaging](../../Messaging/README.md).
- Keep Wolverine and broker behavior in the optional [RabbitMQ adapter](../../Messaging.Wolverine.RabbitMq/README.md). Consumers own message schemas, business handlers and retry or idempotency policy.
- Follow the [cancellation contract](../cancellation.md) at every cancellable boundary. Check the owner of a resource before attempting rollback, cleanup or disposal.
- Preserve the caller's current culture for scoped localization and the configured source order. Inject `TimeProvider` for clock-dependent behavior and preserve existing UTC precision and timezone validation.
- Keep the public surface small and implementation types internal when possible. Use [LoggerMessage event reservations](../logging-event-ids.md) and keep IDs stable across releases.
