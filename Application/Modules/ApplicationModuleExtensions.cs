using Application.Modules.Files;
using Application.Modules.Health;
using Application.Modules.Contacts;
using Application.Modules.Identity;
using Application.Modules.Addresses;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules
{
    public static class ApplicationModuleExtensions
    {
        public static IServiceCollection AddApplicationModules(this IServiceCollection services)
        {
            services.AddIdentityModule();
            services.AddHealthModule();
            services.AddContactsModule();
            services.AddAddressesModule();
            services.AddFilesModule();

            return services;
        }

    }
}
