using Api.Models;
using Api.Constants;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Extensions
{
    public static class DIFixedWindowRateLimit
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
    }
}
