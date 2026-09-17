using Application.Abstractions;
using Application.Modules.Notifications.Dtos;
using Common.Models.Response;
using Cqrs.Abstractions;

namespace Application.Modules.Notifications.Queries;

/// <summary>Searches the EmailAdmin-visible durable queue.</summary>
public sealed record SearchEmailsQuery : IQuery<PagedResponse<QueuedEmailModel>>, ISystemScopedRequest
{
    public string? AccountReference { get; init; }
    public string? SearchTerm { get; init; }
    public string? Recipient { get; init; }
    public string? Status { get; init; }
    public DateTime? StartUtc { get; init; }
    public DateTime? EndUtc { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = Email.Client.EmailQueueConstants.DefaultPageSize;
}
