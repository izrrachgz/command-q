# Configuration And Local Secrets

Use .NET user-secrets for local backend secrets. Keep real secrets out of
`appsettings*.json`, `.env`, scripts and docs.

## Backend Setup

Run these commands from the repository root and replace the placeholder values:

```powershell
dotnet user-secrets set --project .\Api\Api.csproj "Api:ConnectionStrings:Engine" "MsSql"
dotnet user-secrets set --project .\Api\Api.csproj "Api:ConnectionStrings:Db" "Server=localhost;Database=command-q;User ID=command-q;Password=change-me;TrustServerCertificate=True;"
dotnet user-secrets set --project .\Api\Api.csproj "Api:EmailOutbox:Persistence:Engine" "MsSql"
dotnet user-secrets set --project .\Api\Api.csproj "Api:EmailOutbox:Persistence:ConnectionString" "Server=localhost;Database=command-q;User ID=command-q;Password=change-me;TrustServerCertificate=True;"
dotnet user-secrets set --project .\Api\Api.csproj "Api:ApiClients:0:Name" "Local Development"
dotnet user-secrets set --project .\Api\Api.csproj "Api:ApiClients:0:Id" "replace-with-local-client-secret"
dotnet user-secrets set --project .\Api\Api.csproj "Api:ApiClients:0:Uri" "http://localhost:4200"
```

Development startup invokes the application migration runner automatically. The
`Application.Migrations` project reads `MigrationRecord` first and applies only
pending embedded scripts through the configured Persistence repository.

PostgreSQL and SQL Server application databases require the native account RLS
objects installed by the baseline. API startup verifies them unconditionally
and fails before serving requests if a required policy is missing or disabled.
The PostgreSQL API role must not have superuser or `BYPASSRLS` privileges. See
`docs/row-level-security.md` before assigning database privileges or separating
runtime and migration credentials.

`Application.Test` has a separate user-secrets store. Its database-backed module
tests require the same application database engine and connection, with the
current application migrations already applied:

```powershell
dotnet user-secrets set --project .\Application.Test\Application.Test.csproj "ConnectionStrings:Engine" "MsSql"
dotnet user-secrets set --project .\Application.Test\Application.Test.csproj "ConnectionStrings:Db" "Server=localhost;Database=command-q;User ID=command-q;Password=change-me;TrustServerCertificate=True;"
```

## API Deployment Configuration

The QA and Production API templates intentionally leave the database,
authority and API-client values empty. Supply them through deployment
configuration and a secret store as appropriate:

```text
Api__ConnectionStrings__Engine
Api__ConnectionStrings__Db
Api__Authentication__Authority
Api__Authentication__Audience
Api__Authentication__ApiScope
Api__EmailOutbox__Persistence__Engine
Api__EmailOutbox__Persistence__ConnectionString
Api__ApiClients__0__Name
Api__ApiClients__0__Id
Api__ApiClients__0__Uri
Api__Migrations__RunOnStartup
Api__Application__TimeZone
```

`Api__ApiClients__0__Uri` is the allowed origin outside Development, while
`Api__ApiClients__0__Id` is the secret header value. Add additional clients by
incrementing the array index.

`Api:Application:TimeZone` defaults to `America/Hermosillo`. It is bound once,
is not observed for runtime changes, and startup fails unless it is a valid
NodaTime TZDB/IANA identifier. Account timezone preferences use the same ID
format. Application entity timestamps are persisted as UTC; account-local
conversion is an explicit application-layer operation.

## Standalone Identity Server

The standalone `IdentityServer` project has its own `UserSecretsId` and database.
It issues the access tokens accepted by `Api`; the projects remain independently
deployable and communicate through OpenID Connect. For local development, the
database name is `command-q_identity`.

Configure the IdentityServer engine and connection independently from the API.
This example selects PostgreSQL:

