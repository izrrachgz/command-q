namespace Application.Modules.Notifications.Models
{
    /// <summary>Contains durable queue metadata associated with an application email.</summary>
    public sealed record ApplicationEmailMetadata
    {
        /// <summary>Gets the key used to make repeated queue submissions idempotent.</summary>
        public string? IdempotencyKey { get; init; }

        /// <summary>
        /// Gets the normalized IdentityServer subject used to associate the message with account email history.
        /// Local identity GUID subjects use the compact <c>N</c> format.
        /// </summary>
        public string? AccountReference { get; init; }

        /// <summary>Gets the optional cross-service correlation identifier.</summary>
        public string? CorrelationId { get; init; }
    }
}
