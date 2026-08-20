using SqlKata;
using Common.Cqrs;
using Common.Enumerations;
using Common.Models.Response;
using Application.Abstractions;
using Persistence.Abstractions;
using Persistence.Repository.Extensions;
using Application.Modules.Identity.AccountActivity.Dtos;

namespace Application.Modules.Identity.AccountActivity.Queries
{
    internal sealed class GetAccountActivityQueryHandler(IReadOnlyRepository repository, IMapper<Domain.Modules.Identity.AccountActivity, AccountActivityModel> mapper) : IQueryHandler<GetAccountActivityQuery, PagedResponse<AccountActivityModel>>
    {
        public async Task<PagedResponse<AccountActivityModel>> Handle(GetAccountActivityQuery request, CancellationToken cancellationToken)
        {
            var criteria = request.SearchCriteria ?? new();
            var baseQuery = BuildQuery(request, repository.Engine);

            ApplySorting(baseQuery, criteria.SortBy, criteria.AscendingSort);

            var page = await repository.PaginateAsync<Domain.Modules.Identity.AccountActivity>(baseQuery, criteria.Page, criteria.Size, cancellationToken);

            return new(page.List.Select(record => mapper.Map(record)), page.Count, page.Page, page.PerPage);
        }

        private static Query BuildQuery(GetAccountActivityQuery request, DatabaseEngine engine)
        {
            var criteria = request.SearchCriteria ?? new();
            var query = new Query(nameof(Domain.Modules.Identity.AccountActivity))
                .WhereNull(nameof(Domain.Modules.Identity.AccountActivity.Deleted))
                .Where(nameof(Domain.Modules.Identity.AccountActivity.AccountId), request.AccountId);

            if (!string.IsNullOrWhiteSpace(request.Module))
                query.Where(nameof(Domain.Modules.Identity.AccountActivity.Module), request.Module.Trim());

            if (!string.IsNullOrWhiteSpace(request.Action))
                query.Where(nameof(Domain.Modules.Identity.AccountActivity.Action), request.Action.Trim());

            if (criteria.StartDate.HasValue)
                query.Where(nameof(Domain.Modules.Identity.AccountActivity.CreatedUtc), ">=", criteria.StartDate.Value.ToDateTimeFilterValue(engine));

            if (criteria.EndDate.HasValue)
                query.Where(nameof(Domain.Modules.Identity.AccountActivity.CreatedUtc), "<=", criteria.EndDate.Value.ToDateTimeFilterValue(engine));

            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                var searchTerm = $@"%{criteria.SearchTerm.Trim()}%";

                query.Where(q => q
                    .WhereLike(nameof(Domain.Modules.Identity.AccountActivity.Module), searchTerm)
                    .OrWhereLike(nameof(Domain.Modules.Identity.AccountActivity.Action), searchTerm)
                    .OrWhereLike(nameof(Domain.Modules.Identity.AccountActivity.EntityType), searchTerm)
                    .OrWhereLike(nameof(Domain.Modules.Identity.AccountActivity.EntityId), searchTerm)
                    .OrWhereLike(nameof(Domain.Modules.Identity.AccountActivity.Message), searchTerm));
            }

            return query;
        }

        private static void ApplySorting(Query query, string? sortBy, bool ascendingSort)
        {
            var column = sortBy switch
            {
                nameof(Domain.Modules.Identity.AccountActivity.Module) => nameof(Domain.Modules.Identity.AccountActivity.Module),
                nameof(Domain.Modules.Identity.AccountActivity.Action) => nameof(Domain.Modules.Identity.AccountActivity.Action),
                nameof(Domain.Modules.Identity.AccountActivity.EntityType) => nameof(Domain.Modules.Identity.AccountActivity.EntityType),
                nameof(Domain.Modules.Identity.AccountActivity.CreatedUtc) => nameof(Domain.Modules.Identity.AccountActivity.CreatedUtc),
                _ => nameof(Domain.Modules.Identity.AccountActivity.CreatedUtc)
            };

            if (ascendingSort)
                query.OrderBy(column);
            else
                query.OrderByDesc(column);
        }
    }
}
