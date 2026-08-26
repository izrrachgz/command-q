using FluentValidation;
using Time.Extensions;
using Common.Models.Response;
using Application.Abstractions;
using Application.Extensions;
using Application.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Application.Modules.Configuration.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Application.Modules.Configuration.Queries;
using Application.Modules.Configuration.Commands;
using Application.Modules.Configuration.Mappings;

namespace Application.Modules.Configuration
{
    /// <summary>Registers application-configuration CQRS and immutable startup options.</summary>
    public static class ConfigurationModuleExtensions
    {
        public static IServiceCollection AddConfigurationModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<ApplicationOptions>()
                .Bind(configuration.GetSection(ApplicationOptions.Section))
                .Validate(options => DateTimeUtcExtensions.IsValidTimeZone(options.TimeZone),
                    "Api:Application:TimeZone must be a valid IANA/TZDB timezone identifier.")
                .ValidateOnStart();

            services.AddApplicationRequest<
                RecordApplicationConfigurationCommand,
                BasicResponse,
                RecordApplicationConfigurationCommandHandler>();
            services.AddApplicationRequest<
                GetApplicationConfigurationsQuery,
                PagedResponse<ApplicationConfigurationModel>,
                GetApplicationConfigurationsQueryHandler>();
            services.AddScoped<IValidator<RecordApplicationConfigurationCommand>, RecordApplicationConfigurationCommandValidator>();
            services.AddScoped<IValidator<GetApplicationConfigurationsQuery>, GetApplicationConfigurationsQueryValidator>();
            services.AddSingleton<
                IMapper<Domain.Modules.Configuration.ApplicationConfiguration, ApplicationConfigurationModel>,
                ApplicationConfigurationMapper>();
            return services;
        }
    }
}
