using Application.Modules.Notifications.Dtos;
using Common.Models.Response;
using Cqrs.Abstractions;
using Email.Client.Abstractions;
using Email.Client.Models;

namespace Application.Modules.Notifications.Queries;

internal sealed class SearchEmailsQueryHandler(IEmailQueueClient emailQueueClient)
    : IQueryHandler<SearchEmailsQuery, PagedResponse<QueuedEmailModel>>
{
    public async Task<PagedResponse<QueuedEmailModel>> Handle(SearchEmailsQuery request, CancellationToken cancellationToken)
    {
        EmailQueuePageResult page = await emailQueueClient.SearchAsync(
            new EmailQueueSearchRequest
            {
                AccountReference = request.AccountReference,
                SearchTerm = request.SearchTerm,
                Recipient = request.Recipient,
                Status = string.IsNullOrWhiteSpace(request.Status)
                    ? null
                    : Enum.Parse<EmailDeliveryStatus>(request.Status, ignoreCase: true),
                StartUtc = request.StartUtc,
                EndUtc = request.EndUtc,
                Page = request.Page,
                PageSize = request.PageSize
            },
            cancellationToken);

        return new PagedResponse<QueuedEmailModel>(
            page.Items.Select(GetAccountEmailsQueryHandler.Map).ToArray(),
            page.TotalCount,
            page.Page,
            page.PageSize);
    }
}
