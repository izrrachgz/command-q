using Api.Constants;
using Api.Enumerations;

namespace Api.Extensions
{
    public static class MapControllerExtensions
    {
        /// <summary>
        /// Configures the specified endpoint convention builder to enforce rate limiting based on the provided
        /// configuration settings.
        /// </summary>
        /// <remarks>This method retrieves the rate limiting type from the configuration and applies it to
        /// the endpoint builder. Ensure that the configuration contains a valid rate limiting type to avoid unexpected
        /// behavior.</remarks>
        /// <typeparam name="TBuilder">The type of the endpoint convention builder to configure. Must implement <see
        /// cref="IEndpointConventionBuilder"/>.</typeparam>
        /// <param name="builder">The endpoint convention builder to which rate limiting will be applied.</param>
        /// <param name="configuration">The configuration source containing the rate limiting settings. Must provide a value for the rate limiting
        /// type.</param>
        /// <returns>The same endpoint convention builder instance with rate limiting configured according to the specified
        /// settings.</returns>
        public static TBuilder RequireRateLimiting<TBuilder>(this TBuilder builder, IConfiguration configuration) where TBuilder : IEndpointConventionBuilder
        {
            RateLimiting rateLimitingType = configuration.GetValue(ConfigKeys.RateLimiting.Type, RateLimiting.None);

            RequireRateLimiting(builder, rateLimitingType);

            return builder;
        }

        /// <summary>
        /// Configures the specified rate limiting policy for the endpoint convention builder.
        /// </summary>
        /// <remarks>This method allows for the application of different rate limiting strategies,
        /// including fixed window, sliding window, token bucket, and concurrency limits. The method does not apply any
        /// rate limiting if 'limiting' is set to 'None'.</remarks>
        /// <typeparam name="TBuilder">Specifies the type of the endpoint convention builder being extended.</typeparam>
        /// <param name="builder">The endpoint convention builder to which the rate limiting policy will be applied.</param>
        /// <param name="limiting">The rate limiting strategy to be applied, which determines the policy used for rate limiting.</param>
        /// <returns>The updated endpoint convention builder with the applied rate limiting policy.</returns>
        internal static TBuilder RequireRateLimiting<TBuilder>(this TBuilder builder, RateLimiting limiting) where TBuilder : IEndpointConventionBuilder
        {
            switch (limiting)
            {
                case RateLimiting.FixedWindow:
                    builder.RequireRateLimiting(ConfigKeys.RateLimiting.FixedPolicyName);
                    break;
                case RateLimiting.SlidingWindow:
                    builder.RequireRateLimiting(ConfigKeys.RateLimiting.SlidingPolicyName);
                    break;
                case RateLimiting.TokenBucket:
                    builder.RequireRateLimiting(ConfigKeys.RateLimiting.TokenBucketPolicyName);
                    break;
                case RateLimiting.Concurrent:
                    builder.RequireRateLimiting(ConfigKeys.RateLimiting.ConcurrencyPolicyName);
                    break;
                case RateLimiting.None:
                default:
                    builder.DisableRateLimiting();
                    break;
            }

            return builder;
        }
    }
}
