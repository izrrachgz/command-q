using Microsoft.Extensions.Logging;

namespace Api.Logging
{
    internal static partial class AuthLog
    {
        [LoggerMessage(EventId = ApiEventIds.LoginFailedForUsername, Level = LogLevel.Warning, Message = "Login failed for username {Username}.")]
        public static partial void LoginFailedForUsername(this ILogger logger, string username);

        [LoggerMessage(EventId = ApiEventIds.LoginBlockedForLockedAccount, Level = LogLevel.Warning, Message = "Login blocked because account {AccountId} is locked.")]
        public static partial void LoginBlockedForLockedAccount(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = ApiEventIds.AccountLocked, Level = LogLevel.Warning, Message = "Account {AccountId} locked until {LockedUntilUtc}.")]
        public static partial void AccountLocked(this ILogger logger, Guid accountId, DateTime? lockedUntilUtc);

        [LoggerMessage(EventId = ApiEventIds.LoginFailedForAccount, Level = LogLevel.Warning, Message = "Login failed for account {AccountId}. Failed attempts: {FailedLoginAttempts}.")]
        public static partial void LoginFailedForAccount(this ILogger logger, Guid accountId, byte failedLoginAttempts);

        [LoggerMessage(EventId = ApiEventIds.LoginSucceeded, Level = LogLevel.Information, Message = "Login succeeded for account {AccountId}.")]
        public static partial void LoginSucceeded(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = ApiEventIds.RefreshTokenValidationFailed, Level = LogLevel.Warning, Message = "Refresh token validation failed.")]
        public static partial void RefreshTokenValidationFailed(this ILogger logger);

        [LoggerMessage(EventId = ApiEventIds.RefreshCacheHit, Level = LogLevel.Debug, Message = "Refresh account/session cache hit for account {AccountId}.")]
        public static partial void RefreshCacheHit(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = ApiEventIds.RefreshCacheMiss, Level = LogLevel.Debug, Message = "Refresh account/session cache miss for account {AccountId}.")]
        public static partial void RefreshCacheMiss(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = ApiEventIds.RefreshAccountSessionNotFound, Level = LogLevel.Warning, Message = "Refresh failed because account/session data was not found for account {AccountId}.")]
        public static partial void RefreshAccountSessionNotFound(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = ApiEventIds.RefreshSucceeded, Level = LogLevel.Information, Message = "Refresh succeeded for account {AccountId}. Token version: {TokenVersion}.")]
        public static partial void RefreshSucceeded(this ILogger logger, Guid accountId, int tokenVersion);

        [LoggerMessage(EventId = ApiEventIds.LogoutRevokedRefreshToken, Level = LogLevel.Information, Message = "Logout revoked refresh token and removed cache for account {AccountId}.")]
        public static partial void LogoutRevokedRefreshToken(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = ApiEventIds.LogoutInvalidRefreshToken, Level = LogLevel.Warning, Message = "Logout attempted with an invalid or expired refresh token.")]
        public static partial void LogoutInvalidRefreshToken(this ILogger logger);
    }
}

