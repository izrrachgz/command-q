using Email.Constants;

namespace Email.Configuration
{
    /// <summary>
    /// Configures a single SMTP endpoint.
    /// </summary>
    public sealed class EmailSmtpOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = EmailConstants.DefaultSmtpPort;
        public EmailSecurityMode Security { get; set; } = EmailSecurityMode.StartTls;
        public int TimeoutMilliseconds { get; set; } = EmailConstants.DefaultTimeoutMilliseconds;
        public bool CheckCertificateRevocation { get; set; } = true;
    }
}
