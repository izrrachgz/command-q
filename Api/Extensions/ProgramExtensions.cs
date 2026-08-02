using Api.Models;
using Api.Logging;
using Api.Modules;
using Api.Constants;
using Api.Middleware;
using Api.Enumerations;
using Common.Constants;
using Common.Enumerations;
using Application.Extensions;
using Application.Modules.Health;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Api.Extensions
{
    internal static class ProgramExtensions
    {
        /// <summary>
        /// Configures essential services and middleware for the application, including dependency injection for
        /// controllers, rate limiting and caching.
        /// </summary>
        /// <remarks>This method sets up core infrastructure and application-specific services using
        /// convention-based registration. It enables health checks, bearer authentication, localization, and multiple rate
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

            builder.Services.ValidateAppConfiguration(builder.Configuration);

            // Add resource-server authentication backed by the standalone identity server.
            builder.Services.AddResourceServerAuthentication(builder.Configuration);

            // Add rate limiting policies
            builder.Services.AddRateLimitingPolicies(builder.Configuration);

            // Add cache
            builder.Services.AddResponseCaching();

            if (builder.Environment.IsDevelopment())
            {
                string dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");

                builder.Services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));
            }

            // Add application services
            builder.Services.ConfigureApplicationServices(builder.Configuration);

            var clientsSection = builder.Configuration.GetSection(ConfigKeys.Api.Clients);

            //Register api clients
            builder.Services.AddOptions<List<ApiClient>>()
                .Bind(clientsSection)
                .Validate(clients => clients.Count > 0, "At least one API client must be configured.")
                .Validate(clients => clients.All(client =>
                    !string.IsNullOrWhiteSpace(client.Name) &&
                    !string.IsNullOrWhiteSpace(client.Id) &&
                    Uri.TryCreate(client.Uri, UriKind.Absolute, out _)),
                    "Every API client requires a name, id and absolute URI.")
                .ValidateOnStart();

            //Add Cors
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                    {
                        var allowedOrigins = (clientsSection.Get<List<ApiClient>>() ?? [])
                            .Select(client => client.Uri.Trim().TrimEnd('/'))
                            .Where(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToArray();

                        if (allowedOrigins.Length > 0)
                        {
                            policy.WithOrigins(allowedOrigins);
                        }
                        else
                        {
                            policy.SetIsOriginAllowed(_ => false);
                        }

                        policy.AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
                });
            }
        }

        public static void ValidateStartupConfiguration(this WebApplication app)
        {
            var services = app.Services;

            _ = services.GetRequiredService<IOptions<FixedRateLimitingOptions>>().Value;
            _ = services.GetRequiredService<IOptions<SlidingRateLimitingOptions>>().Value;
            _ = services.GetRequiredService<IOptions<TokenBucketRateLimitingOptions>>().Value;
            _ = services.GetRequiredService<IOptions<ConcurrencyRateLimitingOptions>>().Value;
            _ = services.GetRequiredService<IOptions<CacheOptions>>().Value;
            _ = services.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            _ = services.GetRequiredService<IOptions<BearerAuthenticationOptions>>().Value;
            _ = services.GetRequiredService<IOptions<List<ApiClient>>>().Value;
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

            app.MapHealthChecks(RouteConstants.HealthLive, new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains(HealthCheckConstants.LiveTag)
            });

            app.MapHealthChecks(RouteConstants.HealthReady, new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains(HealthCheckConstants.ReadyTag)
            });

            app.MapHealthChecks(RouteConstants.HealthChecks);

            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                // Prefer not to use swagger gen unless it's strictly necessary
                // app.UseSwagger();
                // app.UseSwaggerUI();
                app.UseCors();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<SecurityHeadersMiddleware>();

            // Adds caching headers to responses and enables caching of responses based on the configured cache profiles
            app.UseResponseCaching();

            // Adds cache control headers to responses based on the configured cache settings
            app.UseMiddleware<CacheMiddleware>();

            // Check for exceptions
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Check for api keys presence
            app.UseMiddleware<CheckApiClientMiddleware>();

            // Add authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Add rate limiting to all endpoints based on configuration
            app.MapApiModules(configuration);
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
        public static void LogAppConfig(this ILogger logger, IConfiguration configuration, IHostEnvironment environment)
        {
            logger.StartupSetting("Environment", environment.EnvironmentName);

            if (environment.IsDevelopment())
                logger.RunningInDevelopment();

            logger.StartupSetting("Language", Language.English);

            var authentication = configuration
                .GetSection(ConfigKeys.Authentication.Section)
                .Get<BearerAuthenticationOptions>()!;

            logger.StartupSetting("Authentication Authority", authentication.Authority);
            logger.StartupSetting("Authentication Audience", authentication.Audience);
            logger.StartupSetting("Authentication API Scope", authentication.ApiScope);

            RateLimiting? rateLimiting = configuration.GetValue<RateLimiting?>(ConfigKeys.RateLimiting.Type);
            logger.StartupSetting("Rate Limiting", rateLimiting);

            DatabaseEngine? databaseEngine = configuration.GetValue<DatabaseEngine?>(ConfigKeys.Database.Engine);
            logger.StartupSetting("Database Engine", databaseEngine);

            string? healthCheckInternetUrl = configuration.GetValue<string?>(ConfigKeys.HealthCheck.Url);
            logger.StartupSetting("Health Check URL", healthCheckInternetUrl);

            int? cacheMaxAgeSeconds = configuration.GetValue<int?>(ConfigKeys.Cache.MaxAgeSeconds);
            logger.StartupSetting("Cache Max Age (seconds)", cacheMaxAgeSeconds);
        }

        private static void ValidateAppConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<FixedRateLimitingOptions>()
                .Bind(configuration.GetSection(ConfigKeys.RateLimiting.FixedOptions))
                .Validate(IsValidRateLimit, "FixedWindow rate limiting options must be greater than zero, except QueueLimit which cannot be negative.")
                .ValidateOnStart();

            services.AddOptions<SlidingRateLimitingOptions>()
                .Bind(configuration.GetSection(ConfigKeys.RateLimiting.SlidingOptions))
                .Validate(options => IsValidRateLimit(options) && options.SegmentsPerWindow > 0, "Sliding rate limiting options must be greater than zero, except QueueLimit which cannot be negative.")
                .ValidateOnStart();

            services.AddOptions<TokenBucketRateLimitingOptions>()
                .Bind(configuration.GetSection(ConfigKeys.RateLimiting.TokenBucketOptions))
                .Validate(options => IsValidRateLimit(options) && options.TokenLimit > 0 && options.TokensPerPeriod > 0, "TokenBucket rate limiting options must be greater than zero, except QueueLimit which cannot be negative.")
                .ValidateOnStart();

            services.AddOptions<ConcurrencyRateLimitingOptions>()
                .Bind(configuration.GetSection(ConfigKeys.RateLimiting.ConcurrencyOptions))
                .Validate(options => options.PermitLimit > 0 && options.QueueLimit >= 0, "Concurrency rate limiting options must be greater than zero, except QueueLimit which cannot be negative.")
                .ValidateOnStart();

            services.AddOptions<CacheOptions>()
                .Bind(configuration.GetSection(ConfigKeys.Cache.Section))
                .Validate(options => options.MaxAgeSeconds >= 0, "CacheSettings:MaxAgeSeconds cannot be negative.")
                .ValidateOnStart();

            services.AddOptions<DatabaseOptions>()
                .Bind(configuration.GetSection(ConfigKeys.Database.Section))
                .Validate(options => Enum.IsDefined(options.Engine), "ConnectionStrings:Engine must be a supported database engine.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Db), "ConnectionStrings:Db is required.")
                .ValidateOnStart();
        }

        private static bool IsValidRateLimit(RateLimitingOptions options)
        {
            return options.PermitLimit > 0 &&
                options.WindowSeconds > 0 &&
                options.QueueLimit >= 0;
        }

        private sealed class CacheOptions
        {
            public int MaxAgeSeconds { get; init; }
        }

        private sealed class DatabaseOptions
        {
            public string Db { get; init; } = string.Empty;
            public DatabaseEngine Engine { get; init; }
        }
    }
}
