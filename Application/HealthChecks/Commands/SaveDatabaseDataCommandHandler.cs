using Common.Cqrs;
using Domain.Entities;
using Domain.Interfaces;
using SqlKata.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Commands
{
    public class SaveDatabaseDataCommandHandler : ICommandHandler<SaveDatabaseDataCommand, HealthCheckResult>, IHealthCheck
    {
        private readonly IWritableRepository _repository;
        private readonly ILogger<SaveDatabaseDataCommandHandler> _logger;

        public SaveDatabaseDataCommandHandler(IWritableRepository repository, ILogger<SaveDatabaseDataCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return await Handle(new SaveDatabaseDataCommand(), cancellationToken);
        }

        public async Task<HealthCheckResult> Handle(SaveDatabaseDataCommand request, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            var @params = new
            {
                Created = now,
                Modified = now
            };

            int affected = await _repository.Db.Query($"{nameof(HealthCheckLog)}")
                .InsertAsync(@params, cancellationToken: cancellationToken);

            if (affected == 0)
            {
                _logger.LogDebug(@"Checking database write : no access.");

                return HealthCheckResult.Unhealthy($@"Application is unable to write on database.");
            }

            _logger.LogDebug(@"Checking database write : access granted.");

            return HealthCheckResult.Healthy($@"Application is able to write on database.");
        }
    }
}
