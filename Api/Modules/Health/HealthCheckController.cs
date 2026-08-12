using MediatR;
using Microsoft.AspNetCore.Mvc;
using Api.Modules.Health.Logging;
using Application.Modules.Health.Queries;
using Microsoft.AspNetCore.Authorization;
using Application.Modules.Health.Commands;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Modules.Health
{
    [ApiController, Authorize, Route(@"[controller]")]
    public sealed class HealthCheckController(ISender mediator, ILogger<HealthCheckController> logger) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        private readonly ILogger<HealthCheckController> _logger = logger;

        /// <summary>
        /// Checks if application is healthy enough in order to be able to properly run
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(@"scan")]
        public async Task<List<HealthCheckResult>> Scan(CancellationToken cancellationToken = default)
        {
            _logger.CheckingApplicationHealth();

            var writeLocalStorage = new SaveLocalStorageDataCommand();
            var canWriteOnLocalStorage = await _mediator.Send(writeLocalStorage, cancellationToken);

            var writeDb = new SaveDatabaseDataCommand();
            var canWriteOnDb = await _mediator.Send(writeDb, cancellationToken);

            var readDb = new GetDatabaseDataQuery();
            var canReadOnDb = await _mediator.Send(readDb, cancellationToken);

            var accessInternet = new CheckInternetConnectionQuery();
            var canAccessInternet = await _mediator.Send(accessInternet, cancellationToken);

            return [canWriteOnLocalStorage, canWriteOnDb, canReadOnDb, canAccessInternet];
        }
    }
}
