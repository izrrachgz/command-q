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
        public const string ActivitySourceName = "CommandQ.Email.Worker";

        /// <summary>
        /// Gets the meter name used by the worker host.
        /// </summary>
        public const string MeterName = "CommandQ.Email.Worker";

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
    }
}
