using Api.Constants;
using Microsoft.Net.Http.Headers;

namespace Api.Middleware
{
    /// <summary>
    /// Middleware that adds HTTP cache headers to successful GET and HEAD responses to enable client-side caching of
    /// safe, idempotent requests.
    /// </summary>
    /// <remarks>Cache headers are only applied to responses with a 200 (OK) status code for GET and HEAD
    /// requests. If the response contains cookies or already specifies cache-control headers, no additional cache
    /// headers are added. The maximum cache duration can be configured via application settings and defaults to 10
    /// seconds if not specified.</remarks>
    /// <param name="next">The delegate representing the next middleware component in the HTTP request processing pipeline.</param>
    /// <param name="configuration">The configuration instance used to retrieve cache-related settings, such as the maximum age for cached
    /// responses.</param>
    public sealed class CacheMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        public async Task Invoke(HttpContext context)
        {
            await next(context);

            // Only apply cache headers for safe idempotent requests and successful responses
            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
                return;

            if (context.Response.StatusCode != StatusCodes.Status200OK)
                return;

            // Do not add cache headers if response sets cookies (sensitive) or already has cache-control set
            if (context.Response.Headers.ContainsKey(HeaderNames.SetCookie))
                return;

            if (context.Response.Headers.ContainsKey(HeaderNames.CacheControl))
                return;

            int maxAge = configuration.GetValue(ApiConfiguration.CacheSettingsMaxAgeSeconds, 10);

            context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromSeconds(maxAge)
            };

            context.Response.Headers[HeaderNames.Vary] = new[] { HeaderNames.AcceptEncoding };
        }
    }
}
