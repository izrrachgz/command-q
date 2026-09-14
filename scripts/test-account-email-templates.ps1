param(
    [string]$Recipient = "account-preview@command-q.local",
    [string]$SmtpHost = "localhost",
    [int]$SmtpPort = 25,
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "IdentityServer.Test\IdentityServer.Test.csproj"
$arguments = @(
    "test",
    $project,
    "--filter",
    "FullyQualifiedName~AccountEmailSmtpPreviewTest"
)

if ($NoBuild) {
    $arguments += "--no-build"
}

$previousEnabled = $env:AccountEmailPreview__Enabled
$previousRecipient = $env:AccountEmailPreview__Recipient
$previousSmtpHost = $env:AccountEmailPreview__SmtpHost
$previousSmtpPort = $env:AccountEmailPreview__SmtpPort

try {
    $env:AccountEmailPreview__Enabled = "true"
    $env:AccountEmailPreview__Recipient = $Recipient
    $env:AccountEmailPreview__SmtpHost = $SmtpHost
    $env:AccountEmailPreview__SmtpPort = $SmtpPort.ToString()

    dotnet @arguments
}
finally {
    $env:AccountEmailPreview__Enabled = $previousEnabled
    $env:AccountEmailPreview__Recipient = $previousRecipient
    $env:AccountEmailPreview__SmtpHost = $previousSmtpHost
    $env:AccountEmailPreview__SmtpPort = $previousSmtpPort
}
