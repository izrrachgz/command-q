using Email.Models;

namespace Application.Modules.Notifications.Models
{
    public sealed record ApplicationEmailRequest
    {
        public required EmailMessage Message { get; init; }

        public ApplicationEmailMetadata? Metadata { get; init; } = new();
    }
}
