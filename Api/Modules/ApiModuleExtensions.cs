using Api.Extensions;
using Api.Modules.Files;
using Api.Modules.Contacts;
using Api.Modules.Identity;
using Api.Modules.Addresses;
using Api.Modules.Configuration;
using Api.Modules.Notifications;

namespace Api.Modules
{
    public static class ApiModuleExtensions
    {
        public static IEndpointRouteBuilder MapApiModules(this IEndpointRouteBuilder endpoints, IConfiguration configuration)
        {
            var controllers = endpoints.MapControllers().RequireRateLimiting(configuration);

            controllers
                .MapIdentityModule()
                .MapConfigurationModule()
                .MapContactsModule()
                .MapAddressesModule()
                .MapFilesModule()
                .MapNotificationsModule();

            return endpoints;
        }
    }
}
