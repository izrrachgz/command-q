using Application.Modules.Notifications.Queries;
using Email.Client.Abstractions;
using Email.Client.Models;
using Localization.Providers;

namespace Application.Test.Modules.Notifications.Queries;

public sealed class SearchEmailsQueryTest
{
    [Fact]
    public async Task Handle_Should_Search_The_Admin_Queue_And_Map_The_Page()
    {
        DateTime createdUtc = new(2026, 9, 16, 12, 30, 0, DateTimeKind.Utc);
        var client = new CapturingEmailQueueClient(new EmailQueuePageResult
        {
            Items =
            [
                new QueuedEmailSummary
                {
                    Id = Guid.NewGuid(),
                    Subject = "Account update",
                    Recipients = ["person@example.test"],
                    AccountReference = "account-1",
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
        var handler = new SearchEmailsQueryHandler(client);

        var result = await handler.Handle(
            new SearchEmailsQuery
            {
                SearchTerm = "update",
                Recipient = "person@example.test",
                Status = "sent",
                StartUtc = createdUtc.AddDays(-1),
                EndUtc = createdUtc.AddDays(1),
                Page = 2,
                PageSize = 10
            },
            TestContext.Current.CancellationToken);

        Assert.NotNull(client.Request);
        Assert.Equal("update", client.Request.SearchTerm);
        Assert.Equal("person@example.test", client.Request.Recipient);
        Assert.Equal(EmailDeliveryStatus.Sent, client.Request.Status);
        Assert.Equal(11, result.Total);
        Assert.Equal("Account update", Assert.Single(result.Collection).Subject);
    }

    [Fact]
    public async Task Validate_Should_Reject_Unknown_Status_And_Invalid_Paging()
    {
        var validator = new SearchEmailsQueryValidator(new ValidationTextProvider());
        var result = await validator.ValidateAsync(
            new SearchEmailsQuery { Status = "missing", Page = 0, PageSize = 101 },
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SearchEmailsQuery.Status));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SearchEmailsQuery.Page));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SearchEmailsQuery.PageSize));
    }

    private sealed class CapturingEmailQueueClient(EmailQueuePageResult result) : IEmailQueueClient
    {
        public EmailQueueSearchRequest? Request { get; private set; }

        public Task<EmailQueueResult> EnqueueAsync(EmailQueueRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<EmailQueuePageResult> GetPageAsync(EmailQueuePageRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<EmailQueuePageResult> SearchAsync(
            EmailQueueSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            return Task.FromResult(result);
        }

        public Task<QueuedEmailDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
