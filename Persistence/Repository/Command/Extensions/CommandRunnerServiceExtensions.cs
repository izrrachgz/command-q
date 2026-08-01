using Common.Enumerations;
using Persistence.Interfaces;
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
        public static void AddCommandRunners(this IServiceCollection services)
        {
            services.AddScoped<PgSqlCommandRunner>();
            services.AddScoped<MsSqlCommandRunner>();
            services.AddScoped<MySqlCommandRunner>();
            services.AddScoped<SqLiteCommandRunner>();

            services.AddScoped<ICommandRunner>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                return configuration.GetDatabaseEngine() switch
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
