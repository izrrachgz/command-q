using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Modules.Identity.Accounts.Dtos;

namespace Application.Modules.Identity.Accounts.Queries
{
    public record GetAccountByIdQuery : IQuery<DetailResponse<AccountModel>>
    {
        public required Guid AccountId { get; set; }
    }
}
