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
            var options = new LocalizationOptions();

            configure?.Invoke(options);

            services.AddSingleton(options);
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

        private static IEnumerable<ITextProvider> CreateProviders(CultureInfo culture, LocalizationOptions options)
        {
            var fallbackCulture = CreateFallbackCulture(options);

            foreach (var provider in options.ProviderOrder)
            {
                yield return provider switch
                {
                    LocalizationProviderType.Json => new JsonTextProvider(culture.Name, options),
                    LocalizationProviderType.Resx => new ResxTextProvider(culture, fallbackCulture),
                    _ => new ResxTextProvider(culture, fallbackCulture)
                };
            }
        }

        private static CultureInfo CreateFallbackCulture(LocalizationOptions options)
        {
            return CultureInfo.GetCultureInfo(options.FallbackCulture);
        }

        private static CultureInfo ResolveCulture(CultureInfo culture, LocalizationOptions options)
        {
            if (options.SupportedCultures.Any(supportedCulture => supportedCulture.Equals(culture.Name, StringComparison.OrdinalIgnoreCase)))
                return culture;

            if (options.SupportedCultures.Any(supportedCulture => supportedCulture.Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase)))
                return culture;

            return CreateFallbackCulture(options);
        }
    }
}
