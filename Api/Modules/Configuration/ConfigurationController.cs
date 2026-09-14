using Cqrs.Abstractions;
using Api.Constants;
using Api.Extensions;
using Api.Models;
using Common.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Application.Modules.Configuration.Queries;
using Application.Modules.Configuration.Dtos;

namespace Api.Modules.Configuration
{
    [ApiController, Route(ConfigurationRoute.Root)]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    [EnableRateLimiting(ApiConfigKeys.RateLimiting.SlidingPolicyName)]
    public sealed class ConfigurationController(IRequestDispatcher dispatcher) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PageResponse<ApplicationConfigurationModel>>> Get(
            [FromQuery] SearchCriteria searchCriteria,
            CancellationToken cancellationToken)
        {
            var response = await dispatcher.Send(new GetApplicationConfigurationsQuery
            {
                SearchCriteria = searchCriteria ?? new()
            }, cancellationToken);

            return response.ToPageResponse();
        }
    }
}
