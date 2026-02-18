using Api.Models;
using Api.Constants;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Extensions
{
    public static class DITokenBucketRateLimit
    {
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
    }
}