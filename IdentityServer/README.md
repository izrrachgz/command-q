# IdentityServer

`IdentityServer` is command-q's standalone OpenID Connect and OAuth 2.0 authorization server. ASP.NET Core Identity owns users and credentials; OpenIddict owns protocol clients, scopes, authorizations and tokens. The API remains an independently deployable resource server.

## Capabilities

- local registration with required email confirmation and cookie sign-in
- reusable English and Spanish account-email layouts delivered through the durable email outbox
- expiring one-time confirmation and recovery links paired with six-digit verification codes
- password recovery grants, separate three-attempt verification/reset budgets and administrator-only recovery lockout
- ASP.NET Core Identity password hashing, validation and lockout
- persisted roles, claims, recovery tokens and external login links
- internal API-key protected role and account-unlock administration for the application API
- optional Google and Microsoft sign-in through secret-backed configuration
- authorization code with PKCE, refresh token and client credentials flows
- signed `at+jwt` access tokens and encrypted authorization/refresh tokens
- configurable clients, redirect URIs, permissions and the `command-q_api` resource scope
- persisted security activity for registration, login, lockout, refresh and logout
- EF Core stores for PostgreSQL, SQL Server, MySQL and SQLite
- provider-specific version-one migrations selected through configuration
- persisted Data Protection keys for stable authentication cookies
- health endpoints, security headers, rate limiting and source-generated logging

IdentityServer does not write to the application business schema. It owns the
identity database and produces account-security messages through Email.Api.
Account email is first staged in the identity transaction as an encrypted
dispatch, then a hosted reconciler queues it idempotently over protobuf. After receiving a user access token
for the first time, call authenticated `POST /accounts`; the API creates its
profile and links it through the token's `(iss, sub)` identity.

Role management remains owned by IdentityServer. The browser does not call these
internal endpoints directly; the application API exposes protected `/admin/*`
routes and calls IdentityServer with `Api:IdentityServerAdmin` settings.

## Run Locally

Trust the ASP.NET Core HTTPS development certificate once if needed:

```powershell
dotnet dev-certs https --trust
```

Create an empty `command-q_identity` database, select an engine, configure the
local connection and run the host. This example uses PostgreSQL:

```powershell
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj `
  "IdentityServer:ConnectionStrings:Engine" `
  "PgSql"
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj `
  "IdentityServer:ConnectionStrings:IdentityDb" `
  "Host=localhost;Port=5432;Database=command-q_identity;Username=postgres;Password=change-me;"
dotnet user-secrets set --project .\IdentityServer\IdentityServer.csproj `
  "IdentityServer:EmailApiClient:Password" `
  "replace-with-email-api-password"
.\scripts\run-identity-server.ps1
```

Run the `command-emails` Email API and worker beside IdentityServer. Its Development profile
sends to smtp4dev at `localhost:25`; smtp4dev's HTTP UI/API commonly runs at
`http://localhost:5000`. Only Email.Api and Email.Worker access the email-outbox
store. Email.Api applies migrations and the worker talks to SMTP.

For an existing local account, request a message for smtp4dev inspection with:

```powershell
.\scripts\request-account-email.ps1 -Email address@example.test -Purpose Confirmation
.\scripts\request-account-email.ps1 -Email address@example.test -Purpose PasswordRecovery
```

Confirmation requests are sent only for unconfirmed accounts; recovery requests
are sent only for confirmed, unlocked accounts. Both endpoints still return
`202` when the address is unknown or ineligible.

To preview both templates without an account, database or running worker, use
the opt-in integration test. It sends through the released `Email` transport
library directly to local smtp4dev and is never part of production delivery:

```powershell
.\scripts\test-account-email-templates.ps1
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
PgSql:  Host=localhost;Port=5432;Database=command-q_identity;Username=postgres;Password=change-me;
MsSql:  Server=localhost;Database=command-q_identity;User ID=command-q;Password=change-me;TrustServerCertificate=True;
MySql:  Server=localhost;Database=command-q_identity;User ID=command-q;Password=change-me;
SqLite: Data Source=App_Data/command-q_identity.db
```

