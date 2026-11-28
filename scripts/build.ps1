param(
    [string]$Configuration = "Debug",
    [switch]$NoRestore,
    [switch]$LockedMode,
    [switch]$Audit,
    [string]$PackageDirectory = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root "command-q.sln"
if ([string]::IsNullOrWhiteSpace($PackageDirectory)) {
    $PackageDirectory = Join-Path (Split-Path -Parent $root) "command-q-build-output"
}
$PackageDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PackageDirectory)

function Invoke-Dotnet {
    param([string[]]$DotnetArguments)

    dotnet @DotnetArguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($DotnetArguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Get-FeedMutexName([string]$path) {
    if ([IO.Path]::DirectorySeparatorChar -eq '\') {
        $path = $path.ToUpperInvariant()
    }

    $bytes = [Text.Encoding]::UTF8.GetBytes($path)
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try {
        return "command-q-pack-" + [BitConverter]::ToString($sha256.ComputeHash($bytes)).Replace("-", "")
    }
    finally {
        $sha256.Dispose()
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

$sourceCommit = git -C $root rev-parse HEAD
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sourceCommit)) {
    throw "Could not determine the source commit for the package build."
}

Invoke-Dotnet @("build", $solution, "--configuration", $Configuration, "--no-restore")

$staging = Join-Path (Split-Path -Parent $PackageDirectory) (".command-q-pack-" + [Guid]::NewGuid().ToString("N"))
[void][IO.Directory]::CreateDirectory($staging)
$preserveStaging = $false
$mutex = $null
$hasLock = $false

try {
    Invoke-Dotnet @("pack", $solution, "--configuration", $Configuration, "--no-restore", "--no-build", "-p:PackageOutputPath=$staging")
    & (Join-Path $PSScriptRoot "verify-packages.ps1") -PackageDirectory $staging -ExpectedCommit $sourceCommit -ExactSet

    $mutex = [Threading.Mutex]::new($false, (Get-FeedMutexName $PackageDirectory))
    try {
        $hasLock = $mutex.WaitOne([TimeSpan]::FromSeconds(30))
    }
    catch [Threading.AbandonedMutexException] {
        $hasLock = $true
    }
    if (-not $hasLock) {
        throw "Timed out waiting to update the local package feed at $PackageDirectory."
    }

    [void][IO.Directory]::CreateDirectory($PackageDirectory)
    $artifacts = @(Get-ChildItem -LiteralPath $staging -File | Where-Object Extension -in ".nupkg", ".snupkg" | Sort-Object Name)
    $artifacts += Get-Item -LiteralPath (Join-Path $staging "SHA256SUMS")
    $installed = [System.Collections.Generic.List[object]]::new()

    try {
        foreach ($artifact in $artifacts) {
            $destination = Join-Path $PackageDirectory $artifact.Name
            $backup = $artifact.FullName + ".previous"
            $replaced = [IO.File]::Exists($destination)
            if ($replaced) {
                [IO.File]::Replace($artifact.FullName, $destination, $backup)
            }
            else {
                [IO.File]::Move($artifact.FullName, $destination)
            }

            $installed.Add([pscustomobject]@{
                Source = $artifact.FullName
                Destination = $destination
                Backup = $backup
                Replaced = $replaced
            })
        }
    }
    catch {
        for ($i = $installed.Count - 1; $i -ge 0; $i--) {
            $item = $installed[$i]
            try {
                if ($item.Replaced) {
                    [IO.File]::Replace($item.Backup, $item.Destination, $item.Source)
                }
                else {
                    [IO.File]::Move($item.Destination, $item.Source)
                }
            }
            catch {
                $preserveStaging = $true
                Write-Warning "Could not restore $($item.Destination). Recovery files are in $staging."
            }
        }
        throw
    }

    Write-Output "Prepared verified packages in $PackageDirectory."
}
finally {
    if ($hasLock) {
        $mutex.ReleaseMutex()
    }
    if ($null -ne $mutex) {
        $mutex.Dispose()
    }
    if (-not $preserveStaging) {
        try {
            foreach ($file in Get-ChildItem -LiteralPath $staging -File -Force) {
                Remove-Item -LiteralPath $file.FullName
            }
            [IO.Directory]::Delete($staging)
        }
        catch {
            Write-Warning "Could not remove the package staging directory at $staging."
        }
    }
}
