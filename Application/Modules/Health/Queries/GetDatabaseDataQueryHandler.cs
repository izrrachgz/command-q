using Common.Cqrs;
using Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using Application.Modules.Health.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Queries
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
            var query = _readOnlyRepository.Query($@"{nameof(HealthCheckLog)}")
                .WhereNull($@"{nameof(HealthCheckLog.Deleted)}")
                .OrderByDesc($@"{nameof(HealthCheckLog.Modified)}")
                .Limit(1);

            var record = await _readOnlyRepository.FirstOrDefaultAsync<HealthCheckLog>(query, cancellationToken);

            if (record is null)
            {
                _logger.DependencyCheckFailed("database connection");

                return HealthCheckResult.Unhealthy(@"Application is unable to read last health check execution.");
            }

            _logger.DependencyCheckSucceeded("database connection");

            return HealthCheckResult.Healthy($@"Application is able to read from database, last health check ran at : {record.Modified:G}.");
        }
    }
}
