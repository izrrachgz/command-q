<#
.SYNOPSIS
Runs one of the command-q k6 benchmark scenarios.
#>
param(
    [Parameter(Mandatory)]
    [ValidateSet("ApiRead", "IdentitySignIn", "EmailRestEnqueue", "EmailGrpcEnqueue", "JobsRead")]
    [string]$Scenario,
    [ValidateSet("smoke", "ramp", "spike", "soak")]
    [string]$Mode = "smoke",
    [ValidateRange(1, 100000)]
    [int]$TargetVirtualUsers = 20,
    [string]$SummaryExport
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if (-not (Get-Command k6 -ErrorAction SilentlyContinue)) {
    throw "k6 is not installed or is not on PATH. See benchmarks/README.md for installation guidance."
}

$scenarioFiles = @{
    ApiRead = "api-read.js"
    IdentitySignIn = "identity-sign-in.js"
    EmailRestEnqueue = "email-rest-enqueue.js"
    EmailGrpcEnqueue = "email-grpc-enqueue.js"
    JobsRead = "jobs-read.js"
}

$previousMode = $env:MODE
$previousTarget = $env:TARGET_VUS
$env:MODE = $Mode
$env:TARGET_VUS = $TargetVirtualUsers.ToString([System.Globalization.CultureInfo]::InvariantCulture)
$scriptPath = Join-Path $PSScriptRoot "k6\$($scenarioFiles[$Scenario])"
$arguments = @("run")
if (-not [string]::IsNullOrWhiteSpace($SummaryExport)) {
    $summaryPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($SummaryExport)
    $summaryDirectory = Split-Path -Parent $summaryPath
    if (-not [string]::IsNullOrWhiteSpace($summaryDirectory)) {
        New-Item -ItemType Directory -Force -Path $summaryDirectory | Out-Null
    }
    $arguments += @("--summary-export", $summaryPath)
}
$arguments += $scriptPath

$commandQRoot = Split-Path -Parent $PSScriptRoot
try {
    Push-Location $commandQRoot
    k6 @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "k6 exited with code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
    $env:MODE = $previousMode
    $env:TARGET_VUS = $previousTarget
}
