using Api.Models;
using Api.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using Application.AccountSessions.Dtos;

namespace Api.Services
{
    public sealed class RefreshTokenService(IOptions<JwtOptions> options, IOptions<AuthOptions> authOptions) : IRefreshTokenService
    {
        private readonly JwtOptions _options = options.Value;
        private readonly AuthOptions _authOptions = authOptions.Value;
        private static readonly ConcurrentDictionary<string, RefreshToken> Tokens = [];

        public Task<string> Create(AccountSessionModel session)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_authOptions.RefreshTokenByteSize));

            Tokens[token] = new RefreshToken
            {
                Token = token,
                AccountId = session.AccountId,
                ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays)
            };

            return Task.FromResult(token);
        }

        public Task<RefreshToken?> Validate(string refreshToken)
        {
            if (!Tokens.TryGetValue(refreshToken, out var token))
                return Task.FromResult<RefreshToken?>(null);

            if (token.Revoked || token.ExpiresAt <= DateTime.UtcNow)
                return Task.FromResult<RefreshToken?>(null);

            return Task.FromResult<RefreshToken?>(token);
        }

        public Task Rotate(string oldToken, string newToken)
        {
            if (!Tokens.TryGetValue(oldToken, out var token))
                return Task.CompletedTask;

            token.Revoked = true;

            Tokens[newToken] = new RefreshToken
            {
                Token = newToken,
                AccountId = token.AccountId,
                ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays)
            };

            return Task.CompletedTask;
        }

        public Task Revoke(string refreshToken)
        {
            Tokens.TryGetValue(refreshToken, out var token);

            if (token != null)
                token.Revoked = true;

            return Task.CompletedTask;
        }
    }
}
