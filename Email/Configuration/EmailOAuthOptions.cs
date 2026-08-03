namespace Email.Configuration
{
    /// <summary>
    /// Contains an OAuth 2.0 bearer token that is already ready for SMTP authentication.
    /// </summary>
    public sealed class EmailOAuthOptions
    {
        public string Username { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }
}
