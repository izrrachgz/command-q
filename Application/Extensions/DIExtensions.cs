using MediatR;
using Common.Cqrs;
using FluentValidation;
using Common.Extensions;
using Localization.Extensions;
using Microsoft.Extensions.Hosting;
using Persistence.Repository.Extensions;
using Persistence.Migrations.Extensions;
using Application.HealthChecks.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repository.Command.Extensions;

namespace Application.Extensions
{
    public static class DIExtensions
    {
        /// <summary>
        /// Configures essential services and middleware for the application, including dependency injection for
        /// AutoMapper, MediatR, validation, localization, and health checks.
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureApplicationServices(this IServiceCollection services)
        {
            var applicationAssembly = typeof(ProjectLinkReference).Assembly;

            // Auto mapper config for all assemblies
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // MediatR + validation pipeline
            services.AddMediatR(config => config.RegisterServicesFromAssembly(applicationAssembly));
            services.AddValidatorsFromAssembly(applicationAssembly, ServiceLifetime.Singleton);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

            // Repositories
            services.AddRepositories();

            //// Command Runners
            services.AddCommandRunners();

            // Task runner
            services.AddTaskRunner();

            // Add application services (convention-based registration)
            applicationAssembly.ExportedTypes
                .Where(x => !x.IsAbstract && !x.IsInterface && x.Name.EndsWith("Service"))
                .ToList()
                .ForEach(service =>
                {
                    var @interface = service.GetInterface("I" + service.Name);
                    if (@interface != null)
                    {
                        services.AddTransient(@interface, service);
                    }
                });

            // Configure health checks
            services.ConfigureAppHealthChecks();

            // Add localization services
            services.AddLocalizationServices();
        }

        /// <summary>
        /// Runs database migrations on application startup to ensure the database schema is up to date with the application's
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static async Task RunApplicationMigrations(this IHost host)
        {
            //Run database migrations on startup
            await host.RunMigrations();
        }
    }
}
