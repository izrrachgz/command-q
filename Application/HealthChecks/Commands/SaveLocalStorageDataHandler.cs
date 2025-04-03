using Common.Cqrs;
using Application.Constants;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.HealthChecks.Commands
{
    public class SaveLocalStorageCommandHandler : ICommandHandler<SaveLocalStorageDataCommand, HealthCheckResult>, IHealthCheck
    {

        public SaveLocalStorageCommandHandler()
        {
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
                string[] content = new string[1]
                {
                    now.ToString("G")
                };

                string filePath = $@"{basePath}HealthCheckLog.log";

                if (!File.Exists(filePath))
                {
                    using var file = File.CreateText(filePath);
                    file.Dispose();
                }

                await File.AppendAllLinesAsync(filePath, content, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return HealthCheckResult.Unhealthy(ApplicationMessages.CommandCancelled);
                }

                return HealthCheckResult.Healthy($@"Application is able to write on local storage.");
            }
            catch
            {
                return HealthCheckResult.Unhealthy($@"Application is unable to write on local storage.");
            }
        }
    }
}
