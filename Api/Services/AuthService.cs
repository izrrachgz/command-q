using MediatR;
using Api.Dtos;
using Api.Interfaces;
using System.Security;
using Application.Accounts.Queries;
using System.Security.Cryptography;

namespace Api.Services
{
    public sealed class AuthService
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutTime = TimeSpan.FromMinutes(10);

        private readonly ISender _sender;
        private readonly PasswordHasher _hasher;
        private readonly IJwtTokenService _jwt;
        private readonly IRefreshTokenService _refresh;

        public AuthService(ISender sender, PasswordHasher hasher, IJwtTokenService jwt, IRefreshTokenService refresh)
        {
            _sender = sender;
            _hasher = hasher;
            _jwt = jwt;
            _refresh = refresh;
        }

        public async Task<LoginResponse?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            GetAccountByUsernameQuery query = new()
            {
                Username = username,
                Password = password
            };

            var user = await _sender.Send(query, cancellationToken);

            if (user is null) return null;

            //TODO: implement user return type by account query
            if (user.LockedUntilUtc > DateTime.UtcNow)
                throw new SecurityException("Account locked");

            if (!_hasher.Verify(password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockedUntilUtc = DateTime.UtcNow.Add(LockoutTime);
                }

                return null;
            }

            user.FailedLoginAttempts = 0;
            user.LockedUntilUtc = null;

            var (accessToken, expiresAt) = _jwt.GenerateAccessToken(user);
            var refreshToken = await _refresh.CreateAsync(user, cancellationToken);

            return new LoginResponse(accessToken, expiresAt, refreshToken);
        }

        public async Task<LoginResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var user = await _refresh.ValidateAsync(refreshToken, cancellationToken);
            if (user is null) return null;

            user.TokenVersion++;

            var (accessToken, expiresAt) = _jwt.GenerateAccessToken(user);
            var newRefreshToken =
                Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            await _refresh.RotateAsync(refreshToken, newRefreshToken);

            return new LoginResponse(accessToken, expiresAt, newRefreshToken);
        }

        public Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
            => _refresh.RevokeAsync(refreshToken, cancellationToken);
    }

}
