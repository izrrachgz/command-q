using Api.Constants;
using Api.Enumerations;
using Microsoft.AspNetCore.RateLimiting;

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
            RateLimiting rateLimitingType = configuration.GetValue(ApiConfigKeys.RateLimiting.Type, RateLimiting.None);

            RequireRateLimiting(builder, rateLimitingType);

            return builder;
        }

        /// <summary>
        /// Configures the specified rate limiting policy for the endpoint convention builder.
        /// </summary>
        /// <remarks>
        /// The selected strategy is applied only when an endpoint has not declared its own policy. Selecting
        /// <see cref="RateLimiting.None"/> disables rate limiting for the mapped controller endpoints, including explicit
        /// endpoint policies, so an unthrottled capacity benchmark can be run through configuration alone.
        /// </remarks>
        /// <typeparam name="TBuilder">Specifies the type of the endpoint convention builder being extended.</typeparam>
        /// <param name="builder">The endpoint convention builder to which the rate limiting policy will be applied.</param>
        /// <param name="limiting">The rate limiting strategy to be applied, which determines the policy used for rate limiting.</param>
        /// <returns>The updated endpoint convention builder with the applied rate limiting policy.</returns>
        internal static TBuilder RequireRateLimiting<TBuilder>(this TBuilder builder, RateLimiting limiting) where TBuilder : IEndpointConventionBuilder
        {
            if (limiting == RateLimiting.None)
            {
                builder.DisableRateLimiting();
                return builder;
            }

            var policyName = limiting switch
            {
                RateLimiting.FixedWindow => ApiConfigKeys.RateLimiting.FixedPolicyName,
                RateLimiting.SlidingWindow => ApiConfigKeys.RateLimiting.SlidingPolicyName,
                RateLimiting.TokenBucket => ApiConfigKeys.RateLimiting.TokenBucketPolicyName,
                RateLimiting.Concurrent => ApiConfigKeys.RateLimiting.ConcurrencyPolicyName,
                _ => throw new ArgumentOutOfRangeException(nameof(limiting), limiting, "Unsupported rate limiting strategy.")
            };

            builder.Add(endpointBuilder =>
            {
                if (endpointBuilder.Metadata.OfType<EnableRateLimitingAttribute>().Any() ||
                    endpointBuilder.Metadata.OfType<DisableRateLimitingAttribute>().Any())
                {
                    return;
                }

                endpointBuilder.Metadata.Add(new EnableRateLimitingAttribute(policyName));
            });

            return builder;
        }
    }
}
