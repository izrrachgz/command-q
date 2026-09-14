using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using IdentityServer.Configuration;
using IdentityServer.Constants;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OpenIddict.Abstractions;

namespace IdentityServer.Extensions
{
    /// <summary>Registers and applies workload-specific IdentityServer rate-limit policies.</summary>
    internal static class IdentityServerRateLimitingExtensions
    {
        /// <summary>Registers validated, reloadable IdentityServer rate-limit policies.</summary>
        public static IServiceCollection AddIdentityServerRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetRequiredSection(IdentityServerConfigKeys.RateLimiting.Section);
            services.AddOptions<IdentityServerRateLimitingOptions>()
                .Bind(section)
                .Validate(
                    ValidateOptions,
                    "IdentityServer rate-limit values must be positive, except queue limits which cannot be negative.")
                .ValidateOnStart();

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = AddRetryAfterHeader;

                options.AddPolicy(IdentityServerRateLimitPolicies.Authentication, context =>
                {
                    FixedWindowRateLimitOptions current = context.RequestServices
                        .GetRequiredService<IOptionsMonitor<IdentityServerRateLimitingOptions>>().CurrentValue.Authentication;
                    string key = $"{GetPartitionKey(context)}:{current.PermitLimit}:{current.WindowSeconds}:{current.QueueLimit}";
                    return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = current.PermitLimit,
                        Window = TimeSpan.FromSeconds(current.WindowSeconds),
                        QueueLimit = current.QueueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
                });

                options.AddPolicy(IdentityServerRateLimitPolicies.Challenges, context =>
                {
                    SlidingWindowRateLimitOptions current = context.RequestServices
                        .GetRequiredService<IOptionsMonitor<IdentityServerRateLimitingOptions>>().CurrentValue.Challenges;
                    string key = $"{GetPartitionKey(context)}:{current.PermitLimit}:" +
                        $"{current.WindowSeconds}:{current.SegmentsPerWindow}:{current.QueueLimit}";
                    return RateLimitPartition.GetSlidingWindowLimiter(key, _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = current.PermitLimit,
                        Window = TimeSpan.FromSeconds(current.WindowSeconds),
                        SegmentsPerWindow = current.SegmentsPerWindow,
                        QueueLimit = current.QueueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
                });

                options.AddPolicy(IdentityServerRateLimitPolicies.Default, context =>
                {
                    TokenBucketRateLimitOptions current = context.RequestServices
                        .GetRequiredService<IOptionsMonitor<IdentityServerRateLimitingOptions>>().CurrentValue.Default;
                    string key = $"{GetPartitionKey(context)}:{current.TokenLimit}:" +
                        $"{current.TokensPerPeriod}:{current.ReplenishmentPeriodSeconds}:{current.QueueLimit}";
                    return RateLimitPartition.GetTokenBucketLimiter(key, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = current.TokenLimit,
                        TokensPerPeriod = current.TokensPerPeriod,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(current.ReplenishmentPeriodSeconds),
                        QueueLimit = current.QueueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
                });

                options.AddPolicy(IdentityServerRateLimitPolicies.Administration, context =>
                {
                    SlidingWindowRateLimitOptions current = context.RequestServices
                        .GetRequiredService<IOptionsMonitor<IdentityServerRateLimitingOptions>>().CurrentValue.Administration;
                    string key = $"{GetPartitionKey(context)}:{current.PermitLimit}:" +
                        $"{current.WindowSeconds}:{current.SegmentsPerWindow}:{current.QueueLimit}";
                    return RateLimitPartition.GetSlidingWindowLimiter(key, _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = current.PermitLimit,
                        Window = TimeSpan.FromSeconds(current.WindowSeconds),
                        SegmentsPerWindow = current.SegmentsPerWindow,
                        QueueLimit = current.QueueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
                });
            });

            return services;
        }

        /// <summary>Adds the default policy only when an endpoint does not already select or disable a policy.</summary>
        public static void AddDefaultIdentityServerRateLimit(this IEndpointConventionBuilder builder)
        {
            builder.Add(endpointBuilder =>
            {
                if (endpointBuilder.Metadata.OfType<EnableRateLimitingAttribute>().Any() ||
                    endpointBuilder.Metadata.OfType<DisableRateLimitingAttribute>().Any())
                {
                    return;
                }

                endpointBuilder.Metadata.Add(new EnableRateLimitingAttribute(IdentityServerRateLimitPolicies.Default));
            });
        }

        /// <summary>Builds a stable identity partition and falls back to the remote address before authentication.</summary>
        internal static string GetPartitionKey(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            string? subject = context.User.FindFirstValue(OpenIddictConstants.Claims.Subject) ??
                context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (context.User.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(subject))
            {
                string issuer = context.User.FindFirstValue(OpenIddictConstants.Claims.Issuer) ??
                    context.User.Identity.AuthenticationType ?? "local";
                return $"identity:{issuer}:{subject}";
            }

            return $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
        }

        private static bool ValidateOptions(IdentityServerRateLimitingOptions options) =>
            Validate(options.Authentication) &&
            Validate(options.Challenges) &&
            Validate(options.Default) &&
            Validate(options.Administration);

        private static bool Validate(FixedWindowRateLimitOptions options) =>
            options.PermitLimit > 0 && options.WindowSeconds > 0 && options.QueueLimit >= 0;

        private static bool Validate(SlidingWindowRateLimitOptions options) =>
            options.PermitLimit > 0 && options.WindowSeconds > 0 && options.SegmentsPerWindow > 0 && options.QueueLimit >= 0;

        private static bool Validate(TokenBucketRateLimitOptions options) =>
            options.TokenLimit > 0 && options.TokensPerPeriod > 0 && options.ReplenishmentPeriodSeconds > 0 && options.QueueLimit >= 0;

        private static ValueTask AddRetryAfterHeader(OnRejectedContext context, CancellationToken cancellationToken)
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
            {
                context.HttpContext.Response.Headers[HeaderNames.RetryAfter] = Math.Max(
                    1,
                    (int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
            }

            return ValueTask.CompletedTask;
        }
    }
}
