using Email.Outbox.Abstractions;
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
        public static IServiceCollection AddEmailOutboxPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddOptions<EmailOutboxPersistenceOptions>()
                .Bind(configuration.GetSection(EmailOutboxPersistenceConstants.ConfigurationSection))
                .ValidateOnStart();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<
                IValidateOptions<EmailOutboxPersistenceOptions>,
                EmailOutboxPersistenceOptionsValidator>());
            services.TryAddSingleton<EmailOutboxRepositoryFactory>();
            services.TryAddSingleton<IEmailOutboxStore, SqlKataEmailOutboxStore>();
            services.TryAddSingleton<IEmailOutboxMigrationScriptSource, EmbeddedEmailOutboxMigrationScriptSource>();
            services.TryAddSingleton<EmailOutboxMigrationRunner>();

            return services;
        }

        public static async Task RunEmailOutboxMigrations(
            this IHost host,
            CancellationToken cancellationToken = default)
        {
            var options = host.Services
                .GetRequiredService<IOptions<EmailOutboxPersistenceOptions>>()
                .Value;

            if (!options.RunMigrationsOnStartup)
            {
                return;
            }

            var runner = host.Services.GetRequiredService<EmailOutboxMigrationRunner>();
            await runner.RunAsync(cancellationToken);
        }
    }
}
