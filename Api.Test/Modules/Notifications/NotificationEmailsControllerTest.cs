using System.Reflection;
using Api.Constants;
using Api.Models;
using Api.Modules.Notifications;
using Api.Modules.Notifications.Requests;
using Application.Modules.Notifications.Dtos;
using Application.Modules.Notifications.Queries;
using Common.Models.Response;
using Cqrs.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.Test.Modules.Notifications;

public sealed class NotificationEmailsControllerTest
{
    [Fact]
    public async Task Get_Should_Forward_Administrator_Search_Filters()
    {
        DateTimeOffset start = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset end = new(2026, 9, 16, 23, 59, 0, TimeSpan.Zero);
        var dispatcher = new CapturingDispatcher();
        var controller = new NotificationEmailsController(dispatcher, NullLogger<NotificationEmailsController>.Instance);

        ActionResult<PageResponse<QueuedEmailModel>> result = await controller.Get(
            new NotificationEmailSearchRequest
            {
                SearchTerm = "confirmation",
                Recipient = "person@example.test",
                Status = "Sent",
                StartDate = start,
                EndDate = end,
                Page = 3,
                Size = 15
            },
            TestContext.Current.CancellationToken);

        Assert.NotNull(dispatcher.Query);
        Assert.Equal("confirmation", dispatcher.Query.SearchTerm);
        Assert.Equal("person@example.test", dispatcher.Query.Recipient);
        Assert.Equal("Sent", dispatcher.Query.Status);
        Assert.Equal(start.UtcDateTime, dispatcher.Query.StartUtc);
        Assert.Equal(end.UtcDateTime, dispatcher.Query.EndUtc);
        Assert.Equal(3, dispatcher.Query.Page);
        Assert.Equal(15, dispatcher.Query.PageSize);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Page);
        Assert.Equal(15, result.Value.PageSize);
    }

    [Fact]
    public void Controller_Should_Require_The_Admin_Role()
    {
        AuthorizeAttribute attribute = Assert.Single(
            typeof(NotificationEmailsController).GetCustomAttributes<AuthorizeAttribute>());

        Assert.Equal(AuthorizationRoles.Admin, attribute.Roles);
    }

    [Fact]
    public async Task GetDetail_Should_Return_NotFound_When_Email_Is_Not_Visible()
    {
        var controller = new NotificationEmailsController(
            new MissingDetailDispatcher(),
            NullLogger<NotificationEmailsController>.Instance);

        ActionResult<QueuedEmailDetailModel> result = await controller.GetDetail(
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    private sealed class CapturingDispatcher : IRequestDispatcher
    {
        public SearchEmailsQuery? Query { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Query = Assert.IsType<SearchEmailsQuery>(request);
            object response = new PagedResponse<QueuedEmailModel>([], 0, Query.Page, Query.PageSize);
            return Task.FromResult((TResponse)response);
        }
    }

    private sealed class MissingDetailDispatcher : IRequestDispatcher
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Assert.IsType<GetEmailDetailQuery>(request);
            return Task.FromResult((TResponse)(object?)null!);
        }
    }
}
