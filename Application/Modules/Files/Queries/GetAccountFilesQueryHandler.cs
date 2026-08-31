using SqlKata;
using Cqrs.Abstractions;
using Common.Enumerations;
using Common.Models.Response;
using Application.Abstractions;
using Persistence.Abstractions;
using Application.Modules.Files.Dtos;
using Persistence.Repository.Extensions;

namespace Application.Modules.Files.Queries
{
    internal sealed class GetAccountFilesQueryHandler : IQueryHandler<GetAccountFilesQuery, PagedResponse<AccountFileModel>>
    {
        private static readonly string[] SearchColumns =
        [
            nameof(AccountFile.FileName),
            nameof(AccountFile.Path),
            nameof(AccountFile.ContentType)
        ];

        private static readonly IReadOnlyDictionary<string, string> SortColumns =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [nameof(AccountFile.FileName)] = nameof(AccountFile.FileName),
                [nameof(AccountFile.ContentType)] = nameof(AccountFile.ContentType),
                [nameof(AccountFile.Size)] = nameof(AccountFile.Size),
                [nameof(AccountFile.CreatedUtc)] = nameof(AccountFile.CreatedUtc),
                [nameof(AccountFile.ModifiedUtc)] = nameof(AccountFile.ModifiedUtc)
            };

        private readonly IReadOnlyRepository _repository;
        private readonly IMapper<AccountFile, AccountFileModel> _mapper;

        public GetAccountFilesQueryHandler(IReadOnlyRepository repository, IMapper<AccountFile, AccountFileModel> mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        /// <summary>
        /// Gets paged file metadata for the specified account without returning file contents.
        /// </summary>
        public async Task<PagedResponse<AccountFileModel>> Handle(GetAccountFilesQuery request, CancellationToken cancellationToken)
        {
            var criteria = request.SearchCriteria ?? new();
            var baseQuery = BuildQuery(request, _repository.Engine);

            baseQuery.ApplySorting(
                criteria.SortBy,
                criteria.AscendingSort,
                SortColumns,
                nameof(AccountFile.CreatedUtc));

            var page = await _repository.PaginateAsync<AccountFile>(baseQuery, criteria.Page, criteria.Size, cancellationToken);

            return new(page.List.Select(record => _mapper.Map(record)), page.Count, page.Page, page.PerPage);
        }

        private static Query BuildQuery(GetAccountFilesQuery request, DatabaseEngine engine)
        {
            var criteria = request.SearchCriteria ?? new();
            var query = new Query($@"{nameof(AccountFile)}")
                .WhereNull($@"{nameof(AccountFile.DeletedUtc)}")
                .Where($@"{nameof(AccountFile.AccountId)}", request.AccountId)
                .ApplyUtcRange(nameof(AccountFile.CreatedUtc), criteria, engine);

            return query.ApplySearchTerm(criteria.SearchTerm, SearchColumns);
        }
    }
}
