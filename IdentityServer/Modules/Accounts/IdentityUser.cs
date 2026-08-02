using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.Accounts
{
    public sealed class IdentityUser : IdentityUser<Guid>
    {
        public DateTime CreatedUtc { get; set; }
        public DateTime ModifiedUtc { get; set; }
    }
}
