using Api.Models;
using Api.Middleware.Exceptions;
using Microsoft.Extensions.Options;

namespace Api.Middleware
{
    /// <summary>
    /// Checks if the api keys for the client are present in the request
    /// </summary>
    public sealed class CheckApiClientMiddleware(RequestDelegate next, IOptions<List<ApiClient>> clientsOptions)
    {
        public async Task Invoke(HttpContext context)
        {
            if (clientsOptions.Value != null && clientsOptions.Value.Count != 0)
            {
                bool clientExists = context.Request.Headers
                    .Any(h => clientsOptions.Value.Any(c => c.Name == h.Key && c.Id == h.Value));

                if (!clientExists)
                {
                    throw new NotValidApiClientException();
                }
            }

            await next(context);
        }
    }
}
