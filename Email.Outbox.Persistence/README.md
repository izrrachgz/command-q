# Email Outbox Persistence

`Email.Outbox.Persistence` implements `IEmailOutboxStore` with the reusable
SqlKata capabilities from `Persistence`. It owns only the email-outbox schema
and does not alter the application database migrations.

## Configuration

SQLite is the committed default and requires no external database server:

```json
{
  "EmailOutbox": {
    "Persistence": {
      "Engine": "SqLite",
      "ConnectionString": "Data Source=App_Data/command_q_email_outbox.db;Version=3;Foreign Keys=True;Journal Mode=WAL;Busy Timeout=5000;Synchronous=Normal;",
      "RunMigrationsOnStartup": true
    }
  }
}
```

`MsSql`, `PgSql` and `MySql` are also supported. Supply a provider-appropriate
connection string and select the matching engine. Keep non-SQLite credentials
in the consuming host's secret store.

The adapter stores provider scripts as embedded resources under
`Migrations/{Engine}`. Its private `EmailOutboxMigrationRecord` ledger makes
execution forward-only and skips already-applied migrations.

Startup migration execution is enabled for Development and disabled in the
committed QA/Production worker settings. In a multi-instance deployment, run
migrations in one deployment step or one designated instance before enabling
the workers.

Claims use conditional updates and unique lease IDs. A worker can reclaim a
record after `LockedUntilUtc`, allowing recovery from terminated processes.
Configure the lease longer than the worst expected SMTP batch duration to avoid
two workers processing the same batch concurrently.
