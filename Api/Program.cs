using Api.Constants;
using Api.Extensions;
using Application.Migrations.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();

builder.ConfigureServices();

var app = builder.Build();
IConfigurationSection apiConfiguration = app.Configuration.GetRequiredSection(ConfigKeys.Section);

app.ValidateStartupConfiguration();

app.ConfigurePipeline(apiConfiguration, builder.Environment);

app.Logger.LogAppConfig(apiConfiguration, builder.Environment);

// Run migrations before starting the application
if (app.Environment.IsDevelopment() || apiConfiguration.GetValue(ConfigKeys.Migrations.RunOnStartup, false))
{
    await app.RunApplicationMigrations();
}

app.Run();
