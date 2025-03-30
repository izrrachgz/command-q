using Domain.Interfaces;
using Persistence.Repository.Command;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Repository.Extensions
{
    public static class DIRepositoryExtensions
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICommandRunner, NpgsqlCommandRunner>();
            services.AddScoped<IWritableRepository, WritableRepository>();
            services.AddScoped<IReadOnlyRepository, ReadOnlyRepository>();
        }
    }
}
