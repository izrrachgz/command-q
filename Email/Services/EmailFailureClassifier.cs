using MailKit;
using Email.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Net.Sockets;

namespace Email.Services
{
    internal static class EmailFailureClassifier
    {
        public static EmailFailureKind Classify(Exception exception)
        {
            for (var current = exception; current is not null; current = current.InnerException)
            {
                switch (current)
                {
                    case AuthenticationException:
                    case ServiceNotAuthenticatedException:
                        return EmailFailureKind.Authentication;
                    case SmtpCommandException smtpException:
                        return ClassifyStatusCode(smtpException.StatusCode);
                    case SmtpProtocolException:
                        return EmailFailureKind.Protocol;
                    case ServiceNotConnectedException:
                    case SocketException:
                    case IOException:
                    case TimeoutException:
                        return EmailFailureKind.Connectivity;
                }
            }

            return EmailFailureKind.Unknown;
        }

        private static EmailFailureKind ClassifyStatusCode(SmtpStatusCode statusCode)
        {
            var numericStatusCode = (int)statusCode;

            if (numericStatusCode is >= 400 and < 500)
            {
                return EmailFailureKind.TransientSmtp;
            }

            if (numericStatusCode is >= 500 and < 600)
            {
                return EmailFailureKind.PermanentSmtp;
            }

            return EmailFailureKind.Protocol;
        }
    }
}
