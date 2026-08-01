using Microsoft.Extensions.DependencyInjection;
using Application.Modules.Identity.Accounts.Services;

namespace Application.Modules.Identity
{
    public static class IdentityModuleExtensions
    {
        public static IServiceCollection AddIdentityModule(this IServiceCollection services)
        {
            services.AddScoped<AccountService>();

            return services;
        }
    }
}
