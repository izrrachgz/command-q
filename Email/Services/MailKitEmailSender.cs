using Email.Models;
using Email.Logging;
using Email.Exceptions;
using Email.Abstractions;
using Email.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Email.Services
{
    internal sealed class MailKitEmailSender : IEmailSender
    {
        private readonly EmailOptions _options;
        private readonly ILogger<MailKitEmailSender> _logger;
        private readonly MimeMessageFactory _messageFactory;
        private readonly EmailMessageValidator _messageValidator;
        private readonly ISmtpTransportFactory _transportFactory;

        public MailKitEmailSender(
            IOptions<EmailOptions> options,
            ISmtpTransportFactory transportFactory,
            MimeMessageFactory messageFactory,
            EmailMessageValidator messageValidator,
            ILogger<MailKitEmailSender> logger)
        {
            _options = options.Value;
            _transportFactory = transportFactory;
            _messageFactory = messageFactory;
            _messageValidator = messageValidator;
            _logger = logger;
        }

        public async Task<EmailSendResult> SendAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            var batchResult = await SendAsync(new[] { message }, cancellationToken);
            return batchResult.Messages[0];
        }

        public async Task<EmailBatchResult> SendAsync(
            IEnumerable<EmailMessage> messages,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(messages);

            var batch = messages.ToList();

            if (batch.Count == 0)
            {
                return EmailBatchResult.Empty;
            }

            if (!_options.Enabled)
            {
                throw EmailDeliveryException.Disabled(batch.Count);
            }

            foreach (var message in batch)
            {
                _messageValidator.Validate(message, _options.Sender, _options.DeliveryPolicy);
            }

            using var transport = _transportFactory.Create(_options.Smtp);
            var sentMessages = new List<EmailSendResult>(batch.Count);
            string? currentMessageId = null;

            try
            {
                _logger.BatchStarting(_options.Smtp.Host, _options.Smtp.Port, batch.Count);

                await transport.ConnectAsync(
                    _options.Smtp.Host,
                    _options.Smtp.Port,
                    _options.Smtp.Security,
                    cancellationToken);

                _logger.SmtpConnected();

                if (_options.Authentication.Type != EmailAuthenticationType.Anonymous)
                {
                    await transport.AuthenticateAsync(
                        _options.Authentication,
                        cancellationToken);

                    _logger.SmtpAuthenticated();
                }

                foreach (var source in batch)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using var message = _messageFactory.Create(source, _options.Sender, cancellationToken);
                    var messageId = message.MessageId ?? throw new InvalidOperationException(
                        "The MIME message factory did not assign a message ID.");
                    currentMessageId = messageId;

                    var smtpResponse = await transport.SendAsync(message, cancellationToken);
                    var result = new EmailSendResult(
                        sentMessages.Count + 1,
                        messageId,
                        smtpResponse,
                        DateTimeOffset.UtcNow);

                    sentMessages.Add(result);
                    _logger.MessageSent(sentMessages.Count, batch.Count);
                    currentMessageId = null;
                }

                _logger.BatchCompleted(sentMessages.Count);

                return new EmailBatchResult(sentMessages.ToArray());
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.BatchCancelled(sentMessages.Count, batch.Count);
                throw;
            }
            catch (Exception exception)
            {
                var failureKind = EmailFailureClassifier.Classify(exception);

                _logger.BatchFailed(exception, failureKind, sentMessages.Count, batch.Count);

                throw new EmailDeliveryException(
                    "Email delivery failed. Inspect the failure classification and inner exception for details.",
                    failureKind,
                    batch.Count,
                    sentMessages.ToArray(),
                    sentMessages.Count + 1,
                    currentMessageId,
                    exception);
            }
            finally
            {
                await DisconnectSafely(transport);
            }
        }

        public async Task<EmailSendResult> SendAsync(
            string recipientAddress,
            string subject,
            string textBody,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync(
                CreateMessage(new[] { recipientAddress }, subject, textBody, null, null),
                cancellationToken);
        }

        public async Task<EmailSendResult> SendAsync(
            IEnumerable<string> recipientAddresses,
            string subject,
            string textBody,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync(
                CreateMessage(recipientAddresses, subject, textBody, null, null),
                cancellationToken);
        }

        public async Task<EmailSendResult> SendAsync(
            string recipientAddress,
            string subject,
            string textBody,
            IEnumerable<string> attachmentFilePaths,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(attachmentFilePaths);

            return await SendAsync(
                CreateMessage(
                    new[] { recipientAddress },
                    subject,
                    textBody,
                    null,
                    attachmentFilePaths.Select(filePath => new EmailAttachment(filePath))),
                cancellationToken);
        }

        public async Task<EmailSendResult> SendAsync(
            string recipientAddress,
            string subject,
            string textBody,
            IEnumerable<EmailAttachment> attachments,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync(
                CreateMessage(new[] { recipientAddress }, subject, textBody, null, attachments),
                cancellationToken);
        }

        public async Task<EmailSendResult> SendHtmlAsync(
            string recipientAddress,
            string subject,
            string htmlBody,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync(
                CreateMessage(new[] { recipientAddress }, subject, null, htmlBody, null),
                cancellationToken);
        }

        public async Task<EmailSendResult> SendHtmlAsync(
            IEnumerable<string> recipientAddresses,
            string subject,
            string htmlBody,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync(
                CreateMessage(recipientAddresses, subject, null, htmlBody, null),
                cancellationToken);
        }

        public async Task<EmailSendResult> SendHtmlAsync(
            string recipientAddress,
            string subject,
            string htmlBody,
            IEnumerable<string> attachmentFilePaths,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(attachmentFilePaths);

            return await SendAsync(
                CreateMessage(
                    new[] { recipientAddress },
                    subject,
                    null,
                    htmlBody,
                    attachmentFilePaths.Select(filePath => new EmailAttachment(filePath))),
                cancellationToken);
        }

        public async Task<EmailSendResult> SendHtmlAsync(
            string recipientAddress,
            string subject,
            string htmlBody,
            IEnumerable<EmailAttachment> attachments,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync(
                CreateMessage(new[] { recipientAddress }, subject, null, htmlBody, attachments),
                cancellationToken);
        }

        public async Task<EmailSendResult> SendHtmlTemplateAsync(
            string recipientAddress,
            string subject,
            string htmlTemplateFilePath,
            CancellationToken cancellationToken = default)
        {
            var htmlBody = await ReadHtmlTemplate(htmlTemplateFilePath, cancellationToken);
            return await SendHtmlAsync(recipientAddress, subject, htmlBody, cancellationToken);
        }

        public async Task<EmailSendResult> SendHtmlTemplateAsync(
            string recipientAddress,
            string subject,
            string htmlTemplateFilePath,
            IEnumerable<string> attachmentFilePaths,
            CancellationToken cancellationToken = default)
        {
            var htmlBody = await ReadHtmlTemplate(htmlTemplateFilePath, cancellationToken);
            return await SendHtmlAsync(
                recipientAddress,
                subject,
                htmlBody,
                attachmentFilePaths,
                cancellationToken);
        }

        private async Task DisconnectSafely(ISmtpTransport transport)
        {
            if (!transport.IsConnected)
            {
                return;
            }

            try
            {
                await transport.DisconnectAsync(quit: true, CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.DisconnectFailed(exception);
            }
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
                To = recipientAddresses.Select(address => new EmailAddress(address)).ToArray(),
                Subject = subject,
                TextBody = textBody,
                HtmlBody = htmlBody,
                Attachments = attachments?.ToArray() ?? Array.Empty<EmailAttachment>()
            };
        }

        private static async Task<string> ReadHtmlTemplate(
            string htmlTemplateFilePath,
            CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(htmlTemplateFilePath);

            return await File.ReadAllTextAsync(htmlTemplateFilePath, cancellationToken);
        }
    }
}
