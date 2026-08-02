# IdentityServer

`IdentityServer` is CommandQ's standalone OpenID Connect and OAuth 2.0 authorization server. ASP.NET Core Identity owns users and credentials; OpenIddict owns protocol clients, scopes, authorizations and tokens. The API remains an independently deployable resource server.

## Capabilities

- local registration and cookie sign-in
- ASP.NET Core Identity password hashing, validation and lockout
- persisted roles, claims, recovery tokens and external login links
- optional Google and Microsoft sign-in through secret-backed configuration
- authorization code with PKCE, refresh token and client credentials flows
- signed `at+jwt` access tokens and encrypted authorization/refresh tokens
- configurable clients, redirect URIs, permissions and the `command_q_api` resource scope
- persisted security activity for registration, login, lockout, refresh and logout
- SQL Server EF Core stores and a version-one migration
- persisted Data Protection keys for stable authentication cookies
- health endpoints, security headers, rate limiting and source-generated logging

IdentityServer does not write to the application database. After receiving a user access token for the first time, call authenticated `POST /Account/create`; the API creates its profile and links it through the token's `(iss, sub)` identity.

## Run Locally

Trust the ASP.NET Core HTTPS development certificate once if needed:

```powershell
dotnet dev-certs https --trust
```

Configure the SQL Server Express database and run the host:

```powershell
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj `
  "ConnectionStrings:IdentityDb" `
  "Server=localhost\SQLEXPRESS;Database=command_q_identity;Integrated Security=True;TrustServerCertificate=True;"
.\scripts\run-identity-server.ps1
```

The Development issuer is `https://localhost:7125/`. Important endpoints include:

- `POST /account/register`
- `POST /account/sign-in`
- `POST /account/sign-out`
- `GET /account/me`
- `GET /account/security-activity`
- `GET|POST /connect/authorize`
- `POST /connect/token`
- `GET|POST /connect/logout`
- `POST /connect/revoke`
- `GET|POST /connect/userinfo`
- `GET /.well-known/openid-configuration`
- `GET /health/live` and `GET /health/ready`

The local `command_q_local` public client is configured for Postman's OAuth callback. Request authorization code with PKCE and scopes `openid profile email roles offline_access command_q_api`. The JSON sign-in endpoint establishes the IdentityServer cookie before authorization.

Ending the browser session does not revoke an already issued refresh token. A client that stores a refresh token should call `POST /connect/revoke` before `GET|POST /connect/logout`; administrative global revocation remains a separate account-management concern.

## Production Configuration

QA and Production must supply these values from a deployment secret store:

- `ConnectionStrings:IdentityDb`
- `OpenIddict:Issuer`
- signing and encryption certificate paths/passwords
- `DataProtection:KeysPath`
- Data Protection certificate path/password
- confidential client secrets and external provider credentials when enabled

Development certificates are rejected outside Development. Data Protection keys must be in storage shared by every IdentityServer instance and protected with the configured certificate.

## Migrations

The initial migration contains ASP.NET Identity, OpenIddict and security-event tables. Restore the repository-local tool and add later migrations with:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef migrations add MigrationName `
  --project .\IdentityServer\IdentityServer.csproj `
  --startup-project .\IdentityServer\IdentityServer.csproj `
  --output-dir Persistence\Migrations
```

Development applies pending migrations at startup. Outside Development, `Migrations:RunOnStartup` is opt-in. While version one is unfinished, manually recreate the development identity database after changing the initial migration.
