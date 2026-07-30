using Api.Services;
using Common.System;
using Api.Interfaces;
using Common.Interfaces;

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
            services.AddSingleton(typeof(ITypeCache<>), typeof(TypeMemoryCache<>));
            services.AddSingleton<IAccountSessionCache, AccountSessionCache>();
            services.AddHostedService<AccountSessionCacheCleanupService>();

            return services;
        }
    }
}
