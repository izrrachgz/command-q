using Microsoft.Extensions.Logging;

namespace Email.Outbox.Logging
{
    internal static partial class EmailOutboxLog
    {
        [LoggerMessage(EventId = EmailOutboxEventIds.BatchClaimed, Level = LogLevel.Debug, Message = "Claimed {Count} email outbox records.")]
        public static partial void BatchClaimed(this ILogger logger, int count);

        [LoggerMessage(EventId = EmailOutboxEventIds.PayloadInvalid, Level = LogLevel.Error, Message = "Email outbox record {OutboxId} contains an invalid payload and will be dead-lettered.")]
        public static partial void PayloadInvalid(this ILogger logger, Exception exception, Guid outboxId);

        [LoggerMessage(EventId = EmailOutboxEventIds.BatchSent, Level = LogLevel.Information, Message = "Sent {Count} email outbox records.")]
        public static partial void BatchSent(this ILogger logger, int count);

        [LoggerMessage(EventId = EmailOutboxEventIds.DeliveryFailed, Level = LogLevel.Warning, Message = "Email outbox delivery failed for record {OutboxId}. Failure: {FailureKind}. Dead letter: {IsDeadLetter}.")]
        public static partial void DeliveryFailed(this ILogger logger, Exception exception, Guid outboxId, object failureKind, bool isDeadLetter);

        [LoggerMessage(EventId = EmailOutboxEventIds.RecordsReleased, Level = LogLevel.Debug, Message = "Released {Count} unattempted email outbox records.")]
        public static partial void RecordsReleased(this ILogger logger, int count);
    }
}
