using System.Globalization;
using Localization.Providers;
using Localization.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Localization.Extensions
{
    public static class LocalizationServiceExtensions
    {
        public static void AddLocalizationServices(this IServiceCollection services)
        {
            //TODO: Consider adding configuration options for selecting providers and their order.
            //also consider adding caching layer if needed.
            //also consider adding support for changing culture at runtime.
            //also consider adding support for fallback cultures.
            //also add different instances for different scopes if needed.
            services.AddSingleton<ITextProvider>(sp =>
            {
                return new CompositeTextProvider(
                [
                    new ResxTextProvider(),
                    new JsonTextProvider(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                ]);
            });

            services.AddSingleton<ValidationTextProvider>();
        }
    }
}
