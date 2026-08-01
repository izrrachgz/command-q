using Api.Extensions;
using Api.Modules.Files;
using Api.Modules.Health;
using Api.Modules.Contacts;
using Api.Modules.Identity;
using Api.Modules.Addresses;

namespace Api.Modules
{
    public static class ApiModuleExtensions
    {
        public static IEndpointRouteBuilder MapApiModules(this IEndpointRouteBuilder endpoints, IConfiguration configuration)
        {
            var controllers = endpoints.MapControllers().RequireRateLimiting(configuration);

            controllers
                .MapIdentityModule()
                .MapContactsModule()
                .MapAddressesModule()
                .MapFilesModule()
                .MapHealthModule();

            return endpoints;
        }
    }
}
