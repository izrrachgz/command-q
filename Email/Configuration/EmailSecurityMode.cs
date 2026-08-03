namespace Email.Configuration
{
    /// <summary>
    /// Controls how the SMTP connection negotiates transport security.
    /// </summary>
    public enum EmailSecurityMode
    {
        Auto = 0,
        None = 1,
        StartTlsWhenAvailable = 2,
        StartTls = 3,
        SslOnConnect = 4
    }
}
