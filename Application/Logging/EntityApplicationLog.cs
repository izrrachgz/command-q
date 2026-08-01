using Microsoft.Extensions.Logging;

namespace Application.Logging
{
    /// <summary>
    /// Source-generated logging messages used across application modules.
    /// </summary>
    internal static partial class EntityApplicationLog
    {
        [LoggerMessage(EventId = ApplicationEventIds.EntityCreatedForAccount, Level = LogLevel.Information, Message = "{EntityName} created for account {AccountId}.")]
        public static partial void EntityCreatedForAccount(this ILogger logger, string entityName, Guid accountId);

        [LoggerMessage(EventId = ApplicationEventIds.EntityCreationAffectedNoRecords, Level = LogLevel.Warning, Message = "{EntityName} creation affected no records for account {AccountId}.")]
        public static partial void EntityCreationAffectedNoRecords(this ILogger logger, string entityName, Guid accountId);

        [LoggerMessage(EventId = ApplicationEventIds.EntityCreationFailed, Level = LogLevel.Error, Message = "Error creating {EntityName} for AccountId: {AccountId}")]
        public static partial void EntityCreationFailed(this ILogger logger, Exception exception, string entityName, Guid accountId);
    }
}

