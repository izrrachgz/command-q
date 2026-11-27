param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path -LiteralPath $_ -PathType Container })]
    [string]$ConsumerRoot,
    [string]$PackageDirectory
)

$ErrorActionPreference = 'Stop'
$sharedRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($PackageDirectory)) {
    $PackageDirectory = Join-Path (Split-Path -Parent $sharedRoot) 'command-q-build-output'
}

$hashes = @{}
# CI builds the pinned internal packages from source. Refresh their artifact hashes
# so locked restore still checks versions, dependency graphs, and public package hashes.
foreach ($project in Get-ChildItem -LiteralPath $ConsumerRoot -Directory) {
    $lockPath = Join-Path $project.FullName 'packages.lock.json'
    if (-not (Test-Path -LiteralPath $lockPath -PathType Leaf)) { continue }

    $lock = ConvertFrom-Json ([System.IO.File]::ReadAllText($lockPath))
    $changed = $false
    foreach ($framework in $lock.dependencies.PSObject.Properties) {
        foreach ($package in $framework.Value.PSObject.Properties) {
            if (-not $package.Name.StartsWith('CommandQ.', [StringComparison]::OrdinalIgnoreCase)) { continue }

            $fileName = "$($package.Name).$($package.Value.resolved).nupkg"
            if (-not $hashes.ContainsKey($fileName)) {
                $stream = [System.IO.File]::OpenRead((Join-Path $PackageDirectory $fileName))
                $sha512 = [System.Security.Cryptography.SHA512]::Create()
                try {
                    $hashes[$fileName] = [Convert]::ToBase64String($sha512.ComputeHash($stream))
                }
                finally {
                    $sha512.Dispose()
                    $stream.Dispose()
                }
            }

            if ($package.Value.contentHash -ne $hashes[$fileName]) {
                $package.Value.contentHash = $hashes[$fileName]
                $changed = $true
            }
        }
    }

    if ($changed) {
        $json = ConvertTo-Json $lock -Depth 100
        [System.IO.File]::WriteAllText($lockPath, $json + [Environment]::NewLine, [System.Text.UTF8Encoding]::new($false))
    }
}

Write-Output "Matched $($hashes.Count) local CommandQ package hashes before locked restore."
