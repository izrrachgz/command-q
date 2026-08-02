using Api.Models;
using System.Security.Claims;

namespace Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool TryGetIdentityKey(
            this ClaimsPrincipal user,
            out string issuer,
            out string subject)
        {
            issuer = user.FindFirstValue(AuthenticationClaimTypes.Issuer) ?? string.Empty;
            subject = user.FindFirstValue(AuthenticationClaimTypes.Subject) ?? string.Empty;

            return user.Identity?.IsAuthenticated == true &&
                !string.IsNullOrWhiteSpace(issuer) &&
                !string.IsNullOrWhiteSpace(subject);
        }

        public static string? GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(AuthenticationClaimTypes.Name)
                ?? user.FindFirstValue(AuthenticationClaimTypes.PreferredUsername)
                ?? user.FindFirstValue(ClaimTypes.Name);
        }

        public static string? GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(AuthenticationClaimTypes.Email)
                ?? user.FindFirstValue(ClaimTypes.Email);
        }

        public static string? GetJwtId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(AuthenticationClaimTypes.JwtId);
        }

        public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal user)
        {
            return user.FindAll(AuthenticationClaimTypes.Role)
                .Concat(user.FindAll(ClaimTypes.Role))
                .Select(claim => claim.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public static IReadOnlyList<string> GetScopes(this ClaimsPrincipal user)
        {
            return user.FindAll(AuthenticationClaimTypes.Scope)
                .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }
    }
}
