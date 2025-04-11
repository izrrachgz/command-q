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
        private string TokenIssuer { get; }
        private string TokenAudience { get; }
        private int TokenMinutesToExpire { get; }

        public AccountController(ISender sender, IConfiguration configuration)
        {
            _sender = sender;
            _configuration = configuration;
            //TODO: put this into an instance and get it through DI
            TokenSecretKey = _configuration.GetValue<string>(SearchConfiguration.JwtSecretKey) ?? string.Empty;
            TokenIssuer = _configuration.GetValue<string>(SearchConfiguration.JwtIssuer) ?? string.Empty;
            TokenAudience = _configuration.GetValue<string>(SearchConfiguration.JwtAudience) ?? string.Empty;
            TokenMinutesToExpire = configuration.GetValue<int>(SearchConfiguration.JwtMinutesToExpire);
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
                issuer: TokenIssuer,
                audience: TokenAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(TokenMinutesToExpire),
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
