using Public;
using MediatR;
using Api.Dtos;
using Common.Hash;
using Api.Interfaces;
using System.Security;
using Application.Accounts.Dtos;
using Localization.Abstractions;
using Application.Accounts.Queries;
using System.Security.Cryptography;
using Application.AccountSessions.Dtos;
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

        private Dictionary<Guid, Tuple<AccountModel, AccountSessionModel>> AccountsCache = [];

        public AuthService(ISender sender, IJwtTokenService jwt, IRefreshTokenService refresh, ITextProvider textProvider)
        {
            _jwt = jwt;
            _sender = sender;
            _refresh = refresh;
            _messages = textProvider;

            _message_account_locked = _messages.Get(nameof(Messages.Account_Locked));
        }

        public async Task<LoginResponse?> Login(string username, string password, CancellationToken cancellationToken = default)
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

            var (accessToken, expiresAt) = _jwt.GenerateAccessToken(account, 1);
            var refreshToken = await _refresh.Create(session);

            await _sender.Send(updateSessionCommand, cancellationToken);

            return new LoginResponse(accessToken, expiresAt, refreshToken);
        }

        public async Task<LoginResponse?> Refresh(string refreshToken, CancellationToken cancellationToken = default)
        {
            var token = await _refresh.Validate(refreshToken);

            if (token is null)
                return null;

            AccountModel account;
            AccountSessionModel session;

            // Implement in-memory cache for accounts and sessions to avoid multiple database calls during token refresh
            // set a setting based on the average token lifespan to determine how long to cache the data
            if (AccountsCache.TryGetValue(token.AccountId, out Tuple<AccountModel, AccountSessionModel>? value))
            {
                (account, session) = value;
            }
            else
            {
                GetAccountByIdQuery query = new()
                {
                    AccountId = token.AccountId
                };

                var accountResponse = await _sender.Send(query, cancellationToken);

                GetAccountSessionByIdQuery getSessionQuery = new()
                {
                    AccountId = token.AccountId
                };

                var sessionResponse = await _sender.Send(getSessionQuery, cancellationToken);

                if (!accountResponse.Success || accountResponse.Detail == null || !sessionResponse.Success || sessionResponse.Detail == null)
                    return null;

                account = accountResponse.Detail;
                session = sessionResponse.Detail;
            }

            session.TokenVersion++;

            var (accessToken, expiresAt) = _jwt.GenerateAccessToken(account, session.TokenVersion);

            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            await _refresh.Rotate(refreshToken, newRefreshToken);

            if (!AccountsCache.ContainsKey(token.AccountId))
            {
                AccountsCache.TryAdd(token.AccountId, Tuple.Create(account, session));
            }

            return new LoginResponse(accessToken, expiresAt, newRefreshToken);
        }

        public Task Logout(string refreshToken) => _refresh.Revoke(refreshToken);

        public void ClearCache() => AccountsCache.Clear();
    }

}
