<#
.SYNOPSIS
Runs a k6 scenario while recording system-under-test and load-generator resource usage.
#>
param(
    [Parameter(Mandatory)]
    [ValidateSet("ApiRead", "IdentitySignIn", "EmailRestEnqueue", "EmailGrpcEnqueue", "JobsRead")]
    [string]$Scenario,

    [Parameter(Mandatory)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$ProcessId,

    [ValidateSet("smoke", "ramp", "spike", "soak")]
    [string]$Mode = "smoke",

    [ValidateRange(1, 100000)]
    [int]$TargetVirtualUsers = 20,

    [Parameter(Mandatory)]
    [string]$SummaryExport,

    [Parameter(Mandatory)]
    [string]$ResourceExport
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$k6Command = Get-Command k6 -ErrorAction SilentlyContinue
$installedK6Path = Join-Path $env:ProgramFiles "k6\k6.exe"
$k6Path = if (Test-Path -LiteralPath $installedK6Path) {
    $installedK6Path
}
elseif ($null -ne $k6Command) {
    $k6Command.Source
}
else {
    $null
}
if ([string]::IsNullOrWhiteSpace($k6Path) -or -not (Test-Path -LiteralPath $k6Path)) {
    throw "k6 is not installed or is not on PATH. See benchmarks/README.md for installation guidance."
}

$scenarioFiles = @{
    ApiRead = "api-read.js"
    IdentitySignIn = "identity-sign-in.js"
    EmailRestEnqueue = "email-rest-enqueue.js"
    EmailGrpcEnqueue = "email-grpc-enqueue.js"
    JobsRead = "jobs-read.js"
}

$sutProcess = Get-Process -Id $ProcessId
if ($sutProcess.ProcessName -ne "dotnet") {
    throw "Process $ProcessId is not a dotnet host."
}

$summaryPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($SummaryExport)
$resourcePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($ResourceExport)
$summaryDirectory = Split-Path -Parent $summaryPath
$resourceDirectory = Split-Path -Parent $resourcePath
New-Item -ItemType Directory -Force -Path $summaryDirectory, $resourceDirectory | Out-Null

$basePath = [IO.Path]::Combine(
    [IO.Path]::GetDirectoryName($summaryPath),
    [IO.Path]::GetFileNameWithoutExtension($summaryPath))
$stdoutPath = $basePath + ".stdout.log"
$stderrPath = $basePath + ".stderr.log"
$scriptPath = Join-Path $PSScriptRoot "k6\$($scenarioFiles[$Scenario])"
$arguments = @("run", "--summary-export", $summaryPath, $scriptPath)

$previousMode = $env:MODE
$previousTarget = $env:TARGET_VUS
$env:MODE = $Mode
$env:TARGET_VUS = $TargetVirtualUsers.ToString([Globalization.CultureInfo]::InvariantCulture)

try {
    if (Get-Process -Name "k6" -ErrorAction SilentlyContinue) {
        throw "Another k6 process is already running. Wait for it to finish before starting a measured pass."
    }

    $sutProcess.Refresh()
    $sutCpuStart = $sutProcess.TotalProcessorTime.TotalSeconds
    $sutCpuEnd = $sutCpuStart
    $sutPeakWorkingSet = $sutProcess.WorkingSet64
    $k6CpuSamples = @{}
    $k6CpuSeconds = 0.0
    $k6PeakWorkingSet = 0L
    $startedUtc = [DateTime]::UtcNow
    $completedUtc = $startedUtc
    $deadlineUtc = $startedUtc.AddHours(1)
    $observedK6 = $false
    $emptySamples = 0
    $launcher = Start-Process `
        -FilePath $k6Path `
        -ArgumentList $arguments `
        -WorkingDirectory (Split-Path -Parent $PSScriptRoot) `
        -RedirectStandardOutput $stdoutPath `
        -RedirectStandardError $stderrPath `
        -WindowStyle Hidden `
        -PassThru

    while (-not $observedK6 -or $emptySamples -lt 8) {
        if ([DateTime]::UtcNow -ge $deadlineUtc) {
            throw "The measured k6 pass exceeded the one-hour safety timeout."
        }

        Start-Sleep -Milliseconds 250
        $sutProcess.Refresh()
        $sutPeakWorkingSet = [Math]::Max($sutPeakWorkingSet, $sutProcess.WorkingSet64)
        $activeK6 = @(Get-Process -Name "k6" -ErrorAction SilentlyContinue |
            Where-Object { $_.Path -eq $k6Path })
        if ($activeK6.Count -eq 0) {
            if ($observedK6) {
                $emptySamples++
            }
            continue
        }

        $observedK6 = $true
        $emptySamples = 0
        $completedUtc = [DateTime]::UtcNow
        $sutCpuEnd = $sutProcess.TotalProcessorTime.TotalSeconds
        foreach ($process in $activeK6) {
            $process.Refresh()
            $previousCpu = if ($k6CpuSamples.ContainsKey($process.Id)) { $k6CpuSamples[$process.Id] } else { 0.0 }
            $k6CpuSeconds += [Math]::Max(0, $process.TotalProcessorTime.TotalSeconds - $previousCpu)
            $k6CpuSamples[$process.Id] = $process.TotalProcessorTime.TotalSeconds
            $k6PeakWorkingSet = [Math]::Max($k6PeakWorkingSet, $process.WorkingSet64)
        }
    }

    $launcher.WaitForExit()
    $k6ExitCode = $launcher.ExitCode
    $elapsedSeconds = ($completedUtc - $startedUtc).TotalSeconds
    $processorCount = [Environment]::ProcessorCount
    $resourceResult = [ordered]@{
        StartedUtc = $startedUtc.ToString("O", [Globalization.CultureInfo]::InvariantCulture)
        CompletedUtc = $completedUtc.ToString("O", [Globalization.CultureInfo]::InvariantCulture)
        ElapsedSeconds = [Math]::Round($elapsedSeconds, 3)
        LogicalProcessorCount = $processorCount
        HostAverageCpuPercent = [Math]::Round(
            (($sutCpuEnd - $sutCpuStart) / $elapsedSeconds / $processorCount) * 100,
            3)
        HostPeakWorkingSetMb = [Math]::Round($sutPeakWorkingSet / 1MB, 3)
        K6AverageCpuPercent = [Math]::Round(($k6CpuSeconds / $elapsedSeconds / $processorCount) * 100, 3)
        K6PeakWorkingSetMb = [Math]::Round($k6PeakWorkingSet / 1MB, 3)
        K6ExitCode = $k6ExitCode
    }
    $json = $resourceResult | ConvertTo-Json
    [IO.File]::WriteAllText($resourcePath, $json + [Environment]::NewLine, (New-Object Text.UTF8Encoding($false)))

    if (-not (Test-Path -LiteralPath $summaryPath)) {
        throw "k6 did not write the expected summary. See $stderrPath."
    }
    if ($null -ne $k6ExitCode -and $k6ExitCode -ne 0) {
        throw "k6 exited with code $k6ExitCode. See $stderrPath."
    }

    [pscustomobject]$resourceResult
}
finally {
    $env:MODE = $previousMode
    $env:TARGET_VUS = $previousTarget
}
