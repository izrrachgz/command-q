using Api.Constants;
using Microsoft.Net.Http.Headers;

namespace Api.Middleware
{
    internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            var headers = context.Response.Headers;

            headers.TryAdd(HeaderNames.XContentTypeOptions, SecurityHeaderValues.NoSniff);
            headers.TryAdd(HeaderNames.XFrameOptions, SecurityHeaderValues.Deny);
            headers.TryAdd(SecurityHeaderNames.ReferrerPolicy, SecurityHeaderValues.NoReferrer);
            headers.TryAdd(SecurityHeaderNames.XPermittedCrossDomainPolicies, SecurityHeaderValues.None);

            if (context.Request.IsHttps)
            {
                headers.TryAdd(HeaderNames.StrictTransportSecurity, SecurityHeaderValues.HstsOneYearIncludeSubDomains);
            }

            await next(context);
        }
    }
}
