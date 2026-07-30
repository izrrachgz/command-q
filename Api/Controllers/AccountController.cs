using MediatR;
using Api.Dtos;
using Api.Services;
using Api.Constants;
using Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Application.Accounts.Queries;
using Application.Accounts.Commands;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [ApiController, Route(@"[controller]")]
    public class AccountController : Controller
    {
        private readonly ISender _sender;
        private readonly int _daysToExpire;
        private readonly string _cookieName;
        private readonly bool _isProduction;
        private readonly AuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ISender sender, IConfiguration configuration, IHostEnvironment environment, AuthService authService, ILogger<AccountController> logger)
        {
            _sender = sender;
            _logger = logger;
            _authService = authService;
            _isProduction = environment.IsProduction();
            _cookieName = configuration.GetValue<string>(ConfigKeys.Cookie.Name)!;
            _daysToExpire = configuration.GetValue<int>(ConfigKeys.Cookie.DaysToExpire);
        }

        /// <summary>
        /// Authenticates a user with the provided credentials and returns an authentication result if successful.
        /// </summary>
        /// <param name="request">The login request containing the user's credentials. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the login operation.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the authentication attempt. Returns 200 OK with
        /// authentication details if successful; otherwise, returns 401 Unauthorized.</returns>
        [HttpPost(@"sign-in"), EnableRateLimiting(ConfigKeys.RateLimiting.FixedPolicyName)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _authService.Login(request.Username, request.Password, cancellationToken);

            if (result is null)
            {
                _logger.LogWarning(@"Sign-in returned unauthorized for username {Username}.", request.Username);
                return Unauthorized();
            }

            // Set the refresh token in an HTTP-only cookie to enhance security by preventing client-side scripts from accessing it.
            Response.Cookies.Append(_cookieName, result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(_daysToExpire)
            });

            return Ok(new
            {
                accessToken = result.AccessToken
            });
        }

        /// <summary>
        /// Generates a new access token using the provided refresh token.
        /// </summary>
        /// <param name="request">The refresh request containing the refresh token to use for generating a new access token. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the login operation.</param>
        /// <returns>An <see cref="IActionResult"/> that contains the new access token if the refresh token is valid; otherwise,
        /// an unauthorized result.</returns>
        [HttpPost(@"refresh")]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken = default)
        {
            // read the refresh token from the HTTP-only cookie set during login.
            // this approach enhances security by preventing client-side scripts from accessing the token.
            var refreshToken = Request.Cookies[_cookieName];

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning(@"Refresh requested without refresh cookie.");
                return Unauthorized();
            }

            var result = await _authService.Refresh(refreshToken, cancellationToken);

            if (result is null)
            {
                _logger.LogWarning(@"Refresh returned unauthorized.");
                return Unauthorized();
            }

            // update the refresh token in the HTTP-only cookie with the new token received from the authentication service.
            // this ensures that the client has the latest refresh token for future access token refresh operations.
            Response.Cookies.Append(_cookieName, result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(_daysToExpire)
            });

            return Ok(new
            {
                accessToken = result.AccessToken
            });
        }

        /// <summary>
        /// Logs out the user by invalidating the specified refresh token.
        /// </summary>
        /// <param name="request">The logout request containing the refresh token to be invalidated. Cannot be null.</param>
        /// <returns>A 204 No Content response if the logout operation succeeds.</returns>
        [HttpPost(@"logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var refreshToken = Request.Cookies[_cookieName];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.Logout(refreshToken);
            }
            else
            {
                _logger.LogDebug(@"Logout requested without refresh cookie.");
            }

            // delete cookie
            Response.Cookies.Delete(_cookieName);

            return NoContent();
        }

        /// <summary>
        /// Creates a new account using the specified account creation command.
        /// </summary>
        /// <remarks>This action is invoked via an HTTP POST request to the 'create' endpoint. The
        /// operation is performed asynchronously and returns appropriate HTTP status codes based on the outcome of the
        /// account creation process.</remarks>
        /// <param name="request">The command containing the details required to create a new account. Cannot be null.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the account creation operation. Returns a
        /// BadRequest result if the creation fails, or an Ok result with the response data if successful.</returns>
        [HttpPost(@"create"), EnableRateLimiting(ConfigKeys.RateLimiting.FixedPolicyName)]
        public async Task<IActionResult> Create([FromBody] CreateAccountCommand request, CancellationToken cancellationToken = default)
        {
            var response = await _sender.Send(request, cancellationToken);

            return !response.Success ? BadRequest(response.Message) : Ok(response);
        }

        /// <summary>
        /// Gets the authenticated account information.
        /// </summary>
        [Authorize, HttpGet(@"me")]
        public async Task<IActionResult> Me(CancellationToken cancellationToken = default)
        {
            if (!User.TryGetAccountId(out var accountId))
                return Unauthorized();

            var query = new GetAccountByIdQuery
            {
                AccountId = accountId
            };

            var response = await _sender.Send(query, cancellationToken);

            return response.Success ? Ok(response) : NotFound(response);
        }
    }
}
