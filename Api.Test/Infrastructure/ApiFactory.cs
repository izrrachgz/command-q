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
    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(
            Path.GetTempPath(),
            $"command-q-api-test-{Guid.NewGuid():N}.db");

        public const string ApiClientHeaderName = "X-Test-Client";
        public const string ApiClientHeaderValue = "test-client-secret";
        public const string ApiClientOrigin = "https://test-client.local";
        public const string IdentityIssuer = "https://identity.test/";
        public const string ApiAudience = "command-q_api";
        public const string ApiScope = "command-q_api";
        public const string AccountCreatePath = "/accounts";
        public const string AccountMePath = "/accounts/me";

        private static readonly SymmetricSecurityKey SigningKey = new(
            Encoding.UTF8.GetBytes("command-q-test-signing-key-with-32-bytes-minimum"));

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            string connectionString = $"Data Source={_databasePath};Version=3;";

            builder.UseEnvironment("Testing");
            builder.UseSetting("Api:ConnectionStrings:Engine", "SqLite");
            builder.UseSetting("Api:ConnectionStrings:Db", connectionString);
            builder.UseSetting("Api:Migrations:RunOnStartup", "true");

            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Api:Localization:DefaultCulture"] = "en-US",
                    ["Api:Authentication:Authority"] = IdentityIssuer,
                    ["Api:Authentication:Audience"] = ApiAudience,
                    ["Api:Authentication:ApiScope"] = ApiScope,
                    ["Api:Authentication:RequireHttpsMetadata"] = "true",
                    ["Api:Authentication:ClockSkewSeconds"] = "60",
                    ["Api:Cors:Enabled"] = "true",
                    ["Api:Cors:AllowedOrigins:0"] = ApiClientOrigin,
                    ["Api:RateLimiting:Type"] = "None",
                    ["Api:RateLimiting:FixedWindow:PermitLimit"] = "4",
                    ["Api:RateLimiting:FixedWindow:WindowSeconds"] = "16",
                    ["Api:RateLimiting:FixedWindow:QueueLimit"] = "2",
                    ["Api:RateLimiting:Sliding:PermitLimit"] = "100",
                    ["Api:RateLimiting:Sliding:WindowSeconds"] = "16",
                    ["Api:RateLimiting:Sliding:QueueLimit"] = "8",
                    ["Api:RateLimiting:Sliding:SegmentsPerWindow"] = "4",
                    ["Api:RateLimiting:TokenBucket:PermitLimit"] = "4",
                    ["Api:RateLimiting:TokenBucket:WindowSeconds"] = "16",
                    ["Api:RateLimiting:TokenBucket:QueueLimit"] = "2",
                    ["Api:RateLimiting:TokenBucket:TokenLimit"] = "4",
                    ["Api:RateLimiting:TokenBucket:TokensPerPeriod"] = "4",
                    ["Api:RateLimiting:Concurrency:PermitLimit"] = "4",
                    ["Api:RateLimiting:Concurrency:WindowSeconds"] = "16",
                    ["Api:RateLimiting:Concurrency:QueueLimit"] = "2",
                    ["Api:ConnectionStrings:Engine"] = "SqLite",
                    ["Api:ConnectionStrings:Db"] = connectionString,
                    ["Api:Migrations:RunOnStartup"] = "true",
                    ["Api:EmailApiClient:Enabled"] = "false",
                    ["Api:ApiClients:0:Name"] = ApiClientHeaderName,
                    ["Api:ApiClients:0:Id"] = ApiClientHeaderValue,
                    ["Api:ApiClients:0:Uri"] = ApiClientOrigin,
                    ["Api:HealthCheck:UrlCheckInternetAccess"] = "https://example.com/",
                    ["Api:HealthCheck:RunOnStartup"] = "false",
                    ["Api:HealthCheck:FailStartupOnUnhealthy"] = "false",
                    ["Api:HealthCheck:PublishReports"] = "false",
                    ["Api:CacheSettings:MaxAgeSeconds"] = "10"
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (File.Exists(_databasePath))
                File.Delete(_databasePath);
        }

        public static string CreateAccessToken(string? scope = ApiScope, string audience = ApiAudience)
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
                Expires = TimeProvider.System.GetUtcNow().UtcDateTime.AddMinutes(5),
                SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256),
                TokenType = "at+jwt"
            };

            return new JwtSecurityTokenHandler().CreateEncodedJwt(descriptor);
        }
    }
}
