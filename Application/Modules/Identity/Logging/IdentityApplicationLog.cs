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

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountProvisioningAlreadyLinked, Level = LogLevel.Information, Message = "Account provisioning skipped because identity is already linked. Username: {Username}.")]
        public static partial void AccountProvisioningAlreadyLinked(this ILogger logger, string username);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountIdentityLinked, Level = LogLevel.Information, Message = "Account identity linked to existing account. AccountId: {AccountId}. Username: {Username}.")]
        public static partial void AccountIdentityLinked(this ILogger logger, Guid accountId, string username);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountActivityRecorded, Level = LogLevel.Information, Message = "Account activity recorded. AccountId: {AccountId}. Module: {Module}. Action: {Action}. EntityType: {EntityType}. EntityId: {EntityId}.")]
        public static partial void AccountActivityRecorded(this ILogger logger, Guid accountId, string module, string action, string? entityType, string? entityId);

        [LoggerMessage(EventId = IdentityApplicationEventIds.AccountActivityRecordFailed, Level = LogLevel.Error, Message = "Error recording account activity. AccountId: {AccountId}. Module: {Module}. Action: {Action}.")]
        public static partial void AccountActivityRecordFailed(this ILogger logger, Exception exception, Guid accountId, string module, string action);
    }
}
