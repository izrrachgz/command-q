using Email.Client.Abstractions;
using Email.Client.Configuration;
using Email.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Email.Client.Extensions;

public static class EmailApiClientExtensions
{
    /// <summary>Registers the authenticated protobuf email queue client.</summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The host-relative configuration containing an EmailApiClient section.</param>
    /// <returns>The supplied service collection.</returns>
    public static IServiceCollection AddEmailApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        IConfigurationSection section = configuration.GetSection(EmailQueueConstants.ConfigurationSection);
        services.AddOptions<EmailApiClientOptions>().Bind(section).ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<EmailApiClientOptions>, EmailApiClientOptionsValidator>());
        services.AddHttpClient(EmailQueueClient.HttpClientName, (provider, client) =>
        {
            EmailApiClientOptions options = provider.GetRequiredService<IOptions<EmailApiClientOptions>>().Value;
            client.BaseAddress = options.BaseUrl;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IEmailQueueClient, EmailQueueClient>();
        return services;
    }
}
