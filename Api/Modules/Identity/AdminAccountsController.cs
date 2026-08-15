using Api.Constants;
using Api.Extensions;
using Application.Modules.Identity.Roles.Commands;
using Application.Modules.Identity.Roles.Queries;
using Common.Models.Request;
using Common.Models.Response;
using Localization.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Public;

namespace Api.Modules.Identity
{
    [ApiController, Route(IdentityRouteConstants.AdminAccounts)]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public sealed class AdminAccountsController(ISender mediator, ITextProvider messages) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] SearchCriteria searchCriteria,
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new SearchIdentityUsersQuery
            {
                SearchCriteria = searchCriteria ?? new()
            }, cancellationToken);

            return Ok(response);
        }

        [HttpGet(IdentityRouteConstants.AccountId)]
        public async Task<IActionResult> Details(Guid accountId, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetIdentityUserRolesQuery
            {
                UserId = accountId
            }, cancellationToken);

            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpPut(IdentityRouteConstants.AccountRoles)]
        public async Task<IActionResult> UpdateRoles(
            Guid accountId,
            UpdateAccountRolesRequest request,
            CancellationToken cancellationToken)
        {
            if (IsRemovingOwnAdminRole(accountId, request.Roles))
            {
                return BadRequest(new BasicResponse(false, messages.Get(nameof(Messages.Admin_Cannot_Remove_Own_Role))));
            }

            var response = await mediator.Send(new UpdateIdentityUserRolesCommand
            {
                UserId = accountId,
                Roles = request.Roles
            }, cancellationToken);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        public sealed record UpdateAccountRolesRequest
        {
            public IReadOnlyList<string> Roles { get; init; } = [];
        }

        private bool IsRemovingOwnAdminRole(Guid accountId, IReadOnlyList<string> requestedRoles)
        {
            return User.TryGetIdentityKey(out _, out var subject) &&
                Guid.TryParse(subject, out var currentIdentityUserId) &&
                currentIdentityUserId == accountId &&
                !requestedRoles.Contains(AuthorizationRoles.Admin, StringComparer.OrdinalIgnoreCase);
        }
    }
}
