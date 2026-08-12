using Localization.Configuration;
using Localization.Models;
using System.Globalization;
using Localization.Providers;
using Localization.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Localization.Extensions
{
    /// <summary>
    /// Registers localization text providers.
    /// </summary>
    public static class LocalizationServiceExtensions
    {
        /// <summary>
        /// Registers scoped text providers that resolve values using the current UI culture for the active scope.
        /// </summary>
        /// <param name="services">Service collection to configure.</param>
        /// <param name="configure">Optional localization settings such as provider order, supported cultures and fallback culture.</param>
        public static IServiceCollection AddLocalizationServices(this IServiceCollection services, Action<LocalizationOptions>? configure = null)
        {
            var localizationOptions = new LocalizationOptions();

            configure?.Invoke(localizationOptions);

            services.AddSingleton(localizationOptions);
            services.AddScoped<ITextProvider>(serviceProvider =>
            {
                var localizationOptions = serviceProvider.GetRequiredService<LocalizationOptions>();

                return new CompositeTextProvider(CreateProviders(ResolveCulture(CultureInfo.CurrentUICulture, localizationOptions), localizationOptions));
            });

            services.AddScoped(serviceProvider =>
            {
                var localizationOptions = serviceProvider.GetRequiredService<LocalizationOptions>();

                return new ValidationTextProvider(ResolveCulture(CultureInfo.CurrentUICulture, localizationOptions), CreateFallbackCulture(localizationOptions));
            });

            return services;
        }

        /// <summary>
        /// Registers localization providers bound to reloadable configuration.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">The configuration section containing localization settings.</param>
        /// <returns>The supplied service collection.</returns>
        public static IServiceCollection AddLocalizationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<LocalizationOptions>()
                .Bind(configuration)
                .Validate(options => options.SupportedCultures.Length > 0, "Localization:SupportedCultures must contain at least one culture.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.FallbackCulture), "Localization:FallbackCulture is required.")
                .ValidateOnStart();

            services.AddScoped<ITextProvider>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptionsMonitor<LocalizationOptions>>().CurrentValue;
                var primaryCulture = ResolveCulture(CultureInfo.CurrentUICulture, options);

                return new CompositeTextProvider(CreateProviders(primaryCulture, options));
            });

            services.AddScoped(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptionsMonitor<LocalizationOptions>>().CurrentValue;

                return new ValidationTextProvider(ResolveCulture(CultureInfo.CurrentUICulture, options), CreateFallbackCulture(options));
            });

            return services;
        }

        private static IEnumerable<ITextProvider> CreateProviders(CultureInfo primaryCulture, LocalizationOptions localizationOptions)
        {
            var fallbackCulture = CreateFallbackCulture(localizationOptions);

            foreach (var providerType in localizationOptions.ProviderOrder)
            {
                yield return providerType switch
                {
                    LocalizationProviderType.Json => new JsonTextProvider(primaryCulture.Name, localizationOptions),
                    LocalizationProviderType.Resx => new ResxTextProvider(primaryCulture, fallbackCulture),
                    _ => new ResxTextProvider(primaryCulture, fallbackCulture)
                };
            }
        }

        private static CultureInfo CreateFallbackCulture(LocalizationOptions localizationOptions)
        {
            return CultureInfo.GetCultureInfo(localizationOptions.FallbackCulture);
        }

        private static CultureInfo ResolveCulture(CultureInfo requestedCulture, LocalizationOptions localizationOptions)
        {
            if (localizationOptions.SupportedCultures.Any(supportedCulture => supportedCulture.Equals(requestedCulture.Name, StringComparison.OrdinalIgnoreCase)))
                return requestedCulture;

            if (localizationOptions.SupportedCultures.Any(supportedCulture => supportedCulture.Equals(requestedCulture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase)))
                return requestedCulture;

            return CreateFallbackCulture(localizationOptions);
        }
    }
}
