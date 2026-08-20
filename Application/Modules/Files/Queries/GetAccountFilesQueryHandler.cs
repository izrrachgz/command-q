using SqlKata;
using Common.Cqrs;
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

            ApplySorting(baseQuery, criteria.SortBy, criteria.AscendingSort);

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

            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                var searchTerm = $@"%{criteria.SearchTerm.Trim()}%";

                query.Where(q => q
                    .WhereLike($@"{nameof(AccountFile.FileName)}", searchTerm)
                    .OrWhereLike($@"{nameof(AccountFile.Path)}", searchTerm)
                    .OrWhereLike($@"{nameof(AccountFile.ContentType)}", searchTerm));
            }

            return query;
        }

        private static void ApplySorting(Query query, string? sortBy, bool ascendingSort)
        {
            var column = sortBy switch
            {
                nameof(AccountFile.FileName) => nameof(AccountFile.FileName),
                nameof(AccountFile.ContentType) => nameof(AccountFile.ContentType),
                nameof(AccountFile.Size) => nameof(AccountFile.Size),
                nameof(AccountFile.CreatedUtc) => nameof(AccountFile.CreatedUtc),
                nameof(AccountFile.ModifiedUtc) => nameof(AccountFile.ModifiedUtc),
                _ => nameof(AccountFile.CreatedUtc)
            };

            if (ascendingSort)
                query.OrderBy(column);
            else
                query.OrderByDesc(column);
        }
    }
}
