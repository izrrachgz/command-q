using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Time.Extensions
{
    /// <summary>
    /// Registers the shared platform time provider while preserving host and test overrides.
    /// </summary>
    public static class TimeServiceCollectionExtensions
    {
        public static IServiceCollection AddTimeServices(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton(TimeProvider.System);
            return services;
        }
    }
}
