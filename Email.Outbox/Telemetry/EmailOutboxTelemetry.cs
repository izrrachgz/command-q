using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Email.Outbox.Telemetry
{
    /// <summary>
    /// Defines the durable email outbox tracing and metrics instrumentation.
    /// </summary>
    public static class EmailOutboxTelemetry
    {
        /// <summary>
        /// Gets the activity source name used by the email outbox library.
        /// </summary>
        public const string ActivitySourceName = "CommandQ.Email.Outbox";

        /// <summary>
        /// Gets the meter name used by the email outbox library.
        /// </summary>
        public const string MeterName = "CommandQ.Email.Outbox";

        /// <summary>
        /// Gets the activity source used for durable queue operations.
        /// </summary>
        public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

        /// <summary>
        /// Gets the meter used for durable queue measurements.
        /// </summary>
        public static readonly Meter Meter = new(MeterName);

        /// <summary>
        /// Counts successfully persisted outbox messages.
        /// </summary>
        public static readonly Counter<long> Enqueued = Meter.CreateCounter<long>(
            "email.outbox.enqueued",
            "{message}");

        /// <summary>
        /// Counts enqueue requests resolved by an existing idempotency record.
        /// </summary>
        public static readonly Counter<long> Duplicates = Meter.CreateCounter<long>(
            "email.outbox.duplicates",
            "{message}");

        /// <summary>
        /// Counts messages scheduled for another delivery attempt.
        /// </summary>
        public static readonly Counter<long> Retried = Meter.CreateCounter<long>(
            "email.outbox.retried",
            "{message}");

        /// <summary>
        /// Counts messages moved to the dead-letter state.
        /// </summary>
        public static readonly Counter<long> DeadLettered = Meter.CreateCounter<long>(
            "email.outbox.dead_lettered",
            "{message}");

        /// <summary>
        /// Measures the number of messages claimed for processing in a batch.
        /// </summary>
        public static readonly Histogram<long> BatchSize = Meter.CreateHistogram<long>(
            "email.outbox.batch.size",
            "{message}");

        /// <summary>
        /// Measures the age of messages when they are claimed for processing.
        /// </summary>
        public static readonly Histogram<double> QueueAge = Meter.CreateHistogram<double>(
            "email.outbox.queue.age",
            "s");

        /// <summary>
        /// Contains the low-cardinality tag name for the persistence engine.
        /// </summary>
        public const string EngineTag = "engine";

        /// <summary>
        /// Contains the low-cardinality tag name for an email profile.
        /// </summary>
        public const string ProfileTag = "profile";

        /// <summary>
        /// Contains the low-cardinality tag name for a failure category.
        /// </summary>
        public const string FailureCategoryTag = "failure.category";

        /// <summary>
        /// Contains the low-cardinality tag name for an operation outcome.
        /// </summary>
        public const string OutcomeTag = "outcome";
    }
}
