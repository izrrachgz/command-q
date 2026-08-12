using Application.Modules.Files;
using Application.Modules.Health;
using Application.Modules.Contacts;
using Application.Modules.Identity;
using Application.Modules.Addresses;
using Application.Modules.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddNotificationsModule();

            return services;
        }

    }
}
