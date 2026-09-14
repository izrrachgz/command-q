namespace Email.Client.Configuration;

/// <summary>Configures authenticated access to the command-emails protobuf API.</summary>
public sealed class EmailApiClientOptions
{
    /// <summary>Gets or sets whether queue operations are enabled.</summary>
    public bool Enabled { get; set; } = true;
    /// <summary>Gets or sets the absolute Email API base URL.</summary>
    public Uri BaseUrl { get; set; } = new("https://localhost:7325");
    /// <summary>Gets or sets the required API-client header name.</summary>
    public string ApiClientHeaderName { get; set; } = string.Empty;
    /// <summary>Gets or sets the API-client header credential.</summary>
    public string ApiClientKey { get; set; } = string.Empty;
    /// <summary>Gets or sets the Email API service-account username.</summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>Gets or sets the Email API service-account password.</summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>Gets or sets the HTTP and gRPC timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 30;
}
