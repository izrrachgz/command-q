# CommandQ testing

Use a package's matching test project and select tests by caller behavior. The pinned SDK uses Microsoft.Testing.Platform and the test script passes xUnit method patterns through `-Filter`.

~~~powershell
.\scripts\test.ps1 -Project Cqrs.Test\Cqrs.Test.csproj -Filter "*RequestDispatcherTest*"
.\scripts\test.ps1 -Configuration Release -NoBuild
~~~

Confirm that each selected suite finds tests. For shared behavior, use deterministic time, controlled cancellation and synthetic data. Run on both target frameworks. Keep the ordinary solution suite independent of application repositories, network services and brokers. A provider-specific SQL or broker-runtime claim needs separate evidence from an appropriate disposable local environment.

For library and packaging changes, the [quality workflow](../../.github/workflows/quality.yml) builds, verifies packages and then tests the Release binaries. For scaffold-only verification, build Release and pack explicitly to a new temporary directory to avoid replacing the shared feed:

~~~powershell
$packageCheckDir = Join-Path ([System.IO.Path]::GetTempPath()) ("command-q-package-check-" + [guid]::NewGuid())
dotnet build .\command-q.sln --configuration Release
dotnet pack .\command-q.sln --configuration Release --no-build --no-restore --output $packageCheckDir
.\scripts\verify-packages.ps1 -PackageDirectory $packageCheckDir
~~~

For packaging evidence, also compare each archived README with the corresponding project source when its identity or contents might be wrong. Report target frameworks, filter and counts, artifact checks and gaps.
