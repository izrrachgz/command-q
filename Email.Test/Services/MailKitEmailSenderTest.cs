using MimeKit;
using Email.Models;
using Email.Services;
using Email.Exceptions;
using Email.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;

namespace Email.Test.Services
{
    public sealed class MailKitEmailSenderTest
    {
        [Fact]
        public async Task SendAsync_Should_Reuse_One_Connection_For_A_Batch()
        {
            var transport = new RecordingSmtpTransport();
            var sender = CreateSender(transport);
            var messages = new[]
            {
                CreateMessage("first@example.test", "First", "First body"),
                CreateMessage("second@example.test", "Second", "Second body")
            };

            var result = await sender.SendAsync(messages, TestContext.Current.CancellationToken);

            Assert.Equal(1, transport.ConnectCount);
            Assert.Equal(1, transport.AuthenticateCount);
            Assert.Equal(1, transport.NoOpCount);
            Assert.Equal(EmailAuthenticationType.Login, transport.AuthenticationType);
            Assert.Equal(2, transport.SendCount);
            Assert.Equal(1, transport.DisconnectCount);
            Assert.Equal(1, transport.DisposeCount);
            Assert.Equal(new[] { "First", "Second" }, transport.Messages.Select(message => message.Subject));
            Assert.Equal(2, result.SentCount);
            Assert.Equal(new[] { 1, 2 }, result.Messages.Select(message => message.MessageNumber));
            Assert.All(result.Messages, message => Assert.Equal("250 Message accepted", message.SmtpResponse));
            Assert.All(result.Messages, message => Assert.False(string.IsNullOrWhiteSpace(message.MessageId)));
        }

        [Fact]
        public async Task SendAsync_Should_Not_Connect_When_Delivery_Is_Disabled()
        {
            var transport = new RecordingSmtpTransport();
            var sender = CreateSender(transport, enabled: false);

            var exception = await Assert.ThrowsAsync<EmailDeliveryException>(() =>
                sender.SendAsync(
                    CreateMessage("recipient@example.test", "Subject", "Body"),
                    TestContext.Current.CancellationToken));

            Assert.Equal(EmailFailureKind.Disabled, exception.FailureKind);
            Assert.Equal(0, exception.SentCount);
            Assert.Equal(1, exception.RequestedCount);
            Assert.Equal(0, transport.ConnectCount);
        }

        [Fact]
        public async Task SendAsync_Should_Skip_Authentication_When_Credentials_Are_Empty()
        {
            var transport = new RecordingSmtpTransport();
            var sender = CreateSender(transport, EmailAuthenticationType.Anonymous);

            await sender.SendAsync(
                CreateMessage("recipient@example.test", "Subject", "Body"),
                TestContext.Current.CancellationToken);

            Assert.Equal(1, transport.ConnectCount);
            Assert.Equal(0, transport.AuthenticateCount);
            Assert.Equal(1, transport.SendCount);
            Assert.Equal(1, transport.DisconnectCount);
        }

        [Fact]
        public async Task SendAsync_Should_Map_Complete_Message_Addressing()
        {
            var transport = new RecordingSmtpTransport();
            var sender = CreateSender(transport);
            var message = new EmailMessage
            {
                From = new EmailAddress("override@example.test", "Override Sender"),
                To = new[] { new EmailAddress("to@example.test", "To") },
                Cc = new[] { new EmailAddress("cc@example.test") },
                Bcc = new[] { new EmailAddress("bcc@example.test") },
                ReplyTo = new[] { new EmailAddress("reply@example.test") },
                Subject = "Addressing",
                TextBody = "Plain text",
                HtmlBody = "<p>HTML</p>"
            };

            await sender.SendAsync(message, TestContext.Current.CancellationToken);

            var delivered = Assert.Single(transport.Messages);
            Assert.Equal("override@example.test", Assert.Single(delivered.FromAddresses));
            Assert.Equal("to@example.test", Assert.Single(delivered.To));
            Assert.Equal("cc@example.test", Assert.Single(delivered.Cc));
            Assert.Equal("bcc@example.test", Assert.Single(delivered.Bcc));
            Assert.Equal("reply@example.test", Assert.Single(delivered.ReplyTo));
            Assert.Equal("Plain text", delivered.TextBody);
            Assert.Equal("<p>HTML</p>", delivered.HtmlBody);
        }

