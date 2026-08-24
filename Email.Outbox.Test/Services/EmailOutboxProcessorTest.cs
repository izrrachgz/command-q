using NodaTime;
using Email.Models;
using Email.Exceptions;
using Email.Abstractions;
using Email.Outbox.Models;
using Email.Outbox.Services;
using Email.Outbox.Abstractions;
using Email.Outbox.Configuration;
using Email.Outbox.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;

namespace Email.Outbox.Test.Services
{
    public sealed class EmailOutboxProcessorTest
    {
        [Fact]
        public async Task Processor_Should_Send_Claimed_Messages_In_One_Batch()
        {
            var serializer = CreateSerializer();
            var records = new[]
            {
                await CreateClaimedRecord(serializer, "one@example.com"),
                await CreateClaimedRecord(serializer, "two@example.com")
            };
            var store = new RecordingStore(records);
            var sender = new RecordingSender();
            var processor = CreateProcessor(store, sender, serializer);

            var result = await processor.ProcessBatchAsync(
                CreateClaimRequest(),
                TestContext.Current.CancellationToken);

            Assert.Equal(1, sender.BatchCalls);
            Assert.Equal(2, sender.LastBatch.Count);
            Assert.Equal(2, result.SentCount);
            Assert.Equal(2, store.SentUpdates.Count);
            Assert.Empty(store.FailedUpdates);
            Assert.Empty(store.ReleasedRecords);
        }

        [Fact]
        public async Task Processor_Should_Open_One_Sender_Batch_Per_Profile()
        {
            var serializer = CreateSerializer();
            var records = new[]
            {
                await CreateClaimedRecord(serializer, "one@example.com", "Transactional"),
                await CreateClaimedRecord(serializer, "two@example.com", "Support"),
                await CreateClaimedRecord(serializer, "three@example.com", "Transactional")
            };
            var store = new RecordingStore(records);
            var senderFactory = new RecordingSenderFactory();
            var processor = CreateProcessor(store, senderFactory, serializer);

            var result = await processor.ProcessBatchAsync(
                CreateClaimRequest(),
                TestContext.Current.CancellationToken);

            Assert.Equal(3, result.SentCount);
            Assert.Equal(1, senderFactory.Senders["Transactional"].BatchCalls);
            Assert.Equal(2, senderFactory.Senders["Transactional"].LastBatch.Count);
            Assert.Equal(1, senderFactory.Senders["Support"].BatchCalls);
            Assert.Single(senderFactory.Senders["Support"].LastBatch);
        }

        [Fact]
        public async Task Processor_Should_Retry_The_First_Record_And_Release_Unattempted_Records()
        {
            var serializer = CreateSerializer();
            var records = new[]
            {
                await CreateClaimedRecord(serializer, "one@example.com"),
                await CreateClaimedRecord(serializer, "two@example.com")
            };
            var store = new RecordingStore(records);
            var sender = new RecordingSender(new IOException("SMTP is temporarily unavailable."));
            var processor = CreateProcessor(store, sender, serializer);

            var result = await processor.ProcessBatchAsync(
                CreateClaimRequest(),
                TestContext.Current.CancellationToken);

            var failed = Assert.Single(store.FailedUpdates);
            Assert.False(failed.IsDeadLetter);
            Assert.Equal(EmailOutboxFailureKind.Unknown, failed.FailureKind);
            Assert.Equal(1, result.RetryCount);
            Assert.Equal(1, result.ReleasedCount);
            Assert.Single(store.ReleasedRecords);
        }

        [Fact]
        public async Task Processor_Should_Complete_Accepted_Messages_And_Retry_The_Failed_Message()
        {
            var serializer = CreateSerializer();
            var records = new[]
            {
                await CreateClaimedRecord(serializer, "one@example.com"),
                await CreateClaimedRecord(serializer, "two@example.com")
            };
            var accepted = new EmailSendResult(
                1,
                "accepted-message",
                "Accepted",
                SystemClock.Instance.GetCurrentInstant());
            var deliveryException = new EmailDeliveryException(
                "Second message failed.",
                EmailFailureKind.TransientSmtp,
                2,
                new[] { accepted },
                failedMessageNumber: 2);
            var store = new RecordingStore(records);
            var sender = new RecordingSender(deliveryException);
            var processor = CreateProcessor(store, sender, serializer);

            var result = await processor.ProcessBatchAsync(
                CreateClaimRequest(),
                TestContext.Current.CancellationToken);

            Assert.Equal(1, result.SentCount);
            Assert.Equal(1, result.RetryCount);
            Assert.Single(store.SentUpdates);
            var failed = Assert.Single(store.FailedUpdates);
            Assert.Equal(EmailOutboxFailureKind.TransientSmtp, failed.FailureKind);
            Assert.Empty(store.ReleasedRecords);
        }

        private static EmailOutboxProcessor CreateProcessor(IEmailOutboxStore store, IEmailSender sender, IEmailOutboxSerializer serializer)
        {
            return new EmailOutboxProcessor(
                store,
                new FixedSenderFactory(sender),
                serializer,
                Options.Create(new EmailOutboxOptions()),
                NullLogger<EmailOutboxProcessor>.Instance,
                TimeProvider.System);
        }

        private static EmailOutboxProcessor CreateProcessor(
            IEmailOutboxStore store,
            IEmailSenderFactory senderFactory,
            IEmailOutboxSerializer serializer)
        {
            return new EmailOutboxProcessor(
                store,
                senderFactory,
                serializer,
                Options.Create(new EmailOutboxOptions()),
                NullLogger<EmailOutboxProcessor>.Instance,
                TimeProvider.System);
        }

