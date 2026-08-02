param(
    [string]$Environment = "Development",
    [string]$Urls = "",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "Api\Api.csproj"
$env:ASPNETCORE_ENVIRONMENT = $Environment

if (-not [string]::IsNullOrWhiteSpace($Urls)) {
    $env:ASPNETCORE_URLS = $Urls
}

$arguments = @("run", "--project", $project)

if ($NoBuild) {
    $arguments += "--no-build"
}

dotnet @arguments
