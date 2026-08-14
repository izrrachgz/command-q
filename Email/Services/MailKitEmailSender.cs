using MimeKit;
using Email.Models;
using Email.Logging;
using Email.Constants;
using Email.Telemetry;
using Email.Exceptions;
using System.Diagnostics;
using Email.Abstractions;
using Email.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Email.Services
{
    internal sealed class MailKitEmailSender : IEmailSender, IDisposable
    {
        private readonly string _profileName;
        private readonly IOptionsMonitor<EmailOptions> _options;
        private readonly ILogger<MailKitEmailSender> _logger;
        private readonly MimeMessageFactory _messageFactory;
        private readonly EmailMessageValidator _messageValidator;
        private readonly MimeMessageSizeCalculator _messageSizeCalculator;
        private readonly ISmtpTransportFactory _transportFactory;
        private readonly IDisposable? _configurationSubscription;

        public MailKitEmailSender(
            IOptionsMonitor<EmailOptions> options,
            ISmtpTransportFactory transportFactory,
            MimeMessageFactory messageFactory,
            EmailMessageValidator messageValidator,
            ILogger<MailKitEmailSender> logger,
            MimeMessageSizeCalculator? messageSizeCalculator = null)
            : this(
                EmailConstants.DefaultProfileName,
                options,
                transportFactory,
                messageFactory,
                messageValidator,
                logger,
                messageSizeCalculator)
        {
        }

        public MailKitEmailSender(
            string profileName,
            IOptionsMonitor<EmailOptions> options,
            ISmtpTransportFactory transportFactory,
            MimeMessageFactory messageFactory,
            EmailMessageValidator messageValidator,
            ILogger<MailKitEmailSender> logger,
            MimeMessageSizeCalculator? messageSizeCalculator = null)
        {
            _profileName = profileName;
            _options = options;
            _transportFactory = transportFactory;
            _messageFactory = messageFactory;
            _messageValidator = messageValidator;
            _messageSizeCalculator = messageSizeCalculator ?? new MimeMessageSizeCalculator();
            _logger = logger;
            _configurationSubscription = options.OnChange(LogConfigurationReloaded);
        }

        public void Dispose()
        {
            _configurationSubscription?.Dispose();
        }

        public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            var batchResult = await SendAsync(new[] { message }, cancellationToken);
            return batchResult.Messages[0];
        }

        public async Task<EmailBatchResult> SendAsync(IEnumerable<EmailMessage> messages, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(messages);

            var batch = messages.ToList();

            if (batch.Count == 0)
            {
                return EmailBatchResult.Empty;
            }

            EmailOptions emailOptions = _options.CurrentValue;
            (string profileName, EmailProfileOptions options) = emailOptions.ResolveProfile(_profileName);
            var tags = new TagList { { EmailTelemetry.ProfileTag, profileName } };
            long startedAt = Stopwatch.GetTimestamp();
            using Activity? sendActivity = EmailTelemetry.ActivitySource.StartActivity("email.send");
            sendActivity?.SetTag(EmailTelemetry.ProfileTag, profileName);

            if (!emailOptions.Enabled || !options.Enabled)
            {
                sendActivity?.SetTag(EmailTelemetry.OutcomeTag, "disabled");
                TagList failureTags = tags;
                failureTags.Add(EmailTelemetry.FailureCategoryTag, EmailFailureKind.Disabled.ToString());
                failureTags.Add(EmailTelemetry.OutcomeTag, "disabled");
                EmailTelemetry.MessagesFailed.Add(batch.Count, failureTags);
                throw EmailDeliveryException.Disabled(batch.Count);
            }

            IReadOnlyList<MimeMessage> mimeMessages = await BuildMimeMessagesAsync(
                batch,
                options,
                profileName,
                cancellationToken);
            var sentMessages = new List<EmailSendResult>(batch.Count);
            string? currentMessageId = null;
            using var transport = _transportFactory.Create(options.Smtp);

            try
            {
                _logger.BatchStarting(options.Smtp.Host, options.Smtp.Port, batch.Count);

                long connectStartedAt = Stopwatch.GetTimestamp();
                using (Activity? activity = StartActivity("email.smtp.connect", profileName))
                {
                    try
                    {
                        await transport.ConnectAsync(
                            options.Smtp.Host,
                            options.Smtp.Port,
                            options.Smtp.Security,
                            cancellationToken);
                        CompleteActivity(activity, "success");
                    }
                    catch (Exception exception)
                    {
                        CompleteActivity(activity, "failure", exception, EmailFailureClassifier.Classify(exception).ToString());
                        throw;
                    }
                    finally
                    {
                        EmailTelemetry.SmtpConnectionDuration.Record(
                            Stopwatch.GetElapsedTime(connectStartedAt).TotalSeconds,
                            tags);
                    }
                }

                _logger.SmtpConnected();

                if (options.Authentication.Type != EmailAuthenticationType.Anonymous)
                {
                    using Activity? activity = StartActivity("email.smtp.authenticate", profileName);
                    try
                    {
                        await transport.AuthenticateAsync(options.Authentication, cancellationToken);
                        CompleteActivity(activity, "success");
                    }
                    catch (Exception exception)
                    {
                        CompleteActivity(activity, "failure", exception, EmailFailureClassifier.Classify(exception).ToString());
                        throw;
                    }

                    _logger.SmtpAuthenticated();
                }

                await transport.NoOpAsync(cancellationToken);

                foreach (MimeMessage message in mimeMessages)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var messageId = message.MessageId ?? throw new InvalidOperationException(
                        "The MIME message factory did not assign a message ID.");
                    currentMessageId = messageId;

                    string smtpResponse;
                    using (Activity? activity = StartActivity("email.smtp.send", profileName))
                    {
                        try
                        {
                            smtpResponse = await transport.SendAsync(message, cancellationToken);
                            CompleteActivity(activity, "success");
                        }
                        catch (Exception exception)
                        {
                            CompleteActivity(activity, "failure", exception, EmailFailureClassifier.Classify(exception).ToString());
                            throw;
                        }
                    }

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
                sendActivity?.SetTag(EmailTelemetry.OutcomeTag, "success");
                EmailTelemetry.MessagesSent.Add(sentMessages.Count, tags);

                return new EmailBatchResult(sentMessages.ToArray());
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.BatchCancelled(sentMessages.Count, batch.Count);
                sendActivity?.SetTag(EmailTelemetry.OutcomeTag, "cancelled");
                throw;
            }
            catch (Exception exception)
            {
                var failureKind = EmailFailureClassifier.Classify(exception);

                _logger.BatchFailed(exception, failureKind, sentMessages.Count, batch.Count);
                sendActivity?.SetTag(EmailTelemetry.OutcomeTag, "failure");
                sendActivity?.SetTag(EmailTelemetry.FailureCategoryTag, failureKind.ToString());
                sendActivity?.SetStatus(ActivityStatusCode.Error, failureKind.ToString());
                TagList failureTags = tags;
                failureTags.Add(EmailTelemetry.FailureCategoryTag, failureKind.ToString());
                failureTags.Add(EmailTelemetry.OutcomeTag, "failure");
                EmailTelemetry.MessagesSent.Add(sentMessages.Count, tags);
                EmailTelemetry.MessagesFailed.Add(batch.Count - sentMessages.Count, failureTags);

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
                foreach (MimeMessage message in mimeMessages)
                {
                    message.Dispose();
                }

                await DisconnectSafely(transport);
                EmailTelemetry.SendDuration.Record(Stopwatch.GetElapsedTime(startedAt).TotalSeconds, tags);
            }
        }

        private async Task<IReadOnlyList<MimeMessage>> BuildMimeMessagesAsync(
            IReadOnlyList<EmailMessage> batch,
            EmailProfileOptions options,
            string profileName,
            CancellationToken cancellationToken)
        {
            var mimeMessages = new List<MimeMessage>(batch.Count);

            try
            {
                foreach (EmailMessage source in batch)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _messageValidator.Validate(source, options.Sender, options.DeliveryPolicy);

                    MimeMessage? message = null;
                    using (Activity? activity = StartActivity("email.mime.build", profileName))
                    {
                        try
                        {
                            message = _messageFactory.Create(source, options.Sender, cancellationToken);
                            long messageBytes = await _messageSizeCalculator.MeasureAsync(message, cancellationToken);

                            if (messageBytes > options.DeliveryPolicy.MaxMessageBytes)
                            {
                                throw new ArgumentException(
                                    $"The serialized MIME message is {messageBytes} bytes and exceeds the configured limit of " +
                                    $"{options.DeliveryPolicy.MaxMessageBytes} bytes.",
                                    nameof(source));
                            }

                            CompleteActivity(activity, "success");
                        }
                        catch (Exception exception)
                        {
                            CompleteActivity(activity, "failure", exception, "mime");
                            message?.Dispose();
                            throw;
                        }
                    }

                    mimeMessages.Add(message);
                }

                return mimeMessages;
            }
            catch
            {
                foreach (MimeMessage message in mimeMessages)
                {
                    message.Dispose();
                }

                throw;
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
                using Activity? activity = StartActivity("email.smtp.disconnect", _profileName);
                try
                {
                    await transport.DisconnectAsync(quit: true, CancellationToken.None);
                    CompleteActivity(activity, "success");
                }
                catch (Exception exception)
                {
                    CompleteActivity(activity, "failure", exception, EmailFailureClassifier.Classify(exception).ToString());
                    throw;
                }
            }
            catch (Exception exception)
            {
                _logger.DisconnectFailed(exception);
            }
        }

        private static Activity? StartActivity(string name, string profile)
        {
            var activity = EmailTelemetry.ActivitySource.StartActivity(name);
            activity?.SetTag(EmailTelemetry.ProfileTag, profile);
            return activity;
        }

        private static void CompleteActivity(
            Activity? activity,
            string outcome,
            Exception? exception = null,
            string? failureCategory = null)
        {
            if (activity is null)
            {
                return;
            }

            activity.SetTag(EmailTelemetry.OutcomeTag, outcome);

            if (failureCategory is not null)
            {
                activity.SetTag(EmailTelemetry.FailureCategoryTag, failureCategory);
            }

            if (exception is not null)
            {
                activity.SetStatus(ActivityStatusCode.Error, failureCategory);
            }
        }

        private void LogConfigurationReloaded(EmailOptions options, string? name)
        {
            try
            {
                (string profileName, EmailProfileOptions profile) = options.ResolveProfile(_profileName);
                _logger.ConfigurationReloaded(
                    profileName,
                    profile.Smtp.Host,
                    profile.Smtp.Port,
                    profile.Authentication.Type,
                    options.Enabled && profile.Enabled,
                    !string.IsNullOrWhiteSpace(profile.Sender.Address));
            }
            catch (EmailProfileNotFoundException)
            {
                // The next send will reject the deleted profile and the worker will dead-letter its records.
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

        private static async Task<string> ReadHtmlTemplate(string htmlTemplateFilePath, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(htmlTemplateFilePath);

            return await File.ReadAllTextAsync(htmlTemplateFilePath, cancellationToken);
        }
    }
}
