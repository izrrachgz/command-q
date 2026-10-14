# command-q

Reusable .NET 10 application-building libraries shared by the CommandQ applications.
The product backend is maintained separately in
[`command-q-back`](https://github.com/izrrachgz/command-q-back).

## Libraries

- [`CommandQ.Common`](Common/README.md): general utilities, caching, task execution and shared models.
- [`CommandQ.Cqrs`](Cqrs/README.md): explicit, AOT-friendly in-process request dispatching.
- [`CommandQ.Localization`](Localization/README.md): application-owned RESX and JSON localization composition.
- [`CommandQ.Persistence`](Persistence/README.md): provider-neutral SqlKata repositories and database adapters.
- [`CommandQ.Time`](Time/README.md): UTC and timezone primitives built on NodaTime.

Applications consume versioned NuGet packages written to the sibling
`command-q-build-output` local package feed.

## Build and test

```powershell
dotnet restore .\command-q.sln
.\scripts\build.ps1 -Configuration Release
.\scripts\test.ps1 -Configuration Release -NoBuild
```

## NuGet package creation

This repository is the source of the five `CommandQ.*` NuGet packages. Package creation is
explicit: a normal Visual Studio **Build Solution** or `dotnet build` only compiles the
projects, even when the selected configuration is Release. There is no package-on-build
profile.

Use the repository build script to compile and package the complete solution:

```powershell
.\scripts\build.ps1 -Configuration Release
```

The script performs these operations in order:

1. `dotnet restore`, unless `-NoRestore` is supplied.
2. `dotnet build` using the requested configuration.
3. `dotnet pack --no-build` using the same configuration.

Always pass `-Configuration Release` when producing packages for applications. The script's
default configuration remains Debug for local development.

`Directory.Build.props` marks only `Common`, `Cqrs`, `Localization`, `Persistence` and `Time`
as packable. It also defines their `CommandQ.*` package IDs, shared version and package
metadata. Test projects are not packaged.

Generated `.nupkg` files are written outside this repository to the sibling local feed:

```text
..\command-q-build-output\CommandQ.Common.0.3.1.nupkg
..\command-q-build-output\CommandQ.Cqrs.0.3.1.nupkg
..\command-q-build-output\CommandQ.Localization.0.3.1.nupkg
..\command-q-build-output\CommandQ.Persistence.0.3.1.nupkg
..\command-q-build-output\CommandQ.Time.0.3.1.nupkg
```

Consuming repositories reference that directory as a local NuGet source during development.
The repository README, package configuration and build/test scripts are visible under the
solution's `Solution Items` folder in Visual Studio.

## Logging event IDs

Reusable package event IDs keep the same meaning in every consuming application.
`CommandQ.Common` owns `300000-300999`, while `CommandQ.Persistence` owns
`400000-400999`. Tests enforce uniqueness and range membership.

The cross-repository registry is documented in
[`docs/logging-event-ids.md`](docs/logging-event-ids.md) and is visible under
Solution Items. Application repositories own the detailed module allocation
inside their reserved ranges.

## Cancellation

Async APIs propagate caller cancellation through CQRS, task execution, HTTP, and persistence operations. Cancellation
is reported with `OperationCanceledException`, while any required I/O cleanup uses a separately bounded token. The
implementation rules and review checklist are documented in [`docs/cancellation.md`](docs/cancellation.md).

## Localization

The localization package contains no application text. Applications register their own
supported cultures, RESX catalogs and JSON locale files:

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

`ITextProvider` resolves sources in the configured provider order and returns the key when
no registered source contains it.

## Project documentation

Every library and its matching test project owns a `README.md`. Library readmes are also
embedded in their NuGet packages, so package documentation stays specific to the API being
consumed.
