using Common.Cqrs;
using SqlKata.Execution;
using Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using Application.Modules.Health.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Commands
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

            int affected = await _repository.Db.Query($@"{nameof(HealthCheckLog)}")
                .InsertAsync(@params, cancellationToken: cancellationToken);

            if (affected == 0)
            {
                _logger.DependencyCheckFailed("database write");

                return HealthCheckResult.Unhealthy($@"Application is unable to write on database.");
            }

            _logger.DependencyCheckSucceeded("database write");

            return HealthCheckResult.Healthy($@"Application is able to write on database.");
        }
    }
}
