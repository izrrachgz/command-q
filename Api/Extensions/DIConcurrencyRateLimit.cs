using Api.Models;
using Api.Constants;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Extensions
{
    public static class DIConcurrencyRateLimit
    {
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
