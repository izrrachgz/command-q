using NodaTime;

namespace Email.Models
{
    /// <summary>
    /// Captures the SMTP server acceptance of one message.
    /// </summary>
    /// <param name="MessageNumber">The one-based position in the requested batch.</param>
    /// <param name="MessageId">The MIME message identifier accepted by the server.</param>
    /// <param name="SmtpResponse">The SMTP server response.</param>
    /// <param name="AcceptedAtUtc">The unambiguous UTC instant at which acceptance was recorded.</param>
    public sealed record EmailSendResult(int MessageNumber, string MessageId, string SmtpResponse, Instant AcceptedAtUtc);
}
