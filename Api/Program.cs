using Api.Extensions;
using Persistence.Migrations.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureServices();

var app = builder.Build();

app.ConfigurePipeline(builder.Configuration, builder.Environment);

//Run database migrations on startup
await app.RunMigrations();

app.Run();
