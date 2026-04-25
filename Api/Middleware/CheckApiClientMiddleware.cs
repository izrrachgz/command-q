using Api.Models;
using Api.Middleware.Exceptions;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace Api.Middleware
{
    /// <summary>
    /// Checks if the api keys for the client are present in the request
    /// </summary>
    public sealed class CheckApiClientMiddleware(RequestDelegate next, IHostEnvironment environment, IOptions<List<ApiClient>> clients)
    {
        public async Task Invoke(HttpContext context)
        {
            if (clients.Value is null || clients.Value.Count == 0)
            {
                throw new NotValidApiClientException();
            }

            ApiClient? client = clients.Value.FirstOrDefault(c => context.Request.Headers.Any(h => c.Name == h.Key && c.Id == h.Value));

            if (client is null || (!environment.IsDevelopment() && !context.Request.Headers[HeaderNames.Origin].Equals(client.Uri)))
            {
                throw new NotValidApiClientException();
            }

            await next(context);
        }
    }
}
