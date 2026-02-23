using Api.Constants;
using Api.Middleware.Exceptions;

namespace Api.Middleware
{
    /// <summary>
    /// Checks if the api keys are present in the request
    /// </summary>
    public sealed class CheckApiKeysMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        public async Task Invoke(HttpContext context)
        {
            string? headerKeyName = configuration.GetValue<string>(AppSettingsKeys.ApiKeyHeaderKeyName);

            if (!string.IsNullOrEmpty(headerKeyName))
            {
                string? apiKey = context.Request.Headers[headerKeyName];
                string? excpectedValue = configuration.GetValue<string>(AppSettingsKeys.ApiKeyHeaderKeyValue);

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(excpectedValue) || apiKey != excpectedValue)
                {
                    throw new NotValidApiKeysException();
                }
            }

            await next(context);
        }
    }
}
