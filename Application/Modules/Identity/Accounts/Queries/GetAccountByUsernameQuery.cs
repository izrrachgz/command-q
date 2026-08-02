using Common.Cqrs;
using Common.Models.Response;
using Application.Modules.Identity.Accounts.Dtos;

namespace Application.Modules.Identity.Accounts.Queries
{
    public record GetAccountByUsernameQuery : IQuery<DetailResponse<AccountModel>>
    {
        public string Username { get; set; } = string.Empty;
    }
}
