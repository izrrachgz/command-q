using Email.Models;
using Email.Outbox.Configuration;
using Email.Outbox.Serialization;
using Microsoft.Extensions.Options;

namespace Email.Outbox.Test.Serialization
{
    public sealed class EmailOutboxSerializerTest
    {
        [Fact]
        public async Task Serializer_Should_Snapshot_All_Message_Content()
        {
            var serializer = new EmailOutboxSerializer(Options.Create(new EmailOutboxOptions()));
            var message = new EmailMessage
            {
                From = new EmailAddress("sender@example.com", "Sender"),
                To = new[] { new EmailAddress("to@example.com", "Recipient") },
                Cc = new[] { new EmailAddress("cc@example.com") },
                ReplyTo = new[] { new EmailAddress("reply@example.com") },
                Subject = "Queued message",
                TextBody = "Plain body",
                HtmlBody = "<p>HTML body<img src=\"cid:logo\"></p>",
                Attachments = new[]
                {
                    EmailAttachment.FromBytes("report.txt", "attachment"u8.ToArray(), "text/plain")
                },
                InlineResources = new[]
                {
                    EmailInlineResource.FromBytes("logo.txt", "logo", "inline"u8.ToArray(), "text/plain")
                }
            };

            string payload = await serializer.SerializeAsync(
                message,
                TestContext.Current.CancellationToken);
            EmailMessage restored = serializer.Deserialize(payload);

            Assert.Equal(message.From, restored.From);
            Assert.Equal(message.To, restored.To);
            Assert.Equal(message.Cc, restored.Cc);
            Assert.Equal(message.ReplyTo, restored.ReplyTo);
            Assert.Equal(message.Subject, restored.Subject);
            Assert.Equal(message.TextBody, restored.TextBody);
            Assert.Equal(message.HtmlBody, restored.HtmlBody);
            Assert.Equal("attachment", await ReadContent(restored.Attachments.Single()));
            Assert.Equal("inline", await ReadContent(restored.InlineResources.Single()));
        }

        private static async Task<string> ReadContent(EmailAttachment attachment)
        {
            using var content = new MemoryStream();
            await attachment.CopyContentToAsync(content, TestContext.Current.CancellationToken);
            return System.Text.Encoding.UTF8.GetString(content.ToArray());
        }

        private static async Task<string> ReadContent(EmailInlineResource resource)
        {
            using var content = new MemoryStream();
            await resource.CopyContentToAsync(content, TestContext.Current.CancellationToken);
            return System.Text.Encoding.UTF8.GetString(content.ToArray());
        }
    }
}
