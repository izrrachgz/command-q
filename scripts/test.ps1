param(
    [string]$Configuration = "Debug",
    [string]$Project = "command-q.sln",
    [string]$Filter = "",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$target = Join-Path $root $Project
$arguments = @("test", $target, "--configuration", $Configuration)

if ($NoBuild) {
    $arguments += "--no-build"
}

if (-not [string]::IsNullOrWhiteSpace($Filter)) {
    $arguments += @("--filter", $Filter)
}

dotnet @arguments
