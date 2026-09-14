namespace IdentityServer.Modules.Accounts
{
    /// <summary>Represents a security-relevant authentication or account event retained for auditing and administration.</summary>
    public sealed class IdentitySecurityEvent
    {
        /// <summary>Gets or sets the event identifier.</summary>
        public long Id { get; set; }
        /// <summary>Gets or sets the related identity user identifier, when known.</summary>
        public Guid? UserId { get; set; }
        /// <summary>Gets or sets the stable security action name.</summary>
        public required string Action { get; set; }
        /// <summary>Gets or sets the related OpenID client identifier, when known.</summary>
        public string? ClientId { get; set; }
        /// <summary>Gets or sets the originating IP address, when available.</summary>
        public string? IpAddress { get; set; }
        /// <summary>Gets or sets the originating user-agent value, when available.</summary>
        public string? UserAgent { get; set; }
        /// <summary>Gets or sets optional structured JSON details.</summary>
        public string? DetailsJson { get; set; }
        /// <summary>Gets or sets the UTC event time.</summary>
        public DateTime OccurredUtc { get; set; }
    }
}
