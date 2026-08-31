using SqlKata;
using Cqrs.Abstractions;
using Common.Models.Response;
using Application.Abstractions;
using Persistence.Abstractions;
using Persistence.Repository.Extensions;
using Application.Modules.Configuration.Dtos;
using ApplicationConfiguration = Domain.Modules.Configuration.ApplicationConfiguration;

namespace Application.Modules.Configuration.Queries
{
    internal sealed class GetApplicationConfigurationsQueryHandler(
        IReadOnlyRepository repository,
        IMapper<ApplicationConfiguration, ApplicationConfigurationModel> mapper)
        : IQueryHandler<GetApplicationConfigurationsQuery, PagedResponse<ApplicationConfigurationModel>>
    {
        private static readonly string[] SearchColumns =
        [
            nameof(ApplicationConfiguration.ApplicationVersion),
            nameof(ApplicationConfiguration.Environment),
            nameof(ApplicationConfiguration.Language),
            nameof(ApplicationConfiguration.ServerTimeZone),
            nameof(ApplicationConfiguration.InstanceName)
        ];

        private static readonly IReadOnlyDictionary<string, string> SortColumns =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [nameof(ApplicationConfiguration.ApplicationVersion)] = nameof(ApplicationConfiguration.ApplicationVersion),
                [nameof(ApplicationConfiguration.Environment)] = nameof(ApplicationConfiguration.Environment),
                [nameof(ApplicationConfiguration.InstanceName)] = nameof(ApplicationConfiguration.InstanceName),
                [nameof(ApplicationConfiguration.Language)] = nameof(ApplicationConfiguration.Language),
                [nameof(ApplicationConfiguration.ServerTimeZone)] = nameof(ApplicationConfiguration.ServerTimeZone),
                [nameof(ApplicationConfiguration.CreatedUtc)] = nameof(ApplicationConfiguration.CreatedUtc)
            };

        public async Task<PagedResponse<ApplicationConfigurationModel>> Handle(
            GetApplicationConfigurationsQuery request,
            CancellationToken cancellationToken)
        {
            var criteria = request.SearchCriteria;
            var query = new Query(nameof(ApplicationConfiguration))
                .WhereNull(nameof(ApplicationConfiguration.DeletedUtc))
                .ApplyUtcRange(
                    nameof(ApplicationConfiguration.CreatedUtc),
                    criteria,
                    repository.Engine);

            query
                .ApplySearchTerm(criteria.SearchTerm, SearchColumns)
                .ApplySorting(
                    criteria.SortBy,
                    criteria.AscendingSort,
                    SortColumns,
                    nameof(ApplicationConfiguration.CreatedUtc),
                    defaultAscending: false);
            var page = await repository.PaginateAsync<ApplicationConfiguration>(
                query, criteria.Page, criteria.Size, cancellationToken);

            return new(page.List.Select(mapper.Map), page.Count, page.Page, page.PerPage);
        }
    }
}
