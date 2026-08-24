using Api.Constants;
using Api.Extensions;
using Api.Models;
using Application.Modules.Identity.Roles.Commands;
using Application.Modules.Identity.Roles.Dtos;
using Application.Modules.Identity.Roles.Queries;
using Common.Models.Request;
using Localization.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.Identity.Requests;
using Public;

namespace Api.Modules.Identity
{
    [ApiController, Route(IdentityRoute.AdminAccounts)]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public sealed class AdminAccountsController(ISender mediator, ITextProvider messages) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PageResponse<IdentityUserRoleModel>>> Search(
            [FromQuery] SearchCriteria searchCriteria,
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new SearchIdentityUsersQuery
            {
                SearchCriteria = searchCriteria ?? new()
            }, cancellationToken);

            return response.ToPageResponse();
        }

        [HttpGet(IdentityRoute.AccountId)]
        public async Task<ActionResult<IdentityUserRoleModel>> Details(Guid accountId, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetIdentityUserRolesQuery
            {
                UserId = accountId
            }, cancellationToken);

            return response.Success
                ? response.Detail!
                : this.ApiProblem(
                    StatusCodes.Status404NotFound,
                    ApiProblemCodes.Accounts.ProfileMissing,
                    response.Message);
        }

        [HttpPut(IdentityRoute.AccountRoles)]
        public async Task<IActionResult> UpdateRoles(
            Guid accountId,
            UpdateAccountRolesRequest request,
            CancellationToken cancellationToken)
        {
            if (IsRemovingOwnAdminRole(accountId, request.Roles))
            {
                return this.ApiProblem(
                    StatusCodes.Status409Conflict,
                    ApiProblemCodes.Accounts.RoleSelfRemoval,
                    messages.Get(nameof(Messages.Admin_Cannot_Remove_Own_Role)));
            }

            var response = await mediator.Send(new UpdateIdentityUserRolesCommand
            {
                UserId = accountId,
                Roles = request.Roles
            }, cancellationToken);

            return response.Success
                ? NoContent()
                : this.ApiProblem(
                    StatusCodes.Status502BadGateway,
                    ApiProblemCodes.Accounts.RolesUpdateFailed,
                    response.Message);
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
