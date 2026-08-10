using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Email.Configuration;

namespace Email.Services
{
    internal sealed class MailKitSmtpTransport : ISmtpTransport
    {
        private readonly SmtpClient _client;
        private readonly MailKitAuthenticationMechanismFactory _authenticationFactory;

        public MailKitSmtpTransport(EmailSmtpOptions options, MailKitAuthenticationMechanismFactory authenticationFactory)
        {
            _authenticationFactory = authenticationFactory;
            _client = new SmtpClient
            {
                Timeout = options.TimeoutMilliseconds,
                CheckCertificateRevocation = options.CheckCertificateRevocation
            };
        }

        public bool IsConnected => _client.IsConnected;

        public async Task ConnectAsync(string host, int port, EmailSecurityMode securityMode, CancellationToken cancellationToken)
        {
            await _client.ConnectAsync(host, port, ToSecureSocketOptions(securityMode), cancellationToken);
        }

        public async Task AuthenticateAsync(EmailAuthenticationOptions authentication, CancellationToken cancellationToken)
        {
            var mechanism = _authenticationFactory.Create(authentication);
            await _client.AuthenticateAsync(mechanism, cancellationToken);
        }

        public async Task<string> SendAsync(MimeMessage message, CancellationToken cancellationToken)
        {
            return await _client.SendAsync(message, cancellationToken);
        }

        public async Task DisconnectAsync(bool quit, CancellationToken cancellationToken)
        {
            await _client.DisconnectAsync(quit, cancellationToken);
        }

        public async Task NoOpAsync(CancellationToken cancellationToken)
        {
            if (_client != null && _client.IsConnected)
            {
                await _client.NoOpAsync(cancellationToken); // Ensure the connection is alive
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private static SecureSocketOptions ToSecureSocketOptions(EmailSecurityMode securityMode)
        {
            return securityMode switch
            {
                EmailSecurityMode.Auto => SecureSocketOptions.Auto,
                EmailSecurityMode.None => SecureSocketOptions.None,
                EmailSecurityMode.StartTlsWhenAvailable => SecureSocketOptions.StartTlsWhenAvailable,
                EmailSecurityMode.StartTls => SecureSocketOptions.StartTls,
                EmailSecurityMode.SslOnConnect => SecureSocketOptions.SslOnConnect,
                _ => throw new ArgumentOutOfRangeException(nameof(securityMode), securityMode, "Unsupported email security mode.")
            };
        }
    }
}
