using Api.Models;
using Api.Interfaces;
using Api.Modules.Identity.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Application.Modules.Identity.AccountSessions.Dtos;

namespace Api.Services
{
    public sealed class RefreshTokenService(IOptions<JwtOptions> jwtOptions, IOptions<AuthOptions> authOptions, IRefreshTokenStore tokenStore, ILogger<RefreshTokenService> logger) : IRefreshTokenService
    {
        private readonly JwtOptions _jwtOptions = jwtOptions.Value;
        private readonly AuthOptions _authOptions = authOptions.Value;

        public async Task<string> Create(AccountSessionModel session)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_authOptions.RefreshTokenByteSize));

            await tokenStore.Save(new RefreshToken
            {
                Token = token,
                AccountId = session.AccountId,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
            });

            logger.RefreshTokenCreated(session.AccountId);

            return token;
        }

        public async Task<RefreshToken?> Validate(string refreshToken)
        {
            var token = await tokenStore.Get(refreshToken);

            if (token is null)
            {
                logger.RefreshTokenStoreMiss();
                return null;
            }

            if (token.Revoked || token.ExpiresAt <= DateTime.UtcNow)
            {
                logger.RefreshTokenRejected(token.AccountId, token.Revoked, token.ExpiresAt <= DateTime.UtcNow);
                return null;
            }

            return token;
        }

        public async Task Rotate(string oldToken, string newToken)
        {
            var token = await tokenStore.Get(oldToken);

            if (token is null)
            {
                logger.RefreshTokenRotationSkipped();
                return;
            }

            token.Revoked = true;
            await tokenStore.Save(token);

            await tokenStore.Save(new RefreshToken
            {
                Token = newToken,
                AccountId = token.AccountId,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
            });

            logger.RefreshTokenRotated(token.AccountId);
        }

        public async Task Revoke(string refreshToken)
        {
            var token = await tokenStore.Get(refreshToken);

            if (token != null)
            {
                token.Revoked = true;
                await tokenStore.Save(token);
                logger.RefreshTokenRevoked(token.AccountId);
            }
        }
    }
}
