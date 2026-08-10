using Email.Models;
using FluentValidation;
using Email.Outbox.Models;
using Email.Outbox.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Modules.Notifications.Models;
using Application.Modules.Notifications.Logging;
using Application.Modules.Notifications.Validation;

namespace Application.Modules.Notifications.Services
{
    public sealed class ApplicationEmailService(
        IEmailOutbox emailOutbox,
        IValidator<ApplicationEmailRequest> validator,
        ILogger<ApplicationEmailService> logger)
        : IApplicationEmailService
    {
        public async Task<QueuedEmailResult> QueueAsync(ApplicationEmailRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                ApplicationEmailMetadata metadata = request.Metadata ?? new ApplicationEmailMetadata();
                EmailOutboxEnqueueResult result = await emailOutbox.EnqueueAsync(
                    new EmailOutboxRequest
                    {
                        Message = request.Message,
                        IdempotencyKey = metadata.IdempotencyKey,
                        AccountReference = metadata.AccountReference,
                        CorrelationId = metadata.CorrelationId
                    },
                    cancellationToken);

                if (result.WasDuplicate)
                {
                    logger.EmailQueueDuplicate(result.Id);
                }
                else
                {
                    logger.EmailQueued(result.Id);
                }

                return new QueuedEmailResult(result.Id, result.WasDuplicate, result.CreatedUtc);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.EmailQueueFailed(exception);
                throw;
            }
        }

        public Task<QueuedEmailResult> QueueAsync(
            EmailMessage message,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default)
        {
            return QueueAsync(
                new ApplicationEmailRequest
                {
                    Message = message,
                    Metadata = metadata ?? new ApplicationEmailMetadata()
                },
                cancellationToken);
        }

        public Task<QueuedEmailResult> QueueAsync(
            string recipientAddress,
            string subject,
            string textBody,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default)
        {
            return QueueAsync(
                CreateMessage([recipientAddress], subject, textBody, null, null),
                metadata,
                cancellationToken);
        }

        public Task<QueuedEmailResult> QueueAsync(
            IEnumerable<string> recipientAddresses,
            string subject,
            string textBody,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default)
        {
            return QueueAsync(
                CreateMessage(recipientAddresses, subject, textBody, null, null),
                metadata,
                cancellationToken);
        }

        public Task<QueuedEmailResult> QueueAsync(
            string recipientAddress,
            string subject,
            string textBody,
            IEnumerable<string> attachmentFilePaths,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(attachmentFilePaths);

            return QueueAsync(
                CreateMessage(
                    [recipientAddress],
                    subject,
                    textBody,
                    null,
                    attachmentFilePaths.Select(filePath => new EmailAttachment(filePath))),
                metadata,
                cancellationToken);
        }

        public Task<QueuedEmailResult> QueueAsync(
            string recipientAddress,
            string subject,
            string textBody,
            IEnumerable<EmailAttachment> attachments,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default)
        {
            return QueueAsync(
                CreateMessage([recipientAddress], subject, textBody, null, attachments),
                metadata,
                cancellationToken);
        }

        public Task<QueuedEmailResult> QueueHtmlAsync(
            string recipientAddress,
            string subject,
            string htmlBody,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default)
        {
            return QueueAsync(
                CreateMessage([recipientAddress], subject, null, htmlBody, null),
                metadata,
                cancellationToken);
        }

        public Task<QueuedEmailResult> QueueHtmlAsync(
            IEnumerable<string> recipientAddresses,
            string subject,
            string htmlBody,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default)
        {
            return QueueAsync(
                CreateMessage(recipientAddresses, subject, null, htmlBody, null),
                metadata,
                cancellationToken);
        }

        public Task<QueuedEmailResult> QueueHtmlAsync(
            string recipientAddress,
            string subject,
            string htmlBody,
            IEnumerable<string> attachmentFilePaths,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(attachmentFilePaths);

            return QueueAsync(
                CreateMessage(
                    [recipientAddress],
                    subject,
                    null,
                    htmlBody,
                    attachmentFilePaths.Select(filePath => new EmailAttachment(filePath))),
                metadata,
                cancellationToken);
        }

        public Task<QueuedEmailResult> QueueHtmlAsync(
            string recipientAddress,
            string subject,
            string htmlBody,
            IEnumerable<EmailAttachment> attachments,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default)
        {
            return QueueAsync(
                CreateMessage([recipientAddress], subject, null, htmlBody, attachments),
                metadata,
                cancellationToken);
        }

        public async Task<QueuedEmailResult> QueueHtmlTemplateAsync(
            string recipientAddress,
            string subject,
            string htmlTemplateFilePath,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default)
        {
            string htmlBody = await ReadHtmlTemplate(htmlTemplateFilePath, cancellationToken);

            return await QueueHtmlAsync(
                recipientAddress,
                subject,
                htmlBody,
                metadata,
                cancellationToken);
        }

        public async Task<QueuedEmailResult> QueueHtmlTemplateAsync(
            string recipientAddress,
            string subject,
            string htmlTemplateFilePath,
            IEnumerable<string> attachmentFilePaths,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default)
        {
            string htmlBody = await ReadHtmlTemplate(htmlTemplateFilePath, cancellationToken);

            return await QueueHtmlAsync(
                recipientAddress,
                subject,
                htmlBody,
                attachmentFilePaths,
                metadata,
                cancellationToken);
        }

        private static EmailMessage CreateMessage(
            IEnumerable<string> recipientAddresses,
            string subject,
            string? textBody,
            string? htmlBody,
            IEnumerable<EmailAttachment>? attachments)
        {
            ArgumentNullException.ThrowIfNull(recipientAddresses);

            return new EmailMessage
            {
                To = recipientAddresses
                    .Select(address => new EmailAddress(address))
                    .ToArray(),
                Subject = subject,
                TextBody = textBody,
                HtmlBody = htmlBody,
                Attachments = attachments?.ToArray() ?? Array.Empty<EmailAttachment>()
            };
        }

        private static async Task<string> ReadHtmlTemplate(string htmlTemplateFilePath, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(htmlTemplateFilePath);

            return await File.ReadAllTextAsync(htmlTemplateFilePath, cancellationToken);
        }
    }
}
