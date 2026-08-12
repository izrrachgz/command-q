using MediatR;
using Application;
using Common.Cqrs;
using FluentValidation;
using Common.Extensions;
using Application.Modules;
using Application.Abstractions;
using Localization.Extensions;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repository.Command.Extensions;

namespace Application.Extensions
{
    public static class ApplicationServiceExtensions
    {
        /// <summary>
        /// Configures essential services and middleware for the application, including dependency injection for
        /// AutoMapper, MediatR, validation, localization, and health checks.
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var applicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;

            // Add mapping profiles
            applicationAssembly.ExportedTypes
                .Where(x => !x.IsAbstract && !x.IsInterface && x.Name.EndsWith(@"Mapper"))
                .ToList()
                .ForEach(service =>
                {
                    var interfaces = service.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapper<,>));

                    foreach (var @interface in interfaces)
                    {
                        services.AddSingleton(@interface, service);
                    }
                });

            // MediatR + validation pipeline
            services.AddMediatR(config => config.RegisterServicesFromAssembly(applicationAssembly));
            services.AddValidatorsFromAssembly(applicationAssembly, ServiceLifetime.Scoped);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

            // Repositories
            services.AddRepositories(configuration);

            // Command runners
            services.AddCommandRunners(configuration);

            // Task runner
            services.AddTaskRunner();

            // Feature modules
            services.AddApplicationModules(configuration);

            // Add localization services
            services.AddLocalizationServices();
        }
    }
}
