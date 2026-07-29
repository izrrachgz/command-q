using Api.Models;
using Api.Services;
using Api.Interfaces;

namespace Api.Extensions
{
    /// <summary>
    /// Registers account/session cache services used by authentication refresh flows.
    /// </summary>
    public static class AccountSessionCacheExtensions
    {
        public static IServiceCollection AddAccountSessionCache(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<IAccountSessionCache, AccountSessionCache>();
            services.AddHostedService<AccountSessionCacheCleanupService>();

            return services;
        }
    }
}
