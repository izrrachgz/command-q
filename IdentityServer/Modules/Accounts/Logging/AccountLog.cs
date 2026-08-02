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
    }
}
