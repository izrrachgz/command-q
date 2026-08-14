using Email.Models;
using Email.Configuration;
using Microsoft.Extensions.Logging;

namespace Email.Logging
{
    internal static partial class EmailSenderLog
    {
        [LoggerMessage(EventId = EmailEventIds.BatchStarting, Level = LogLevel.Debug, Message = "Connecting to SMTP endpoint {SmtpHost}:{SmtpPort} to send {MessageCount} messages.")]
        public static partial void BatchStarting(this ILogger logger, string smtpHost, int smtpPort, int messageCount);

        [LoggerMessage(EventId = EmailEventIds.SmtpConnected, Level = LogLevel.Debug, Message = "SMTP connection established.")]
        public static partial void SmtpConnected(this ILogger logger);

        [LoggerMessage(EventId = EmailEventIds.SmtpAuthenticated, Level = LogLevel.Debug, Message = "SMTP session authenticated.")]
        public static partial void SmtpAuthenticated(this ILogger logger);

        [LoggerMessage(EventId = EmailEventIds.MessageSent, Level = LogLevel.Debug, Message = "Sent email message {MessageNumber} of {MessageCount}.")]
        public static partial void MessageSent(this ILogger logger, int messageNumber, int messageCount);

        [LoggerMessage(EventId = EmailEventIds.BatchCompleted, Level = LogLevel.Information, Message = "Email batch completed with {MessageCount} messages sent.")]
        public static partial void BatchCompleted(this ILogger logger, int messageCount);

        [LoggerMessage(EventId = EmailEventIds.BatchCancelled, Level = LogLevel.Debug, Message = "Email batch was cancelled after {SentCount} of {MessageCount} messages.")]
        public static partial void BatchCancelled(this ILogger logger, int sentCount, int messageCount);

        [LoggerMessage(EventId = EmailEventIds.BatchFailed, Level = LogLevel.Error, Message = "Email batch failed with classification {FailureKind} after {SentCount} of {MessageCount} messages.")]
        public static partial void BatchFailed(this ILogger logger, Exception exception, EmailFailureKind failureKind, int sentCount, int messageCount);

        [LoggerMessage(EventId = EmailEventIds.DisconnectFailed, Level = LogLevel.Warning, Message = "The SMTP session could not disconnect gracefully and will be disposed.")]
        public static partial void DisconnectFailed(this ILogger logger, Exception exception);

        [LoggerMessage(EventId = EmailEventIds.ConfigurationReloaded, Level = LogLevel.Information, Message = "Email delivery profile {ProfileName} configuration reloaded. Host: {SmtpHost}. Port: {SmtpPort}. Authentication: {AuthenticationType}. Enabled: {Enabled}. Sender configured: {SenderConfigured}.")]
        public static partial void ConfigurationReloaded(this ILogger logger, string profileName, string smtpHost, int smtpPort, EmailAuthenticationType authenticationType, bool enabled, bool senderConfigured);
    }
}
