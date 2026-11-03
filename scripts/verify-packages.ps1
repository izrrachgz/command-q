[CmdletBinding()]
param(
    [string]$PackageDirectory = "",
    [string]$Version = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($PackageDirectory)) {
    $PackageDirectory = Join-Path (Split-Path -Parent $repositoryRoot) "command-q-build-output"
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    [xml]$buildProps = Get-Content -LiteralPath (Join-Path $repositoryRoot "Directory.Build.props")
    $versionNode = $buildProps.SelectSingleNode("/*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='Version']")
    if ($null -eq $versionNode -or [string]::IsNullOrWhiteSpace($versionNode.InnerText)) {
        throw "The package version could not be read from Directory.Build.props."
    }

    $Version = $versionNode.InnerText.Trim()
}

if (-not (Test-Path -LiteralPath $PackageDirectory -PathType Container)) {
    throw "Package directory not found: $PackageDirectory"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

$expectedIds = @(
    "CommandQ.Common",
    "CommandQ.Cqrs",
    "CommandQ.Localization",
    "CommandQ.Persistence",
    "CommandQ.Time")
$verifiedArtifacts = [System.Collections.Generic.List[System.IO.FileInfo]]::new()

foreach ($packageId in $expectedIds) {
    $assemblyName = $packageId.Substring("CommandQ.".Length)
    $packagePath = Join-Path $PackageDirectory "$packageId.$Version.nupkg"
    $symbolPackagePath = Join-Path $PackageDirectory "$packageId.$Version.snupkg"

    foreach ($requiredPath in @($packagePath, $symbolPackagePath)) {
        if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
            throw "Required package artifact not found: $requiredPath"
        }
    }

    $packageArchive = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
    try {
        $nuspecEntries = @($packageArchive.Entries | Where-Object { $_.FullName -like "*.nuspec" })
        if ($nuspecEntries.Count -ne 1) {
            throw "Package $packageId must contain exactly one nuspec file."
        }

        $nuspecEntry = $nuspecEntries[0]
        $reader = [System.IO.StreamReader]::new($nuspecEntry.Open())
        try {
            [xml]$nuspec = $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }

        $metadata = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']")
        $actualId = $metadata.SelectSingleNode("*[local-name()='id']").InnerText
        $actualVersion = $metadata.SelectSingleNode("*[local-name()='version']").InnerText
        $description = $metadata.SelectSingleNode("*[local-name()='description']").InnerText
        $repository = $metadata.SelectSingleNode("*[local-name()='repository']")

        if ($actualId -ne $packageId -or $actualVersion -ne $Version) {
            throw "Unexpected package identity in ${packagePath}: $actualId $actualVersion."
        }

        if ([string]::IsNullOrWhiteSpace($description) -or $description -eq "Package Description") {
            throw "Package $packageId has no meaningful description."
        }

        if ($null -eq $repository -or
            $repository.GetAttribute("type") -ne "git" -or
            $repository.GetAttribute("url") -ne "https://github.com/izrrachgz/command-q" -or
            [string]::IsNullOrWhiteSpace($repository.GetAttribute("commit"))) {
            throw "Package $packageId is missing repository URL or commit provenance."
        }

        $entryNames = $packageArchive.Entries.FullName
        if ($entryNames -notcontains "README.md") {
            throw "Package $packageId does not contain its README.md."
        }

        if ($entryNames -notcontains "lib/net10.0/$assemblyName.dll") {
            throw "Package $packageId does not contain lib/net10.0/$assemblyName.dll."
        }

        if ($entryNames | Where-Object { $_ -like "*.pdb" }) {
            throw "Package $packageId contains a PDB in the primary package instead of its symbol package."
        }
    }
    finally {
        $packageArchive.Dispose()
    }

    $symbolArchive = [System.IO.Compression.ZipFile]::OpenRead($symbolPackagePath)
    try {
        if ($symbolArchive.Entries.FullName -notcontains "lib/net10.0/$assemblyName.pdb") {
            throw "Symbol package $packageId does not contain lib/net10.0/$assemblyName.pdb."
        }
    }
    finally {
        $symbolArchive.Dispose()
    }

    $verifiedArtifacts.Add((Get-Item -LiteralPath $packagePath))
    $verifiedArtifacts.Add((Get-Item -LiteralPath $symbolPackagePath))
}

$unexpectedPackages = Get-ChildItem -LiteralPath $PackageDirectory -File |
    Where-Object { $_.Extension -in ".nupkg", ".snupkg" } |
    Where-Object { $_.FullName -notin $verifiedArtifacts.FullName }
if ($unexpectedPackages) {
    throw "Unexpected package artifacts found: $($unexpectedPackages.Name -join ', ')."
}

$checksumLines = $verifiedArtifacts |
    Sort-Object Name |
    ForEach-Object {
        $hash = Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256
        "$($hash.Hash.ToLowerInvariant())  $($_.Name)"
    }
$checksumPath = Join-Path $PackageDirectory "SHA256SUMS"
[System.IO.File]::WriteAllLines($checksumPath, $checksumLines, [System.Text.UTF8Encoding]::new($false))

Write-Output "Verified $($expectedIds.Count) NuGet packages, symbol packages, provenance metadata and SHA-256 checksums."
