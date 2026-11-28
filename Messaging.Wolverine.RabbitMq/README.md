# CommandQ.Messaging.Wolverine.RabbitMQ

`CommandQ.Messaging.Wolverine.RabbitMQ` implements `CommandQ.Messaging` with Wolverine and RabbitMQ. It provides explicit message routing and
worker registration while leaving business contracts, retry classification, persistence and deployment topology to consuming applications.

## Install

```powershell
dotnet add package CommandQ.Messaging.Wolverine.RabbitMQ
```

The package transitively includes `CommandQ.Messaging`, `WolverineFx` and `WolverineFx.RabbitMQ`.

## Configure a worker

```csharp
builder.Host.UseWolverine(options =>
{
    RabbitMqMessagingBuilder messaging = options.UseCommandQMessagingWithRabbitMq(
        builder.Configuration.GetConnectionString("RabbitMq")!,
        rabbit =>
        {
            rabbit.AutoProvision = builder.Environment.IsDevelopment();
        });

    messaging.AddQueueWorker<GenerateReport, GenerateReportHandler>(
        "report-generate",
        worker =>
        {
            worker.PrefetchCount = 8;
            worker.MaxDegreeOfParallelism = 2;
            worker.SingleActiveConsumer = false;
        });
});
```

`AddQueueWorker` explicitly registers the concrete handler, message route and listener. The same queue cannot be registered twice in one host.
Handlers implement `IMessageHandler<TMessage>` and remain independent from Wolverine types.

Set `SingleActiveConsumer = true` to keep one broker consumer active across replicas. RabbitMQ keeps the other consumers available for failover.
The active worker uses `MaxDegreeOfParallelism` to control how many messages it handles at once. For workloads that process one message at a time,
set `MaxDegreeOfParallelism = 1` along with `SingleActiveConsumer = true`. Keep handlers idempotent because broker delivery remains at least once.

For a send-only host, register only the route:

```csharp
builder.Host.UseWolverine(options =>
{
    options
        .UseCommandQMessagingWithRabbitMq(builder.Configuration.GetConnectionString("RabbitMq")!)
        .RouteToQueue<GenerateReport>("report-generate");
});
```

## Reliability defaults

- Durable, non-auto-delete queues.
- RabbitMQ quorum queues unless explicitly disabled.
- Publisher confirmations and confirmation tracking.
- Native acknowledgements after successful parallel processing.
- Bounded prefetch and handler concurrency per worker.
- Wolverine correlation and causation tracking.
- A two-minute bounded broker initialization window.
- Static handler registries enabled, with Wolverine's documented runtime fallback when generated code is absent.

Automatic broker provisioning is off by default. Prefer infrastructure-as-code and a production identity without topology deletion rights. Enable
`AutoProvision` deliberately for development or when the deployment model makes the application responsible for topology.

The adapter does not install a catch-all retry policy. Applications must identify transient exceptions and configure their Wolverine policies;
retrying every exception can repeat permanent failures and external side effects. RabbitMQ/Wolverine dead-letter behavior remains available for
exhausted or unhandled failures.

## Inbox and outbox

The default worker uses RabbitMQ native acknowledgements and does not require a database. Applications that require a transactional outbox or
durable inbox can configure Wolverine persistence and endpoint durability in their composition root. The application owns provider selection,
schema migrations, retention and the transaction joining business data to outgoing messages.

## AOT and code generation

The transport-neutral package is fully AOT-compatible. This adapter uses explicit handler types and enables Wolverine static registries. A Native
AOT host must follow the Wolverine version's code-generation workflow, including generating and committing handler code before publishing:

```powershell
dotnet run -- codegen write
```

Wolverine still determines the complete host's trimming and AOT compatibility. Keep the normal release build and an application-level Native AOT
publish smoke test as separate gates.

## Security and privacy

Keep RabbitMQ connection strings in secret storage. Do not place credentials, access tokens, large files or unnecessary personal data in message
bodies or headers. This adapter does not enable enhanced exception metadata in broker dead-letter headers because exception text and stack traces
can expose application data.
