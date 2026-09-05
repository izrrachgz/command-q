Project: Persistence
Purpose: Reusable data access implementations, SqlKata repositories and database command runners.
Framework: .NET 10 (class library).

Repositories:
- Repository interfaces live under `Persistence.Abstractions`.
- IReadOnlyRepository is intended for query handlers and exposes read operations without exposing the write-capable SqlKata QueryFactory.
- IWritableRepository is intended for write-capable consumers. It extends the read operations and exposes the write-capable QueryFactory.
- AddRepositories maps IReadOnlyRepository to a scoped ReadOnlyRepository and IWritableRepository to a separate scoped WritableRepository.
- ReadOnlyRepository and WritableRepository can also be created manually with a connection string and DatabaseEngine.
- IDatabaseSessionContextRepository is the infrastructure-only repository capability for applying provider-neutral DatabaseSessionValue items. PostgreSQL maps them to set_config, SQL Server maps them to sp_set_session_context, and MySQL/SQLite intentionally perform no session-context work. Application handlers should depend on IReadOnlyRepository or IWritableRepository and must not set session context directly.

Command runners:
- Command runner interfaces live under `Persistence.Abstractions`.
- ICommandRunner executes raw commands and engine-specific bulk insert operations.
- AddCommandRunners registers all concrete runners and resolves the default ICommandRunner from ConnectionStrings:Engine.
- Concrete runners can also be constructed manually with a connection string when a service needs to target a different database engine or connection at runtime.

Database engines:
- Supported engines are MsSql, MySql, SqLite and PgSql.
- `DateTimeFilterExtensions.ToDateTimeFilterValue` centralizes UTC normalization and the SQLite datetime representation used by provider-aware query predicates.
- Persistence does not own an application schema, SQL migration scripts, migration ledger or startup policy.
- Application migrations live in Application.Migrations, which references Persistence and uses IWritableRepository to execute its embedded scripts.
- Persistence provides only the reusable session-context mechanism; application account RLS policy names, keys and protected-table ownership remain application-owned.

Configuration:
- Main database connection string name: Db.
- Selected engine key: ConnectionStrings:Engine.
- Use appsettings.json for local defaults and environment variables or deployment configuration outside local development.

Notes:
- Keep application code depending on `Persistence.Abstractions` instead of concrete persistence classes.
- Use IReadOnlyRepository for read-only query flows and IWritableRepository or ICommandRunner for write flows.
