using Application.Modules.Notifications.Dtos;
using Common.Models.Response;
using Cqrs.Abstractions;
using Email.Client.Abstractions;
using Email.Client.Models;

namespace Application.Modules.Notifications.Queries;

internal sealed class GetAccountEmailsQueryHandler(IEmailQueueClient emailQueueClient)
    : IQueryHandler<GetAccountEmailsQuery, PagedResponse<QueuedEmailModel>>
{
    public async Task<PagedResponse<QueuedEmailModel>> Handle(GetAccountEmailsQuery request, CancellationToken cancellationToken)
    {
        EmailQueuePageResult page = await emailQueueClient.GetPageAsync(
            new EmailQueuePageRequest
            {
                AccountReference = request.AccountReference,
                Page = request.Page,
                PageSize = request.PageSize
            },
            cancellationToken);

        return new PagedResponse<QueuedEmailModel>(
            page.Items.Select(Map).ToArray(),
            page.TotalCount,
            page.Page,
            page.PageSize);
    }

    internal static QueuedEmailModel Map(QueuedEmailSummary source) => new()
    {
        Id = source.Id,
        Subject = source.Subject,
        Recipients = source.Recipients,
        AccountReference = source.AccountReference,
        Status = source.Status.ToString(),
        AttemptCount = source.AttemptCount,
        CreatedUtc = source.CreatedUtc,
        ModifiedUtc = source.ModifiedUtc,
        SentUtc = source.SentUtc,
        DeadLetterUtc = source.DeadLetterUtc,
        MessageId = source.MessageId
    };
}
