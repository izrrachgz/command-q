using Dapper;
using System.Data;
using Domain.Interfaces;
using Persistence.Constant;
using Common.Models.Command;
using Common.Models.Response;
using Microsoft.Data.SqlClient;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    public class MsSqlCommandRunner : ICommandRunner
    {
        private string ConnectionString { get; }

        public MsSqlCommandRunner(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public MsSqlCommandRunner(IConfiguration configuration) : this(configuration.GetDatabaseConnection())
        { }

        public async Task<IDbConnection> GetConnection(CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(ConnectionString);

            await connection.OpenAsync(cancellationToken);

            return connection;
        }

        public async Task<List<Task<List<object>>>> RunInParallel(List<QueryCommand> commands, CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task<List<object>>>(commands.Count);

            foreach (var command in commands)
            {
                tasks.Add(Execute(command.Query, command.ReturnType, command.Parameters, command.CancellationToken));
            }

            await Parallel.ForEachAsync(tasks, cancellationToken, async (t, token) => { await t.WaitAsync(token); });

            return tasks;
        }

        public async Task<List<object>> Execute(string query, Type type, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default)
        {
            List<object> results = [];

            var connection = await GetConnection(cancellationToken);

            using var cmd = (SqlCommand)connection.CreateCommand();

            cmd.CommandText = query;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    object value = parameter.Value is null ? DBNull.Value : parameter.Value;
                    cmd.Parameters.AddWithValue(parameter.Key, value);
                }
            }

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (reader.HasRows)
                results = [.. reader.Parse(type)];

            return results;
        }

        public async Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
            using var connection = (SqlConnection)await GetConnection(cancellationToken);

            var data = new DataTable();

            try
            {
                using var cmd = connection.CreateCommand();

                cmd.CommandText = $@"SELECT TOP 0 * FROM {table}";

                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

                data.Load(reader);

                foreach (var row in rows)
                {
                    var r = data.NewRow();
                    foreach (var column in columns)
                    {
                        r[column] = row[column] ?? DBNull.Value;
                    }

                    data.Rows.Add(r);
                }
                await cmd.DisposeAsync();
                await reader.DisposeAsync();
            }
            catch
            {
                await connection.DisposeAsync();

                data.Dispose();

                return new(false, CommandConstants.CannotReadSchema);
            }

            using var transaction = connection.BeginTransaction();
            try
            {
                using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);

                bulkCopy.DestinationTableName = table;

                await bulkCopy.WriteToServerAsync(data, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                bulkCopy.Close();

                return new(true, CommandConstants.CopyCompleted);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);

                return new(false, CommandConstants.ErrorDuringCopy);
            }
            finally
            {
                await transaction.DisposeAsync();

                await connection.DisposeAsync();

                data.Dispose();
            }
        }
    }
}
