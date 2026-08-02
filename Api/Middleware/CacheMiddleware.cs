using Api.Logging;
using Api.Constants;
using Microsoft.Net.Http.Headers;

namespace Api.Middleware
{
    /// <summary>
    /// Middleware that configures HTTP cache headers for successful GET and HEAD responses.
    /// </summary>
    /// <remarks>Authenticated responses and responses that set cookies are marked as no-store. Anonymous responses
    /// receive public cache headers unless the endpoint already specified its own cache policy.</remarks>
    /// <param name="next">The delegate representing the next middleware component in the HTTP request processing pipeline.</param>
    /// <param name="configuration">The configuration instance used to retrieve cache-related settings, such as the maximum age for cached
    /// responses.</param>
    public sealed class CacheMiddleware(RequestDelegate next, IConfiguration cacheConfiguration, ILogger<CacheMiddleware> logger)
    {
        private readonly int maxAgeSeconds = cacheConfiguration.GetValue<int?>(ConfigKeys.Cache.MaxAgeSeconds)
            ?? throw new InvalidOperationException($"{ConfigKeys.Cache.MaxAgeSeconds} is required.");

        public Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                ApplyCacheHeaders(context);
                return Task.CompletedTask;
            });

            return next(context);
        }

        private void ApplyCacheHeaders(HttpContext context)
        {
            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            {
                logger.ResponseCacheSkippedForMethod(context.Request.Method);
                return;
            }

            if (context.Response.StatusCode != StatusCodes.Status200OK)
            {
                logger.ResponseCacheSkippedForStatusCode(context.Response.StatusCode);
                return;
            }

            if (context.Response.Headers.ContainsKey(HeaderNames.CacheControl))
            {
                logger.ResponseCacheSkippedForExistingHeader();
                return;
            }

            if (IsAuthenticatedRequest(context))
            {
                DisableCaching(context.Response);
                logger.ResponseCacheDisabledForAuthenticatedRequest();
                return;
            }

            if (context.Response.Headers.ContainsKey(HeaderNames.SetCookie))
            {
                DisableCaching(context.Response);
                logger.ResponseCacheSkippedForCookies();
                return;
            }

            context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromSeconds(maxAgeSeconds)
            };

            context.Response.Headers.Append(HeaderNames.Vary, HeaderNames.AcceptEncoding);

            logger.ResponseCacheApplied(maxAgeSeconds);
        }

        private static bool IsAuthenticatedRequest(HttpContext context)
        {
            return context.User.Identity?.IsAuthenticated == true ||
                context.Request.Headers.ContainsKey(HeaderNames.Authorization);
        }

        private static void DisableCaching(HttpResponse response)
        {
            response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
            {
                NoStore = true
            };
        }
    }
}
