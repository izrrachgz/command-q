using Common.Cqrs;
using Common.Models.Request;
using Common.Models.Response;
using Application.Files.Dtos;

namespace Application.Files.Queries
{
    public sealed record GetAccountFilesQuery : IQuery<PagedResponse<AccountFileModel>>
    {
        public required Guid AccountId { get; set; }

        public SearchCriteria SearchCriteria { get; set; } = new();
    }
}
