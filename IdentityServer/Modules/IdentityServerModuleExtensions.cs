using IdentityServer.Modules.Health;
using IdentityServer.Modules.Accounts;
using IdentityServer.Modules.OpenIddict;
using IdentityServer.Persistence;

namespace IdentityServer.Modules
{
    internal static class IdentityServerModuleExtensions
    {
        public static IServiceCollection AddIdentityServerModules(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            var identityBuilder = services.AddAccountsModule(configuration);
            services.AddIdentityServerPersistence(configuration, identityBuilder);
            services.AddOpenIddictModule(configuration, environment);
            services.AddHealthModule();

            services.AddScoped<OpenIddictSeeder>();

            return services;
        }

        public static IEndpointRouteBuilder MapIdentityServerModules(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapControllers();
            endpoints.MapHealthModule();

            return endpoints;
        }
    }
}
