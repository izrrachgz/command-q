using Api.Models;
using Api.Middleware.Exceptions;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace Api.Middleware
{
    /// <summary>
    /// Checks if the api keys for the client are present in the request
    /// </summary>
    public sealed class CheckApiClientMiddleware(RequestDelegate next, IHostEnvironment environment, IOptions<List<ApiClient>> clients, ILogger<CheckApiClientMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            if (clients.Value is null || clients.Value.Count == 0)
            {
                logger.LogWarning(@"API client validation failed because no API clients are configured.");
                throw new NotValidApiClientException();
            }

            ApiClient? client = clients.Value.FirstOrDefault(c => context.Request.Headers.Any(h => c.Name == h.Key && c.Id == h.Value));

            if (client is null || (!environment.IsDevelopment() && !context.Request.Headers[HeaderNames.Origin].Equals(client.Uri)))
            {
                logger.LogWarning(@"API client validation failed. Origin: {Origin}.", context.Request.Headers[HeaderNames.Origin].ToString());
                throw new NotValidApiClientException();
            }

            logger.LogDebug(@"API client validation succeeded. Client: {ClientName}.", client.Name);

            await next(context);
        }
    }
}
