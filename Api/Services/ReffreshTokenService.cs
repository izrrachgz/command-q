using Api.Models;
using Api.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Api.Services
{
    public sealed class RefreshTokenService : IRefreshTokenService
    {
        private static readonly List<RefreshToken> _tokens = new();
        private readonly JwtOptions _options;

        public RefreshTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public Task<string> CreateAsync(User user, CancellationToken ct)
        {
            var token = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));

            _tokens.Add(new RefreshToken
            {
                Token = token,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays)
            });

            return Task.FromResult(token);
        }

        public Task<User?> ValidateAsync(string refreshToken, CancellationToken ct)
        {
            var token = _tokens.FirstOrDefault(t =>
                t.Token == refreshToken &&
                !t.Revoked &&
                t.ExpiresAt > DateTime.UtcNow);

            if (token is null)
                return Task.FromResult<User?>(null);

            // You would fetch user from repo here
            return Task.FromResult<User?>(null);
        }

        public Task RotateAsync(string oldToken, string newToken)
        {
            var token = _tokens.FirstOrDefault(t => t.Token == oldToken);
            if (token != null)
                token.Revoked = true;

            _tokens.Add(new RefreshToken
            {
                Token = newToken,
                UserId = token!.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays)
            });

            return Task.CompletedTask;
        }

        public Task RevokeAsync(string refreshToken, CancellationToken ct)
        {
            var token = _tokens.FirstOrDefault(t => t.Token == refreshToken);
            if (token != null)
                token.Revoked = true;

            return Task.CompletedTask;
        }
    }
}
