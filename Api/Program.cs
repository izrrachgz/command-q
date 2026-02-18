using MediatR;
using Common.Cqrs;
using Api.Constants;
using Api.Extensions;
using Api.Middleware;
using Common.Constants;
using FluentValidation;
using Common.Extensions;
using Localization.Extensions;
using Persistence.Repository.Extensions;
using Persistence.Migrations.Extensions;
using Application.HealthChecks.Extensions;
using Persistence.Repository.Command.Extensions;

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

//Command Runners
builder.Services.AddCommandRunners();

//Task runner
builder.Services.AddTaskRunner();

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

//Add jwt authorization flow
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);

//Add localization services
builder.Services.AddLocalizationServices();

//Add rate limiting policies
builder.Services.AddFixedWindowRateLimit(builder.Configuration);

builder.Services.AddSlidingWindowRateLimit(builder.Configuration);

builder.Services.AddTokenBucketRateLimit(builder.Configuration);

builder.Services.AddConcurrencyRateLimit(builder.Configuration);

var app = builder.Build();

//Add rate limiting
app.UseRateLimiter();

//Configure localization
app.UseRequestLocalization(options =>
{
    string[] supportedCultures = [Language.English, Language.Spanish];

    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
    options.SetDefaultCulture(Language.English);
});

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

app.MapControllers().RequireRateLimiting(ApiConfiguration.RateLimitingSlidingPolicyName);

await app.RunMigrations();

app.Run();
