using Common.Enumerations;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repository.Extensions;

namespace Persistence.Repository.Command.Extensions
{
    public static class CommandRunnerServiceExtensions
    {
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
