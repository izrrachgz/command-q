using Email.Abstractions;
using Email.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Email.Services
{
    internal sealed class MailKitEmailSenderFactory : IEmailSenderFactory, IDisposable
    {
        private readonly IOptionsMonitor<EmailOptions> _options;
        private readonly ISmtpTransportFactory _transportFactory;
        private readonly MimeMessageFactory _messageFactory;
        private readonly EmailMessageValidator _messageValidator;
        private readonly ILoggerFactory _loggerFactory;
        private readonly Dictionary<string, MailKitEmailSender> _senders = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new();
        private readonly IDisposable? _configurationSubscription;

        public MailKitEmailSenderFactory(
            IOptionsMonitor<EmailOptions> options,
            ISmtpTransportFactory transportFactory,
            MimeMessageFactory messageFactory,
            EmailMessageValidator messageValidator,
            ILoggerFactory loggerFactory)
        {
            _options = options;
            _transportFactory = transportFactory;
            _messageFactory = messageFactory;
            _messageValidator = messageValidator;
            _loggerFactory = loggerFactory;
            _configurationSubscription = options.OnChange(RemoveStaleSenders);
        }

        public IEmailSender Create(string? profileName = null)
        {
            (string name, _) = _options.CurrentValue.ResolveProfile(profileName);

            lock (_lock)
            {
                if (_senders.TryGetValue(name, out MailKitEmailSender? sender))
                {
                    return sender;
                }

                sender = new MailKitEmailSender(
                    name,
                    _options,
                    _transportFactory,
                    _messageFactory,
                    _messageValidator,
                    _loggerFactory.CreateLogger<MailKitEmailSender>());
                _senders.Add(name, sender);
                return sender;
            }
        }

        public void Dispose()
        {
            _configurationSubscription?.Dispose();

            lock (_lock)
            {
                foreach (MailKitEmailSender sender in _senders.Values)
                {
                    sender.Dispose();
                }

                _senders.Clear();
            }
        }

        private void RemoveStaleSenders(EmailOptions options, string? _)
        {
            lock (_lock)
            {
                string[] staleNames = _senders.Keys
                    .Where(profileName => !IsConfigured(options, profileName))
                    .ToArray();

                foreach (string staleName in staleNames)
                {
                    _senders[staleName].Dispose();
                    _senders.Remove(staleName);
                }
            }
        }

        private static bool IsConfigured(EmailOptions options, string profileName)
        {
            if (options.Profiles.Count == 0)
            {
                return string.Equals(
                    profileName,
                    options.DefaultProfile.Trim(),
                    StringComparison.OrdinalIgnoreCase);
            }

            return options.Profiles.ContainsKey(profileName);
        }
    }
}
