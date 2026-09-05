using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Modules.Identity.Accounts.Dtos;
using Application.Abstractions;

namespace Application.Modules.Identity.Accounts.Queries
{
    public record GetAccountByUsernameQuery : IQuery<DetailResponse<AccountModel>>, ISystemScopedRequest
    {
        public string Username { get; set; } = string.Empty;
    }
}
