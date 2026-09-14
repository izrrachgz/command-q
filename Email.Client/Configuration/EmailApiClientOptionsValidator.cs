using Microsoft.Extensions.Options;

namespace Email.Client.Configuration;

internal sealed class EmailApiClientOptionsValidator : IValidateOptions<EmailApiClientOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailApiClientOptions options)
    {
        if (!options.Enabled) return ValidateOptionsResult.Success;
        if (!options.BaseUrl.IsAbsoluteUri || (options.BaseUrl.Scheme != Uri.UriSchemeHttp && options.BaseUrl.Scheme != Uri.UriSchemeHttps))
            return ValidateOptionsResult.Fail("EmailApiClient:BaseUrl must be an absolute HTTP or HTTPS URL.");
        if (string.IsNullOrWhiteSpace(options.ApiClientHeaderName) || string.IsNullOrWhiteSpace(options.ApiClientKey))
            return ValidateOptionsResult.Fail("EmailApiClient client header name and key are required.");
        if (string.IsNullOrWhiteSpace(options.UserName) || string.IsNullOrWhiteSpace(options.Password))
            return ValidateOptionsResult.Fail("EmailApiClient credentials are required.");
        return options.TimeoutSeconds > 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("EmailApiClient:TimeoutSeconds must be greater than zero.");
    }
}
