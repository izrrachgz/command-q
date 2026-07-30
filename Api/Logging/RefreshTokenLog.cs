using Microsoft.Extensions.Logging;

namespace Api.Logging
{
    internal static partial class RefreshTokenLog
    {
        [LoggerMessage(EventId = ApiEventIds.RefreshTokenCreated, Level = LogLevel.Debug, Message = "Refresh token created for account {AccountId}.")]
        public static partial void RefreshTokenCreated(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = ApiEventIds.RefreshTokenStoreMiss, Level = LogLevel.Debug, Message = "Refresh token validation missed in token store.")]
        public static partial void RefreshTokenStoreMiss(this ILogger logger);

        [LoggerMessage(EventId = ApiEventIds.RefreshTokenRejected, Level = LogLevel.Debug, Message = "Refresh token rejected for account {AccountId}. Revoked: {Revoked}. Expired: {Expired}.")]
        public static partial void RefreshTokenRejected(this ILogger logger, Guid accountId, bool revoked, bool expired);

        [LoggerMessage(EventId = ApiEventIds.RefreshTokenRotationSkipped, Level = LogLevel.Warning, Message = "Refresh token rotation skipped because the old token was not found.")]
        public static partial void RefreshTokenRotationSkipped(this ILogger logger);

        [LoggerMessage(EventId = ApiEventIds.RefreshTokenRotated, Level = LogLevel.Debug, Message = "Refresh token rotated for account {AccountId}.")]
        public static partial void RefreshTokenRotated(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = ApiEventIds.RefreshTokenRevoked, Level = LogLevel.Debug, Message = "Refresh token revoked for account {AccountId}.")]
        public static partial void RefreshTokenRevoked(this ILogger logger, Guid accountId);
    }
}

