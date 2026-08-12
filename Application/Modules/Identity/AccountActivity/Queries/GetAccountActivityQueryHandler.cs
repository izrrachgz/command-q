using SqlKata;
using Common.Cqrs;
using Common.Enumerations;
using System.Globalization;
using Common.Models.Response;
using Application.Abstractions;
using Persistence.Abstractions;
using Application.Modules.Identity.AccountActivity.Dtos;

namespace Application.Modules.Identity.AccountActivity.Queries
{
    internal sealed class GetAccountActivityQueryHandler(IReadOnlyRepository repository, IMapper<Domain.Modules.Identity.AccountActivity, AccountActivityModel> mapper) : IQueryHandler<GetAccountActivityQuery, PagedResponse<AccountActivityModel>>
    {
        public async Task<PagedResponse<AccountActivityModel>> Handle(GetAccountActivityQuery request, CancellationToken cancellationToken)
        {
            var criteria = request.SearchCriteria ?? new();
            var baseQuery = BuildQuery(request, repository.Engine);

            var total = await repository.FirstOrDefaultAsync<int>(baseQuery.Clone().AsCount(), cancellationToken);

            ApplySorting(baseQuery, criteria.SortBy, criteria.AscendingSort);

            var records = await repository.GetAsync<Domain.Modules.Identity.AccountActivity>(
                baseQuery
                    .Limit(criteria.Size)
                    .Offset(criteria.CalculatedOffset),
                cancellationToken);

            return new(records.Select(mapper.Map), total, criteria.Page, criteria.Size);
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
                query.Where(nameof(Domain.Modules.Identity.AccountActivity.CreatedUtc), ">=", FormatDateFilter(criteria.StartDate.Value, engine));

            if (criteria.EndDate.HasValue)
                query.Where(nameof(Domain.Modules.Identity.AccountActivity.CreatedUtc), "<=", FormatDateFilter(criteria.EndDate.Value, engine));

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

        private static object FormatDateFilter(DateTime value, DatabaseEngine engine)
        {
            if (engine != DatabaseEngine.SqLite)
                return value;

            return value.ToString(@"yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
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
