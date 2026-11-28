[CmdletBinding()]
param(
    [string]$PackageDirectory = "",
    [string]$Version = "",
    [string]$ExpectedCommit = "",
    [switch]$ExactSet
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

$checkoutCommit = git -C $repositoryRoot rev-parse HEAD
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($checkoutCommit)) {
    throw "Could not determine the source commit for package verification."
}
if ([string]::IsNullOrWhiteSpace($ExpectedCommit)) {
    $ExpectedCommit = $checkoutCommit
}
if ($checkoutCommit -cne $ExpectedCommit) {
    throw "The checkout moved from the package build revision $ExpectedCommit to $checkoutCommit."
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Read-PackageEntry($entry) {
    $reader = [System.IO.StreamReader]::new($entry.Open())
    try {
        return $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
    }
}

function Read-PackageMetadata($archive, [string]$packageId) {
    $nuspecEntries = @($archive.Entries | Where-Object { $_.FullName -like "*.nuspec" })
    if ($nuspecEntries.Count -ne 1) {
        throw "Archive for $packageId must contain exactly one nuspec file."
    }

    [xml]$nuspec = Read-PackageEntry $nuspecEntries[0]
    $metadata = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']")
    $actualId = $metadata.SelectSingleNode("*[local-name()='id']").InnerText
    $actualVersion = $metadata.SelectSingleNode("*[local-name()='version']").InnerText
    $repository = $metadata.SelectSingleNode("*[local-name()='repository']")

    if ($actualId -ne $packageId -or $actualVersion -ne $Version) {
        throw "Expected $packageId $Version but found $actualId $actualVersion."
    }

    if ($null -eq $repository -or
        $repository.GetAttribute("type") -ne "git" -or
        $repository.GetAttribute("url") -ne "https://github.com/izrrachgz/command-q" -or
        $repository.GetAttribute("commit") -cne $ExpectedCommit) {
        throw "Archive for $packageId does not match the expected repository and source commit $ExpectedCommit."
    }

    return $metadata
}

$expectedIds = @(
    "CommandQ.Common",
    "CommandQ.Cqrs",
    "CommandQ.Localization",
    "CommandQ.Messaging",
    "CommandQ.Messaging.Wolverine.RabbitMQ",
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
        $metadata = Read-PackageMetadata $packageArchive $packageId
        $description = $metadata.SelectSingleNode("*[local-name()='description']").InnerText

        if ([string]::IsNullOrWhiteSpace($description) -or $description -eq "Package Description") {
            throw "Package $packageId has no meaningful description."
        }

        $readmeEntries = @($packageArchive.Entries | Where-Object { $_.FullName -ceq "README.md" })
        if ($readmeEntries.Count -ne 1) {
            throw "Package $packageId must contain exactly one root README.md."
        }
        $projectDirectory = $assemblyName
        if ($packageId -eq "CommandQ.Messaging.Wolverine.RabbitMQ") {
            $projectDirectory = "Messaging.Wolverine.RabbitMq"
        }
        $readmePath = Join-Path $repositoryRoot "$projectDirectory/README.md"
        $expectedReadme = [IO.File]::ReadAllText($readmePath).Replace("`r`n", "`n")
        $actualReadme = (Read-PackageEntry $readmeEntries[0]).Replace("`r`n", "`n")
        if ($actualReadme -cne $expectedReadme) {
            throw "Package $packageId README.md does not match $projectDirectory/README.md."
        }

        $entryNames = $packageArchive.Entries.FullName

        foreach ($targetFramework in @("net10.0", "net11.0")) {
            if ($entryNames -notcontains "lib/$targetFramework/$assemblyName.dll") {
                throw "Package $packageId does not contain lib/$targetFramework/$assemblyName.dll."
            }
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
        $null = Read-PackageMetadata $symbolArchive $packageId
        foreach ($targetFramework in @("net10.0", "net11.0")) {
            if ($symbolArchive.Entries.FullName -notcontains "lib/$targetFramework/$assemblyName.pdb") {
                throw "Symbol package $packageId does not contain lib/$targetFramework/$assemblyName.pdb."
            }
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
    Where-Object {
        $_.FullName -notin $verifiedArtifacts.FullName -and
        ($ExactSet -or $_.Name.EndsWith(".$Version$($_.Extension)", [StringComparison]::OrdinalIgnoreCase))
    }
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
