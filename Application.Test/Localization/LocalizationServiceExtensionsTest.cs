using Localization.Models;
using System.Globalization;
using Localization.Extensions;
using Localization.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Test.Localization
{
    public class LocalizationServiceExtensionsTest
    {
        [Fact]
        public void ITextProvider_Should_Use_Current_Ui_Culture_Per_Scope()
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUiCulture = CultureInfo.CurrentUICulture;
            var services = new ServiceCollection();

            services.AddLocalizationServices();

            using var provider = services.BuildServiceProvider();

            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

                using var englishScope = provider.CreateScope();
                var english = englishScope.ServiceProvider.GetRequiredService<ITextProvider>();

                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es-ES");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("es-ES");

                using var spanishScope = provider.CreateScope();
                var spanish = spanishScope.ServiceProvider.GetRequiredService<ITextProvider>();

                Assert.Equal("Record not found.", english.Get("Record_Not_Found"));
                Assert.Equal("No se ha encontrado el registro.", spanish.Get("Record_Not_Found"));
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUiCulture;
            }
        }

        [Fact]
        public void ITextProvider_Should_Use_Fallback_Culture_When_Current_Culture_Is_Not_Supported()
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUiCulture = CultureInfo.CurrentUICulture;
            var services = new ServiceCollection();

            services.AddLocalizationServices(options =>
            {
                options.FallbackCulture = "en-US";
                options.SupportedCultures = ["en-US", "es-ES"];
            });

            using var provider = services.BuildServiceProvider();

            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");

                using var scope = provider.CreateScope();
                var textProvider = scope.ServiceProvider.GetRequiredService<ITextProvider>();

                Assert.Equal("Record not found.", textProvider.Get("Record_Not_Found"));
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUiCulture;
            }
        }

        [Fact]
        public void LocalizationOptions_Should_Use_Json_Provider_First_By_Default()
        {
            var options = new LocalizationOptions();

            Assert.Equal(LocalizationProviderType.Json, options.ProviderOrder[0]);
            Assert.Equal(LocalizationProviderType.Resx, options.ProviderOrder[1]);
        }
    }
}
