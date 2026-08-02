using Serilog;

namespace IdentityServer.Extensions
{
    internal static class LoggingExtensions
    {
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
