using Dapper;
using System.Data;
using Domain.Interfaces;
using System.Data.SQLite;
using Persistence.Constant;
using Application.Constants;
using Common.Models.Command;
using Common.Models.Response;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    public class SqLiteCommandRunner : ICommandRunner
    {
        private string ConnectionString { get; }

        public SqLiteCommandRunner(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString(ApplicationConfiguration.ConnectionStringKeyName) ?? string.Empty;
        }

        public IDbConnection GetConnection()
        {
            var connection = new SQLiteConnection(ConnectionString);

            connection.Open();

            return connection;
        }

        public async Task<List<Task<List<object>>>> RunInParallel(List<QueryCommand> commands)
        {
            var tasks = new List<Task<List<object>>>(commands.Count);

            foreach (var command in commands)
            {
                tasks.Add(Execute(command.Query, command.ReturnType, command.Parameters, command.CancellationToken));
            }

            await Parallel.ForEachAsync(tasks, async (t, token) => { await t.WaitAsync(token); });

            return tasks;
        }

        public async Task<List<object>> Execute(string query, Type type, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default)
        {
            List<object> results = [];

            var connection = GetConnection();

            using var cmd = (SQLiteCommand)connection.CreateCommand();

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
            using var connection = (SQLiteConnection)GetConnection();

            using var transaction = connection.BeginTransaction();
            try
            {
                using var cmd = transaction.Connection.CreateCommand();

                cmd.CommandText = $@"INSERT INTO {table} ({string.Join(",", columns)}) VALUES (@{string.Join(",@", columns)})";

                foreach (var column in columns)
                    cmd.Parameters.Add(new SQLiteParameter($@"@{column}"));

                await cmd.PrepareAsync(cancellationToken);

                foreach (var row in rows)
                {
                    foreach (var column in columns)
                        cmd.Parameters[$@"@{column}"].Value = row[column];

                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

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
            }
        }
    }
}
