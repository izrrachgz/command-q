using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Modules.Identity.Accounts.Dtos;
using Application.Abstractions;

namespace Application.Modules.Identity.Accounts.Queries
{
    public sealed record GetAccountByIdentityQuery : IQuery<DetailResponse<AccountModel>>, ISystemScopedRequest
    {
        public required string Issuer { get; init; }
        public required string Subject { get; init; }
    }
}
