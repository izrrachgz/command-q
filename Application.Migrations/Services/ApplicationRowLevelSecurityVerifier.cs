using System.Data;
using System.Data.Common;
using Common.Enumerations;
using Persistence.Abstractions;
using Persistence.Extensions;
using Microsoft.Extensions.Logging;
using Application.Migrations.Logging;

namespace Application.Migrations.Services
{
    internal sealed class ApplicationRowLevelSecurityVerifier(
        IWritableRepository repository,
        ILogger<ApplicationRowLevelSecurityVerifier> logger)
    {
        private const string PostgreSqlAccountPredicateFunction = "can_access_account";
        private const string PostgreSqlRegisterPredicateFunction = "can_access_register";
        private const string SqlServerAccountPredicateFunction = "fn_account_access";
        private const string SqlServerRegisterPredicateFunction = "fn_register_access";
        private const string AccountIdColumn = "AccountId";
        private const string RegisterIdColumn = "RegisterId";

        private static readonly ProtectedTablePolicy[] ProtectedTables =
        [
            new("Account", PostgreSqlAccountPredicateFunction, SqlServerAccountPredicateFunction, "Id"),
            new("AccountIdentity", PostgreSqlAccountPredicateFunction, SqlServerAccountPredicateFunction, AccountIdColumn),
            new("AccountActivity", PostgreSqlAccountPredicateFunction, SqlServerAccountPredicateFunction, AccountIdColumn),
            new("AccountPreference", PostgreSqlAccountPredicateFunction, SqlServerAccountPredicateFunction, AccountIdColumn),
            new("Address", PostgreSqlAccountPredicateFunction, SqlServerAccountPredicateFunction, AccountIdColumn),
            new("Contact", PostgreSqlAccountPredicateFunction, SqlServerAccountPredicateFunction, AccountIdColumn),
            new("AccountFile", PostgreSqlAccountPredicateFunction, SqlServerAccountPredicateFunction, AccountIdColumn),
            new("Register", PostgreSqlAccountPredicateFunction, SqlServerAccountPredicateFunction, AccountIdColumn),
            new("RegisterEvent", PostgreSqlRegisterPredicateFunction, SqlServerRegisterPredicateFunction, RegisterIdColumn),
            new("RegisterPlace", PostgreSqlRegisterPredicateFunction, SqlServerRegisterPredicateFunction, RegisterIdColumn)
        ];

        public async Task VerifyAsync(CancellationToken cancellationToken = default)
        {
            if (!repository.Engine.SupportsNativeRowLevelSecurity())
            {
                logger.RowLevelSecurityNotSupported(repository.Engine);
                return;
            }

            bool valid = await ExecuteVerificationAsync(cancellationToken);

            if (!valid)
            {
                logger.RowLevelSecurityVerificationFailed(repository.Engine);
                throw new InvalidOperationException(
                    $"Required row-level-security policies are missing or disabled for database engine '{repository.Engine}'.");
            }

            logger.RowLevelSecurityVerified(repository.Engine, ProtectedTables.Length);
        }

        private async Task<bool> ExecuteVerificationAsync(CancellationToken cancellationToken)
        {
            if (repository.Db.Connection is not DbConnection connection)
                throw new InvalidOperationException("The configured repository does not expose an asynchronous database connection.");

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            await using DbCommand command = connection.CreateCommand();
            string protectedPolicyParameters = CreateProtectedPolicyParameters(command, repository.Engine);
            command.CommandText = CreateVerificationSql(repository.Engine, protectedPolicyParameters);

            object? result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture) == ProtectedTables.Length;
        }

        private static string CreateVerificationSql(DatabaseEngine engine, string protectedPolicyParameters)
        {
            return engine switch
            {
                DatabaseEngine.PgSql => PostgreSqlVerification(protectedPolicyParameters),
                DatabaseEngine.MsSql => SqlServerVerification(protectedPolicyParameters),
                _ => throw new InvalidOperationException(
                    $"Native row-level security is enabled without a verifier for database engine '{engine}'.")
            };
        }

        private static string CreateProtectedPolicyParameters(DbCommand command, DatabaseEngine engine)
        {
            string[] parameterRows = new string[ProtectedTables.Length];

            for (int index = 0; index < ProtectedTables.Length; index++)
            {
                string tableParameter = $"@protected_table_{index}";
                string functionParameter = $"@predicate_function_{index}";
                string columnParameter = $"@predicate_column_{index}";
                ProtectedTablePolicy policy = ProtectedTables[index];

                AddParameter(command, tableParameter, policy.TableName);
                AddParameter(command, functionParameter, policy.GetPredicateFunctionName(engine));
                AddParameter(command, columnParameter, policy.ColumnName);
                parameterRows[index] = $"({tableParameter}, {functionParameter}, {columnParameter})";
            }

            return string.Join(", ", parameterRows);
        }