        private static EmailOutboxSerializer CreateSerializer()
        {
            return new EmailOutboxSerializer(Options.Create(new EmailOutboxOptions()));
        }

        private static EmailOutboxClaimRequest CreateClaimRequest()
        {
            return new EmailOutboxClaimRequest(
                10,
                "test-worker",
                Duration.FromMinutes(1),
                SystemClock.Instance.GetCurrentInstant());
        }

        private static async Task<ClaimedEmailOutboxRecord> CreateClaimedRecord(
            IEmailOutboxSerializer serializer,
            string recipient,
            string profileName = "default")
        {
            string payload = await serializer.SerializeAsync(
                new EmailMessage
                {
                    From = new EmailAddress("sender@example.com"),
                    To = new[] { new EmailAddress(recipient) },
                    Subject = "Queued",
                    TextBody = "Body"
                },
                TestContext.Current.CancellationToken);

            return new ClaimedEmailOutboxRecord(
                Guid.NewGuid(),
                payload,
                Guid.NewGuid().ToString("N"),
                0,
                null,
                SystemClock.Instance.GetCurrentInstant(),
                profileName);
        }

        private sealed class RecordingStore(IReadOnlyList<ClaimedEmailOutboxRecord> claimed)
            : IEmailOutboxStore
        {
            public List<EmailOutboxSentUpdate> SentUpdates { get; } = [];
            public List<EmailOutboxFailedUpdate> FailedUpdates { get; } = [];
            public List<ClaimedEmailOutboxRecord> ReleasedRecords { get; } = [];

            public Task<EmailOutboxEnqueueResult> EnqueueAsync(EmailOutboxRecord record, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            public Task<IReadOnlyList<ClaimedEmailOutboxRecord>> ClaimPendingAsync(EmailOutboxClaimRequest request, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(claimed);
            }

            public Task<bool> MarkSentAsync(EmailOutboxSentUpdate update, CancellationToken cancellationToken = default)
            {
                SentUpdates.Add(update);
                return Task.FromResult(true);
            }

            public Task<bool> MarkFailedAsync(EmailOutboxFailedUpdate update, CancellationToken cancellationToken = default)
            {
                FailedUpdates.Add(update);
                return Task.FromResult(true);
            }

            public Task<int> ReleaseAsync(
                IReadOnlyCollection<ClaimedEmailOutboxRecord> records,
                Instant nextAttemptUtc,
                CancellationToken cancellationToken = default)
            {
                ReleasedRecords.AddRange(records);
                return Task.FromResult(records.Count);
            }
        }

        private sealed class RecordingSender(Exception? exception = null) : IEmailSender
        {
            public int BatchCalls { get; private set; }
            public IReadOnlyList<EmailMessage> LastBatch { get; private set; } = Array.Empty<EmailMessage>();

            public Task<EmailBatchResult> SendAsync(IEnumerable<EmailMessage> messages, CancellationToken cancellationToken = default)
            {
                BatchCalls++;
                LastBatch = messages.ToArray();

                if (exception is not null)
                {
                    return Task.FromException<EmailBatchResult>(exception);
                }

                return Task.FromResult(new EmailBatchResult(
                    LastBatch.Select((_, index) => new EmailSendResult(
                        index + 1,
                        $"message-{index + 1}",
                        "Accepted",
                        SystemClock.Instance.GetCurrentInstant())).ToArray()));
            }

            public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<EmailSendResult> SendAsync(string recipientAddress, string subject, string textBody, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<EmailSendResult> SendAsync(IEnumerable<string> recipientAddresses, string subject, string textBody, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<EmailSendResult> SendAsync(string recipientAddress, string subject, string textBody, IEnumerable<string> attachmentFilePaths, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<EmailSendResult> SendAsync(string recipientAddress, string subject, string textBody, IEnumerable<EmailAttachment> attachments, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<EmailSendResult> SendHtmlAsync(string recipientAddress, string subject, string htmlBody, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<EmailSendResult> SendHtmlAsync(IEnumerable<string> recipientAddresses, string subject, string htmlBody, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<EmailSendResult> SendHtmlAsync(string recipientAddress, string subject, string htmlBody, IEnumerable<string> attachmentFilePaths, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<EmailSendResult> SendHtmlAsync(string recipientAddress, string subject, string htmlBody, IEnumerable<EmailAttachment> attachments, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<EmailSendResult> SendHtmlTemplateAsync(string recipientAddress, string subject, string htmlTemplateFilePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<EmailSendResult> SendHtmlTemplateAsync(string recipientAddress, string subject, string htmlTemplateFilePath, IEnumerable<string> attachmentFilePaths, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        }

        private sealed class RecordingSenderFactory : IEmailSenderFactory
        {
            public Dictionary<string, RecordingSender> Senders { get; } = new(StringComparer.OrdinalIgnoreCase);

            public IEmailSender Create(string? profileName = null)
            {
                string name = profileName ?? "default";
                return Senders.TryGetValue(name, out RecordingSender? sender)
                    ? sender
                    : Senders[name] = new RecordingSender();
            }
        }

        private sealed class FixedSenderFactory(IEmailSender sender) : IEmailSenderFactory
        {
            public IEmailSender Create(string? profileName = null)
            {
                return sender;
            }
        }
    }
}
