namespace Email.Configuration
{
    /// <summary>
    /// Configures the sender used when a message does not override it.
    /// </summary>
    public sealed class EmailSenderOptions
    {
        public string Address { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
