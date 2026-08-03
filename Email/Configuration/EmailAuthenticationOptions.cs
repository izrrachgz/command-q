namespace Email.Configuration
{
    /// <summary>
    /// Contains each supported SMTP authentication configuration.
    /// Only the section selected by <see cref="Type"/> is used.
    /// </summary>
    public sealed class EmailAuthenticationOptions
    {
        public EmailAuthenticationType Type { get; set; } = EmailAuthenticationType.Anonymous;
        public EmailUsernamePasswordOptions Login { get; set; } = new();
        public EmailUsernamePasswordOptions Plain { get; set; } = new();
        public EmailOAuthOptions OAuth { get; set; } = new();
    }
}