        private static void AddParameter(DbCommand command, string name, string value)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private static string PostgreSqlVerification(string protectedPolicyParameters) => $@"
            WITH expected_policy (table_name, predicate_function, predicate_column) AS
            (
                VALUES {protectedPolicyParameters}
            )
            SELECT COUNT(*)
            FROM expected_policy
            INNER JOIN pg_catalog.pg_class AS protected_table
                ON protected_table.relname = expected_policy.table_name
            INNER JOIN pg_catalog.pg_namespace AS table_schema
                ON table_schema.oid = protected_table.relnamespace
            INNER JOIN pg_catalog.pg_policy AS policy
                ON policy.polrelid = protected_table.oid
            WHERE table_schema.nspname = current_schema()
              AND protected_table.relrowsecurity
              AND protected_table.relforcerowsecurity
              AND policy.polname = 'AccountIsolationPolicy'
              AND policy.polcmd = '*'
              AND policy.polroles = ARRAY[0::OID]
              AND REGEXP_REPLACE(
                    pg_get_expr(policy.polqual, policy.polrelid, FALSE),
                    '\s+',
                    '',
                    'g') = REGEXP_REPLACE(
                        FORMAT(
                            'security.%I(%I)',
                            expected_policy.predicate_function,
                            expected_policy.predicate_column),
                        '\s+',
                        '',
                        'g')
              AND REGEXP_REPLACE(
                    pg_get_expr(policy.polwithcheck, policy.polrelid, FALSE),
                    '\s+',
                    '',
                    'g') = REGEXP_REPLACE(
                        FORMAT(
                            'security.%I(%I)',
                            expected_policy.predicate_function,
                            expected_policy.predicate_column),
                        '\s+',
                        '',
                        'g')
              AND to_regprocedure('security.{PostgreSqlAccountPredicateFunction}(uuid)') IS NOT NULL
              AND to_regprocedure('security.{PostgreSqlRegisterPredicateFunction}(bigint)') IS NOT NULL;";

        private static string SqlServerVerification(string protectedPolicyParameters) => $@"
            WITH expected_policy (TableName, PredicateFunction, PredicateColumn) AS
            (
                SELECT TableName, PredicateFunction, PredicateColumn
                FROM (VALUES {protectedPolicyParameters}) AS expected_values (TableName, PredicateFunction, PredicateColumn)
            )
            SELECT COUNT(*)
            FROM
            (
                SELECT target.object_id
                FROM expected_policy
                INNER JOIN sys.tables AS target
                    ON target.name = expected_policy.TableName
                INNER JOIN sys.security_predicates AS predicate_definition
                    ON target.object_id = predicate_definition.target_object_id
                INNER JOIN sys.security_policies AS policy
                    ON policy.object_id = predicate_definition.object_id
                INNER JOIN sys.schemas AS target_schema
                    ON target_schema.schema_id = target.schema_id
                INNER JOIN sys.schemas AS policy_schema
                    ON policy_schema.schema_id = policy.schema_id
                WHERE target_schema.name = N'dbo'
                  AND policy_schema.name = N'Security'
                  AND policy.name = N'AccountIsolationPolicy'
                  AND policy.is_enabled = 1
                  AND LOWER(REPLACE(REPLACE(REPLACE(
                        predicate_definition.predicate_definition,
                        N'[',
                        N''),
                        N']',
                        N''),
                        N' ',
                        N'')) = LOWER(CONCAT(
                        N'(Security.',
                        expected_policy.PredicateFunction,
                        N'(',
                        expected_policy.PredicateColumn,
                        N'))'))
                GROUP BY target.object_id
                HAVING SUM(CASE WHEN predicate_definition.predicate_type_desc = N'FILTER' THEN 1 ELSE 0 END) = 1
                   AND SUM(CASE WHEN predicate_definition.predicate_type_desc = N'BLOCK'
                                 AND predicate_definition.operation_desc = N'AFTER INSERT' THEN 1 ELSE 0 END) = 1
                   AND SUM(CASE WHEN predicate_definition.predicate_type_desc = N'BLOCK'
                                 AND predicate_definition.operation_desc = N'AFTER UPDATE' THEN 1 ELSE 0 END) = 1
                   AND OBJECT_ID(N'Security.{SqlServerAccountPredicateFunction}', N'IF') IS NOT NULL
                   AND OBJECT_ID(N'Security.{SqlServerRegisterPredicateFunction}', N'IF') IS NOT NULL
            ) AS verified_table;";

        private sealed record ProtectedTablePolicy(
            string TableName,
            string PostgreSqlPredicateFunction,
            string SqlServerPredicateFunction,
            string ColumnName)
        {
            public string GetPredicateFunctionName(DatabaseEngine engine) => engine switch
            {
                DatabaseEngine.PgSql => PostgreSqlPredicateFunction,
                DatabaseEngine.MsSql => SqlServerPredicateFunction,
                _ => throw new InvalidOperationException(
                    $"Native row-level security is enabled without a predicate mapping for database engine '{engine}'.")
            };
        }
    }
}
