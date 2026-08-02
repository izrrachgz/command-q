using Microsoft.Extensions.DependencyInjection;
using Application.Modules.Identity.Accounts.Services;
using Application.Modules.Identity.AccountActivity.Services;

namespace Application.Modules.Identity
{
    public static class IdentityModuleExtensions
    {
        public static IServiceCollection AddIdentityModule(this IServiceCollection services)
        {
            services.AddScoped<AccountService>();
            services.AddScoped<IAccountActivityRecorder, AccountActivityRecorder>();

            return services;
        }
    }
}
