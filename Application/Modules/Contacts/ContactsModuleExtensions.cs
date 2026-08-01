using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules.Contacts
{
    public static class ContactsModuleExtensions
    {
        public static IServiceCollection AddContactsModule(this IServiceCollection services)
        {
            return services;
        }
    }
}
