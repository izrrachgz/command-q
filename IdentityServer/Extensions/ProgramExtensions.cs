using System.Globalization;
using IdentityServer.Logging;
using IdentityServer.Modules;
using IdentityServer.Constants;
using IdentityServer.Middleware;
using IdentityServer.Modules.OpenIddict;

namespace IdentityServer.Extensions
{
    internal static class ProgramExtensions
    {
        public static void ConfigureServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            builder.Services.AddAuthorization();
            builder.Services.AddIdentityServerDataProtection(builder.Configuration, builder.Environment);
            builder.Services.AddIdentityServerModules(builder.Configuration, builder.Environment);
        }

        public static void ConfigurePipeline(this WebApplication app)
        {
            app.UseHttpsRedirection();
            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapIdentityServerModules();
        }

        public static void LogAppConfig(
            this ILogger logger,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            var options = configuration
                .GetRequiredSection(ConfigKeys.OpenIddict.Section)
                .Get<AuthorizationServerOptions>()!;

            logger.StartupSetting(nameof(environment.EnvironmentName), environment.EnvironmentName);
            logger.StartupSetting(nameof(options.Issuer), options.Issuer);
            logger.StartupSetting(
                nameof(options.AccessTokenMinutes),
                options.AccessTokenMinutes.ToString(CultureInfo.InvariantCulture));
            logger.StartupSetting(
                nameof(options.RefreshTokenDays),
                options.RefreshTokenDays.ToString(CultureInfo.InvariantCulture));

            if (environment.IsDevelopment())
            {
                logger.RunningInDevelopment();
            }
        }
    }
}
