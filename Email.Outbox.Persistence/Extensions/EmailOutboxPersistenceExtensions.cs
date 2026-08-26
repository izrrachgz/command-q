using Email.Outbox.Abstractions;
using Time.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Email.Outbox.Persistence.Storage;
using Email.Outbox.Persistence.Constants;
using Microsoft.Extensions.Configuration;
using Email.Outbox.Persistence.Migrations;
using Email.Outbox.Persistence.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Email.Outbox.Persistence.Extensions
{
    public static class EmailOutboxPersistenceExtensions
    {
        public static IServiceCollection AddEmailOutboxPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            return services.AddEmailOutboxPersistence(configuration.GetSection(
                EmailOutboxPersistenceConstants.ConfigurationSection));
        }

        public static IServiceCollection AddEmailOutboxPersistence(this IServiceCollection services, IConfigurationSection section)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(section);

            services.AddOptions<EmailOutboxPersistenceOptions>()
                .Bind(section)
                .ValidateOnStart();
            services.AddTimeServices();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<
                IValidateOptions<EmailOutboxPersistenceOptions>,
                EmailOutboxPersistenceOptionsValidator>());
            services.TryAddSingleton<EmailOutboxRepositoryFactory>();
            services.TryAddSingleton<IEmailOutboxStore, SqlKataEmailOutboxStore>();
            services.TryAddSingleton<IEmailOutboxMigrationScriptSource, EmbeddedEmailOutboxMigrationScriptSource>();
            services.TryAddSingleton<EmailOutboxMigrationRunner>();

            return services;
        }

        public static async Task RunEmailOutboxMigrations(this IHost host, CancellationToken cancellationToken = default)
        {
            var runner = host.Services.GetRequiredService<EmailOutboxMigrationRunner>();
            await runner.RunAsync(cancellationToken);
        }
    }
}
