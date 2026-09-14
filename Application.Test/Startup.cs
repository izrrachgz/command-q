using Serilog;
using NodaTime;
using Email.Client.Models;
using FluentValidation;
using Common.Extensions;
using Email.Client.Abstractions;
using Application.Constants;
using Application.Extensions;
using Localization.Extensions;
using Application.Abstractions;
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
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(ApplicationConfigurationConstants.ConfigurationFileName, optional: false)
                .AddUserSecrets(ApplicationConfigurationConstants.UserSecrets)
                .AddEnvironmentVariables()
                .Build();

            //Configuration
            services.AddSingleton((IConfiguration)configuration);

            services.ConfigureApplicationServices(configuration);
            services.AddSingleton<IEmailQueueClient, ApplicationCompositionOutbox>();

            //Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            //Add logging services
            services.AddLogging(builder => builder.AddSerilog(dispose: true));
        }

        private sealed class ApplicationCompositionOutbox : IEmailQueueClient
        {
            public Task<EmailQueueResult> EnqueueAsync(EmailQueueRequest request, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new EmailQueueResult(Guid.Empty, false, SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc()));
            }
        }
    }
}
