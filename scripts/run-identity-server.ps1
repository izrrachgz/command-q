param(
    [string]$Environment = "Development",
    [string]$Urls = "https://localhost:7125",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "IdentityServer\IdentityServer.csproj"
$env:ASPNETCORE_ENVIRONMENT = $Environment
$env:ASPNETCORE_URLS = $Urls

$arguments = @("run", "--project", $project, "--no-launch-profile")

if ($NoBuild) {
    $arguments += "--no-build"
}

dotnet @arguments
