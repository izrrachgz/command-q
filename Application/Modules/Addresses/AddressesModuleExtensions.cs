using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules.Addresses
{
    public static class AddressesModuleExtensions
    {
        public static IServiceCollection AddAddressesModule(this IServiceCollection services)
        {
            return services;
        }
    }
}
