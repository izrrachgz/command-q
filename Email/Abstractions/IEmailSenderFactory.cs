namespace Email.Abstractions
{
    /// <summary>
    /// Creates email senders bound to named SMTP profiles.
    /// </summary>
    public interface IEmailSenderFactory
    {
        /// <summary>
        /// Creates or retrieves a sender bound to a logical profile.
        /// </summary>
        /// <param name="profileName">The profile name, or <see langword="null"/> for the configured default.</param>
        /// <returns>A sender that uses only the selected profile's settings.</returns>
        IEmailSender Create(string? profileName = null);
    }
}
