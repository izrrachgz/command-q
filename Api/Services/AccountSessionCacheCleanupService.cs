using Api.Models;
using Api.Interfaces;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    /// <summary>
    /// Periodically clears cached account/session data based on authentication settings.
    /// </summary>
    public sealed class AccountSessionCacheCleanupService(IAccountSessionCache accountSessionCache, IOptions<AuthOptions> authOptions, ILogger<AccountSessionCacheCleanupService> logger) : BackgroundService
    {
        private readonly AuthOptions _authOptions = authOptions.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var period = TimeSpan.FromMinutes(_authOptions.RefreshCacheCleanupMinutes);

            using var timer = new PeriodicTimer(period);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    logger.LogDebug(@"Account session cache cleanup started.");
                    accountSessionCache.Clear();
                    logger.LogDebug(@"Account session cache cleanup completed.");
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogDebug(@"Account session cache cleanup stopped.");
            }
        }
    }
}
