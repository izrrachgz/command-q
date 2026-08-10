using MediatR;
using Api.Constants;
using Api.Extensions;
using Api.Interfaces;
using Common.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Application.Modules.Identity.Accounts.Queries;
using Application.Modules.Identity.Accounts.Commands;
using Application.Modules.Identity.AccountActivity.Queries;

namespace Api.Modules.Identity
{
    [ApiController, Route("[controller]")]
    public sealed class AccountController(ISender mediator, ICurrentAccountAccessor currentAccountAccessor) : ControllerBase
    {
        [Authorize, HttpPost("create")]
        [EnableRateLimiting(ConfigKeys.RateLimiting.FixedPolicyName)]
        public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
        {
            if (!User.TryGetIdentityKey(out var issuer, out var subject))
            {
                return Unauthorized();
            }

            var username = User.GetUsername();
            var email = User.GetEmail();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email))
            {
                return BadRequest();
            }

            var response = await mediator.Send(new CreateAccountCommand
            {
                Username = username,
                Email = email,
                IdentityIssuer = issuer,
                IdentitySubject = subject
            }, cancellationToken);

            return response.Success ? Ok(response) : BadRequest(response.Message);
        }

        [Authorize, HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken cancellationToken = default)
        {
            var currentAccount = await currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
            {
                return Forbid();
            }

            var response = await mediator.Send(new GetAccountByIdQuery
            {
                AccountId = currentAccount.AccountId
            }, cancellationToken);

            return response.Success ? Ok(response) : NotFound(response);
        }

        [Authorize, HttpGet("activity")]
        public async Task<IActionResult> Activity(
            [FromQuery] SearchCriteria searchCriteria,
            [FromQuery] string? module = null,
            [FromQuery] string? action = null,
            CancellationToken cancellationToken = default)
        {
            var currentAccount = await currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
            {
                return Forbid();
            }

            var response = await mediator.Send(new GetAccountActivityQuery
            {
                AccountId = currentAccount.AccountId,
                Module = module,
                Action = action,
                SearchCriteria = searchCriteria ?? new()
            }, cancellationToken);

            return Ok(response);
        }
    }
}
