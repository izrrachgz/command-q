using System.Net;
using System.Reflection;
using System.Security.Claims;
using Api.Constants;
using Api.Enumerations;
using Api.Extensions;
using Api.Modules.Configuration;
using Api.Modules.Files;
using Api.Modules.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Test.Extensions
{
    public sealed class RateLimitingExtensionsTest
    {
        private const string IdentityIssuer = "https://identity.test/";
        private const string IdentitySubject = "d6ff56b6-d8bb-46dd-ae66-d575376fee11";

        [Fact]
        public void GetPartitionKey_Should_Partition_Authenticated_Requests_By_Identity()
        {
            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(AuthenticationClaimTypes.Issuer, IdentityIssuer),
                    new Claim(AuthenticationClaimTypes.Subject, IdentitySubject)
                ], "Bearer"))
            };
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            var key = RateLimitingExtensions.GetPartitionKey(context);

            Assert.Equal($"identity:{IdentityIssuer}:{IdentitySubject}", key);
        }

        [Fact]
        public void GetPartitionKey_Should_Fall_Back_To_Remote_Address_For_Anonymous_Requests()
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.25");

            var key = RateLimitingExtensions.GetPartitionKey(context);

            Assert.Equal("ip:192.0.2.25", key);
        }

        [Theory]
        [InlineData(typeof(AdminAccountsController))]
        [InlineData(typeof(AdminRolesController))]
        [InlineData(typeof(ConfigurationController))]
        public void Administrative_Controllers_Should_Use_Sliding_Window_Limits(Type controllerType)
        {
            var attribute = controllerType.GetCustomAttribute<EnableRateLimitingAttribute>();

            Assert.NotNull(attribute);
            Assert.Equal(ApiConfigKeys.RateLimiting.SlidingPolicyName, attribute.PolicyName);
        }

        [Fact]
        public void Account_Provisioning_Should_Use_Fixed_Window_Limits()
        {
            var method = typeof(AccountController).GetMethod(nameof(AccountController.Create));
            var attribute = method?.GetCustomAttribute<EnableRateLimitingAttribute>();

            Assert.NotNull(attribute);
            Assert.Equal(ApiConfigKeys.RateLimiting.FixedPolicyName, attribute.PolicyName);
        }

        [Fact]
        public void File_Uploads_Should_Use_Global_Concurrency_Limits()
        {
            var method = typeof(FilesController).GetMethod(nameof(FilesController.Upload));
            var attribute = method?.GetCustomAttribute<EnableRateLimitingAttribute>();

            Assert.NotNull(attribute);
            Assert.Equal(ApiConfigKeys.RateLimiting.ConcurrencyPolicyName, attribute.PolicyName);
        }

        [Fact]
        public void Global_Default_Should_Not_Replace_An_Explicit_Endpoint_Policy()
        {
            var builder = new CapturingEndpointConventionBuilder();
            var endpoint = CreateEndpointBuilder();
            endpoint.Metadata.Add(new EnableRateLimitingAttribute(ApiConfigKeys.RateLimiting.ConcurrencyPolicyName));

            builder.RequireRateLimiting(RateLimiting.TokenBucket);
            builder.ApplyTo(endpoint);

            var policies = endpoint.Metadata.OfType<EnableRateLimitingAttribute>().ToArray();
            Assert.Single(policies);
            Assert.Equal(ApiConfigKeys.RateLimiting.ConcurrencyPolicyName, policies[0].PolicyName);
        }

        [Fact]
        public void Global_Default_Should_Apply_When_An_Endpoint_Has_No_Explicit_Policy()
        {
            var builder = new CapturingEndpointConventionBuilder();
            var endpoint = CreateEndpointBuilder();

            builder.RequireRateLimiting(RateLimiting.TokenBucket);
            builder.ApplyTo(endpoint);

            var policy = Assert.Single(endpoint.Metadata.OfType<EnableRateLimitingAttribute>());
            Assert.Equal(ApiConfigKeys.RateLimiting.TokenBucketPolicyName, policy.PolicyName);
        }

        [Fact]
        public void None_Should_Disable_Explicit_Policies_For_Unthrottled_Benchmarks()
        {
            var builder = new CapturingEndpointConventionBuilder();
            var endpoint = CreateEndpointBuilder();
            endpoint.Metadata.Add(new EnableRateLimitingAttribute(ApiConfigKeys.RateLimiting.ConcurrencyPolicyName));

            builder.RequireRateLimiting(RateLimiting.None);
            builder.ApplyTo(endpoint);

            Assert.Contains(endpoint.Metadata, value => value is DisableRateLimitingAttribute);
        }

        private static RouteEndpointBuilder CreateEndpointBuilder() => new(
            _ => Task.CompletedTask,
            RoutePatternFactory.Parse("/test"),
            order: 0);

        private sealed class CapturingEndpointConventionBuilder : IEndpointConventionBuilder
        {
            private readonly List<Action<EndpointBuilder>> conventions = [];

            public void Add(Action<EndpointBuilder> convention)
            {
                conventions.Add(convention);
            }

            public void ApplyTo(EndpointBuilder endpoint)
            {
                foreach (var convention in conventions)
                {
                    convention(endpoint);
                }
            }
        }
    }
}
