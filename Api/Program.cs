using Api.Extensions;
using Persistence.Migrations.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();

builder.ConfigureServices();

var app = builder.Build();

app.ConfigurePipeline(builder.Configuration, builder.Environment);

app.LogApplicationConfiguration(builder.Configuration, builder.Environment);

app.Logger.LogInformation(@"Running database migrations...");

//Run database migrations on startup
await app.RunMigrations().ContinueWith(_ =>
{
    app.Logger.LogInformation(@"Database migrations completed. Starting application...");
});

app.Run();