using System.Net;
using System.Reflection;
using System.Security.Claims;
using IdentityServer.Constants;
using IdentityServer.Extensions;
using IdentityServer.Modules.Accounts;
using IdentityServer.Modules.OpenIddict;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace IdentityServer.Test.Extensions
{
    public sealed class IdentityServerRateLimitingExtensionsTest
    {
        [Fact]
        public void GetPartitionKey_Should_Use_Authenticated_Subject()
        {
            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim("iss", "https://identity.test/"),
                    new Claim("sub", "subject-1")
                ], "Bearer"))
            };
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            string key = IdentityServerRateLimitingExtensions.GetPartitionKey(context);

            Assert.Equal("identity:https://identity.test/:subject-1", key);
        }

        [Fact]
        public void GetPartitionKey_Should_Fall_Back_To_Remote_Address()
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.30");

            string key = IdentityServerRateLimitingExtensions.GetPartitionKey(context);

            Assert.Equal("ip:192.0.2.30", key);
        }

        [Fact]
        public void Administration_Should_Use_Its_Sliding_Window_Policy()
        {
            EnableRateLimitingAttribute? attribute = typeof(IdentityAdminController)
                .GetCustomAttribute<EnableRateLimitingAttribute>();

            Assert.NotNull(attribute);
            Assert.Equal(IdentityServerRateLimitPolicies.Administration, attribute.PolicyName);
        }

        [Fact]
        public void Token_Issuance_Should_Use_Authentication_Policy()
        {
            MethodInfo? method = typeof(AuthorizationController).GetMethod(nameof(AuthorizationController.Exchange));
            EnableRateLimitingAttribute? attribute = method?.GetCustomAttribute<EnableRateLimitingAttribute>();

            Assert.NotNull(attribute);
            Assert.Equal(IdentityServerRateLimitPolicies.Authentication, attribute.PolicyName);
        }

        [Fact]
        public void Default_Should_Not_Replace_An_Explicit_Policy()
        {
            var conventionBuilder = new CapturingEndpointConventionBuilder();
            var endpointBuilder = new RouteEndpointBuilder(_ => Task.CompletedTask, RoutePatternFactory.Parse("/test"), 0);
            endpointBuilder.Metadata.Add(new EnableRateLimitingAttribute(IdentityServerRateLimitPolicies.Challenges));

            conventionBuilder.AddDefaultIdentityServerRateLimit();
            conventionBuilder.ApplyTo(endpointBuilder);

            EnableRateLimitingAttribute policy = Assert.Single(endpointBuilder.Metadata.OfType<EnableRateLimitingAttribute>());
            Assert.Equal(IdentityServerRateLimitPolicies.Challenges, policy.PolicyName);
        }

        private sealed class CapturingEndpointConventionBuilder : IEndpointConventionBuilder
        {
            private readonly List<Action<EndpointBuilder>> conventions = [];

            public void Add(Action<EndpointBuilder> convention) => conventions.Add(convention);

            public void ApplyTo(EndpointBuilder endpoint)
            {
                foreach (Action<EndpointBuilder> convention in conventions)
                {
                    convention(endpoint);
                }
            }
        }
    }
}
