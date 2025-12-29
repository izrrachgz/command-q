using Common.Cqrs;
using Common.Models.Response;
using Application.AccountSessions.Dtos;

namespace Application.AccountSessions.Queries
{
    public record GetAccountSessionByIdQuery : IQuery<DetailResponse<AccountSessionModel>>
    {
        public required Guid AccountId { get; set; }
    }
}
