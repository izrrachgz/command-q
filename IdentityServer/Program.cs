using IdentityServer.Constants;
using IdentityServer.Extensions;
using IdentityServer.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.ConfigureServices();

var app = builder.Build();
IConfigurationSection identityServerConfiguration = app.Configuration
    .GetRequiredSection(IdentityServerConfigKeys.Section);

app.ConfigurePipeline();
app.Logger.LogApplicationConfigurationConstants(identityServerConfiguration, app.Environment);

if (app.Environment.IsDevelopment() ||
    identityServerConfiguration.GetValue(IdentityServerConfigKeys.Migrations.RunOnStartup, false))
{
    await app.RunIdentityServerMigrations();
}

await app.SeedIdentityServer();

app.Run();
