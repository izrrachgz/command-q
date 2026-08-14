using Email.Models;
using Email.Exceptions;
using System.Diagnostics;
using Email.Abstractions;
using Email.Outbox.Models;
using Email.Outbox.Logging;
using Email.Outbox.Telemetry;
using Email.Outbox.Abstractions;
using Email.Outbox.Configuration;
using Email.Outbox.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Email.Outbox.Services
{
    internal sealed class EmailOutboxProcessor(
        IEmailOutboxStore store,
        IEmailSenderFactory senderFactory,
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
            using Activity? activity = EmailOutboxTelemetry.ActivitySource.StartActivity("email.outbox.process");
            IReadOnlyList<ClaimedEmailOutboxRecord> claimed;

            try
            {
                claimed = await store.ClaimPendingAsync(request, cancellationToken);
            }
            catch (Exception exception)
            {
                activity?.SetTag(EmailOutboxTelemetry.OutcomeTag, "failure");
                activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
                throw;
            }

            logger.BatchClaimed(claimed.Count);

            if (claimed.Count == 0)
            {
                activity?.SetTag(EmailOutboxTelemetry.OutcomeTag, "empty");
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
                EmailOutboxBatchResult emptyDeliveryResult = result.ToResult();
                activity?.SetTag(EmailOutboxTelemetry.OutcomeTag, "serialization_failure");
                return emptyDeliveryResult;
            }

            try
            {
                foreach (var profileBatch in deliverable
                    .GroupBy(item => item.Record.ProfileName, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.ToArray()))
                {
                    await ProcessProfileBatch(profileBatch, result, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await Release(deliverable.Select(item => item.Record).ToArray(), result, CancellationToken.None);
                throw;
            }

            logger.BatchSent(result.SentCount);
            EmailOutboxBatchResult batchResult = result.ToResult();
            activity?.SetTag(
                EmailOutboxTelemetry.OutcomeTag,
                batchResult.RetryCount > 0 || batchResult.DeadLetterCount > 0 ? "partial" : "success");
            return batchResult;
        }

        private async Task ProcessProfileBatch(
            IReadOnlyList<(ClaimedEmailOutboxRecord Record, EmailMessage Message)> profileBatch,
            MutableBatchResult result,
            CancellationToken cancellationToken)
        {
            string profileName = profileBatch[0].Record.ProfileName;
            using Activity? activity = EmailOutboxTelemetry.ActivitySource.StartActivity("email.outbox.profile.process");
            activity?.SetTag(EmailOutboxTelemetry.ProfileTag, profileName);
            AddTraceLinks(activity, profileBatch);

            IEmailSender sender;
            try
            {
                sender = senderFactory.Create(profileName);
            }
            catch (EmailProfileNotFoundException exception)
            {
                foreach ((ClaimedEmailOutboxRecord record, _) in profileBatch)
                {
                    await MarkFailure(
                        record,
                        EmailOutboxFailureKind.ProfileNotFound,
                        exception,
                        isPermanent: true,
                        result,
                        cancellationToken);
                }

                activity?.SetTag(EmailOutboxTelemetry.OutcomeTag, "dead_letter");
                return;
            }

            try
            {
                EmailBatchResult sendResult = await sender.SendAsync(
                    profileBatch.Select(item => item.Message),
                    cancellationToken);

                if (sendResult.Messages.Count != profileBatch.Count)
                {
                    throw new InvalidOperationException("The email sender returned a result count that does not match the outbox profile batch.");
                }

                for (var index = 0; index < profileBatch.Count; index++)
                {
                    await MarkSent(profileBatch[index].Record, sendResult.Messages[index], result, cancellationToken);
                }

                activity?.SetTag(EmailOutboxTelemetry.OutcomeTag, "success");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await Release(
                    profileBatch.Select(item => item.Record).ToArray(),
                    result,
                    CancellationToken.None);
                throw;
            }
            catch (EmailProfileNotFoundException exception)
            {
                foreach ((ClaimedEmailOutboxRecord record, _) in profileBatch)
                {
                    await MarkFailure(
                        record,
                        EmailOutboxFailureKind.ProfileNotFound,
                        exception,
                        isPermanent: true,
                        result,
                        cancellationToken);
                }

                activity?.SetTag(EmailOutboxTelemetry.OutcomeTag, "dead_letter");
            }
            catch (EmailDeliveryException exception)
            {
                int sentCount = Math.Min(exception.SentCount, profileBatch.Count);
                EmailOutboxFailureKind failureKind = EmailOutboxFailureMapper.Map(exception.FailureKind);

                for (var index = 0; index < sentCount; index++)
                {
                    await MarkSent(profileBatch[index].Record, exception.SentMessages[index], result, cancellationToken);
                }

                if (sentCount < profileBatch.Count)
                {
                    await MarkFailure(
                        profileBatch[sentCount].Record,
                        failureKind,
                        exception,
                        IsPermanent(failureKind),
                        result,
                        cancellationToken);

                    await Release(
                        profileBatch.Skip(sentCount + 1).Select(item => item.Record).ToArray(),
                        result,
                        cancellationToken);
                }

                activity?.SetTag(EmailOutboxTelemetry.OutcomeTag, "partial");
            }
            catch (Exception exception)
            {
                EmailOutboxFailureKind failureKind = exception is ArgumentException
                    ? EmailOutboxFailureKind.Validation
                    : EmailOutboxFailureKind.Unknown;
                await MarkFailure(
                    profileBatch[0].Record,
                    failureKind,
                    exception,
                    IsPermanent(failureKind),
                    result,
                    cancellationToken);
                await Release(
                    profileBatch.Skip(1).Select(item => item.Record).ToArray(),
                    result,
                    cancellationToken);
                activity?.SetTag(EmailOutboxTelemetry.OutcomeTag, "partial");
            }
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
            using Activity? failureActivity = EmailOutboxTelemetry.ActivitySource.StartActivity(
                isDeadLetter ? "email.outbox.dead_letter" : "email.outbox.retry");
            failureActivity?.SetTag(EmailOutboxTelemetry.ProfileTag, record.ProfileName);
            failureActivity?.SetTag(EmailOutboxTelemetry.FailureCategoryTag, failureKind.ToString());
            failureActivity?.SetTag(
                EmailOutboxTelemetry.OutcomeTag,
                isDeadLetter ? "dead_letter" : "retry");

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
                    EmailOutboxTelemetry.DeadLettered.Add(1, new TagList
                    {
                        { EmailOutboxTelemetry.ProfileTag, record.ProfileName },
                        { EmailOutboxTelemetry.FailureCategoryTag, failureKind.ToString() },
                        { EmailOutboxTelemetry.OutcomeTag, "dead_letter" }
                    });
                }
                else
                {
                    result.RetryCount++;
                    EmailOutboxTelemetry.Retried.Add(1, new TagList
                    {
                        { EmailOutboxTelemetry.ProfileTag, record.ProfileName },
                        { EmailOutboxTelemetry.FailureCategoryTag, failureKind.ToString() },
                        { EmailOutboxTelemetry.OutcomeTag, "retry" }
                    });
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

        private static void AddTraceLinks(
            Activity? activity,
            IReadOnlyList<(ClaimedEmailOutboxRecord Record, EmailMessage Message)> profileBatch)
        {
            if (activity is null)
            {
                return;
            }

            foreach ((ClaimedEmailOutboxRecord record, _) in profileBatch)
            {
                if (ActivityContext.TryParse(record.TraceParent, record.TraceState, out ActivityContext context))
                {
                    activity.AddLink(new ActivityLink(context));
                }
            }
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
                EmailOutboxFailureKind.Validation or
                EmailOutboxFailureKind.ProfileNotFound;
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
