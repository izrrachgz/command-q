using Cqrs.Abstractions;
using Common.Models.Request;
using Common.Models.Response;
using Application.Abstractions;
using Application.Modules.Files.Dtos;

namespace Application.Modules.Files.Queries
{
    public sealed record GetAccountFilesQuery : IQuery<PagedResponse<AccountFileModel>>, IAccountScopedRequest
    {
        public required Guid AccountId { get; set; }

        public SearchCriteria SearchCriteria { get; set; } = new();
    }
}
