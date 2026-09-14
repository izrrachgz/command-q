using SqlKata;
using Cqrs.Abstractions;
using Common.Enumerations;
using Common.Models.Response;
using Application.Abstractions;
using Persistence.Abstractions;
using Persistence.Repository.Extensions;
using Application.Modules.Identity.AccountActivity.Dtos;
using AccountActivityEntity = Domain.Modules.Identity.AccountActivity;

namespace Application.Modules.Identity.AccountActivity.Queries
{
    internal sealed class GetAccountActivityQueryHandler(IReadOnlyRepository repository, IMapper<AccountActivityEntity, AccountActivityModel> mapper) : IQueryHandler<GetAccountActivityQuery, PagedResponse<AccountActivityModel>>
    {
        private static readonly string[] SearchColumns =
        [
            nameof(AccountActivityEntity.Module),
            nameof(AccountActivityEntity.Action),
            nameof(AccountActivityEntity.EntityType),
            nameof(AccountActivityEntity.EntityId),
            nameof(AccountActivityEntity.Message)
        ];

        private static readonly IReadOnlyDictionary<string, string> SortColumns =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [nameof(AccountActivityEntity.Module)] = nameof(AccountActivityEntity.Module),
                [nameof(AccountActivityEntity.Action)] = nameof(AccountActivityEntity.Action),
                [nameof(AccountActivityEntity.EntityType)] = nameof(AccountActivityEntity.EntityType),
                [nameof(AccountActivityEntity.CreatedUtc)] = nameof(AccountActivityEntity.CreatedUtc)
            };

        public async Task<PagedResponse<AccountActivityModel>> Handle(GetAccountActivityQuery request, CancellationToken cancellationToken)
        {
            var criteria = request.SearchCriteria ?? new();
            var baseQuery = BuildQuery(request, repository.Engine);

            baseQuery.ApplySorting(
                criteria.SortBy,
                criteria.AscendingSort,
                SortColumns,
                nameof(AccountActivityEntity.CreatedUtc));

            var page = await repository.PaginateAsync<AccountActivityEntity>(baseQuery, criteria.Page, criteria.Size, cancellationToken);

            return new(page.List.Select(record => mapper.Map(record)), page.Count, page.Page, page.PerPage);
        }

        private static Query BuildQuery(GetAccountActivityQuery request, DatabaseEngine engine)
        {
            var criteria = request.SearchCriteria ?? new();
            var query = new Query(nameof(Domain.Modules.Identity.AccountActivity))
                .WhereNull(nameof(AccountActivityEntity.DeletedUtc))
                .Where(nameof(AccountActivityEntity.AccountId), request.AccountId)
                .ApplyUtcRange(nameof(AccountActivityEntity.CreatedUtc), criteria, engine);

            if (!string.IsNullOrWhiteSpace(request.Module))
                query.Where(nameof(AccountActivityEntity.Module), request.Module.Trim());

            if (!string.IsNullOrWhiteSpace(request.Action))
                query.Where(nameof(AccountActivityEntity.Action), request.Action.Trim());

            return query.ApplySearchTerm(criteria.SearchTerm, SearchColumns);
        }
    }
}
