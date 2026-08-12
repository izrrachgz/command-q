using Microsoft.Extensions.Logging;

namespace Email.Worker.Logging
{
    internal static partial class EmailWorkerLog
    {
        [LoggerMessage(EventId = EmailWorkerEventIds.Started, Level = LogLevel.Information, Message = "Email outbox worker {WorkerId} started. Batch size: {BatchSize}. Poll interval: {PollIntervalSeconds} seconds.")]
        public static partial void Started(this ILogger logger, string workerId, int batchSize, int pollIntervalSeconds);

        [LoggerMessage(EventId = EmailWorkerEventIds.Disabled, Level = LogLevel.Warning, Message = "Email outbox worker is disabled by configuration.")]
        public static partial void Disabled(this ILogger logger);

        [LoggerMessage(EventId = EmailWorkerEventIds.BatchCompleted, Level = LogLevel.Information, Message = "Email outbox batch completed. Claimed: {Claimed}. Sent: {Sent}. Retry: {Retry}. Dead letter: {DeadLetter}. Released: {Released}.")]
        public static partial void BatchCompleted(this ILogger logger, int claimed, int sent, int retry, int deadLetter, int released);

        [LoggerMessage(EventId = EmailWorkerEventIds.PollFailed, Level = LogLevel.Error, Message = "Email outbox polling cycle failed.")]
        public static partial void PollFailed(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = EmailWorkerEventIds.Stopped, Level = LogLevel.Information, Message = "Email outbox worker {WorkerId} stopped.")]
        public static partial void Stopped(this ILogger logger, string workerId);

        [LoggerMessage(EventId = EmailWorkerEventIds.StartupConfiguration, Level = LogLevel.Information, Message = "Email worker starting. Environment: {Environment}. Database engine: {DatabaseEngine}. Migrations on startup: {RunMigrationsOnStartup}. Polling enabled: {WorkerEnabled}. SMTP delivery enabled: {EmailEnabled}.")]
        public static partial void StartupConfiguration(
            this ILogger logger,
            string environment,
            string databaseEngine,
            bool runMigrationsOnStartup,
            bool workerEnabled,
            bool emailEnabled);

        [LoggerMessage(EventId = EmailWorkerEventIds.ConfigurationReloaded, Level = LogLevel.Information, Message = "Email worker configuration reloaded. Enabled: {Enabled}. Batch size: {BatchSize}. Poll interval: {PollIntervalSeconds} seconds. Lease: {LeaseSeconds} seconds. Error delay: {ErrorDelaySeconds} seconds.")]
        public static partial void ConfigurationReloaded(this ILogger logger, bool enabled, int batchSize, int pollIntervalSeconds, int leaseSeconds, int errorDelaySeconds);
    }
}
