# CommandQ.Localization

`CommandQ.Localization` composes application-owned RESX and JSON text sources behind `ITextProvider`. The package contains the mechanism only: it embeds no product messages, validation text, field names, JSON catalogs or fixed list of application languages.

## Install

```powershell
dotnet add package CommandQ.Localization
```

## Register application resources

The consuming application defines its cultures and registers only the sources it owns:

```csharp
services
    .AddCommandQLocalization(options =>
    {
        options.DefaultCulture = "en-US";
        options.FallbackCulture = "en-US";
        options.SupportedCultures = ["en-US", "es-ES"];
    })
    .AddJsonFiles("locales")
    .AddResx(Messages.ResourceManager)
    .AddResx(ValidationText.ResourceManager)
    .AddResx(FieldNames.ResourceManager);
```

Configuration binding is also supported:

```csharp
services
    .AddCommandQLocalization(configuration.GetSection("Localization"))
    .AddJsonFiles()
    .AddResx(Messages.ResourceManager);
```

Example configuration:

```json
{
  "Localization": {
    "DefaultCulture": "en-US",
    "FallbackCulture": "en-US",
    "SupportedCultures": ["en-US", "es-ES"],
    "JsonLocalesPath": "locales",
    "ProviderOrder": ["Json", "Resx"]
  }
}
```

## Resolution behavior

- `CompositeTextProvider` evaluates sources in `LocalizationOptions.ProviderOrder`.
- `JsonTextProvider` reads application JSON files by culture and caches each culture/path catalog.
- `ResxTextProvider` reads the application-provided `ResourceManager` instances.
- Resolution uses the active `CurrentUICulture`, then configured fallback behavior.
- Missing keys return the key itself, making unresolved text visible without throwing.
- Format arguments are applied to the resolved value.

`ITextProvider` and `ValidationTextProvider` are scoped so they follow the active request culture. The host remains responsible for configuring request-culture middleware or setting the culture for non-HTTP work.

## Resource ownership

Applications decide which languages they support and maintain all `.resx` and JSON values. Add every user-facing key to every supported application language. This package should remain reusable even when two consumers use completely different cultures and resource layouts.

## Tests

Representative RESX and JSON composition tests live in [`Localization.Test`](../Localization.Test/README.md).
