# Email Outbox Persistence

`Email.Outbox.Persistence` implements `IEmailOutboxStore` with the reusable
SqlKata capabilities from `Persistence`. It owns only the email-outbox schema
and does not alter the application database migrations.

## Configuration

The CommandQ worker's Development settings use local MsSql `command_q`, sharing
the physical application database while retaining an independent schema and
migration ledger. A consuming host can pass this section directly:

```json
{
  "EmailOutbox": {
    "Persistence": {
      "Engine": "MsSql",
      "ConnectionString": "Server=localhost\\SQLEXPRESS;Database=command_q;Trusted_Connection=True;TrustServerCertificate=True;"
    }
  }
}
```

`SqLite`, `PgSql` and `MySql` are also supported. Supply a provider-appropriate
connection string and select the matching engine. Keep database credentials in
the consuming host's secret store.

The adapter stores provider scripts as embedded resources under
`Migrations/{Engine}`. Its private `EmailOutboxMigrationRecord` ledger makes
execution forward-only and skips already-applied migrations.

`RunEmailOutboxMigrations` applies pending migrations when the consuming host
calls it. Startup policy is intentionally not part of persistence options. The
CommandQ worker owns that decision through
`EmailWorker:Migrations:RunOnStartup`: Development enables it and the committed
QA/Production settings disable it. In a multi-instance deployment, run
migrations in one deployment step or one designated instance before enabling
the workers.

Claims use conditional updates and unique lease IDs. A worker can reclaim a
record after `LockedUntilUtc`, allowing recovery from terminated processes.
Configure the lease longer than the worst expected SMTP batch duration to avoid
two workers processing the same batch concurrently.
