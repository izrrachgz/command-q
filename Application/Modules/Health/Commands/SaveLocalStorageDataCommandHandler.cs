using Common.Cqrs;
using Microsoft.Extensions.Logging;
using Application.Modules.Health.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Modules.Health.Commands
{
    internal sealed class SaveLocalStorageDataCommandHandler : ICommandHandler<SaveLocalStorageDataCommand, HealthCheckResult>, IHealthCheck
    {
        private readonly ILogger<SaveLocalStorageDataCommandHandler> _logger;

        public SaveLocalStorageDataCommandHandler(ILogger<SaveLocalStorageDataCommandHandler> logger)
        {
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return await Handle(new SaveLocalStorageDataCommand(), cancellationToken);
        }

        public async Task<HealthCheckResult> Handle(SaveLocalStorageDataCommand request, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                string[] content = [now.ToString("G")];

                string filePath = $@"{basePath}HealthCheckLog.log";

                if (!File.Exists(filePath))
                {
                    using var file = File.CreateText(filePath);
                    file.Dispose();
                }

                await File.AppendAllLinesAsync(filePath, content, cancellationToken);

                _logger.DependencyCheckSucceeded("storage write");

                return HealthCheckResult.Healthy($@"Application is able to write on local storage.");
            }
            catch
            {
                _logger.DependencyCheckFailed("storage write");

                return HealthCheckResult.Unhealthy($@"Application is unable to write on local storage.");
            }
        }
    }
}
