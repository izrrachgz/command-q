using MediatR;
using Api.Logging;
using Api.Modules;
using Api.Services;
using Api.Constants;
using Api.Middleware;
using Api.Abstractions;
using Api.Enumerations;
using Common.Constants;
using Api.Configuration;
using Api.Serialization;
using Common.Extensions;
using System.Net.Mime;
using System.Reflection;
using Api.Modules.Health;
using Common.Enumerations;
using Application.Extensions;
using Email.Outbox.Extensions;
using Application.Configuration;
using Localization.Configuration;
using Localization.Abstractions;
using Microsoft.Extensions.Options;
using Application.Migrations.Extensions;
using Email.Outbox.Persistence.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Application.Modules.Health.Configuration;
using Application.Modules.Configuration.Commands;
using Application.Modules.Identity.Roles.Services;
using Public;

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
            IConfigurationSection apiConfiguration = builder.Configuration.GetRequiredSection(ApiConfigKeys.Section);

            // Add controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
                    options.JsonSerializerOptions.Converters.Add(new UtcDateTimeOffsetJsonConverter());
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = actionContext =>
                    {
                        var messages = actionContext.HttpContext.RequestServices.GetRequiredService<ITextProvider>();
                        var problem = new Microsoft.AspNetCore.Mvc.ValidationProblemDetails(actionContext.ModelState)
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Title = messages.Get(nameof(Messages.Invalid_Model)),
                            Instance = actionContext.HttpContext.Request.Path
                        };

                        ApiProblemDetailsExtensions.Enrich(
                            problem,
                            actionContext.HttpContext,
                            ApiProblemCodes.ValidationFailed);

                        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(problem)
                        {
                            ContentTypes = { MediaTypeNames.Application.ProblemJson, MediaTypeNames.Application.Json }
                        };
                    };
                });

            builder.Services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                    ApiProblemDetailsExtensions.Enrich(context.ProblemDetails, context.HttpContext);
            });
            builder.Services.AddExceptionHandler<ApiExceptionHandler>();
            builder.AddApiOpenTelemetry();

            // Endpoints explorer (Swagger generation kept commented as original)
            builder.Services.AddEndpointsApiExplorer();
            // Prefer not to use swagger gen unless it's strictly necessary
            // builder.Services.AddSwaggerGen();

            builder.Services.ValidateApplicationConfiguration(apiConfiguration);
            builder.Services.AddSingleton<IFeatureFlags, FeatureFlags>();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IExternalApiClientFactory, ExternalApiClientFactory>();
            builder.Services.AddScoped<IIdentityRoleAdministrationClient, IdentityServerAdminClient>();
            builder.Services.AddHostedService<RuntimeConfigurationChangeMonitor>();

            // Add resource-server authentication backed by the standalone identity server.
            builder.Services.AddResourceServerAuthentication(apiConfiguration);

            // Add rate limiting policies
            builder.Services.AddRateLimitingPolicies(apiConfiguration);

            builder.Services.AddResponseCaching();
            builder.Services.AddConfiguredCors(apiConfiguration, builder.Environment);

            if (builder.Environment.IsDevelopment())
            {
                string dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");

                builder.Services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));
            }

            // Add application services
            builder.Services.ConfigureApplicationServices(apiConfiguration);

            // Add durable notification queue services. The worker owns delivery and schema migration.
            IConfigurationSection outboxConfiguration = apiConfiguration
                .GetRequiredSection(ApiConfigKeys.EmailOutbox.Section);
            builder.Services.AddEmailOutboxProducer(outboxConfiguration);
            builder.Services.AddEmailOutboxPersistence(
                outboxConfiguration.GetRequiredSection(ApiConfigKeys.EmailOutbox.PersistenceSection));

            // Add application-owned database migrations
            builder.Services.AddApplicationMigrations();

            var clientsSection = apiConfiguration.GetSection(ApiConfigKeys.Api.Clients);

            //Register api clients
            builder.Services.AddOptions<List<ApiClientOptions>>()
                .Bind(clientsSection)
                .Validate(clients => clients.Count > 0, "At least one API client must be configured.")
                .Validate(clients => clients.All(client =>
                    !string.IsNullOrWhiteSpace(client.Name) &&
                    !string.IsNullOrWhiteSpace(client.Id) &&
                    client.Uri.IsAbsoluteUri()),
                    "Every API client requires a name, id and absolute URI.")
                .ValidateOnStart();

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
            _ = services.GetRequiredService<IOptions<CorsOptions>>().Value;
            _ = services.GetRequiredService<IOptions<IdentityServerAdminOptions>>().Value;
            _ = services.GetRequiredService<IOptions<List<ApiClientOptions>>>().Value;
            _ = services.GetRequiredService<IOptions<ApplicationOptions>>().Value;
        }

        /// <summary>Records the effective startup configuration after the application schema is ready.</summary>
        public static async Task RecordApplicationConfiguration(this WebApplication app, CancellationToken cancellationToken = default)
        {
            await using var scope = app.Services.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<ApplicationOptions>>().Value;
            IConfigurationSection apiConfiguration = app.Configuration.GetRequiredSection(ApiConfigKeys.Section);

            var response = await sender.Send(new RecordApplicationConfigurationCommand
            {
                ServerTimeZone = options.TimeZone,
                ApplicationVersion = ResolveApplicationVersion(),
                Language = apiConfiguration.GetValue<string>(ApiConfigKeys.Localization.DefaultCulture) ?? "en-US",
                Environment = app.Environment.EnvironmentName,
                InstanceName = Environment.MachineName
            }, cancellationToken);

            if (!response.Success)
                throw new InvalidOperationException(response.Message);
        }

        private static string ResolveApplicationVersion()
        {
            var assembly = Assembly.GetEntryAssembly();
            return assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? assembly?.GetName().Version?.ToString()
                ?? "unknown";
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
            app.UseRouting();

            app.UseCors();

            // Add rate limiting
            app.UseRateLimiter();

            // Configure localization
            app.UseRequestLocalization(options =>
            {
                var localization = configuration.GetSection(ApiConfigKeys.Localization.Section)
                    .Get<LocalizationOptions>() ?? new LocalizationOptions();
                string[] supportedCultures = localization.SupportedCultures;
                string defaultCulture = localization.DefaultCulture;

                options.AddSupportedCultures(supportedCultures);
                options.AddSupportedUICultures(supportedCultures);
                options.SetDefaultCulture(defaultCulture);
            });

            // Convert downstream exceptions and bodyless error status codes to the shared localized problem-details contract.
            app.UseExceptionHandler();
            app.UseStatusCodePages();

            app.MapHealthEndpoints();

            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                // Prefer not to use swagger gen unless it's strictly necessary
                // app.UseSwagger();
                // app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<SecurityHeadersMiddleware>();

            // Adds caching headers to responses and enables caching of responses based on the configured cache profiles
            app.UseResponseCaching();

            // Adds cache control headers to responses based on the configured cache settings
            app.UseMiddleware<CacheMiddleware>();

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
        public static void LogApplicationConfigurationConstants(this ILogger logger, IConfiguration configuration, IHostEnvironment environment)
        {
            logger.StartupSetting("Environment", environment.EnvironmentName);

            if (environment.IsDevelopment())
                logger.RunningInDevelopment();

            logger.StartupSetting(
                "Localization Default Culture",
                configuration.GetSection(ApiConfigKeys.Localization.Section)
                    .GetValue<string>(nameof(LocalizationOptions.DefaultCulture)) ?? "en-US");

            var authentication = configuration
                .GetSection(ApiConfigKeys.Authentication.Section)
                .Get<BearerAuthenticationOptions>()!;

            logger.StartupSetting("Authentication Authority", authentication.Authority);
            logger.StartupSetting("Authentication Audience", authentication.Audience);
            logger.StartupSetting("Authentication API Scope", authentication.ApiScope);

            var identityServerAdmin = configuration
                .GetSection(ApiConfigKeys.IdentityServerAdmin.Section)
                .Get<IdentityServerAdminOptions>() ?? new IdentityServerAdminOptions();
            logger.StartupSetting("IdentityServer Admin Client Enabled", identityServerAdmin.Enabled);
            logger.StartupSetting("IdentityServer Admin Client Base URL", identityServerAdmin.BaseUrl);

            RateLimiting? rateLimiting = configuration.GetValue<RateLimiting?>(ApiConfigKeys.RateLimiting.Type);
            logger.StartupSetting("Rate Limiting", rateLimiting);

            DatabaseEngine? databaseEngine = configuration.GetValue<DatabaseEngine?>(ApiConfigKeys.Database.Engine);
            logger.StartupSetting("Database Engine", databaseEngine);

            logger.StartupSetting(
                "Application Migrations On Startup",
                environment.IsDevelopment() || configuration.GetValue(ApiConfigKeys.Migrations.RunOnStartup, false));

            DatabaseEngine? outboxEngine = configuration.GetValue<DatabaseEngine?>(ApiConfigKeys.EmailOutbox.Engine);
            logger.StartupSetting("Email Outbox Engine", outboxEngine);
            logger.StartupSetting("Email Outbox Migrator", "EmailWorker");

            var healthCheck = configuration
                .GetSection(ApiConfigKeys.HealthCheck.Section)
                .Get<HealthCheckConfigurationOptions>() ?? new HealthCheckConfigurationOptions();
            logger.StartupSetting("Health Check URL", healthCheck.UrlCheckInternetAccess);
            logger.StartupSetting("Health Check Run On Startup", healthCheck.RunOnStartup);
            logger.StartupSetting("Health Check Fail Startup On Unhealthy", healthCheck.FailStartupOnUnhealthy);
            logger.StartupSetting("Health Check Publish Reports", healthCheck.ShouldPublishReports);

            int? cacheMaxAgeSeconds = configuration.GetValue<int?>(ApiConfigKeys.Cache.MaxAgeSeconds);
            logger.StartupSetting("Cache Max Age (seconds)", cacheMaxAgeSeconds);
        }

        private static void ValidateApplicationConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            RateLimiting rateLimiting = configuration.GetValue(
                ApiConfigKeys.RateLimiting.Type,
                RateLimiting.None);

            if (!Enum.IsDefined(rateLimiting))
            {
                throw new InvalidOperationException(
                    "Api:RateLimiting:Type must be a supported rate limiting strategy.");
            }

            services.AddOptions<FixedRateLimitingOptions>()
                .Bind(configuration.GetSection(ApiConfigKeys.RateLimiting.FixedOptions))
                .Validate(IsValidRateLimit, "FixedWindow rate limiting options must be greater than zero, except QueueLimit which cannot be negative.")
                .ValidateOnStart();

            services.AddOptions<SlidingRateLimitingOptions>()
                .Bind(configuration.GetSection(ApiConfigKeys.RateLimiting.SlidingOptions))
                .Validate(options => IsValidRateLimit(options) && options.SegmentsPerWindow > 0, "Sliding rate limiting options must be greater than zero, except QueueLimit which cannot be negative.")
                .ValidateOnStart();

            services.AddOptions<TokenBucketRateLimitingOptions>()
                .Bind(configuration.GetSection(ApiConfigKeys.RateLimiting.TokenBucketOptions))
                .Validate(options => IsValidRateLimit(options) && options.TokenLimit > 0 && options.TokensPerPeriod > 0, "TokenBucket rate limiting options must be greater than zero, except QueueLimit which cannot be negative.")
                .ValidateOnStart();

            services.AddOptions<ConcurrencyRateLimitingOptions>()
                .Bind(configuration.GetSection(ApiConfigKeys.RateLimiting.ConcurrencyOptions))
                .Validate(options => options.PermitLimit > 0 && options.QueueLimit >= 0, "Concurrency rate limiting options must be greater than zero, except QueueLimit which cannot be negative.")
                .ValidateOnStart();

            services.AddOptions<CacheOptions>()
                .Bind(configuration.GetSection(ApiConfigKeys.Cache.Section))
                .Validate(options => options.MaxAgeSeconds >= 0, "Api:CacheSettings:MaxAgeSeconds cannot be negative.")
                .ValidateOnStart();

            services.AddOptions<FeatureFlagOptions>()
                .Bind(configuration.GetSection(ApiConfigKeys.Features.Section));

            services.AddOptions<ExternalApiClientsOptions>()
                .Bind(configuration.GetSection(ApiConfigKeys.Api.ExternalClients))
                .Validate(options => options.Clients.Values.All(client =>
                    client.BaseUrl.IsAbsoluteUri() && client.TimeoutSeconds > 0),
                    "Every external API client requires an absolute BaseUrl and a positive TimeoutSeconds.")
                .ValidateOnStart();

            services.AddOptions<DatabaseOptions>()
                .Bind(configuration.GetSection(ApiConfigKeys.Database.Section))
                .Validate(options => Enum.IsDefined(options.Engine), "Api:ConnectionStrings:Engine must be a supported database engine.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Db), "Api:ConnectionStrings:Db is required.")
                .ValidateOnStart();

            services.AddOptions<IdentityServerAdminOptions>()
                .Bind(configuration.GetSection(ApiConfigKeys.IdentityServerAdmin.Section))
                .Validate(
                    options => !options.Enabled || options.BaseUrl.IsAbsoluteUri,
                    "Api:IdentityServerAdmin:BaseUrl must be an absolute URI when IdentityServer admin integration is enabled.")
                .Validate(
                    options => !options.Enabled || !string.IsNullOrWhiteSpace(options.ApiKeyHeaderName),
                    "Api:IdentityServerAdmin:ApiKeyHeaderName is required when IdentityServer admin integration is enabled.")
                .Validate(
                    options => !options.Enabled || !string.IsNullOrWhiteSpace(options.ApiKey),
                    "Api:IdentityServerAdmin:ApiKey is required when IdentityServer admin integration is enabled.")
                .Validate(
                    options => !options.Enabled || options.TimeoutSeconds > 0,
                    "Api:IdentityServerAdmin:TimeoutSeconds must be greater than zero when IdentityServer admin integration is enabled.")
                .ValidateOnStart();

        }

        private static void AddConfiguredCors(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.AddOptions<CorsOptions>()
                .Bind(configuration.GetSection(ApiConfigKeys.Cors.Section))
                .Validate(options => IsValidCorsConfiguration(options, environment), "Api:Cors:AllowedOrigins must contain absolute HTTP or HTTPS origins when CORS is enabled.")
                .ValidateOnStart();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.SetIsOriginAllowed(origin => IsAllowedCorsOrigin(origin, configuration, environment))
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        private static string[] GetAllowedCorsOrigins(CorsOptions options)
        {
            return options.AllowedOrigins
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .Select(origin => origin.Trim().TrimEnd('/'))
                .Where(IsHttpOrigin)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static bool IsHttpOrigin(string origin)
        {
            return Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                string.IsNullOrEmpty(uri.AbsolutePath.Trim('/')) &&
                string.IsNullOrEmpty(uri.Query) &&
                string.IsNullOrEmpty(uri.Fragment);
        }

        private static bool IsValidCorsConfiguration(CorsOptions options, IHostEnvironment environment)
        {
            return !ShouldUseCors(options, environment) ||
                (options.AllowedOrigins.All(origin =>
                    !string.IsNullOrWhiteSpace(origin) &&
                    IsHttpOrigin(origin.Trim().TrimEnd('/'))) &&
                 GetAllowedCorsOrigins(options).Length > 0);
        }

        private static bool IsAllowedCorsOrigin(string origin, IConfiguration configuration, IHostEnvironment environment)
        {
            var options = configuration.GetSection(ApiConfigKeys.Cors.Section).Get<CorsOptions>() ?? new CorsOptions();

            return ShouldUseCors(options, environment) &&
                GetAllowedCorsOrigins(options).Contains(origin.Trim().TrimEnd('/'), StringComparer.OrdinalIgnoreCase);
        }

        private static bool ShouldUseCors(CorsOptions options, IHostEnvironment environment)
        {
            return environment.IsDevelopment() || options.Enabled;
        }

        private static bool IsValidRateLimit(RateLimitingOptions options)
        {
            return options.PermitLimit > 0 &&
                options.WindowSeconds > 0 &&
                options.QueueLimit >= 0;
        }

    }
}
