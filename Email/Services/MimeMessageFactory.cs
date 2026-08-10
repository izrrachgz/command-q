using MimeKit;
using Email.Models;
using MimeKit.Utils;
using Email.Configuration;

namespace Email.Services
{
    internal sealed class MimeMessageFactory
    {
        public MimeMessage Create(EmailMessage source, EmailSenderOptions defaultSender, CancellationToken cancellationToken)
        {
            var message = new MimeMessage
            {
                Date = DateTimeOffset.UtcNow,
                MessageId = MimeUtils.GenerateMessageId(),
                Subject = source.Subject
            };

            Add(message.From, source.From ?? new EmailAddress(defaultSender.Address, defaultSender.Name));
            Add(message.To, source.To);
            Add(message.Cc, source.Cc);
            Add(message.Bcc, source.Bcc);
            Add(message.ReplyTo, source.ReplyTo);

            var body = new BodyBuilder
            {
                TextBody = source.TextBody,
                HtmlBody = source.HtmlBody
            };

            foreach (var attachment in source.Attachments)
            {
                AddAttachment(body, attachment, cancellationToken);
            }

            foreach (var resource in source.InlineResources)
            {
                var entity = AddInlineResource(body, resource, cancellationToken);
                entity.ContentId = resource.ContentId;
            }

            message.Body = body.ToMessageBody();

            return message;
        }

        private static MimeEntity AddAttachment(BodyBuilder body, EmailAttachment attachment, CancellationToken cancellationToken)
        {
            MimeEntity entity;

            if (attachment.FilePath is not null)
            {
                entity = string.IsNullOrWhiteSpace(attachment.ContentType)
                    ? body.Attachments.Add(attachment.FilePath, cancellationToken)
                    : body.Attachments.Add(
                        attachment.FilePath,
                        ContentType.Parse(attachment.ContentType),
                        cancellationToken);
            }
            else
            {
                entity = body.Attachments.Add(
                    attachment.FileName,
                    attachment.Content!.Value.ToArray(),
                    GetContentType(attachment.FileName, attachment.ContentType));
            }

            if (entity is MimePart part)
            {
                part.FileName = attachment.FileName;
            }

            return entity;
        }

        private static MimeEntity AddInlineResource(BodyBuilder body, EmailInlineResource resource, CancellationToken cancellationToken)
        {
            MimeEntity entity;

            if (resource.FilePath is not null)
            {
                entity = string.IsNullOrWhiteSpace(resource.ContentType)
                    ? body.LinkedResources.Add(resource.FilePath, cancellationToken)
                    : body.LinkedResources.Add(
                        resource.FilePath,
                        ContentType.Parse(resource.ContentType),
                        cancellationToken);
            }
            else
            {
                entity = body.LinkedResources.Add(
                    resource.FileName,
                    resource.Content!.Value.ToArray(),
                    GetContentType(resource.FileName, resource.ContentType));
            }

            if (entity is MimePart part)
            {
                part.FileName = resource.FileName;
            }

            return entity;
        }

        private static ContentType GetContentType(string fileName, string? configuredContentType)
        {
            return ContentType.Parse(configuredContentType ?? MimeTypes.GetMimeType(fileName));
        }

        private static void Add(InternetAddressList target, EmailAddress address)
        {
            target.Add(ToMailboxAddress(address));
        }

        private static void Add(InternetAddressList target, IEnumerable<EmailAddress> addresses)
        {
            foreach (var address in addresses)
            {
                Add(target, address);
            }
        }

        private static MailboxAddress ToMailboxAddress(EmailAddress address)
        {
            if (!MailboxAddress.TryParse(address.Address, out var parsed))
            {
                throw new ArgumentException("Email mailbox addresses must be valid.", nameof(address));
            }

            return new MailboxAddress(address.Name ?? parsed.Name, parsed.Address);
        }
    }
}
