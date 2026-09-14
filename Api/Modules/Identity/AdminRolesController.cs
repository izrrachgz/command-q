using Api.Constants;
using Api.Extensions;
using Application.Modules.Identity.Roles.Queries;
using Application.Modules.Identity.Roles.Dtos;
using Cqrs.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Identity
{
    [ApiController, Route(IdentityRoute.AdminRoles)]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    [EnableRateLimiting(ApiConfigKeys.RateLimiting.SlidingPolicyName)]
    public sealed class AdminRolesController(IRequestDispatcher dispatcher) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IdentityRoleListModel>> Get(CancellationToken cancellationToken)
        {
            var response = await dispatcher.Send(new GetIdentityRolesQuery(), cancellationToken);

            return response.Success
                ? response.Detail!
                : this.ApiProblem(
                    StatusCodes.Status503ServiceUnavailable,
                    ApiProblemCodes.ServiceUnavailable,
                    response.Message);
        }
    }
}