The PostgreSQL and MySQL providers reuse the same underlying Npgsql and
Connector/NET package families as application persistence. EF Core's SQLite
provider uses `Microsoft.Data.Sqlite`; the application's SqlKata implementation
continues to use `System.Data.SQLite` independently.

All EF `DateTime` properties, including nullable OpenIddict timestamps, use the
IdentityServer UTC converter. It rejects non-UTC values before persistence and
restores `DateTimeKind.Utc` when providers return timezone-neutral values.
`DateTimeOffset` remains in use for ASP.NET Identity and OpenIddict contracts
that own that type. Both types are canonicalized to UTC and normalized to
microsecond precision. The relational models use `datetime2(6)`,
`timestamptz(6)` or `datetime(6)` where the provider exposes a precision-bearing
native type; SQLite enforces the same precision through the converters.

The Development issuer is `https://localhost:7125/`. Important endpoints include:

- `POST /account/register`
- `POST /account/email-confirmation`
- `POST /account/email-confirmation/verify`
- `POST /account/password-recovery`
- `POST /account/password-recovery/verify`
- `POST /account/password-recovery/reset`
- `GET /account/sign-in`
- `POST /account/sign-in`
- `POST /account/sign-out`
- `GET /account/me`
- `GET /account/security-activity`
- `GET /account/external/{provider}`
- `GET /account/external/{provider}/link`
- `GET /internal/account-admin/users`
- `GET /internal/account-admin/users/{userId}`
- `PUT /internal/account-admin/users/{userId}/roles`
- `POST /internal/account-admin/users/{userId}/unlock`
- `GET /internal/account-admin/roles`
- `GET|POST /connect/authorize`
- `POST /connect/token`
- `GET|POST /connect/logout`
- `POST /connect/revoke`
- `GET|POST /connect/userinfo`
- `GET /.well-known/openid-configuration`
- `GET /health/live`, `GET /health/ready` and `GET /health/checks`

The internal user search accepts the shared paging and search fields plus an
optional `StartDate`/`EndDate` creation range. The application API forwards both
dates as ISO-8601 UTC instants ending in `Z`; both values are required together
and the database comparison remains in UTC.

The local `command-q_local` public client is configured for Postman's OAuth callback. Request authorization code with PKCE and scopes `openid profile email roles offline_access command-q_api`. Browser clients should use the hosted `GET /account/sign-in` page reached from the authorization challenge; JSON callers can still use `POST /account/sign-in` directly. The Angular SPA is maintained in [`command-q-front`](https://github.com/izrrachgz/command-q-front). In Development, its client allows the usual `localhost` and `127.0.0.1` Angular ports from `4200` through `4205`; restart IdentityServer after changing client redirect settings so the OpenIddict application seed is reconciled.

Ending the browser session does not revoke an already issued refresh token. A client that stores a refresh token should call `POST /connect/revoke` before `GET|POST /connect/logout`; administrative global revocation remains a separate account-management concern.

Local and external registration always assign the default `User` role. In
Development, the first account is also granted `Admin` immediately when no
administrator exists; startup role reconciliation remains a recovery path and
registration does not require a host restart.

## Account Email And Recovery

Registration creates an unconfirmed account and queues a confirmation message.
The message contains a cryptographically random 256-bit link token and a
separate six-digit code. The link opens `/auth/confirm-email` in the Angular
application and carries the token in its URL fragment so it is not sent in HTTP
requests or ordinary server logs; confirmation succeeds only when both values
match. Issuing a new
message revokes every older unconsumed challenge for the same account and
purpose.

Password recovery returns `202 Accepted` for known and unknown email addresses
so the response does not disclose whether an account exists. Eligible,
confirmed and unlocked accounts receive a one-time link to
`/auth/recover-password` plus a six-digit code. Successful code verification
returns a short-lived Data Protection grant that activates the password form.
The final reset consumes the persisted challenge and changes the ASP.NET
Identity security stamp, so the grant cannot reset the password twice.

