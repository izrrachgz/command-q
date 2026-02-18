using Api.Models;
using Api.Constants;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Extensions
{
    public static class DISlidingWindowRateLimit
    {
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
    }
}
