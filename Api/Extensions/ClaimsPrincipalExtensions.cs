using Api.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Api.Extensions
{
    /// <summary>
    /// Helpers for resolving authenticated account information from JWT claims.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the account identifier from the JWT subject claim.
        /// </summary>
        public static Guid? GetAccountId(this ClaimsPrincipal user)
        {
            return user.TryGetAccountId(out var accountId) ? accountId : null;
        }

        /// <summary>
        /// Tries to get the account identifier from the JWT subject claim.
        /// </summary>
        public static bool TryGetAccountId(this ClaimsPrincipal user, out Guid accountId)
        {
            var accountIdValue = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(accountIdValue, out accountId);
        }

        /// <summary>
        /// Gets the username from the JWT unique name claim.
        /// </summary>
        public static string? GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
                ?? user.FindFirstValue(ClaimTypes.Name);
        }

        /// <summary>
        /// Gets the JWT identifier claim when present.
        /// </summary>
        public static string? GetJwtId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(JwtRegisteredClaimNames.Jti);
        }

        /// <summary>
        /// Gets all role claims for the current user.
        /// </summary>
        public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal user)
        {
            return [.. user.FindAll(ClaimTypes.Role).Select(claim => claim.Value)];
        }

        /// <summary>
        /// Checks whether the current user has the provided role.
        /// </summary>
        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            return user.GetRoles().Any(value => string.Equals(value, role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets a current-user snapshot when the principal contains a valid account id.
        /// </summary>
        public static CurrentUser? GetCurrentUser(this ClaimsPrincipal user)
        {
            if (!user.TryGetAccountId(out var accountId))
                return null;

            return new()
            {
                AccountId = accountId,
                Username = user.GetUsername(),
                JwtId = user.GetJwtId(),
                Roles = user.GetRoles(),
                IsAuthenticated = user.Identity?.IsAuthenticated == true
            };
        }
    }
}
