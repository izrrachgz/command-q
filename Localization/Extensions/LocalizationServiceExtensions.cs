using System.Globalization;
using Localization.Abstractions;
using Localization.Configuration;
using Localization.DependencyInjection;
using Localization.Models;
using Localization.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Localization.Extensions
{
    /// <summary>
    /// Registers the generic localization mechanism. Applications register their own
    /// RESX and JSON resources through the returned <see cref="LocalizationBuilder"/>.
    /// </summary>
    public static class LocalizationServiceExtensions
    {
        public static LocalizationBuilder AddCommandQLocalization(
            this IServiceCollection services,
            Action<LocalizationOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            var options = new LocalizationOptions();
            configure?.Invoke(options);
            Validate(options);

            services.AddSingleton(options);
            AddCoreServices(services);
            return new LocalizationBuilder(services);
        }

        public static LocalizationBuilder AddCommandQLocalization(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddOptions<LocalizationOptions>()
                .Bind(configuration)
                .Validate(
                    options => options.SupportedCultures.Length > 0,
                    "Localization:SupportedCultures must contain at least one culture.")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.DefaultCulture)
                        && options.SupportedCultures.Contains(options.DefaultCulture, StringComparer.OrdinalIgnoreCase),
                    "Localization:DefaultCulture must be one of the supported cultures.")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.FallbackCulture),
                    "Localization:FallbackCulture is required.")
                .ValidateOnStart();

            AddCoreServices(services);
            return new LocalizationBuilder(services);
        }

        private static void AddCoreServices(IServiceCollection services)
        {
            services.TryAddScoped<ITextProvider>(serviceProvider =>
            {
                var options = ResolveOptions(serviceProvider);
                var primaryCulture = ResolveCulture(CultureInfo.CurrentUICulture, options);
                var fallbackCulture = CultureInfo.GetCultureInfo(options.FallbackCulture);
                var providers = CreateProviders(serviceProvider, primaryCulture, fallbackCulture, options);
                return new CompositeTextProvider(providers);
            });

            services.TryAddScoped<ValidationTextProvider>();
        }

        private static IEnumerable<ITextProvider> CreateProviders(
            IServiceProvider serviceProvider,
            CultureInfo primaryCulture,
            CultureInfo fallbackCulture,
            LocalizationOptions options)
        {
            var resxResources = serviceProvider
                .GetServices<ResxLocalizationSourceRegistration>()
                .Select(registration => registration.ResourceManager)
                .ToArray();
            var jsonRegistrations = serviceProvider
                .GetServices<JsonLocalizationSourceRegistration>()
                .ToArray();

            foreach (var providerType in options.ProviderOrder)
            {
                if (providerType == LocalizationProviderType.Json)
                {
                    foreach (var registration in jsonRegistrations)
                    {
                        var providerOptions = CopyOptions(options);
                        if (!string.IsNullOrWhiteSpace(registration.LocalesPath))
                            providerOptions.JsonLocalesPath = registration.LocalesPath;

                        yield return new JsonTextProvider(primaryCulture.Name, providerOptions);
                    }
                }
                else if (providerType == LocalizationProviderType.Resx && resxResources.Length > 0)
                {
                    yield return new ResxTextProvider(resxResources, primaryCulture, fallbackCulture);
                }
            }
        }

        private static LocalizationOptions ResolveOptions(IServiceProvider serviceProvider)
        {
            var monitor = serviceProvider.GetService<IOptionsMonitor<LocalizationOptions>>();
            return monitor?.CurrentValue
                ?? serviceProvider.GetRequiredService<LocalizationOptions>();
        }

        private static CultureInfo ResolveCulture(CultureInfo requestedCulture, LocalizationOptions options)
        {
            if (options.SupportedCultures.Contains(requestedCulture.Name, StringComparer.OrdinalIgnoreCase))
                return requestedCulture;

            if (options.SupportedCultures.Contains(requestedCulture.TwoLetterISOLanguageName, StringComparer.OrdinalIgnoreCase))
                return CultureInfo.GetCultureInfo(requestedCulture.TwoLetterISOLanguageName);

            return CultureInfo.GetCultureInfo(options.DefaultCulture);
        }

        private static LocalizationOptions CopyOptions(LocalizationOptions options)
        {
            return new LocalizationOptions
            {
                DefaultCulture = options.DefaultCulture,
                FallbackCulture = options.FallbackCulture,
                SupportedCultures = [.. options.SupportedCultures],
                JsonLocalesPath = options.JsonLocalesPath,
                ProviderOrder = [.. options.ProviderOrder]
            };
        }

        private static void Validate(LocalizationOptions options)
        {
            if (options.SupportedCultures.Length == 0)
                throw new ArgumentException("At least one supported culture is required.", nameof(options));
            if (string.IsNullOrWhiteSpace(options.DefaultCulture)
                || !options.SupportedCultures.Contains(options.DefaultCulture, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("The default culture must be supported.", nameof(options));
            if (string.IsNullOrWhiteSpace(options.FallbackCulture))
                throw new ArgumentException("A fallback culture is required.", nameof(options));
        }
    }
}
