using Email.Models;

namespace Email.Outbox.Serialization
{
    internal interface IEmailOutboxSerializer
    {
        Task<string> SerializeAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default);

        EmailMessage Deserialize(string payload);
    }
}
