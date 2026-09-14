using Api.Logging;
using Api.Configuration;
using Localization.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Modules.Health.Configuration;

namespace Api.Services
{
    /// <summary>
    /// Observes reloadable operational settings and records safe configuration-change summaries.
    /// </summary>
    internal sealed class RuntimeConfigurationChangeMonitor(
        IOptionsMonitor<CacheOptions> cacheOptions,
        IOptionsMonitor<FixedRateLimitingOptions> fixedRateOptions,
        IOptionsMonitor<SlidingRateLimitingOptions> slidingRateOptions,
        IOptionsMonitor<TokenBucketRateLimitingOptions> tokenBucketRateOptions,
        IOptionsMonitor<ConcurrencyRateLimitingOptions> concurrencyRateOptions,
        IOptionsMonitor<HealthCheckConfigurationOptions> healthOptions,
        IOptionsMonitor<FeatureFlagOptions> featureOptions,
        IOptionsMonitor<ExternalApiClientsOptions> externalApiOptions,
        IOptionsMonitor<LocalizationOptions> localizationOptions,
        ILogger<RuntimeConfigurationChangeMonitor> logger) : IHostedService
    {
        private readonly List<IDisposable?> subscriptions = [];

        /// <summary>
        /// Subscribes to all reloadable settings groups.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            subscriptions.Add(cacheOptions.OnChange(options => logger.ConfigurationReloaded("Api:CacheSettings", $"MaxAgeSeconds={options.MaxAgeSeconds}")));
            subscriptions.Add(fixedRateOptions.OnChange(options => logger.ConfigurationReloaded("Api:RateLimiting:FixedWindow", DescribeRateLimit(options))));
            subscriptions.Add(slidingRateOptions.OnChange(options => logger.ConfigurationReloaded("Api:RateLimiting:Sliding", DescribeRateLimit(options))));
            subscriptions.Add(tokenBucketRateOptions.OnChange(options => logger.ConfigurationReloaded("Api:RateLimiting:TokenBucket", DescribeRateLimit(options))));
            subscriptions.Add(concurrencyRateOptions.OnChange(options => logger.ConfigurationReloaded("Api:RateLimiting:Concurrency", DescribeRateLimit(options))));
            subscriptions.Add(healthOptions.OnChange(options => logger.ConfigurationReloaded("Api:HealthCheck", DescribeHealthChecks(options))));
            subscriptions.Add(featureOptions.OnChange(options => logger.ConfigurationReloaded("Api:Features", $"Flags={options.Values.Count}")));
            subscriptions.Add(externalApiOptions.OnChange(options => logger.ConfigurationReloaded("Api:ExternalApiClients", $"Clients={options.Clients.Count}")));
            subscriptions.Add(localizationOptions.OnChange(options => logger.ConfigurationReloaded("Api:Localization", $"DefaultCulture={options.DefaultCulture}; FallbackCulture={options.FallbackCulture}; SupportedCultures={options.SupportedCultures.Length}")));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes configuration subscriptions when the host stops.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            IDisposable?[] snapshot = [.. subscriptions];
            subscriptions.Clear();

            foreach (var subscription in snapshot)
                subscription?.Dispose();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Formats rate-limit values without including unrelated configuration data.
        /// </summary>
        private static string DescribeRateLimit(TokenBucketRateLimitingOptions options)
        {
            return $"TokenLimit={options.TokenLimit}; TokensPerPeriod={options.TokensPerPeriod}; ReplenishmentPeriodSeconds={options.WindowSeconds}; QueueLimit={options.QueueLimit}";
        }

        private static string DescribeRateLimit(ConcurrencyRateLimitingOptions options)
        {
            return $"PermitLimit={options.PermitLimit}; QueueLimit={options.QueueLimit}";
        }

        private static string DescribeRateLimit(SlidingRateLimitingOptions options)
        {
            return $"PermitLimit={options.PermitLimit}; WindowSeconds={options.WindowSeconds}; SegmentsPerWindow={options.SegmentsPerWindow}; QueueLimit={options.QueueLimit}";
        }

        private static string DescribeRateLimit(RateLimitingOptions options)
        {
            return $"PermitLimit={options.PermitLimit}; WindowSeconds={options.WindowSeconds}; QueueLimit={options.QueueLimit}";
        }

        /// <summary>
        /// Formats health-check settings without including unrelated configuration data.
        /// </summary>
        private static string DescribeHealthChecks(HealthCheckConfigurationOptions options)
        {
            return $"RunOnStartup={options.RunOnStartup}; " +
                $"FailStartupOnUnhealthy={options.FailStartupOnUnhealthy}; " +
                $"PublishReports={options.ShouldPublishReports}; " +
                $"PublishIntervalSeconds={options.PublishIntervalSeconds}";
        }
    }
}
