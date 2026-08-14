using Email.Constants;

namespace Email.Configuration
{
    /// <summary>
    /// Configures one named SMTP delivery profile.
    /// </summary>
    public class EmailProfileOptions
    {
        /// <summary>
        /// Gets or sets whether delivery through this profile is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the SMTP endpoint configuration.
        /// </summary>
        public EmailSmtpOptions Smtp { get; set; } = new();

        /// <summary>
        /// Gets or sets the SMTP authentication configuration.
        /// </summary>
        public EmailAuthenticationOptions Authentication { get; set; } = new();

        /// <summary>
        /// Gets or sets the default sender configuration.
        /// </summary>
        public EmailSenderOptions Sender { get; set; } = new();

        /// <summary>
        /// Gets or sets the delivery policy for this profile.
        /// </summary>
        public EmailDeliveryPolicyOptions DeliveryPolicy { get; set; } = new();
    }
}
