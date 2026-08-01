using Api.Middleware;
using Microsoft.AspNetCore.Http;

namespace Api.Test.Modules.Common.Middleware
{
    public sealed class SecurityHeadersMiddlewareTest
    {
        [Fact]
        public async Task Invoke_Should_Add_Common_Security_Headers()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var middleware = new SecurityHeadersMiddleware(async httpContext => await httpContext.Response.WriteAsync("ok", TestContext.Current.CancellationToken));

            await middleware.Invoke(context);

            Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
            Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"]);
            Assert.Equal("no-referrer", context.Response.Headers["Referrer-Policy"]);
            Assert.Equal("none", context.Response.Headers["X-Permitted-Cross-Domain-Policies"]);
        }

        [Fact]
        public async Task Invoke_Should_Add_Hsts_For_Https_Requests()
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Response.Body = new MemoryStream();

            var middleware = new SecurityHeadersMiddleware(async httpContext => await httpContext.Response.WriteAsync("ok", TestContext.Current.CancellationToken));

            await middleware.Invoke(context);

            Assert.Equal("max-age=31536000; includeSubDomains", context.Response.Headers["Strict-Transport-Security"]);
        }
    }
}
