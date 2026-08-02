using IdentityServer.Constants;
using IdentityServer.Extensions;
using IdentityServer.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.ConfigureServices();

var app = builder.Build();

app.ConfigurePipeline();
app.Logger.LogAppConfig(app.Configuration, app.Environment);

if (app.Environment.IsDevelopment() ||
    app.Configuration.GetValue(ConfigKeys.Migrations.RunOnStartup, false))
{
    await app.RunIdentityServerMigrations();
}

await app.SeedIdentityServer();

app.Run();
