using Common.Cqrs;
using Common.Models.Response;

namespace Application.AccountSessions.Queries
{
    public record CheckIfAccountSessionIsLockedByIdQuery : IQuery<BasicResponse>
    {
        public required Guid AccountId { get; set; }
        public required int MaxFailedAttempts { get; set; }
    }
}
