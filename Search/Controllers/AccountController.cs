using MediatR;
using System.Text;
using Search.Constants;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Application.Accounts.Queries;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Search.Controllers
{
    [ApiController, Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly ISender _sender;
        private readonly IConfiguration _configuration;
        private string TokenSecretKey { get; }

        public AccountController(ISender sender, IConfiguration configuration)
        {
            _sender = sender;
            _configuration = configuration;
            TokenSecretKey = _configuration.GetValue<string>(SearchConfiguration.JwtSecretKey) ?? string.Empty;
        }

        private JwtSecurityToken GenerateToken(string username)
        {
            var claims = new Claim[2]{
                new (ClaimTypes.Name, username),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenSecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return token;
        }

        [HttpPost(@"sign-in")]
        public async Task<IActionResult> Login([FromBody] GetAccountByUsernameQuery request, CancellationToken cancellationToken = default)
        {
            var authorized = await _sender.Send(request, cancellationToken);

            if (!authorized.Success || authorized.Detail is null)
            {
                return Forbid();
            }

            var token = GenerateToken(request.Username);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
    }
}
