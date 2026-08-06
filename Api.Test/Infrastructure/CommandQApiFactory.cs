using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Api.Test.Infrastructure
{
    public sealed class CommandQApiFactory : WebApplicationFactory<Program>
    {
        public const string ApiClientHeaderName = "X-Test-Client";
        public const string ApiClientHeaderValue = "test-client-secret";
        public const string ApiClientOrigin = "https://test-client.local";
        public const string IdentityIssuer = "https://identity.test/";
        public const string ApiAudience = "command_q_api";
        public const string ApiScope = "command_q_api";

        private static readonly SymmetricSecurityKey SigningKey = new(
            Encoding.UTF8.GetBytes("CommandQ-test-signing-key-with-32-bytes-minimum"));

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Authentication:Authority"] = IdentityIssuer,
                    ["Authentication:Audience"] = ApiAudience,
                    ["Authentication:ApiScope"] = ApiScope,
                    ["Authentication:RequireHttpsMetadata"] = "true",
                    ["Authentication:ClockSkewSeconds"] = "60",
                    ["RateLimiting:Type"] = "None",
                    ["RateLimiting:FixedWindow:PermitLimit"] = "4",
                    ["RateLimiting:FixedWindow:WindowSeconds"] = "16",
                    ["RateLimiting:FixedWindow:QueueLimit"] = "2",
                    ["RateLimiting:Sliding:PermitLimit"] = "100",
                    ["RateLimiting:Sliding:WindowSeconds"] = "16",
                    ["RateLimiting:Sliding:QueueLimit"] = "8",
                    ["RateLimiting:Sliding:SegmentsPerWindow"] = "4",
                    ["RateLimiting:TokenBucket:PermitLimit"] = "4",
                    ["RateLimiting:TokenBucket:WindowSeconds"] = "16",
                    ["RateLimiting:TokenBucket:QueueLimit"] = "2",
                    ["RateLimiting:TokenBucket:TokenLimit"] = "4",
                    ["RateLimiting:TokenBucket:TokensPerPeriod"] = "4",
                    ["RateLimiting:Concurrency:PermitLimit"] = "4",
                    ["RateLimiting:Concurrency:WindowSeconds"] = "16",
                    ["RateLimiting:Concurrency:QueueLimit"] = "2",
                    ["ConnectionStrings:Engine"] = "SqLite",
                    ["ConnectionStrings:Db"] = "Data Source=:memory:",
                    ["Migrations:RunOnStartup"] = "false",
                    ["ApiClients:0:Name"] = ApiClientHeaderName,
                    ["ApiClients:0:Id"] = ApiClientHeaderValue,
                    ["ApiClients:0:Uri"] = ApiClientOrigin,
                    ["HealthCheck:UrlCheckInternetAccess"] = "https://example.com/",
                    ["HealthCheck:PublishOnStartup"] = "false",
                    ["CacheSettings:MaxAgeSeconds"] = "10"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    var metadata = new OpenIdConnectConfiguration
                    {
                        Issuer = IdentityIssuer
                    };

                    metadata.SigningKeys.Add(SigningKey);
                    options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(metadata);
                    options.TokenValidationParameters.ValidIssuer = IdentityIssuer;
                    options.TokenValidationParameters.IssuerSigningKey = SigningKey;
                });
            });
        }

        public static string CreateAccessToken(
            string? scope = ApiScope,
            string audience = ApiAudience)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Name, "Test User"),
                new(JwtRegisteredClaimNames.Email, "test@example.com")
            };

            if (!string.IsNullOrWhiteSpace(scope))
            {
                claims.Add(new Claim("scope", scope));
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = IdentityIssuer,
                Audience = audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256),
                TokenType = "at+jwt"
            };

            return new JwtSecurityTokenHandler().CreateEncodedJwt(descriptor);
        }
    }
}
