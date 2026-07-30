using Api.Models;
using Api.Logging;
using Api.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using Application.AccountSessions.Dtos;

namespace Api.Services
{
    public sealed class RefreshTokenService(IOptions<JwtOptions> jwtOptions, IOptions<AuthOptions> authOptions, ILogger<RefreshTokenService> logger) : IRefreshTokenService
    {
        private readonly JwtOptions _jwtOptions = jwtOptions.Value;
        private readonly AuthOptions _authOptions = authOptions.Value;
        private static readonly ConcurrentDictionary<string, RefreshToken> Tokens = [];

        public Task<string> Create(AccountSessionModel session)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_authOptions.RefreshTokenByteSize));

            Tokens[token] = new RefreshToken
            {
                Token = token,
                AccountId = session.AccountId,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
            };

            logger.RefreshTokenCreated(session.AccountId);

            return Task.FromResult(token);
        }

        public Task<RefreshToken?> Validate(string refreshToken)
        {
            if (!Tokens.TryGetValue(refreshToken, out var token))
            {
                logger.RefreshTokenStoreMiss();
                return Task.FromResult<RefreshToken?>(null);
            }

            if (token.Revoked || token.ExpiresAt <= DateTime.UtcNow)
            {
                logger.RefreshTokenRejected(token.AccountId, token.Revoked, token.ExpiresAt <= DateTime.UtcNow);
                return Task.FromResult<RefreshToken?>(null);
            }

            return Task.FromResult<RefreshToken?>(token);
        }

        public Task Rotate(string oldToken, string newToken)
        {
            if (!Tokens.TryGetValue(oldToken, out var token))
            {
                logger.RefreshTokenRotationSkipped();
                return Task.CompletedTask;
            }

            token.Revoked = true;

            Tokens[newToken] = new RefreshToken
            {
                Token = newToken,
                AccountId = token.AccountId,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
            };

            logger.RefreshTokenRotated(token.AccountId);

            return Task.CompletedTask;
        }

        public Task Revoke(string refreshToken)
        {
            Tokens.TryGetValue(refreshToken, out var token);

            if (token != null)
            {
                token.Revoked = true;
                logger.RefreshTokenRevoked(token.AccountId);
            }

            return Task.CompletedTask;
        }
    }
}
