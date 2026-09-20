# Persistence.Test

`Persistence.Test` contains isolated tests for provider-neutral behavior in [`CommandQ.Persistence`](../Persistence/README.md).

## Scope

- Search-term and sortable-column query composition.
- Safe fallback behavior for invalid or absent sort input.
- UTC date-range normalization across supported `DatabaseEngine` values.
- SQLite persistence values produced for provider-aware filters.

These tests intentionally avoid an external database. Add provider integration tests only when the behavior cannot be verified through query construction or an isolated local fixture.

## Run

```powershell
dotnet test .\Persistence.Test\Persistence.Test.csproj
```
