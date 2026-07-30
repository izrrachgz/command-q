using FluentValidation.Results;

namespace Api.Models
{
    /// <summary>
    /// Response body returned by the API exception middleware.
    /// </summary>
    public sealed class ErrorResponse
    {
        /// <summary>
        /// Localized error title.
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// HTTP status code associated with the error.
        /// </summary>
        public int Status { get; init; }

        /// <summary>
        /// Detailed exception message.
        /// </summary>
        public string ExceptionDetail { get; init; } = string.Empty;

        /// <summary>
        /// Validation errors associated with the request, when available.
        /// </summary>
        public IEnumerable<ValidationFailure> Errors { get; init; } = [];
    }
}
