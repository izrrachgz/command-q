using IdentityServer.Constants;
using IdentityServer.Extensions;
using IdentityServer.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.ConfigureServices();

var app = builder.Build();
IConfigurationSection identityServerConfiguration = app.Configuration
    .GetRequiredSection(ConfigKeys.Section);

app.ConfigurePipeline();
app.Logger.LogAppConfig(identityServerConfiguration, app.Environment);

if (app.Environment.IsDevelopment() ||
    identityServerConfiguration.GetValue(ConfigKeys.Migrations.RunOnStartup, false))
{
    await app.RunIdentityServerMigrations();
}

await app.SeedIdentityServer();

app.Run();
