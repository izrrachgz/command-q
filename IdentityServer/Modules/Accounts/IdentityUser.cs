using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.Accounts
{
    /// <summary>Represents a local or externally linked IdentityServer user and its administrator-required recovery lock state.</summary>
    public sealed class IdentityUser : IdentityUser<Guid>
    {
        /// <summary>Gets or sets the UTC account creation time.</summary>
        public DateTime CreatedUtc { get; set; }
        /// <summary>Gets or sets the UTC time of the most recent account change.</summary>
        public DateTime ModifiedUtc { get; set; }
        /// <summary>Gets or sets whether only an administrator may unlock the account.</summary>
        public bool RequiresAdminUnlock { get; set; }
        /// <summary>Gets or sets when the administrator-required lock was applied.</summary>
        public DateTime? AdminLockoutUtc { get; set; }
    }
}
