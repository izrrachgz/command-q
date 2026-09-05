# Application Row-Level Security

The application database uses native row-level security (RLS) as defense in
depth for account-owned data when the selected engine is PostgreSQL or SQL
Server. Existing application `AccountId` predicates remain required; database
policies protect against an accidentally missing or incorrect predicate.

MySQL and SQLite remain supported application engines, but they do not provide
a native table-policy facility equivalent to PostgreSQL `CREATE POLICY` or SQL
Server `CREATE SECURITY POLICY`. The application therefore does not provision
RLS session context or perform an RLS startup check for those engines.

## Protected Data

The baseline protects these tables directly by `AccountId`, or by `Id` for
`Account`:

- `Account`
- `AccountIdentity`
- `AccountActivity`
- `AccountPreference`
- `Address`
- `Contact`
- `AccountFile`
- `Register`

`RegisterEvent` and `RegisterPlace` are protected through their owning
`Register`. `Event` and `Place` are catalogs and intentionally remain outside
account RLS. Operational tables such as `HealthCheckLog`,
`ApplicationConfiguration` and `MigrationRecord` are also outside account RLS.

## Database Session Context

Policies consume two internal session keys:

- `command_q.account_id` identifies the account allowed by the current data
  operation.
- `command_q.rls_bypass` permits narrowly scoped system operations.

Every registered CQRS request runs through `AccountRowLevelSecurityBehavior`.
Requests that implement `IAccountScopedRequest` supply the account context;
identity resolution, username lookup and account provisioning implement the
narrow `ISystemScopedRequest` marker. Unclassified requests receive a
restrictive context with no account and no bypass, so a future protected-table
access fails closed instead of inheriting session state.

Handlers and services must not provision RLS context themselves. Queries use a
scoped `ReadOnlyRepository`; commands use a separate scoped
`WritableRepository`. The behavior selects and provisions the matching
connection before the handler runs, so command transactions inherit the same
context. Nested request scopes restore the previous context on disposal, and
the outer scope restores restrictive values before the selected connection
returns to the provider pool. Validation runs before RLS provisioning.

The reusable Persistence project exposes provider-neutral
`DatabaseSessionValue` values. Its repository implementation selects the native
mechanism from `DatabaseEngine`: PostgreSQL uses `set_config`, SQL Server uses
`sp_set_session_context`, and unsupported engines perform no session-context
work. A future native implementation can be added behind this boundary without
changing account handlers.

Session context is trusted application state. It must be derived from the
authenticated account or an internal system workflow, never from a client-body,
route or query value without authorization. RLS does not make an application
safe from arbitrary SQL execution: SQL Server allows a database user to set its
own session context, and PostgreSQL custom settings are session-controlled.
Continue to prevent SQL injection, parameterize values and keep database
credentials private.

## PostgreSQL

The baseline creates `security.can_access_account(UUID)` and
`security.can_access_register(BIGINT)`. Every protected table has an
`AccountIsolationPolicy` for all commands, with both `USING` and `WITH CHECK`.
Application tables use the default `public` schema; keep it on the API role's
search path because application queries intentionally use unqualified table
names.
Both RLS flags are mandatory:

```sql
ALTER TABLE "TableName" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "TableName" FORCE ROW LEVEL SECURITY;
```

`FORCE` prevents an ordinary table owner from silently bypassing the policy.
Do not give the API database role PostgreSQL superuser or `BYPASSRLS`
privileges. Deployment/migration credentials remain sensitive even though the
current baseline provisions its narrowly scoped bypass while needed.
The baseline grants only schema usage and predicate-function execution needed
to evaluate and verify the policies; it does not grant access to protected
tables.

Inspect the effective state with:

```sql
SELECT table_schema.nspname,
       protected_table.relname,
       protected_table.relrowsecurity,
       protected_table.relforcerowsecurity,
       policy.polname,
       policy.polcmd
FROM pg_catalog.pg_class AS protected_table
JOIN pg_catalog.pg_namespace AS table_schema
  ON table_schema.oid = protected_table.relnamespace
LEFT JOIN pg_catalog.pg_policy AS policy
  ON policy.polrelid = protected_table.oid
WHERE policy.polname = 'AccountIsolationPolicy';
```

## SQL Server

The baseline creates schema-bound `Security.fn_account_access` and
`Security.fn_register_access` predicate functions. The enabled
`Security.AccountIsolationPolicy` applies one filter predicate plus `AFTER
INSERT` and `AFTER UPDATE` block predicates to each protected table. Filter
predicates also prevent updates or deletes of rows hidden from the account.
The `Security` schema grants public `VIEW DEFINITION` so a low-privilege API
principal can perform the startup integrity check. Policy definitions contain
no credentials or other secrets.

