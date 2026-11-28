# CommandQ.Persistence

`CommandQ.Persistence` provides reusable SqlKata repositories, SQL command runners and database session-context primitives for .NET 10 and .NET 11 applications. It supports SQL Server, PostgreSQL, MySQL and SQLite without owning an application schema or migration lifecycle.

## Install

```powershell
dotnet add package CommandQ.Persistence
```

This package depends on `CommandQ.Common` and `CommandQ.Time`.

## Configuration and registration

The default configuration uses `ConnectionStrings:Db` for the connection string and `ConnectionStrings:Engine` for the provider:

```json
{
  "ConnectionStrings": {
    "Db": "your-connection-string",
    "Engine": "PgSql"
  }
}
```

Supported engine names are `MsSql`, `MySql`, `SqLite` and `PgSql`.

```csharp
services.AddRepositories(configuration);
services.AddCommandRunners(configuration);
```

Store real connection strings in user secrets, environment variables or deployment configuration rather than committed settings.

## Repositories

- `IReadOnlyRepository` exposes query construction and read operations without exposing the write-capable `QueryFactory`.
- `IWritableRepository` extends the read contract and exposes write-capable access.
- `AddRepositories` registers separate scoped `ReadOnlyRepository` and `WritableRepository` instances.
- Concrete repositories can also be created with an explicit connection string and `DatabaseEngine` when a consumer manages its own composition.

Query handlers should depend on `IReadOnlyRepository`. Commands and other write flows should depend on `IWritableRepository` or `ICommandRunner`.

Use `Persistence.Constants.QueryComparisonOperator` for SqlKata comparison predicates instead of repeating SQL operator literals in consumers.

## Command runners

`ICommandRunner` executes raw parameterized commands and engine-specific bulk insert operations. Bulk inserts can either own their connection and transaction or participate in a caller-owned `IDbTransaction`; the latter overload never commits, rolls back or disposes the supplied transaction. `AddCommandRunners` registers every provider implementation and resolves the default runner from `ConnectionStrings:Engine`.

Command runner cancellation always propagates as `OperationCanceledException`; it is not converted into an empty result or failed `BasicResponse`. `ExecuteCommands` links its batch token with each command's token. Runner-owned bulk inserts attempt rollback with a short independent cleanup token so an already-cancelled caller token does not skip cleanup, while caller-owned transaction overloads leave rollback and disposal to the caller. See [`docs/cancellation.md`](https://github.com/izrrachgz/command-q/blob/main/docs/cancellation.md) for the repository-wide rules.

The MySQL runner uses batched, parameterized multi-row `INSERT` statements. It does not use `LOAD DATA LOCAL INFILE`, create temporary files or require the MySQL `local_infile` server setting.

## Session context and security

`IDatabaseSessionContextRepository` applies provider-neutral `DatabaseSessionValue` entries. PostgreSQL maps them to `set_config`, SQL Server maps them to `sp_set_session_context`, and MySQL/SQLite intentionally perform no session-context work.

This is an infrastructure capability. Consuming applications own context keys, protected tables, row-level-security policies and authorization semantics. Application handlers should use read/write repository contracts and let their application pipeline establish session context.

## UTC and provider behavior

`DateTimeFilterExtensions` centralizes UTC range normalization and the SQLite persistence representation. SQLite type handlers preserve UTC `DateTime` and `Guid` behavior. The SQLite command runner formats typed `DateTime` and `DateTimeOffset` bulk values with the same six-digit UTC text as SqlKata. `DateTime` inputs must have UTC kind. `DateTimeOffset` inputs are converted to UTC. The caller retains ownership of any supplied transaction.

The SQLite adapter can materialize legacy UTC text with zero to seven fractional digits. Text range predicates and indexes expect the canonical six-digit representation. If a consumer stored legacy values through the older runner, use a consumer-owned migration to read those values as UTC, rewrite them in canonical form with `UtcTime.FormatForPersistence`, and verify the affected range queries. Keep the migration limited to columns with a known UTC contract.

## Ownership rules

- This package does not own schemas, migration scripts, migration ledgers or startup migration policy.
- Keep commands parameterized and propagate cancellation tokens.
- Do not expose writable dependencies to read-only query flows.
- Put application table names, policy names and migration behavior in the consuming application.

## Tests

Provider-neutral query and UTC filter tests live in [`Persistence.Test`](../Persistence.Test/README.md).
