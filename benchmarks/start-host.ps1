<#
.SYNOPSIS
Starts a command-q host with repeatable local benchmark settings.

.DESCRIPTION
Runs the selected host in Release configuration while preserving the selected ASP.NET Core environment.
The Ceiling profile disables the host rate limiter; the Protected profile enables its normal configured policy.
OpenTelemetry export is opt-in so a missing collector cannot distort a benchmark run.
#>
param(
    [Parameter(Mandatory)]
    [ValidateSet("Api", "IdentityServer", "EmailApi", "EmailWorker", "JobsApi")]
    [string]$HostName,

    [ValidateSet("Ceiling", "Protected")]
    [string]$Profile = "Protected",

    [string]$Environment = "Development",
    [string]$Urls,
    [switch]$OpenTelemetry,
    [string]$OtlpEndpoint = "http://localhost:4317",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$commandQRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $commandQRoot
$hostDefinitions = @{
    Api = @{
        Project = Join-Path $commandQRoot "Api\Api.csproj"
        Urls = "https://localhost:7025;http://localhost:7000"
        RateLimitKey = "Api__RateLimiting__Type"
        ProtectedValue = "TokenBucket"
        CeilingValue = "None"
        TelemetryKey = "Api__OpenTelemetry__Enabled"
        OtlpKey = "Api__OpenTelemetry__Otlp__Endpoint"
    }
    IdentityServer = @{
        Project = Join-Path $commandQRoot "IdentityServer\IdentityServer.csproj"
        Urls = "https://localhost:7125"
        RateLimitKey = "IdentityServer__RateLimiting__Enabled"
        ProtectedValue = "true"
        CeilingValue = "false"
        TelemetryKey = "IdentityServer__OpenTelemetry__Enabled"
        OtlpKey = "IdentityServer__OpenTelemetry__Otlp__Endpoint"
    }
    EmailApi = @{
        Project = Join-Path $repositoryRoot "command-emails\Email.Api\Email.Api.csproj"
        Urls = "https://localhost:7325;http://localhost:7300"
        RateLimitKey = "EmailApi__RateLimiting__Enabled"
        ProtectedValue = "true"
        CeilingValue = "false"
        TelemetryKey = "EmailApi__OpenTelemetry__Enabled"
        OtlpKey = "EmailApi__OpenTelemetry__Otlp__Endpoint"
    }
    EmailWorker = @{
        Project = Join-Path $repositoryRoot "command-emails\Email.Worker\Email.Worker.csproj"
        Urls = "http://localhost:7105"
        TelemetryKey = "EmailWorker__OpenTelemetry__Enabled"
        OtlpKey = "EmailWorker__OpenTelemetry__Otlp__Endpoint"
    }
    JobsApi = @{
        Project = Join-Path $repositoryRoot "command-jobs\Jobs.Api\Jobs.Api.csproj"
        Urls = "https://localhost:7225;http://localhost:7200"
        RateLimitKey = "JobsApi__RateLimiting__Enabled"
        ProtectedValue = "true"
        CeilingValue = "false"
        TelemetryKey = "JobsApi__OpenTelemetry__Enabled"
        OtlpKey = "JobsApi__OpenTelemetry__Otlp__Endpoint"
    }
}

$hostDefinition = $hostDefinitions[$HostName]
if (-not (Test-Path -LiteralPath $hostDefinition.Project)) {
    throw "The project for host '$HostName' was not found at '$($hostDefinition.Project)'."
}

$effectiveUrls = if ([string]::IsNullOrWhiteSpace($Urls)) { $hostDefinition.Urls } else { $Urls }
$overrides = @{
    "DOTNET_ENVIRONMENT" = $Environment
    "ASPNETCORE_ENVIRONMENT" = $Environment
    "ASPNETCORE_URLS" = $effectiveUrls
    "Logging__LogLevel__Default" = "Warning"
    "Logging__LogLevel__Microsoft.AspNetCore" = "Warning"
    "Serilog__MinimumLevel__Default" = "Warning"
}
$overrides[$hostDefinition.TelemetryKey] = $OpenTelemetry.IsPresent.ToString().ToLowerInvariant()
$overrides[$hostDefinition.OtlpKey] = $OtlpEndpoint

if ($hostDefinition.ContainsKey("RateLimitKey")) {
    $overrides[$hostDefinition.RateLimitKey] = if ($Profile -eq "Ceiling") {
        $hostDefinition.CeilingValue
    } else {
        $hostDefinition.ProtectedValue
    }
}

$previousValues = @{}
try {
    foreach ($entry in $overrides.GetEnumerator()) {
        $previousValues[$entry.Key] = [Environment]::GetEnvironmentVariable($entry.Key, "Process")
        [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value, "Process")
    }

    Write-Host "Starting $HostName in $Profile profile ($Environment environment, Release configuration)."
    if ($OpenTelemetry) {
        Write-Host "OpenTelemetry export target: $OtlpEndpoint"
    }

    $arguments = @("run", "--project", $hostDefinition.Project, "--configuration", "Release", "--no-launch-profile")
    if ($NoBuild) {
        $arguments += "--no-build"
    }

    dotnet @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$HostName exited with code $LASTEXITCODE."
    }
}
finally {
    foreach ($entry in $previousValues.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value, "Process")
    }
}
