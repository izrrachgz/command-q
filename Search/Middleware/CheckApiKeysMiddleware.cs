using Search.Constants;
using Search.Middleware.Exceptions;

namespace Search.Middleware
{
    /// <summary>
    /// Checks if the api keys are present in the request
    /// </summary>
    public sealed class CheckApiKeysMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public CheckApiKeysMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            string? headerKeyName = _configuration.GetValue<string>(SearchConfiguration.ApiKeyHeaderKeyName);

            if (!string.IsNullOrEmpty(headerKeyName))
            {
                string? apiKey = context.Request.Headers[headerKeyName];
                string? excpectedValue = _configuration.GetValue<string>(SearchConfiguration.ApiKeyHeaderKeyValue);

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(excpectedValue) || apiKey != excpectedValue)
                {
                    throw new NotValidApiKeysException();
                }
            }

            await _next(context);
        }
    }
}
