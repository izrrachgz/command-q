namespace Email.Models
{
    /// <summary>
    /// Captures the SMTP server acceptance of one message.
    /// </summary>
    public sealed record EmailSendResult(int MessageNumber, string MessageId, string SmtpResponse, DateTimeOffset AcceptedAtUtc);
}
