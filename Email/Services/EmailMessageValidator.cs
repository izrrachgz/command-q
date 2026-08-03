using MimeKit;
using Email.Models;
using Email.Configuration;

namespace Email.Services
{
    internal sealed class EmailMessageValidator
    {
        public void Validate(
            EmailMessage message,
            EmailSenderOptions defaultSender,
            EmailDeliveryPolicyOptions policy)
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

            ValidateSenderDomain(message.From?.Address ?? defaultSender.Address, policy.AllowedSenderDomains);

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
                ValidateContent(
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

                ValidateContent(
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

        private static void ValidateContent(
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
