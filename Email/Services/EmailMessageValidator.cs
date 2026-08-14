using MimeKit;
using Email.Models;
using Email.Configuration;

namespace Email.Services
{
    /// <summary>
    /// Validates email recipients, bodies and content against a delivery policy.
    /// </summary>
    public sealed class EmailMessageValidator
    {
        /// <summary>
        /// Validates a message for SMTP delivery, including the active sender policy.
        /// </summary>
        /// <param name="message">The message to validate.</param>
        /// <param name="defaultSender">The sender configured by the active profile.</param>
        /// <param name="policy">The active profile delivery policy.</param>
        public void Validate(EmailMessage message, EmailSenderOptions defaultSender, EmailDeliveryPolicyOptions policy)
        {
            ArgumentNullException.ThrowIfNull(message);
            ValidateContent(message, policy);
            ValidateSenderDomain(message.From?.Address ?? defaultSender.Address, policy.AllowedSenderDomains);
        }

        /// <summary>
        /// Validates message content before it is persisted in the outbox.
        /// </summary>
        /// <param name="message">The message to validate.</param>
        /// <param name="policy">The producer-side delivery policy.</param>
        public void ValidateContent(EmailMessage message, EmailDeliveryPolicyOptions policy)
        {
            ArgumentNullException.ThrowIfNull(message);

            var recipientCount = message.To.Count + message.Cc.Count + message.Bcc.Count;

            if (recipientCount == 0)
            {
                throw new ArgumentException("An email message requires at least one To, Cc or Bcc recipient.", nameof(message));
            }

            if (recipientCount > policy.MaxRecipients)
            {
                throw new ArgumentException(
                    $"An email message cannot have more than {policy.MaxRecipients} recipients.",
                    nameof(message));
            }

            if (string.IsNullOrWhiteSpace(message.TextBody) && string.IsNullOrWhiteSpace(message.HtmlBody))
            {
                throw new ArgumentException("An email message requires a text or HTML body.", nameof(message));
            }

            ValidateAddresses(message.To, nameof(message.To));
            ValidateAddresses(message.Cc, nameof(message.Cc));
            ValidateAddresses(message.Bcc, nameof(message.Bcc));
            ValidateAddresses(message.ReplyTo, nameof(message.ReplyTo));

            if (message.From is not null)
            {
                ValidateAddress(message.From, nameof(message.From));
            }

            var contentCount = message.Attachments.Count + message.InlineResources.Count;

            if (contentCount > policy.MaxAttachmentCount)
            {
                throw new ArgumentException(
                    $"An email message cannot have more than {policy.MaxAttachmentCount} attachments and inline resources.",
                    nameof(message));
            }

            long totalContentBytes = 0;

            foreach (var attachment in message.Attachments)
            {
                ValidateAttachmentContent(
                    attachment?.FilePath,
                    attachment?.FileName,
                    attachment?.ContentType,
                    attachment?.GetLength() ?? 0,
                    policy,
                    nameof(message));

                totalContentBytes += attachment!.GetLength();
            }

            if (message.InlineResources.Count > 0 && string.IsNullOrWhiteSpace(message.HtmlBody))
            {
                throw new ArgumentException("Inline resources require an HTML body.", nameof(message));
            }

            var contentIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var resource in message.InlineResources)
            {
                if (resource is null || string.IsNullOrWhiteSpace(resource.ContentId))
                {
                    throw new ArgumentException("Inline resource content IDs cannot be empty.", nameof(message));
                }

                if (!contentIds.Add(resource.ContentId))
                {
                    throw new ArgumentException("Inline resource content IDs must be unique within a message.", nameof(message));
                }

                ValidateAttachmentContent(
                    resource.FilePath,
                    resource.FileName,
                    resource.ContentType,
                    resource.GetLength(),
                    policy,
                    nameof(message));

                totalContentBytes += resource.GetLength();
            }

            if (totalContentBytes > policy.MaxTotalAttachmentBytes)
            {
                throw new ArgumentException(
                    $"Email attachment content cannot exceed {policy.MaxTotalAttachmentBytes} bytes in total.",
                    nameof(message));
            }
        }

        private static void ValidateAttachmentContent(
            string? filePath,
            string? fileName,
            string? contentType,
            long length,
            EmailDeliveryPolicyOptions policy,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Email content file names cannot be empty.", parameterName);
            }

