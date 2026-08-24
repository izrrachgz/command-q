using MediatR;
using Serilog;
using NodaTime;
using Common.Cqrs;
using Email.Models;
using FluentValidation;
using Common.Extensions;
using Email.Outbox.Models;
using Application.Constants;
using Application.Extensions;
using Localization.Extensions;
using Application.Abstractions;
using Email.Outbox.Abstractions;
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
            services.AddSingleton<IEmailOutbox, ApplicationCompositionOutbox>();

            //Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            //Add logging services
            services.AddLogging(builder => builder.AddSerilog(dispose: true));
        }

        private sealed class ApplicationCompositionOutbox : IEmailOutbox
        {
            public Task<EmailOutboxEnqueueResult> EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default)
            {
                return EnqueueAsync(new EmailOutboxRequest { Message = message }, cancellationToken);
            }

            public Task<EmailOutboxEnqueueResult> EnqueueAsync(EmailOutboxRequest request, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new EmailOutboxEnqueueResult(Guid.Empty, false, SystemClock.Instance.GetCurrentInstant()));
            }
        }
    }
}
