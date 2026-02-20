using Api.Models;
using Api.Constants;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Extensions
{
    public static class RateLimitingExtensions
    {
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
            var rateOptions = configuration.GetSection(ApiConfiguration.RateLimitingFixedOptions).Get<RateLimitingOptions>()!;

            services.AddRateLimiter((RateLimiterOptions configureOptions) =>
                configureOptions.AddFixedWindowLimiter(policyName: ApiConfiguration.RateLimitingFixedPolicyName, options =>
                {
                    options.PermitLimit = rateOptions.PermitLimit;
                    options.Window = TimeSpan.FromSeconds(rateOptions.WindowSeconds);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = rateOptions.QueueLimit;
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
            var rateOptions = configuration.GetSection(ApiConfiguration.RateLimitingSlidingOptions).Get<SlidingRateLimitingOptions>()!;

            services.AddRateLimiter((RateLimiterOptions configureOptions) =>
                configureOptions.AddSlidingWindowLimiter(policyName: ApiConfiguration.RateLimitingSlidingPolicyName, options =>
                {
                    options.PermitLimit = rateOptions.PermitLimit;
                    options.Window = TimeSpan.FromSeconds(rateOptions.WindowSeconds);
                    options.SegmentsPerWindow = rateOptions.SegmentsPerWindow;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = rateOptions.QueueLimit;
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
            var rateOptions = configuration.GetSection(ApiConfiguration.RateLimitingTokenBucketOptions).Get<TokenBucketRateLimitingOptions>()!;

            services.AddRateLimiter((RateLimiterOptions configureOptions) =>
                configureOptions.AddTokenBucketLimiter(policyName: ApiConfiguration.RateLimitingTokenBucketPolicyName, options =>
                {
                    options.TokenLimit = rateOptions.TokenLimit;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = rateOptions.QueueLimit;
                    options.ReplenishmentPeriod = TimeSpan.FromSeconds(rateOptions.WindowSeconds);
                    options.TokensPerPeriod = rateOptions.TokensPerPeriod;
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
            var rateOptions = configuration.GetSection(ApiConfiguration.RateLimitingConcurrencyOptions).Get<RateLimitingOptions>()!;

            services.AddRateLimiter((RateLimiterOptions configureOptions) =>
                configureOptions.AddConcurrencyLimiter(policyName: ApiConfiguration.RateLimitingConcurrencyPolicyName, options =>
                {
                    options.PermitLimit = rateOptions.PermitLimit;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = rateOptions.QueueLimit;
                })
            );
        }
    }
}
