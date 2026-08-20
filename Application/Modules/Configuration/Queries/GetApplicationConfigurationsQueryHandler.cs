using SqlKata;
using Common.Cqrs;
using Common.Models.Response;
using Application.Abstractions;
using Persistence.Abstractions;
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
                .WhereNull(nameof(Domain.Modules.Configuration.ApplicationConfiguration.Deleted));

            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                string term = $"%{criteria.SearchTerm.Trim()}%";
                query.Where(q => q
                    .WhereLike(nameof(Domain.Modules.Configuration.ApplicationConfiguration.ApplicationVersion), term)
                    .OrWhereLike(nameof(Domain.Modules.Configuration.ApplicationConfiguration.Environment), term)
                    .OrWhereLike(nameof(Domain.Modules.Configuration.ApplicationConfiguration.Language), term)
                    .OrWhereLike(nameof(Domain.Modules.Configuration.ApplicationConfiguration.ServerTimeZone), term));
            }

            query.OrderByDesc(nameof(Domain.Modules.Configuration.ApplicationConfiguration.Created));
            var page = await repository.PaginateAsync<Domain.Modules.Configuration.ApplicationConfiguration>(
                query, criteria.Page, criteria.Size, cancellationToken);

            return new(page.List.Select(mapper.Map), page.Count, page.Page, page.PerPage);
        }
    }
}
