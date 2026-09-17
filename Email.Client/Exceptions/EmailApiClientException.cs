namespace Email.Client.Exceptions;

/// <summary>Indicates that an authenticated Email API operation failed before a valid response was received.</summary>
public sealed class EmailApiClientException : Exception
{
    /// <summary>Initializes an Email API client exception.</summary>
    /// <param name="message">A description of the failed operation.</param>
    /// <param name="innerException">The underlying client, protocol or response error.</param>
    public EmailApiClientException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
