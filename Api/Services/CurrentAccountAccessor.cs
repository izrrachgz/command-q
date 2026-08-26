using Cqrs.Abstractions;
using Api.Models;
using Api.Extensions;
using Api.Abstractions;
using Application.Modules.Identity.Accounts.Queries;

namespace Api.Services
{
    internal sealed class CurrentAccountAccessor(IHttpContextAccessor httpContextAccessor, IRequestDispatcher dispatcher) : ICurrentAccountAccessor
    {
        private Task<CurrentUser?>? _currentAccount;

        public Task<CurrentUser?> Get(CancellationToken cancellationToken = default)
        {
            return _currentAccount ??= Resolve(cancellationToken);
        }

        private async Task<CurrentUser?> Resolve(CancellationToken cancellationToken)
        {
            var principal = httpContextAccessor.HttpContext?.User;

            if (principal is null || !principal.TryGetIdentityKey(out var issuer, out var subject))
            {
                return null;
            }

            var response = await dispatcher.Send(new GetAccountByIdentityQuery
            {
                Issuer = issuer,
                Subject = subject
            }, cancellationToken);

            if (!response.Success || response.Detail is null)
            {
                return null;
            }

            return new CurrentUser
            {
                AccountId = response.Detail.Id,
                Issuer = issuer,
                Subject = subject,
                Username = principal.GetUsername(),
                Email = principal.GetEmail(),
                JwtId = principal.GetJwtId(),
                Roles = principal.GetRoles(),
                IsAuthenticated = true
            };
        }
    }
}
