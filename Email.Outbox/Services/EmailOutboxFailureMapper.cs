using Email.Models;
using Email.Outbox.Models;

namespace Email.Outbox.Services
{
    internal static class EmailOutboxFailureMapper
    {
        public static EmailOutboxFailureKind Map(EmailFailureKind failureKind)
        {
            return failureKind switch
            {
                EmailFailureKind.Disabled => EmailOutboxFailureKind.Disabled,
                EmailFailureKind.Authentication => EmailOutboxFailureKind.Authentication,
                EmailFailureKind.Connectivity => EmailOutboxFailureKind.Connectivity,
                EmailFailureKind.TransientSmtp => EmailOutboxFailureKind.TransientSmtp,
                EmailFailureKind.PermanentSmtp => EmailOutboxFailureKind.PermanentSmtp,
                EmailFailureKind.Protocol => EmailOutboxFailureKind.Protocol,
                EmailFailureKind.ProfileNotFound => EmailOutboxFailureKind.ProfileNotFound,
                _ => EmailOutboxFailureKind.Unknown
            };
        }
    }
}
