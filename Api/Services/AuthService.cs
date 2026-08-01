using Public;
using MediatR;
using Api.Dtos;
using Api.Models;
using Common.Hash;
using Api.Interfaces;
using System.Security;
using Localization.Abstractions;
using Api.Modules.Identity.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Application.Modules.Identity.Accounts.Dtos;
using Application.Modules.Identity.Accounts.Queries;
using Application.Modules.Identity.AccountSessions.Dtos;
using Application.Modules.Identity.AccountSessions.Queries;
using Application.Modules.Identity.AccountSessions.Commands;

namespace Api.Services
{
    public sealed class AuthService
    {
        private readonly ISender _mediator;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ITextProvider _messages;
        private readonly AuthOptions _authOptions;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAccountSessionCache _accountSessionCache;
        private readonly ILogger<AuthService> _logger;

        private readonly string _message_account_locked;

        public AuthService(ISender mediator, IJwtTokenService jwtTokenService, IRefreshTokenService refreshTokenService, IAccountSessionCache accountSessionCache, ITextProvider textProvider, IOptions<AuthOptions> authOptions, ILogger<AuthService> logger)
        {
            _jwtTokenService = jwtTokenService;
            _mediator = mediator;
            _refreshTokenService = refreshTokenService;
            _messages = textProvider;
            _authOptions = authOptions.Value;
            _logger = logger;
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

            var response = await _mediator.Send(getAccounQuery, cancellationToken);

            if (!response.Success || response.Detail == null)
            {
                _logger.LoginFailedForUsername(username);
                return null;
            }

            var account = response.Detail;

            CheckIfAccountSessionIsLockedByIdQuery checkLockedSessionQuery = new()
            {
                AccountId = account.Id,
                MaxFailedAttempts = _authOptions.MaxFailedAttempts
            };

            var accountIsLocked = await _mediator.Send(checkLockedSessionQuery, cancellationToken);

            if (accountIsLocked.Success)
            {
                _logger.LoginBlockedForLockedAccount(account.Id);
                throw new SecurityException(_message_account_locked);
            }

            GetAccountSessionByIdQuery getSessionQuery = new()
            {
                AccountId = account.Id
            };

            var sessionResponse = await _mediator.Send(getSessionQuery, cancellationToken);

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
                {
                    session.LockedUntilUtc = DateTime.UtcNow.AddMinutes(_authOptions.LockoutMinutes);
                    _logger.AccountLocked(account.Id, session.LockedUntilUtc);
                }

                updateSessionCommand.FailedLoginAttempts = session.FailedLoginAttempts;
                updateSessionCommand.LockedUntilUtc = session.LockedUntilUtc;

                await _mediator.Send(updateSessionCommand, cancellationToken);

                _logger.LoginFailedForAccount(account.Id, session.FailedLoginAttempts);

                return null;
            }

            updateSessionCommand.FailedLoginAttempts = 0;
            updateSessionCommand.LockedUntilUtc = null;

            session.Roles = account.Roles;
            session.TokenVersion = 1;
            updateSessionCommand.TokenVersion = session.TokenVersion;

            var (accessToken, expiresAt) = _jwtTokenService.GenerateAccessToken(account, session.TokenVersion);
            var refreshToken = await _refreshTokenService.Create(session);

            await _mediator.Send(updateSessionCommand, cancellationToken);

            _accountSessionCache.Set(account.Id, account, session);

            _logger.LoginSucceeded(account.Id);

            return new LoginResponse(accessToken, expiresAt, refreshToken);
        }

        public async Task<LoginResponse?> Refresh(string refreshToken, CancellationToken cancellationToken = default)
        {
            var token = await _refreshTokenService.Validate(refreshToken);

            if (token is null)
            {
                _logger.RefreshTokenValidationFailed();
                return null;
            }

            AccountModel account;
            AccountSessionModel session;

            if (_accountSessionCache.TryGet(token.AccountId, out var cached))
            {
                _logger.RefreshCacheHit(token.AccountId);
                account = cached.Account;
                session = cached.Session;
            }
            else
            {
                _logger.RefreshCacheMiss(token.AccountId);

                GetAccountByIdQuery query = new()
                {
                    AccountId = token.AccountId
                };

                var accountResponse = await _mediator.Send(query, cancellationToken);

                GetAccountSessionByIdQuery getSessionQuery = new()
                {
                    AccountId = token.AccountId
                };

                var sessionResponse = await _mediator.Send(getSessionQuery, cancellationToken);

                if (!accountResponse.Success || accountResponse.Detail == null || !sessionResponse.Success || sessionResponse.Detail == null)
                {
                    _logger.RefreshAccountSessionNotFound(token.AccountId);
                    return null;
                }

                account = accountResponse.Detail;
                session = sessionResponse.Detail;

                _accountSessionCache.Set(token.AccountId, account, session);
            }

            session.TokenVersion++;

            var (accessToken, expiresAt) = _jwtTokenService.GenerateAccessToken(account, session.TokenVersion);

            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_authOptions.RefreshTokenByteSize));

            await _refreshTokenService.Rotate(refreshToken, newRefreshToken);

            _accountSessionCache.Set(token.AccountId, account, session);

            _logger.RefreshSucceeded(token.AccountId, session.TokenVersion);

            return new LoginResponse(accessToken, expiresAt, newRefreshToken);
        }

        public async Task Logout(string refreshToken)
        {
            var token = await _refreshTokenService.Validate(refreshToken);

            await _refreshTokenService.Revoke(refreshToken);

            if (token is not null)
            {
                _accountSessionCache.Remove(token.AccountId);
                _logger.LogoutRevokedRefreshToken(token.AccountId);
            }
            else
            {
                _logger.LogoutInvalidRefreshToken();
            }
        }

    }

}