The challenge record stores only SHA-256 hashes of the link and combined
link/code secret. The pending email dispatch is encrypted with ASP.NET Core Data
Protection until it reaches the durable outbox and is cleared after queueing.
Challenges and reset grants expire according to `IdentityServer:AccountEmail`.
Three failed confirmation-code checks consume only that confirmation challenge.
For recovery, three incorrect codes or three rejected password submissions each
consume the challenge and set an explicit administrator-required lock. Only the
internal API-key protected administrator endpoint can clear that lockout;
unlocking also revokes remaining active challenges. The browser reaches that
operation only through the role-protected application API. Anonymous endpoint
rate limiting, resend cooldowns and daily per-account delivery limits apply.

Rate limiting is workload-specific: credential operations use a fixed window,
confirmation and recovery use a sliding window, ordinary protocol/account
traffic uses a token bucket, and internal administration uses a separate
sliding window. Authenticated requests partition by subject and anonymous
requests by remote address. Values are configured under
`IdentityServer:RateLimiting`; disabling that section requires a restart and is
intended only for controlled capacity benchmarks. Rejections return `429 Too
Many Requests` and `Retry-After` when available.

HTML templates live under `Modules/Accounts/Templates/Email`. `layout.html`
owns the background, header and footer; culture- and purpose-specific fragments
replace only `{{content}}`. Plain-text messages use the same arrangement through
`layout.txt`. Dynamic values are encoded before rendering. Add both `en-US` and
`es-ES` fragments when introducing another account-email purpose.

Relevant configuration:

```text
IdentityServer:AccountEmail:ApplicationBaseUrl
IdentityServer:AccountEmail:ProfileName
IdentityServer:AccountEmail:ConfirmationLifetimeMinutes
IdentityServer:AccountEmail:PasswordRecoveryLifetimeMinutes
IdentityServer:AccountEmail:ResetGrantLifetimeMinutes
IdentityServer:AccountEmail:VerificationMaxFailedAttempts
IdentityServer:AccountEmail:PasswordResetMaxFailedAttempts
IdentityServer:AccountEmail:ResendCooldownSeconds
IdentityServer:AccountEmail:DailyDeliveryLimit
IdentityServer:AccountEmail:DispatchPollSeconds
IdentityServer:AccountEmail:DispatchBatchSize
IdentityServer:AccountEmail:DispatchRetryBaseSeconds
IdentityServer:AccountEmail:DispatchRetryMaxMinutes
IdentityServer:AccountEmail:CleanupIntervalMinutes
IdentityServer:AccountEmail:ChallengeRetentionDays
IdentityServer:AccountEmail:SecurityEventRetentionDays
IdentityServer:EmailApiClient:Enabled
IdentityServer:EmailApiClient:BaseUrl
IdentityServer:EmailApiClient:ApiClientHeaderName
IdentityServer:EmailApiClient:ApiClientKey
IdentityServer:EmailApiClient:UserName
IdentityServer:EmailApiClient:Password
```

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
- `IdentityServer:AccountEmail:ApplicationBaseUrl`
- `IdentityServer:EmailApiClient:BaseUrl`
- `IdentityServer:OpenIddict:Issuer`
- client IDs, redirect URIs and flow choices under `IdentityServer:OpenIddict:Clients`
- `IdentityServer:AdminApi:Enabled` and `IdentityServer:AdminApi:ApiKeyHeaderName` when API role management is enabled
- signing, encryption and Data Protection certificate paths
- `IdentityServer:DataProtection:KeysPath` pointing to storage shared by every instance

Supply these sensitive values through the deployment secret store:

- `IdentityServer:ConnectionStrings:IdentityDb`
- `IdentityServer:EmailApiClient:ApiClientKey` and `IdentityServer:EmailApiClient:Password`
- `IdentityServer:AdminApi:ApiKey`
- signing, encryption and Data Protection certificate passwords
- confidential client secrets and external provider credentials when enabled

Development certificates are rejected outside Development. Data Protection keys must be in storage shared by every IdentityServer instance and protected with the configured certificate.

## Migrations

The initial migration contains ASP.NET Identity, OpenIddict, account-challenge,
encrypted email-dispatch and security-event tables. Each provider owns a migration set under
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
creates or updates the default `User` role, the `Admin` role, API scope and
configured OpenIddict clients. In Development only, when no admin user exists,
the first identity user is granted `Admin` so the local SPA can reach protected
admin pages after signing in again. If non-Development startup migrations are
disabled, deploy the current schema before starting the host or seeding will
fail against missing tables.
