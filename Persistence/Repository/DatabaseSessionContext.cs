using System.Data;
using System.Data.Common;
using System.Globalization;
using Common.Enumerations;
using Persistence.Abstractions;
using Persistence.Extensions;

namespace Persistence.Repository
{
    internal static class DatabaseSessionContext
    {
        public static async Task ApplyAsync(
            IDbConnection connection,
            DatabaseEngine engine,
            IReadOnlyCollection<DatabaseSessionValue> values,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(connection);
            ArgumentNullException.ThrowIfNull(values);

            if (values.Count == 0 || !engine.SupportsNativeRowLevelSecurity())
                return;

            if (connection is not DbConnection dbConnection)
                throw new InvalidOperationException($"Database engine '{engine}' does not expose an asynchronous database connection.");

            if (dbConnection.State != ConnectionState.Open)
                await dbConnection.OpenAsync(cancellationToken);

            foreach (DatabaseSessionValue value in values)
            {
                await using DbCommand command = dbConnection.CreateCommand();
                ConfigureCommand(command, engine, value);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        private static void ConfigureCommand(DbCommand command, DatabaseEngine engine, DatabaseSessionValue value)
        {
            switch (engine)
            {
                case DatabaseEngine.PgSql:
                    command.CommandText = "SELECT set_config(@key, @value, false);";
                    AddParameter(command, "@key", value.Key);
                    AddParameter(command, "@value", ToPostgresValue(value.Value));
                    break;
                case DatabaseEngine.MsSql:
                    command.CommandText = "EXEC sys.sp_set_session_context @key = @key, @value = @value;";
                    AddParameter(command, "@key", value.Key);
                    AddParameter(command, "@value", value.Value);
                    break;
                default:
                    throw new NotSupportedException($"Database session context is not supported for engine '{engine}'.");
            }
        }

        private static void AddParameter(DbCommand command, string name, object? value)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        private static string ToPostgresValue(object? value)
        {
            return value switch
            {
                null => string.Empty,
                bool boolean => boolean ? "true" : "false",
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty
            };
        }
    }
}
