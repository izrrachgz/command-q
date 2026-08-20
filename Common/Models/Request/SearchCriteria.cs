#nullable enable

namespace Common.Models.Request
{
    public class SearchCriteria
    {
        /// <summary>Gets or sets the inclusive UTC start of the requested range.</summary>
        public DateTimeOffset? StartDate { get; set; }

        /// <summary>Gets or sets the inclusive UTC end of the requested range.</summary>
        public DateTimeOffset? EndDate { get; set; }

        /// <summary>Gets or sets the IANA/TZDB timezone used when account-local conversion is required.</summary>
        public string? TimeZone { get; set; }
        public string? SearchTerm { get; set; }
        public int Size { get; set; } = 25;
        public int Page { get; set; } = 1;
        public string? SortBy { get; set; }
        public bool AscendingSort { get; set; } = true;
    }
}
