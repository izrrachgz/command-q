<#
.SYNOPSIS
Monitors the runtime and ASP.NET Core meters used for benchmark analysis.
#>
param(
    [Parameter(Mandatory)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$ProcessId,
    [ValidateRange(1, 60)]
    [int]$RefreshIntervalSeconds = 1
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if (-not (Get-Command dotnet-counters -ErrorAction SilentlyContinue)) {
    throw "dotnet-counters is not installed. Install it with: dotnet tool install --global dotnet-counters"
}

$meters = @(
    "System.Runtime",
    "Microsoft.AspNetCore.Hosting",
    "Microsoft.AspNetCore.Routing",
    "Microsoft.AspNetCore.RateLimiting",
    "Microsoft.AspNetCore.HeaderParsing",
    "Microsoft.AspNetCore.Diagnostics",
    "Microsoft.AspNetCore.Server.Kestrel",
    "Microsoft.AspNetCore.Authentication",
    "Microsoft.AspNetCore.Authorization",
    "Microsoft.AspNetCore.Identity",
    "command-q.Email",
    "command-q.Email.Outbox",
    "command-q.Email.Worker"
) -join ","

$arguments = @(
    "monitor",
    "--process-id", $ProcessId,
    "--refresh-interval", $RefreshIntervalSeconds,
    "--counters", $meters
)
dotnet-counters @arguments
