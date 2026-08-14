using Email.Constants;
using Email.Exceptions;

namespace Email.Configuration
{
    /// <summary>
    /// Configures global email delivery and the named SMTP profiles available to a host.
    /// </summary>
    public sealed class EmailOptions : EmailProfileOptions
    {
        /// <summary>
        /// Initializes a disabled email configuration for safe defaults.
        /// </summary>
        public EmailOptions()
        {
            Enabled = false;
        }

        /// <summary>
        /// Gets or sets the profile used when a sender does not specify a profile.
        /// </summary>
        public string DefaultProfile { get; set; } = EmailConstants.DefaultProfileName;

        /// <summary>
        /// Gets or sets the named SMTP profiles.
        /// </summary>
        public Dictionary<string, EmailProfileOptions> Profiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the legacy default profile name property.
        /// </summary>
        public string Profile
        {
            get => DefaultProfile;
            set => DefaultProfile = value;
        }

        /// <summary>
        /// Resolves a requested profile name to a configured profile.
        /// </summary>
        /// <param name="profileName">The requested profile name, or <see langword="null"/> for the default profile.</param>
        /// <returns>The configured profile and its logical name.</returns>
        /// <exception cref="EmailProfileNotFoundException">The requested profile is not configured.</exception>
        public (string Name, EmailProfileOptions Options) ResolveProfile(string? profileName)
        {
            string defaultProfileName = DefaultProfile.Trim();
            string name = string.IsNullOrWhiteSpace(profileName)
                ? defaultProfileName
                : profileName.Trim();

            if (string.Equals(name, EmailConstants.DefaultProfileName, StringComparison.OrdinalIgnoreCase))
            {
                name = defaultProfileName;
            }

            if (Profiles.Count == 0 && string.Equals(name, defaultProfileName, StringComparison.OrdinalIgnoreCase))
            {
                return (name, this);
            }

            foreach ((string configuredName, EmailProfileOptions profile) in Profiles)
            {
                if (string.Equals(configuredName, name, StringComparison.OrdinalIgnoreCase))
                {
                    return (configuredName, profile);
                }
            }

            throw new EmailProfileNotFoundException(name);
        }
    }
}
