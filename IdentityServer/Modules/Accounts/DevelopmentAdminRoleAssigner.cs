using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.Accounts
{
    public sealed class DevelopmentAdminRoleAssigner(
        UserManager<IdentityUser> userManager,
        IHostEnvironment environment)
    {
        public async Task<IdentityResult> AssignIfRequired(IdentityUser user)
        {
            if (!environment.IsDevelopment() || (await userManager.GetUsersInRoleAsync(AccountConstants.AdminRole)).Count > 0)
                return IdentityResult.Success;

            return await userManager.AddToRoleAsync(user, AccountConstants.AdminRole);
        }
    }
}