        [Fact]
        public async Task SendAsync_Should_Disconnect_After_A_Delivery_Failure()
        {
            var transport = new RecordingSmtpTransport
            {
                FailOnSendNumber = 2
            };
            var sender = CreateSender(transport);
            var messages = new[]
            {
                CreateMessage("first@example.test", "First", "First body"),
                CreateMessage("second@example.test", "Second", "Second body")
            };

            var exception = await Assert.ThrowsAsync<EmailDeliveryException>(() =>
                sender.SendAsync(messages, TestContext.Current.CancellationToken));

            Assert.Equal(EmailFailureKind.Unknown, exception.FailureKind);
            Assert.Equal(1, exception.SentCount);
            Assert.Equal(2, exception.RequestedCount);
            Assert.Equal(2, exception.FailedMessageNumber);
            Assert.Single(exception.SentMessages);
            Assert.Equal(1, transport.ConnectCount);
            Assert.Equal(2, transport.SendCount);
            Assert.Equal(1, transport.DisconnectCount);
            Assert.Equal(1, transport.DisposeCount);
        }

        [Fact]
        public async Task SendAsync_Should_Add_InMemory_Attachments_And_Inline_Resources()
        {
            var transport = new RecordingSmtpTransport();
            var sender = CreateSender(transport);
            await using var source = new MemoryStream("stream attachment"u8.ToArray());
            var streamAttachment = await EmailAttachment.FromStreamAsync(
                "stream.txt",
                source,
                "text/plain",
                TestContext.Current.CancellationToken);
            var message = new EmailMessage
            {
                To = new[] { new EmailAddress("recipient@example.test") },
                Subject = "Memory content",
                HtmlBody = "<html><body><img src=\"cid:brand-logo\"></body></html>",
                Attachments = new[]
                {
                    EmailAttachment.FromBytes("bytes.txt", "byte attachment"u8.ToArray(), "text/plain"),
                    streamAttachment
                },
                InlineResources = new[]
                {
                    EmailInlineResource.FromBytes(
                        "logo.png",
                        "brand-logo",
                        new byte[] { 1, 2, 3 },
                        "image/png")
                }
            };

            await sender.SendAsync(message, TestContext.Current.CancellationToken);

            var delivered = Assert.Single(transport.Messages);
            Assert.Equal(new[] { "bytes.txt", "stream.txt" }, delivered.AttachmentNames);
            Assert.Equal("brand-logo", Assert.Single(delivered.InlineContentIds));
        }

        [Fact]
        public async Task SendAsync_Should_Validate_All_Attachments_Before_Connecting()
        {
            var transport = new RecordingSmtpTransport();
            var sender = CreateSender(transport);
            var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
            var message = CreateMessage("recipient@example.test", "Subject", "Body") with
            {
                Attachments = new[] { new EmailAttachment(missingPath) }
            };

            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                sender.SendAsync(message, TestContext.Current.CancellationToken));

            Assert.Equal(0, transport.ConnectCount);
        }

        [Fact]
        public async Task SendAsync_Should_Enforce_Delivery_Policy_Before_Connecting()
        {
            var transport = new RecordingSmtpTransport();
            var sender = CreateSender(transport, configure: options =>
            {
                options.DeliveryPolicy.MaxRecipients = 1;
                options.DeliveryPolicy.AllowedSenderDomains = new[] { "example.test" };
            });
            var message = CreateMessage("first@example.test", "Subject", "Body") with
            {
                Cc = new[] { new EmailAddress("second@example.test") }
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                sender.SendAsync(message, TestContext.Current.CancellationToken));

            Assert.Equal(0, transport.ConnectCount);
        }

        [Fact]
        public async Task SendAsync_Should_Reject_Mime_Message_That_Exceeds_Policy()
        {
            var transport = new RecordingSmtpTransport();
            var sender = CreateSender(transport, configure: options => options.DeliveryPolicy.MaxMessageBytes = 100);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                sender.SendAsync(
                    CreateMessage("recipient@example.test", "Subject", "Body"),
                    TestContext.Current.CancellationToken));

