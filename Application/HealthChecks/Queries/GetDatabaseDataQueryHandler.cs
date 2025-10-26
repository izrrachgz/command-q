#nullable disable
using AutoMapper;
using Common.Cqrs;
using Domain.Entities;
using Domain.Interfaces;
using SqlKata.Execution;
using Application.HealthChecks.Dtos;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Queries
{
    public class GetDatabaseDataQueryHandler : IQueryHandler<GetDatabaseDataQuery, HealthCheckResult>, IHealthCheck
    {
        private readonly IMapper _mapper;
        private readonly IReadOnlyRepository _readOnlyRepository;

        public GetDatabaseDataQueryHandler(IMapper mapper, IReadOnlyRepository readOnlyRepository)
        {
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

            var records = await query.GetAsync<HealthCheckLog>(cancellationToken: cancellationToken);

            var result = _mapper.Map<HealthCheckResponseModel>(records);

            bool success = result is not null;

            if (!success)
            {
                return HealthCheckResult.Unhealthy($@"Application is unable to read last health check execution.");
            }

            HealthCheckResponseModel lastOne = result;

            return HealthCheckResult.Healthy($@"Application is able to read from database, last health check ran at : {lastOne.Modified:G}.");
        }
    }
}
