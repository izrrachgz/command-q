param(
    [string]$Environment = "Development",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "Email.Worker\Email.Worker.csproj"
$env:DOTNET_ENVIRONMENT = $Environment

$arguments = @("run", "--project", $project, "--no-launch-profile")

if ($NoBuild) {
    $arguments += "--no-build"
}

dotnet @arguments
