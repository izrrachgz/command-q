using Api.Constants;
using Application.Modules.Identity.Roles.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Identity
{
    [ApiController, Route(IdentityRouteConstants.AdminRoles)]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public sealed class AdminRolesController(ISender mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetIdentityRolesQuery(), cancellationToken);

            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
