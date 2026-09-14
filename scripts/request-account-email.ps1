param(
    [Parameter(Mandatory)]
    [ValidatePattern("^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    [string]$Email,

    [ValidateSet("Confirmation", "PasswordRecovery")]
    [string]$Purpose = "Confirmation",

    [ValidateSet("en-US", "es-ES")]
    [string]$Culture = "en-US",

    [string]$IdentityServerBaseUrl = "https://localhost:7125"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$route = if ($Purpose -eq "Confirmation") {
    "account/email-confirmation"
}
else {
    "account/password-recovery"
}
$uri = "$($IdentityServerBaseUrl.TrimEnd('/'))/$route"
$body = @{
    email = $Email
    culture = $Culture
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri $uri -ContentType "application/json" -Body $body
Write-Host "$Purpose email request accepted for local inspection. Check smtp4dev at http://localhost:5000."
