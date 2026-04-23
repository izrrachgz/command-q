using Api.Models;
using Api.Middleware.Exceptions;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace Api.Middleware
{
    /// <summary>
    /// Checks if the api keys for the client are present in the request
    /// </summary>
    public sealed class CheckApiClientMiddleware(RequestDelegate next, IOptions<List<ApiClient>> clients)
    {
        public async Task Invoke(HttpContext context)
        {
            if (clients.Value != null && clients.Value.Count != 0)
            {
                ApiClient? client = clients.Value.FirstOrDefault(c => context.Request.Headers.Any(h => c.Name == h.Key && c.Id == h.Value));

                if (client is null || context.Request.Headers[HeaderNames.Origin].Equals(client.Uri))
                {
                    throw new NotValidApiClientException();
                }
            }

            await next(context);
        }
    }
}