Inspect the effective state with:

```sql
SELECT policy_schema.name AS PolicySchema,
       policy.name AS PolicyName,
       policy.is_enabled,
       OBJECT_SCHEMA_NAME(predicate_definition.target_object_id) AS TargetSchema,
       OBJECT_NAME(predicate_definition.target_object_id) AS TargetTable,
       predicate_definition.predicate_type_desc,
       predicate_definition.operation_desc
FROM sys.security_policies AS policy
JOIN sys.schemas AS policy_schema
  ON policy_schema.schema_id = policy.schema_id
JOIN sys.security_predicates AS predicate_definition
  ON predicate_definition.object_id = policy.object_id
WHERE policy_schema.name = N'Security'
  AND policy.name = N'AccountIsolationPolicy';
```

## Startup Enforcement

After optional application migrations and before configuration recording or
health execution, API startup verifies native RLS unconditionally for
PostgreSQL and SQL Server. Startup fails if:

- an expected protected table is missing from the policy;
- a PostgreSQL table does not have both `ENABLE` and `FORCE` active;
- a PostgreSQL `USING` or `WITH CHECK` policy is missing;
- the SQL Server policy is disabled;
- an expected SQL Server filter or write-block predicate is missing; or
- an expected predicate function is missing.

This is intentionally not controlled by a feature flag. Disabling an RLS
policy directly in the database prevents the next API instance from starting.
MySQL and SQLite log that native verification is unsupported and continue.

`ApplicationRowLevelSecurityVerifier.ProtectedTables` is the authoritative
application-side mapping of each protected table to its predicate function and
ownership column. Both provider catalogs are checked against that exact
mapping. Any migration that adds, renames or removes an RLS-protected table, or
changes its predicate function or ownership column, must update the mapping in
the same change. Forgetting it will either stop startup or leave a newly
introduced account-owned table outside the expected-policy check.

## Deferred Native Provider Tests

Automated native-provider integration tests are intentionally deferred. Unit
and migration-content tests do not replace exercising the policies on actual
PostgreSQL and SQL Server instances. When this coverage is added, prefer
repeatable, opt-in CI infrastructure such as Testcontainers while keeping the
ordinary unit-test suite independent of Docker.

The native suite should verify:

- account A cannot read, update or delete account B data;
- cross-account inserts and ownership changes are blocked;
- `RegisterEvent` and `RegisterPlace` inherit `Register` ownership;
- the three allowlisted system workflows can perform their required operations;
- disabling, removing or rewiring a policy prevents startup; and
- pooled connections do not retain account or bypass context between requests.

## Adding Native Support To Another Engine

Provider capability detection is centralized in
`DatabaseEngineSecurityExtensions.SupportsNativeRowLevelSecurity`. If a future
MySQL or SQLite release adds a native, enforceable table-policy facility:

1. enable that engine in the capability extension;
2. map `DatabaseSessionValue` in `DatabaseSessionContext`;
3. add the provider policy DDL to its migration baseline or next migration;
4. add an engine catalog query to `ApplicationRowLevelSecurityVerifier`; and
5. replace the unsupported-engine tests with provider integration coverage.

Account request contracts and handlers should not need to change.

## Migration Lifecycle

RLS currently belongs to `migration-1.sql` because version 1 has not been
released. Recreate existing development databases after pulling this change.
Once version 1 becomes immutable, policy changes must use the next matching
forward-only migration for every affected engine rather than editing the
baseline.

When adding an account-owned table:

1. Add and index a non-null ownership column, preferably `AccountId`.
2. Add the PostgreSQL policy and both table flags.
3. Add SQL Server filter, insert-block and update-block predicates.
4. Add the table, predicate function and ownership column to
   `ApplicationRowLevelSecurityVerifier.ProtectedTables`. For a rename,
   removal or predicate change, update this mapping in the same migration
   change.
5. Make account-owned requests implement `IAccountScopedRequest`; use
   `ISystemScopedRequest` only for an audited cross-account workflow.
6. Update migration-content and provider integration tests.

## References

- [PostgreSQL row security policies](https://www.postgresql.org/docs/current/ddl-rowsecurity.html)
- [PostgreSQL `pg_class` RLS flags](https://www.postgresql.org/docs/current/catalog-pg-class.html)
- [SQL Server row-level security](https://learn.microsoft.com/en-us/sql/relational-databases/security/row-level-security)
- [SQL Server security-policy catalog](https://learn.microsoft.com/en-us/sql/relational-databases/system-catalog-views/sys-security-policies-transact-sql)
- [MySQL access-control model](https://dev.mysql.com/doc/refman/8.4/en/access-control.html)
- [SQLite authorization callbacks](https://www.sqlite.org/c3ref/set_authorizer.html)
