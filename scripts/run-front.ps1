param(
    [ValidateSet("Development", "Testing", "Staging", "Production")]
    [string]$Environment = "Development",
    [switch]$Install
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$frontPath = Join-Path $root "FrontApplication"
$packageJson = Join-Path $frontPath "package.json"

if (-not (Test-Path -LiteralPath $packageJson)) {
    Write-Host "FrontApplication is not present in this checkout."
    exit 0
}

Push-Location $frontPath
try {
    if ($Install -or -not (Test-Path -LiteralPath (Join-Path $frontPath "node_modules"))) {
        npm install
    }

    $scriptName = switch ($Environment) {
        "Testing" { "start:testing" }
        "Staging" { "start:staging" }
        "Production" { "start:production" }
        default { "start" }
    }

    npm run $scriptName
}
finally {
    Pop-Location
}
