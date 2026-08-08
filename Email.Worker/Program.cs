using Email.Extensions;
using Email.Outbox.Extensions;
using Email.Worker.Extensions;
using Email.Outbox.Persistence.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddEmailServices(builder.Configuration);
builder.Services.AddEmailOutbox(builder.Configuration);
builder.Services.AddEmailOutboxPersistence(builder.Configuration);
builder.Services.AddEmailOutboxWorker(builder.Configuration);

var host = builder.Build();

await host.RunEmailOutboxMigrations();
await host.RunAsync();
