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
    public static class DIJwtExtensions
    {
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
            services.AddScoped<PasswordHasher>();

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
                    ClockSkew = TimeSpan.FromSeconds(jwt.AccessTokenMinutes),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                    NameClaimType = JwtRegisteredClaimNames.UniqueName,
                    RoleClaimType = ClaimTypes.Role
                };
            });

            services.AddAuthorization();
        }
    }
}
