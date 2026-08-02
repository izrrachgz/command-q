# Local Secrets

Use .NET user-secrets for local backend secrets. Keep real secrets out of
`appsettings*.json`, `.env`, scripts and docs.

## Backend Setup

Run these commands from the repository root and replace the placeholder values:

```powershell
dotnet user-secrets set --project .\Api\Api.csproj "ConnectionStrings:Engine" "MsSql"
dotnet user-secrets set --project .\Api\Api.csproj "ConnectionStrings:Db" "Server=localhost;Database=CommandQ;User ID=commandq;Password=change-me;TrustServerCertificate=True;"
dotnet user-secrets set --project .\Api\Api.csproj "Jwt:SecretKey" "replace-with-at-least-32-random-bytes"
dotnet user-secrets set --project .\Api\Api.csproj "Jwt:Issuer" "localhost"
dotnet user-secrets set --project .\Api\Api.csproj "Jwt:Audience" "localhost"
dotnet user-secrets set --project .\Api\Api.csproj "ApiClients:0:Name" "Local Development"
dotnet user-secrets set --project .\Api\Api.csproj "ApiClients:0:Id" "replace-with-local-client-secret"
dotnet user-secrets set --project .\Api\Api.csproj "ApiClients:0:Uri" "https://localhost:4200"
```

Development startup invokes the migration runner automatically. The runner reads
`MigrationRecord` first and applies only pending migrations.

## Environment Variables

`.env.example` documents equivalent environment variable names for deployment
systems or local tools that load `.env` files. The API does not read `.env`
files by itself.

ASP.NET Core maps double underscores to configuration sections:

```text
ConnectionStrings__Db -> ConnectionStrings:Db
Jwt__SecretKey -> Jwt:SecretKey
ApiClients__0__Id -> ApiClients:0:Id
```

## Resetting Pre-v1 Development Databases

While the version 1 baseline is still changing, migration scripts are treated as
forward-only bootstrap scripts. If `migration-1.sql` changes after you have
already applied it locally, drop and recreate the development database manually.
