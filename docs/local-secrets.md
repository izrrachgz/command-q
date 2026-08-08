# Configuration And Local Secrets

Use .NET user-secrets for local backend secrets. Keep real secrets out of
`appsettings*.json`, `.env`, scripts and docs.

## Backend Setup

Run these commands from the repository root and replace the placeholder values:

```powershell
dotnet user-secrets set --project .\Api\Api.csproj "ConnectionStrings:Engine" "MsSql"
dotnet user-secrets set --project .\Api\Api.csproj "ConnectionStrings:Db" "Server=localhost;Database=CommandQ;User ID=commandq;Password=change-me;TrustServerCertificate=True;"
dotnet user-secrets set --project .\Api\Api.csproj "ApiClients:0:Name" "Local Development"
dotnet user-secrets set --project .\Api\Api.csproj "ApiClients:0:Id" "replace-with-local-client-secret"
dotnet user-secrets set --project .\Api\Api.csproj "ApiClients:0:Uri" "http://localhost:4200"
```

Development startup invokes the application migration runner automatically. The
`Application.Migrations` project reads `MigrationRecord` first and applies only
pending embedded scripts through the configured Persistence repository.

`Application.Test` has a separate user-secrets store. Its database-backed module
tests require the same application database engine and connection, with the
current application migrations already applied:

```powershell
dotnet user-secrets set --project .\Application.Test\Application.Test.csproj "ConnectionStrings:Engine" "MsSql"
dotnet user-secrets set --project .\Application.Test\Application.Test.csproj "ConnectionStrings:Db" "Server=localhost;Database=CommandQ;User ID=commandq;Password=change-me;TrustServerCertificate=True;"
```

## API Deployment Configuration

The QA and Production API templates select `PgSql` but intentionally leave the
database, authority and API-client values empty. Supply them through deployment
configuration and a secret store as appropriate:

```text
ConnectionStrings__Engine
ConnectionStrings__Db
Authentication__Authority
Authentication__Audience
Authentication__ApiScope
ApiClients__0__Name
ApiClients__0__Id
ApiClients__0__Uri
Migrations__RunOnStartup
```

`ApiClients__0__Uri` is the allowed origin outside Development, while
`ApiClients__0__Id` is the secret header value. Add additional clients by
incrementing the array index.

## Standalone Identity Server

The standalone `IdentityServer` project has its own `UserSecretsId` and database.
It issues the access tokens accepted by `Api`; the projects remain independently
deployable and communicate through OpenID Connect. For local development, the
database name is `command_q_identity`.

PostgreSQL is the default IdentityServer provider. Configure both the engine and
connection in the IdentityServer secret store:

```powershell
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "ConnectionStrings:Engine" "PgSql"
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "ConnectionStrings:IdentityDb" "Host=localhost;Port=5432;Database=command_q_identity;Username=postgres;Password=change-me;"
```

The supported engine values are `PgSql`, `MsSql`, `MySql` and `SqLite`. All use
the `ConnectionStrings:IdentityDb` key, with a connection string appropriate for
the selected EF Core provider. IdentityServer does not reuse the API's SqlKata
connection or provider registration.

User-secrets are for local Development. QA and Production should provide the
database, public issuer, PKCS#12 certificates and shared Data Protection storage
through their deployment configuration and secret store. Typical environment
variable names are:

```text
ConnectionStrings__Engine
ConnectionStrings__IdentityDb
OpenIddict__Issuer
OpenIddict__Clients__0__ClientId
OpenIddict__Clients__0__DisplayName
OpenIddict__Clients__0__ClientSecret
OpenIddict__Clients__0__AllowClientCredentials
OpenIddict__Clients__0__RedirectUris__0
OpenIddict__Clients__0__PostLogoutRedirectUris__0
OpenIddict__Certificates__Signing__Path
OpenIddict__Certificates__Signing__Password
OpenIddict__Certificates__Encryption__Path
OpenIddict__Certificates__Encryption__Password
DataProtection__KeysPath
DataProtection__Certificate__Path
DataProtection__Certificate__Password
ExternalProviders__Google__ClientId
ExternalProviders__Google__ClientSecret
ExternalProviders__Microsoft__ClientId
ExternalProviders__Microsoft__ClientSecret
```

