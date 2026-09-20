# Common.Test

`Common.Test` contains focused xUnit tests for [`CommandQ.Common`](../Common/README.md).

## Scope

- `TaskRunner` bounded concurrency, failure handling and dependency-injection registration.
- String and enum conversion rules, including absolute URI validation.
- Add tests here for changes to common extensions, typed caching, HTTP helpers or shared models.

Tests should be deterministic and isolated from network services.

## Run

```powershell
dotnet test .\Common.Test\Common.Test.csproj
```

From the repository script:

```powershell
.\scripts\test.ps1 -Project Common.Test\Common.Test.csproj
```
