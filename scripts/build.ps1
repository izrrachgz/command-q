param(
    [string]$Configuration = "Debug",
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root "CommandQ.sln"

if (-not $NoRestore) {
    dotnet restore $solution
}

dotnet build $solution --configuration $Configuration --no-restore
