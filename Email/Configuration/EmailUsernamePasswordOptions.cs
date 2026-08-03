namespace Email.Configuration
{
    /// <summary>
    /// Contains credentials supplied directly to LOGIN or PLAIN authentication.
    /// </summary>
    public sealed class EmailUsernamePasswordOptions
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
