using Email.Extensions;
using Email.Configuration;
using Email.Worker.Logging;
using Email.Worker.Constants;
using Email.Outbox.Extensions;
using Email.Worker.Extensions;
using Email.Worker.Configuration;
using Email.Worker.Modules.Health;
using Microsoft.Extensions.Options;
using Email.Outbox.Persistence.Extensions;
using Email.Outbox.Persistence.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.AddEmailWorkerOpenTelemetry();

IConfigurationSection workerConfiguration = builder.Configuration.GetRequiredSection(EmailWorkerConstants.ConfigurationSection);
IConfigurationSection outboxConfiguration = workerConfiguration.GetRequiredSection(EmailWorkerConstants.OutboxSection);

builder.Services.AddEmailServices(workerConfiguration.GetRequiredSection(EmailWorkerConstants.EmailSection));
builder.Services.AddEmailOutbox(outboxConfiguration);
builder.Services.AddEmailOutboxPersistence(outboxConfiguration.GetRequiredSection(EmailWorkerConstants.PersistenceSection));
builder.Services.AddEmailOutboxWorker(workerConfiguration.GetRequiredSection(EmailWorkerConstants.WorkerSection));
builder.Services.AddEmailWorkerMigrations(workerConfiguration.GetRequiredSection(EmailWorkerConstants.MigrationsSection));
builder.Services.AddEmailWorkerHealthModule();

var app = builder.Build();

var migrationOptions = app.Services.GetRequiredService<IOptions<EmailWorkerMigrationOptions>>().Value;
var workerOptions = app.Services.GetRequiredService<IOptions<EmailWorkerOptions>>().Value;
var emailOptions = app.Services.GetRequiredService<IOptions<EmailOptions>>().Value;
var persistenceOptions = app.Services.GetRequiredService<IOptions<EmailOutboxPersistenceOptions>>().Value;

ILogger startupLogger = app.Services
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
    await app.RunEmailOutboxMigrations();
}

app.MapEmailWorkerHealthModule();

await app.RunAsync();
