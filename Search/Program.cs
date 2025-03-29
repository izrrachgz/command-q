using MediatR;
using Common.Cqrs;
using FluentValidation;
using Search.Middleware;
using Persistence.Repository.Extensions;
using Application.HealthChecks.Extensions;
using Persistence.Migrations.Extensions;

var applicationAssembly = typeof(Application.ProjectLinkReference).Assembly;
var builder = WebApplication.CreateBuilder(args);

//auto mapper config for all assamblies
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//Prefer not to use swagger gen unless it's strictly necessary
//builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(applicationAssembly));
builder.Services.AddValidatorsFromAssembly(applicationAssembly, ServiceLifetime.Singleton);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

//Repositories
builder.Services.AddRepositories();

//Migrations
builder.Services.RunMigrations();

//Add application services
applicationAssembly.ExportedTypes
    .Where(x => !x.IsAbstract && !x.IsInterface && x.Name.EndsWith("Service"))
    .ToList()
    .ForEach(service =>
    {
        var @interface = service.GetInterface("I" + service.Name);
        if (@interface != null)
        {
            builder.Services.AddTransient(@interface, service);
        }
    });

//Register exception handler service
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

//Configure Health checks
builder.Services.ConfigureAppHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health/checks");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //Prefer not to use swagger gen unless it's strictly necessary
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Check for exceptions
app.UseMiddleware<ExceptionHandlingMiddleware>();

//Check for api keys presence
app.UseMiddleware<CheckApiKeysMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
