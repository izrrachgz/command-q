using Email.Extensions;
using Email.Configuration;
using Email.Worker.Logging;
using Email.Worker.Constants;
using Email.Outbox.Extensions;
using Email.Worker.Extensions;
using Email.Worker.Configuration;
using Microsoft.Extensions.Options;
using Email.Outbox.Persistence.Extensions;
using Email.Outbox.Persistence.Configuration;

var builder = Host.CreateApplicationBuilder(args);

IConfigurationSection workerConfiguration = builder.Configuration.GetRequiredSection(EmailWorkerConstants.ConfigurationSection);
IConfigurationSection outboxConfiguration = workerConfiguration.GetRequiredSection(EmailWorkerConstants.OutboxSection);

builder.Services.AddEmailServices(workerConfiguration.GetRequiredSection(EmailWorkerConstants.EmailSection));
builder.Services.AddEmailOutbox(outboxConfiguration);
builder.Services.AddEmailOutboxPersistence(outboxConfiguration.GetRequiredSection(EmailWorkerConstants.PersistenceSection));
builder.Services.AddEmailOutboxWorker(workerConfiguration.GetRequiredSection(EmailWorkerConstants.WorkerSection));
builder.Services.AddEmailWorkerMigrations(workerConfiguration.GetRequiredSection(EmailWorkerConstants.MigrationsSection));

var host = builder.Build();

var migrationOptions = host.Services.GetRequiredService<IOptions<EmailWorkerMigrationOptions>>().Value;
var workerOptions = host.Services.GetRequiredService<IOptions<EmailWorkerOptions>>().Value;
var emailOptions = host.Services.GetRequiredService<IOptions<EmailOptions>>().Value;
var persistenceOptions = host.Services.GetRequiredService<IOptions<EmailOutboxPersistenceOptions>>().Value;

ILogger startupLogger = host.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger(EmailWorkerConstants.StartupLoggerCategory);

startupLogger.StartupConfiguration(
    builder.Environment.EnvironmentName,
    persistenceOptions.Engine.ToString(),
    migrationOptions.RunOnStartup,
    workerOptions.Enabled,
    emailOptions.Enabled);

if (migrationOptions.RunOnStartup)
{
    await host.RunEmailOutboxMigrations();
}

await host.RunAsync();
