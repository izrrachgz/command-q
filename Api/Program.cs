using Api.Extensions;
using Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();

builder.ConfigureServices();

var app = builder.Build();

app.ConfigurePipeline(builder.Configuration, builder.Environment);

app.Logger.LogAppConfig(builder.Configuration, builder.Environment);

// Run migrations before starting the application    
await app.RunApplicationMigrations();

app.Run();
