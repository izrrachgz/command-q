namespace Api.Models
{
    /// <summary>
    /// Represents one page of API resources.
    /// </summary>
    public sealed record PageResponse<T>
    {
        /// <summary>Gets the resources in the current page.</summary>
        public required IReadOnlyList<T> Items { get; init; }

        /// <summary>Gets the total number of matching resources.</summary>
        public required long Total { get; init; }

        /// <summary>Gets the one-based current page number.</summary>
        public required int Page { get; init; }

        /// <summary>Gets the maximum number of resources in one page.</summary>
        public required int PageSize { get; init; }

        /// <summary>Gets the total number of available pages.</summary>
        public required int TotalPages { get; init; }
    }
}
