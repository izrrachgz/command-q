using Email.Models;
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
        public const string ActivitySourceName = "command-q.Email";

        /// <summary>
        /// Gets the meter name used by the email library.
        /// </summary>
        public const string MeterName = "command-q.Email";

        /// <summary>
        /// Gets the activity name for an email delivery batch.
        /// </summary>
        public const string SendActivityName = "email.send";

        /// <summary>
        /// Gets the activity name for SMTP connection operations.
        /// </summary>
        public const string SmtpConnectActivityName = "email.smtp.connect";

        /// <summary>
        /// Gets the activity name for SMTP authentication operations.
        /// </summary>
        public const string SmtpAuthenticateActivityName = "email.smtp.authenticate";

        /// <summary>
        /// Gets the activity name for SMTP message delivery operations.
        /// </summary>
        public const string SmtpSendActivityName = "email.smtp.send";

        /// <summary>
        /// Gets the activity name for SMTP disconnect operations.
        /// </summary>
        public const string SmtpDisconnectActivityName = "email.smtp.disconnect";

        /// <summary>
        /// Gets the activity name for MIME construction operations.
        /// </summary>
        public const string MimeBuildActivityName = "email.mime.build";

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

        /// <summary>
        /// Represents a successful operation.
        /// </summary>
        public const string SuccessOutcome = "success";

        /// <summary>
        /// Represents a failed operation.
        /// </summary>
        public const string FailureOutcome = "failure";

        /// <summary>
        /// Represents an operation skipped because email delivery is disabled.
        /// </summary>
        public const string DisabledOutcome = "disabled";

        /// <summary>
        /// Represents an operation cancelled by its caller.
        /// </summary>
        public const string CancelledOutcome = "cancelled";

        /// <summary>
        /// Identifies failures that occur while constructing MIME content.
        /// </summary>
        public const string MimeFailureCategory = "mime";

        /// <summary>
        /// Converts a failure kind to its stable, low-cardinality telemetry value.
        /// </summary>
        /// <param name="failureKind">The classified delivery failure.</param>
        /// <returns>A stable snake-case value suitable for traces and metrics.</returns>
        public static string GetFailureCategory(EmailFailureKind failureKind)
        {
            return failureKind switch
            {
                EmailFailureKind.Disabled => "disabled",
                EmailFailureKind.Authentication => "authentication",
                EmailFailureKind.Connectivity => "connectivity",
                EmailFailureKind.TransientSmtp => "transient_smtp",
                EmailFailureKind.PermanentSmtp => "permanent_smtp",
                EmailFailureKind.Protocol => "protocol",
                EmailFailureKind.ProfileNotFound => "profile_not_found",
                _ => "unknown"
            };
        }
    }
}
