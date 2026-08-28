# Email Outbox

`Email.Outbox` provides the durable queue contracts and delivery workflow for
the transport-neutral `Email` library. It does not reference Persistence, API,
Application or IdentityServer.

## Enqueueing

Register the producer services and one `IEmailOutboxStore` implementation, then
inject `IEmailOutbox`:

```csharp
services.AddEmailOutboxProducer(configuration.GetRequiredSection("Api:EmailOutbox"));

var result = await emailOutbox.EnqueueAsync(new EmailOutboxRequest
{
    Message = preparedMessage,
    ProfileName = "Transactional",
    IdempotencyKey = $"order-confirmation:{orderId}",
    AccountReference = accountId.ToString(),
    CorrelationId = correlationId
}, cancellationToken);
```

`EmailOutboxEnqueueResult` returns the durable record ID and indicates whether
the idempotency key resolved to an existing record. Creation, claim, lease,
retry, sent and dead-letter timestamps use NodaTime `Instant`; lease and retry
intervals use `Duration`. These types make UTC semantics part of the contract
instead of relying on `DateTimeKind` or caller offsets.

Producer and processor registration use the host's existing `TimeProvider`, or
the shared time registration installs `TimeProvider.System` when none exists.
Register a custom provider before `AddEmailOutboxProducer` when deterministic
time is required.

Current instants and persistence boundaries normalize to the shared microsecond
precision before durable values are returned. This keeps equality and retry
boundaries consistent across SQL Server, PostgreSQL, MySQL and SQLite.

Enqueue instrumentation distinguishes newly persisted messages from duplicate
idempotency hits. Duplicate attempts increment `email.outbox.duplicates`, while
only newly persisted messages increment `email.outbox.enqueued`.

`ProfileName` stores only a logical SMTP profile name. It never stores SMTP
credentials. Omitting it stores the durable `default` marker, which the worker
resolves to its configured `Email:DefaultProfile`.

Within command-q, application use cases should normally use
`QueueEmailCommand` or `IApplicationEmailService` from
`Application.Modules.Notifications`. That module validates prepared messages
and maps application metadata into this transport-independent contract. A
producer host still owns registration of the outbox core and persistence
adapter.

File attachments and inline resources are read and copied into the serialized
payload before enqueueing. The worker therefore does not depend on temporary
source files remaining available. The default payload limit is 64 MiB and is
configured through `EmailOutbox:MaxPayloadBytes`. The producer-side
`EmailOutbox:DeliveryPolicy` applies the same recipient and content checks
before the JSON payload is persisted. The worker's active
`Email:Profiles:<name>:DeliveryPolicy` remains authoritative and revalidates
the message before measuring and sending its final MIME representation.
The reusable extension also accepts an `IConfigurationSection`, allowing each
host to choose its own prefix without teaching the outbox library about host
names.

## Delivery Semantics

`IEmailOutboxProcessor` claims a leased batch, groups valid messages by logical
profile, and sends each group through one profile-bound sender. This opens one
SMTP connection and authenticates once per profile group.

Consumer hosts such as `Email.Worker` pass their owned outbox section to
`AddEmailOutbox(section)`, which combines producer registration with
`AddEmailOutboxProcessor()`. Producer-only
hosts must not register the processor because it requires `IEmailSender`.

The workflow provides:

- idempotent enqueueing when the caller supplies an idempotency key
- exclusive time-limited claims for multiple worker instances
- exponential retry scheduling with a configured maximum delay
- permanent-failure and exhausted-attempt dead-letter states
- partial SMTP batch handling using `EmailDeliveryException.SentMessages`
- release of messages that were claimed but not attempted after a batch failure
- dead-lettering of messages that reference a profile deleted from worker configuration

Delivery is at least once. SMTP acceptance and the database status update cannot
share a transaction, so process termination after SMTP acceptance can cause a
duplicate delivery. Use stable business idempotency keys and make message
content tolerant of rare duplicates.

When enqueueing under a W3C `Activity`, the outbox stores its trace-parent and
trace-state alongside the message. The worker creates a local profile-processing
activity and adds the stored contexts as trace links. Trace context is used for
diagnostic continuity only and is never emitted as a metric tag.

The outbox stores complete message bodies and attachment bytes. Protect the
database with appropriate file permissions, encrypted storage or database
encryption, retention controls and access auditing for the sensitivity of the
messages being queued.
