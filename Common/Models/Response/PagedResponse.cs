#nullable disable
namespace Common.Models.Response
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Collection { get; }
        public int Total { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalPages { get; }

        public PagedResponse()
        {

        }

        public PagedResponse(IEnumerable<T> collection, int total, int page, int itemsPerPage = 25)
        {
            Page = page;
            Total = total;
            Collection = collection;
            PageSize = collection.Count();
            TotalPages = total > 0 && itemsPerPage > 0 ? (int)Math.Ceiling(decimal.Divide(total, itemsPerPage)) : 0;
        }
    }
}
