using Api.Configuration;
using Api.Logging;
using System.Text;
using Api.Constants;
using Api.Middleware.Exceptions;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Api.Middleware
{
    /// <summary>
    /// Checks if the api keys for the client are present in the request
    /// </summary>
    public sealed class CheckApiClientMiddleware(RequestDelegate next, IHostEnvironment environment, IOptions<List<ApiClientOptions>> configuredClients, ILogger<CheckApiClientMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(RouteConstants.HealthPrefix))
            {
                await next(context);
                return;
            }

            if (configuredClients.Value is null || configuredClients.Value.Count == 0)
            {
                logger.ApiClientsNotConfigured();
                throw new NotValidApiClientException();
            }

            ApiClientOptions? client = configuredClients.Value.FirstOrDefault(c =>
                context.Request.Headers.TryGetValue(c.Name, out var headerValues) &&
                headerValues.Any(headerValue => SecureEquals(c.Id, headerValue)));

            if (client is null || (!environment.IsDevelopment() && !IsAllowedOrigin(context, client)))
            {
                logger.ApiClientValidationFailed(context.Request.Headers[HeaderNames.Origin].ToString());
                throw new NotValidApiClientException();
            }

            logger.ApiClientValidationSucceeded(client.Name);

            await next(context);
        }

        private static bool IsAllowedOrigin(HttpContext context, ApiClientOptions client)
        {
            string? origin = context.Request.Headers[HeaderNames.Origin].FirstOrDefault();

            return Uri.TryCreate(origin, UriKind.Absolute, out var requestOrigin) &&
                Uri.TryCreate(client.Uri, UriKind.Absolute, out var configuredOrigin) &&
                Uri.Compare(requestOrigin, configuredOrigin, UriComponents.SchemeAndServer, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0;
        }

        private static bool SecureEquals(string expected, string? actual)
        {
            if (string.IsNullOrEmpty(actual))
            {
                return false;
            }

            byte[] expectedBytes = Encoding.UTF8.GetBytes(expected);
            byte[] actualBytes = Encoding.UTF8.GetBytes(actual);

            return expectedBytes.Length == actualBytes.Length &&
                CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
        }
    }
}
