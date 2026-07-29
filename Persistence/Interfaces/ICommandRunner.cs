using System.Data;
using Common.Models.Command;
using Common.Models.Response;
using Microsoft.Extensions.Logging;

namespace Persistence.Interfaces
{
    /// <summary>
    /// Executes raw database commands and engine-specific bulk insert operations.
    /// </summary>
    public interface ICommandRunner
    {
        /// <summary>
        /// Logger associated with the concrete runner.
        /// </summary>
        ILogger Logger { get; set; }

        /// <summary>
        /// Opens a database connection for the runner's configured connection string.
        /// </summary>
        /// <param name="cancellationToken">Token used while opening the connection.</param>
        Task<IDbConnection> GetConnection(CancellationToken cancellationToken);

        /// <summary>
        /// Executes multiple commands and returns each command result set.
        /// </summary>
        /// <param name="commands">Commands to execute.</param>
        /// <param name="cancellationToken">Token used while executing the command batch.</param>
        Task<List<List<object>>> ExecuteCommands(List<QueryCommand> commands, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a command and returns the result set parsed as the requested response type.
        /// </summary>
        /// <param name="query">Command statement.</param>
        /// <param name="type">Type of expected response.</param>
        /// <param name="parameters">Set of objects to use as params, key is the name and value is the used value.</param>
        /// <param name="cancellationToken">Token used while executing the command.</param>
        Task<List<object>> Execute(string query, Type type, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts rows using the most efficient bulk operation available for the concrete database engine.
        /// </summary>
        /// <param name="table">Target table.</param>
        /// <param name="columns">Columns to insert, not alias and in order.</param>
        /// <param name="rows">Row data where each key is the associated column name.</param>
        /// <param name="cancellationToken">Token used while inserting rows.</param>
        Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default);
    }
}
