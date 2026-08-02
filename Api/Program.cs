using Api.Extensions;
using Api.Constants;
using Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();

builder.ConfigureServices();

var app = builder.Build();

app.ValidateStartupConfiguration();

app.ConfigurePipeline(builder.Configuration, builder.Environment);

app.Logger.LogAppConfig(builder.Configuration, builder.Environment);

// Run migrations before starting the application
if (!app.Environment.IsEnvironment("Testing") &&
    app.Configuration.GetValue(ConfigKeys.Migrations.RunOnStartup, false))
{
    await app.RunApplicationMigrations();
}

app.Run();
