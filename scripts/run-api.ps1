param(
    [string]$Environment = "Development",
    [string]$Urls = "https://localhost:7025;http://localhost:7000",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "Api\Api.csproj"
$env:ASPNETCORE_ENVIRONMENT = $Environment
$env:ASPNETCORE_URLS = $Urls

$arguments = @("run", "--project", $project, "--no-launch-profile")

if ($NoBuild) {
    $arguments += "--no-build"
}

dotnet @arguments
