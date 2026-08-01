using Microsoft.Extensions.Logging;

namespace Api.Modules.Identity.Logging
{
    internal static partial class IdentityApiLog
    {
        [LoggerMessage(EventId = IdentityApiEventIds.SignInUnauthorized, Level = LogLevel.Warning, Message = "Sign-in returned unauthorized for username {Username}.")]
        public static partial void SignInUnauthorized(this ILogger logger, string username);

        [LoggerMessage(EventId = IdentityApiEventIds.RefreshCookieMissing, Level = LogLevel.Warning, Message = "Refresh requested without refresh cookie.")]
        public static partial void RefreshCookieMissing(this ILogger logger);

        [LoggerMessage(EventId = IdentityApiEventIds.RefreshUnauthorized, Level = LogLevel.Warning, Message = "Refresh returned unauthorized.")]
        public static partial void RefreshUnauthorized(this ILogger logger);

        [LoggerMessage(EventId = IdentityApiEventIds.LogoutCookieMissing, Level = LogLevel.Debug, Message = "Logout requested without refresh cookie.")]
        public static partial void LogoutCookieMissing(this ILogger logger);

        [LoggerMessage(EventId = IdentityApiEventIds.LoginFailedForUsername, Level = LogLevel.Warning, Message = "Login failed for username {Username}.")]
        public static partial void LoginFailedForUsername(this ILogger logger, string username);

        [LoggerMessage(EventId = IdentityApiEventIds.LoginBlockedForLockedAccount, Level = LogLevel.Warning, Message = "Login blocked because account {AccountId} is locked.")]
        public static partial void LoginBlockedForLockedAccount(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = IdentityApiEventIds.AccountLocked, Level = LogLevel.Warning, Message = "Account {AccountId} locked until {LockedUntilUtc}.")]
        public static partial void AccountLocked(this ILogger logger, Guid accountId, DateTime? lockedUntilUtc);

        [LoggerMessage(EventId = IdentityApiEventIds.LoginFailedForAccount, Level = LogLevel.Warning, Message = "Login failed for account {AccountId}. Failed attempts: {FailedLoginAttempts}.")]
        public static partial void LoginFailedForAccount(this ILogger logger, Guid accountId, byte failedLoginAttempts);

        [LoggerMessage(EventId = IdentityApiEventIds.LoginSucceeded, Level = LogLevel.Information, Message = "Login succeeded for account {AccountId}.")]
        public static partial void LoginSucceeded(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = IdentityApiEventIds.RefreshTokenValidationFailed, Level = LogLevel.Warning, Message = "Refresh token validation failed.")]
        public static partial void RefreshTokenValidationFailed(this ILogger logger);

        [LoggerMessage(EventId = IdentityApiEventIds.RefreshCacheHit, Level = LogLevel.Debug, Message = "Refresh account/session cache hit for account {AccountId}.")]
        public static partial void RefreshCacheHit(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = IdentityApiEventIds.RefreshCacheMiss, Level = LogLevel.Debug, Message = "Refresh account/session cache miss for account {AccountId}.")]
        public static partial void RefreshCacheMiss(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = IdentityApiEventIds.RefreshAccountSessionNotFound, Level = LogLevel.Warning, Message = "Refresh failed because account/session data was not found for account {AccountId}.")]
        public static partial void RefreshAccountSessionNotFound(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = IdentityApiEventIds.RefreshSucceeded, Level = LogLevel.Information, Message = "Refresh succeeded for account {AccountId}. Token version: {TokenVersion}.")]
        public static partial void RefreshSucceeded(this ILogger logger, Guid accountId, int tokenVersion);

        [LoggerMessage(EventId = IdentityApiEventIds.LogoutRevokedRefreshToken, Level = LogLevel.Information, Message = "Logout revoked refresh token and removed cache for account {AccountId}.")]
        public static partial void LogoutRevokedRefreshToken(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = IdentityApiEventIds.LogoutInvalidRefreshToken, Level = LogLevel.Warning, Message = "Logout attempted with an invalid or expired refresh token.")]
        public static partial void LogoutInvalidRefreshToken(this ILogger logger);

        [LoggerMessage(EventId = IdentityApiEventIds.RefreshTokenCreated, Level = LogLevel.Debug, Message = "Refresh token created for account {AccountId}.")]
        public static partial void RefreshTokenCreated(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = IdentityApiEventIds.RefreshTokenStoreMiss, Level = LogLevel.Debug, Message = "Refresh token validation missed in token store.")]
        public static partial void RefreshTokenStoreMiss(this ILogger logger);

        [LoggerMessage(EventId = IdentityApiEventIds.RefreshTokenRejected, Level = LogLevel.Debug, Message = "Refresh token rejected for account {AccountId}. Revoked: {Revoked}. Expired: {Expired}.")]
        public static partial void RefreshTokenRejected(this ILogger logger, Guid accountId, bool revoked, bool expired);

        [LoggerMessage(EventId = IdentityApiEventIds.RefreshTokenRotationSkipped, Level = LogLevel.Warning, Message = "Refresh token rotation skipped because the old token was not found.")]
        public static partial void RefreshTokenRotationSkipped(this ILogger logger);

        [LoggerMessage(EventId = IdentityApiEventIds.RefreshTokenRotated, Level = LogLevel.Debug, Message = "Refresh token rotated for account {AccountId}.")]
        public static partial void RefreshTokenRotated(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = IdentityApiEventIds.RefreshTokenRevoked, Level = LogLevel.Debug, Message = "Refresh token revoked for account {AccountId}.")]
        public static partial void RefreshTokenRevoked(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = IdentityApiEventIds.AccountSessionCacheCleanupStarted, Level = LogLevel.Debug, Message = "Account session cache cleanup started.")]
        public static partial void AccountSessionCacheCleanupStarted(this ILogger logger);

        [LoggerMessage(EventId = IdentityApiEventIds.AccountSessionCacheCleanupCompleted, Level = LogLevel.Debug, Message = "Account session cache cleanup completed.")]
        public static partial void AccountSessionCacheCleanupCompleted(this ILogger logger);

        [LoggerMessage(EventId = IdentityApiEventIds.AccountSessionCacheCleanupStopped, Level = LogLevel.Debug, Message = "Account session cache cleanup stopped.")]
        public static partial void AccountSessionCacheCleanupStopped(this ILogger logger);
    }
}
