namespace Api.Models
{
    /// <summary>
    /// Describes a successful operation that does not return a resource representation.
    /// </summary>
    /// <param name="Message">The localized acknowledgement message.</param>
    public sealed record OperationResponse(string Message);
}
