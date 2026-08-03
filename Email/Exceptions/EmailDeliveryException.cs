using Email.Models;

namespace Email.Exceptions
{
    /// <summary>
    /// Reports a classified SMTP failure and any messages accepted before it occurred.
    /// </summary>
    public sealed class EmailDeliveryException : Exception
    {
        internal EmailDeliveryException(
            string message,
            EmailFailureKind failureKind,
            int requestedCount,
            IReadOnlyList<EmailSendResult> sentMessages,
            int? failedMessageNumber = null,
            string? failedMessageId = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            FailureKind = failureKind;
            RequestedCount = requestedCount;
            SentMessages = sentMessages;
            FailedMessageNumber = failedMessageNumber;
            FailedMessageId = failedMessageId;
        }

        public EmailFailureKind FailureKind { get; }
        public int RequestedCount { get; }
        public IReadOnlyList<EmailSendResult> SentMessages { get; }
        public int SentCount => SentMessages.Count;
        public int? FailedMessageNumber { get; }
        public string? FailedMessageId { get; }

        internal static EmailDeliveryException Disabled(int requestedCount)
        {
            return new EmailDeliveryException(
                "Email delivery is disabled by configuration.",
                EmailFailureKind.Disabled,
                requestedCount,
                Array.Empty<EmailSendResult>());
        }
    }
}
