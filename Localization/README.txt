Project: Localization
Purpose: Resources and translations to support multiple languages.
Framework: .NET 10 (resource library).
How to use: Add/update resources in `.resx` files and rebuild; inject localization services in `Api`/UI.
Resource groups:
- Messages: user-facing operation messages.
- ValidationText: reusable validation message templates.
- FieldNames: localized field display names used by validators.
Providers:
- ResxTextProvider reads operation messages from resx resources.
- ValidationTextProvider reads validator messages and field names from resx resources.
- CompositeTextProvider resolves text through registered providers.
- JsonTextProvider remains available for JSON-backed text lookup.
Conventions: Use clear and consistent resource keys; add localized values for every supported language.
