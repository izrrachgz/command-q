using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Repository.Command.Extensions
{
    public static class CommandExtensions
    {
        public static void AddCommandRunners(this IServiceCollection services)
        {
            services.AddScoped<ICommandRunner, PgSqlCommandRunner>();
            services.AddScoped<ICommandRunner, MsSqlCommandRunner>();
            services.AddScoped<ICommandRunner, MySqlCommandRunner>();
            services.AddScoped<ICommandRunner, SqLiteCommandRunner>();
        }
    }
}
