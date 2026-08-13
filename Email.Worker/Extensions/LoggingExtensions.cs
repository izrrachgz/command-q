using Serilog;

namespace Email.Worker.Extensions
{
    internal static class LoggingExtensions
    {
        /// <summary>
        /// Configures structured logging for the email worker using Serilog settings from configuration.
        /// </summary>
        /// <param name="builder">The web application builder used to configure host logging.</param>
        public static void ConfigureLogging(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}
