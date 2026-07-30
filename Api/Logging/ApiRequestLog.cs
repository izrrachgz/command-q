using Microsoft.Extensions.Logging;

namespace Api.Logging
{
    /// <summary>
    /// Source-generated logging messages used by API controllers and middleware.
    /// </summary>
    internal static partial class ApiRequestLog
    {
        [LoggerMessage(EventId = ApiEventIds.SignInUnauthorized, Level = LogLevel.Warning, Message = "Sign-in returned unauthorized for username {Username}.")]
        public static partial void SignInUnauthorized(this ILogger logger, string username);

        [LoggerMessage(EventId = ApiEventIds.RefreshCookieMissing, Level = LogLevel.Warning, Message = "Refresh requested without refresh cookie.")]
        public static partial void RefreshCookieMissing(this ILogger logger);

        [LoggerMessage(EventId = ApiEventIds.RefreshUnauthorized, Level = LogLevel.Warning, Message = "Refresh returned unauthorized.")]
        public static partial void RefreshUnauthorized(this ILogger logger);

        [LoggerMessage(EventId = ApiEventIds.LogoutCookieMissing, Level = LogLevel.Debug, Message = "Logout requested without refresh cookie.")]
        public static partial void LogoutCookieMissing(this ILogger logger);

        [LoggerMessage(EventId = ApiEventIds.CheckingApplicationHealth, Level = LogLevel.Debug, Message = "Checking application health status.")]
        public static partial void CheckingApplicationHealth(this ILogger logger);

        [LoggerMessage(EventId = ApiEventIds.ApiClientsNotConfigured, Level = LogLevel.Warning, Message = "API client validation failed because no API clients are configured.")]
        public static partial void ApiClientsNotConfigured(this ILogger logger);

        [LoggerMessage(EventId = ApiEventIds.ApiClientValidationFailed, Level = LogLevel.Warning, Message = "API client validation failed. Origin: {Origin}.")]
        public static partial void ApiClientValidationFailed(this ILogger logger, string origin);

        [LoggerMessage(EventId = ApiEventIds.ApiClientValidationSucceeded, Level = LogLevel.Debug, Message = "API client validation succeeded. Client: {ClientName}.")]
        public static partial void ApiClientValidationSucceeded(this ILogger logger, string clientName);

        [LoggerMessage(EventId = ApiEventIds.ResponseCacheSkippedForMethod, Level = LogLevel.Debug, Message = "Response cache headers skipped for method {Method}.")]
        public static partial void ResponseCacheSkippedForMethod(this ILogger logger, string method);

        [LoggerMessage(EventId = ApiEventIds.ResponseCacheSkippedForStatusCode, Level = LogLevel.Debug, Message = "Response cache headers skipped for status code {StatusCode}.")]
        public static partial void ResponseCacheSkippedForStatusCode(this ILogger logger, int statusCode);

        [LoggerMessage(EventId = ApiEventIds.ResponseCacheSkippedForCookies, Level = LogLevel.Debug, Message = "Response cache headers skipped because response sets cookies.")]
        public static partial void ResponseCacheSkippedForCookies(this ILogger logger);

        [LoggerMessage(EventId = ApiEventIds.ResponseCacheSkippedForExistingHeader, Level = LogLevel.Debug, Message = "Response cache headers skipped because cache-control already exists.")]
        public static partial void ResponseCacheSkippedForExistingHeader(this ILogger logger);

        [LoggerMessage(EventId = ApiEventIds.ResponseCacheApplied, Level = LogLevel.Debug, Message = "Response cache headers applied. MaxAgeSeconds: {MaxAgeSeconds}.")]
        public static partial void ResponseCacheApplied(this ILogger logger, int maxAgeSeconds);

        [LoggerMessage(EventId = ApiEventIds.UnhandledRequestException, Level = LogLevel.Error, Message = "An exception has occurred.")]
        public static partial void UnhandledRequestException(this ILogger logger, Exception exception);
    }
}

