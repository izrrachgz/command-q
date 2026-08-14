using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Email.Telemetry
{
    /// <summary>
    /// Defines the email library's tracing and metrics instrumentation.
    /// </summary>
    public static class EmailTelemetry
    {
        /// <summary>
        /// Gets the activity source name used by the email library.
        /// </summary>
        public const string ActivitySourceName = "CommandQ.Email";

        /// <summary>
        /// Gets the meter name used by the email library.
        /// </summary>
        public const string MeterName = "CommandQ.Email";

        /// <summary>
        /// Gets the activity source used for SMTP and MIME operations.
        /// </summary>
        public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

        /// <summary>
        /// Gets the meter used for email delivery measurements.
        /// </summary>
        public static readonly Meter Meter = new(MeterName);

        /// <summary>
        /// Counts messages accepted by the SMTP server.
        /// </summary>
        public static readonly Counter<long> MessagesSent = Meter.CreateCounter<long>(
            "email.messages.sent",
            "{message}");

        /// <summary>
        /// Counts messages that failed during SMTP delivery.
        /// </summary>
        public static readonly Counter<long> MessagesFailed = Meter.CreateCounter<long>(
            "email.messages.failed",
            "{message}");

        /// <summary>
        /// Measures end-to-end email batch delivery duration.
        /// </summary>
        public static readonly Histogram<double> SendDuration = Meter.CreateHistogram<double>(
            "email.send.duration",
            "s");

        /// <summary>
        /// Measures SMTP connection duration.
        /// </summary>
        public static readonly Histogram<double> SmtpConnectionDuration = Meter.CreateHistogram<double>(
            "email.smtp.connection.duration",
            "s");

        /// <summary>
        /// Contains the low-cardinality tag name for the configured email profile.
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
