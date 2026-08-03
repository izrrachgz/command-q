namespace Email.IntegrationTest.Configuration
{
    internal sealed class Smtp4DevOptions
    {
        public const string SectionName = "Smtp4Dev";

        public bool Enabled { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string ApiBaseUrl { get; set; } = string.Empty;
        public string SenderAddress { get; set; } = string.Empty;
        public string RecipientAddress { get; set; } = string.Empty;
        public int PollingTimeoutSeconds { get; set; }
    }
}
