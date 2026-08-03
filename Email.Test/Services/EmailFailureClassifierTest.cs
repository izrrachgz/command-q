using Email.Models;
using Email.Services;
using MailKit.Net.Smtp;

namespace Email.Test.Services
{
    public sealed class EmailFailureClassifierTest
    {
        [Fact]
        public void Classify_Should_Identify_Transient_Smtp_Status()
        {
            var exception = new SmtpCommandException(
                SmtpErrorCode.MessageNotAccepted,
                SmtpStatusCode.ServiceNotAvailable,
                "SMTP service is temporarily unavailable.");

            var result = EmailFailureClassifier.Classify(exception);

            Assert.Equal(EmailFailureKind.TransientSmtp, result);
        }

        [Fact]
        public void Classify_Should_Identify_Permanent_Smtp_Status()
        {
            var exception = new SmtpCommandException(
                SmtpErrorCode.RecipientNotAccepted,
                SmtpStatusCode.MailboxUnavailable,
                "Mailbox is unavailable.");

            var result = EmailFailureClassifier.Classify(exception);

            Assert.Equal(EmailFailureKind.PermanentSmtp, result);
        }
    }
}
