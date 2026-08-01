using Api.Models;
using System.Text;
using Api.Services;
using Api.Constants;
using Api.Interfaces;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Api.Extensions
{
    public static class JwtExtensions
    {
        /// <summary>
        /// Configures JWT authentication and authorization services for the application, enabling secure token-based
        /// authentication using settings from the provided configuration.
        /// </summary>
        /// <remarks>This method registers authentication and authorization services, configures token
        /// validation parameters, and ensures that JWT options are loaded from configuration. In production
        /// environments, additional validation and HTTPS requirements are enforced.</remarks>
        /// <param name="services">The service collection to which JWT authentication and related services will be added.</param>
        /// <param name="configuration">The configuration source containing JWT options, including the secret key, issuer, audience, and token
        /// lifetime settings.</param>
        /// <param name="environment">The host environment used to determine whether production-specific authentication settings should be
        /// applied.</param>
        /// <exception cref="Exception">Thrown if the JWT secret key is missing or invalid in the configuration.</exception>
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            bool isProduction = environment.IsProduction();

            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(ConfigKeys.Jwt.Section))
                .Validate(options => !string.IsNullOrWhiteSpace(options.SecretKey), "Jwt:SecretKey is required.")
                .Validate(options => Encoding.UTF8.GetByteCount(options.SecretKey) >= 32, "Jwt:SecretKey must be at least 32 bytes.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
                .Validate(options => options.MinutesToExpire > 0, "Jwt:MinutesToExpire must be greater than zero.")
                .Validate(options => options.ClockSkewMinutes >= 0, "Jwt:ClockSkewMinutes cannot be negative.")
                .Validate(options => options.RefreshTokenDays > 0, "Jwt:RefreshTokenDays must be greater than zero.")
                .ValidateOnStart();
            services.AddOptions<AuthOptions>()
                .Bind(configuration.GetSection(ConfigKeys.Auth.Section))
                .Validate(options => options.MaxFailedAttempts > 0, "Auth:MaxFailedAttempts must be greater than zero.")
                .Validate(options => options.LockoutMinutes > 0, "Auth:LockoutMinutes must be greater than zero.")
                .Validate(options => options.RefreshTokenByteSize > 0, "Auth:RefreshTokenByteSize must be greater than zero.")
                .Validate(options => options.RefreshCacheMinutes > 0, "Auth:RefreshCacheMinutes must be greater than zero.")
                .Validate(options => options.RefreshCacheCleanupMinutes > 0, "Auth:RefreshCacheCleanupMinutes must be greater than zero.")
                .ValidateOnStart();

            services.AddScoped<AuthService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();
            services.AddAccountSessionCache();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var jwt = configuration.GetSection(ConfigKeys.Jwt.Section).Get<JwtOptions>()!;

                options.RequireHttpsMetadata = isProduction;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = isProduction,
                    ValidateAudience = isProduction,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    ClockSkew = TimeSpan.FromMinutes(jwt.ClockSkewMinutes),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                    NameClaimType = JwtRegisteredClaimNames.UniqueName,
                    RoleClaimType = ClaimTypes.Role
                };
            });

            services.AddAuthorization();
        }
    }
}
