using Email.Models;
using System.Diagnostics;
using Email.Outbox.Models;
using Email.Outbox.Constants;
using Email.Outbox.Telemetry;
using Email.Outbox.Abstractions;
using Email.Outbox.Serialization;

namespace Email.Outbox.Services
{
    internal sealed class EmailOutboxService(IEmailOutboxStore store, IEmailOutboxSerializer serializer)
        : IEmailOutbox
    {
        public Task<EmailOutboxEnqueueResult> EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            return EnqueueAsync(
                new EmailOutboxRequest { Message = message },
                cancellationToken);
        }

        public async Task<EmailOutboxEnqueueResult> EnqueueAsync(EmailOutboxRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Message);

            using Activity? activity = EmailOutboxTelemetry.ActivitySource.StartActivity("email.outbox.enqueue");
            Activity? traceActivity = activity ?? Activity.Current;
            string? traceParent = traceActivity?.IdFormat == ActivityIdFormat.W3C
                ? traceActivity.Id
                : null;
            string? traceState = traceActivity?.IdFormat == ActivityIdFormat.W3C
                ? traceActivity.TraceStateString
                : null;

            try
            {
                string payload = await serializer.SerializeAsync(request.Message, cancellationToken);
                DateTime utcNow = DateTime.UtcNow;
                string profileName = Normalize(
                    request.ProfileName,
                    EmailOutboxConstants.MaxProfileNameLength,
                    nameof(request.ProfileName)) ?? EmailOutboxConstants.DefaultProfileName;
                var record = new EmailOutboxRecord(
                    Guid.NewGuid(),
                    payload,
                    Normalize(
                        request.IdempotencyKey,
                        EmailOutboxConstants.MaxIdempotencyKeyLength,
                        nameof(request.IdempotencyKey)),
                    Normalize(
                        request.AccountReference,
                        EmailOutboxConstants.MaxReferenceLength,
                        nameof(request.AccountReference)),
                    Normalize(
                        request.CorrelationId,
                        EmailOutboxConstants.MaxReferenceLength,
                        nameof(request.CorrelationId)),
                    0,
                    utcNow,
                    utcNow,
                    profileName,
                    traceParent,
                    traceState);

                EmailOutboxEnqueueResult result = await store.EnqueueAsync(record, cancellationToken);
                string outcome = result.WasDuplicate ? "duplicate" : "enqueued";
                activity?.SetTag(EmailOutboxTelemetry.OutcomeTag, outcome);
                TagList tags = new()
                {
                    { EmailOutboxTelemetry.ProfileTag, profileName },
                    { EmailOutboxTelemetry.OutcomeTag, outcome }
                };

                if (result.WasDuplicate)
                {
                    EmailOutboxTelemetry.Duplicates.Add(1, tags);
                }
                else
                {
                    EmailOutboxTelemetry.Enqueued.Add(1, tags);
                }

                return result;
            }
            catch (Exception exception)
            {
                activity?.SetTag(EmailOutboxTelemetry.OutcomeTag, "failure");
                activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
                throw;
            }
        }

        private static string? Normalize(string? value, int maxLength, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string normalized = value.Trim();

            if (normalized.Length > maxLength)
            {
                throw new ArgumentException(
                    $"The value cannot exceed {maxLength} characters.",
                    parameterName);
            }

            return normalized;
        }
    }
}
