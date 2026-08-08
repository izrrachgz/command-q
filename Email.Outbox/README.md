# Email Outbox

`Email.Outbox` provides the durable queue contracts and delivery workflow for
the transport-neutral `Email` library. It does not reference Persistence, API,
Application or IdentityServer.

## Enqueueing

Register the outbox core and one `IEmailOutboxStore` implementation, then inject
`IEmailOutbox`:

```csharp
services.AddEmailOutbox(configuration);

var result = await emailOutbox.EnqueueAsync(new EmailOutboxRequest
{
    Message = preparedMessage,
    IdempotencyKey = $"order-confirmation:{orderId}",
    AccountReference = accountId.ToString(),
    CorrelationId = correlationId
}, cancellationToken);
```

`EmailOutboxEnqueueResult` returns the durable record ID and indicates whether
the idempotency key resolved to an existing record.

File attachments and inline resources are read and copied into the serialized
payload before enqueueing. The worker therefore does not depend on temporary
source files remaining available. The default payload limit is 64 MiB and is
configured through `EmailOutbox:MaxPayloadBytes`.

## Delivery Semantics

`IEmailOutboxProcessor` claims a leased batch and sends all valid messages
through the collection overload of `IEmailSender`. This opens one SMTP
connection and authenticates once per claimed batch.

The workflow provides:

- idempotent enqueueing when the caller supplies an idempotency key
- exclusive time-limited claims for multiple worker instances
- exponential retry scheduling with a configured maximum delay
- permanent-failure and exhausted-attempt dead-letter states
- partial SMTP batch handling using `EmailDeliveryException.SentMessages`
- release of messages that were claimed but not attempted after a batch failure

Delivery is at least once. SMTP acceptance and the database status update cannot
share a transaction, so process termination after SMTP acceptance can cause a
duplicate delivery. Use stable business idempotency keys and make message
content tolerant of rare duplicates.

The outbox stores complete message bodies and attachment bytes. Protect the
database with appropriate file permissions, encrypted storage or database
encryption, retention controls and access auditing for the sensitivity of the
messages being queued.
