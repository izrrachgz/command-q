using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Modules.Identity.Accounts.Dtos;
using Application.Abstractions;

namespace Application.Modules.Identity.Accounts.Queries
{
    public record GetAccountByIdQuery : IQuery<DetailResponse<AccountModel>>, IAccountScopedRequest
    {
        public required Guid AccountId { get; set; }
    }
}
