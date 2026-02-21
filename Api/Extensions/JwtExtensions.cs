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
        /// <param name="environment">The web host environment used to determine whether production-specific authentication settings should be
        /// applied.</param>
        /// <exception cref="Exception">Thrown if the JWT secret key is missing or invalid in the configuration.</exception>
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            string jwtSecretKey = configuration.GetValue<string>(ApiConfiguration.JwtSecretKey) ?? string.Empty;

            if (string.IsNullOrEmpty(jwtSecretKey))
            {
                throw new Exception(ApiMessages.NotValidJwtSecretKey);
            }

            bool isProduction = environment.IsProduction();

            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

            services.AddScoped<AuthService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var jwt = configuration.GetSection("Jwt").Get<JwtOptions>()!;

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
