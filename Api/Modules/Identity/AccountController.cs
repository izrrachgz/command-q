using Cqrs.Abstractions;
using Api.Constants;
using Api.Extensions;
using Api.Models;
using Api.Abstractions;
using Common.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Api.Modules.Identity.Requests;
using Application.Modules.Identity.Accounts.Queries;
using Application.Modules.Identity.Accounts.Commands;
using Application.Modules.Identity.AccountActivity.Queries;
using Application.Modules.Identity.Accounts.Preferences.Queries;
using Application.Modules.Identity.Accounts.Preferences.Commands;
using Application.Modules.Identity.Accounts.Dtos;
using Application.Modules.Identity.AccountActivity.Dtos;
using Application.Modules.Identity.Accounts.Preferences.Dtos;

namespace Api.Modules.Identity
{
    [ApiController, Route(IdentityRoute.AccountRoot)]
    public sealed class AccountController(IRequestDispatcher dispatcher, ICurrentAccountAccessor currentAccountAccessor) : ControllerBase
    {
        [Authorize, HttpPost]
        [EnableRateLimiting(ApiConfigKeys.RateLimiting.FixedPolicyName)]
        public async Task<ActionResult<OperationResponse>> Create(CancellationToken cancellationToken = default)
        {
            if (!User.TryGetIdentityKey(out var issuer, out var subject))
            {
                return this.ApiProblem(
                    StatusCodes.Status401Unauthorized,
                    ApiProblemCodes.Unauthorized);
            }

            var username = User.GetUsername();
            var email = User.GetEmail();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email))
            {
                return this.ApiProblem(
                    StatusCodes.Status400BadRequest,
                    ApiProblemCodes.BadRequest);
            }

            var response = await dispatcher.Send(new CreateAccountCommand
            {
                Username = username,
                Email = email,
                IdentityIssuer = issuer,
                IdentitySubject = subject
            }, cancellationToken);

            return response.Success
                ? CreatedAtAction(nameof(Me), response.ToOperationResponse())
                : this.ApiProblem(
                    StatusCodes.Status409Conflict,
                    ApiProblemCodes.Accounts.ProfileConflict,
                    response.Message);
        }

        [Authorize, HttpGet(IdentityRoute.Me)]
        public async Task<ActionResult<AccountModel>> Me(CancellationToken cancellationToken = default)
        {
            if (!User.TryGetIdentityKey(out _, out _))
            {
                return this.ApiProblem(
                    StatusCodes.Status401Unauthorized,
                    ApiProblemCodes.Unauthorized);
            }

            var currentAccount = await currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
            {
                return this.ApiProblem(
                    StatusCodes.Status404NotFound,
                    ApiProblemCodes.Accounts.ProfileMissing);
            }

            var response = await dispatcher.Send(new GetAccountByIdQuery
            {
                AccountId = currentAccount.AccountId
            }, cancellationToken);

            return response.Success
                ? response.Detail!
                : this.ApiProblem(
                    StatusCodes.Status404NotFound,
                    ApiProblemCodes.Accounts.ProfileMissing,
                    response.Message);
        }

        [Authorize, HttpGet(IdentityRoute.Activity)]
        public async Task<ActionResult<PageResponse<AccountActivityModel>>> Activity(
            [FromQuery] SearchCriteria searchCriteria,
            [FromQuery] string? module = null,
            [FromQuery] string? action = null,
            CancellationToken cancellationToken = default)
        {
            var currentAccount = await currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
            {
                return this.ApiProblem(
                    StatusCodes.Status403Forbidden,
                    ApiProblemCodes.Forbidden);
            }

            var response = await dispatcher.Send(new GetAccountActivityQuery
            {
                AccountId = currentAccount.AccountId,
                Module = module,
                Action = action,
                SearchCriteria = searchCriteria ?? new()
            }, cancellationToken);

            return response.ToPageResponse();
        }

        [Authorize, HttpGet(IdentityRoute.Preferences)]
        public async Task<ActionResult<AccountPreferenceModel>> Preferences(CancellationToken cancellationToken = default)
        {
            var currentAccount = await currentAccountAccessor.Get(cancellationToken);
            if (currentAccount is null)
                return this.ApiProblem(
                    StatusCodes.Status403Forbidden,
                    ApiProblemCodes.Forbidden);

            var response = await dispatcher.Send(new GetAccountPreferencesQuery
            {
                AccountId = currentAccount.AccountId
            }, cancellationToken);

            return response.Success
                ? response.Detail!
                : this.ApiProblem(
                    StatusCodes.Status404NotFound,
                    ApiProblemCodes.Accounts.PreferencesMissing,
                    response.Message);
        }

        [Authorize, HttpPut(IdentityRoute.Preferences)]
        public async Task<IActionResult> SavePreferences(
            [FromBody] SaveAccountPreferencesRequest request,
            CancellationToken cancellationToken = default)
        {
            var currentAccount = await currentAccountAccessor.Get(cancellationToken);
            if (currentAccount is null)
                return this.ApiProblem(
                    StatusCodes.Status403Forbidden,
                    ApiProblemCodes.Forbidden);

            var response = await dispatcher.Send(new SaveAccountPreferencesCommand
            {
                AccountId = currentAccount.AccountId,
                Culture = request.Culture,
                Theme = request.Theme,
                CompactNavigation = request.CompactNavigation,
                EnhancedFocus = request.EnhancedFocus,
                ReduceMotion = request.ReduceMotion,
                ShowDenseTables = request.ShowDenseTables,
                TimeZone = request.TimeZone
            }, cancellationToken);

            return response.Success
                ? NoContent()
                : this.ApiProblem(
                    StatusCodes.Status500InternalServerError,
                    ApiProblemCodes.Accounts.PreferencesUpdateFailed,
                    response.Message);
        }
    }
}
