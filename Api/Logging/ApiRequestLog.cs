using Microsoft.Extensions.Logging;

namespace Api.Logging
{
    /// <summary>
    /// Source-generated logging messages used by API controllers and middleware.
    /// </summary>
    internal static partial class ApiRequestLog
    {
        [LoggerMessage(EventId = 1400, Level = LogLevel.Warning, Message = "Sign-in returned unauthorized for username {Username}.")]
        public static partial void SignInUnauthorized(this ILogger logger, string username);

        [LoggerMessage(EventId = 1401, Level = LogLevel.Warning, Message = "Refresh requested without refresh cookie.")]
        public static partial void RefreshCookieMissing(this ILogger logger);

        [LoggerMessage(EventId = 1402, Level = LogLevel.Warning, Message = "Refresh returned unauthorized.")]
        public static partial void RefreshUnauthorized(this ILogger logger);

        [LoggerMessage(EventId = 1403, Level = LogLevel.Debug, Message = "Logout requested without refresh cookie.")]
        public static partial void LogoutCookieMissing(this ILogger logger);

        [LoggerMessage(EventId = 1410, Level = LogLevel.Debug, Message = "Checking application health status.")]
        public static partial void CheckingApplicationHealth(this ILogger logger);

        [LoggerMessage(EventId = 1420, Level = LogLevel.Warning, Message = "API client validation failed because no API clients are configured.")]
        public static partial void ApiClientsNotConfigured(this ILogger logger);

        [LoggerMessage(EventId = 1421, Level = LogLevel.Warning, Message = "API client validation failed. Origin: {Origin}.")]
        public static partial void ApiClientValidationFailed(this ILogger logger, string origin);

        [LoggerMessage(EventId = 1422, Level = LogLevel.Debug, Message = "API client validation succeeded. Client: {ClientName}.")]
        public static partial void ApiClientValidationSucceeded(this ILogger logger, string clientName);

        [LoggerMessage(EventId = 1430, Level = LogLevel.Debug, Message = "Response cache headers skipped for method {Method}.")]
        public static partial void ResponseCacheSkippedForMethod(this ILogger logger, string method);

        [LoggerMessage(EventId = 1431, Level = LogLevel.Debug, Message = "Response cache headers skipped for status code {StatusCode}.")]
        public static partial void ResponseCacheSkippedForStatusCode(this ILogger logger, int statusCode);

        [LoggerMessage(EventId = 1432, Level = LogLevel.Debug, Message = "Response cache headers skipped because response sets cookies.")]
        public static partial void ResponseCacheSkippedForCookies(this ILogger logger);

        [LoggerMessage(EventId = 1433, Level = LogLevel.Debug, Message = "Response cache headers skipped because cache-control already exists.")]
        public static partial void ResponseCacheSkippedForExistingHeader(this ILogger logger);

        [LoggerMessage(EventId = 1434, Level = LogLevel.Debug, Message = "Response cache headers applied. MaxAgeSeconds: {MaxAgeSeconds}.")]
        public static partial void ResponseCacheApplied(this ILogger logger, int maxAgeSeconds);

        [LoggerMessage(EventId = 1440, Level = LogLevel.Error, Message = "An exception has occurred.")]
        public static partial void UnhandledRequestException(this ILogger logger, Exception exception);
    }
}