            if (filePath is not null && !File.Exists(filePath))
            {
                throw new FileNotFoundException("Email content file was not found.", filePath);
            }

            if (!string.IsNullOrWhiteSpace(contentType) && !ContentType.TryParse(contentType, out _))
            {
                throw new ArgumentException("Email content types must be valid MIME content types.", parameterName);
            }

            if (length > policy.MaxAttachmentBytes)
            {
                throw new ArgumentException(
                    $"Each email attachment or inline resource cannot exceed {policy.MaxAttachmentBytes} bytes.",
                    parameterName);
            }

            string effectiveContentType = string.IsNullOrWhiteSpace(contentType)
                ? MimeTypes.GetMimeType(fileName)
                : contentType.Trim();
            if (ContentType.TryParse(effectiveContentType, out ContentType? parsedContentType) && parsedContentType is not null)
            {
                effectiveContentType = $"{parsedContentType.MediaType}/{parsedContentType.MediaSubtype}";
            }

            bool isUnknownContentType = string.Equals(
                effectiveContentType,
                "application/octet-stream",
                StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(contentType);

            if (isUnknownContentType && !policy.AllowUnknownContentTypes)
            {
                throw new ArgumentException(
                    $"Email content '{fileName}' has an unknown MIME content type.",
                    parameterName);
            }

            if (policy.AllowedAttachmentContentTypes is { Length: > 0 } &&
                (isUnknownContentType || !IsAllowedContentType(effectiveContentType, policy.AllowedAttachmentContentTypes)))
            {
                throw new ArgumentException(
                    $"Email content type '{effectiveContentType}' is not allowed by delivery policy.",
                    parameterName);
            }

            if (policy.AllowedAttachmentExtensions is { Length: > 0 })
            {
                string extension = Path.GetExtension(fileName);
                if (!policy.AllowedAttachmentExtensions.Any(allowed =>
                    string.Equals(allowed.Trim(), extension, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException(
                        $"Email content extension for '{fileName}' is not allowed by delivery policy.",
                        parameterName);
                }
            }
        }

        private static bool IsAllowedContentType(string contentType, IReadOnlyCollection<string> allowedContentTypes)
        {
            int separatorIndex = contentType.IndexOf('/');
            if (separatorIndex <= 0 || separatorIndex == contentType.Length - 1)
            {
                return false;
            }

            string mediaType = contentType[..separatorIndex];
            string mediaSubtype = contentType[(separatorIndex + 1)..];

            return allowedContentTypes.Any(allowed =>
            {
                string pattern = allowed.Trim();
                int patternSeparatorIndex = pattern.IndexOf('/');
                if (patternSeparatorIndex <= 0 || patternSeparatorIndex == pattern.Length - 1)
                {
                    return false;
                }

                string allowedType = pattern[..patternSeparatorIndex];
                string allowedSubtype = pattern[(patternSeparatorIndex + 1)..];
                return (allowedType == "*" || string.Equals(allowedType, mediaType, StringComparison.OrdinalIgnoreCase)) &&
                    (allowedSubtype == "*" || string.Equals(allowedSubtype, mediaSubtype, StringComparison.OrdinalIgnoreCase));
            });
        }

        private static void ValidateSenderDomain(string senderAddress, IReadOnlyCollection<string> allowedDomains)
        {
            if (allowedDomains.Count == 0)
            {
                return;
            }

            var sender = MailboxAddress.Parse(senderAddress);
            var separatorIndex = sender.Address.LastIndexOf('@');
            var senderDomain = separatorIndex >= 0
                ? sender.Address[(separatorIndex + 1)..]
                : string.Empty;

            if (!allowedDomains.Any(domain =>
                string.Equals(senderDomain, domain.Trim().TrimStart('@'), StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("The email sender domain is not allowed by delivery policy.", nameof(senderAddress));
            }
        }

        private static void ValidateAddresses(IEnumerable<EmailAddress> addresses, string parameterName)
        {
            foreach (var address in addresses)
            {
                ValidateAddress(address, parameterName);
            }
        }

        private static void ValidateAddress(EmailAddress? address, string parameterName)
        {
            if (address is null || !MailboxAddress.TryParse(address.Address, out _))
            {
                throw new ArgumentException("Email mailbox addresses must be valid.", parameterName);
            }
        }
    }
}
