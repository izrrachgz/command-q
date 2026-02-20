using Serilog;

namespace Api.Extensions
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Configures structured logging for the application using Serilog, reading settings from the provided
        /// configuration.
        /// </summary>
        /// <remarks>This method sets Serilog as the application's logging provider, enabling enriched log
        /// context and flexible configuration through app settings. Use this method during application startup to
        /// ensure logging is properly initialized.</remarks>
        /// <param name="builder">The WebApplicationBuilder instance used to configure logging for the application. Cannot be null.</param>
        public static void ConfigureLogging(this WebApplicationBuilder builder)
        {
            //Configure Serilog for logging, reading settings.
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            //Add Serilog to the application host
            builder.Host.UseSerilog();
        }
    }
}
