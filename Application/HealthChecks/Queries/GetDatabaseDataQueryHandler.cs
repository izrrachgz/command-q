using Common.Cqrs;
using Domain.Entities;
using SqlKata.Execution;
using Domain.Interfaces;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Application.HealthChecks.Dtos;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Queries
{
    public class GetDatabaseDataQueryHandler : IQueryHandler<GetDatabaseDataQuery, HealthCheckResult>, IHealthCheck
    {
        private readonly IMapper<HealthCheckLog, HealthCheckResponseModel> _mapper;
        private readonly ILogger<GetDatabaseDataQueryHandler> _logger;
        private readonly IReadOnlyRepository _readOnlyRepository;

        public GetDatabaseDataQueryHandler(IMapper<HealthCheckLog, HealthCheckResponseModel> mapper, IReadOnlyRepository readOnlyRepository, ILogger<GetDatabaseDataQueryHandler> logger)
        {
            _logger = logger;
            _mapper = mapper;
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
                .Take(1);

            var records = await query.FirstOrDefaultAsync<HealthCheckLog>(cancellationToken: cancellationToken);

            var result = _mapper.Map(records);

            bool success = result is not null;

            if (!success)
            {
                _logger.LogDebug(@"Checking database connection : no access.");

                return HealthCheckResult.Unhealthy(@"Application is unable to read last health check execution.");
            }

            HealthCheckResponseModel lastOne = result;

            _logger.LogDebug(@"Checking database connection : access granted.");

            return HealthCheckResult.Healthy($@"Application is able to read from database, last health check ran at : {lastOne.Modified:G}.");
        }
    }
}
