using Email.Client.Models;

namespace Application.Modules.Notifications.Models
{
    public sealed record ApplicationEmailRequest
    {
        public required EmailMessage Message { get; init; }

        /// <summary>
        /// Gets the logical email profile selected by the owning workflow.
        /// </summary>
        public string? ProfileName { get; init; }

        public ApplicationEmailMetadata? Metadata { get; init; } = new();
    }
}
