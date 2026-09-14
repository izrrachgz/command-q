namespace IdentityServer.Modules.Accounts.Logging
{
    internal static partial class AccountLog
    {
        [LoggerMessage(EventId = AccountEventIds.Registered, Level = LogLevel.Information, Message = "Identity account registered. UserId: {UserId}.")]
        public static partial void AccountRegistered(this ILogger logger, Guid userId);

        [LoggerMessage(EventId = AccountEventIds.RegistrationFailed, Level = LogLevel.Warning, Message = "Identity account registration failed for username {Username}.")]
        public static partial void AccountRegistrationFailed(this ILogger logger, string username);

        [LoggerMessage(EventId = AccountEventIds.LoginSucceeded, Level = LogLevel.Information, Message = "Identity login succeeded. UserId: {UserId}.")]
        public static partial void LoginSucceeded(this ILogger logger, Guid userId);

        [LoggerMessage(EventId = AccountEventIds.LoginFailed, Level = LogLevel.Warning, Message = "Identity login failed for {UsernameOrEmail}.")]
        public static partial void LoginFailed(this ILogger logger, string usernameOrEmail);

        [LoggerMessage(EventId = AccountEventIds.LoginLockedOut, Level = LogLevel.Warning, Message = "Identity login blocked because the account is locked. UserId: {UserId}.")]
        public static partial void LoginLockedOut(this ILogger logger, Guid userId);

        [LoggerMessage(EventId = AccountEventIds.Logout, Level = LogLevel.Information, Message = "Identity account signed out. UserId: {UserId}.")]
        public static partial void AccountSignedOut(this ILogger logger, Guid userId);

        [LoggerMessage(EventId = AccountEventIds.ExternalLoginSucceeded, Level = LogLevel.Information, Message = "External identity login succeeded. UserId: {UserId}. Provider: {Provider}.")]
        public static partial void ExternalLoginSucceeded(this ILogger logger, Guid userId, string provider);

        [LoggerMessage(EventId = AccountEventIds.ExternalLoginFailed, Level = LogLevel.Warning, Message = "External identity login failed. Provider: {Provider}.")]
        public static partial void ExternalLoginFailed(this ILogger logger, string provider);

        [LoggerMessage(EventId = AccountEventIds.SecurityEventRecordFailed, Level = LogLevel.Error, Message = "Identity security event could not be recorded. Action: {Action}. UserId: {UserId}.")]
        public static partial void SecurityEventRecordFailed(this ILogger logger, Exception exception, string action, Guid? userId);

        [LoggerMessage(EventId = AccountEventIds.EmailQueueFailed, Level = LogLevel.Error, Message = "Identity account email could not be queued. UserId: {UserId}. Purpose: {Purpose}.")]
        public static partial void AccountEmailQueueFailed(this ILogger logger, Exception exception, Guid userId, string purpose);

        [LoggerMessage(EventId = AccountEventIds.EmailRequestFailed, Level = LogLevel.Error, Message = "Identity account email request failed. Purpose: {Purpose}.")]
        public static partial void AccountEmailRequestFailed(this ILogger logger, Exception exception, string purpose);

        [LoggerMessage(EventId = AccountEventIds.EmailDispatchQueued, Level = LogLevel.Information, Message = "Identity account email dispatch queued. DispatchId: {DispatchId}. ChallengeId: {ChallengeId}.")]
        public static partial void AccountEmailDispatchQueued(this ILogger logger, Guid dispatchId, Guid challengeId);

        [LoggerMessage(EventId = AccountEventIds.EmailDispatchRetry, Level = LogLevel.Warning, Message = "Identity account email dispatch will be retried. DispatchId: {DispatchId}. Attempt: {Attempt}. NextAttemptUtc: {NextAttemptUtc}.")]
        public static partial void AccountEmailDispatchRetry(
            this ILogger logger,
            Exception exception,
            Guid dispatchId,
            int attempt,
            DateTime nextAttemptUtc);

        [LoggerMessage(EventId = AccountEventIds.EmailDispatchInvalid, Level = LogLevel.Error, Message = "Identity account email dispatch payload is invalid and was abandoned. DispatchId: {DispatchId}.")]
        public static partial void AccountEmailDispatchInvalid(this ILogger logger, Exception exception, Guid dispatchId);

        [LoggerMessage(EventId = AccountEventIds.EmailMaintenanceFailed, Level = LogLevel.Error, Message = "Identity account email maintenance iteration failed.")]
        public static partial void AccountEmailMaintenanceFailed(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = AccountEventIds.AccountDataCleanedUp, Level = LogLevel.Information, Message = "Expired identity account data removed. Challenges: {ChallengeCount}. SecurityEvents: {SecurityEventCount}.")]
        public static partial void AccountDataCleanedUp(this ILogger logger, int challengeCount, int securityEventCount);
    }
}
