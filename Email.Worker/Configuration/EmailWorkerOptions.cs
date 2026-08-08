using Email.Worker.Constants;

namespace Email.Worker.Configuration
{
    internal sealed class EmailWorkerOptions
    {
        public bool Enabled { get; set; } = true;
        public int BatchSize { get; set; } = EmailWorkerConstants.DefaultBatchSize;
        public int PollIntervalSeconds { get; set; } = EmailWorkerConstants.DefaultPollIntervalSeconds;
        public int LeaseSeconds { get; set; } = EmailWorkerConstants.DefaultLeaseSeconds;
        public int ErrorDelaySeconds { get; set; } = EmailWorkerConstants.DefaultErrorDelaySeconds;
    }
}
