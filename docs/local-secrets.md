# Local Secrets

Use .NET user-secrets for local backend secrets. Keep real secrets out of
`appsettings*.json`, `.env`, scripts and docs.

## Backend Setup

Run these commands from the repository root and replace the placeholder values:

```powershell
dotnet user-secrets set --project .\Api\Api.csproj "ConnectionStrings:Engine" "MsSql"
dotnet user-secrets set --project .\Api\Api.csproj "ConnectionStrings:Db" "Server=localhost;Database=CommandQ;User ID=commandq;Password=change-me;TrustServerCertificate=True;"
dotnet user-secrets set --project .\Api\Api.csproj "ApiClients:0:Name" "Local Development"
dotnet user-secrets set --project .\Api\Api.csproj "ApiClients:0:Id" "replace-with-local-client-secret"
dotnet user-secrets set --project .\Api\Api.csproj "ApiClients:0:Uri" "https://localhost:4200"
```

Development startup invokes the migration runner automatically. The runner reads
`MigrationRecord` first and applies only pending migrations.

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

For QA or Production, configure the database, public issuer and PKCS#12
certificate files, persisted Data Protection keys and key-protection certificate
through the deployment secret store. For example:

```powershell
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "OpenIddict:Issuer" "https://identity.example.com/"
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "OpenIddict:Certificates:Signing:Path" "C:/secure-certs/signing.pfx"
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "OpenIddict:Certificates:Signing:Password" "replace-with-secret"
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "OpenIddict:Certificates:Encryption:Path" "C:/secure-certs/encryption.pfx"
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "OpenIddict:Certificates:Encryption:Password" "replace-with-secret"
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "DataProtection:KeysPath" "C:/shared-secure-keys/commandq-identity"
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "DataProtection:Certificate:Path" "C:/secure-certs/data-protection.pfx"
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj "DataProtection:Certificate:Password" "replace-with-secret"
```

Do not enable `OpenIddict:Certificates:UseDevelopmentCertificates` outside the
Development environment.

Google and Microsoft sign-in are activated only when both provider credentials
are present. Store them as `ExternalProviders:Google:ClientId`/
`ClientSecret` or `ExternalProviders:Microsoft:ClientId`/`ClientSecret`.

## Environment Variables

`.env.example` documents equivalent environment variable names for deployment
systems or local tools that load `.env` files. The API does not read `.env`
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
