using MediatR;
using Api.Models;
using Api.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Application.AccountSessions.Dtos;
using Application.AccountSessions.Queries;

namespace Api.Services
{
    public sealed class RefreshTokenService : IRefreshTokenService
    {
        private readonly ISender _sender;
        private readonly JwtOptions _options;
        private static readonly List<RefreshToken> _tokens = [];

        public RefreshTokenService(IOptions<JwtOptions> options, ISender sender)
        {
            _sender = sender;
            _options = options.Value;
        }

        public Task<string> CreateAsync(AccountSessionModel session)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            _tokens.Add(new RefreshToken
            {
                Token = token,
                AccountId = session.AccountId,
                ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays)
            });

            return Task.FromResult(token);
        }

        public async Task<AccountSessionModel?> ValidateAsync(string refreshToken)
        {
            var token = _tokens.FirstOrDefault(t => t.Token == refreshToken && !t.Revoked && t.ExpiresAt > DateTime.UtcNow);

            if (token is null)
                return null;

            //This could be cached for better performance, but for simplicity, we will query the database every time
            GetAccountSessionByIdQuery query = new()
            {
                AccountId = token.AccountId
            };

            var sessionResponse = await _sender.Send(query);

            return sessionResponse.Detail;
        }

        public Task RotateAsync(string oldToken, string newToken)
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

        public Task RevokeAsync(string refreshToken)
        {
            var token = _tokens.FirstOrDefault(t => t.Token == refreshToken);

            if (token != null)
                token.Revoked = true;

            return Task.CompletedTask;
        }
    }
}
