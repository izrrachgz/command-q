using Email.Models;
using Email.Exceptions;
using Email.Abstractions;
using Email.Outbox.Models;
using Email.Outbox.Logging;
using Email.Outbox.Abstractions;
using Email.Outbox.Configuration;
using Email.Outbox.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Email.Outbox.Services
{
    internal sealed class EmailOutboxProcessor(
        IEmailOutboxStore store,
        IEmailSender sender,
        IEmailOutboxSerializer serializer,
        IOptions<EmailOutboxOptions> options,
        ILogger<EmailOutboxProcessor> logger)
        : IEmailOutboxProcessor
    {
        private readonly EmailOutboxOptions _options = options.Value;

        public async Task<EmailOutboxBatchResult> ProcessBatchAsync(
            EmailOutboxClaimRequest request,
            CancellationToken cancellationToken = default)
        {
            var claimed = await store.ClaimPendingAsync(request, cancellationToken);
            logger.BatchClaimed(claimed.Count);

            if (claimed.Count == 0)
            {
                return EmailOutboxBatchResult.Empty;
            }

            var deliverable = new List<(ClaimedEmailOutboxRecord Record, EmailMessage Message)>(claimed.Count);
            var result = new MutableBatchResult(claimed.Count);

            foreach (var record in claimed)
            {
                try
                {
                    deliverable.Add((record, serializer.Deserialize(record.Payload)));
                }
                catch (Exception exception)
                {
                    logger.PayloadInvalid(exception, record.Id);
                    await MarkFailure(
                        record,
                        EmailOutboxFailureKind.Serialization,
                        exception,
                        isPermanent: true,
                        result,
                        cancellationToken);
                }
            }

            if (deliverable.Count == 0)
            {
                return result.ToResult();
            }

            try
            {
                var sendResult = await sender.SendAsync(
                    deliverable.Select(item => item.Message),
                    cancellationToken);

                if (sendResult.Messages.Count != deliverable.Count)
                {
                    throw new InvalidOperationException("The email sender returned a result count that does not match the outbox batch.");
                }

                for (var index = 0; index < deliverable.Count; index++)
                {
                    await MarkSent(deliverable[index].Record, sendResult.Messages[index], result, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await Release(deliverable.Select(item => item.Record).ToArray(), result, CancellationToken.None);
                throw;
            }
            catch (EmailDeliveryException exception)
            {
                int sentCount = Math.Min(exception.SentCount, deliverable.Count);

                for (var index = 0; index < sentCount; index++)
                {
                    await MarkSent(deliverable[index].Record, exception.SentMessages[index], result, cancellationToken);
                }

                if (sentCount < deliverable.Count)
                {
                    var failed = deliverable[sentCount].Record;
                    EmailOutboxFailureKind failureKind = EmailOutboxFailureMapper.Map(exception.FailureKind);
                    await MarkFailure(
                        failed,
                        failureKind,
                        exception,
                        IsPermanent(failureKind),
                        result,
                        cancellationToken);

                    await Release(
                        deliverable.Skip(sentCount + 1).Select(item => item.Record).ToArray(),
                        result,
                        cancellationToken);
                }
            }
            catch (Exception exception)
            {
                EmailOutboxFailureKind failureKind = exception is ArgumentException
                    ? EmailOutboxFailureKind.Validation
                    : EmailOutboxFailureKind.Unknown;
                await MarkFailure(
                    deliverable[0].Record,
                    failureKind,
                    exception,
                    IsPermanent(failureKind),
                    result,
                    cancellationToken);
                await Release(
                    deliverable.Skip(1).Select(item => item.Record).ToArray(),
                    result,
                    cancellationToken);
            }

            logger.BatchSent(result.SentCount);
            return result.ToResult();
        }

        private async Task MarkSent(
            ClaimedEmailOutboxRecord record,
            EmailSendResult sendResult,
            MutableBatchResult result,
            CancellationToken cancellationToken)
        {
            bool updated = await store.MarkSentAsync(
                new EmailOutboxSentUpdate(
                    record.Id,
                    record.LockId,
                    record.AttemptCount + 1,
                    sendResult.MessageId,
                    sendResult.SmtpResponse,
                    sendResult.AcceptedAtUtc.UtcDateTime),
                cancellationToken);

            if (updated)
            {
                result.SentCount++;
            }
        }

        private async Task MarkFailure(
            ClaimedEmailOutboxRecord record,
            EmailOutboxFailureKind failureKind,
            Exception exception,
            bool isPermanent,
            MutableBatchResult result,
            CancellationToken cancellationToken)
        {
            int attemptCount = record.AttemptCount + 1;
            bool isDeadLetter = isPermanent || attemptCount >= _options.MaxAttempts;
            DateTime failedUtc = DateTime.UtcNow;
            DateTime nextAttemptUtc = isDeadLetter
                ? failedUtc
                : failedUtc.Add(CalculateRetryDelay(attemptCount));
            string error = Truncate(exception.GetBaseException().Message, _options.MaxErrorLength);

            bool updated = await store.MarkFailedAsync(
                new EmailOutboxFailedUpdate(
                    record.Id,
                    record.LockId,
                    attemptCount,
                    failureKind,
                    error,
                    nextAttemptUtc,
                    isDeadLetter,
                    failedUtc),
                cancellationToken);

            if (updated)
            {
                if (isDeadLetter)
                {
                    result.DeadLetterCount++;
                }
                else
                {
                    result.RetryCount++;
                }
            }

            logger.DeliveryFailed(exception, record.Id, failureKind, isDeadLetter);
        }

        private async Task Release(
            IReadOnlyCollection<ClaimedEmailOutboxRecord> records,
            MutableBatchResult result,
            CancellationToken cancellationToken)
        {
            if (records.Count == 0)
            {
                return;
            }

            int released = await store.ReleaseAsync(records, DateTime.UtcNow, cancellationToken);
            result.ReleasedCount += released;
            logger.RecordsReleased(released);
        }

        private TimeSpan CalculateRetryDelay(int attemptCount)
        {
            double multiplier = Math.Pow(2, Math.Max(0, attemptCount - 1));
            double seconds = Math.Min(
                _options.BaseRetryDelaySeconds * multiplier,
                _options.MaxRetryDelaySeconds);

            return TimeSpan.FromSeconds(seconds);
        }

        private static bool IsPermanent(EmailOutboxFailureKind failureKind)
        {
            return failureKind is EmailOutboxFailureKind.PermanentSmtp or
                EmailOutboxFailureKind.Serialization or
                EmailOutboxFailureKind.Validation;
        }

        private static string Truncate(string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value[..maxLength];
        }

        private sealed class MutableBatchResult(int claimedCount)
        {
            public int ClaimedCount { get; } = claimedCount;
            public int SentCount { get; set; }
            public int RetryCount { get; set; }
            public int DeadLetterCount { get; set; }
            public int ReleasedCount { get; set; }

            public EmailOutboxBatchResult ToResult()
            {
                return new EmailOutboxBatchResult(
                    ClaimedCount,
                    SentCount,
                    RetryCount,
                    DeadLetterCount,
                    ReleasedCount);
            }
        }
    }
}
