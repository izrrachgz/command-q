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
    public sealed class CacheMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<CacheMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            await next(context);

            // Only apply cache headers for safe idempotent requests and successful responses
            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            {
                logger.LogDebug(@"Response cache headers skipped for method {Method}.", context.Request.Method);
                return;
            }

            if (context.Response.StatusCode != StatusCodes.Status200OK)
            {
                logger.LogDebug(@"Response cache headers skipped for status code {StatusCode}.", context.Response.StatusCode);
                return;
            }

            // Do not add cache headers if response sets cookies (sensitive) or already has cache-control set
            if (context.Response.Headers.ContainsKey(HeaderNames.SetCookie))
            {
                logger.LogDebug(@"Response cache headers skipped because response sets cookies.");
                return;
            }

            if (context.Response.Headers.ContainsKey(HeaderNames.CacheControl))
            {
                logger.LogDebug(@"Response cache headers skipped because cache-control already exists.");
                return;
            }

            int maxAge = configuration.GetValue(ConfigKeys.Cache.MaxAgeSeconds, 10);

            context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromSeconds(maxAge)
            };

            context.Response.Headers[HeaderNames.Vary] = new[] { HeaderNames.AcceptEncoding };

            logger.LogDebug(@"Response cache headers applied. MaxAgeSeconds: {MaxAgeSeconds}.", maxAge);
        }
    }
}
