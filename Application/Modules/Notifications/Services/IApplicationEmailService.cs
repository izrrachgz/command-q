using Email.Client.Models;
using Application.Modules.Notifications.Models;

namespace Application.Modules.Notifications.Services
{
    public interface IApplicationEmailService
    {
        Task<QueuedEmailResult> QueueAsync(ApplicationEmailRequest request, CancellationToken cancellationToken = default);

        Task<QueuedEmailResult> QueueAsync(
            EmailMessage message,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default);

        Task<QueuedEmailResult> QueueAsync(
            string recipientAddress,
            string subject,
            string textBody,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default);

        Task<QueuedEmailResult> QueueAsync(
            IEnumerable<string> recipientAddresses,
            string subject,
            string textBody,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default);

        Task<QueuedEmailResult> QueueAsync(
            string recipientAddress,
            string subject,
            string textBody,
            IEnumerable<string> attachmentFilePaths,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default);

        Task<QueuedEmailResult> QueueAsync(
            string recipientAddress,
            string subject,
            string textBody,
            IEnumerable<EmailAttachment> attachments,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default);

        Task<QueuedEmailResult> QueueHtmlAsync(
            string recipientAddress,
            string subject,
            string htmlBody,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default);

        Task<QueuedEmailResult> QueueHtmlAsync(
            IEnumerable<string> recipientAddresses,
            string subject,
            string htmlBody,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default);

        Task<QueuedEmailResult> QueueHtmlAsync(
            string recipientAddress,
            string subject,
            string htmlBody,
            IEnumerable<string> attachmentFilePaths,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default);

        Task<QueuedEmailResult> QueueHtmlAsync(
            string recipientAddress,
            string subject,
            string htmlBody,
            IEnumerable<EmailAttachment> attachments,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default);

        Task<QueuedEmailResult> QueueHtmlTemplateAsync(
            string recipientAddress,
            string subject,
            string htmlTemplateFilePath,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default);

        Task<QueuedEmailResult> QueueHtmlTemplateAsync(
            string recipientAddress,
            string subject,
            string htmlTemplateFilePath,
            IEnumerable<string> attachmentFilePaths,
            ApplicationEmailMetadata? metadata = null,
            CancellationToken cancellationToken = default);
    }
}
