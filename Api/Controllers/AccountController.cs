using MediatR;
using Api.Dtos;
using Api.Services;
using System.Security;
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

        [HttpPost("sign-in")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.LoginAsync(request.Username, request.Password, cancellationToken);

                return result is null ? Unauthorized() : Ok(result);
            }
            catch (SecurityException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _authService.RefreshAsync(request.RefreshToken, cancellationToken);

            return result is null ? Unauthorized() : Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken = default)
        {
            await _authService.LogoutAsync(request.RefreshToken, cancellationToken);

            return NoContent();
        }

        [HttpPost(@"create")]
        public async Task<IActionResult> Create([FromBody] CreateAccountCommand request, CancellationToken cancellationToken = default)
        {
            var response = await _sender.Send(request, cancellationToken);

            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

            return Ok(response);
        }
    }
}
