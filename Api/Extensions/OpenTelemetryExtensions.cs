using OpenTelemetry;
using Api.Constants;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Api.Extensions
{
    /// <summary>
    /// Registers the API host's OpenTelemetry SDK and exporter configuration.
    /// </summary>
    internal static class OpenTelemetryExtensions
    {
        /// <summary>
        /// Registers API tracing and metrics when OpenTelemetry is enabled in the API configuration.
        /// </summary>
        /// <param name="builder">The host application builder used to register telemetry services.</param>
        public static void AddApiOpenTelemetry(this IHostApplicationBuilder builder)
        {
            IConfigurationSection section = builder.Configuration
                .GetRequiredSection(ApiConfigKeys.Section)
                .GetRequiredSection(ApiConfigKeys.OpenTelemetry.Section);

            if (!section.GetValue("Enabled", false))
            {
                return;
            }

            string serviceName = section.GetValue<string>("ServiceName") ?? "command-q-api";

            Uri endpoint = section.GetValue<Uri>("Otlp:Endpoint") ??
                throw new InvalidOperationException($"{ApiConfigKeys.Section}:{ApiConfigKeys.OpenTelemetry.OtlpEndpoint} is required when OpenTelemetry is enabled.");

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter(options => options.Endpoint = endpoint))
                .WithMetrics(metrics => metrics.AddOtlpExporter(options => options.Endpoint = endpoint));
        }
    }
}