Do not enable `OpenIddict:Certificates:UseDevelopmentCertificates` outside the
Development environment.

Google and Microsoft sign-in are activated only when both provider credentials
are present. Store them as `ExternalProviders:Google:ClientId`/
`ClientSecret` or `ExternalProviders:Microsoft:ClientId`/`ClientSecret`.

## Email Delivery

The standalone `Email` project is consumed by `Email.Worker` and remains
disabled by default. Put SMTP credentials or an OAuth access token in the
consuming host's user-secrets store, not in `Email/appsettings.json` or a
class-library secret store. For example:

```powershell
dotnet user-secrets set --project .\Api\Api.csproj "Email:Enabled" "true"
dotnet user-secrets set --project .\Api\Api.csproj "Email:Smtp:Host" "smtp.example.com"
dotnet user-secrets set --project .\Api\Api.csproj "Email:Authentication:Type" "Login"
dotnet user-secrets set --project .\Api\Api.csproj "Email:Authentication:Login:Username" "smtp-user"
dotnet user-secrets set --project .\Api\Api.csproj "Email:Authentication:Login:Password" "change-me"
dotnet user-secrets set --project .\Api\Api.csproj "Email:Sender:Address" "no-reply@example.com"
```

The host still needs an explicit project reference and
`services.AddEmailServices(configuration)` registration. OAuth tokens are
ready-to-use values and should be supplied through the deployment secret store;
the Email library does not acquire, refresh or persist them.

## Email Outbox Worker

`Email.Worker` has a separate user-secrets store. Development requires no
database secret because its dedicated outbox uses SQLite by default and is
prepared for anonymous smtp4dev delivery on `localhost:25`.

For a real SMTP endpoint, configure secrets on the worker project:

```powershell
dotnet user-secrets set --project .\Email.Worker\Email.Worker.csproj "Email:Enabled" "true"
dotnet user-secrets set --project .\Email.Worker\Email.Worker.csproj "Email:Smtp:Host" "smtp.example.com"
dotnet user-secrets set --project .\Email.Worker\Email.Worker.csproj "Email:Authentication:Type" "Login"
dotnet user-secrets set --project .\Email.Worker\Email.Worker.csproj "Email:Authentication:Login:Username" "smtp-user"
dotnet user-secrets set --project .\Email.Worker\Email.Worker.csproj "Email:Authentication:Login:Password" "change-me"
dotnet user-secrets set --project .\Email.Worker\Email.Worker.csproj "Email:Sender:Address" "no-reply@example.com"
dotnet user-secrets set --project .\Email.Worker\Email.Worker.csproj "EmailOutbox:Worker:Enabled" "true"
```

To replace SQLite, set `EmailOutbox:Persistence:Engine` and
`EmailOutbox:Persistence:ConnectionString` in the same worker secret store.
Supported values are `SqLite`, `MsSql`, `PgSql` and `MySql`. QA and Production
keep polling and startup migrations disabled until deployment configuration
enables them explicitly.

## Environment Variables

`.env.example` documents equivalent environment variable names for deployment
systems or local tools that load `.env` files. Neither backend host reads `.env`
files by itself.

ASP.NET Core maps double underscores to configuration sections:

```text
ConnectionStrings__Db -> ConnectionStrings:Db
ConnectionStrings__Engine -> ConnectionStrings:Engine
ConnectionStrings__IdentityDb -> ConnectionStrings:IdentityDb
Authentication__Authority -> Authentication:Authority
ApiClients__0__Id -> ApiClients:0:Id
OpenIddict__Issuer -> OpenIddict:Issuer
```

## Resetting Pre-v1 Development Databases

While the version 1 baseline is still changing, migration scripts are treated as
forward-only bootstrap scripts. If `migration-1.sql` or the initial IdentityServer
EF migration changes after it has already been applied locally, drop and recreate
the corresponding development database manually.

Recreating the IdentityServer database gives newly registered users new `sub`
values. Existing application `AccountIdentity` rows will still point to the old
subjects. During pre-v1 development, reset the corresponding application profile
or application database before provisioning it again with `POST /Account/create`.
Do not reconnect identities automatically by email.
