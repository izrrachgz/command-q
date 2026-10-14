using System.Data.Common;

namespace Persistence.Repository.Command;

internal static class TransactionCleanup
{
    private static readonly TimeSpan RollbackTimeout = TimeSpan.FromSeconds(5);
    private const string RollbackExceptionKey = "CommandQ.Persistence.RollbackException";

    public static async Task RollbackPreservingOriginal(DbTransaction transaction, Exception exception)
    {
        try
        {
            using var cleanupTokenSource = new CancellationTokenSource(RollbackTimeout);
            await transaction.RollbackAsync(cleanupTokenSource.Token);
        }
        catch (Exception rollbackException)
        {
            exception.Data[RollbackExceptionKey] = rollbackException;
        }
    }
}
