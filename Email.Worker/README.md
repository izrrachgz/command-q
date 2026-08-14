# Email Worker

`Email.Worker` is the independently deployable host that composes `Email`,
`Email.Outbox` and `Email.Outbox.Persistence`.

Run locally with smtp4dev listening on SMTP port 25:

```powershell
.\scripts\run-email-worker.ps1
```

Visual Studio uses the `Email.Worker` launch profile, which sets
`DOTNET_ENVIRONMENT=Development` and `ASPNETCORE_ENVIRONMENT=Development`.
This is required for the standard .NET configuration pipeline to load the
worker project's user-secrets. The helper script sets the same environment
explicitly and listens on `http://localhost:7105` by default.

The worker exposes unauthenticated health endpoints on its configured HTTP
listener:

- `/health/live` for process liveness.
- `/health/ready` for email outbox persistence readiness.
- `/health/checks` for the complete registered health check set.

Development enables SMTP delivery and the worker, applies the outbox MsSql
migrations, and stores the queue in the local `command_q` database through its
independent `EmailOutboxMigrationRecord` ledger. The smtp4dev web UI remains
available at `http://localhost:5000`.

Base, QA and Production settings keep delivery and polling disabled. Enable
both `EmailWorker:Email:Enabled` and `EmailWorker:Worker:Enabled` only after
supplying valid SMTP configuration. QA and Production also require an explicit
`EmailWorker:Migrations:RunOnStartup` deployment decision because startup
migrations are disabled there.

All custom settings owned by this executable live below `EmailWorker`:

- `EmailWorker:Email` configures SMTP transport.
- `EmailWorker:EmailOutbox` configures queue behavior and persistence.
- `EmailWorker:Worker` configures polling and leases.
- `EmailWorker:Migrations` controls this host's startup policy.
- `EmailWorker:OpenTelemetry` controls optional OTLP tracing and metrics export.

OpenTelemetry is disabled by default. When enabled, configure
`EmailWorker:OpenTelemetry:ServiceName` and
`EmailWorker:OpenTelemetry:Otlp:Endpoint`. The worker subscribes to its own
polling instrumentation and the `Email`, `Email.Outbox` and persistence
instrumentation sources. Recipients, subjects, bodies, tokens, correlation
IDs and outbox IDs are never emitted as telemetry tags.

The worker passes those exact subsections to the reusable libraries. The
persistence adapter therefore knows how to migrate, but does not decide when a
host should run migrations.

`EmailWorker:Email:DefaultProfile` selects the default profile and
`EmailWorker:Email:Profiles` contains the named SMTP configurations. Claimed
outbox messages are grouped by their logical profile, so each profile group
uses one SMTP connection. A message referencing a deleted profile is treated
as a permanent profile configuration failure and is dead-lettered.
Profile-specific sender and attachment limits are supported; profile-specific
rate limiting remains a later extension.

The OAuth SMTP option still expects a ready access token. Token acquisition and
refresh remain the responsibility of the host's credential provider; the
worker does not persist or refresh OAuth tokens automatically.

## Runtime Reload

The worker monitors `EmailWorker:Worker` and applies changes to polling interval,
batch size, lease duration, error delay and enabled state to new polling cycles.
Disabling the worker pauses polling without stopping the host; enabling it again
resumes polling. An active batch keeps its original lease and completes with its
current settings.

The SMTP sender monitors `EmailWorker:Email`. Changes to a profile's SMTP host,
port, security mode, sender, delivery limits, authentication mode and supplied
OAuth token are used by the next profile batch. Each profile sender creates and
disposes its own SMTP transport, so an active connection is not changed
underneath an in-progress delivery.

Configuration reload events log safe operational metadata only. Passwords,
OAuth tokens and complete sender credentials are never written to logs.
