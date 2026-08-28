using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Email.Worker.Telemetry
{
    /// <summary>
    /// Defines the email worker's polling and batch tracing instrumentation.
    /// </summary>
    internal static class EmailWorkerTelemetry
    {
        /// <summary>
        /// Gets the activity source name used by the worker host.
        /// </summary>
        public const string ActivitySourceName = "command-q.Email.Worker";

        /// <summary>
        /// Gets the meter name used by the worker host.
        /// </summary>
        public const string MeterName = "command-q.Email.Worker";

        /// <summary>
        /// Gets the activity name for a worker polling cycle.
        /// </summary>
        public const string PollActivityName = "email.worker.poll";

        /// <summary>
        /// Gets the activity name for a worker batch-processing attempt.
        /// </summary>
        public const string BatchActivityName = "email.worker.batch";

        /// <summary>
        /// Gets the activity source used for polling and batch operations.
        /// </summary>
        public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

        /// <summary>
        /// Gets the meter used for worker measurements.
        /// </summary>
        public static readonly Meter Meter = new(MeterName);

        /// <summary>
        /// Measures the duration of a worker polling cycle.
        /// </summary>
        public static readonly Histogram<double> PollDuration = Meter.CreateHistogram<double>(
            "email.worker.poll.duration",
            "s");

        /// <summary>
        /// Contains the low-cardinality tag name for a worker outcome.
        /// </summary>
        public const string OutcomeTag = "outcome";

        /// <summary>
        /// Represents a successfully processed polling cycle.
        /// </summary>
        public const string SuccessOutcome = "success";

        /// <summary>
        /// Represents a polling cycle that found no queued messages.
        /// </summary>
        public const string EmptyOutcome = "empty";

        /// <summary>
        /// Represents a failed polling or batch-processing operation.
        /// </summary>
        public const string FailureOutcome = "failure";
    }
}
