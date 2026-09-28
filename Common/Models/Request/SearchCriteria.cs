#nullable enable

namespace Common.Models.Request
{
    public class SearchCriteria : DateRangeCriteria
    {
        public string? SearchTerm { get; set; }
        public int Size { get; set; } = 25;
        public int Page { get; set; } = 1;
        public string? SortBy { get; set; }
        public bool AscendingSort { get; set; } = true;
    }
}
