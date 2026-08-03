using Email.Models;

namespace Email.Abstractions
{
    /// <summary>
    /// Sends fully prepared email messages through the configured SMTP endpoint.
    /// </summary>
    public interface IEmailSender
    {
        Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends all messages sequentially over one SMTP connection and authentication session.
        /// </summary>
        Task<EmailBatchResult> SendAsync(IEnumerable<EmailMessage> messages, CancellationToken cancellationToken = default);

        Task<EmailSendResult> SendAsync(
            string recipientAddress,
            string subject,
            string textBody,
            CancellationToken cancellationToken = default);

        Task<EmailSendResult> SendAsync(
            IEnumerable<string> recipientAddresses,
            string subject,
            string textBody,
            CancellationToken cancellationToken = default);

        Task<EmailSendResult> SendAsync(
            string recipientAddress,
            string subject,
            string textBody,
            IEnumerable<string> attachmentFilePaths,
            CancellationToken cancellationToken = default);

        Task<EmailSendResult> SendAsync(
            string recipientAddress,
            string subject,
            string textBody,
            IEnumerable<EmailAttachment> attachments,
            CancellationToken cancellationToken = default);

        Task<EmailSendResult> SendHtmlAsync(
            string recipientAddress,
            string subject,
            string htmlBody,
            CancellationToken cancellationToken = default);

        Task<EmailSendResult> SendHtmlAsync(
            IEnumerable<string> recipientAddresses,
            string subject,
            string htmlBody,
            CancellationToken cancellationToken = default);

        Task<EmailSendResult> SendHtmlAsync(
            string recipientAddress,
            string subject,
            string htmlBody,
            IEnumerable<string> attachmentFilePaths,
            CancellationToken cancellationToken = default);

        Task<EmailSendResult> SendHtmlAsync(
            string recipientAddress,
            string subject,
            string htmlBody,
            IEnumerable<EmailAttachment> attachments,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads an already-rendered HTML file verbatim and sends it as the message body.
        /// </summary>
        Task<EmailSendResult> SendHtmlTemplateAsync(
            string recipientAddress,
            string subject,
            string htmlTemplateFilePath,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads an already-rendered HTML file verbatim and sends it with file-system attachments.
        /// </summary>
        Task<EmailSendResult> SendHtmlTemplateAsync(
            string recipientAddress,
            string subject,
            string htmlTemplateFilePath,
            IEnumerable<string> attachmentFilePaths,
            CancellationToken cancellationToken = default);
    }
}
