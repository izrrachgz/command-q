namespace IdentityServer.Constants
{
    internal static partial class IdentityServerConfigKeys
    {
        public static class OpenTelemetry
        {
            public const string Section = "OpenTelemetry";
            public const string Enabled = "OpenTelemetry:Enabled";
            public const string ServiceName = "OpenTelemetry:ServiceName";
            public const string OtlpEndpoint = "OpenTelemetry:Otlp:Endpoint";
        }
    }
}
