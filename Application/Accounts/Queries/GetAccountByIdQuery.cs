using Common.Cqrs;
using Common.Models.Response;
using Application.Accounts.Dtos;

namespace Application.Accounts.Queries
{
    public record GetAccountByIdQuery : IQuery<DetailResponse<AccountModel>>
    {
        public required Guid AccountId { get; set; }
    }
}
