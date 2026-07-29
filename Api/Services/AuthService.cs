using Public;
using MediatR;
using Api.Dtos;
using Api.Models;
using Common.Hash;
using Api.Interfaces;
using System.Security;
using Application.Accounts.Dtos;
using Localization.Abstractions;
using Application.Accounts.Queries;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Application.AccountSessions.Dtos;
using Application.AccountSessions.Queries;
using Application.AccountSessions.Commands;

namespace Api.Services
{
    public sealed class AuthService
    {
        private readonly ISender _sender;
        private readonly IJwtTokenService _jwt;
        private readonly ITextProvider _messages;
        private readonly AuthOptions _authOptions;
        private readonly IRefreshTokenService _refresh;
        private readonly IAccountSessionCache _accountSessionCache;

        private readonly string _message_account_locked;

        public AuthService(ISender sender, IJwtTokenService jwt, IRefreshTokenService refresh, IAccountSessionCache accountSessionCache, ITextProvider textProvider, IOptions<AuthOptions> authOptions)
        {
            _jwt = jwt;
            _sender = sender;
            _refresh = refresh;
            _messages = textProvider;
            _authOptions = authOptions.Value;
            _accountSessionCache = accountSessionCache;

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
                MaxFailedAttempts = _authOptions.MaxFailedAttempts
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
            session.AccountId = account.Id;

            UpdateAccountSessionCommand updateSessionCommand = new()
            {
                AccountId = account.Id,
            };

            password = Hash.ComputeSHA256(password);

            if (!Hash.CompareSHA256(password, account.Password))
            {
                session.FailedLoginAttempts++;

                if (session.FailedLoginAttempts >= _authOptions.MaxFailedAttempts)
                    session.LockedUntilUtc = DateTime.UtcNow.AddMinutes(_authOptions.LockoutMinutes);

                updateSessionCommand.FailedLoginAttempts = session.FailedLoginAttempts;
                updateSessionCommand.LockedUntilUtc = session.LockedUntilUtc;

                await _sender.Send(updateSessionCommand, cancellationToken);

                return null;
            }

            updateSessionCommand.FailedLoginAttempts = 0;
            updateSessionCommand.LockedUntilUtc = null;

            session.Roles = account.Roles;
            session.TokenVersion = 1;
            updateSessionCommand.TokenVersion = session.TokenVersion;

            var (accessToken, expiresAt) = _jwt.GenerateAccessToken(account, session.TokenVersion);
            var refreshToken = await _refresh.Create(session);

            await _sender.Send(updateSessionCommand, cancellationToken);

            _accountSessionCache.Set(account.Id, account, session);

            return new LoginResponse(accessToken, expiresAt, refreshToken);
        }

        public async Task<LoginResponse?> Refresh(string refreshToken, CancellationToken cancellationToken = default)
        {
            var token = await _refresh.Validate(refreshToken);

            if (token is null)
                return null;

            AccountModel account;
            AccountSessionModel session;

            if (_accountSessionCache.TryGet(token.AccountId, out var cached))
            {
                account = cached.Account;
                session = cached.Session;
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

                _accountSessionCache.Set(token.AccountId, account, session);
            }

            session.TokenVersion++;

            var (accessToken, expiresAt) = _jwt.GenerateAccessToken(account, session.TokenVersion);

            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_authOptions.RefreshTokenByteSize));

            await _refresh.Rotate(refreshToken, newRefreshToken);

            _accountSessionCache.Set(token.AccountId, account, session);

            return new LoginResponse(accessToken, expiresAt, newRefreshToken);
        }

        public async Task Logout(string refreshToken)
        {
            var token = await _refresh.Validate(refreshToken);

            await _refresh.Revoke(refreshToken);

            if (token is not null)
                _accountSessionCache.Remove(token.AccountId);
        }

    }

}
