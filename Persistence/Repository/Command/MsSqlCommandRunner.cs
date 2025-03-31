using Dapper;
using System.Data;
using Domain.Interfaces;
using Application.Constants;
using Common.Models.Command;
using Common.Models.Response;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Persistence.Repository.Command
{
    public class MsSqlCommandRunner : ICommandRunner
    {
        private string ConnectionString { get; }

        public MsSqlCommandRunner(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString(ApplicationConfiguration.ConnectionStringKeyName) ?? string.Empty;
        }

        public IDbConnection GetConnection()
        {
            var connection = new SqlConnection(ConnectionString);

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

        public Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
