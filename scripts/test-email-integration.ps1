param(
    [string]$Configuration = "Debug",
    [string]$SmtpHost = "localhost",
    [int]$SmtpPort = 25,
    [string]$ApiBaseUrl = "http://localhost:5000",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "Email.IntegrationTest\Email.IntegrationTest.csproj"
$arguments = @("test", $project, "--configuration", $Configuration)

if ($NoBuild) {
    $arguments += "--no-build"
}

$previousEnabled = $env:Smtp4Dev__Enabled
$previousSmtpHost = $env:Smtp4Dev__SmtpHost
$previousSmtpPort = $env:Smtp4Dev__SmtpPort
$previousApiBaseUrl = $env:Smtp4Dev__ApiBaseUrl

try {
    $env:Smtp4Dev__Enabled = "true"
    $env:Smtp4Dev__SmtpHost = $SmtpHost
    $env:Smtp4Dev__SmtpPort = $SmtpPort.ToString()
    $env:Smtp4Dev__ApiBaseUrl = $ApiBaseUrl

    dotnet @arguments
}
finally {
    $env:Smtp4Dev__Enabled = $previousEnabled
    $env:Smtp4Dev__SmtpHost = $previousSmtpHost
    $env:Smtp4Dev__SmtpPort = $previousSmtpPort
    $env:Smtp4Dev__ApiBaseUrl = $previousApiBaseUrl
}
