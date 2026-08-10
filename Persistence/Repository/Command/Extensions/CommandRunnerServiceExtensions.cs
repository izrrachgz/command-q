using Common.Enumerations;
using Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Repository.Command.Extensions
{
    /// <summary>
    /// Registers database command runners and selects the default runner from configuration.
    /// </summary>
    public static class CommandRunnerServiceExtensions
    {
        /// <summary>
        /// Registers all concrete command runners and maps <see cref="ICommandRunner"/> to the configured database engine.
        /// </summary>
        public static void AddCommandRunners(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            string connectionString = configuration.GetDatabaseConnection();
            DatabaseEngine engine = configuration.GetDatabaseEngine();

            services.AddScoped(serviceProvider => new PgSqlCommandRunner(connectionString, serviceProvider.GetRequiredService<ILogger<PgSqlCommandRunner>>()));
            services.AddScoped(serviceProvider => new MsSqlCommandRunner(connectionString, serviceProvider.GetRequiredService<ILogger<MsSqlCommandRunner>>()));
            services.AddScoped(serviceProvider => new MySqlCommandRunner(connectionString, serviceProvider.GetRequiredService<ILogger<MySqlCommandRunner>>()));
            services.AddScoped(serviceProvider => new SqLiteCommandRunner(connectionString, serviceProvider.GetRequiredService<ILogger<SqLiteCommandRunner>>()));

            services.AddScoped<ICommandRunner>(serviceProvider =>
            {
                return engine switch
                {
                    DatabaseEngine.MsSql => serviceProvider.GetRequiredService<MsSqlCommandRunner>(),
                    DatabaseEngine.MySql => serviceProvider.GetRequiredService<MySqlCommandRunner>(),
                    DatabaseEngine.SqLite => serviceProvider.GetRequiredService<SqLiteCommandRunner>(),
                    DatabaseEngine.PgSql => serviceProvider.GetRequiredService<PgSqlCommandRunner>(),
                    _ => serviceProvider.GetRequiredService<PgSqlCommandRunner>()
                };
            });
        }
    }
}
