using Application.Modules.Notifications.Queries;
using Email.Client.Abstractions;
using Email.Client.Models;

namespace Application.Test.Modules.Notifications.Queries;

public sealed class GetEmailDetailQueryTest
{
    [Fact]
    public async Task Handle_Should_Map_Detail_Without_Attachment_Bytes()
    {
        Guid id = Guid.NewGuid();
        DateTime utc = new(2026, 9, 16, 12, 30, 0, DateTimeKind.Utc);
        var client = new DetailEmailQueueClient(new QueuedEmailDetail
        {
            Id = id,
            Message = new EmailMessageDetail
            {
                To = [new EmailAddress("person@example.test")],
                Cc = [],
                Bcc = [],
                ReplyTo = [],
                Subject = "Account update",
                TextBody = "The account changed.",
                Attachments = [new EmailContentMetadata("details.txt", "text/plain", 42)],
                InlineResources = []
            },
            ProfileName = "default",
            AccountReference = "account-1",
            Status = EmailDeliveryStatus.Sent,
            AttemptCount = 1,
            CreatedUtc = utc,
            ModifiedUtc = utc,
            NextAttemptUtc = utc,
            SentUtc = utc
        });
        var handler = new GetEmailDetailQueryHandler(client);

        var result = await handler.Handle(
            new GetEmailDetailQuery(id),
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(id, client.Id);
        Assert.Equal("Account update", result.Message.Subject);
        Assert.Equal(42, Assert.Single(result.Message.Attachments).Size);
    }

    private sealed class DetailEmailQueueClient(QueuedEmailDetail detail) : IEmailQueueClient
    {
        public Guid? Id { get; private set; }

        public Task<EmailQueueResult> EnqueueAsync(EmailQueueRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<EmailQueuePageResult> GetPageAsync(EmailQueuePageRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<EmailQueuePageResult> SearchAsync(EmailQueueSearchRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<QueuedEmailDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Id = id;
            return Task.FromResult<QueuedEmailDetail?>(detail);
        }
    }
}
