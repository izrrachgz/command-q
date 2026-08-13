param(
    [string]$Environment = "Development",
    [string]$Urls = "http://localhost:7105",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "Email.Worker\Email.Worker.csproj"
$env:DOTNET_ENVIRONMENT = $Environment
$env:ASPNETCORE_ENVIRONMENT = $Environment
$env:ASPNETCORE_URLS = $Urls

$arguments = @("run", "--project", $project, "--no-launch-profile")

if ($NoBuild) {
    $arguments += "--no-build"
}

dotnet @arguments