            Assert.Equal(0, transport.ConnectCount);
        }

        [Fact]
        public async Task SendAsync_Should_Enforce_Attachment_Content_And_Extension_Allowlists()
        {
            var transport = new RecordingSmtpTransport();
            var sender = CreateSender(transport, configure: options =>
            {
                options.DeliveryPolicy.AllowedAttachmentContentTypes = new[] { "application/pdf" };
                options.DeliveryPolicy.AllowedAttachmentExtensions = new[] { ".pdf" };
            });
            var message = CreateMessage("recipient@example.test", "Subject", "Body") with
            {
                Attachments = new[]
                {
                    EmailAttachment.FromBytes("notes.txt", "plain text"u8.ToArray(), "text/plain")
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                sender.SendAsync(message, TestContext.Current.CancellationToken));

            Assert.Equal(0, transport.ConnectCount);
        }

        [Fact]
        public async Task SendAsync_Should_Reject_Unknown_Content_Type_By_Default()
        {
            var transport = new RecordingSmtpTransport();
            var sender = CreateSender(transport);
            var message = CreateMessage("recipient@example.test", "Subject", "Body") with
            {
                Attachments = new[]
                {
                    EmailAttachment.FromBytes("payload.unknown", new byte[] { 1, 2, 3 })
                }
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                sender.SendAsync(message, TestContext.Current.CancellationToken));

            Assert.Equal(0, transport.ConnectCount);
        }

        [Fact]
        public async Task SendAsync_Should_Allow_Unknown_Content_Type_When_Configured()
        {
            var transport = new RecordingSmtpTransport();
            var sender = CreateSender(transport, configure: options => options.DeliveryPolicy.AllowUnknownContentTypes = true);
            var message = CreateMessage("recipient@example.test", "Subject", "Body") with
            {
                Attachments = new[]
                {
                    EmailAttachment.FromBytes("payload.unknown", new byte[] { 1, 2, 3 })
                }
            };

            await sender.SendAsync(message, TestContext.Current.CancellationToken);

            Assert.Equal(1, transport.ConnectCount);
            Assert.Equal(1, transport.SendCount);
        }

        [Fact]
        public async Task SendHtmlTemplateAsync_Should_Read_Rendered_Html_And_Attach_Files()
        {
            var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"commandq-email-{Guid.NewGuid():N}");
            Directory.CreateDirectory(temporaryDirectory);

            try
            {
                var templatePath = Path.Combine(temporaryDirectory, "template.html");
                var attachmentPath = Path.Combine(temporaryDirectory, "attachment.txt");
                const string html = "<html><body><strong>Ready to send</strong></body></html>";

                await File.WriteAllTextAsync(templatePath, html, TestContext.Current.CancellationToken);
                await File.WriteAllTextAsync(attachmentPath, "attachment", TestContext.Current.CancellationToken);

                var transport = new RecordingSmtpTransport();
                var sender = CreateSender(transport);

                await sender.SendHtmlTemplateAsync(
                    "recipient@example.test",
                    "Template subject",
                    templatePath,
                    new[] { attachmentPath },
                    TestContext.Current.CancellationToken);

                var delivered = Assert.Single(transport.Messages);
                Assert.Equal(html, delivered.HtmlBody);
                Assert.Equal("attachment.txt", Assert.Single(delivered.AttachmentNames));
            }
            finally
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }

        private static MailKitEmailSender CreateSender(
            RecordingSmtpTransport transport,
            EmailAuthenticationType authenticationType = EmailAuthenticationType.Login,
            bool enabled = true,
            Action<EmailOptions>? configure = null)
        {
            var options = Options.Create(new EmailOptions
            {
                Enabled = enabled,
                Smtp = new EmailSmtpOptions
                {
                    Host = "smtp.example.test",
                    Port = 587,
                    Security = EmailSecurityMode.StartTls
                },
                Authentication = new EmailAuthenticationOptions
                {
                    Type = authenticationType,
                    Login = new EmailUsernamePasswordOptions
                    {
                        Username = "login-user",
                        Password = "login-password"
                    },
                    Plain = new EmailUsernamePasswordOptions
                    {
                        Username = "plain-user",
                        Password = "plain-password"
                    },
                    OAuth = new EmailOAuthOptions
                    {
                        Username = "oauth-user",
                        AccessToken = "oauth-access-token"
                    }
                },
                Sender = new EmailSenderOptions
                {
                    Address = "sender@example.test",
                    Name = "Example Sender"
                }
            });

            configure?.Invoke(options.Value);

            return new MailKitEmailSender(
                new TestOptionsMonitor<EmailOptions>(options.Value),
                new RecordingSmtpTransportFactory(transport),
                new MimeMessageFactory(TimeProvider.System),
                new EmailMessageValidator(),
                NullLogger<MailKitEmailSender>.Instance,
                TimeProvider.System);
        }

        private sealed class TestOptionsMonitor<T>(T value) : IOptionsMonitor<T>
        {
            public T CurrentValue => value;

            public T Get(string? name) => value;

            public IDisposable OnChange(Action<T, string?> listener) => NoopDisposable.Instance;

            private sealed class NoopDisposable : IDisposable
            {
                public static readonly NoopDisposable Instance = new();

                public void Dispose()
                {
                }
            }
        }

        private static EmailMessage CreateMessage(string recipient, string subject, string body)
        {
            return new EmailMessage
            {
                To = new[] { new EmailAddress(recipient) },
                Subject = subject,
                TextBody = body
            };
        }

        private sealed class RecordingSmtpTransportFactory(RecordingSmtpTransport transport)
            : ISmtpTransportFactory
        {
            public ISmtpTransport Create(EmailSmtpOptions options)
            {
                return transport;
            }
        }

        private sealed class RecordingSmtpTransport : ISmtpTransport
        {
            public int? FailOnSendNumber { get; init; }
            public int ConnectCount { get; private set; }
            public int AuthenticateCount { get; private set; }
            public int NoOpCount { get; private set; }
            public int SendCount { get; private set; }
            public int DisconnectCount { get; private set; }
            public int DisposeCount { get; private set; }
            public bool IsConnected { get; private set; }
            public EmailAuthenticationType? AuthenticationType { get; private set; }
            public List<DeliveredMessage> Messages { get; } = new();

            public Task ConnectAsync(string host, int port, EmailSecurityMode securityMode, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ConnectCount++;
                IsConnected = true;

                return Task.CompletedTask;
            }

            public Task AuthenticateAsync(EmailAuthenticationOptions authentication, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AuthenticateCount++;
                AuthenticationType = authentication.Type;

                return Task.CompletedTask;
            }

            public Task<string> SendAsync(MimeMessage message, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                SendCount++;

                if (FailOnSendNumber == SendCount)
                {
                    throw new InvalidOperationException("Simulated SMTP failure.");
                }

                Messages.Add(new DeliveredMessage(
                    message.Subject ?? string.Empty,
                    message.TextBody,
                    message.HtmlBody,
                    GetAddresses(message.From),
                    GetAddresses(message.To),
                    GetAddresses(message.Cc),
                    GetAddresses(message.Bcc),
                    GetAddresses(message.ReplyTo),
                    message.Attachments
                        .OfType<MimePart>()
                        .Select(part => part.FileName)
                        .Where(fileName => fileName is not null)
                        .Cast<string>()
                        .ToArray(),
                    message.BodyParts
                        .OfType<MimePart>()
                        .Select(part => part.ContentId)
                        .Where(contentId => contentId is not null)
                        .Cast<string>()
                        .ToArray()));

                return Task.FromResult("250 Message accepted");
            }

            public Task NoOpAsync(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                NoOpCount++;

                return Task.CompletedTask;
            }

            private static IReadOnlyCollection<string> GetAddresses(InternetAddressList addresses)
            {
                return addresses
                    .OfType<MailboxAddress>()
                    .Select(address => address.Address)
                    .ToArray();
            }

            public Task DisconnectAsync(bool quit, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                DisconnectCount++;
                IsConnected = false;

                return Task.CompletedTask;
            }

            public void Dispose()
            {
                DisposeCount++;
                IsConnected = false;
            }
        }

        private sealed record DeliveredMessage(
            string Subject,
            string? TextBody,
            string? HtmlBody,
            IReadOnlyCollection<string> FromAddresses,
            IReadOnlyCollection<string> To,
            IReadOnlyCollection<string> Cc,
            IReadOnlyCollection<string> Bcc,
            IReadOnlyCollection<string> ReplyTo,
            IReadOnlyCollection<string> AttachmentNames,
            IReadOnlyCollection<string> InlineContentIds);
    }
}
