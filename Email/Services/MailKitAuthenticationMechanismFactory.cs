using MailKit.Security;
using Email.Configuration;

namespace Email.Services
{
    internal sealed class MailKitAuthenticationMechanismFactory
    {
        public SaslMechanism Create(EmailAuthenticationOptions authentication)
        {
            ArgumentNullException.ThrowIfNull(authentication);

            return authentication.Type switch
            {
                EmailAuthenticationType.Login => new SaslMechanismLogin(
                    authentication.Login.Username,
                    authentication.Login.Password),
                EmailAuthenticationType.Plain => new SaslMechanismPlain(
                    authentication.Plain.Username,
                    authentication.Plain.Password),
                EmailAuthenticationType.OAuth => new SaslMechanismOAuth2(
                    authentication.OAuth.Username,
                    authentication.OAuth.AccessToken),
                EmailAuthenticationType.Anonymous => throw new InvalidOperationException(
                    "Anonymous SMTP delivery does not use an authentication mechanism."),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(authentication),
                    authentication.Type,
                    "Unsupported email authentication type.")
            };
        }
    }
}
