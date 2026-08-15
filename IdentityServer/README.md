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

Create an empty `command_q_identity` database, select an engine, configure the
local connection and run the host. This example uses PostgreSQL:

```powershell
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj `
  "IdentityServer:ConnectionStrings:Engine" `
  "PgSql"
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj `
  "IdentityServer:ConnectionStrings:IdentityDb" `
  "Host=localhost;Port=5432;Database=command_q_identity;Username=postgres;Password=change-me;"
.\scripts\run-identity-server.ps1
```

## Database Providers

IdentityServer uses EF Core independently from the application's SqlKata
persistence. Select exactly one provider per IdentityServer process through
`IdentityServer:ConnectionStrings:Engine`:

| Engine | EF Core provider | Typical use |
| --- | --- | --- |
| `PgSql` | Npgsql | Common production choice |
| `MsSql` | Microsoft SQL Server | SQL Server and Azure SQL deployments |
| `MySql` | Oracle MySQL Connector/NET | MySQL deployments |
| `SqLite` | Microsoft SQLite | Local, test or single-instance deployments |

Every engine uses the same `IdentityServer:ConnectionStrings:IdentityDb` key. Provider
selection is explicit and validated during startup; connection strings are not
inspected to guess an engine.

Provider-specific `IdentityDb` examples:

```text
PgSql:  Host=localhost;Port=5432;Database=command_q_identity;Username=postgres;Password=change-me;
MsSql:  Server=localhost;Database=command_q_identity;User ID=commandq;Password=change-me;TrustServerCertificate=True;
MySql:  Server=localhost;Database=command_q_identity;User ID=commandq;Password=change-me;
SqLite: Data Source=App_Data/command_q_identity.db
```

The PostgreSQL and MySQL providers reuse the same underlying Npgsql and
Connector/NET package families as application persistence. EF Core's SQLite
provider uses `Microsoft.Data.Sqlite`; the application's SqlKata implementation
continues to use `System.Data.SQLite` independently.

The Development issuer is `https://localhost:7125/`. Important endpoints include:

- `POST /account/register`
- `GET /account/sign-in`
- `POST /account/sign-in`
- `POST /account/sign-out`
- `GET /account/me`
- `GET /account/security-activity`
- `GET /account/external/{provider}`
- `GET /account/external/{provider}/link`
- `GET|POST /connect/authorize`
- `POST /connect/token`
- `GET|POST /connect/logout`
- `POST /connect/revoke`
- `GET|POST /connect/userinfo`
- `GET /.well-known/openid-configuration`
- `GET /health/live`, `GET /health/ready` and `GET /health/checks`

The local `command_q_local` public client is configured for Postman's OAuth callback. Request authorization code with PKCE and scopes `openid profile email roles offline_access command_q_api`. Browser clients should use the hosted `GET /account/sign-in` page reached from the authorization challenge; JSON callers can still use `POST /account/sign-in` directly. In Development, the Angular SPA client allows the usual `localhost` and `127.0.0.1` Angular ports from `4200` through `4205`; restart IdentityServer after changing client redirect settings so the OpenIddict application seed is reconciled.

Ending the browser session does not revoke an already issued refresh token. A client that stores a refresh token should call `POST /connect/revoke` before `GET|POST /connect/logout`; administrative global revocation remains a separate account-management concern.

## Text Resources

IdentityServer keeps host-owned user-facing and reusable operational messages in
`Resources/IdentityServerMessages.resx`. Call sites use the generated
`IdentityServerMessages.*` properties directly; parameterized text goes through
`IdentityServerText.Format(...)`.

Use those resources for sign-in page labels, validation summaries,
configuration validation messages and OpenIddict protocol responses. Source-
generated logging templates stay in logging classes because they are structured
diagnostic events, not UI/protocol text. The host is still single-language for
now, but centralizing these strings keeps the future localization path clean.

## Production Configuration

QA and Production must supply these settings through deployment configuration:

- `IdentityServer:ConnectionStrings:Engine`
- `IdentityServer:OpenIddict:Issuer`
- client IDs, redirect URIs and flow choices under `IdentityServer:OpenIddict:Clients`
- signing, encryption and Data Protection certificate paths
- `IdentityServer:DataProtection:KeysPath` pointing to storage shared by every instance

Supply these sensitive values through the deployment secret store:

- `IdentityServer:ConnectionStrings:IdentityDb`
- signing, encryption and Data Protection certificate passwords
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

Provider-specific design-time factories live in separate files under
`Persistence/DesignTime`. They use intentionally non-routable connection
placeholders because EF requires configured provider options while scaffolding.
They never read deployment secrets and are not used by runtime DI. Do not use
these factories with `dotnet ef database update`; apply migrations through
IdentityServer startup with the real configured connection instead.

Development applies only pending migrations for the configured provider at
startup. Outside Development, `IdentityServer:Migrations:RunOnStartup` is opt-in. While version
one is unfinished, manually recreate the selected development identity database
after changing the initial migration.

IdentityServer seeds after migration handling on every startup. The seeder
creates or updates the default `User` role, API scope and configured OpenIddict
clients. If non-Development startup migrations are disabled, deploy the current
schema before starting the host or seeding will fail against missing tables.
