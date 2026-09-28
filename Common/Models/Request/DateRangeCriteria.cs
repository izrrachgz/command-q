#nullable enable

namespace Common.Models.Request
{
    /// <summary>Represents an optional inclusive date and time range.</summary>
    public class DateRangeCriteria
    {
        /// <summary>Gets or sets the inclusive UTC start of the requested range.</summary>
        public DateTimeOffset? StartDate { get; set; }

        /// <summary>Gets or sets the inclusive UTC end of the requested range.</summary>
        public DateTimeOffset? EndDate { get; set; }
    }
}
