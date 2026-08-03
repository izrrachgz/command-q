using Email.Configuration;

namespace Email.Services
{
    internal interface ISmtpTransportFactory
    {
        ISmtpTransport Create(EmailSmtpOptions options);
    }
}
