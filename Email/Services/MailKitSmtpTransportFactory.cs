using Email.Configuration;

namespace Email.Services
{
    internal sealed class MailKitSmtpTransportFactory : ISmtpTransportFactory
    {
        private readonly MailKitAuthenticationMechanismFactory _authenticationFactory;

        public MailKitSmtpTransportFactory(MailKitAuthenticationMechanismFactory authenticationFactory)
        {
            _authenticationFactory = authenticationFactory;
        }

        public ISmtpTransport Create(EmailSmtpOptions options)
        {
            return new MailKitSmtpTransport(options, _authenticationFactory);
        }
    }
}
