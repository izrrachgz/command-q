using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Application.Logging
{
    /// <summary>
    /// Source-generated logging messages used by application handlers and health checks.
    /// </summary>
    internal static partial class ApplicationLog
    {
        [LoggerMessage(EventId = ApplicationEventIds.ApplicationUnhealthy, Level = LogLevel.Critical, Message = "Application is unhealthy.")]
        public static partial void ApplicationUnhealthy(this ILogger logger);

        [LoggerMessage(EventId = ApplicationEventIds.ApplicationDegraded, Level = LogLevel.Warning, Message = "Application is degraded.")]
        public static partial void ApplicationDegraded(this ILogger logger);

        [LoggerMessage(EventId = ApplicationEventIds.ApplicationHealthy, Level = LogLevel.Information, Message = "Application is healthy.")]
        public static partial void ApplicationHealthy(this ILogger logger);

        [LoggerMessage(EventId = ApplicationEventIds.ApplicationRunning, Level = LogLevel.Information, Message = "Application is running.")]
        public static partial void ApplicationRunning(this ILogger logger);

        [LoggerMessage(EventId = ApplicationEventIds.DependencyCheckFailed, Level = LogLevel.Debug, Message = "Checking {DependencyName}: no access.")]
        public static partial void DependencyCheckFailed(this ILogger logger, string dependencyName);

        [LoggerMessage(EventId = ApplicationEventIds.DependencyCheckSucceeded, Level = LogLevel.Debug, Message = "Checking {DependencyName}: access granted.")]
        public static partial void DependencyCheckSucceeded(this ILogger logger, string dependencyName);

        [LoggerMessage(EventId = ApplicationEventIds.EntityCreatedForAccount, Level = LogLevel.Information, Message = "{EntityName} created for account {AccountId}.")]
        public static partial void EntityCreatedForAccount(this ILogger logger, string entityName, Guid accountId);

        [LoggerMessage(EventId = ApplicationEventIds.EntityCreationAffectedNoRecords, Level = LogLevel.Warning, Message = "{EntityName} creation affected no records for account {AccountId}.")]
        public static partial void EntityCreationAffectedNoRecords(this ILogger logger, string entityName, Guid accountId);

        [LoggerMessage(EventId = ApplicationEventIds.EntityCreationFailed, Level = LogLevel.Error, Message = "Error creating {EntityName} for AccountId: {AccountId}")]
        public static partial void EntityCreationFailed(this ILogger logger, Exception exception, string entityName, Guid accountId);

        [LoggerMessage(EventId = ApplicationEventIds.AccountCreationDuplicate, Level = LogLevel.Warning, Message = "Account creation rejected because username or email already exists. Username: {Username}.")]
        public static partial void AccountCreationDuplicate(this ILogger logger, string username);

        [LoggerMessage(EventId = ApplicationEventIds.AccountCreationVerificationFailed, Level = LogLevel.Warning, Message = "Account creation verification failed for username {Username}.")]
        public static partial void AccountCreationVerificationFailed(this ILogger logger, string username);

        [LoggerMessage(EventId = ApplicationEventIds.AccountCreated, Level = LogLevel.Information, Message = "Account created. AccountId: {AccountId}. Username: {Username}.")]
        public static partial void AccountCreated(this ILogger logger, Guid accountId, string username);

        [LoggerMessage(EventId = ApplicationEventIds.AccountCreationAffectedNoRecords, Level = LogLevel.Warning, Message = "Account creation affected no records for username {Username}.")]
        public static partial void AccountCreationAffectedNoRecords(this ILogger logger, string username);

        [LoggerMessage(EventId = ApplicationEventIds.AccountCreationFailed, Level = LogLevel.Error, Message = "Error creating account for Email: {Email}, Username: {Username}")]
        public static partial void AccountCreationFailed(this ILogger logger, Exception exception, string email, string username);

        [LoggerMessage(EventId = ApplicationEventIds.AccountSessionNotFound, Level = LogLevel.Warning, Message = "Account session {Operation} failed because account session was not found. AccountId: {AccountId}.")]
        public static partial void AccountSessionNotFound(this ILogger logger, string operation, Guid accountId);

        [LoggerMessage(EventId = ApplicationEventIds.AccountSessionLocked, Level = LogLevel.Information, Message = "Account session locked. AccountId: {AccountId}. LockedUntilUtc: {LockedUntilUtc}.")]
        public static partial void AccountSessionLocked(this ILogger logger, Guid accountId, DateTime? lockedUntilUtc);

        [LoggerMessage(EventId = ApplicationEventIds.AccountSessionUnlocked, Level = LogLevel.Information, Message = "Account session unlocked. AccountId: {AccountId}.")]
        public static partial void AccountSessionUnlocked(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = ApplicationEventIds.AccountSessionChanged, Level = LogLevel.Information, Message = "Account session {Operation}. AccountId: {AccountId}. TokenVersion: {TokenVersion}.")]
        public static partial void AccountSessionChanged(this ILogger logger, string operation, Guid accountId, int tokenVersion);

        [LoggerMessage(EventId = ApplicationEventIds.AccountSessionAffectedNoRecords, Level = LogLevel.Warning, Message = "Account session {Operation} affected no records. AccountId: {AccountId}.")]
        public static partial void AccountSessionAffectedNoRecords(this ILogger logger, string operation, Guid accountId);

        [LoggerMessage(EventId = ApplicationEventIds.AccountSessionChangeFailed, Level = LogLevel.Error, Message = "Error {Operation} account for Id: {AccountId}")]
        public static partial void AccountSessionChangeFailed(this ILogger logger, Exception exception, string operation, Guid accountId);

        [LoggerMessage(EventId = ApplicationEventIds.FileUploadEmptyName, Level = LogLevel.Warning, Message = "File upload rejected for account {AccountId} because the file name was empty.")]
        public static partial void FileUploadEmptyName(this ILogger logger, Guid accountId);

        [LoggerMessage(EventId = ApplicationEventIds.FileUploadTempPathCreated, Level = LogLevel.Debug, Message = "File upload temp path created for account {AccountId}. FileName: {FileName}.")]
        public static partial void FileUploadTempPathCreated(this ILogger logger, Guid accountId, string fileName);

        [LoggerMessage(EventId = ApplicationEventIds.FileUploadFailed, Level = LogLevel.Error, Message = "Error uploading file {FileName}")]
        public static partial void FileUploadFailed(this ILogger logger, Exception exception, string fileName);

        [LoggerMessage(EventId = ApplicationEventIds.FileUploadDestinationGenerated, Level = LogLevel.Debug, Message = "File upload destination path generated for account {AccountId}. RelativePath: {RelativePath}.")]
        public static partial void FileUploadDestinationGenerated(this ILogger logger, Guid accountId, string relativePath);

        [LoggerMessage(EventId = ApplicationEventIds.FileMetadataSaved, Level = LogLevel.Information, Message = "File metadata saved. AccountId: {AccountId}. AccountFileId: {AccountFileId}. FileName: {FileName}. Size: {Size}. ContentType: {ContentType}.")]
        public static partial void FileMetadataSaved(this ILogger logger, Guid accountId, long accountFileId, string fileName, long size, string? contentType);

        [LoggerMessage(EventId = ApplicationEventIds.FileMetadataCreationFailed, Level = LogLevel.Error, Message = "Error creating file {FileName}")]
        public static partial void FileMetadataCreationFailed(this ILogger logger, Exception exception, string fileName);
    }
}

