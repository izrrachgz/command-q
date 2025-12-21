using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.HealthChecks.Queries;
using Application.HealthChecks.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Controllers
{
    [ApiController, Authorize, Route("[controller]")]
    public class HealthCheckController(ISender sender, ILogger<HealthCheckController> logger) : ControllerBase
    {

        /// <summary>
        /// Checks if application is healthy enough in order to be able to properly run
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(@"scan")]
        public async Task<List<HealthCheckResult>> Scan(CancellationToken cancellationToken)
        {
            logger.LogDebug(@"Checking application health status.");

            var writeLocalStorage = new SaveLocalStorageDataCommand();
            var canWriteOnLocalStorage = await sender.Send(writeLocalStorage, cancellationToken);

            var writeDb = new SaveDatabaseDataCommand();
            var canWriteOnDb = await sender.Send(writeDb, cancellationToken);

            var readDb = new GetDatabaseDataQuery();
            var canReadOnDb = await sender.Send(readDb, cancellationToken);

            var accessInternet = new CheckInternetConnectionQuery();
            var canAccessInternet = await sender.Send(accessInternet, cancellationToken);

            return [canWriteOnLocalStorage, canWriteOnDb, canReadOnDb, canAccessInternet];
        }
    }
}