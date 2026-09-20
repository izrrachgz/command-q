using System.Globalization;
using System.Resources;
using Localization.Abstractions;
using Localization.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Localization.Test;

public sealed class LocalizationServiceExtensionsTest
{
    private static readonly ResourceManager TestResources = new(
        "Localization.Test.Resources.TestMessages",
        typeof(LocalizationServiceExtensionsTest).Assembly);

    [Theory]
    [InlineData("en-US", "Hello from JSON", "Hello from RESX")]
    [InlineData("es-ES", "Hola desde JSON", "Hola desde RESX")]
    public void Registered_Application_Resources_Use_Current_Ui_Culture(
        string cultureName,
        string expectedJson,
        string expectedResx)
    {
        using var provider = CreateProvider();
        var previousCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
            using var scope = provider.CreateScope();
            var text = scope.ServiceProvider.GetRequiredService<ITextProvider>();

            Assert.Equal(expectedJson, text.Get("Json_Greeting"));
            Assert.Equal(expectedResx, text.Get("Resx_Greeting"));
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public void Valid_Translation_Equal_To_Key_Is_Not_Treated_As_Missing()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();

        Assert.Equal("Same_As_Key", scope.ServiceProvider.GetRequiredService<ITextProvider>().Get("Same_As_Key"));
    }

    [Fact]
    public void Unsupported_Culture_Uses_Application_Default()
    {
        using var provider = CreateProvider();
        var previousCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            using var scope = provider.CreateScope();

            Assert.Equal(
                "Hello from JSON",
                scope.ServiceProvider.GetRequiredService<ITextProvider>().Get("Json_Greeting"));
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services
            .AddCommandQLocalization(options =>
            {
                options.DefaultCulture = "en-US";
                options.FallbackCulture = "en-US";
                options.SupportedCultures = ["en-US", "es-ES"];
            })
            .AddJsonFiles()
            .AddResx(TestResources);

        return services.BuildServiceProvider();
    }
}
