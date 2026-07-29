using Api.Models;
using Api.Constants;
using Api.Middleware;
using Api.Enumerations;
using Common.Constants;
using Common.Enumerations;
using Application.Extensions;

namespace Api.Extensions
{
    internal static class ProgramExtensions
    {
        /// <summary>
        /// Configures essential services and middleware for the application, including dependency injection for
        /// controllers, rate limiting and caching.
        /// </summary>
        /// <remarks>This method sets up core infrastructure and application-specific services using
        /// convention-based registration. It enables health checks, JWT authentication, localization, and multiple rate
        /// limiting policies. Application services are registered based on naming conventions, and exception handling
        /// middleware is added to support robust error management.</remarks>
        /// <param name="builder">The ApplicationBuilder instance used to register services and configure the application's dependency
        /// injection container.</param>
        public static void ConfigureServices(this IHostApplicationBuilder builder)
        {
            // Add controllers
            builder.Services.AddControllers();

            // Endpoints explorer (Swagger generation kept commented as original)
            builder.Services.AddEndpointsApiExplorer();
            // Prefer not to use swagger gen unless it's strictly necessary
            // builder.Services.AddSwaggerGen();          

            // Register exception handler service
            builder.Services.AddTransient<ExceptionHandlingMiddleware>();

            // Add JWT authorization flow
            builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);

            // Add rate limiting policies
            builder.Services.AddFixedWindowRateLimit(builder.Configuration);
            builder.Services.AddSlidingWindowRateLimit(builder.Configuration);
            builder.Services.AddTokenBucketRateLimit(builder.Configuration);
            builder.Services.AddConcurrencyRateLimit(builder.Configuration);

            // Add cache
            builder.Services.AddResponseCaching();

            // Add application services
            builder.Services.ConfigureApplicationServices();

            var clientsSection = builder.Configuration.GetSection(ConfigKeys.Api.Clients);

            //Register api clients
            builder.Services.Configure<List<ApiClient>>(clientsSection);

            //Add Cors
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                    {
                        string clientsUris = clientsSection.Get<List<ApiClient>>()!
                            .Select(client => client.Uri.Trim().TrimEnd('/'))
                            .Aggregate((current, next) => $"{current},{next}");

                        policy.WithOrigins(clientsUris.Split(','))
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    });
                });
            }
        }

        /// <summary>
        /// Configures the HTTP request pipeline for the web application, enabling middleware for rate limiting,
        /// localization, health checks, response caching, exception handling, API key validation, and authorization.
        /// </summary>
        /// <remarks>This method sets up essential middleware components to enhance security, performance,
        /// and localization. It ensures health checks are available, applies response caching, and enforces API key
        /// validation and authorization. Middleware is conditionally configured based on the hosting environment to
        /// support development or production scenarios.</remarks>
        /// <param name="app">The application instance whose request pipeline is being configured.</param>
        /// <param name="configuration">The configuration settings used to apply rate limiting and other middleware options to the application's
        /// endpoints.</param>
        /// <param name="env">The hosting environment that determines conditional middleware configuration, such as development-specific
        /// features.</param>
        public static void ConfigurePipeline(this WebApplication app, IConfiguration configuration, IHostEnvironment env)
        {
            // Add rate limiting
            app.UseRateLimiter();

            // Configure localization
            app.UseRequestLocalization(options =>
            {
                string[] supportedCultures = [Language.English, Language.Spanish];

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
                app.UseCors();
            }

            app.UseHttpsRedirection();

            // Adds caching headers to responses and enables caching of responses based on the configured cache profiles
            app.UseResponseCaching();

            // Adds cache control headers to responses based on the configured cache settings
            app.UseMiddleware<CacheMiddleware>();

            // Check for exceptions
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Check for api keys presence
            app.UseMiddleware<CheckApiClientMiddleware>();

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
        /// <param name="logger">The application instance logger used to record configuration information.</param>
        /// <param name="configuration">The configuration source from which application-specific settings are retrieved.</param>
        /// <param name="environment">The hosting environment that provides information about the application's current environment, such as
        /// Development or Production.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1873:Evitar el registro potencialmente costoso", Justification = "All logging in this method comes from configuration file and there is no heavy calculations.")]
        public static void LogAppConfig(this ILogger logger, IConfiguration configuration, IHostEnvironment environment)
        {
            logger.LogInformation(@"Environment: {Environment}.", environment.EnvironmentName);

            if (environment.IsDevelopment())
                logger.LogInformation(@"Running on Development mode.");

            logger.LogInformation(@"Language: {Language}.", Language.English);

            var jwt = configuration.GetSection(@"Jwt").Get<JwtOptions>()!;

            logger.LogInformation(@"JWT Issuer: {JwtIssuer}.", jwt.Issuer);
            logger.LogInformation(@"JWT Audience: {JwtAudience}.", jwt.Audience);
            logger.LogInformation(@"JWT Minutes To Expire: {JwtMinutesToExpire}.", jwt.MinutesToExpire);
            logger.LogInformation(@"JWT Clock Skew Minutes: {JwtClockSkewMinutes}.", jwt.ClockSkewMinutes);

            RateLimiting? rateLimiting = configuration.GetValue<RateLimiting?>(ConfigKeys.RateLimiting.Type);
            logger.LogInformation(@"Rate Limiting: {RateLimiting}.", rateLimiting);

            DatabaseEngine? databaseEngine = configuration.GetValue<DatabaseEngine?>(ConfigKeys.Database.Engine);
            logger.LogInformation(@"Database Engine: {DatabaseEngine}.", databaseEngine);

            string? healthCheckInternetUrl = configuration.GetValue<string?>(ConfigKeys.HealthCheck.Url);
            logger.LogInformation(@"Health Check URL: {HealthCheckUrl}.", healthCheckInternetUrl);

            int? cacheMaxAgeSeconds = configuration.GetValue<int?>(ConfigKeys.Cache.MaxAgeSeconds);
            logger.LogInformation(@"Cache Max Age (seconds): {CacheMaxAgeSeconds}.", cacheMaxAgeSeconds);
        }
    }
}
