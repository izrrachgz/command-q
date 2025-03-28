using System.Data;
using Common.Models.Command;
using Common.Models.Response;

namespace Domain.Interfaces
{
    /// <summary>
    /// Should provide a way to run commands against a database engine
    /// </summary>
    public interface ICommandRunner
    {
        /// <summary>
        /// Should provide a connection with the database
        /// </summary>
        /// <returns></returns>
        IDbConnection GetConnection();

        /// <summary>
        /// Should run task list in parallel
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        Task<List<Task<List<object>>>> RunInParallel(List<QueryCommand> commands);

        /// <summary>
        /// Should execute the command and resturn the set of results
        /// </summary>
        /// <param name="query">Command statement</param>
        /// <param name="type">Type of expected response</param>
        /// <param name="parameters">Set of objects to use as params, key is the name and value is the used value</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<object>> Execute(string query, Type type, Dictionary<string, object?>? parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Should copy in the most optimized way possible the rows into the table
        /// </summary>
        /// <param name="table">Target table</param>
        /// <param name="columns">Columns to insert, not alias.</param>
        /// <param name="rows">Row data, key is the asociated column name, value is used value</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<BasicResponse> BulkInsert(string table, string[] columns, Dictionary<string, object?>[] rows, CancellationToken cancellationToken = default);
    }
}
