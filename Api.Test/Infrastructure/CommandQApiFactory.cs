using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Api.Test.Infrastructure
{
    public sealed class CommandQApiFactory : WebApplicationFactory<Program>
    {
        public const string ApiClientHeaderName = "X-Test-Client";
        public const string ApiClientHeaderValue = "test-client-secret";
        public const string ApiClientOrigin = "https://test-client.local";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = "test-secret-key-with-at-least-32-bytes",
                    ["Jwt:MinutesToExpire"] = "15",
                    ["Jwt:Issuer"] = "test-issuer",
                    ["Jwt:Audience"] = "test-audience",
                    ["Jwt:ClockSkewMinutes"] = "1",
                    ["Jwt:RefreshTokenDays"] = "1",
                    ["Auth:MaxFailedAttempts"] = "5",
                    ["Auth:LockoutMinutes"] = "10",
                    ["Auth:RefreshTokenByteSize"] = "64",
                    ["Auth:RefreshCacheMinutes"] = "10",
                    ["Auth:RefreshCacheCleanupMinutes"] = "60",
                    ["Cookie:Name"] = "CommandQ.Test",
                    ["Cookie:DaysToExpire"] = "7",
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
                    ["ApiClients:0:Name"] = ApiClientHeaderName,
                    ["ApiClients:0:Id"] = ApiClientHeaderValue,
                    ["ApiClients:0:Uri"] = ApiClientOrigin,
                    ["HealthCheck:UrlCheckInternetAccess"] = "https://example.com/",
                    ["CacheSettings:MaxAgeSeconds"] = "10"
                });
            });
        }
    }
}
