using Microsoft.Extensions.Logging;

namespace Application.Modules.Identity.Logging
{
    internal static partial class IdentityApplicationLog
    {
        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountCreationDuplicate, Level = LogLevel.Warning, Message = "Account creation rejected because username or email already exists. Username: {Username}.")]
        public static partial void AccountCreationDuplicate(this ILogger logger, string username);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountCreationVerificationFailed, Level = LogLevel.Warning, Message = "Account creation verification failed for username {Username}.")]
        public static partial void AccountCreationVerificationFailed(this ILogger logger, string username);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountCreated, Level = LogLevel.Information, Message = "Account created. AccountId: {AccountId}. Username: {Username}.")]
        public static partial void AccountCreated(this ILogger logger, Guid accountId, string username);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountCreationAffectedNoRecords, Level = LogLevel.Warning, Message = "Account creation affected no records for username {Username}.")]
        public static partial void AccountCreationAffectedNoRecords(this ILogger logger, string username);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountCreationFailed, Level = LogLevel.Error, Message = "Error creating account for Email: {Email}, Username: {Username}")]
        public static partial void AccountCreationFailed(this ILogger logger, Exception exception, string email, string username);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountSessionNotFound, Level = LogLevel.Warning, Message = "Account session {Operation} failed because account session was not found. AccountId: {AccountId}.")]
        public static partial void AccountSessionNotFound(this ILogger logger, string operation, Guid accountId);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountSessionLocked, Level = LogLevel.Information, Message = "Account session locked. AccountId: {AccountId}. LockedUntilUtc: {LockedUntilUtc}.")]
        public static partial void AccountSessionLocked(this ILogger logger, Guid accountId, DateTime? lockedUntilUtc);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountSessionUnlocked, Level = LogLevel.Information, Message = "Account session unlocked. AccountId: {AccountId}.")]
        public static partial void AccountSessionUnlocked(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountSessionChanged, Level = LogLevel.Information, Message = "Account session {Operation}. AccountId: {AccountId}. TokenVersion: {TokenVersion}.")]
        public static partial void AccountSessionChanged(this ILogger logger, string operation, Guid accountId, int tokenVersion);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountSessionAffectedNoRecords, Level = LogLevel.Warning, Message = "Account session {Operation} affected no records. AccountId: {AccountId}.")]
        public static partial void AccountSessionAffectedNoRecords(this ILogger logger, string operation, Guid accountId);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountSessionChangeFailed, Level = LogLevel.Error, Message = "Error {Operation} account for Id: {AccountId}")]
        public static partial void AccountSessionChangeFailed(this ILogger logger, Exception exception, string operation, Guid accountId);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountActivityRecorded, Level = LogLevel.Information, Message = "Account activity recorded. AccountId: {AccountId}. Module: {Module}. Action: {Action}. EntityType: {EntityType}. EntityId: {EntityId}.")]
        public static partial void AccountActivityRecorded(this ILogger logger, Guid accountId, string module, string action, string? entityType, string? entityId);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountActivityRecordFailed, Level = LogLevel.Error, Message = "Error recording account activity. AccountId: {AccountId}. Module: {Module}. Action: {Action}.")]
        public static partial void AccountActivityRecordFailed(this ILogger logger, Exception exception, Guid accountId, string module, string action);
    }
}
