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
- EF Core stores for PostgreSQL, SQL Server, MySQL and SQLite
- provider-specific version-one migrations selected through configuration
- persisted Data Protection keys for stable authentication cookies
- health endpoints, security headers, rate limiting and source-generated logging

IdentityServer does not write to the application database. After receiving a user access token for the first time, call authenticated `POST /Account/create`; the API creates its profile and links it through the token's `(iss, sub)` identity.

## Run Locally

Trust the ASP.NET Core HTTPS development certificate once if needed:

```powershell
dotnet dev-certs https --trust
```

PostgreSQL is the default IdentityServer engine. Create an empty
`command_q_identity` database, configure the local connection and run the host:

```powershell
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj `
  "ConnectionStrings:Engine" `
  "PgSql"
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj `
  "ConnectionStrings:IdentityDb" `
  "Host=localhost;Port=5432;Database=command_q_identity;Username=postgres;Password=change-me;"
.\scripts\run-identity-server.ps1
```

## Database Providers

IdentityServer uses EF Core independently from the application's SqlKata
persistence. Select exactly one provider per IdentityServer process through
`ConnectionStrings:Engine`:

| Engine | EF Core provider | Typical use |
| --- | --- | --- |
| `PgSql` | Npgsql | Default development and production choice |
| `MsSql` | Microsoft SQL Server | SQL Server and Azure SQL deployments |
| `MySql` | Oracle MySQL Connector/NET | MySQL deployments |
| `SqLite` | Microsoft SQLite | Local, test or single-instance deployments |

Every engine uses the same `ConnectionStrings:IdentityDb` key. Provider
selection is explicit and validated during startup; connection strings are not
inspected to guess an engine.

The PostgreSQL and MySQL providers reuse the same underlying Npgsql and
Connector/NET package families as application persistence. EF Core's SQLite
provider uses `Microsoft.Data.Sqlite`; the application's SqlKata implementation
continues to use `System.Data.SQLite` independently.

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
- `ConnectionStrings:Engine`
- `OpenIddict:Issuer`
- signing and encryption certificate paths/passwords
- `DataProtection:KeysPath`
- Data Protection certificate path/password
- confidential client secrets and external provider credentials when enabled

Development certificates are rejected outside Development. Data Protection keys must be in storage shared by every IdentityServer instance and protected with the configured certificate.

## Migrations

The initial migration contains ASP.NET Identity, OpenIddict and security-event
tables. Each provider owns a migration set under
`Persistence/Migrations/{Engine}`. Add every future model change to all four
sets with one command:

```powershell
.\scripts\add-identity-migration.ps1 -Name MigrationName
```

The script restores the repository-local EF tool, builds IdentityServer and
scaffolds the same migration name for `PgSql`, `MsSql`, `MySql` and `SqLite`.
Commit all four generated migration sets together.

Development applies only pending migrations for the configured provider at
startup. Outside Development, `Migrations:RunOnStartup` is opt-in. While version
one is unfinished, manually recreate the selected development identity database
after changing the initial migration.
