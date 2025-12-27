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

        [HttpPost("sign-in")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request.Username, request.Password, cancellationToken);

            return result is null ? Unauthorized() : Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var result = await _authService.RefreshAsync(request.RefreshToken);

            return result is null ? Unauthorized() : Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _authService.LogoutAsync(request.RefreshToken);

            return NoContent();
        }

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
