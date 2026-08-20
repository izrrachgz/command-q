using MediatR;
using Api.Constants;
using Common.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Modules.Configuration.Queries;

namespace Api.Modules.Configuration
{
    [ApiController, Route(ConfigurationRouteConstants.Root)]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public sealed class ApplicationConfigurationController(ISender mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] SearchCriteria searchCriteria, CancellationToken cancellationToken)
        {
            return Ok(await mediator.Send(new GetApplicationConfigurationsQuery
            {
                SearchCriteria = searchCriteria ?? new()
            }, cancellationToken));
        }
    }
}
