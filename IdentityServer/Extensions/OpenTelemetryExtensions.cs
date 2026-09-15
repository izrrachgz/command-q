using IdentityServer.Constants;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace IdentityServer.Extensions
{
    /// <summary>
    /// Registers the IdentityServer host's OpenTelemetry SDK and exporter configuration.
    /// </summary>
    internal static class OpenTelemetryExtensions
    {
        private static readonly string[] RuntimeAndAspNetCoreMeters =
        [
            "System.Runtime",
            "Microsoft.AspNetCore.Hosting",
            "Microsoft.AspNetCore.Routing",
            "Microsoft.AspNetCore.RateLimiting",
            "Microsoft.AspNetCore.HeaderParsing",
            "Microsoft.AspNetCore.Diagnostics",
            "Microsoft.AspNetCore.Server.Kestrel",
            "Microsoft.AspNetCore.Authentication",
            "Microsoft.AspNetCore.Authorization",
            "Microsoft.AspNetCore.Identity"
        ];

        /// <summary>
        /// Registers IdentityServer tracing and metrics when OpenTelemetry is enabled in host configuration.
        /// </summary>
        /// <param name="builder">The host application builder used to register telemetry services.</param>
        public static void AddIdentityServerOpenTelemetry(this IHostApplicationBuilder builder)
        {
            IConfigurationSection section = builder.Configuration
                .GetRequiredSection(IdentityServerConfigKeys.Section)
                .GetRequiredSection(IdentityServerConfigKeys.OpenTelemetry.Section);

            if (!section.GetValue("Enabled", false))
            {
                return;
            }

            string serviceName = section.GetValue<string>("ServiceName") ?? "command-q-identity-server";
            Uri endpoint = section.GetValue<Uri>("Otlp:Endpoint") ??
                throw new InvalidOperationException(
                    $"{IdentityServerConfigKeys.Section}:{IdentityServerConfigKeys.OpenTelemetry.OtlpEndpoint} " +
                    "is required when OpenTelemetry is enabled.");

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter(options => options.Endpoint = endpoint))
                .WithMetrics(metrics => metrics
                    .AddMeter(RuntimeAndAspNetCoreMeters)
                    .AddOtlpExporter(options => options.Endpoint = endpoint));
        }
    }
}
