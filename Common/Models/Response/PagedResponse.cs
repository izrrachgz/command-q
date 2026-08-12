#nullable disable
namespace Common.Models.Response
{
    public sealed class PagedResponse<T>
    {
        public IEnumerable<T> Collection { get; } = [];
        public long Total { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalPages { get; }

        public PagedResponse(IEnumerable<T> collection, long total, int page, int itemsPerPage = 25)
        {
            Page = page;
            Total = total;
            Collection = collection;
            PageSize = itemsPerPage;
            TotalPages = total > 0 && itemsPerPage > 0 ? (int)Math.Ceiling(decimal.Divide(total, itemsPerPage)) : 0;
        }
    }
}
