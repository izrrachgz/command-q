Project: Persistence
Purpose: Data access implementations, SqlKata repositories, database command runners and SQL migrations.
Framework: .NET 10 (class library).

Repositories:
- IReadOnlyRepository is intended for query handlers and exposes read operations without exposing the write-capable SqlKata QueryFactory.
- IWritableRepository is intended for commands and migrations. It extends the read operations and exposes the write-capable QueryFactory.
- ReadOnlyRepository and WritableRepository can be created from IConfiguration through DI or manually with a connection string and DatabaseEngine.

Command runners:
- ICommandRunner executes raw commands and engine-specific bulk insert operations.
- AddCommandRunners registers all concrete runners and resolves the default ICommandRunner from ConnectionStrings:Engine.
- Concrete runners can also be constructed manually with a connection string when a service needs to target a different database engine or connection at runtime.

Database engines:
- Supported engines are MsSql, MySql, SqLite and PgSql.
- Migrations live under Persistence/Migrations/{Engine}.
- MigrationExtensions runs SQL migration scripts from the folder matching the configured engine.

Configuration:
- Main database connection string name: Db.
- Selected engine key: ConnectionStrings:Engine.
- Use appsettings.json for local defaults and environment variables or deployment configuration outside local development.

Notes:
- Keep application code depending on Domain interfaces instead of concrete persistence classes.
- Use IReadOnlyRepository for read-only query flows and IWritableRepository or ICommandRunner for write flows.
