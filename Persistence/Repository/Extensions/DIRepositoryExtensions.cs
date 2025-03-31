using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Repository.Extensions
{
    public static class DIRepositoryExtensions
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IWritableRepository, WritableRepository>();
            services.AddScoped<IReadOnlyRepository, ReadOnlyRepository>();
        }
    }
}
