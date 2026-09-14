using Api.Constants;
using Api.Configuration;
using System.Globalization;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Net.Http.Headers;

namespace Api.Extensions
{
    public static class RateLimitingExtensions
    {
        /// <summary>
        /// Configures every supported rate limiting policy.
        /// </summary>
        public static void AddRateLimitingPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = static (context, _) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers[HeaderNames.RetryAfter] = Math.Max(
                            1,
                            (int)Math.Ceiling(retryAfter.TotalSeconds))
                            .ToString(CultureInfo.InvariantCulture);
                    }

                    return ValueTask.CompletedTask;
                };
            });

            services.AddFixedWindowRateLimit(configuration);
            services.AddSlidingWindowRateLimit(configuration);
            services.AddTokenBucketRateLimit(configuration);
            services.AddConcurrencyRateLimit(configuration);
        }

        /// <summary>
        /// Configures a fixed window rate limiting policy for the application using the provided service collection.
        /// </summary>
        /// <remarks>The configured fixed-window policy is intended for infrequent operations such as account provisioning.</remarks>
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
        /// <remarks>The configured sliding-window policy smooths administrative request bursts over the full window.</remarks>
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
        /// <remarks>The configured token-bucket policy permits short SPA request bursts while bounding sustained traffic.</remarks>
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
        /// <remarks>The configured concurrency policy is global to the API instance and protects expensive operations such as uploads.</remarks>
        /// <param name="services">The service collection to which the concurrency rate limiter is added. This parameter must not be null.</param>
        /// <param name="configuration">The configuration from which to retrieve rate limiting options. This parameter must not be null.</param>
        public static void AddConcurrencyRateLimit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRateLimiter((RateLimiterOptions configureOptions) =>
                configureOptions.AddPolicy(ApiConfigKeys.RateLimiting.ConcurrencyPolicyName, context =>
                {
                    var rateOptions = context.RequestServices
                        .GetRequiredService<IOptionsMonitor<ConcurrencyRateLimitingOptions>>().CurrentValue;

                    string partitionKey = $"global:{rateOptions.PermitLimit}:{rateOptions.QueueLimit}";

                    return RateLimitPartition.GetConcurrencyLimiter(partitionKey, _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = rateOptions.PermitLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateOptions.QueueLimit
                    });
                })
            );
        }

        internal static string GetPartitionKey(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.User.TryGetIdentityKey(out var issuer, out var subject))
            {
                return $"identity:{issuer}:{subject}";
            }

            return $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
        }
    }
}
