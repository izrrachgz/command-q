using Localization.Configuration;
using Localization.Models;
using System.Globalization;
using Localization.Providers;
using Localization.Abstractions;
using Microsoft.Extensions.DependencyInjection;

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
