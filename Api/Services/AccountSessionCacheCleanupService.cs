using Api.Models;
using Api.Interfaces;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    /// <summary>
    /// Periodically clears cached account/session data based on authentication settings.
    /// </summary>
    public sealed class AccountSessionCacheCleanupService(IAccountSessionCache cache, IOptions<AuthOptions> options, ILogger<AccountSessionCacheCleanupService> logger) : BackgroundService
    {
        private readonly AuthOptions _options = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var period = TimeSpan.FromMinutes(_options.RefreshCacheCleanupMinutes);

            using var timer = new PeriodicTimer(period);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                cache.Clear();
                logger.LogDebug(@"Account session cache cleared.");
            }
        }
    }
}
