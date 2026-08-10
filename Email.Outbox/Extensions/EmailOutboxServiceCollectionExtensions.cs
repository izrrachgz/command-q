using Email.Outbox.Services;
using Email.Outbox.Constants;
using Email.Outbox.Abstractions;
using Email.Outbox.Configuration;
using Email.Outbox.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Email.Outbox.Extensions
{
    public static class EmailOutboxServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            return services.AddEmailOutbox(configuration.GetSection(
                EmailOutboxConstants.ConfigurationSection));
        }

        public static IServiceCollection AddEmailOutbox(this IServiceCollection services, IConfigurationSection section)
        {
            return services
                .AddEmailOutboxProducer(section)
                .AddEmailOutboxProcessor();
        }

        public static IServiceCollection AddEmailOutboxProducer(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            return services.AddEmailOutboxProducer(configuration.GetSection(
                EmailOutboxConstants.ConfigurationSection));
        }

        public static IServiceCollection AddEmailOutboxProducer(this IServiceCollection services, IConfigurationSection section)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(section);

            services.AddOptions<EmailOutboxOptions>()
                .Bind(section)
                .ValidateOnStart();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IValidateOptions<EmailOutboxOptions>, EmailOutboxOptionsValidator>());
            services.TryAddSingleton<IEmailOutboxSerializer, EmailOutboxSerializer>();
            services.TryAddSingleton<IEmailOutbox, EmailOutboxService>();

            return services;
        }

        public static IServiceCollection AddEmailOutboxProcessor(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<IEmailOutboxProcessor, EmailOutboxProcessor>();

            return services;
        }
    }
}
