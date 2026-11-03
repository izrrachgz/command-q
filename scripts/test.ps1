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
$targetExtension = [System.IO.Path]::GetExtension($target)

$arguments = switch ($targetExtension) {
    ".sln" { @("test", "--solution", $target, "--configuration", $Configuration) }
    ".csproj" { @("test", "--project", $target, "--configuration", $Configuration) }
    default { throw "Project must identify a .sln or .csproj file." }
}

if ($NoBuild) {
    $arguments += "--no-build"
}

if (-not [string]::IsNullOrWhiteSpace($Filter)) {
    $arguments += @("--", "--filter-method", $Filter)
}

dotnet @arguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet $($arguments -join ' ') failed with exit code $LASTEXITCODE."
}
