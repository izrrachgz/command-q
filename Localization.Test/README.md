# Localization.Test

`Localization.Test` verifies that [`CommandQ.Localization`](../Localization/README.md) remains generic while correctly composing application-owned resources.

## Scope

- Fluent and configuration-based service registration.
- JSON catalogs under `Locales` and test-owned RESX resources under `Resources`.
- Current UI culture, supported cultures and fallback behavior.
- Provider ordering, formatting and missing-key behavior.
- Scoped `ITextProvider` and `ValidationTextProvider` resolution.

The resources in this project are test fixtures, not content shipped by the localization package. Add representative fixtures for new mechanisms without introducing application catalogs into `Localization`.

## Run

```powershell
dotnet test .\Localization.Test\Localization.Test.csproj
```
