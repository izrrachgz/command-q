using System.Data;
using Persistence.Abstractions;
using Persistence.Repository.Command;

namespace Persistence.Test;

public sealed class CommandRunnerContractTest
{
    [Fact]
    public void Every_Command_Runner_Should_Expose_The_Transactional_Bulk_Insert_Contract()
    {
        Type[] runners =
        [
            typeof(MsSqlCommandRunner),
            typeof(MySqlCommandRunner),
            typeof(PgSqlCommandRunner),
            typeof(SqLiteCommandRunner)
        ];

        Type[] parameters =
        [
            typeof(string),
            typeof(string[]),
            typeof(Dictionary<string, object?>[]),
            typeof(IDbTransaction),
            typeof(CancellationToken)
        ];

        Assert.NotNull(typeof(ICommandRunner).GetMethod(nameof(ICommandRunner.BulkInsert), parameters));
        Assert.All(runners, runner => Assert.NotNull(runner.GetMethod(nameof(ICommandRunner.BulkInsert), parameters)));
    }

    [Fact]
    public void MySql_Command_Runner_Should_Not_Expose_A_Local_File_Bulk_Loader()
    {
        Assert.Null(typeof(MySqlCommandRunner).GetMethod("BulkInsertLocalFile"));
    }
}
