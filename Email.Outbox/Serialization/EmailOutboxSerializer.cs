using System.Text;
using Email.Models;
using System.Text.Json;
using System.Diagnostics;
using Email.Outbox.Telemetry;
using Email.Outbox.Configuration;
using Microsoft.Extensions.Options;

namespace Email.Outbox.Serialization
{
    internal sealed class EmailOutboxSerializer(IOptions<EmailOutboxOptions> options)
        : IEmailOutboxSerializer
    {
        private readonly EmailOutboxOptions _options = options.Value;

        public async Task<string> SerializeAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);
            using Activity? activity = EmailOutboxTelemetry.ActivitySource.StartActivity("email.outbox.serialize");

            var attachments = new List<EmailContentSnapshot>(message.Attachments.Count);
            foreach (var attachment in message.Attachments)
            {
                attachments.Add(new EmailContentSnapshot(
                    attachment.FileName,
                    attachment.ContentType,
                    await ReadContentAsync(attachment.CopyContentToAsync, cancellationToken)));
            }

            var inlineResources = new List<EmailInlineContentSnapshot>(message.InlineResources.Count);
            foreach (var resource in message.InlineResources)
            {
                inlineResources.Add(new EmailInlineContentSnapshot(
                    resource.FileName,
                    resource.ContentId,
                    resource.ContentType,
                    await ReadContentAsync(resource.CopyContentToAsync, cancellationToken)));
            }

            var snapshot = new EmailMessageSnapshot(
                ToSnapshot(message.From),
                ToSnapshots(message.To),
                ToSnapshots(message.Cc),
                ToSnapshots(message.Bcc),
                ToSnapshots(message.ReplyTo),
                message.Subject,
                message.TextBody,
                message.HtmlBody,
                attachments.ToArray(),
                inlineResources.ToArray());

            string payload = JsonSerializer.Serialize(
                snapshot,
                EmailOutboxJsonContext.Default.EmailMessageSnapshot);
            int payloadBytes = Encoding.UTF8.GetByteCount(payload);

            if (payloadBytes > _options.MaxPayloadBytes)
            {
                throw new InvalidOperationException(
                    $"The serialized email outbox payload is {payloadBytes} bytes and exceeds the configured limit " +
                    $"of {_options.MaxPayloadBytes} bytes.");
            }

            activity?.SetTag(EmailOutboxTelemetry.OutcomeTag, "success");
            return payload;
        }

        public EmailMessage Deserialize(string payload)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(payload);

            var snapshot = JsonSerializer.Deserialize(
                payload,
                EmailOutboxJsonContext.Default.EmailMessageSnapshot) ??
                throw new InvalidOperationException("The email outbox payload could not be deserialized.");

            return new EmailMessage
            {
                From = FromSnapshot(snapshot.From),
                To = FromSnapshots(snapshot.To),
                Cc = FromSnapshots(snapshot.Cc),
                Bcc = FromSnapshots(snapshot.Bcc),
                ReplyTo = FromSnapshots(snapshot.ReplyTo),
                Subject = snapshot.Subject,
                TextBody = snapshot.TextBody,
                HtmlBody = snapshot.HtmlBody,
                Attachments = snapshot.Attachments
                    .Select(item => EmailAttachment.FromBytes(
                        item.FileName,
                        item.Content,
                        item.ContentType))
                    .ToArray(),
                InlineResources = snapshot.InlineResources
                    .Select(item => EmailInlineResource.FromBytes(
                        item.FileName,
                        item.ContentId,
                        item.Content,
                        item.ContentType))
                    .ToArray()
            };
        }

        private static EmailAddressSnapshot? ToSnapshot(EmailAddress? address)
        {
            return address is null
                ? null
                : new EmailAddressSnapshot(address.Address, address.Name);
        }

        private static EmailAddressSnapshot[] ToSnapshots(IEnumerable<EmailAddress> addresses)
        {
            return addresses
                .Select(address => new EmailAddressSnapshot(address.Address, address.Name))
                .ToArray();
        }

        private static EmailAddress? FromSnapshot(EmailAddressSnapshot? address)
        {
            return address is null
                ? null
                : new EmailAddress(address.Address, address.Name);
        }

        private static EmailAddress[] FromSnapshots(IEnumerable<EmailAddressSnapshot> addresses)
        {
            return addresses
                .Select(address => new EmailAddress(address.Address, address.Name))
                .ToArray();
        }

        private static async Task<byte[]> ReadContentAsync(
            Func<Stream, CancellationToken, Task> copyContent,
            CancellationToken cancellationToken)
        {
            using var content = new MemoryStream();
            await copyContent(content, cancellationToken);
            return content.ToArray();
        }
    }
}
