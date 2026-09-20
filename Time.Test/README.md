# Time.Test

`Time.Test` verifies the UTC guarantees and architecture of [`CommandQ.Time`](../Time/README.md).

## Scope

- UTC validation and restoration after database reads.
- Persistence precision, formatting and parsing.
- `TimeProvider` dependency-injection registration and clock extensions.
- Named time-zone conversion, invalid zones and daylight-saving boundaries.
- Strict `DateTime` and `DateTimeOffset` JSON converters.
- Architecture rules that prevent direct system-clock access in reusable code.

Use deterministic timestamps and injected/fake `TimeProvider` instances when testing current-time behavior.

## Run

```powershell
dotnet test .\Time.Test\Time.Test.csproj
```

Run only architecture rules with:

```powershell
dotnet test .\Time.Test\Time.Test.csproj --filter FullyQualifiedName~TimeArchitectureTest
```
