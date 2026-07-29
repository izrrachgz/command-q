using Public;
using SqlKata;
using Common.Cqrs;
using Domain.Entities;
using Common.Enumerations;
using System.Globalization;
using Application.Interfaces;
using Common.Models.Response;
using Persistence.Interfaces;
using Application.Files.Dtos;

namespace Application.Files.Queries
{
    public class GetAccountFilesQueryHandler : IQueryHandler<GetAccountFilesQuery, PagedResponse<AccountFileModel>>
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

            var total = await _repository.FirstOrDefaultAsync<int>(baseQuery.Clone().AsCount(), cancellationToken);

            ApplySorting(baseQuery, criteria.SortBy, criteria.AscendingSort);

            var records = await _repository.GetAsync<AccountFile>(
                baseQuery
                    .Limit(criteria.Size)
                    .Offset(criteria.CalculatedOffset),
                cancellationToken);

            return new(records.Select(_mapper.Map), total, criteria.Page, criteria.Size);
        }

        private static Query BuildQuery(GetAccountFilesQuery request, DatabaseEngine engine)
        {
            var criteria = request.SearchCriteria ?? new();
            var query = new Query($@"{nameof(AccountFile)}")
                .WhereNull($@"{nameof(AccountFile.Deleted)}")
                .Where($@"{nameof(AccountFile.AccountId)}", request.AccountId);

            if (criteria.StartDate.HasValue)
                query.Where($@"{nameof(AccountFile.Created)}", ">=", FormatDateFilter(criteria.StartDate.Value, engine));

            if (criteria.EndDate.HasValue)
                query.Where($@"{nameof(AccountFile.Created)}", "<=", FormatDateFilter(criteria.EndDate.Value, engine));

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
                nameof(AccountFile.FileName) => nameof(AccountFile.FileName),
                nameof(AccountFile.ContentType) => nameof(AccountFile.ContentType),
                nameof(AccountFile.Size) => nameof(AccountFile.Size),
                nameof(AccountFile.Created) => nameof(AccountFile.Created),
                nameof(AccountFile.Modified) => nameof(AccountFile.Modified),
                _ => nameof(AccountFile.Created)
            };

            if (ascendingSort)
                query.OrderBy(column);
            else
                query.OrderByDesc(column);
        }
    }
}
