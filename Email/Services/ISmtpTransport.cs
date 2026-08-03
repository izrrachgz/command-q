using MimeKit;
using Email.Configuration;

namespace Email.Services
{
    internal interface ISmtpTransport : IDisposable
    {
        bool IsConnected { get; }

        Task ConnectAsync(
            string host,
            int port,
            EmailSecurityMode securityMode,
            CancellationToken cancellationToken);

        Task AuthenticateAsync(
            EmailAuthenticationOptions authentication,
            CancellationToken cancellationToken);

        Task<string> SendAsync(MimeMessage message, CancellationToken cancellationToken);

        Task DisconnectAsync(bool quit, CancellationToken cancellationToken);
    }
}
