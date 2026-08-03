namespace Email.Configuration
{
    /// <summary>
    /// Configures SMTP delivery and the default message sender.
    /// </summary>
    public sealed class EmailOptions
    {
        public bool Enabled { get; set; }
        public EmailSmtpOptions Smtp { get; set; } = new();
        public EmailAuthenticationOptions Authentication { get; set; } = new();
        public EmailSenderOptions Sender { get; set; } = new();
        public EmailDeliveryPolicyOptions DeliveryPolicy { get; set; } = new();
    }
}
