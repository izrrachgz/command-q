# CommandQ.Messaging

`CommandQ.Messaging` provides small, transport-neutral contracts for asynchronous commands, events and scheduled work. It does not depend on
a broker, worker host, serializer, database or `CommandQ.Cqrs`.

## Install

```powershell
dotnet add package CommandQ.Messaging
```

## Sending messages

Inject `IMessageSender` and choose the semantic operation deliberately:

```csharp
await sender.SendAsync(new GenerateReport(reportId), cancellationToken: cancellationToken);
await sender.PublishAsync(new ReportGenerated(reportId), cancellationToken: cancellationToken);
await sender.ScheduleAsync(new ExpireReport(reportId), expiresAtUtc, cancellationToken: cancellationToken);
```

- `SendAsync` requires at least one configured destination and is normally used for commands or queued work.
- `PublishAsync` sends to every configured destination and succeeds when no destination is configured.
- `ScheduleAsync` sends work at or after an absolute instant. The adapter normalizes the value to UTC.

`MessageSendOptions` carries correlation, causation, logical deduplication, grouping, partitioning, expiry and string headers without exposing
transport types.

## Handling messages

Applications own their message types and handlers:

```csharp
public sealed record GenerateReport(Guid ReportId);

internal sealed class GenerateReportHandler : IMessageHandler<GenerateReport>
{
    public Task HandleAsync(GenerateReport message, MessageContext context, CancellationToken cancellationToken)
    {
        // Application-owned, duplicate-safe work.
    }
}
```

`MessageContext` exposes the message ID, correlation and causation IDs, UTC sent time, delivery attempt and headers. It deliberately excludes
broker-specific delivery handles and acknowledgement APIs.

## Delivery contract

Messaging transports normally provide at-least-once delivery. A handler can therefore see the same logical work more than once. Consumers must
use an application idempotency key or an inbox when duplicate effects would be harmful. Do not place large files or generated reports in a
message; store them in application-owned storage and send an identifier, location and integrity metadata.

Cancellation stops waiting and is passed to handlers. Once a transport has accepted a send, cancellation cannot retract or "unsend" it.

## AOT compatibility

This package uses closed generic contracts, performs no assembly scanning or runtime generic construction, and opts into .NET AOT and reference
AOT verification. Transport adapters determine whether a complete consuming host can be published with Native AOT.

## Ownership rules

- Message schemas, queue names, retry classification, idempotency policy and handler behavior belong to consuming applications.
- Broker connections, topology, serialization, acknowledgements and worker hosting belong to adapter packages.
- Database-backed inbox/outbox schemas and migrations belong to the consuming application.
- Keep in-process request dispatch in `CommandQ.Cqrs`; neither package depends on the other.
