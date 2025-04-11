using Common.Cqrs;
using Common.Models.Response;
using Application.Accounts.Dtos;

namespace Application.Accounts.Queries
{
    public record GetAccountByUsernameQuery : IQuery<DetailResponse<AccountModel>>
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
