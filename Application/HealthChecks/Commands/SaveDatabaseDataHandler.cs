using Common.Cqrs;
using Domain.Entities;
using Domain.Interfaces;
using SqlKata.Execution;
using Application.Constants;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Commands
{
    public class SaveDataCommandHandler : ICommandHandler<SaveDatabaseDataCommand, HealthCheckResult>, IHealthCheck
    {
        private readonly IWritableRepository _repository;

        public SaveDataCommandHandler(IWritableRepository repository)
        {
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

            if (cancellationToken.IsCancellationRequested)
            {
                return HealthCheckResult.Unhealthy(ApplicationMessages.CommandCancelled);
            }

            if (affected == 0)
            {
                return HealthCheckResult.Unhealthy($@"Application is unable to write on database.");
            }

            return HealthCheckResult.Healthy($@"Application is able to write on database.");
        }
    }
}
