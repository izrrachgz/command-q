using IdentityServer.Modules.Accounts.Configuration;
using IdentityServer.Modules.Accounts.Abstractions.Email;
using IdentityServer.Modules.Accounts.Logging;
using Microsoft.Extensions.Options;
using Time.Extensions;

namespace IdentityServer.Modules.Accounts.Workers.Email
{
    /// <summary>
    /// Continuously relays staged account emails to the Email API and periodically cleans up retained account-security data.
    /// </summary>
    /// <remarks>
    /// The ASP.NET Core host starts this worker automatically because the Accounts module registers it with
    /// <c>AddHostedService&lt;AccountEmailMaintenanceWorker&gt;()</c>. Each polling cycle creates a dependency-injection scope,
    /// processes a configured batch of due dispatches, and waits for the configured polling interval. Cleanup runs immediately
    /// after startup and then at the configured cleanup interval. The worker queues email; it never sends SMTP directly.
    /// </remarks>
    internal sealed class AccountEmailMaintenanceWorker(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        IOptions<AccountEmailOptions> options,
        ILogger<AccountEmailMaintenanceWorker> logger) : BackgroundService
    {
        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var nextCleanupUtc = timeProvider.GetCurrentUtcDateTime();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var maintenance = scope.ServiceProvider.GetRequiredService<IAccountEmailMaintenanceService>();
                    await maintenance.ProcessPending(stoppingToken);

                    if (timeProvider.GetCurrentUtcDateTime() >= nextCleanupUtc)
                    {
                        await maintenance.Cleanup(stoppingToken);
                        nextCleanupUtc = timeProvider.GetCurrentUtcDateTime()
                            .AddMinutes(options.Value.CleanupIntervalMinutes);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.AccountEmailMaintenanceFailed(exception);
                }

                await Task.Delay(TimeSpan.FromSeconds(options.Value.DispatchPollSeconds), timeProvider, stoppingToken);
            }
        }
    }
}