```powershell
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "IdentityServer:ConnectionStrings:Engine" "PgSql"
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "IdentityServer:ConnectionStrings:IdentityDb" "Host=localhost;Port=5432;Database=command-q_identity;Username=postgres;Password=change-me;"
```

The supported engine values are `PgSql`, `MsSql`, `MySql` and `SqLite`. All use
the `IdentityServer:ConnectionStrings:IdentityDb` key, with a connection string appropriate for
the selected EF Core provider. IdentityServer does not reuse the API's SqlKata
connection or provider registration.

User-secrets are for local Development. QA and Production should provide the
database, public issuer, PKCS#12 certificates and shared Data Protection storage
through their deployment configuration and secret store. Typical environment
variable names are:

```text
IdentityServer__ConnectionStrings__Engine
IdentityServer__ConnectionStrings__IdentityDb
IdentityServer__OpenIddict__Issuer
IdentityServer__OpenIddict__Clients__0__ClientId
IdentityServer__OpenIddict__Clients__0__DisplayName
IdentityServer__OpenIddict__Clients__0__ClientSecret
IdentityServer__OpenIddict__Clients__0__AllowClientCredentials
IdentityServer__OpenIddict__Clients__0__RedirectUris__0
IdentityServer__OpenIddict__Clients__0__PostLogoutRedirectUris__0
IdentityServer__OpenIddict__Certificates__Signing__Path
IdentityServer__OpenIddict__Certificates__Signing__Password
IdentityServer__OpenIddict__Certificates__Encryption__Path
IdentityServer__OpenIddict__Certificates__Encryption__Password
IdentityServer__DataProtection__KeysPath
IdentityServer__DataProtection__Certificate__Path
IdentityServer__DataProtection__Certificate__Password
IdentityServer__ExternalProviders__Google__ClientId
IdentityServer__ExternalProviders__Google__ClientSecret
IdentityServer__ExternalProviders__Microsoft__ClientId
IdentityServer__ExternalProviders__Microsoft__ClientSecret
```

Do not enable `IdentityServer:OpenIddict:Certificates:UseDevelopmentCertificates` outside the
Development environment.

Google and Microsoft sign-in are activated only when both provider credentials
are present. Store them under `IdentityServer:ExternalProviders:Google` or
`IdentityServer:ExternalProviders:Microsoft`.

## Email Delivery and Worker

SMTP delivery, outbox migration execution and the worker host live in the
sibling `command-emails` repository. Configure its `Email.Worker` project through
that project's user-secrets store or deployment secret store. Do not place SMTP
credentials or OAuth tokens in command-q configuration.

command-q remains an outbox producer. Its API needs only
`Api:EmailOutbox:Persistence:Engine` and
`Api:EmailOutbox:Persistence:ConnectionString`; the worker's delivery and
migration settings are documented in `command-emails`.

## Environment Variables

`.env.example` documents equivalent environment variable names for deployment
systems or local tools that load `.env` files. The backend hosts do not read
`.env` files by themselves.

ASP.NET Core maps double underscores to configuration sections:

```text
Api__ConnectionStrings__Db -> Api:ConnectionStrings:Db
Api__Authentication__Authority -> Api:Authentication:Authority
Api__ApiClients__0__Id -> Api:ApiClients:0:Id
IdentityServer__ConnectionStrings__IdentityDb -> IdentityServer:ConnectionStrings:IdentityDb
IdentityServer__OpenIddict__Issuer -> IdentityServer:OpenIddict:Issuer
```

## Resetting Pre-v1 Development Databases

While the version 1 baseline is still changing, migration scripts are treated as
forward-only bootstrap scripts. If `migration-1.sql` or the initial IdentityServer
EF migration changes after it has already been applied locally, drop and recreate
the corresponding development database manually.

Recreating the IdentityServer database gives newly registered users new `sub`
values. Existing application `AccountIdentity` rows will still point to the old
subjects. During pre-v1 development, reset the corresponding application profile
or application database before provisioning it again with `POST /accounts`.
Do not reconnect identities automatically by email.
