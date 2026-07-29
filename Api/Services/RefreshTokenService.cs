using Api.Models;
using Api.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Application.AccountSessions.Dtos;

namespace Api.Services
{
    public sealed class RefreshTokenService(IOptions<JwtOptions> options, IOptions<AuthOptions> authOptions) : IRefreshTokenService
    {
        private readonly JwtOptions _options = options.Value;
        private readonly AuthOptions _authOptions = authOptions.Value;
        private static readonly List<RefreshToken> _tokens = [];

        public Task<string> Create(AccountSessionModel session)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_authOptions.RefreshTokenByteSize));

            _tokens.Add(new RefreshToken
            {
                Token = token,
                AccountId = session.AccountId,
                ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays)
            });

            return Task.FromResult(token);
        }

        public async Task<RefreshToken?> Validate(string refreshToken)
        {
            return _tokens.FirstOrDefault(t => t.Token == refreshToken && !t.Revoked && t.ExpiresAt > DateTime.UtcNow);
        }

        public Task Rotate(string oldToken, string newToken)
        {
            var token = _tokens.FirstOrDefault(t => t.Token == oldToken);

            if (token != null)
                token.Revoked = true;

            _tokens.Add(new RefreshToken
            {
                Token = newToken,
                AccountId = token!.AccountId,
                ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays)
            });

            return Task.CompletedTask;
        }

        public Task Revoke(string refreshToken)
        {
            var token = _tokens.FirstOrDefault(t => t.Token == refreshToken);

            if (token != null)
                token.Revoked = true;

            return Task.CompletedTask;
        }
    }
}
