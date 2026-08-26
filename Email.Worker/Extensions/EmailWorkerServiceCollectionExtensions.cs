using Email.Worker.Services;
using Time.Extensions;
using Email.Worker.Constants;
using Email.Worker.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Email.Worker.Extensions
{
    internal static class EmailWorkerServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailOutboxWorker(this IServiceCollection services, IConfigurationSection section)
        {
            services.AddOptions<EmailWorkerOptions>()
                .Bind(section)
                .ValidateOnStart();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IValidateOptions<EmailWorkerOptions>, EmailWorkerOptionsValidator>());
            services.AddTimeServices();
            services.AddHostedService<EmailOutboxBackgroundService>();

            return services;
        }

        public static IServiceCollection AddEmailWorkerMigrations(this IServiceCollection services, IConfigurationSection section)
        {
            services.AddOptions<EmailWorkerMigrationOptions>()
                .Bind(section)
                .ValidateOnStart();

            return services;
        }
    }
}
