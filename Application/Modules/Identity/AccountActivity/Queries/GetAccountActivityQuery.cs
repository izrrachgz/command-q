using Cqrs.Abstractions;
using Common.Models.Request;
using Common.Models.Response;
using Application.Modules.Identity.AccountActivity.Dtos;

namespace Application.Modules.Identity.AccountActivity.Queries
{
    public sealed record GetAccountActivityQuery : IQuery<PagedResponse<AccountActivityModel>>
    {
        public required Guid AccountId { get; set; }

        public string? Module { get; set; }

        public string? Action { get; set; }

        public SearchCriteria SearchCriteria { get; set; } = new();
    }
}
