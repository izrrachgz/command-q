# command-q Postman

The collection exercises the standalone IdentityServer and application API using authorization code with PKCE. It does not use the removed password grant or the former API-issued JWT flow.

## Import

Import these files into Postman:

- `command-q.postman_collection.json`
- `environments/command-q.Development.postman_environment.json`
- the QA or Production environment template when needed

Select `command-q - Development` as the active environment and start the Visual Studio `Backend + Email Worker` profile. This starts IdentityServer, API and Email.Worker. Trust the local ASP.NET Core HTTPS certificate with `dotnet dev-certs https --trust` if needed.

As a command-line alternative, run `.\scripts\run-identity-server.ps1` and
`.\scripts\run-api.ps1` from separate terminals. Their default HTTPS URLs are
the same `https://localhost:7125` and `https://localhost:7025` values committed
in the Development environment.

## Local Secrets

Set these values locally in the imported Development environment before sending authenticated requests:

- `adminPassword`: the admin password
- `tenantPassword`: the tenant-user password
- `apiClientKey`: the value configured in the API's `ApiClients:0:Id` user-secret

The former password, `encoding`, does not satisfy the new minimum length and complexity policy. Choose a new local password; do not place it in the committed environment file.

The committed environment already contains:

- `activeAccount`: `admin`
- `adminUsername`: `TurboDecoder`
- `adminEmail`: `admin@local.com`
- `tenantUsername`: `TenantDecoder`
- `tenantEmail`: `tenant@local.com`
- `apiClientHeaderName`: `location_family`
- local IdentityServer/API URLs and the `command-q_local` OAuth client

Do not share or commit an exported environment after adding passwords, API keys or tokens. Postman marks those variables as secret, but repository files must remain secret-free.

## Switching Accounts

One Postman environment holds both personas. Set `activeAccount` to `admin` or
`tenant`; the collection-level pre-request script maps the selected persona's
credentials, identity ID and tokens to the generic variables used by existing
requests. The token exchange and refresh requests write their results back to
the selected persona, so changing `activeAccount` is enough to select that
persona for API requests when it already has a valid access token.

The generic `accountUsername`, `accountEmail`, `accountPassword`,
`identityUserId`, `accessToken`, `refreshToken`, `idToken` and
`accessTokenExpiresAt` entries remain compatibility aliases. Configure the
`admin*` and `tenant*` variables instead; the collection supplies the aliases
at local scope for each request.

IdentityServer's authentication cookie is shared by the Postman cookie jar and
is not persona-specific. After changing `activeAccount`, run `Sign in legacy
local account` before creating a new authorization code. Sign out first when
you want to make the cookie transition explicit. Existing persona access and
refresh tokens remain stored independently.

In Development, the first registered user is granted the `Admin` role
immediately when no administrator exists; startup reconciliation remains a
recovery path. No IdentityServer restart is required. Complete sign-in and the
authorization-code flow after registration so the access token contains the
current roles. Later role changes never modify an access token already issued.

## Initial Account Flow

Choose the intended `activeAccount`, then send these requests in order:

1. `01 - Identity account / Register local account`
2. Copy the `token` fragment value (the text after `#token=`) and six-digit code from smtp4dev into
   `accountEmailChallengeToken` and `accountEmailCode`
3. `01 - Identity account / Confirm email`
4. `01 - Identity account / Sign in local account`
5. `02 - OAuth 2.0 with PKCE / Create authorization code`
6. `02 - OAuth 2.0 with PKCE / Exchange authorization code`
7. `03 - Application API / Account profile / Provision application profile`

The registration request creates an unconfirmed account in ASP.NET Identity and
queues its one-time confirmation message. The account receives the default
`User` role. IdentityServer and application account IDs are generated afresh
after the database reset. A registration or profile-provisioning `400` is
expected when that record already exists. Sign-in remains unavailable until the
email link and code have been confirmed.

The optional password-recovery requests in the same folder use
`passwordRecoveryChallengeToken`, `passwordRecoveryCode`, `passwordResetGrant`
and `newAccountPassword`. Copy the first two values from smtp4dev. The verify
request stores the reset grant automatically; a successful reset updates the
active persona's local Postman password. Never export or commit those populated
secret values.

The authorization request generates a fresh PKCE verifier/challenge and intentionally does not follow the callback redirect. Its post-response script validates `state`, captures the authorization code from the `Location` header and stores it in the active environment. The token exchange stores access, refresh and ID tokens for the selected persona as environment secrets.

Postman's cookie jar must remain enabled between sign-in and authorization. The application API folder validates the access token type, issuer, audience, scope and expiration before explicitly adding it as a bearer token. It also injects the configured API-client header and `Origin`. If the access token has expired, run `02 - OAuth 2.0 with PKCE / Refresh access token`; if the refresh token is unavailable or rejected, repeat the authorization-code flow.

IdentityServer sign-in and API authorization are separate. Identity account endpoints use the IdentityServer cookie, while the application API requires a short-lived OAuth access token. After obtaining a valid access token, run `Provision application profile` once to create the application-owned `Account` and `AccountIdentity` records. Until that request succeeds, the `command-q` account tables are expected to be empty.

After provisioning, run `Save account preferences` before `Get account
preferences`; the save request intentionally omits `accountId` because the API
always replaces it with the authenticated account ID. The application
configuration requests under `03 - Application API / Configuration` and the
identity-administration requests require an access token
containing the `Admin` role. Configuration history is read-only;
the API records authoritative server values during startup.

Application API paths use lowercase resource names. Profile provisioning uses
`POST /accounts`, current-account resources use `/accounts/me`, and admin
resources use `/admin`. The general `POST /files` request accepts supported
documents, images, audio, short videos and ZIP files no larger than 200 MB.
Successful resource queries return their resource directly, paged requests use
an `items` collection, and errors use `application/problem+json` with stable
`errorCode` and `traceId` fields.

## Paged Search Filters

Search requests include disabled examples for `StartDate` and `EndDate`. Enable
both together when filtering by date. Both timestamps must be ISO-8601 UTC
instants with a zero offset, preferably serialized with a trailing `Z`, and
`EndDate` must not precede `StartDate`. Database comparisons remain in UTC;
account timezone is a saved preference and is not repeated on UTC searches.

The examples remain disabled so the requests return their normal unfiltered
first page after import. Keep `Page` and `Size` enabled, then opt into the date
range explicitly when testing it.

If IdentityServer was reset while the application database was retained, the
new user has a different token subject and the old `AccountIdentity` mapping is
stale. Reset that development application profile before provisioning it again;
do not relink it automatically by email.

## Other Environments

QA and Production use non-resolving `.invalid` URL placeholders. Before using either template, configure its real URLs, OAuth client ID, API-client header/key, allowed origin and both personas' account values. The Postman callback URI must also be registered for that environment's OpenIddict client.

Use `99 - Session cleanup` to revoke the refresh token and clear the IdentityServer cookie. Revoking a refresh token or ending the browser session does not immediately invalidate an already issued access token.
