using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.Accounts
{
    /// <summary>Represents an IdentityServer role keyed by a globally unique identifier.</summary>
    public sealed class IdentityRole : IdentityRole<Guid>
    {
    }
}
