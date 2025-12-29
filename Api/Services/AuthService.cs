using Public;
using MediatR;
using Api.Dtos;
using Common.Hash;
using Api.Interfaces;
using System.Security;
using Localization.Abstractions;
using Application.Accounts.Queries;
using System.Security.Cryptography;
using Application.AccountSessions.Queries;
using Application.AccountSessions.Commands;

namespace Api.Services
{
    public sealed class AuthService
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutTime = TimeSpan.FromMinutes(10);

        private readonly ISender _sender;
        private readonly IJwtTokenService _jwt;
        private readonly ITextProvider _messages;
        private readonly IRefreshTokenService _refresh;

        private readonly string _message_account_locked;

        public AuthService(ISender sender, IJwtTokenService jwt, IRefreshTokenService refresh, ITextProvider textProvider)
        {
            _jwt = jwt;
            _sender = sender;
            _refresh = refresh;
            _messages = textProvider;

            _message_account_locked = _messages.Get(nameof(Messages.Account_Locked));
        }

        public async Task<LoginResponse?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            GetAccountByUsernameQuery getAccounQuery = new()
            {
                Username = username,
                Password = password
            };

            var response = await _sender.Send(getAccounQuery, cancellationToken);

            if (!response.Success || response.Detail == null)
                return null;

            var account = response.Detail;

            CheckIfAccountSessionIsLockedByIdQuery checkLockedSessionQuery = new()
            {
                AccountId = account.Id,
                MaxFailedAttempts = MaxFailedAttempts
            };

            var accountIsLocked = await _sender.Send(checkLockedSessionQuery, cancellationToken);

            if (accountIsLocked.Success)
                throw new SecurityException(_message_account_locked);

            GetAccountSessionByIdQuery getSessionQuery = new()
            {
                AccountId = account.Id
            };

            var sessionResponse = await _sender.Send(getSessionQuery, cancellationToken);

            var session = sessionResponse.Detail ?? new();

            UpdateAccountSessionCommand updateSessionCommand = new()
            {
                AccountId = account.Id,
            };

            password = Hash.ComputeSHA256(password);

            if (!Hash.CompareSHA256(password, account.Password))
            {
                session.FailedLoginAttempts++;

                if (session.FailedLoginAttempts >= MaxFailedAttempts)
                    session.LockedUntilUtc = DateTime.UtcNow.Add(LockoutTime);

                updateSessionCommand.FailedLoginAttempts = session.FailedLoginAttempts;
                updateSessionCommand.LockedUntilUtc = session.LockedUntilUtc;

                await _sender.Send(updateSessionCommand, cancellationToken);

                return null;
            }

            updateSessionCommand.FailedLoginAttempts = 0;
            updateSessionCommand.LockedUntilUtc = null;

            session.Roles = account.Roles;

            var (accessToken, expiresAt) = _jwt.GenerateAccessToken(session);
            var refreshToken = await _refresh.CreateAsync(session);

            await _sender.Send(updateSessionCommand, cancellationToken);

            return new LoginResponse(accessToken, expiresAt, refreshToken);
        }

        public async Task<LoginResponse?> RefreshAsync(string refreshToken)
        {
            var user = await _refresh.ValidateAsync(refreshToken);
            if (user is null) return null;

            user.TokenVersion++;

            var (accessToken, expiresAt) = _jwt.GenerateAccessToken(user);

            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            await _refresh.RotateAsync(refreshToken, newRefreshToken);

            return new LoginResponse(accessToken, expiresAt, newRefreshToken);
        }

        public Task LogoutAsync(string refreshToken) => _refresh.RevokeAsync(refreshToken);
    }

}
