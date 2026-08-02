using Api.Models;
using Api.Services;
using Api.Constants;
using Api.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Api.Extensions
{
    internal static class AuthenticationExtensions
    {
        public static void AddResourceServerAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<BearerAuthenticationOptions>()
                .Bind(configuration.GetRequiredSection(ConfigKeys.Authentication.Section))
                .Validate(options =>
                    Uri.TryCreate(options.Authority, UriKind.Absolute, out var authority) &&
                    string.Equals(authority.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase),
                    "Authentication:Authority must be an absolute HTTPS URI.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Authentication:Audience is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.ApiScope), "Authentication:ApiScope is required.")
                .Validate(options => options.ClockSkewSeconds >= 0, "Authentication:ClockSkewSeconds cannot be negative.")
                .ValidateOnStart();

            var settings = configuration
                .GetRequiredSection(ConfigKeys.Authentication.Section)
                .Get<BearerAuthenticationOptions>()
                ?? throw new InvalidOperationException("Authentication configuration is required.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = settings.Authority;
                    options.Audience = settings.Audience;
                    options.MapInboundClaims = false;
                    options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
                    options.SaveToken = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudience = settings.Audience,
                        ClockSkew = TimeSpan.FromSeconds(settings.ClockSkewSeconds),
                        NameClaimType = AuthenticationClaimTypes.Name,
                        RoleClaimType = AuthenticationClaimTypes.Role,
                        ValidTypes = [AuthenticationClaimTypes.AccessTokenType]
                    };
                });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context => HasScope(context.User, settings.ApiScope))
                    .Build();
            });

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentAccountAccessor, CurrentAccountAccessor>();
        }

        private static bool HasScope(System.Security.Claims.ClaimsPrincipal user, string requiredScope)
        {
            return user.FindAll(AuthenticationClaimTypes.Scope)
                .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Contains(requiredScope, StringComparer.Ordinal);
        }
    }
}
