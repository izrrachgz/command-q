param(
    [string]$Configuration = "Debug",
    [switch]$NoRestore,
    [switch]$LockedMode,
    [switch]$Audit
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root "command-q.sln"

function Invoke-Dotnet {
    param([string[]]$DotnetArguments)

    dotnet @DotnetArguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($DotnetArguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

if (-not $NoRestore) {
    $restoreArguments = @("restore", $solution)

    if ($LockedMode) {
        $restoreArguments += "--locked-mode"
    }

    if ($Audit) {
        $restoreArguments += @(
            "-p:NuGetAudit=true",
            "-p:NuGetAuditMode=all",
            "-warnaserror:NU1901,NU1902,NU1903,NU1904")
    }

    Invoke-Dotnet $restoreArguments
}

Invoke-Dotnet @("build", $solution, "--configuration", $Configuration, "--no-restore")
Invoke-Dotnet @("pack", $solution, "--configuration", $Configuration, "--no-restore", "--no-build")

& (Join-Path $PSScriptRoot "verify-packages.ps1")
if ($LASTEXITCODE -ne 0) {
    throw "Package verification failed with exit code $LASTEXITCODE."
}
