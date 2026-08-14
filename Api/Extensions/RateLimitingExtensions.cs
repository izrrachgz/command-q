using Api.Constants;
using Api.Configuration;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Extensions
{
    public static class RateLimitingExtensions
    {
        /// <summary>
        /// Configures every supported rate limiting policy.
        /// </summary>
        public static void AddRateLimitingPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddFixedWindowRateLimit(configuration);
            services.AddSlidingWindowRateLimit(configuration);
            services.AddTokenBucketRateLimit(configuration);
            services.AddConcurrencyRateLimit(configuration);
        }

        /// <summary>
        /// Configures a fixed window rate limiting policy for the application using the provided service collection.
        /// </summary>
        /// <remarks>This method registers a rate limiting policy named "fixed" that allows up to 4
        /// requests per 12-second window, with a queue limit of 2 requests. Requests exceeding the limit are queued and
        /// processed in the order they were received.</remarks>
        /// <param name="services">The service collection to which the rate limiter services are added. Must not be null.</param>
        /// <param name="configuration">The configuration from which to retrieve rate limiting options. Must not be null.</param>
        public static void AddFixedWindowRateLimit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRateLimiter((RateLimiterOptions configureOptions) =>
                configureOptions.AddPolicy(ApiConfigKeys.RateLimiting.FixedPolicyName, context =>
                {
                    var rateOptions = context.RequestServices
                        .GetRequiredService<IOptionsMonitor<FixedRateLimitingOptions>>().CurrentValue;

                    string partitionKey = $"{GetPartitionKey(context)}:{rateOptions.PermitLimit}:{rateOptions.WindowSeconds}:{rateOptions.QueueLimit}";

                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateOptions.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateOptions.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateOptions.QueueLimit
                    });
                })
            );
        }

        /// <summary>
        /// Configures a sliding window rate limiter for the specified service collection, enabling rate limiting for
        /// incoming requests based on defined limits.
        /// </summary>
        /// <remarks>The configured rate limiter allows up to 4 requests within a 12-second window,
        /// divided into 4 segments, and supports queuing up to 2 additional requests. Adjust these settings as needed
        /// to match the application's performance and concurrency requirements.</remarks>
        /// <param name="services">The service collection to which the sliding window rate limiter will be added. This parameter cannot be
        /// null.</param>
        /// <param name="configuration">The configuration from which to retrieve the sliding window rate limiting options. This parameter cannot be
        public static void AddSlidingWindowRateLimit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRateLimiter((RateLimiterOptions configureOptions) =>
                configureOptions.AddPolicy(ApiConfigKeys.RateLimiting.SlidingPolicyName, context =>
                {
                    var rateOptions = context.RequestServices
                        .GetRequiredService<IOptionsMonitor<SlidingRateLimitingOptions>>().CurrentValue;

                    string partitionKey = $"{GetPartitionKey(context)}:{rateOptions.PermitLimit}:{rateOptions.WindowSeconds}:{rateOptions.QueueLimit}:{rateOptions.SegmentsPerWindow}";

                    return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = rateOptions.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateOptions.WindowSeconds),
                        SegmentsPerWindow = rateOptions.SegmentsPerWindow,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateOptions.QueueLimit
                    });
                })
            );
        }

        /// <summary>
        /// Configures a token bucket rate limiter for the specified service collection, enabling controlled request
        /// processing based on token availability.
        /// </summary>
        /// <remarks>This method sets up a token bucket limiter with a token limit of 4, a queue
        /// processing order of oldest first, a queue limit of 2, a replenishment period of 12 seconds, and 4 tokens
        /// replenished per period. This configuration helps manage the rate of incoming requests effectively.</remarks>
        /// <param name="services">The service collection to which the rate limiter is added. This parameter must not be null.</param>
        /// <param name="configuration">The configuration from which to retrieve rate limiting options. This parameter must not be null.</param>
        public static void AddTokenBucketRateLimit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRateLimiter((RateLimiterOptions configureOptions) =>
                configureOptions.AddPolicy(ApiConfigKeys.RateLimiting.TokenBucketPolicyName, context =>
                {
                    var rateOptions = context.RequestServices
                        .GetRequiredService<IOptionsMonitor<TokenBucketRateLimitingOptions>>().CurrentValue;

                    string partitionKey = $"{GetPartitionKey(context)}:{rateOptions.TokenLimit}:{rateOptions.WindowSeconds}:{rateOptions.QueueLimit}:{rateOptions.TokensPerPeriod}";

                    return RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = rateOptions.TokenLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateOptions.QueueLimit,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(rateOptions.WindowSeconds),
                        TokensPerPeriod = rateOptions.TokensPerPeriod,
                        AutoReplenishment = true
                    });
                })
            );
        }

        /// <summary>
        /// Configures a concurrency rate limiter for the specified service collection, enabling control over the number
        /// of concurrent requests processed by the application.
        /// </summary>
        /// <remarks>The configured rate limiter allows up to four concurrent requests and queues up to
        /// two additional requests, processing them in the order received. This helps prevent resource exhaustion in
        /// high-concurrency scenarios and can be useful for protecting downstream services or shared
        /// resources.</remarks>
        /// <param name="services">The service collection to which the concurrency rate limiter is added. This parameter must not be null.</param>
        /// <param name="configuration">The configuration from which to retrieve rate limiting options. This parameter must not be null.</param>
        public static void AddConcurrencyRateLimit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRateLimiter((RateLimiterOptions configureOptions) =>
                configureOptions.AddPolicy(ApiConfigKeys.RateLimiting.ConcurrencyPolicyName, context =>
                {
                    var rateOptions = context.RequestServices
                        .GetRequiredService<IOptionsMonitor<ConcurrencyRateLimitingOptions>>().CurrentValue;

                    string partitionKey = $"{GetPartitionKey(context)}:{rateOptions.PermitLimit}:{rateOptions.QueueLimit}";

                    return RateLimitPartition.GetConcurrencyLimiter(partitionKey, _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = rateOptions.PermitLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateOptions.QueueLimit
                    });
                })
            );
        }

        private static string GetPartitionKey(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
