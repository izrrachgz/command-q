namespace Common.Exceptions
{
    /// <summary>
    /// Indicates that the request could not be processed because its input was invalid.
    /// </summary>
    /// <param name="message">The explanation associated with the exception.</param>
    public sealed class HttpBadRequestException(string message) : Exception(message)
    {
    }
}
