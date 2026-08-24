using System.Diagnostics;
using Email.Outbox.Models;
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
        /// Gets the activity name for enqueue operations.
        /// </summary>
        public const string EnqueueActivityName = "email.outbox.enqueue";

        /// <summary>
        /// Gets the activity name for batch-processing operations.
        /// </summary>
        public const string ProcessActivityName = "email.outbox.process";

        /// <summary>
        /// Gets the activity name for profile-specific batch processing.
        /// </summary>
        public const string ProfileProcessActivityName = "email.outbox.profile.process";

        /// <summary>
        /// Gets the activity name for message serialization.
        /// </summary>
        public const string SerializeActivityName = "email.outbox.serialize";

        /// <summary>
        /// Gets the activity name for persistence claim operations.
        /// </summary>
        public const string ClaimActivityName = "email.outbox.claim";

        /// <summary>
        /// Gets the activity name for retry scheduling.
        /// </summary>
        public const string RetryActivityName = "email.outbox.retry";

        /// <summary>
        /// Gets the activity name for dead-letter operations.
        /// </summary>
        public const string DeadLetterActivityName = "email.outbox.dead_letter";

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

        /// <summary>
        /// Represents a successful operation.
        /// </summary>
        public const string SuccessOutcome = "success";

        /// <summary>
        /// Represents a failed operation.
        /// </summary>
        public const string FailureOutcome = "failure";

        /// <summary>
        /// Represents an operation with no available work.
        /// </summary>
        public const string EmptyOutcome = "empty";

        /// <summary>
        /// Represents a newly enqueued message.
        /// </summary>
        public const string EnqueuedOutcome = "enqueued";

        /// <summary>
        /// Represents an enqueue request resolved through idempotency.
        /// </summary>
        public const string DuplicateOutcome = "duplicate";

        /// <summary>
        /// Represents a batch containing both completed and failed work.
        /// </summary>
        public const string PartialOutcome = "partial";

        /// <summary>
        /// Represents a batch in which every payload failed deserialization.
        /// </summary>
        public const string SerializationFailureOutcome = "serialization_failure";

        /// <summary>
        /// Represents an operation scheduled for another attempt.
        /// </summary>
        public const string RetryOutcome = "retry";

        /// <summary>
        /// Represents an operation moved to the dead-letter state.
        /// </summary>
        public const string DeadLetterOutcome = "dead_letter";

        /// <summary>
        /// Converts a failure kind to its stable, low-cardinality telemetry value.
        /// </summary>
        /// <param name="failureKind">The classified outbox delivery failure.</param>
        /// <returns>A stable snake-case value suitable for traces and metrics.</returns>
        public static string GetFailureCategory(EmailOutboxFailureKind failureKind)
        {
            return failureKind switch
            {
                EmailOutboxFailureKind.Disabled => "disabled",
                EmailOutboxFailureKind.Authentication => "authentication",
                EmailOutboxFailureKind.Connectivity => "connectivity",
                EmailOutboxFailureKind.TransientSmtp => "transient_smtp",
                EmailOutboxFailureKind.PermanentSmtp => "permanent_smtp",
                EmailOutboxFailureKind.Protocol => "protocol",
                EmailOutboxFailureKind.Serialization => "serialization",
                EmailOutboxFailureKind.Validation => "validation",
                EmailOutboxFailureKind.ProfileNotFound => "profile_not_found",
                _ => "unknown"
            };
        }
    }
}
