namespace Email.Exceptions
{
    /// <summary>
    /// Indicates that a requested logical email profile is not configured.
    /// </summary>
    public sealed class EmailProfileNotFoundException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance for a missing profile name.
        /// </summary>
        /// <param name="profileName">The requested logical profile name.</param>
        public EmailProfileNotFoundException(string profileName)
            : base($"The email profile '{profileName}' is not configured.")
        {
            ProfileName = profileName;
        }

        /// <summary>
        /// Gets the missing logical profile name.
        /// </summary>
        public string ProfileName { get; }
    }
}
