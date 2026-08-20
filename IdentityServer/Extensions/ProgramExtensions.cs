using System.Globalization;
using IdentityServer.Logging;
using IdentityServer.Modules;
using IdentityServer.Constants;
using IdentityServer.Resources;
using IdentityServer.Middleware;
using IdentityServer.Localization;
using IdentityServer.Configuration;
using IdentityServer.Serialization;
using IdentityServer.Modules.OpenIddict;
using IdentityServer.Modules.OpenIddict.Configuration;

namespace IdentityServer.Extensions
{
    internal static class ProgramExtensions
    {
        private const string CorsPolicy = "IdentityServerCors";

        public static void ConfigureServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton(TimeProvider.System);

            IConfigurationSection identityServerConfiguration = builder.Configuration
                .GetRequiredSection(IdentityServerConfigKeys.Section);

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
                    options.JsonSerializerOptions.Converters.Add(new UtcDateTimeOffsetJsonConverter());
                });
            builder.Services.AddConfiguredCors(identityServerConfiguration, builder.Environment);
            builder.Services.AddAuthorization();
            builder.Services.AddIdentityServerDataProtection(identityServerConfiguration, builder.Environment);
            builder.Services.AddIdentityServerModules(identityServerConfiguration, builder.Environment);
        }

        public static void ConfigurePipeline(this WebApplication app)
        {
            app.UseHttpsRedirection();
            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseCors(CorsPolicy);
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapIdentityServerModules();
        }

        public static void LogApplicationConfigurationConstants(this ILogger logger, IConfiguration configuration, IHostEnvironment environment)
        {
            var options = configuration
                .GetRequiredSection(IdentityServerConfigKeys.OpenIddict.Section)
                .Get<AuthorizationServerOptions>()!;

            logger.StartupSetting(nameof(environment.EnvironmentName), environment.EnvironmentName);
            logger.StartupSetting(
                "DatabaseEngine",
                configuration[IdentityServerConfigKeys.ConnectionStrings.Engine]);
            logger.StartupSetting(nameof(options.Issuer), options.Issuer);
            logger.StartupSetting(
                nameof(options.AccessTokenMinutes),
                options.AccessTokenMinutes.ToString(CultureInfo.InvariantCulture));
            logger.StartupSetting(
                nameof(options.RefreshTokenDays),
                options.RefreshTokenDays.ToString(CultureInfo.InvariantCulture));
            logger.StartupSetting(
                "MigrationsOnStartup",
                (environment.IsDevelopment() ||
                    configuration.GetValue(IdentityServerConfigKeys.Migrations.RunOnStartup, false)).ToString(CultureInfo.InvariantCulture));
            var corsOptions = configuration.GetSection(IdentityServerConfigKeys.Cors.Section).Get<CorsOptions>() ?? new CorsOptions();
            logger.StartupSetting("CorsEnabled", ShouldUseCors(corsOptions, environment).ToString(CultureInfo.InvariantCulture));
            logger.StartupSetting("CorsAllowedOrigins", GetAllowedCorsOrigins(corsOptions).Length.ToString(CultureInfo.InvariantCulture));

            if (environment.IsDevelopment())
            {
                logger.RunningInDevelopment();
            }
        }

        private static void AddConfiguredCors(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.AddOptions<CorsOptions>()
                .Bind(configuration.GetSection(IdentityServerConfigKeys.Cors.Section))
                .Validate(
                    options => IsValidCorsConfiguration(options, environment.IsDevelopment()),
                    IdentityServerText.Format(
                        IdentityServerMessages.Configuration_CorsAllowedOriginsInvalid,
                        IdentityServerConfigKeys.FullPath(IdentityServerConfigKeys.Cors.AllowedOrigins)))
                .ValidateOnStart();

            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicy, policy =>
                {
                    policy.SetIsOriginAllowed(origin => IsAllowedCorsOrigin(origin, configuration, environment))
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        internal static bool IsAllowedCorsOrigin(string origin, CorsOptions options, bool isDevelopment)
        {
            return ShouldUseCors(options, isDevelopment) &&
                GetAllowedCorsOrigins(options).Contains(origin.Trim().TrimEnd('/'), StringComparer.OrdinalIgnoreCase);
        }

        internal static bool IsValidCorsConfiguration(CorsOptions options, bool isDevelopment)
        {
            return !ShouldUseCors(options, isDevelopment) ||
                (options.AllowedOrigins.All(origin =>
                    !string.IsNullOrWhiteSpace(origin) &&
                    IsHttpOrigin(origin.Trim().TrimEnd('/'))) &&
                 GetAllowedCorsOrigins(options).Length > 0);
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

        private static bool IsAllowedCorsOrigin(string origin, IConfiguration configuration, IHostEnvironment environment)
        {
            var options = configuration.GetSection(IdentityServerConfigKeys.Cors.Section).Get<CorsOptions>() ?? new CorsOptions();

            return IsAllowedCorsOrigin(origin, options, environment.IsDevelopment());
        }

        private static bool IsHttpOrigin(string origin)
        {
            return Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                string.IsNullOrEmpty(uri.AbsolutePath.Trim('/')) &&
                string.IsNullOrEmpty(uri.Query) &&
                string.IsNullOrEmpty(uri.Fragment);
        }

        private static bool ShouldUseCors(CorsOptions options, IHostEnvironment environment)
        {
            return ShouldUseCors(options, environment.IsDevelopment());
        }

        private static bool ShouldUseCors(CorsOptions options, bool isDevelopment)
        {
            return isDevelopment || options.Enabled;
        }
    }
}
