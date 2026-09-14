using Microsoft.AspNetCore;
using System.Security.Claims;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Identity;
using IdentityServer.Resources;
using IdentityServer.Modules.Accounts;
using IdentityServer.Modules.Accounts.Abstractions.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using static OpenIddict.Abstractions.OpenIddictConstants;
using IdentityServer.Constants;
using Microsoft.AspNetCore.RateLimiting;
using AppIdentityUser = IdentityServer.Modules.Accounts.IdentityUser;

namespace IdentityServer.Modules.OpenIddict
{
    [ApiController]
    public sealed class AuthorizationController(
        UserManager<AppIdentityUser> userManager,
        SignInManager<AppIdentityUser> signInManager,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        IIdentitySecurityEventRecorder securityEvents) : ControllerBase
    {
        private const string LoginPrompt = "login";
        private const string NonePrompt = "none";

        [AllowAnonymous, HttpGet(OpenIddictRoutes.Authorization), HttpPost(OpenIddictRoutes.Authorization)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AuthorizeEndpoint(CancellationToken cancellationToken)
        {
            var request = HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException(
                    IdentityServerMessages.Protocol_AuthorizationRequestUnavailable);

            var authentication = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

            if (!authentication.Succeeded || request.HasPromptValue(LoginPrompt))
            {
                if (request.HasPromptValue(NonePrompt))
                {
                    return ProtocolForbid(Errors.LoginRequired, IdentityServerMessages.Protocol_UserNotSignedIn);
                }

                return Challenge(IdentityConstants.ApplicationScheme);
            }

            var user = await userManager.GetUserAsync(authentication.Principal!);

            if (user is null || !await signInManager.CanSignInAsync(user) || await userManager.IsLockedOutAsync(user))
            {
                return ProtocolForbid(Errors.LoginRequired, IdentityServerMessages.Protocol_IdentityAccountUnavailable);
            }

            var principal = await CreateUserPrincipal(user, request.GetScopes(), cancellationToken);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [AllowAnonymous, HttpPost(OpenIddictRoutes.Token)]
        [EnableRateLimiting(IdentityServerRateLimitPolicies.Authentication)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Exchange(CancellationToken cancellationToken)
        {
            var request = HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException(
                    IdentityServerMessages.Protocol_TokenRequestUnavailable);

            if (request.IsClientCredentialsGrantType())
            {
                return await CreateClientCredentialsPrincipal(request, cancellationToken);
            }

            if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
            {
                return ProtocolForbid(Errors.UnsupportedGrantType, IdentityServerMessages.Protocol_UnsupportedGrantType);
            }

            var authentication = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var authenticatedPrincipal = authentication.Principal;

            if (!authentication.Succeeded || authenticatedPrincipal is null)
            {
                return ProtocolForbid(Errors.InvalidGrant, IdentityServerMessages.Protocol_TokenRequestCouldNotBeAuthenticated);
            }

            var subject = authenticatedPrincipal?.GetClaim(Claims.Subject);
            var user = string.IsNullOrWhiteSpace(subject) ? null : await userManager.FindByIdAsync(subject);

            if (user is null || !await signInManager.CanSignInAsync(user) || await userManager.IsLockedOutAsync(user))
            {
                return ProtocolForbid(Errors.InvalidGrant, IdentityServerMessages.Protocol_IdentityAccountNoLongerAvailable);
            }

            var principal = await CreateUserPrincipal(
                user,
                authenticatedPrincipal!.GetScopes(),
                cancellationToken);

            principal.SetResources(authenticatedPrincipal!.GetResources());
            principal.SetAuthorizationId(authenticatedPrincipal!.GetAuthorizationId());

            if (request.IsRefreshTokenGrantType())
            {
                await securityEvents.Record(
                    AccountConstants.Actions.TokenRefreshed,
                    user.Id,
                    request.ClientId,
                    cancellationToken: cancellationToken);
            }

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [AllowAnonymous, HttpGet(OpenIddictRoutes.EndSession), HttpPost(OpenIddictRoutes.EndSession)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> EndSession(CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            await signInManager.SignOutAsync();

            if (user is not null)
            {
                await securityEvents.Record(
                    AccountConstants.Actions.Logout,
                    user.Id,
                    cancellationToken: cancellationToken);
            }

            return SignOut(
                new AuthenticationProperties(),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [AllowAnonymous, HttpGet(OpenIddictRoutes.UserInfo), HttpPost(OpenIddictRoutes.UserInfo)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UserInfo()
        {
            var authentication = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (!authentication.Succeeded || authentication.Principal is null)
            {
                return ProtocolForbid(Errors.InvalidToken, IdentityServerMessages.Protocol_TokenCouldNotBeAuthenticated);
            }

            var subject = authentication.Principal.GetClaim(Claims.Subject);
            var user = string.IsNullOrWhiteSpace(subject) ? null : await userManager.FindByIdAsync(subject);

            if (user is null)
            {
                return ProtocolForbid(Errors.InvalidToken, IdentityServerMessages.Protocol_IdentityAccountNoLongerAvailable);
            }

            return Ok(new Dictionary<string, object?>
            {
                [Claims.Subject] = user.Id.ToString(),
                [Claims.Name] = user.UserName,
                [Claims.Email] = user.Email,
                [Claims.Role] = await userManager.GetRolesAsync(user)
            });
        }

        private async Task<IActionResult> CreateClientCredentialsPrincipal(OpenIddictRequest request, CancellationToken cancellationToken)
        {
            var application = await applicationManager.FindByClientIdAsync(request.ClientId!, cancellationToken);

            if (application is null)
            {
                return ProtocolForbid(Errors.InvalidClient, IdentityServerMessages.Protocol_ClientUnavailable);
            }

            var identity = new ClaimsIdentity(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                Claims.Name,
                Claims.Role);

            identity.SetClaim(Claims.Subject, request.ClientId!);
            identity.SetClaim(Claims.Name, await applicationManager.GetDisplayNameAsync(application, cancellationToken));
            identity.SetClaim(Claims.ClientId, request.ClientId!);

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());
            principal.SetResources(await ResolveResources(principal.GetScopes(), cancellationToken));
            principal.SetDestinations(static claim => [Destinations.AccessToken]);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private async Task<ClaimsPrincipal> CreateUserPrincipal(
            AppIdentityUser user,
            IEnumerable<string> scopes,
            CancellationToken cancellationToken)
        {
            var principal = await signInManager.CreateUserPrincipalAsync(user);
            var identity = (ClaimsIdentity)principal.Identity!;

            identity.SetClaim(Claims.Subject, user.Id.ToString());
            identity.SetClaim(Claims.Name, user.UserName);
            identity.SetClaim(Claims.Email, user.Email);
            identity.SetClaims(Claims.Role, (await userManager.GetRolesAsync(user)).ToImmutableArray());

            principal.SetScopes(scopes);
            principal.SetResources(await ResolveResources(principal.GetScopes(), cancellationToken));
            principal.SetDestinations(static claim => claim.Type switch
            {
                Claims.Name when claim.Subject?.HasScope(Scopes.Profile) == true =>
                    [Destinations.AccessToken, Destinations.IdentityToken],
                Claims.Email when claim.Subject?.HasScope(Scopes.Email) == true =>
                    [Destinations.AccessToken, Destinations.IdentityToken],
                Claims.Role when claim.Subject?.HasScope(Scopes.Roles) == true =>
                    [Destinations.AccessToken, Destinations.IdentityToken],
                Claims.Subject => [Destinations.AccessToken, Destinations.IdentityToken],
                _ => [Destinations.AccessToken]
            });

            return principal;
        }

        private async Task<IReadOnlyList<string>> ResolveResources(IEnumerable<string> scopes, CancellationToken cancellationToken)
        {
            return await scopeManager.ListResourcesAsync(scopes.ToImmutableArray(), cancellationToken).ToListAsync(cancellationToken);
        }

        private ForbidResult ProtocolForbid(string error, string description)
        {
            return Forbid(
                new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
                }),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}
