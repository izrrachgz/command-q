using MediatR;
using Common.Cqrs;
using Common.Extensions;
using Application.Modules;
using Localization.Extensions;
using Application.Abstractions;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repository.Command.Extensions;

namespace Application.Extensions
{
    /// <summary>
    /// Provides the Application project service-composition entry point.
    /// </summary>
    public static class ApplicationServiceExtensions
    {
        /// <summary>
        /// Configures essential services and middleware for the application, including dependency injection for
        /// Application modules own their handlers, validators, mappers and feature services.
        /// This method composes those modules with shared repositories, command runners and localization.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">The host configuration used by application infrastructure and modules.</param>
        public static void ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Module-owned handlers are registered explicitly below. MediatR still requires an assembly
            // for its core registration, so scan its own assembly rather than the application assembly.
            services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(IMediator).Assembly));
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
            services.AddLocalizationServices(configuration.GetSection("Localization"));
        }
    }
}
