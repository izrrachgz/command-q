using Email.Worker.Services;
using Email.Worker.Constants;
using Email.Worker.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Email.Worker.Extensions
{
    internal static class EmailWorkerServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailOutboxWorker(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<EmailWorkerOptions>()
                .Bind(configuration.GetSection(EmailWorkerConstants.ConfigurationSection))
                .ValidateOnStart();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IValidateOptions<EmailWorkerOptions>, EmailWorkerOptionsValidator>());
            services.TryAddSingleton(TimeProvider.System);
            services.AddHostedService<EmailOutboxBackgroundService>();

            return services;
        }
    }
}
