using Application.Modules.Files;
using Application.Modules.Health;
using Application.Modules.Contacts;
using Application.Modules.Identity;
using Application.Modules.Addresses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Application.Modules
{
    public static class ApplicationModuleExtensions
    {
        public static IServiceCollection AddApplicationModules(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityModule();
            services.AddHealthModule(configuration);
            services.AddContactsModule();
            services.AddAddressesModule();
            services.AddFilesModule();

            return services;
        }

    }
}
