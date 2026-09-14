using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Modules.Accounts.Services.SignIn
{
    /// <summary>Assigns the first locally registered development account to the default administrator role.</summary>
    /// <remarks>The assignment is disabled outside the Development environment and after an administrator already exists.</remarks>
    public sealed class DevelopmentAdminRoleAssigner(
        UserManager<IdentityUser> userManager,
        IHostEnvironment environment)
    {
        /// <summary>Assigns the administrator role when the development bootstrap conditions are satisfied.</summary>
        /// <param name="user">The newly registered identity user.</param>
        /// <returns>The Identity role-assignment result, or a successful no-op result when assignment is unnecessary.</returns>
        public async Task<IdentityResult> AssignIfRequired(IdentityUser user)
        {
            if (!environment.IsDevelopment() || (await userManager.GetUsersInRoleAsync(AccountConstants.AdminRole)).Count > 0)
                return IdentityResult.Success;

            return await userManager.AddToRoleAsync(user, AccountConstants.AdminRole);
        }
    }
}
