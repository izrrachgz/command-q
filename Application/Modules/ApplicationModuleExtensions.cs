using FluentValidation;
using Common.Models.Request;
using Application.Validation;
using Application.Modules.Files;
using Application.Modules.Health;
using Application.Modules.Contacts;
using Application.Modules.Identity;
using Application.Modules.Addresses;
using Application.Modules.Configuration;
using Application.Modules.Notifications;
using Time.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules
{
    /// <summary>
    /// Provides the composition entry point for application feature modules.
    /// </summary>
    public static class ApplicationModuleExtensions
    {
        /// <summary>
        /// Registers all application feature modules.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">The host configuration used by configuration-aware modules.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddApplicationModules(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTimeServices();
            services.AddScoped<IValidator<SearchCriteria>, SearchCriteriaValidator>();

            services.AddIdentityModule();
            services.AddHealthModule(configuration);
            services.AddContactsModule();
            services.AddAddressesModule();
            services.AddFilesModule();
            services.AddNotificationsModule();
            services.AddConfigurationModule(configuration);

            return services;
        }

    }
}
