using Common.Models.Command;
using Microsoft.Extensions.Logging.Abstractions;
using Persistence.Repository.Command;

namespace Persistence.Test;

public sealed class CommandRunnerCancellationTest
{
    private const string ConnectionString = "Data Source=:memory:;Version=3;";

    [Fact]
    public async Task Execute_Should_Propagate_Caller_Cancellation()
    {
        var runner = new SqLiteCommandRunner(ConnectionString, NullLogger<SqLiteCommandRunner>.Instance);
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runner.Execute(
            "SELECT 1",
            typeof(int),
            parameters: null,
            cancellationToken: cancellationTokenSource.Token));
    }

    [Fact]
    public async Task ExecuteCommands_Should_Propagate_Batch_Cancellation()
    {
        var runner = new SqLiteCommandRunner(ConnectionString, NullLogger<SqLiteCommandRunner>.Instance);
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var command = new QueryCommand
        {
            Query = "SELECT 1",
            ReturnType = typeof(int)
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runner.ExecuteCommands(
            [command],
            cancellationTokenSource.Token));
    }

    [Fact]
    public async Task ExecuteCommands_Should_Propagate_Command_Cancellation()
    {
        var runner = new SqLiteCommandRunner(ConnectionString, NullLogger<SqLiteCommandRunner>.Instance);
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var command = new QueryCommand
        {
            Query = "SELECT 1",
            ReturnType = typeof(int),
            CancellationToken = cancellationTokenSource.Token
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runner.ExecuteCommands(
            [command],
            CancellationToken.None));
    }
}
