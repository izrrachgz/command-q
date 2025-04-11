using System.Text;
using Search.Constants;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Search.Extensions
{
    public static class DIJwtExtensions
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            string jwtSecretKey = configuration.GetValue<string>(SearchConfiguration.JwtSecretKey) ?? string.Empty;

            if (string.IsNullOrEmpty(jwtSecretKey))
            {
                throw new Exception(SearchMessages.NotValidJwtSecretKey);
            }

            bool isProduction = environment.IsProduction();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = isProduction;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = isProduction,
                    ValidateAudience = isProduction,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
                };
            });
        }
    }
}
