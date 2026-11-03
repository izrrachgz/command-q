using Microsoft.Extensions.Logging;

namespace Persistence.Logging
{
    /// <summary>
    /// Source-generated logging messages used by persistence infrastructure.
    /// </summary>
    internal static partial class PersistenceLog
    {
        [LoggerMessage(EventId = PersistenceEventIds.RepositoryCreated, Level = LogLevel.Debug, Message = "{RepositoryName} instance created, using engine: {Engine}.")]
        public static partial void RepositoryCreated(this ILogger logger, string repositoryName, object engine);

        [LoggerMessage(EventId = PersistenceEventIds.CommandRunnerCreated, Level = LogLevel.Debug, Message = "Instance created: {InstanceName}.")]
        public static partial void CommandRunnerCreated(this ILogger logger, string instanceName);

        [LoggerMessage(EventId = PersistenceEventIds.ExecutingCommandBatch, Level = LogLevel.Debug, Message = "Executing {CommandCount} commands.")]
        public static partial void ExecutingCommandBatch(this ILogger logger, int commandCount);

        [LoggerMessage(EventId = PersistenceEventIds.CommandCancelled, Level = LogLevel.Debug, Message = "Command cancelled.")]
        public static partial void CommandCancelled(this ILogger logger);

        [LoggerMessage(EventId = PersistenceEventIds.CommandFailed, Level = LogLevel.Error, Message = "An exception has occurred.")]
        public static partial void CommandFailed(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = PersistenceEventIds.ExecutingQuery, Level = LogLevel.Debug, Message = "Executing database command.")]
        public static partial void ExecutingQuery(this ILogger logger);

        [LoggerMessage(EventId = PersistenceEventIds.ParameterInQuery, Level = LogLevel.Debug, Message = "Binding database parameter. Type: {ParameterType}.")]
        public static partial void ParameterInQuery(this ILogger logger, string parameterType);

        [LoggerMessage(EventId = PersistenceEventIds.SavingRecords, Level = LogLevel.Debug, Message = "Saving {RecordCount} records.")]
        public static partial void SavingRecords(this ILogger logger, int recordCount);

        [LoggerMessage(EventId = PersistenceEventIds.CancellationRequested, Level = LogLevel.Debug, Message = "Cancellation has been requested.")]
        public static partial void CancellationRequested(this ILogger logger);

        [LoggerMessage(EventId = PersistenceEventIds.CannotReadTableMetadata, Level = LogLevel.Debug, Message = "Cannot read table metadata.")]
        public static partial void CannotReadTableMetadata(this ILogger logger);

    }
}

