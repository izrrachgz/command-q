param(
    [Parameter(Mandatory)]
    [ValidatePattern("^[A-Za-z][A-Za-z0-9_]*$")]
    [string]$Name
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "IdentityServer\IdentityServer.csproj"
$providers = @(
    @{
        Context = "PgSqlIdentityServerDbContext"
        OutputDirectory = "Persistence\Migrations\PgSql"
    },
    @{
        Context = "MsSqlIdentityServerDbContext"
        OutputDirectory = "Persistence\Migrations\MsSql"
    },
    @{
        Context = "MySqlIdentityServerDbContext"
        OutputDirectory = "Persistence\Migrations\MySql"
    },
    @{
        Context = "SqLiteIdentityServerDbContext"
        OutputDirectory = "Persistence\Migrations\SqLite"
    }
)

Push-Location $root

try {
    dotnet tool restore

    if ($LASTEXITCODE -ne 0) {
        throw "Unable to restore the repository-local .NET tools."
    }

    dotnet build $project

    if ($LASTEXITCODE -ne 0) {
        throw "IdentityServer must build before migrations can be generated."
    }

    foreach ($provider in $providers) {
        dotnet tool run dotnet-ef migrations add $Name `
            --project $project `
            --startup-project $project `
            --context $provider.Context `
            --output-dir $provider.OutputDirectory `
            --no-build

        if ($LASTEXITCODE -ne 0) {
            throw "Migration generation failed for $($provider.Context)."
        }
    }
}
finally {
    Pop-Location
}
