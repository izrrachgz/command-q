using Email.Models;
using Email.Outbox.Models;
using Email.Outbox.Constants;
using Email.Outbox.Abstractions;
using Email.Outbox.Serialization;

namespace Email.Outbox.Services
{
    internal sealed class EmailOutboxService(
        IEmailOutboxStore store,
        IEmailOutboxSerializer serializer)
        : IEmailOutbox
    {
        public Task<EmailOutboxEnqueueResult> EnqueueAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default)
        {
            return EnqueueAsync(
                new EmailOutboxRequest { Message = message },
                cancellationToken);
        }

        public async Task<EmailOutboxEnqueueResult> EnqueueAsync(
            EmailOutboxRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Message);

            string payload = await serializer.SerializeAsync(request.Message, cancellationToken);
            DateTime utcNow = DateTime.UtcNow;
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
                utcNow);

            return await store.EnqueueAsync(record, cancellationToken);
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
