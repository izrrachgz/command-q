using MediatR;
using Common.Cqrs;
using FluentValidation;
using Application.Test.Constants;
using Persistence.Repository.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            //Automapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            //Meadiator cqrs
            services.AddMediatR(config => config.RegisterServicesFromAssembly(applicationAssembly));
            services.AddValidatorsFromAssembly(applicationAssembly, ServiceLifetime.Singleton);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

            //Repositories
            services.AddFyiRepositories();

            //Add application services
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
        }
    }
}
