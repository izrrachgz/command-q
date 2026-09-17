using Application.Modules.Notifications.Queries;
using Email.Client.Abstractions;
using Email.Client.Models;
using Localization.Providers;

namespace Application.Test.Modules.Notifications.Queries;

public sealed class GetAccountEmailsQueryTest
{
    [Fact]
    public async Task Validate_Should_Reject_Invalid_Account_And_Paging_Values()
    {
        var validator = new GetAccountEmailsQueryValidator(new ValidationTextProvider());
        var query = new GetAccountEmailsQuery
        {
            AccountId = Guid.Empty,
            AccountReference = string.Empty,
            Page = 0,
            PageSize = 101
        };

        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(query.AccountId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(query.AccountReference));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(query.Page));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(query.PageSize));
    }

    [Fact]
    public async Task Handle_Should_Request_And_Map_The_Account_Email_Page()
    {
        Guid messageId = Guid.NewGuid();
        DateTime createdUtc = new(2026, 9, 16, 12, 30, 0, DateTimeKind.Utc);
        var client = new CapturingEmailQueueClient(new EmailQueuePageResult
        {
            Items =
            [
                new QueuedEmailSummary
                {
                    Id = messageId,
                    Subject = "Account update",
                    Recipients = ["person@example.test"],
                    AccountReference = "identity-reference",
                    Status = EmailDeliveryStatus.Sent,
                    AttemptCount = 1,
                    CreatedUtc = createdUtc,
                    ModifiedUtc = createdUtc,
                    SentUtc = createdUtc
                }
            ],
            Page = 2,
            PageSize = 10,
            TotalCount = 11,
            TotalPages = 2
        });
        var handler = new GetAccountEmailsQueryHandler(client);

        var result = await handler.Handle(
            new GetAccountEmailsQuery
            {
                AccountId = Guid.NewGuid(),
                AccountReference = "identity-reference",
                Page = 2,
                PageSize = 10
            },
            TestContext.Current.CancellationToken);

        Assert.NotNull(client.Request);
        Assert.Equal("identity-reference", client.Request.AccountReference);
        Assert.Equal(2, client.Request.Page);
        Assert.Equal(10, client.Request.PageSize);
        Assert.Equal(11, result.Total);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        var item = Assert.Single(result.Collection);
        Assert.Equal(messageId, item.Id);
        Assert.Equal("Sent", item.Status);
        Assert.Equal("Account update", item.Subject);
        Assert.Equal("person@example.test", Assert.Single(item.Recipients));
        Assert.Equal("identity-reference", item.AccountReference);
    }

    private sealed class CapturingEmailQueueClient(EmailQueuePageResult result) : IEmailQueueClient
    {
        public EmailQueuePageRequest? Request { get; private set; }

        public Task<EmailQueueResult> EnqueueAsync(
            EmailQueueRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<EmailQueuePageResult> GetPageAsync(
            EmailQueuePageRequest request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            return Task.FromResult(result);
        }

        public Task<EmailQueuePageResult> SearchAsync(
            EmailQueueSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<QueuedEmailDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
