# Cqrs.Test

`Cqrs.Test` verifies the public behavior and dependency boundaries of [`CommandQ.Cqrs`](../Cqrs/README.md).

## Scope

- Explicit handler registration and scoped dispatch.
- Missing and duplicate handler failures.
- Pipeline ordering, short-circuiting and cancellation propagation.
- Command, query and `Unit` request shapes.
- Architecture rules that protect AOT compatibility and keep the package dependency-light.

When changing registration or dispatch semantics, cover both the successful path and composition/dispatch failures.

## Run

```powershell
dotnet test .\Cqrs.Test\Cqrs.Test.csproj
```

Run only architecture rules with:

```powershell
dotnet test .\Cqrs.Test\Cqrs.Test.csproj --filter FullyQualifiedName~CqrsArchitectureTest
```
