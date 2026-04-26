using Common.Cqrs;
using Domain.Entities;
using SqlKata.Execution;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Queries
{
    public class GetDatabaseDataQueryHandler : IQueryHandler<GetDatabaseDataQuery, HealthCheckResult>, IHealthCheck
    {
        private readonly IReadOnlyRepository _readOnlyRepository;
        private readonly ILogger<GetDatabaseDataQueryHandler> _logger;

        public GetDatabaseDataQueryHandler(IReadOnlyRepository readOnlyRepository, ILogger<GetDatabaseDataQueryHandler> logger)
        {
            _logger = logger;
            _readOnlyRepository = readOnlyRepository;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return await Handle(new GetDatabaseDataQuery(), cancellationToken);
        }

        public async Task<HealthCheckResult> Handle(GetDatabaseDataQuery request, CancellationToken cancellationToken)
        {
            var query = _readOnlyRepository.Db.Query($@"{nameof(HealthCheckLog)}")
                .WhereNull($@"{nameof(HealthCheckLog.Deleted)}")
                .OrderByDesc($@"{nameof(HealthCheckLog.Modified)}")
                .Limit(1);

            var record = await query.FirstOrDefaultAsync<HealthCheckLog>(cancellationToken: cancellationToken);

            if (record is null)
            {
                _logger.LogDebug(@"Checking database connection : no access.");

                return HealthCheckResult.Unhealthy(@"Application is unable to read last health check execution.");
            }

            _logger.LogDebug(@"Checking database connection : access granted.");

            return HealthCheckResult.Healthy($@"Application is able to read from database, last health check ran at : {record.Modified:G}.");
        }
    }
}
