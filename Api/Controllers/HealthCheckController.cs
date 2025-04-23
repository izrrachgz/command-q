using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.HealthChecks.Queries;
using Application.HealthChecks.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Controllers
{
    [ApiController, Authorize, Route("[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly ISender _sender;

        public HealthCheckController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Checks if application is healthy enough in order to be able to properly run
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(@"scan")]
        public async Task<List<HealthCheckResult>> Scan(CancellationToken cancellationToken)
        {
            var writeLocalStorage = new SaveLocalStorageDataCommand();
            var canWriteOnLocalStorage = await _sender.Send(writeLocalStorage, cancellationToken);

            var writeDb = new SaveDatabaseDataCommand();
            var canWriteOnDb = await _sender.Send(writeDb, cancellationToken);

            var readDb = new GetDatabaseDataQuery();
            var canReadOnDb = await _sender.Send(readDb, cancellationToken);

            var accessInternet = new CheckInternetConnectionQuery();
            var canAccessInternet = await _sender.Send(accessInternet, cancellationToken);

            return new List<HealthCheckResult>(4) { canWriteOnLocalStorage, canWriteOnDb, canReadOnDb, canAccessInternet };
        }
    }
}