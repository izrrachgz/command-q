using Application.Abstractions;
using Application.Modules.Notifications.Dtos;
using Common.Models.Response;
using Cqrs.Abstractions;

namespace Application.Modules.Notifications.Queries;

/// <summary>Gets one newest-first page of emails belonging to an authenticated application account.</summary>
public sealed record GetAccountEmailsQuery : IQuery<PagedResponse<QueuedEmailModel>>, IAccountScopedRequest
{
    /// <inheritdoc />
    public required Guid AccountId { get; init; }

    /// <summary>Gets the server-derived Email API account reference.</summary>
    public required string AccountReference { get; init; }

    /// <summary>Gets the one-based page number.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Gets the maximum number of messages in the page.</summary>
    public int PageSize { get; init; } = Email.Client.EmailQueueConstants.DefaultPageSize;
}
