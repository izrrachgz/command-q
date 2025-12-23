using System.Globalization;
using Localization.Providers;
using Localization.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Localization.Extensions
{
    public static class DILocalizationExtensions
    {
        public static void AddLocalizationServices(this IServiceCollection services)
        {
            services.AddSingleton<ITextProvider>(sp =>
            {
                return new CompositeTextProvider(
                [
                    new JsonTextProvider(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName),
                    new ResourceTextProvider()
                ]);
            });
        }
    }
}
