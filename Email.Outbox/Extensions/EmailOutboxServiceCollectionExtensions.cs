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
        public static IServiceCollection AddEmailOutbox(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddOptions<EmailOutboxOptions>()
                .Bind(configuration.GetSection(EmailOutboxConstants.ConfigurationSection))
                .ValidateOnStart();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IValidateOptions<EmailOutboxOptions>, EmailOutboxOptionsValidator>());
            services.TryAddSingleton<IEmailOutboxSerializer, EmailOutboxSerializer>();
            services.TryAddSingleton<IEmailOutbox, EmailOutboxService>();
            services.TryAddSingleton<IEmailOutboxProcessor, EmailOutboxProcessor>();

            return services;
        }
    }
}
