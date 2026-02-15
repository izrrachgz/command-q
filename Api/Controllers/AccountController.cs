using MediatR;
using Api.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Application.Accounts.Commands;

namespace Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly ISender _sender;
        private readonly AuthService _authService;
        private readonly IConfiguration _configuration;

        public AccountController(ISender sender, IConfiguration configuration, AuthService authService)
        {
            _sender = sender;
            _authService = authService;
            _configuration = configuration;
        }

        /// <summary>
        /// Authenticates a user with the provided credentials and returns an authentication result if successful.
        /// </summary>
        /// <param name="request">The login request containing the user's credentials. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the login operation.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the authentication attempt. Returns 200 OK with
        /// authentication details if successful; otherwise, returns 401 Unauthorized.</returns>
        [HttpPost(@"sign-in")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request.Username, request.Password, cancellationToken);

            return result is null ? Unauthorized() : Ok(result);
        }

        /// <summary>
        /// Generates a new access token using the provided refresh token.
        /// </summary>
        /// <param name="request">The refresh request containing the refresh token to use for generating a new access token. Cannot be null.</param>
        /// <returns>An <see cref="IActionResult"/> that contains the new access token if the refresh token is valid; otherwise,
        /// an unauthorized result.</returns>
        [HttpPost(@"refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var result = await _authService.RefreshAsync(request.RefreshToken);

            return result is null ? Unauthorized() : Ok(result);
        }

        /// <summary>
        /// Logs out the user by invalidating the specified refresh token.
        /// </summary>
        /// <param name="request">The logout request containing the refresh token to be invalidated. Cannot be null.</param>
        /// <returns>A 204 No Content response if the logout operation succeeds.</returns>
        [HttpPost(@"logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _authService.LogoutAsync(request.RefreshToken);

            return NoContent();
        }

        /// <summary>
        /// Creates a new account using the specified account creation command.
        /// </summary>
        /// <param name="request">The account creation command containing the details required to create a new account. Cannot be null.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the account creation operation. Returns a 200
        /// OK response with the result if successful; otherwise, returns a 400 Bad Request with an error message.</returns>
        [HttpPost(@"create")]
        public async Task<IActionResult> Create([FromBody] CreateAccountCommand request)
        {
            var response = await _sender.Send(request);

            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

            return Ok(response);
        }
    }
}
