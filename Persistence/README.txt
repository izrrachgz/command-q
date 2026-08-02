Project: Persistence
Purpose: Data access implementations, SqlKata repositories, database command runners and SQL migrations.
Framework: .NET 10 (class library).

Repositories:
- Repository interfaces live under `Persistence.Interfaces`.
- IReadOnlyRepository is intended for query handlers and exposes read operations without exposing the write-capable SqlKata QueryFactory.
- IWritableRepository is intended for commands and migrations. It extends the read operations and exposes the write-capable QueryFactory.
- ReadOnlyRepository and WritableRepository can be created from IConfiguration through DI or manually with a connection string and DatabaseEngine.

Command runners:
- Command runner interfaces live under `Persistence.Interfaces`.
- ICommandRunner executes raw commands and engine-specific bulk insert operations.
- AddCommandRunners registers all concrete runners and resolves the default ICommandRunner from ConnectionStrings:Engine.
- Concrete runners can also be constructed manually with a connection string when a service needs to target a different database engine or connection at runtime.

Database engines:
- Supported engines are MsSql, MySql, SqLite and PgSql.
- Migrations live under Persistence/Migrations/{Engine}/{Module}.
- MigrationExtensions scans SQL migration scripts recursively from the folder matching the configured engine.
- Migration IDs must be unique per engine, even when scripts live in different module folders.
- Core/migration-1.sql creates the initial schema and seeds the MigrationRecord table.
- Migration scripts are forward-only and do not drop existing tables; manually recreate the development database while the v1 baseline is still changing.
- Migration records include MigrationId, Module, Name, Checksum and AppliedUtc.
- Development startup invokes the migration runner by default; non-development startup execution is controlled by Migrations:RunOnStartup in the API configuration.

Configuration:
- Main database connection string name: Db.
- Selected engine key: ConnectionStrings:Engine.
- Optional migrations root path: Persistence:MigrationsPath.
- Use appsettings.json for local defaults and environment variables or deployment configuration outside local development.

Notes:
- Keep application code depending on `Persistence.Interfaces` instead of concrete persistence classes.
- Use IReadOnlyRepository for read-only query flows and IWritableRepository or ICommandRunner for write flows.
