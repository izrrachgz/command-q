using Api.Constants;
using Api.Extensions;
using Application.Migrations.Extensions;
using Application.Modules.Health.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();

builder.ConfigureServices();

var app = builder.Build();
IConfigurationSection apiConfiguration = app.Configuration.GetRequiredSection(ApiConfigKeys.Section);

app.ValidateStartupConfiguration();

app.ConfigurePipeline(apiConfiguration, builder.Environment);

app.Logger.LogApplicationConfigurationConstants(apiConfiguration, builder.Environment);

// Run migrations before starting the application
if (app.Environment.IsDevelopment() || apiConfiguration.GetValue(ApiConfigKeys.Migrations.RunOnStartup, false))
{
    await app.RunApplicationMigrations();
}

await app.RunStartupHealthChecks();

app.Run();
