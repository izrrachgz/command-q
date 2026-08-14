using Email.Abstractions;

namespace Email.Outbox.Services
{
    /// <summary>
    /// Adapts one sender to the profile-aware sender factory contract.
    /// </summary>
    internal sealed class FixedEmailSenderFactory(IEmailSender sender) : IEmailSenderFactory
    {
        /// <summary>
        /// Returns the configured fixed sender for every requested profile.
        /// </summary>
        /// <param name="profileName">The ignored logical profile name.</param>
        /// <returns>The fixed sender.</returns>
        public IEmailSender Create(string? profileName = null)
        {
            return sender;
        }
    }
}
