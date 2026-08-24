using Api.Constants;
using Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Api.Test.Modules.Common.Middleware
{
    public sealed class SecurityHeadersMiddlewareTest
    {
        [Fact]
        public async Task Invoke_Should_Add_Common_Security_Headers()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var middleware = new SecurityHeadersMiddleware(
                async httpContext => await httpContext.Response.WriteAsync("ok", TestContext.Current.CancellationToken));

            await middleware.Invoke(context);

            Assert.Equal(SecurityHeaderValues.NoSniff, context.Response.Headers[HeaderNames.XContentTypeOptions]);
            Assert.Equal(SecurityHeaderValues.Deny, context.Response.Headers[HeaderNames.XFrameOptions]);
            Assert.Equal(SecurityHeaderValues.NoReferrer, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
            Assert.Equal(SecurityHeaderValues.None, context.Response.Headers[SecurityHeaderNames.XPermittedCrossDomainPolicies]);
        }

        [Fact]
        public async Task Invoke_Should_Add_Hsts_For_Https_Requests()
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Response.Body = new MemoryStream();

            var middleware = new SecurityHeadersMiddleware(
                async httpContext => await httpContext.Response.WriteAsync("ok", TestContext.Current.CancellationToken));

            await middleware.Invoke(context);

            Assert.Equal(
                SecurityHeaderValues.HstsOneYearIncludeSubDomains,
                context.Response.Headers[HeaderNames.StrictTransportSecurity]);
        }
    }
}
