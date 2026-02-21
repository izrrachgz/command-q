using MediatR;
using Api.Models;
using Common.Cqrs;
using Api.Constants;
using Api.Middleware;
using Api.Enumerations;
using Common.Constants;
using FluentValidation;
using Common.Extensions;
using Common.Enumerations;
using Localization.Extensions;
using Persistence.Repository.Extensions;
using Application.HealthChecks.Extensions;
using Persistence.Repository.Command.Extensions;

namespace Api.Extensions
{
    internal static class ProgramExtensions
    {
        /// <summary>
        /// Configures essential services and middleware for the application, including dependency injection for
        /// controllers, AutoMapper, MediatR, validation, authentication, localization, health checks, rate limiting,
        /// and caching.
        /// </summary>
        /// <remarks>This method sets up core infrastructure and application-specific services using
        /// convention-based registration. It enables health checks, JWT authentication, localization, and multiple rate
        /// limiting policies. Application services are registered based on naming conventions, and exception handling
        /// middleware is added to support robust error management.</remarks>
        /// <param name="builder">The WebApplicationBuilder instance used to register services and configure the application's dependency
        /// injection container.</param>
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            var applicationAssembly = typeof(Application.ProjectLinkReference).Assembly;

            // Auto mapper config for all assemblies
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Add controllers
            builder.Services.AddControllers();

            // Endpoints explorer (Swagger generation kept commented as original)
            builder.Services.AddEndpointsApiExplorer();
            // Prefer not to use swagger gen unless it's strictly necessary
            // builder.Services.AddSwaggerGen();

            // MediatR + validation pipeline
            builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(applicationAssembly));
            builder.Services.AddValidatorsFromAssembly(applicationAssembly, ServiceLifetime.Singleton);
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

            // Repositories
            builder.Services.AddRepositories();

            // Command Runners
            builder.Services.AddCommandRunners();

            // Task runner
            builder.Services.AddTaskRunner();

            // Add application services (convention-based registration)
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

            // Register exception handler service
            builder.Services.AddTransient<ExceptionHandlingMiddleware>();

            // Configure health checks
            builder.Services.ConfigureAppHealthChecks();

            // Add JWT authorization flow
            builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);

            // Add localization services
            builder.Services.AddLocalizationServices();

            // Add rate limiting policies
            builder.Services.AddFixedWindowRateLimit(builder.Configuration);
            builder.Services.AddSlidingWindowRateLimit(builder.Configuration);
            builder.Services.AddTokenBucketRateLimit(builder.Configuration);
            builder.Services.AddConcurrencyRateLimit(builder.Configuration);

            // Add cache
            builder.Services.AddResponseCaching();
        }

        /// <summary>
        /// Configures the HTTP request pipeline for the web application, enabling middleware for rate limiting,
        /// localization, health checks, response caching, exception handling, API key validation, and authorization.
        /// </summary>
        /// <remarks>This method sets up essential middleware components to enhance security, performance,
        /// and localization. It ensures health checks are available, applies response caching, and enforces API key
        /// validation and authorization. Middleware is conditionally configured based on the hosting environment to
        /// support development or production scenarios.</remarks>
        /// <param name="app">The web application instance whose request pipeline is being configured.</param>
        /// <param name="configuration">The configuration settings used to apply rate limiting and other middleware options to the application's
        /// endpoints.</param>
        /// <param name="env">The hosting environment that determines conditional middleware configuration, such as development-specific
        /// features.</param>
        public static void ConfigurePipeline(this WebApplication app, IConfiguration configuration, IWebHostEnvironment env)
        {
            // Add rate limiting
            app.UseRateLimiter();

            // Configure localization
            app.UseRequestLocalization(options =>
            {
                string[] supportedCultures = new[] { Language.English, Language.Spanish };

                options.AddSupportedCultures(supportedCultures);
                options.AddSupportedUICultures(supportedCultures);
                options.SetDefaultCulture(Language.English);
            });

            app.MapHealthChecks(@"/health/checks");

            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                // Prefer not to use swagger gen unless it's strictly necessary
                // app.UseSwagger();
                // app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Adds caching headers to responses and enables caching of responses based on the configured cache profiles
            app.UseResponseCaching();

            // Adds cache control headers to responses based on the configured cache settings
            app.UseMiddleware<CacheMiddleware>();

            // Check for exceptions
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Check for api keys presence
            app.UseMiddleware<CheckApiKeysMiddleware>();

            // Add authentication and authorization
            app.UseAuthorization();

            // Add rate limiting to all endpoints based on configuration
            app.MapControllers().RequireRateLimiting(configuration);
        }

        /// <summary>
        /// Logs key application configuration details to the application's logger for diagnostic and monitoring
        /// purposes.
        /// </summary>
        /// <remarks>This method is useful for capturing the application's operational context at startup,
        /// aiding in diagnostics and troubleshooting by making configuration values visible in the logs.</remarks>
        /// <param name="app">The web application instance whose logger is used to record configuration information.</param>
        /// <param name="configuration">The configuration source from which application-specific settings are retrieved.</param>
        /// <param name="environment">The hosting environment that provides information about the application's current environment, such as
        /// Development or Production.</param>
        public static void LogApplicationConfiguration(this WebApplication app, IConfiguration configuration, IWebHostEnvironment environment)
        {
            app.Logger.LogInformation(@"Environment: {Environment}.", environment.EnvironmentName);

            if (environment.IsDevelopment())
                app.Logger.LogInformation(@"Running on Development mode.");

            app.Logger.LogInformation(@"Language: {Language}.", Language.English);

            var jwt = configuration.GetSection("Jwt").Get<JwtOptions>()!;

            app.Logger.LogInformation(@"JWT Issuer: {JwtIssuer}.", jwt.Issuer);
            app.Logger.LogInformation(@"JWT Audience: {JwtAudience}.", jwt.Audience);
            app.Logger.LogInformation(@"JWT Minutes To Expire: {JwtMinutesToExpire}.", jwt.MinutesToExpire);
            app.Logger.LogInformation(@"JWT Clock Skew Minutes: {JwtClockSkewMinutes}.", jwt.ClockSkewMinutes);

            RateLimiting? rateLimiting = configuration.GetValue<RateLimiting?>(ApiConfiguration.RateLimitingType);
            app.Logger.LogInformation(@"Rate Limiting: {RateLimiting}.", rateLimiting);

            DatabaseEngine? databaseEngine = configuration.GetValue<DatabaseEngine?>(ApiConfiguration.DatabaseEngine);
            app.Logger.LogInformation(@"Database Engine: {DatabaseEngine}.", databaseEngine);

            string? healthCheckInternetUrl = configuration.GetValue<string?>(ApiConfiguration.HealthCheckUrl);
            app.Logger.LogInformation(@"Health Check URL: {HealthCheckUrl}.", healthCheckInternetUrl);

            int? cacheMaxAgeSeconds = configuration.GetValue<int?>(ApiConfiguration.CacheSettingsMaxAgeSeconds);
            app.Logger.LogInformation(@"Cache Max Age (seconds): {CacheMaxAgeSeconds}.", cacheMaxAgeSeconds);
        }
    }
}