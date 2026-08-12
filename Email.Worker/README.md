# Email Worker

`Email.Worker` is the independently deployable host that composes `Email`,
`Email.Outbox` and `Email.Outbox.Persistence`.

Run locally with smtp4dev listening on SMTP port 25:

```powershell
.\scripts\run-email-worker.ps1
```

Visual Studio uses the `Email.Worker` launch profile, which sets
`DOTNET_ENVIRONMENT=Development`. This is required for the standard .NET
configuration pipeline to load the worker project's user-secrets. The helper
script sets the same environment explicitly.

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

The worker passes those exact subsections to the reusable libraries. The
persistence adapter therefore knows how to migrate, but does not decide when a
host should run migrations.

The OAuth SMTP option still expects a ready access token. Token acquisition and
refresh remain the responsibility of the host's credential provider; the
worker does not persist or refresh OAuth tokens automatically.

## Runtime Reload

The worker monitors `EmailWorker:Worker` and applies changes to polling interval,
batch size, lease duration, error delay and enabled state to new polling cycles.
Disabling the worker pauses polling without stopping the host; enabling it again
resumes polling. An active batch keeps its original lease and completes with its
current settings.

The SMTP sender monitors `EmailWorker:Email`. Changes to the SMTP host, port,
security mode, sender, delivery limits, authentication mode and supplied OAuth
token are used by the next batch. Each batch creates and disposes its own SMTP
transport, so an active connection is not changed underneath an in-progress
delivery.

Configuration reload events log safe operational metadata only. Passwords,
OAuth tokens and complete sender credentials are never written to logs.
