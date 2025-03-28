#nullable enable

namespace Common.Models.Request
{
    public class SearchCriteria
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Size { get; set; } = 25;
        public int Page { get; set; } = 1;
        public string? SortBy { get; set; }
        public bool AscendingSort { get; set; } = true;
        public int CalculatedOffset => Page > 1 ? ((Page - 1) * Size) : 0;
    }
}
