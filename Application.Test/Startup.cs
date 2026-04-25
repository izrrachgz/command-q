using Serilog;
using MediatR;
using Common.Cqrs;
using FluentValidation;
using Common.Extensions;
using Application.Interfaces;
using Localization.Extensions;
using Application.Test.Constants;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repository.Command.Extensions;

namespace Application.Test
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            var applicationAssembly = typeof(ProjectLinkReference).Assembly;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(ApplicationTest.ConfigurationFileName, optional: false)
                .AddUserSecrets(ApplicationTest.UserSecrets)
                .AddEnvironmentVariables()
                .Build();

            //Configuration
            services.AddSingleton((IConfiguration)configuration);


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

            //Meadiator cqrs
            services.AddMediatR(config => config.RegisterServicesFromAssembly(applicationAssembly));
            services.AddValidatorsFromAssembly(applicationAssembly, ServiceLifetime.Singleton);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

            //Repositories
            services.AddRepositories();

            //Command Runners
            services.AddCommandRunners();

            //Task runner
            services.AddTaskRunner();

            //Add application services
            applicationAssembly.ExportedTypes
                .Where(x => !x.IsAbstract && !x.IsInterface && x.Name.EndsWith(@"Service"))
                .ToList()
                .ForEach(service =>
                {
                    var @interface = service.GetInterface(@"I" + service.Name);
                    if (@interface != null)
                    {
                        services.AddTransient(@interface, service);
                    }
                    else
                    {
                        services.AddScoped(service);
                    }
                });


            //Add localization services
            services.AddLocalizationServices();

            //Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            //Add logging services
            services.AddLogging(builder => builder.AddSerilog(dispose: true));
        }
    }
}
