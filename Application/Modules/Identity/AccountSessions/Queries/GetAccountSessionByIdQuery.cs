using Common.Cqrs;
using Common.Models.Response;
using Application.Modules.Identity.AccountSessions.Dtos;

namespace Application.Modules.Identity.AccountSessions.Queries
{
    public record GetAccountSessionByIdQuery : IQuery<DetailResponse<AccountSessionModel>>
    {
        public required Guid AccountId { get; set; }
    }
}
