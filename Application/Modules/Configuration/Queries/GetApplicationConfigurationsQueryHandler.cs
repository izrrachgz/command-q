using SqlKata;
using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Abstractions;
using Persistence.Abstractions;
using Persistence.Repository.Extensions;
using Application.Modules.Configuration.Dtos;

namespace Application.Modules.Configuration.Queries
{
    internal sealed class GetApplicationConfigurationsQueryHandler(
        IReadOnlyRepository repository,
        IMapper<Domain.Modules.Configuration.ApplicationConfiguration, ApplicationConfigurationModel> mapper)
        : IQueryHandler<GetApplicationConfigurationsQuery, PagedResponse<ApplicationConfigurationModel>>
    {
        public async Task<PagedResponse<ApplicationConfigurationModel>> Handle(
            GetApplicationConfigurationsQuery request,
            CancellationToken cancellationToken)
        {
            var criteria = request.SearchCriteria;
            var query = new Query(nameof(Domain.Modules.Configuration.ApplicationConfiguration))
                .WhereNull(nameof(Domain.Modules.Configuration.ApplicationConfiguration.DeletedUtc))
                .ApplyUtcRange(
                    nameof(Domain.Modules.Configuration.ApplicationConfiguration.CreatedUtc),
                    criteria,
                    repository.Engine);

            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                string term = $"%{criteria.SearchTerm.Trim()}%";
                query.Where(q => q
                    .WhereLike(nameof(Domain.Modules.Configuration.ApplicationConfiguration.ApplicationVersion), term)
                    .OrWhereLike(nameof(Domain.Modules.Configuration.ApplicationConfiguration.Environment), term)
                    .OrWhereLike(nameof(Domain.Modules.Configuration.ApplicationConfiguration.Language), term)
                    .OrWhereLike(nameof(Domain.Modules.Configuration.ApplicationConfiguration.ServerTimeZone), term)
                    .OrWhereLike(nameof(Domain.Modules.Configuration.ApplicationConfiguration.InstanceName), term));
            }

            ApplySorting(query, criteria.SortBy, criteria.AscendingSort);
            var page = await repository.PaginateAsync<Domain.Modules.Configuration.ApplicationConfiguration>(
                query, criteria.Page, criteria.Size, cancellationToken);

            return new(page.List.Select(mapper.Map), page.Count, page.Page, page.PerPage);
        }

        private static void ApplySorting(Query query, string? sortBy, bool ascendingSort)
        {
            var column = sortBy switch
            {
                nameof(Domain.Modules.Configuration.ApplicationConfiguration.ApplicationVersion) =>
                    nameof(Domain.Modules.Configuration.ApplicationConfiguration.ApplicationVersion),
                nameof(Domain.Modules.Configuration.ApplicationConfiguration.Environment) =>
                    nameof(Domain.Modules.Configuration.ApplicationConfiguration.Environment),
                nameof(Domain.Modules.Configuration.ApplicationConfiguration.InstanceName) =>
                    nameof(Domain.Modules.Configuration.ApplicationConfiguration.InstanceName),
                nameof(Domain.Modules.Configuration.ApplicationConfiguration.Language) =>
                    nameof(Domain.Modules.Configuration.ApplicationConfiguration.Language),
                nameof(Domain.Modules.Configuration.ApplicationConfiguration.ServerTimeZone) =>
                    nameof(Domain.Modules.Configuration.ApplicationConfiguration.ServerTimeZone),
                nameof(Domain.Modules.Configuration.ApplicationConfiguration.CreatedUtc) =>
                    nameof(Domain.Modules.Configuration.ApplicationConfiguration.CreatedUtc),
                _ => nameof(Domain.Modules.Configuration.ApplicationConfiguration.CreatedUtc)
            };

            if (string.IsNullOrWhiteSpace(sortBy))
                query.OrderByDesc(column);
            else if (ascendingSort)
                query.OrderBy(column);
            else
                query.OrderByDesc(column);
        }
    }
}
