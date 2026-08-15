using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using IdentityServer.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using IdentityServer.Modules.Accounts.Logging;

namespace IdentityServer.Modules.Accounts
{
    [ApiController, Route(AccountConstants.Routes.Root)]
    public sealed class AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IdentityServerDbContext database,
        IIdentitySecurityEventRecorder securityEvents,
        ISignInPageRenderer signInPageRenderer,
        ILogger<AccountController> logger) : ControllerBase
    {
        [AllowAnonymous, HttpPost(AccountConstants.Routes.Register)]
        [EnableRateLimiting(AccountConstants.AuthenticationRateLimitPolicy)]
        public async Task<IActionResult> Register(RegisterAccountRequest request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var user = new IdentityUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Username.Trim(),
                Email = request.Email.Trim(),
                CreatedUtc = now,
                ModifiedUtc = now
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                logger.AccountRegistrationFailed(request.Username);
                return BadRequest(new ValidationProblemDetails(ToValidationErrors(result)));
            }

            var roleResult = await userManager.AddToRoleAsync(user, AccountConstants.DefaultRole);

            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                logger.AccountRegistrationFailed(request.Username);
                return BadRequest(new ValidationProblemDetails(ToValidationErrors(roleResult)));
            }

            logger.AccountRegistered(user.Id);
            await securityEvents.Record(AccountConstants.Actions.Registered, user.Id, cancellationToken: cancellationToken);

            return CreatedAtAction(nameof(Me), new AccountResponse(user.Id, user.UserName, user.Email));
        }

        [AllowAnonymous, HttpGet(AccountConstants.Routes.SignIn)]
        public IActionResult SignInPage([FromQuery] string returnUrl = AccountConstants.DefaultReturnUrl)
        {
            if (!IsSafeReturnUrl(returnUrl))
            {
                return BadRequest();
            }

            return User.Identity?.IsAuthenticated == true
                ? LocalRedirect(returnUrl)
                : Content(signInPageRenderer.Render(returnUrl), AccountConstants.ContentTypes.Html);
        }

        [AllowAnonymous, HttpPost(AccountConstants.Routes.SignIn)]
        [Consumes(AccountConstants.ContentTypes.Json)]
        [EnableRateLimiting(AccountConstants.AuthenticationRateLimitPolicy)]
        public async Task<IActionResult> SignIn(SignInRequest request, CancellationToken cancellationToken)
        {
            var result = await PasswordSignIn(request, cancellationToken);

            return result.Succeeded ? NoContent() : Unauthorized();
        }

        [AllowAnonymous, HttpPost(AccountConstants.Routes.SignIn)]
        [Consumes(AccountConstants.ContentTypes.FormUrlEncoded)]
        [EnableRateLimiting(AccountConstants.AuthenticationRateLimitPolicy)]
        public async Task<IActionResult> SignInForm(
            [FromForm] SignInRequest request,
            [FromQuery] string returnUrl = AccountConstants.DefaultReturnUrl,
            CancellationToken cancellationToken = default)
        {
            if (!IsSafeReturnUrl(returnUrl))
            {
                return BadRequest();
            }

            var result = await PasswordSignIn(request, cancellationToken);

            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Content(signInPageRenderer.Render(returnUrl, failed: true, lockedOut: result.LockedOut), AccountConstants.ContentTypes.Html);
        }

        private async Task<PasswordSignInResult> PasswordSignIn(SignInRequest request, CancellationToken cancellationToken)
        {
            var user = await FindUser(request.UsernameOrEmail);

            if (user is null)
            {
                logger.LoginFailed(request.UsernameOrEmail);
                await securityEvents.Record(AccountConstants.Actions.LoginFailed, cancellationToken: cancellationToken);
                return new PasswordSignInResult(Succeeded: false, LockedOut: false);
            }

            var result = await signInManager.PasswordSignInAsync(
                user,
                request.Password,
                request.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                logger.LoginSucceeded(user.Id);
                await securityEvents.Record(AccountConstants.Actions.LoginSucceeded, user.Id, cancellationToken: cancellationToken);
                return new PasswordSignInResult(Succeeded: true, LockedOut: false);
            }

            if (result.IsLockedOut)
            {
                logger.LoginLockedOut(user.Id);
                await securityEvents.Record(AccountConstants.Actions.LoginLockedOut, user.Id, cancellationToken: cancellationToken);
                return new PasswordSignInResult(Succeeded: false, LockedOut: true);
            }

            logger.LoginFailed(request.UsernameOrEmail);
            await securityEvents.Record(AccountConstants.Actions.LoginFailed, user.Id, cancellationToken: cancellationToken);

            return new PasswordSignInResult(Succeeded: false, LockedOut: false);
        }

        [Authorize, HttpPost(AccountConstants.Routes.SignOut)]
        public async Task<IActionResult> SignOutCurrentUser(CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);

            await signInManager.SignOutAsync();

            if (user is not null)
            {
                logger.AccountSignedOut(user.Id);
                await securityEvents.Record(AccountConstants.Actions.Logout, user.Id, cancellationToken: cancellationToken);
            }

            return NoContent();
        }

        [Authorize, HttpGet(AccountConstants.Routes.Me)]
        public async Task<IActionResult> Me()
        {
            var user = await userManager.GetUserAsync(User);

            return user is null
                ? Unauthorized()
                : Ok(new AccountResponse(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty));
        }

        [Authorize, HttpGet(AccountConstants.Routes.SecurityActivity)]
        public async Task<IActionResult> SecurityActivity(CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);

            if (user is null)
            {
                return Unauthorized();
            }

            var activity = await database.SecurityEvents
                .AsNoTracking()
                .Where(value => value.UserId == user.Id)
                .OrderByDescending(value => value.OccurredUtc)
                .Take(AccountConstants.SecurityActivityLimit)
                .Select(value => new SecurityEventResponse(
                    value.Id,
                    value.Action,
                    value.ClientId,
                    value.IpAddress,
                    value.UserAgent,
                    value.DetailsJson,
                    value.OccurredUtc))
                .ToListAsync(cancellationToken);

            return Ok(activity);
        }

        [AllowAnonymous, HttpGet(AccountConstants.Routes.External)]
        [EnableRateLimiting(AccountConstants.AuthenticationRateLimitPolicy)]
        public async Task<IActionResult> External(string provider, [FromQuery] string returnUrl = AccountConstants.DefaultReturnUrl)
        {
            var scheme = (await signInManager.GetExternalAuthenticationSchemesAsync())
                .SingleOrDefault(value => string.Equals(value.Name, provider, StringComparison.Ordinal));

            if (scheme is null || !IsSafeReturnUrl(returnUrl))
            {
                return BadRequest();
            }

            var callback = Url.Action(nameof(ExternalCallback), values: new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callback);

            return Challenge(properties, provider);
        }

        [Authorize, HttpGet(AccountConstants.Routes.ExternalLink)]
        [EnableRateLimiting(AccountConstants.AuthenticationRateLimitPolicy)]
        public async Task<IActionResult> LinkExternal(string provider, [FromQuery] string returnUrl = AccountConstants.DefaultReturnUrl)
        {
            var user = await userManager.GetUserAsync(User);
            var scheme = (await signInManager.GetExternalAuthenticationSchemesAsync())
                .SingleOrDefault(value => string.Equals(value.Name, provider, StringComparison.Ordinal));

            if (user is null || scheme is null || !IsSafeReturnUrl(returnUrl))
            {
                return BadRequest();
            }

            var callback = Url.Action(nameof(LinkExternalCallback), values: new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(
                provider,
                callback,
                user.Id.ToString());

            return Challenge(properties, provider);
        }

        [Authorize, HttpGet(AccountConstants.Routes.ExternalLinkCallback)]
        public async Task<IActionResult> LinkExternalCallback(
            [FromQuery] string returnUrl = AccountConstants.DefaultReturnUrl,
            CancellationToken cancellationToken = default)
        {
            var user = await userManager.GetUserAsync(User);

            if (user is null || !IsSafeReturnUrl(returnUrl))
            {
                return BadRequest();
            }

            var login = await signInManager.GetExternalLoginInfoAsync(user.Id.ToString());

            if (login is null)
            {
                return Unauthorized();
            }

            var result = await userManager.AddLoginAsync(user, login);

            if (!result.Succeeded)
            {
                return BadRequest(new ValidationProblemDetails(ToValidationErrors(result)));
            }

            await securityEvents.Record(
                AccountConstants.Actions.ExternalAccountLinked,
                user.Id,
                cancellationToken: cancellationToken);

            return LocalRedirect(returnUrl);
        }

        [AllowAnonymous, HttpGet(AccountConstants.Routes.ExternalCallback)]
        public async Task<IActionResult> ExternalCallback(
            [FromQuery] string returnUrl = AccountConstants.DefaultReturnUrl,
            [FromQuery] string? remoteError = null,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(remoteError) || !IsSafeReturnUrl(returnUrl))
            {
                return BadRequest();
            }

            var login = await signInManager.GetExternalLoginInfoAsync();

            if (login is null)
            {
                return Unauthorized();
            }

            var result = await signInManager.ExternalLoginSignInAsync(
                login.LoginProvider,
                login.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: false);

            if (result.Succeeded)
            {
                var existingUser = await userManager.FindByLoginAsync(login.LoginProvider, login.ProviderKey);

                if (existingUser is not null)
                {
                    logger.ExternalLoginSucceeded(existingUser.Id, login.LoginProvider);
                    await securityEvents.Record(
                        AccountConstants.Actions.ExternalLoginSucceeded,
                        existingUser.Id,
                        cancellationToken: cancellationToken);
                }

                return LocalRedirect(returnUrl);
            }

            var email = login.Principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email) || await userManager.FindByEmailAsync(email) is not null)
            {
                logger.ExternalLoginFailed(login.LoginProvider);
                return Conflict();
            }

            var now = DateTime.UtcNow;
            var user = new IdentityUser
            {
                Id = Guid.NewGuid(),
                UserName = await CreateUniqueUsername(email),
                Email = email,
                EmailConfirmed = IsEmailVerified(login.Principal),
                CreatedUtc = now,
                ModifiedUtc = now
            };

            var createResult = await userManager.CreateAsync(user);

            if (!createResult.Succeeded)
            {
                return BadRequest(new ValidationProblemDetails(ToValidationErrors(createResult)));
            }

            var addLoginResult = await userManager.AddLoginAsync(user, login);
            var addRoleResult = await userManager.AddToRoleAsync(user, AccountConstants.DefaultRole);

            if (!addLoginResult.Succeeded || !addRoleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return BadRequest(new ValidationProblemDetails(ToValidationErrors(!addLoginResult.Succeeded ? addLoginResult : addRoleResult)));
            }

            if (!await signInManager.CanSignInAsync(user))
            {
                return Unauthorized();
            }

            await signInManager.SignInAsync(user, isPersistent: false, login.LoginProvider);
            logger.ExternalLoginSucceeded(user.Id, login.LoginProvider);
            await securityEvents.Record(
                AccountConstants.Actions.ExternalAccountCreated,
                user.Id,
                cancellationToken: cancellationToken);

            return LocalRedirect(returnUrl);
        }

        private async Task<IdentityUser?> FindUser(string usernameOrEmail)
        {
            var value = usernameOrEmail.Trim();

            return value.Contains('@', StringComparison.Ordinal)
                ? await userManager.FindByEmailAsync(value)
                : await userManager.FindByNameAsync(value);
        }

        private async Task<string> CreateUniqueUsername(string email)
        {
            var baseName = email.Split('@', 2)[0];
            var candidate = baseName;

            while (await userManager.FindByNameAsync(candidate) is not null)
            {
                candidate = $"{baseName}-{Guid.NewGuid():N}";
            }

            return candidate;
        }

        private bool IsSafeReturnUrl(string returnUrl)
        {
            return Url.IsLocalUrl(returnUrl);
        }

        private static bool IsEmailVerified(ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst("email_verified")?.Value;
            return bool.TryParse(claim, out var verified) && verified;
        }

        private sealed record PasswordSignInResult(bool Succeeded, bool LockedOut);

        private static Dictionary<string, string[]> ToValidationErrors(IdentityResult result)
        {
            return result.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.Description).ToArray());
        }
    }
}
