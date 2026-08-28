using OpenTelemetry;
using Email.Telemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Email.Outbox.Telemetry;
using Email.Worker.Constants;
using Email.Worker.Telemetry;
using OpenTelemetry.Resources;

namespace Email.Worker.Extensions
{
    /// <summary>
    /// Registers the email worker host's OpenTelemetry SDK and exporter configuration.
    /// </summary>
    internal static class OpenTelemetryExtensions
    {
        /// <summary>
        /// Registers email worker tracing and metrics when OpenTelemetry is enabled in worker configuration.
        /// </summary>
        /// <param name="builder">The web application builder used to register telemetry services.</param>
        public static void AddEmailWorkerOpenTelemetry(this WebApplicationBuilder builder)
        {
            IConfigurationSection section = builder.Configuration
                .GetRequiredSection(EmailWorkerConstants.ConfigurationSection)
                .GetRequiredSection(EmailWorkerConstants.OpenTelemetrySection);

            if (!section.GetValue("Enabled", false))
            {
                return;
            }

            string serviceName = section.GetValue<string>("ServiceName")
                ?? "command-q-email-worker";
            string endpointKey = $"{EmailWorkerConstants.ConfigurationSection}:{EmailWorkerConstants.OpenTelemetrySection}:Otlp:Endpoint";
            Uri endpoint = section.GetValue<Uri>("Otlp:Endpoint")
                ?? throw new InvalidOperationException(
                    $"{endpointKey} is required when OpenTelemetry is enabled.");

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddSource(EmailTelemetry.ActivitySourceName)
                    .AddSource(EmailOutboxTelemetry.ActivitySourceName)
                    .AddSource(EmailWorkerTelemetry.ActivitySourceName)
                    .AddOtlpExporter(options => options.Endpoint = endpoint))
                .WithMetrics(metrics => metrics
                    .AddMeter(EmailTelemetry.MeterName)
                    .AddMeter(EmailOutboxTelemetry.MeterName)
                    .AddMeter(EmailWorkerTelemetry.MeterName)
                    .AddOtlpExporter(options => options.Endpoint = endpoint));
        }
    }
}
