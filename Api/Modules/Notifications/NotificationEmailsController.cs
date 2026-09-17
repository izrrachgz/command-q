using Api.Constants;
using Api.Extensions;
using Api.Models;
using Api.Modules.Notifications.Requests;
using Application.Modules.Notifications.Dtos;
using Application.Modules.Notifications.Queries;
using Cqrs.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Modules.Notifications;

/// <summary>Exposes administrator email queue history.</summary>
[ApiController, Authorize(Roles = AuthorizationRoles.Admin), Route(NotificationsRoute.Emails)]
public sealed class NotificationEmailsController(
    IRequestDispatcher dispatcher,
    ILogger<NotificationEmailsController> logger) : ControllerBase
{
    /// <summary>Searches one newest-first page across the EmailAdmin-visible queue.</summary>
    /// <param name="request">The filters and paging values.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The requested email page.</returns>
    [HttpGet]
    public async Task<ActionResult<PageResponse<QueuedEmailModel>>> Get(
        [FromQuery] NotificationEmailSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await dispatcher.Send(
            new SearchEmailsQuery
            {
                AccountReference = request.AccountReference,
                SearchTerm = request.SearchTerm,
                Recipient = request.Recipient,
                Status = request.Status,
                StartUtc = request.StartDate?.UtcDateTime,
                EndUtc = request.EndDate?.UtcDateTime,
                Page = request.Page,
                PageSize = request.Size
            },
            cancellationToken);

        logger.AdminSearch(
            AdministratorSubject(),
            !string.IsNullOrWhiteSpace(request.AccountReference),
            !string.IsNullOrWhiteSpace(request.SearchTerm),
            !string.IsNullOrWhiteSpace(request.Recipient),
            !string.IsNullOrWhiteSpace(request.Status),
            request.StartDate.HasValue || request.EndDate.HasValue,
            response.Total);
        return response.ToPageResponse();
    }

    /// <summary>Gets one complete queued email without attachment bytes.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QueuedEmailDetailModel>> GetDetail(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        QueuedEmailDetailModel? response = await dispatcher.Send(
            new GetEmailDetailQuery(id),
            cancellationToken);
        if (response is null) return NotFound();
        logger.AdminDetail(AdministratorSubject(), id);
        return Ok(response);
    }

    private string AdministratorSubject() =>
        HttpContext?.User?.FindFirstValue("sub") ?? "unknown";
}
