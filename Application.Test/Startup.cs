using MediatR;
using Serilog;
using Common.Cqrs;
using FluentValidation;
using Common.Extensions;
using Application.Constants;
using Application.Interfaces;
using Application.Extensions;
using Localization.Extensions;
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
            var applicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(AppConfig.ConfigurationFileName, optional: false)
                .AddUserSecrets(AppConfig.UserSecrets)
                .AddEnvironmentVariables()
                .Build();

            //Configuration
            services.AddSingleton((IConfiguration)configuration);

            services.ConfigureApplicationServices(configuration);

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
