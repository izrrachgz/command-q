# Application Migrations

`Application.Migrations` owns the SQL schema history for the command-q
application database. It is separate from `Persistence` so the persistence
library remains reusable by consumers with unrelated schemas.

## Responsibilities

- Store provider-specific application SQL under `Migrations/{Engine}/{Module}`.
- Embed migration scripts in the assembly so deployments do not depend on source
  or content-file paths.
- Select the script set matching the configured repository engine.
- Validate migration IDs and ordering before execution.
- Reject a database ledger newer than the scripts in the deployed assembly.
- Read and update the `MigrationRecord` ledger.
- Apply pending scripts through `Persistence.Abstractions.IWritableRepository`.
- Expose registration and host execution through
  `AddApplicationMigrations()` and `RunApplicationMigrations()`.

The API is the composition host. It registers this project and decides whether
startup migrations run. Development always invokes the runner; other
environments use the API host policy at `Api:Migrations:RunOnStartup`.

The version 1 baseline contains the complete current application schema,
including `AccountPreference` and `ApplicationConfiguration`, and uses
provider-appropriate UTC storage for application temporal columns:
SQL Server `datetime2(6)`, PostgreSQL `timestamptz(6)`, MySQL UTC-session
`datetime(6)`, and fixed six-digit ISO-8601 UTC text in SQLite. All persistence
paths normalize to microseconds before writing, and new writes must still supply
explicitly UTC values from the application whenever server time is used.

The PostgreSQL and SQL Server baselines also own native row-level-security
policies for account data. API startup verifies that every required policy is
present and enabled even when startup migrations are disabled. MySQL and SQLite
do not have an equivalent native policy feature and are intentionally skipped.
See `docs/row-level-security.md` for protected tables, session-context behavior
and operational requirements.

When a migration adds, renames or removes an RLS-protected table, or changes its
predicate function or ownership column, update the private
`ApplicationRowLevelSecurityVerifier.ProtectedTables` mapping in the same
change. The verifier uses this mapping for both PostgreSQL and SQL Server;
leaving it stale can stop application startup or omit a new table from the
expected-policy check. Native PostgreSQL and SQL Server integration tests are
deferred; their required coverage is documented in `docs/row-level-security.md`.

## Adding A Migration

Add the same numbered schema change for every supported application engine:

```text
Migrations/MsSql/{Module}/migration-X.sql
Migrations/MySql/{Module}/migration-X.sql
Migrations/PgSql/{Module}/migration-X.sql
Migrations/SqLite/{Module}/migration-X.sql
```

Migration IDs are global within each engine, including scripts in different
module folders. Keep them contiguous and forward-only, and never renumber a
migration that may already be applied. The version 1 baseline creates and seeds
`MigrationRecord`; while that baseline is still changing, recreate the local
development database manually.

`Application.Migrations` must not reference `Application`, `Domain` or `Api`.
It may use `Common` database engine definitions and reusable capabilities from
`Persistence`.
